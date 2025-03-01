using System;
using System.Collections.Generic;
using EbonRiseV2.Comps;
using RimWorld;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobDriver_FeedStalker : JobDriver
    {
        private const TargetIndex TakeeIndex = TargetIndex.A;
        private const TargetIndex FeedingIndex = TargetIndex.B;

        protected Pawn Takee => (Pawn)this.job.GetTarget(TargetIndex.A).Thing;

        protected Pawn Feeding => (Pawn)this.job.GetTarget(TargetIndex.B).Thing;


        public override string GetReport()
        {
            return "Feeding rift stalker with " + Takee;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Takee.ClearAllReservations();
            return pawn.Reserve((LocalTargetInfo)(Thing)Takee, job, errorOnFailed: errorOnFailed) &&
                   pawn.Reserve((LocalTargetInfo)(Thing)Feeding, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            job.count = 1;
            yield return  Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch, true);
            
            Toil toil = Toils_General.Wait(100);
            toil.initAction += () =>
            {
                var comp = Feeding.GetComp<Comp_Stalker>();
                if (comp == null || comp.Swallowed)
                {
                    return;
                }

                if (pawn.carryTracker.CarriedThing == Takee)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
                }

                // TODO fix the error that occurs here
                comp.StartSwallow(Takee);
            };
            toil.AddFinishAction(() => Feeding.GetComp<Comp_Stalker>()?.CompleteSwallow());
            yield return toil;
        }
    }
}