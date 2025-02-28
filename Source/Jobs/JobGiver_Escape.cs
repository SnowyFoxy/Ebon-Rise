using RimWorld;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobGiver_Escape : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            IntVec3 escapeCell = RevenantUtility.FindEscapeCell(pawn);
            if (!escapeCell.IsValid)
                return null;
            using (PawnPath path1 = pawn.Map.pathFinder.FindPath(pawn.Position, escapeCell, TraverseParms.For(pawn, mode: TraverseMode.PassDoors)))
            {
                if (!path1.Found)
                {
                    using PawnPath path2 = pawn.Map.pathFinder.FindPath(pawn.Position, escapeCell, TraverseParms.For(pawn, mode: TraverseMode.PassAllDestroyableThings));
                    Thing blocker = path2.FirstBlockingBuilding(out var cellBefore, pawn);
                    if (blocker != null)
                    {
                        Job job = DigUtility.PassBlockerJob(pawn, blocker, cellBefore, true, true);
                        if (job != null)
                            return job;
                    }
                }
            }
            
            Job job1 = JobMaker.MakeJob(JobsDefOf.SF_Stalker_Escape, escapeCell);
            job1.locomotionUrgency = LocomotionUrgency.Sprint;
            job1.canBashDoors = true;
            job1.canBashFences = true;
            return job1;
        }
    }
}