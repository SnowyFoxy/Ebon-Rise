using RimWorld;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobGiver_Digest : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn) => JobMaker.MakeJob(JobsDefOf.SF_Stalker_Digest);
    }
}