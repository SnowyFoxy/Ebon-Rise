using System.Collections.Generic;
using EbonRiseV2.Comps;
using EbonRiseV2.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobDriver_Escape : JobDriver
    {
        private static int SmearMTBTicks = 60;

        private Comp_Stalker Comp => pawn.TryGetComp<Comp_Stalker>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            pawn.jobs.jobQueue.Clear(pawn, true);
            Toil toil1 = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            toil1.tickAction += () =>
            {
                if (Comp.Pawn.IsPsychologicallyInvisible())
                {
                    ReadyForNextToil();
                }

                if (Find.TickManager.TicksGame % SmearMTBTicks == 0)
                {
                    FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.Filth_RevenantSmear);
                }
            };
            toil1.AddFinishAction(() =>
            {
                Log.Message("Trying spawn! " + (Comp.lastFurClumpTick + 10000) + " > " + Find.TickManager.TicksGame + ", " + 
                    Find.AnalysisManager.TryGetAnalysisProgress(Comp.biosignature, out var test));
                if (Comp.lastFurClumpTick + 10000 > Find.TickManager.TicksGame || 
                    Find.AnalysisManager.TryGetAnalysisProgress(Comp.biosignature, out var details) && details.Satisfied)
                {
                    return;
                }
                Log.Message("Spawning!");
                Thing furClump = ThingMaker.MakeThing(MiscDefOf.SF_FurClump);
                furClump.TryGetComp<CompAnalyzableBiosignature>().biosignature = Comp.biosignature;
                Thing spawnedFurClump = GenSpawn.Spawn(furClump, pawn.PositionHeld, pawn.Map);
                Find.LetterStack.ReceiveLetter(Comp.StalkerProps.furClumpDroppedLabel.Formatted(), 
                    Comp.StalkerProps.furClumpDroppedDesc.Formatted(), 
                    LetterDefOf.NeutralEvent, spawnedFurClump, delayTicks: 600);
                Comp.lastFurClumpTick = Find.TickManager.TicksGame;
            });
            yield return toil1;
            Toil toil2 = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            toil2.AddFinishAction(() =>
            {
                Log.Message("Finished! " + job.GetTarget(TargetIndex.B));
                Comp.stalkerState = StalkerState.Digesting;
            });
            yield return toil2;
        }
    }
}