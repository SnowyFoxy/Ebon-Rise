using System.Collections.Generic;
using EbonRiseV2.Jobs;
using EbonRiseV2.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using AbilityDefOf = EbonRiseV2.Abilities.AbilityDefOf;

namespace EbonRiseV2.Comps
{
    public class Comp_Stalker : ThingComp, IThingHolder
    {
        private static int DigestTickInterval = 100;
        
        public StalkerState stalkerState;
        
        private ThingOwner<Thing> innerContainer;
        private int ticksToDigestFully;
        private bool wasDrafted;

        public bool Swallowed => SwallowedThing != null;
        public Pawn Pawn => parent as Pawn;

        public CompProperties_SF_Stalker Props => (CompProperties_SF_Stalker)props;

        public Thing SwallowedThing =>
            innerContainer.InnerListForReading.Count <= 0 ? null : innerContainer.InnerListForReading[0];

        public Pawn SwallowedPawn
        {
            get
            {
                return SwallowedThing switch
                {
                    null => null,
                    Corpse corpse => corpse.InnerPawn,
                    _ => SwallowedThing as Pawn
                };
            }
        }

        private HediffComp_Invisibility invisibility;
        public HediffComp_Invisibility Invisibility
        {
            get
            {
                if (invisibility != null)
                    return invisibility;
                Hediff firstHediffOfDef = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.HoraxianInvisibility) ??
                                          Pawn.health.AddHediff(HediffDefOf.HoraxianInvisibility);
                return invisibility = firstHediffOfDef?.TryGetComp<HediffComp_Invisibility>();
            }
        }
        
        public int GetDigestionTicks()
        {
            return SwallowedThing == null ? 0 : Mathf.CeilToInt(Props.bodySizeDigestTimeCurve.Evaluate(SwallowedPawn.BodySize) * 60f);
        }
        
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public Comp_Stalker()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        public override void CompTick()
        {
            innerContainer.ThingOwnerTick(false);

            if (!Swallowed) return;
            Find.BattleLog.Add(new BattleLogEntry_Event(SwallowedPawn, RulePackDefOf.Event_DevourerDigestionAborted, Pawn));
            if (Find.TickManager.TicksGame % DigestTickInterval == 0)
            {
                DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, 1f, 0f, -1f, Pawn);
                dinfo.SetApplyAllDamage(value: true);
                SwallowedPawn.TakeDamage(dinfo);
            }

            if (!SwallowedPawn.Dead) return;
            AbortSwallow();
        }


        #region Swallow

        public void StartSwallow(LocalTargetInfo target)
        {
            if (target is not { HasThing: true, Thing: Pawn { Spawned: not false } pawn })
            {
                // Have to initiate a cancellation method, if error occurs RiftStalker will try to repeatedly eat, this can cause a softlock in the game, even if RS is destroyed.
                Pawn.abilities.GetAbility(AbilityDefOf.SF_Swallow).StartCooldown(5);
                return;
            }

            pawn.DeSpawn();
            innerContainer.TryAdd(pawn);

            // Attempts to start the job for the devouring, currently, the job will immediately end after ~10 seconds and follow normal devourer procedures. WIP.
            Pawn.jobs.StartJob(JobMaker.MakeJob(JobsDefOf.SF_Stalker_Swallow), JobCondition.InterruptForced);
            
            if (!Props.messageSwallowed.NullOrEmpty() && pawn.Faction == Faction.OfPlayer)
            {
                Messages.Message(Props.messageSwallowed.Formatted(pawn.Named("PAWN")), Pawn,
                    MessageTypeDefOf.NegativeEvent);
            }

            Pawn.Drawer.renderer.SetAllGraphicsDirty();
            Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerConsumeLeap, Pawn));
        }

        public void CompleteSwallow()
        {
            if (!Swallowed) return;
            
            Pawn pawn = SwallowedPawn;
            if (pawn == null)
            {
                Debug.LogError("Unable to digest pawn, pawn is NULL");
                return;
            }

            Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerDigestionCompleted,
                Pawn));
            if (!Props.messageDigestionCompleted.NullOrEmpty() && !pawn.Dead && pawn.Faction == Faction.OfPlayer)
            {
                Messages.Message(Props.messageDigestionCompleted.Formatted(pawn.Named("PAWN")), pawn,
                    MessageTypeDefOf.NegativeEvent);
            }

            // Failed to swallow
            if (pawn.Spawned)
            {
                Pawn.abilities.GetAbility(AbilityDefOf.SF_Swallow).ResetCooldown();
                return;
            }
            
            stalkerState = StalkerState.Escaping;
        }

        private void AbortSwallow()
        {
            if (!Swallowed)
            {
                return;
            }

            Pawn pawn = DropPawn();
            if (pawn.Faction == Faction.OfPlayer)
            {
                string str = Pawn.Dead ? Props.messageEmergedCorpse : Props.messageEmerged;
                if (!str.NullOrEmpty())
                {
                    str = str.Formatted(pawn.Named("PAWN"));
                    Messages.Message(str, pawn, MessageTypeDefOf.NeutralEvent);
                }
            }

            EndSwallowedJob();
            Pawn.Drawer.renderer.SetAllGraphicsDirty();
            if (Pawn.Drawer.renderer.CurAnimation == AnimationDefOf.DevourerDigesting)
            {
                Pawn.Drawer.renderer.SetAnimation(null);
            }
        }

        private void EndSwallowedJob()
        {
            if (!Pawn.Dead && Pawn.CurJobDef == JobDefOf.DevourerDigest && Pawn.jobs.curDriver is { ended: false })
            {
                Pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        #endregion

        private Pawn DropPawn()
        {
            if (!Swallowed)
            {
                return null;
            }

            var map = Pawn.Map;
            if (!innerContainer.TryDrop(SwallowedThing, Pawn.PositionHeld, map, ThingPlaceMode.Near,
                    out var lastResultingThing))
            {
                if (!RCellFinder.TryFindRandomCellNearWith(Pawn.PositionHeld, c => c.Standable(map), map,
                        out var result, 1))
                {
                    Debug.LogError("Could not drop digesting pawn from devourer!");
                    return null;
                }

                lastResultingThing = GenSpawn.Spawn(innerContainer.Take(SwallowedThing), result, map);
            }

            if (lastResultingThing is Corpse corpse)
            {
                return corpse.InnerPawn;
            }

            Pawn pawn = (Pawn)lastResultingThing;
            pawn.stances.stunner.StunFor(60, Pawn, addBattleLog: false, showMote: false);
            if (pawn.drafter != null)
            {
                pawn.drafter.Drafted = wasDrafted;
            }

            return pawn;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref stalkerState, "stalkerState");
            Scribe_Values.Look(ref ticksToDigestFully, "ticksToDigestFully");
            Scribe_Values.Look(ref wasDrafted, "wasDrafted", defaultValue: false);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }
    }
}