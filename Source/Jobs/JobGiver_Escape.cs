using System.Collections;
using System.Collections.Generic;
using EbonRiseV2.Util;
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
            job1.canBashDoors = true;
            job1.canBashFences = true;
            job1.locomotionUrgency = LocomotionUrgency.Jog;
            return job1;
        }
        
        
        /* The escape code had to be reworked upon the update of 1.6
         * 
         * 
         */

        public static IntVec3? FindEscapeCell(Pawn pawn)
        {

            IntVec3 escapeCell = RevenantUtility.FindEscapeCell(pawn);
            if (!escapeCell.IsValid)
                return null;
            
            
            if (CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 100, (IntVec3 x) => x.Standable(pawn.Map) &&              
                                   RevenantUtility.NearbyHumanlikePawnCount(x, pawn.Map, 20f) == 0 &&
                                   pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly),
                    out var result))
            {
                return result;
            }
            return null;
            
            
        }
    }
}
