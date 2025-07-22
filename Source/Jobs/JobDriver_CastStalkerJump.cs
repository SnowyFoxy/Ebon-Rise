using System.Collections.Generic;
using EbonRiseV2.Comps;
using RimWorld;
using UnityEngine;
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

        Toil DelayDuration(int ticks)
        {

            Toil delayToil = new Toil();
            delayToil.defaultCompleteMode = ToilCompleteMode.Delay;
            delayToil.defaultDuration = ticks;
            Debug.Log("It works fooker");
            delayToil.initAction = () => pawn.pather.StopDead();

            return delayToil;
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            
            yield return DelayDuration(500);
            var toil = Toils_Combat.CastVerb(TargetIndex.A);
            
            toil.AddPreInitAction(() =>
            {
                var comp = pawn.GetComp<Comp_Stalker>();
                if (comp == null) return;
                if (pawn.Faction != Faction.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter(comp.StalkerProps.stalkerSpottedLabel.Formatted(),
                        comp.StalkerProps.stalkerSpottedLabel.Formatted(
                            job.GetTarget(TargetIndex.A).Thing.Named("PAWN")),
                        LetterDefOf.ThreatSmall,
                        (Thing)pawn);
                }

                
                comp.lastSeenLetterTick = Find.TickManager.TicksGame;
                comp.Invisibility.BecomeVisible();
                comp.becomeInvisibleTick = Find.TickManager.TicksGame + 300;
            });
            yield return toil;
        }
    }
}
