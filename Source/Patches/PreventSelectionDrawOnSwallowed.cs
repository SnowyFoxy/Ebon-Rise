using EbonRiseV2.Comps;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace EbonRiseV2.Patches
{
    [HarmonyPatch(typeof(SelectionDrawer), "DrawSelectionBracketFor")]
    public class PreventSelectionDrawOnSwallowed
    {
        private static bool Prefix(object obj)
        {
            return obj is not Pawn { ParentHolder: Comp_Stalker };
        }
    }
    
    [HarmonyPatch(typeof(ColonistBarColonistDrawer), "HandleClicks")]
    public class PreventCameraJumpingOnSwallowed
    {
        private static bool Prefix(Pawn colonist)
        {
            return Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 && colonist is not { ParentHolder: Comp_Stalker };
        }
    }
}