#region license
/*The MIT License (MIT)

SortMyScience - Sort through all that stored science!

Copyright (c) 2025 ZeroMercuri

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using KSP.UI.TooltipTypes;
using KSP.UI.Screens.Flight.Dialogs;
using KSP.Localization;
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
		private static bool CNConstellationChecked;
		private static bool CNConstellationLoaded;

		public static SortMyScience Instance
		{
			get { return instance; }
		}

		private void Awake()
		{
			SMSLog("Entering Awake()");
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

			if (!CNConstellationChecked)
				CommNetConstellationCheck();

			if (!spritesLoaded)
				loadSprite();

			instance = this;
			
			processPrefab();
        }

		private void Start()
		{
            //SMSLog("Entering Start()");
			//GameEvents.OnTriggeredDataTransmission.Add(onTriggeredData);
			//GameEvents.onGamePause.Add(onPause);
			//GameEvents.onGameUnpause.Add(onUnpause);
			GameEvents.OnGameSettingsApplied.Add(onSettingsApplied);

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
            //SMSLog("Entering OnDestroy()");
            instance = null;
			//GameEvents.OnTriggeredDataTransmission.Remove(onTriggeredData);
			GameEvents.OnGameSettingsApplied.Remove(onSettingsApplied);
        }

		private void onSettingsApplied()
		{
			settings = HighLogic.CurrentGame.Parameters.CustomParams<SortMyScienceParameters>();
		}

		private void loadSprite()
		{
			Texture2D normal = GameDatabase.Instance.GetTexture("SortMyScience/Resources/Relay_Normal", false);
			Texture2D highlight = GameDatabase.Instance.GetTexture("SortMyScience/Resources/Relay_Highlight", false);
			Texture2D active = GameDatabase.Instance.GetTexture("SortMyScience/Resources/Relay_Active", false);

			if (normal == null || highlight == null || active == null)
				return;

			transferNormal = Sprite.Create(normal, new Rect(0, 0, normal.width, normal.height), new Vector2(0.5f, 0.5f));
			transferHighlight = Sprite.Create(highlight, new Rect(0, 0, highlight.width, highlight.height), new Vector2(0.5f, 0.5f));
			transferActive = Sprite.Create(active, new Rect(0, 0, active.width, active.height), new Vector2(0.5f, 0.5f));

			spritesLoaded = true;
		}

		private void processPrefab()
		{
			SMSLog("Entering processPrefab()");
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

			SMSLog("Science results prefab processed...");
		}

		private void onSpawn(ExperimentsResultDialog dialog, SortMyScienceDialog relayDialog)
		{
			SMSLog("Entering onSpawn()");
			if (dialog == null)
				return;

			resultsDialog = dialog;

			var buttons = resultsDialog.GetComponentsInChildren<Button>(true);

            //SortMyScienceLog("1");

			for (int i = buttons.Length - 1; i >= 0; i--)
			{
				Button b = buttons[i];

                //SortMyScienceLog("1-1-{0}", i);

                if (b == null)
					continue;

                //SortMyScienceLog("1-2-{0}", i);

                if (b.name == "ButtonKeep")
                {
                    //SortMyScienceLog("1-3-{0}", i);
                    transferButton = Instantiate(relayDialog.buttonTransfer, b.transform.parent) as Button;

                    transferButton.onClick.AddListener(onTransfer);

                    break;
                }
			}

            currentPage = resultsDialog.currentPage;

            if (currentPage.pageData != null)
				currentPage.pageData.baseTransmitValue = currentPage.xmitDataScalar;

            //transferButton.gameObject.SetActive(true);
        }

        private void onClose(ExperimentsResultDialog dialog)
		{
            SMSLog("Entering onClose()");
            if (dialog == null || resultsDialog == null)
				return;

			if (dialog == resultsDialog)
			{
				resultsDialog = null;
				transferButton = null;
				currentPage = null;
			}

			//popupDismiss();
        }

		public void onPageChange()
		{
            SMSLog("Entering onPageChange()");
			if (resultsDialog == null)
				return;

			currentPage = resultsDialog.currentPage;

			if (currentPage.pageData != null)
				currentPage.pageData.baseTransmitValue = currentPage.xmitDataScalar;

			//popupDismiss();
		}

		public void onTransfer()
		{
            SMSLog("Entering onTransfer()");
            if (resultsDialog == null)
				return;

			if (currentPage == null)
				return;

			if (currentPage.pageData == null)
				return;

            var vessel = FlightGlobals.ActiveVessel;

			List<ScienceData> dataQueue = new List<ScienceData>();

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
				ModuleDataTransmitter antenna = (ModuleDataTransmitter)bestTransmitter;

				//Transmit everything above threshold
				if (page.xmitDataScalar >= settings.transmissionThreshold && page.scienceValue > 0 && bestTransmitter != null)  //xmitDataScalar is what percentage of the total data is transmitted
				{
					SMSLog("Transmit:", page.pageData);
					List<ScienceData> data = new List<ScienceData>
                    {
                        page.pageData
                    };
					bestTransmitter.TransmitData(data);
				}
				//Lab everything below threshold if a lab is available
				else if (page.labSearch.NextLabForDataFound && page.pageData.labValue > 0 && page.scienceValue <= settings.labThreshold)
				{
					ModuleScienceLab lab = page.labSearch.NextLabForData;
					SMSLog("Lab Process: ", page.pageData);
					ModuleScienceContainer container = lab.part.FindModuleImplementing<ModuleScienceContainer>();
					StartCoroutine(lab.ProcessData(page.pageData));
				}
				//Discard everything with no value
				else if (page.pageData.labValue == 0f && page.scienceValue == 0f)
				{
					Part p = GetContainerPart(page.pageData);
                    if (p == null)
                    {
						SMSLog("Could not find container for Discard. ", page.pageData);
                    }
					else
					{
                        SMSLog("Discard:", page.pageData);
                        ModuleScienceContainer c = p.FindModuleImplementing<ModuleScienceContainer>();
						c.DumpData(page.pageData);
					}
                }
				//Keep the rest (Returnables, Transmittables w/o Connection, and Lab-worthy data without an available lab)
				else
				{
					SMSLog("Keep:", page.pageData);
				}
            }
			resultsDialog.Dismiss();
        }

        public Part GetContainerPart(ScienceData science)
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

  //      private void onTriggeredData(ScienceData data, Vessel vessel, bool aborted)
		//{
		//	SMSLog("Entering onTriggeredData()");
		//	StartCoroutine(WaitForTrigger(data, vessel, aborted));
		//}

		//private IEnumerator WaitForTrigger(ScienceData data, Vessel vessel, bool aborted)
		//{
		//	SMSLog("Entering WaitForTrigger()");

		//	yield return new WaitForEndOfFrame();

		//	if (vessel == null)
		//		yield break;

		//	if (vessel != FlightGlobals.ActiveVessel)
		//		yield break;

		//	if (data == null)
		//		yield break;

		//	for (int i = queuedData.Count - 1; i >= 0; i--)
		//	{
		//		SortMyScienceData d = queuedData[i];

		//		if (d._data.subjectID != data.subjectID)
		//			continue;

		//		queuedData.Remove(d);

		//		break;
		//	}
		//}

		private bool isVesselConnected()
		{
            SMSLog("Entering isVesselConnected()");
            Vessel vessel = FlightGlobals.ActiveVessel;
			if (vessel == null)
				return false;

			//if (!CommNetScenario.CommNetEnabled)
			//	return true;    //If Comms aren't enabled we are always connected

			return vessel.Connection.IsConnected;
        }

		
		private void CommNetConstellationCheck()
		{
            SMSLog("Entering CommNetConstellationCheck()");
            var assembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name.StartsWith("CommNetConstellation"));

			CNConstellationLoaded = assembly != null;

			//if (CNConstellationLoaded)
			//	SortMyScienceLog("CommNet Constellation addon detected; Science Relay disabling CommNet connection status integration");

			CNConstellationChecked = true;
		}
		
		public static void SMSLog(string s, params object[] o)
		{
            if (o != null && o.Length > 0 && o[0] is ScienceData data)
            {
                ScienceUtil.GetExperimentFieldsFromScienceID(data.subjectID, out string bodyName, out string situation, out string biome);
                Debug.Log(string.Format("[SortMyScience] {0} : {1} : {2} : {3} : {4}", s, data.subjectID, bodyName, situation, biome));
            }
            else
                Debug.Log(string.Format("[SortMyScience] " + s, o));
        }
    }
}
