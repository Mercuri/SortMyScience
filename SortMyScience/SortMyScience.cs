#region license
/*The MIT License (MIT)

Sort My Science - Automatically sorts every experiment in your science containers!

Copyright (c) 2025 Mercuri

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using KSP.Localization;
using KSP.UI.Screens.Flight.Dialogs;
using KSP.UI.TooltipTypes;
using KSPAchievements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SortMyScience
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SortMyScience : MonoBehaviour
    {
        private static Sprite transferNormal;
        private static Sprite transferHighlight;
        private static Sprite transferActive;
        private static bool spritesLoaded;
        private static SortMyScience instance;

        private string version;
        private SortMyScienceParameters settings;
        private Button transferButton;
        private ExperimentsResultDialog resultsDialog;
        private ExperimentResultDialogPage currentPage;

        public static SortMyScience Instance
        {
            get { return instance; }
        }

        private void Awake()
        {
#if DEBUG
			SMSLog("Entering Awake()");
#endif
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                Destroy(gameObject);
                return;
            }

            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            if (!spritesLoaded)
                LoadSprite();

            instance = this;

            ProcessPrefab();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "Using Pattern Matching breaks switch statement")]
        private void Start()
        {
#if DEBUG
			SMSLog("Entering Start()");
#endif
            SortMyScienceDialog.onDialogSpawn.Add(OnSpawn);
            SortMyScienceDialog.onDialogClose.Add(OnClose);
            GameEvents.OnGameSettingsApplied.Add(OnSettingsApplied);

            settings = HighLogic.CurrentGame.Parameters.CustomParams<SortMyScienceParameters>();

            if (settings == null)
            {
                instance = null;
                Destroy(gameObject);
            }

            Assembly assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
            var ainfoV = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            switch (ainfoV == null)
            {
                case true: version = ""; break;
                default: version = ainfoV.InformationalVersion; break;
            }
        }

        private void OnDestroy()
        {
#if DEBUG
            SMSLog("Entering OnDestroy()");
#endif
            instance = null;
            SortMyScienceDialog.onDialogSpawn.Remove(OnSpawn);
            SortMyScienceDialog.onDialogClose.Remove(OnClose);
            GameEvents.OnGameSettingsApplied.Remove(OnSettingsApplied);
        }

        private void OnSettingsApplied()
        {
            settings = HighLogic.CurrentGame.Parameters.CustomParams<SortMyScienceParameters>();
        }

        private void LoadSprite()
        {
#if DEBUG
            SMSLog("Entering LoadSprite()");
#endif
            Texture2D normal = GameDatabase.Instance.GetTexture("SortMyScience/Resources/SortMyScience_Normal", false);
            Texture2D highlight = GameDatabase.Instance.GetTexture("SortMyScience/Resources/SortMyScience_Highlight", false);
            Texture2D active = GameDatabase.Instance.GetTexture("SortMyScience/Resources/SortMyScience_Active", false);

            if (normal == null || highlight == null || active == null)
                return;

            transferNormal = Sprite.Create(normal, new Rect(0, 0, normal.width, normal.height), new Vector2(0.5f, 0.5f));
            transferHighlight = Sprite.Create(highlight, new Rect(0, 0, highlight.width, highlight.height), new Vector2(0.5f, 0.5f));
            transferActive = Sprite.Create(active, new Rect(0, 0, active.width, active.height), new Vector2(0.5f, 0.5f));

            spritesLoaded = true;
        }

        private void ProcessPrefab()
        {
#if DEBUG
            SMSLog("Entering ProcessPrefab()");
#endif
            GameObject prefab = AssetBase.GetPrefab("ScienceResultsDialog");

            if (prefab == null)
                return;

            SortMyScienceDialog dialogListener = prefab.gameObject.AddOrGetComponent<SortMyScienceDialog>();

            Button[] buttons = prefab.GetComponentsInChildren<Button>(true);

            for (int i = buttons.Length - 1; i >= 0; i--)
            {
                Button b = buttons[i];
                if (b.name == "ButtonPrev")
                    dialogListener.buttonPrev = b;
                else if (b.name == "ButtonNext")
                    dialogListener.buttonNext = b;
                else if (b.name == "ButtonKeep")
                {
                    dialogListener.buttonTransfer = Instantiate(b) as Button;

                    dialogListener.buttonTransfer.name = "ButtonTransfer";

                    dialogListener.buttonTransfer.onClick.RemoveAllListeners();

                    TooltipController_Text tooltip = dialogListener.buttonTransfer.GetComponent<TooltipController_Text>();

                    if (tooltip != null)
                        tooltip.textString = Localizer.Format("#autoLOC_SortMyScience_Tooltip");

                    if (spritesLoaded)
                    {
                        Selectable select = dialogListener.buttonTransfer.GetComponent<Selectable>();

                        if (select != null)
                        {
                            select.image.sprite = transferNormal;
                            select.image.type = Image.Type.Simple;
                            select.transition = Selectable.Transition.SpriteSwap;

                            SpriteState state = select.spriteState;
                            state.highlightedSprite = transferHighlight;
                            state.pressedSprite = transferActive;
                            state.disabledSprite = transferActive;
                            select.spriteState = state;
                        }
                    }

                }
            }
#if DEBUG
			SMSLog("Science results prefab processed...");
#endif
        }

        private void OnSpawn(ExperimentsResultDialog dialog, SortMyScienceDialog sortMyScienceDialog)
        {
#if DEBUG
            SMSLog("Entering OnSpawn()");
#endif
            if (dialog == null)
                return;

            resultsDialog = dialog;

            var buttons = resultsDialog.GetComponentsInChildren<Button>(true);

            for (int i = buttons.Length - 1; i >= 0; i--)
            {
                Button b = buttons[i];

                if (b == null)
                    continue;

                if (b.name == "ButtonKeep")
                {
                    transferButton = Instantiate(sortMyScienceDialog.buttonTransfer, b.transform.parent) as Button;
                    transferButton.onClick.AddListener(OnSortScience);
                    break;
                }
            }

            currentPage = resultsDialog.currentPage;

            if (currentPage.pageData != null)
                currentPage.pageData.baseTransmitValue = currentPage.xmitDataScalar;
        }

        private void OnClose(ExperimentsResultDialog dialog, SortMyScienceDialog sortMyScienceDialog)
        {
#if DEBUG
            SMSLog("Entering OnClose()");
#endif
            if (dialog == null || resultsDialog == null)
                return;

            if (dialog == resultsDialog)
            {
                resultsDialog = null;
                transferButton = null;
                currentPage = null;
            }
        }

        public void OnPageChange()
        {
#if DEBUG
			SMSLog("Entering OnPageChange()");
#endif
            if (resultsDialog == null)
                return;

            currentPage = resultsDialog.currentPage;

            if (currentPage.pageData != null)
                currentPage.pageData.baseTransmitValue = currentPage.xmitDataScalar;
        }

        public void OnSortScience()
        {
            SMSLog("SortScience: Vessel-wide processing triggered.");

            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
                return;

            try { resultsDialog?.Dismiss(); } catch { }

            List<IScienceDataContainer> containers = vessel.FindPartModulesImplementing<IScienceDataContainer>();
            if (containers == null || containers.Count == 0)
            {
                SMSLog("No science containers found on vessel.");
                return;
            }

            // --------------------------------------------
            //   BUILD FAST LOOKUP TABLES (MAJOR SPEEDUP)
            // --------------------------------------------

            // Cache experiment modules by experimentID
            Dictionary<string, ModuleScienceExperiment> expModuleCache = new Dictionary<string, ModuleScienceExperiment>(64);
            foreach (Part p in vessel.parts)
            {
                var exps = p.FindModulesImplementing<ModuleScienceExperiment>();
                for (int i = 0; i < exps.Count; i++)
                {
                    ModuleScienceExperiment m = exps[i];
                    if (!expModuleCache.ContainsKey(m.experimentID))
                        expModuleCache.Add(m.experimentID, m);
                }
            }

            // Cache subjects once
            Dictionary<string, ScienceSubject> subjCache = new Dictionary<string, ScienceSubject>(256);

            // Pre-scan all science on vessel — avoids N^2 lookups.
            List<(ScienceData data, IScienceDataContainer container, string expID)> allScience
                = new List<(ScienceData, IScienceDataContainer, string)>(256);

            foreach (var c in containers)
            {
                var ds = c.GetData();
                if (ds == null) continue;

                foreach (var d in ds)
                {
                    string expID = GetExperimentIDFromSubjectID(d.subjectID);
                    allScience.Add((d, c, expID));
                }
            }

            int transmitted = 0, labbed = 0, discarded = 0, kept = 0;

            // --------------------------------------------
            //           PROCESS EACH SCIENCE PACKET
            // --------------------------------------------

            foreach (var entry in allScience)
            {
                ScienceData data = entry.data;
                IScienceDataContainer container = entry.container;
                string experimentID = entry.expID;

                if (data == null || container == null || string.IsNullOrEmpty(experimentID))
                {
                    kept++;
                    continue;
                }

                // Cached subject lookup
                if (!subjCache.TryGetValue(data.subjectID, out ScienceSubject subj))
                {
                    subj = ResearchAndDevelopment.GetSubjectByID(data.subjectID);
                    subjCache[data.subjectID] = subj;
                }

                if (subj == null)
                {
                    SMSLog("MissingSubjectKeep", data);
                    kept++;
                    continue;
                }

                // Experiment definition (cached by KSP already)
                ScienceExperiment expDef = ResearchAndDevelopment.GetExperiment(experimentID);
                if (expDef == null)
                {
                    SMSLog("NoExperimentDefKeep", data);
                    kept++;
                    continue;
                }

                // Fast lookup: experiment module (if exists)
                expModuleCache.TryGetValue(experimentID, out ModuleScienceExperiment expModule);

                // Compute dialog-true values
                float baseValue = expDef.baseValue;
                float dataScale = expDef.dataScale;
                float fullValue = baseValue * dataScale;

                if (fullValue <= 0f)
                {
                    kept++;
                    continue;
                }

                float remaining = subj.scienceCap - subj.science;
                if (remaining < 0f)
                    remaining = 0f;

                float recoveryValue = Mathf.Min(fullValue, remaining);

                float labValue = data.labValue;

                float xmitScalar = expModule != null ? expModule.xmitDataScalar : 1f;
                float transmitValue = recoveryValue * xmitScalar;

                float transmitFrac = transmitValue / fullValue;
                float remainingFrac = recoveryValue / fullValue;

                bool allResearched = subj.science >= subj.scienceCap;
                bool hasLabValue = labValue > 0f;

                // -------------------------------
                //          SORTING RULES
                // -------------------------------

                // 1) TRANSMIT
                if (transmitFrac >= settings.txThreshold)
                {
                    if (TransmitScience(data, container))
                        transmitted++;
                    else
                        kept++;
                    continue;
                }

                // 2) LAB
                if (hasLabValue && remainingFrac <= settings.labThreshold)
                {
                    if (SendToLab(data, container, vessel))
                        labbed++;
                    else
                        kept++;
                    continue;
                }

                // 3) DISCARD
                if (settings.discardDuds && !hasLabValue &&
                    (remainingFrac <= settings.discardThreshold || allResearched))
                {
                    if (DiscardScience(data, container))
                        discarded++;
                    else
                        kept++;
                    continue;
                }

                // 4) KEEP
                kept++;
            }

            // --------------------------------------------
            //              FINAL MESSAGE
            // --------------------------------------------

            string msg = Localizer.Format("#autoLOC_SortMyScience_CompleteMsg",
                                          transmitted, labbed, discarded, kept);

            ScreenMessages.PostScreenMessage(msg, 7.5f, ScreenMessageStyle.UPPER_CENTER);
            SMSLog(msg);
        }

        private string GetExperimentIDFromSubjectID(string subjectID)
        {
            if (string.IsNullOrEmpty(subjectID)) return null;
            int i = subjectID.IndexOf('@');
            if (i <= 0) return null;
            return subjectID.Substring(0, i);
        }

        private bool TransmitScience(ScienceData d, IScienceDataContainer c)
        {
            var tx = ScienceUtil.GetBestTransmitter(FlightGlobals.ActiveVessel);
            if (tx == null || !tx.CanTransmit()) return false;

            tx.TransmitData(new List<ScienceData> { d });

            // Ensure the original container loses the data
            try { c.DumpData(d); } catch { }
            return true;
        }

        private bool SendToLab(ScienceData d, IScienceDataContainer c, Vessel v)
        {
            var labs = v.FindPartModulesImplementing<ModuleScienceLab>();
            if (labs == null || labs.Count == 0) return false;

            // pick the first operational lab (or adapt to pick best)
            foreach (var lab in labs)
            {
                if (!lab.IsOperational()) continue;
                lab.StartCoroutine(lab.ProcessData(d));
                try { c.DumpData(d); } catch { }
                return true;
            }
            return false;
        }

        private bool DiscardScience(ScienceData d, IScienceDataContainer c)
        {
            try { c.DumpData(d); return true; } catch { return false; }
        }

        public static void SMSLog(string s, params object[] o)
        {
            if (o != null && o.Length > 0 && o[0] is ScienceData data)
            {
                Debug.Log(string.Format($"[SortMyScience] {s}:{data.subjectID}"));
            }
            else
            {
                Debug.Log(string.Format("[SortMyScience] " + s, o));
            }
        }
    }
}
