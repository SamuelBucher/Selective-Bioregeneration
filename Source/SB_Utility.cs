using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Selective_Bioregeneration
{
	public interface ITargetedHealingCycle
	{
		Hediff TargetHediff { get; set; }
	}

	public static class SB_Utility
	{
		public static void CycleCompleted(this CompBiosculpterPod_HealingCycle cycle, Pawn pawn, Hediff targetHediff)
		{
			if (pawn.health == null)
				return;

			cycle.tmpHediffs.Clear();
			cycle.tmpHediffs.AddRange(pawn.health.hediffSet.hediffs);
			try
			{
				foreach (Hediff tmpHediff in cycle.tmpHediffs)
				{
					if (cycle.WillHeal(pawn, tmpHediff))
						HealthUtility.Cure(tmpHediff);
					else if (tmpHediff is Hediff_MissingPart hediff_MissingPart && hediff_MissingPart.IsFresh)
					{
						hediff_MissingPart.IsFresh = false;
						pawn.health.Notify_HediffChanged(hediff_MissingPart);
					}
				}
				cycle.tmpHediffs.Clear();
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					if (cycle.CanPotentiallyHeal(pawn, hediff))
						cycle.tmpHediffs.Add(hediff);
				}
				if (cycle.tmpHediffs.Contains(targetHediff))
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
				cycle.tmpHediffs.Clear();
			}
		}
	}
}
