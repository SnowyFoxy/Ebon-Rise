using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace SF_DefOF
{
    [DefOf]
    public static class SF_DefOf
    {
        public static AbilityDef MaxBodySize;

        static SF_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SF_DefOf));
        }

        public static JobDef SF_Stalker_swallow;
    }

    [DefOf]
    public static class SF_AbilityDefOf
    {
        [MayRequireAnomaly]
        public static AbilityDef SF_Stalker_Swallow;
    }


}

