using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Selective_Bioregeneration
{
    public class CompBiosculpterPod_TargetedRegenerationCycle : CompBiosculpterPod_RegenerationCycle
    {
        public Hediff targetHediff;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref targetHediff, "targetHediff");
        }

        public override void CycleCompleted(Pawn pawn)
        {
            if (pawn.health == null)
            {
                return;
            }
            tmpHediffs.Clear();
            tmpHediffs.AddRange(pawn.health.hediffSet.hediffs);
            try
            {
                foreach (Hediff tmpHediff in tmpHediffs)
                {
                    if (WillHeal(pawn, tmpHediff))
                    {
                        HealthUtility.Cure(tmpHediff);
                    }
                    else if (tmpHediff is Hediff_MissingPart hediff_MissingPart && hediff_MissingPart.IsFresh)
                    {
                        hediff_MissingPart.IsFresh = false;
                        pawn.health.Notify_HediffChanged(hediff_MissingPart);
                    }
                }
                tmpHediffs.Clear();
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (CanPotentiallyHeal(pawn, hediff))
                    {
                        tmpHediffs.Add(hediff);
                    }
                }
                if (tmpHediffs.Contains(targetHediff))
                {
                    HealthUtility.Cure(targetHediff);
                    Messages.Message("BiosculpterHealCompletedWithCureMessage".Translate(pawn.Named("PAWN"), targetHediff.Named("HEDIFF")), pawn, MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("BiosculpterHealCompletedMessage".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
            finally
            {
                tmpHediffs.Clear();
            }
        }
    }
}
