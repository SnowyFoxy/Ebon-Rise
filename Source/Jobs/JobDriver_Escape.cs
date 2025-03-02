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
                if (Comp.lastFurClumpTick + 10000 < Find.TickManager.TicksGame || Find.AnalysisManager.TryGetAnalysisProgress(Comp.biosignature, out var details) && details.Satisfied)
                {
                    return;
                }
                Thing furClump = ThingMaker.MakeThing(MiscDefOf.SF_FurClump);
                furClump.TryGetComp<CompAnalyzableBiosignature>().biosignature = Comp.biosignature;
                Thing spawnedFurClump = GenSpawn.Spawn(furClump, pawn.PositionHeld, pawn.Map);
                Find.LetterStack.ReceiveLetter("Fur Clump", 
                    "A fur clump has fallen from a fleeing Rift Stalker. It could be analyzed to track it down.", 
                    LetterDefOf.NeutralEvent, spawnedFurClump, delayTicks: 600);
                Comp.lastFurClumpTick = Find.TickManager.TicksGame;
            });
            yield return toil1;
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
        }
    }
}