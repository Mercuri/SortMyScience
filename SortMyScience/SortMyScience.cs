#region license
/*The MIT License (MIT)

SortMyScience - Sort through all that stored science!

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
using System;
using System.Collections.Generic;
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
			//GameEvents.OnTriggeredDataTransmission.Add(onTriggeredData);
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
			//GameEvents.OnTriggeredDataTransmission.Remove(onTriggeredData);
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
#if DEBUG
			SMSLog("Entering OnSortScience()");
#endif
			if (resultsDialog == null)
				return;

			if (currentPage == null)
				return;

			if (currentPage.pageData == null)
				return;

            var vessel = FlightGlobals.ActiveVessel;

			int transmitted = 0, labbed = 0, discarded = 0, kept = 0;

            for (int i = resultsDialog.pages.Count - 1; i >= 0; i--)
			{
				ExperimentResultDialogPage page = resultsDialog.pages[i];

				if (page == null)
					continue;

				if (page.pageData == null)
					continue;

				if (page.host == null)
					continue;

				//Get the antenna
				IScienceDataTransmitter bestTransmitter = ScienceUtil.GetBestTransmitter(vessel);
				//ModuleDataTransmitter antenna = (ModuleDataTransmitter)bestTransmitter;
                ModuleScienceLab lab = page.labSearch.NextLabForData;

                //Transmit everything above efficiency threshold
                //xmitDataScalar is the transmission efficiency
                if (page.xmitDataScalar >= settings.transmissionThreshold && page.scienceValue > 0 && bestTransmitter != null)
				{
					SMSLog("Transmit:", page.pageData);
					List<ScienceData> data = new List<ScienceData>
                    {
                        page.pageData
                    };
					bestTransmitter.TransmitData(data);
					transmitted++;
				}
                //Lab everything below threshold if a lab is available
                else if (lab != null && page.pageData.labValue > 0 && (page.scienceValue/page.refValue) <= settings.labThreshold)
				{
					SMSLog("Lab Process: ", page.pageData);
					ModuleScienceContainer container = lab.part.FindModuleImplementing<ModuleScienceContainer>();
					StartCoroutine(lab.ProcessData(page.pageData));
					labbed++;
				}
				//Discard everything with no value
				else if (page.pageData.labValue == 0f && page.scienceValue == 0f)
				{
					Part p = GetContainerPart(page.pageData);
                    if (p == null)
                    {
						SMSWarn("Could not find container for Discard. ", page.pageData);
                    }
					else
					{
                        SMSLog("Discard:", page.pageData);
                        ModuleScienceContainer c = p.FindModuleImplementing<ModuleScienceContainer>();
						c.DumpData(page.pageData);
						discarded++;
					}
                }
				//Keep the rest (Returnables, Transmittables w/o Connection, and Lab-worthy data without an available lab)
				else
				{
					SMSLog("Keep:", page.pageData);
					kept++;
				}
            }
			resultsDialog.Dismiss();
			string msg = Localizer.Format("#autoLOC_SortMyScience_CompleteMsg", transmitted, labbed, discarded, kept);
			ScreenMessages.PostScreenMessage(msg, duration: 7.5f);
        }

        private Part GetContainerPart(ScienceData science)
        {
            // Ensure we are in a flight scene and the active vessel is available
            if (FlightGlobals.ActiveVessel != null)
            {
                // Iterate through all parts on the active vessel
                foreach (Part p in FlightGlobals.ActiveVessel.parts)
                {
                    // Check if the part's flightID matches the container ID
                    if (p.flightID == science.container)
                    {
                        return p; // Found the correct part
                    }
                }
            }
			// If not found (e.g., vessel unloaded, data corrupted, or in editor scene)
            return null;
        }
		
		public static void SMSLog(string s, params object[] o)
		{
			if (o != null && o.Length > 0 && o[0] is ScienceData data)
			{
				//ScienceUtil.GetExperimentFieldsFromScienceID(data.subjectID, out string bodyName, out string situation, out string biome);
				//Debug.Log(string.Format("[SortMyScience] {0} : {1} : {2} : {3} : {4}", s, data.subjectID, bodyName, situation, biome));
				Debug.Log(string.Format($"[SortMyScience] {s} : {data.subjectID}"));
			}
			else
			{
				Debug.Log(string.Format("[SortMyScience] " + s, o));
			}
        }

        public static void SMSWarn(string s, params object[] o)
        {
            if (o != null && o.Length > 0 && o[0] is ScienceData data)
            {
                Debug.LogWarning(string.Format($"[SortMyScience] {s} : {data.subjectID}"));
            }
            else
            {
                Debug.LogWarning(string.Format("[SortMyScience] " + s, o));
            }
        }

        public static void SMSError(string s, params object[] o)
        {
			if (o != null && o.Length > 0 && o[0] is ScienceData data)
			{
				Debug.LogError(string.Format($"[SortMyScience] {s} : {data.subjectID}"));
			}
			else
			{
				Debug.LogError(string.Format("[SortMyScience] " + s, o));
			}
        }
    }
}
