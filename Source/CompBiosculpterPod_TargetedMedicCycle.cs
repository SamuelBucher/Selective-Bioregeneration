using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Selective_Bioregeneration
{
	public class CompBiosculpterPod_TargetedMedicCycle : CompBiosculpterPod_MedicCycle, ITargetedHealingCycle
	{
		private Hediff _targetHediff;
		public Hediff TargetHediff
		{
			get => _targetHediff; 
			set => _targetHediff = value;
		}

		public override bool Regenerate => false;

		public override void CycleCompleted(Pawn pawn) =>
			this.CycleCompleted(pawn, _targetHediff);

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref _targetHediff, "targetMedicHediff");
		}
	}
}
