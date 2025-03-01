using EbonRiseV2.Comps;
using EbonRiseV2.Util;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EbonRiseV2.Patches
{
    [HarmonyPatch(typeof(Pawn), "get_IsNonMutantAnimal")]
    public class StalkersAreAnimals
    {
        // Make Stalkers an exception to be docile while they're fed by the colony.
        private static bool Prefix(ref bool __result, ref Pawn __instance)
        {
            if (__instance.GetComp<Comp_Stalker>() == null || __instance.Faction != Faction.OfPlayer) return true;
            __result = true;
            return false;
        }
    }
}