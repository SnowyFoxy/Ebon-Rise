using RimWorld;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobGiver_Escape : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            var firstCell = FindEscapeCell(pawn);
            var secondCell = FindEscapeCell(pawn);
            if (firstCell == null || secondCell == null)
            {
                return null;
            }
            Job job1 = JobMaker.MakeJob(JobsDefOf.SF_Stalker_Escape, firstCell.Value, secondCell.Value);
            job1.locomotionUrgency = LocomotionUrgency.Sprint;
            job1.canBashDoors = true;
            job1.canBashFences = true;
            return job1;
        }

        private IntVec3? FindEscapeCell(Pawn pawn)
        {
            IntVec3 escapeCell = RevenantUtility.FindEscapeCell(pawn);
            if (!escapeCell.IsValid)
                return null;
            using PawnPath path1 = pawn.Map.pathFinder.FindPath(pawn.Position, escapeCell, TraverseParms.For(pawn, mode: TraverseMode.PassDoors));
            if (path1.Found) return escapeCell;
            using PawnPath path2 = pawn.Map.pathFinder.FindPath(pawn.Position, escapeCell, TraverseParms.For(pawn, mode: TraverseMode.PassAllDestroyableThings));
            Thing blocker = path2.FirstBlockingBuilding(out var cellBefore, pawn);
            if (blocker != null)
            {
                return null;
            }

            return escapeCell;
        }
    }
}