using RimWorld;
using Verse;

namespace EbonRiseV2.Jobs
{
    [RimWorld.DefOf]
    public static class JobsDefOf
    {
        public static JobDef SF_Stalker_Swallow;
        public static JobDef SF_Stalker_Digest;
        
        static JobsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobsDefOf));
        }
    }
}