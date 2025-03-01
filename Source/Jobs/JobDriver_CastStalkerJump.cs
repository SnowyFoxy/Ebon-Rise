using System.Collections.Generic;
using EbonRiseV2.Comps;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobDriver_CastStalkerJump : JobDriver_CastVerbOnce
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.StopDead();
            var toil = Toils_Combat.CastVerb(TargetIndex.A);
            toil.AddPreInitAction(() =>
            {
                var comp = pawn.GetComp<Comp_Stalker>();
                if (comp == null) return;
                comp.Invisibility.BecomeVisible();
                comp.becomeInvisibleTick = Find.TickManager.TicksGame + 140;
            });
            yield return toil;
        }
    }
}