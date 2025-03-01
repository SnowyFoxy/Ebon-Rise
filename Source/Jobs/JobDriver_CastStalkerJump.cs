using System.Collections.Generic;
using EbonRiseV2.Comps;
using RimWorld;
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
                Find.LetterStack.ReceiveLetter("Rift Stalker Spotted", pawn.Name + " has spotted a Rift Stalker!",
                    LetterDefOf.ThreatBig,
                    (Thing)pawn);
                comp.lastSeenLetterTick = Find.TickManager.TicksGame;
                comp.Invisibility.BecomeVisible();
                comp.becomeInvisibleTick = Find.TickManager.TicksGame + 300;
            });
            yield return toil;
        }
    }
}