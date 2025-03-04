using EbonRiseV2.Comps;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EbonRiseV2.Patches
{
    [HarmonyPatch(typeof(StunHandler), "Notify_DamageApplied")]
    public class EMPForcesStalkerRelease
    {
        private static void Prefix(DamageInfo dinfo, ref StunHandler __instance)
        {
            if (dinfo.Def != DamageDefOf.EMP || !__instance.parent.TryGetComp<Comp_Stalker>(out var comp)) return;
            if (!comp.Swallowed) return;
            comp.AbortSwallow();
            __instance.StunFor(100, dinfo.Instigator);
        }
    }
}