using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Selective_Bioregeneration
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.SamBucher.AllowToolPatchesDeathless");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(CompBiosculpterPod), "OrderToPod")]
        public static class OrderToPod_Patch
        {
            [HarmonyPrefix]
            public static bool OrderToPod_Prefix(ref CompBiosculpterPod_Cycle cycle, ref Pawn pawn, ref Action giveJobAct)
            {
                if (cycle is CompBiosculpterPod_TargetedRegenerationCycle)
                {
                    List<Hediff> hediffs = new List<Hediff>();
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if ((cycle as CompBiosculpterPod_HealingCycle).CanPotentiallyHeal(pawn, hediff))
                        {
                            hediffs.Add(hediff);
                        }
                    }
                    SB_Dialog_HediffSelection.CreateDialog(cycle as CompBiosculpterPod_TargetedRegenerationCycle, pawn, hediffs, giveJobAct);
                }
                else
                {
                    giveJobAct();
                }

                return false;
            }
        }
    }
}
