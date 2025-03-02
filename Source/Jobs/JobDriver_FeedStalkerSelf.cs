using System;
using System.Collections.Generic;
using EbonRiseV2.Comps;
using EbonRiseV2.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobDriver_FeedStalkerSelf : JobDriver
    {
        protected Pawn Feeding => (Pawn)this.job.GetTarget(TargetIndex.A).Thing;


        public override string GetReport()
        {
            return "Feeding self to rift stalker";
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve((LocalTargetInfo)(Thing)Feeding, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, true);
            Toil toil = Toils_General.Wait(100);
            toil.initAction += () =>
            {
                var comp = Feeding.GetComp<Comp_Stalker>();
                if (comp == null || comp.Swallowed)
                {
                    return;
                }

                Feeding.SetFaction(Faction.OfPlayer);
                // TODO fix the error that occurs here
                comp.StartSwallow(pawn);
            };
            toil.AddFinishAction(() => Feeding.GetComp<Comp_Stalker>()?.CompleteSwallow());
            yield return toil;
        }
    }
}