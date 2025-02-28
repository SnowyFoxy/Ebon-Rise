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

    protected Pawn Takee => (Pawn) this.job.GetTarget(TargetIndex.A).Thing;
    
    protected Pawn Feeding => (Pawn) this.job.GetTarget(TargetIndex.B).Thing;


    public override string GetReport()
    {
      return "Feeding rift stalker with " + Takee;
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
      Takee.ClearAllReservations();
      return pawn.Reserve((LocalTargetInfo) (Thing) Takee, job, errorOnFailed: errorOnFailed) &&
             pawn.Reserve((LocalTargetInfo) (Thing) Feeding, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
      Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
      yield return goToTakee;
      
      Log.Message("Carrying " + Takee.stackCount);
      job.count = 1;
      Toil startCarrying = Toils_Haul.StartCarryThing(TargetIndex.A);
      yield return startCarrying;

      Log.Message("Going to target: " +
                  pawn.Map.reachability.CanReach(pawn.Position, Feeding, PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors)));
      Toil goToStalker = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
      yield return goToStalker;
      
      Log.Message("Feeding!");
      Toil toil = Toils_General.Wait(100, TargetIndex.A);
      toil.finishActions = new List<Action>
      {
        () =>
        {
          var comp = Feeding.GetComp<Comp_Stalker>();
          if (comp == null || comp.Swallowed)
          {
            return;
          }
          Log.Message("Swallowing!");
          comp.StartSwallow(Takee);
        }
      };
      yield return toil;
    }
  }
}