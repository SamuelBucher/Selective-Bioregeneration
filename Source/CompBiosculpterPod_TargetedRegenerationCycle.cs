using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Selective_Bioregeneration
{
    public class CompBiosculpterPod_TargetedRegenerationCycle : CompBiosculpterPod_TargetedHealingCycle
    {
        public override bool Regenerate => true;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref targetHediff, "targetRegenHediff");
        }
    }
}
