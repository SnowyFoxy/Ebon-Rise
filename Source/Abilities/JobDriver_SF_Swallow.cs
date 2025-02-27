using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;

public class JobDriver_SF_Swallow : JobDriver
    {
    private Comp_SF_Stalker comp;

    private Comp_SF_Stalker Comp => comp ?? (comp = pawn.GetComp<Comp_SF_Stalker>());
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
        Toil toil = Toils_General.Wait(1000).WithProgressBarToilDelay(TargetIndex.None, 1000);
        toil.FailOn(() => !Comp.Swallowed);
        toil.PlaySustainerOrSound(SoundDefOf.Pawn_Devourer_Digesting);
        toil.AddFinishAction(Comp.SwallowJobFinished);
        yield return toil;
    }
}

