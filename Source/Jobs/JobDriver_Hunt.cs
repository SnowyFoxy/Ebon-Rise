using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EbonRiseV2.Jobs
{
    public class JobDriver_Hunt : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            LocalTargetInfo lookAtTarget = job.GetTarget(TargetIndex.B);
            Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            if (lookAtTarget.IsValid)
            {
                toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate
                {
                    pawn.rotationTracker.FaceCell(lookAtTarget.Cell);
                });
                toil.handlingFacing = true;
            }
            toil.AddFinishAction(delegate
            {


            });
            yield return toil;
        }
    }
}
