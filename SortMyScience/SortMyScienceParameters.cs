#region license
/*The MIT License (MIT)

SortMyScienceParameters - In game settings for Sort My Science

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
using System.Reflection;

namespace SortMyScience
{
    public class SortMyScienceParameters : GameParameters.CustomParameterNode
    {
        public override string Title => string.Empty;
        public override string DisplaySection => "#autoLOC_SortMyScience_Title";
        public override string Section => "#autoLOC_SortMyScience_Title";
        public override GameParameters.GameMode GameMode =>
            GameParameters.GameMode.SCIENCE | GameParameters.GameMode.CAREER;

        public override bool HasPresets => false;
        public override int SectionOrder => 35;

        // --- PARAMETERS ---

        [GameParameters.CustomFloatParameterUI(
            "#autoLOC_SortMyScience_Settings_Tx",
            minValue = 0f,
            maxValue = 1f,
            stepCount = 101,
            displayFormat = "P0",
            toolTip = "#autoLOC_SortMyScience_Settings_Tx_ToolTip"
        )]
        public float txThreshold = 1f;  // 100%

        [GameParameters.CustomFloatParameterUI(
            "#autoLOC_SortMyScience_Settings_Lab",
            minValue = 0f,
            maxValue = 0.20f,  // 0% → 20%
            stepCount = 201,
            displayFormat = "P1", // percentage: 0.0% – 20.0%
            toolTip = "#autoLOC_SortMyScience_Settings_LabTooltip"
        )]
        public float labThreshold = 0.01f; // 1%

        [GameParameters.CustomParameterUI(
            "#autoLOC_SortMyScience_Settings_Discard",
            toolTip = "#autoLOC_SortMyScience_Settings_DiscardTooltip"
        )]
        public bool discardDuds = true;

        [GameParameters.CustomFloatParameterUI(
            "#autoLOC_SortMyScience_Settings_DiscardThreshold",
            minValue = 0f,
            maxValue = 0.01f,     // 0.000 → 0.010 (fractions)
            stepCount = 101,
            displayFormat = "0.000", // show fractional scientific value
            toolTip = "#autoLOC_SortMyScience_Settings_DiscardThresholdTooltip"
        )]
        public float discardThreshold = 0.001f; // default 0.1% (0.001 fraction)

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            // Always display every setting in this category
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == nameof(discardThreshold))
                return discardDuds;   // Greyed out when discardDuds == false

            return true;
        }
    }
}
