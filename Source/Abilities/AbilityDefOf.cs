using RimWorld;
using Verse;

namespace EbonRiseV2.Abilities
{
    [RimWorld.DefOf]
    public static class AbilityDefOf
    {
        public static AbilityDef SF_Swallow;
        public static ThingDef PawnFlyer_SF_Swallow;
        
        static AbilityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
        }
    }
}

