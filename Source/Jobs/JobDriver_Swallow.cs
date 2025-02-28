using System.Collections.Generic;
using EbonRiseV2.Comps;
using RimWorld;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobDriver_Swallow : JobDriver
    {
        private Comp_Stalker comp;
        private Comp_Stalker Comp => comp ??= pawn.GetComp<Comp_Stalker>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override string GetReport()
        {
            return null;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = Toils_General.Wait(100).WithProgressBarToilDelay(TargetIndex.None, 100);
            toil.FailOn(() => !Comp.Swallowed);
            toil.PlaySustainerOrSound(SoundDefOf.Pawn_Devourer_Digesting);
            toil.AddFinishAction(Comp.CompleteSwallow);
            yield return toil;
        }
    }
}