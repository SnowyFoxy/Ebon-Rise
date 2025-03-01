using EbonRiseV2.Comps;
using EbonRiseV2.Util;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EbonRiseV2.Patches
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "get_LethalDamageThreshold")]
    public class DigestiveAcidResistance
    {
        private static void Postfix(ref float __result, ref Pawn_HealthTracker __instance)
        {
            if (__instance.hediffSet.HasHediff(MiscDefOf.SF_AcidResist))
            {
                __result *= 3;
            }
        }
    }
}