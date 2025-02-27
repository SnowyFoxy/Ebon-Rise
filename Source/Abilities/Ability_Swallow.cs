using RimWorld;
using Verse;

namespace EbonRiseV2.Abilities
{
    public class Ability_Swallow : Verb_CastAbilityJump
    {
        public override ThingDef JumpFlyerDef => AbilityDefOf.PawnFlyer_SF_Swallow;
    }   
}