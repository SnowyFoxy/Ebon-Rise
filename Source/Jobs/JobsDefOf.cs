using RimWorld;
using Verse;

namespace EbonRiseV2.Jobs
{
    [DefOf]
    public static class JobsDefOf
    {
        public static JobDef SF_Stalker_Swallow;
        public static JobDef SF_Stalker_Escape;
        public static JobDef SF_Stalker_Digest;
        public static JobDef SF_Stalker_FeedStalker;
        
        static JobsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobsDefOf));
        }
    }
}