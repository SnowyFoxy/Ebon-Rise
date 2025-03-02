using EbonRiseV2.Comps;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace EbonRiseV2.Patches
{
    [HarmonyPatch(typeof(SelectionDrawer), "DrawSelectionBracketFor")]
    public class PreventGivingAwayHiddenStalker
    {
        private static bool Prefix(object obj)
        {
            return obj is not Pawn { ParentHolder: Comp_Stalker };
        }
    }
    
    [HarmonyPatch(typeof(CameraJumper), "TryJumpInternal", typeof(Thing), typeof(CameraJumper.MovementMode))]
    public class PreventCameraJumpingOnSwallowed
    {
        private static bool Prefix(Thing thing)
        {
            if (thing.TryGetComp<Comp_Stalker>(out var stalker))
            {
                return thing.Faction == Faction.OfPlayer || !stalker.Swallowed;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(TargetHighlighter), "Highlight")]
    public class PreventTargetHighlighting
    {
        private static bool Prefix(GlobalTargetInfo target)
        {
            return target.Thing is not Pawn { ParentHolder: Comp_Stalker };
        }
    }
}