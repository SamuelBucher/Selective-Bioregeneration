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
        //private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.SamBucher.SelectiveBioregeneration");
            //harmony.PatchAll();
            harmony.Patch(
                AccessTools.Method(typeof(CompBiosculpterPod), "OrderToPod"),
                new HarmonyMethod(typeof(OrderToPod_Patch), nameof(OrderToPod_Patch.OrderToPod_Prefix)));
            //Compatibility patch for Faster Biosculpter Pod
            /*var isFBPon = AccessTools.Method(Type.GetType("FasterBiosculpterPod.SettingsUtils, FasterBiosculpterPod"), "ApplySettings");//Type.GetType("FasterBiosculpterPod.SettingsUtils")?.GetMethod("ApplySettings");
            if (isFBPon != null)
            {
                harmony.Patch(
                    isFBPon,
                    transpiler: new HarmonyMethod(typeof(FBP_Patcher), nameof(FBP_Patcher.ApplySettings_Transpiler)));
            }
            else
                Log.Message("FBP not detected");*/

        }

        //[HarmonyPatch(typeof(CompBiosculpterPod), "OrderToPod")]
        public static class OrderToPod_Patch
        {
            [HarmonyPrefix]
            public static bool OrderToPod_Prefix(ref CompBiosculpterPod_Cycle cycle, ref Pawn pawn, ref Action giveJobAct)
            {
                if (cycle is CompBiosculpterPod_HealingCycle healingCycle && healingCycle is ITargetedHealingCycle targetedHealingCycle)
                {
                    List<Hediff> maybeHeal = new List<Hediff>();
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (healingCycle.CanPotentiallyHeal(pawn, hediff))
                        {
                            maybeHeal.Add(hediff);
                        }
                    }
                    List<Hediff> willHeal = new List<Hediff>();
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (healingCycle.WillHeal(pawn, hediff))
                        {
                            willHeal.Add(hediff);
                        }
                    }
                    SB_Dialog_HediffSelection.CreateDialog(targetedHealingCycle, pawn, maybeHeal, willHeal, giveJobAct);
                }
                else
                {
                    giveJobAct();
                }
                return false;
            }
        }

        /*public static class FBP_Patcher
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> ApplySettings_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Log.Message("SelectiveBioregeneration - FBP detected");
                return null;
            }
        }*/
    }
}
