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
        private const int SmearMTBTicks = 60;
        private int lastBashTick = -9999;
        private int OneShotDoor = 9999;
        private Building_Door lastBashedDoor;

        private Comp_Stalker Comp => pawn.TryGetComp<Comp_Stalker>();

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
    
            //Escape Toil to make the RS escape, If it's stuck inside then it will find the nearest door to beat the shit out of it (One shot) and then escape
            //Original purpose is to beat the door down slowly and then run away but job always expires.
            
            if (Comp == null) yield break;
            
            Toil escapeToil = new Toil();
            escapeToil.defaultCompleteMode = ToilCompleteMode.Never;
            
            escapeToil.initAction = () => 
            {
                lastBashTick = Find.TickManager.TicksGame;
                pawn.pather.StartPath(TargetA.Cell, PathEndMode.OnCell);
            };

            escapeToil.tickAction = () =>
            {

                if (!pawn.pather.Moving && Find.TickManager.TicksGame > lastBashTick + 30)
                {
                    pawn.pather.StartPath(TargetA.Cell, PathEndMode.OnCell);
                }


                if (Find.TickManager.TicksGame % SmearMTBTicks == 0)
                {
                    FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.Filth_RevenantSmear);
                }
                
                if (pawn.Position == TargetA.Cell && 
                    Comp.lastFurClumpTick + 10000 <= Find.TickManager.TicksGame &&
                    (!Find.AnalysisManager.TryGetAnalysisProgress(Comp.biosignature, out var details) || !details.Satisfied))
                {
                    Thing furClump = ThingMaker.MakeThing(MiscDefOf.SF_FurClump);
                    furClump.TryGetComp<CompAnalyzableBiosignature>().biosignature = Comp.biosignature;
                    GenSpawn.Spawn(furClump, pawn.Position, pawn.Map);
                    Find.LetterStack.ReceiveLetter(
                        Comp.StalkerProps.furClumpDroppedLabel.Formatted(), 
                        Comp.StalkerProps.furClumpDroppedDesc.Formatted(), 
                        LetterDefOf.NeutralEvent, 
                        furClump
                    );
                    Comp.lastFurClumpTick = Find.TickManager.TicksGame;
                }

                if (pawn.Position == TargetA.Cell || 
                   (Find.TickManager.TicksGame > lastBashTick + 600 && 
                    RevenantUtility.NearbyHumanlikePawnCount(pawn.Position, pawn.Map, 20f) == 0))
                {
                    Comp.stalkerState = StalkerState.Digesting;
                    ReadyForNextToil();
                }
            };

            yield return escapeToil;
        }
    }
}