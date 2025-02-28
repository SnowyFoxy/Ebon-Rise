using System;
using System.Collections.Generic;
using EbonRiseV2.Comps;
using EbonRiseV2.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Jobs
{
    public class JobDriver_Escape : JobDriver
    {
        private static int DetectionRangeSquared = 25;
        private static int MinEscapeTime = 300;
        private static int EscapedCheckInterval = 120;
        private static int SmearMTBTicks = 60;

        private Comp_Stalker Comp => pawn.TryGetComp<Comp_Stalker>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var becomeInvisibleTick = Find.TickManager.TicksGame + MinEscapeTime;
            Toil toil1 = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            toil1.tickAction += () =>
            {
                if (Find.TickManager.TicksGame % EscapedCheckInterval == 0 &&
                    Find.TickManager.TicksGame > becomeInvisibleTick &&
                    !pawn.Map.mapPawns.FreeColonistsSpawned.Any(other =>
                        pawn.Position.DistanceToSquared(other.Position) < DetectionRangeSquared))
                {
                    ReadyForNextToil();
                }

                if (Find.TickManager.TicksGame % SmearMTBTicks == 0)
                {
                    FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.Filth_RevenantSmear);
                }
            };
            yield return toil1;

            Toil toil2 = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            toil2.initAction = () => { Comp.Invisibility.BecomeInvisible(); };
            toil2.finishActions = new List<Action>
            {
                () =>
                {
                    Comp.stalkerState = StalkerState.Digesting;
                }
            };
            yield return toil2;
        }
    }
}