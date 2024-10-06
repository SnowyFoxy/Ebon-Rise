using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace SF_DefOF
{
    public static class SF_DefOf
    {
        public static JobDef SF_Stalker_swallow;
        public static ThingDef PawnFlyer_SF_Swallow;

        //public static JobDef SF_Stalker_swallow = DefDatabase<JobDef>.GetNamed("SF_Stalker_swallow");
        //public static ThingDef PawnFlyer_SF_Swallow = DefDatabase<ThingDef>.GetNamed("PawnFlyer_SF_Swallow");
        
        static SF_DefOf()
        {
            //DefOfHelper.EnsureInitializedInCtor(typeof(SF_DefOf));
        }
    }    
    public static class SF_AbilityDefOf
    {
        public static AbilityDef SF_SwallowAbility;

        
        static SF_AbilityDefOf()
        {
            //DefOfHelper.EnsureInitializedInCtor(typeof (SF_AbilityDefOf));
        }
    }
    

}

