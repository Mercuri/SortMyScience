#region license
/*The MIT License (MIT)

SortMyScienceParameters - In game settings for Science Transfer

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

namespace SortMyScience
{
	public class SortMyScienceParameters : GameParameters.CustomParameterNode
	{
        public override string Title => string.Empty; // Section title in settings
        public override string DisplaySection => "#autoLOC_SortMyScience_Title"; // Name in the settings menu
        public override string Section => "#autoLOC_SortMyScience_Title"; // Internal category
        public override GameParameters.GameMode GameMode => GameParameters.GameMode.SCIENCE | GameParameters.GameMode.CAREER; // Available in game modes with science
        public override bool HasPresets => false; // No presets
        public override int SectionOrder => 1; // Determines the position in the settings menu
        [GameParameters.CustomFloatParameterUI("#autoLOC_SortMyScience_Settings_Tx", minValue = 0f, maxValue = 1f, stepCount = 101, displayFormat = "P0", toolTip = "#autoLOC_SortMyScience_Settings_Tx_ToolTip")]
		public float transmissionThreshold = 1f;
		[GameParameters.CustomFloatParameterUI("#autoLOC_SortMyScience_Settings_Lab", minValue = 0f, maxValue = 1f, stepCount = 101, displayFormat = "P0", toolTip = "#autoLOC_SortMyScience_Settings_LabTooltip")]
		public float labThreshold = 0f;
		[GameParameters.CustomParameterUI("#autoLOC_SortMyScience_Settings_Discard", toolTip = "#autoLOC_SortMyScience_Settings_DiscardTooltip")]
		public bool discardDeadScience = true;

    }
}
