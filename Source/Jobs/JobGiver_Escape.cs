using System;
using EbonRiseV2.Jobs;
using RimWorld;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobGiver_Escape : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            
            //Validation check for Pawn -CKG
            IntVec3 c = RevenantUtility.FindEscapeCell(pawn);
            if (!c.IsValid)
            {
                return null;
            }

            //Find the path for RS to move to, if it's blocked then it will bash the door down to escape
            using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, c, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false, false, false, true), null, PathEndMode.OnCell, null))
            {
                if (!pawnPath.Found)
                {
                    using (PawnPath pawnPath2 = pawn.Map.pathFinder.FindPathNow(pawn.Position, c, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false, false, false, true), null, PathEndMode.OnCell, null))
                    {
                        IntVec3 cellBeforeBlocker;
                        Thing thing = pawnPath2.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                        if (thing != null)
                        {
                            // Try to create an attack job for the blocking building, Some of these checks are ignored -CKG
                            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, thing);
                            if (job != null)
                            {
                                job.maxNumMeleeAttacks = 99999;
                                job.expiryInterval = 2000;
                                job.canBashDoors = true;
                                job.canBashFences = true;
                                return job;
                            }
                        }
                    }
                }
            }

            // Some of these checks don't work/are ignored - CKG
            Job job2 = JobMaker.MakeJob(JobsDefOf.SF_Stalker_Escape, c);
            job2.locomotionUrgency = LocomotionUrgency.Jog;
            job2.canBashDoors = true;
            job2.canBashFences = true;
            job2.expiryInterval = 2000; 
            job2.checkOverrideOnExpire = true; 
            return job2;
        }
        
    }
}