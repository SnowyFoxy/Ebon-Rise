﻿using System;
using System.Collections.Generic;
using System.Linq;
using EbonRiseV2.Jobs;
using EbonRiseV2.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using AbilityDefOf = EbonRiseV2.Abilities.AbilityDefOf;
/*  Some Notes, to dos, ect.
 *  
 *  - Add severity based effects from malnutrition 
 *  - have RS change combat stance to flee when caught
 *  - add a new Hediff which makes RS slow down and cant go invisible when "disturbed from his meal"
 *  
 * 
 */

namespace EbonRiseV2.Comps
{
    public class Comp_Stalker : CompStunnable, IThingHolder
    {
        private static int DigestTickInterval = 600;

        public StalkerState stalkerState;
        public int biosignature;

        private ThingOwner<Thing> innerContainer;
        private int startedDigest;
        private bool wasDrafted;
        private HediffComp_Invisibility invisibility;
        private BodyPartRecord[] targetting;
        private float speedFactor;
        
        
        public int lastSeenLetterTick = -99999;
        public int lastSensedLetterTick = -99999;
        public int lastFurClumpTick = -99999;
        public int becomeInvisibleTick = -99999;
        public int escapeSecondStageStartedTick = -99999;

        public bool Swallowed => SwallowedThing != null;
        public Pawn Pawn => parent as Pawn;

        public CompProperties_Stalker StalkerProps => (CompProperties_Stalker)props;

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

        public HediffComp_Invisibility Invisibility
        {
            get
            {
                if (invisibility != null)
                    return invisibility;
                Hediff firstHediffOfDef = Pawn.health.hediffSet.GetFirstHediffOfDef(MiscDefOf.SF_Invisibility) ??
                                          Pawn.health.AddHediff(MiscDefOf.SF_Invisibility);
                return invisibility = firstHediffOfDef?.TryGetComp<HediffComp_Invisibility>();
            }
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

        public bool ShouldTickContents => true;
        public override void PostPostMake() => biosignature = Rand.Int;
        
        
        public override void CompTick()
        {
            
            if (Pawn.Faction == Faction.OfPlayer && Pawn.needs.food.CurLevel == 0)
            {
                // Leave the player's faction when their stomach is empty
                Pawn.SetFaction(Faction.OfEntities);
                stalkerState = StalkerState.Stalking;
                Pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }

            if (Find.TickManager.TicksGame > becomeInvisibleTick)
            {
                if (stalkerState == StalkerState.Escaping)
                {
                    stalkerState = StalkerState.Digesting;
                }

                Invisibility.BecomeInvisible();
                becomeInvisibleTick = int.MaxValue;
            }

            if (Pawn.IsHashIntervalTick(90) && Pawn.Faction != Faction.OfPlayer)
                CheckIfSeen(false);
            
            if (Pawn.IsHashIntervalTick(60) && Pawn.Faction != Faction.OfPlayer)
                CheckIfSeen(true);

            if (!Swallowed) return;
            
            if (Find.TickManager.TicksGame % DigestTickInterval != 0) return;


            if (Find.TickManager.TicksGame % DigestTickInterval * 10 == 0)
            {
                foreach (var missing in
                         SwallowedPawn.health.hediffSet.hediffs.Where(hediff => hediff is Hediff_MissingPart)
                             .Cast<Hediff_MissingPart>())
                {
                    // Max the age to prevent bleeding on every single wound.
                    missing.ageTicks = 100000;
                }

                GetHitParts();
            }

            DamageInfo dinfo = new DamageInfo(MiscDefOf.SF_DigestiveAcid_Injury, 0.13f,
                0f, -1f, Pawn, spawnFilth: false);
            dinfo.SetApplyAllDamage(value: true);
            dinfo.SetIgnoreArmor(true);
            foreach (var part in targetting)
            {
                dinfo.SetHitPart(part);
                SwallowedPawn.TakeDamage(dinfo);

                if (!Swallowed)
                {
                    return;
                }
            }

            if (SwallowedPawn.Dead)
            {
                AbortSwallow();
                return;
            }

            // Keep the swallowed pawn alive
            SwallowedPawn.health.GetOrAddHediff(MiscDefOf.SF_AcidResist).Severity = 1.0f;
            if (SwallowedPawn.needs.food != null)
            {
                SwallowedPawn.needs.food.CurLevel = 0.01f;
            }

            if (SwallowedPawn.health.hediffSet.TryGetHediff(HediffDefOf.BloodLoss, out var bloodloss))
            {
                bloodloss.Severity = 0.0f;
            }

            Pawn.needs.food.CurLevel += 0.1f;
        }

        public Hediff GetHediff(string defName)
        {
            Hediff hediff = null;
            for (int i = 0; i < Pawn.health.hediffSet.hediffs.Count; i++)
            {
                if (Pawn.health.hediffSet.hediffs[i].def.defName == defName)
                {
                    hediff = Pawn.health.hediffSet.hediffs[i];
                    return hediff;
                }
            }


            return hediff;
        }

        private void CheckIfSeen(bool sensed)
        {
            if (!Find.AnalysisManager.TryGetAnalysisProgress(biosignature, out var details))
                return;
            List<Pawn> colonistsSpawned = Pawn.MapHeld.mapPawns.FreeColonistsSpawned;
            var pawn = colonistsSpawned.FirstOrDefault(pawn => !PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn) &&
                                                               Pawn.PositionHeld.InHorDistOf(pawn.PositionHeld,
                                                                   Math.Min(details.timesDone, 6) * 5.0f * (sensed ? 1.5f : 1.0f)) &&
                                                               (sensed || GenSight.LineOfSightToThing(pawn.PositionHeld, Pawn,
                                                                   Pawn.MapHeld)));
            if (pawn == null)
            {
                return;
            }

            if (Pawn.IsPsychologicallyInvisible() &&
                Find.TickManager.TicksGame > (sensed ? lastSensedLetterTick : lastSeenLetterTick) + 1200)
            {
                if (sensed)
                {
                    lastSensedLetterTick = Find.TickManager.TicksGame;
                    Find.LetterStack.ReceiveLetter(StalkerProps.stalkerSensedLabel.Formatted(), 
                        StalkerProps.stalkerSensedLabel.Formatted(pawn.Named("PAWN")),
                        LetterDefOf.ThreatSmall,
                        (Thing)pawn);
                }
                else
                {
                    lastSeenLetterTick = Find.TickManager.TicksGame;
                    Find.LetterStack.ReceiveLetter(StalkerProps.stalkerSpottedLabel.Formatted(), 
                        StalkerProps.stalkerSpottedLabel.Formatted(pawn.Named("PAWN")),
                        LetterDefOf.ThreatSmall,
                        (Thing)pawn);
                }
            }

            Invisibility.BecomeVisible();
            becomeInvisibleTick = Find.TickManager.TicksGame + 140;
            stalkerState = StalkerState.Escaping;
        }

        private void GetHitParts()
        {
            var maxDamage = 10;
            if ((Find.TickManager.TicksGame - startedDigest) / 60000f > 1f)
            {
                maxDamage = int.MaxValue;
            }

            var hitOrgans = Find.TickManager.TicksGame - startedDigest > 120000;


            targetting = SwallowedPawn.RaceProps.body.AllParts
                .Where(part => !SwallowedPawn.health.hediffSet.PartIsMissing(part) && part.coverage > 0f &&
                               part.def.hitPoints <= maxDamage && ((part.depth == BodyPartDepth.Outside &&
                                                                    part.def.destroyableByDamage && !part.IsCorePart) ||
                                                                   hitOrgans))
                .ToArray();
        }

        public override string CompInspectStringExtra()
        {
            if (!Swallowed)
                return null;
            float ticksToDeath = (startedDigest + 150000 - Find.TickManager.TicksGame) / 2500f;
            return StalkerProps.digestingInspector.Formatted(SwallowedThing.Named("PAWN"),
                ticksToDeath.Named("HOURS"));
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (Pawn.Spawned && Pawn.Faction != Faction.OfPlayer) yield break;
            FloatMenuOption feed = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(StalkerProps.jobString.CapitalizeFirst(), 
                () =>
                {
                    Find.Targeter.BeginTargeting(new TargetingParameters
                    {
                        canTargetBuildings = false,
                        onlyTargetColonistsOrPrisonersOrSlaves = true,
                        validator = t => !t.HasThing || t.Thing is not Pawn thing || 
                                         !PrisonBreakUtility.IsPrisonBreaking(thing) || 
                                         // To prevent DD dragons from being feedable
                                         !t.Thing.def.defName.Contains("Dragon")
                    }, target =>
                    {
                        // Prevent selecting itself
                        if (Pawn == target.Pawn)
                        {
                            return;
                        }
                        selPawn.jobs.StartJob(
                            target == selPawn
                                ? JobMaker.MakeJob(JobsDefOf.SF_Stalker_FeedStalkerSelf, Pawn)
                                : JobMaker.MakeJob(JobsDefOf.SF_Stalker_FeedStalker, target, Pawn),
                            JobCondition.InterruptForced);
                    });
                }), selPawn, Pawn.SpawnedParentOrMe);

            if (Swallowed)
            {
                feed.Disabled = Swallowed || Pawn.needs.food.CurLevel > 0.75f;
                feed.Label += " (Rift Stalker is already digesting " + SwallowedPawn + ")";
            }

            yield return feed;
        }

        public override void Notify_Downed() => AbortSwallow();

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            Find.LetterStack.ReceiveLetter(StalkerProps.slainLetterLabel.Formatted(),
                StalkerProps.slainLetterDesc.Formatted(),
                LetterDefOf.NeutralEvent);
            AbortSwallow();
        }

        #region Swallow

        public void StartSwallow(LocalTargetInfo target)
        {
            if (Swallowed || target is not { HasThing: true, Thing: Pawn { Spawned: true } pawn })
            {
                // Have to initiate a cancellation method, if error occurs RiftStalker will try to repeatedly eat, this can cause a softlock in the game, even if RS is destroyed.
                Pawn.abilities.GetAbility(AbilityDefOf.SF_Swallow).StartCooldown(5);
                return;
            }

            pawn.DeSpawn();
            pawn.health.AddHediff(MiscDefOf.SF_AcidResist);
            
            innerContainer.TryAdd(pawn);
            startedDigest = Find.TickManager.TicksGame;

            // Attempts to start the job for the devouring, currently, the job will immediately end after ~10 seconds and follow normal devourer procedures. WIP.
            Pawn.jobs.StartJob(JobMaker.MakeJob(JobsDefOf.SF_Stalker_Swallow), JobCondition.InterruptForced);

            if (!StalkerProps.messageSwallowed.NullOrEmpty() && pawn.Faction == Faction.OfPlayer)
            {
                Messages.Message(StalkerProps.messageSwallowed.Formatted(pawn.Named("PAWN")), Pawn,
                    MessageTypeDefOf.NegativeEvent);
            }

            stalkerState = StalkerState.Swallowing;
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

            // Failed to swallow
            if (pawn.Spawned)
            {
                Pawn.abilities.GetAbility(AbilityDefOf.SF_Swallow).ResetCooldown();
                return;
            }

            stalkerState = StalkerState.Escaping;
            Pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public void AbortSwallow()
        {
            if (!Swallowed)
            {
                return;
            }

            Find.BattleLog.Add(new BattleLogEntry_Event(SwallowedPawn, RulePackDefOf.Event_DevourerDigestionAborted,
                Pawn));

            Pawn pawn = DropPawn();
            if (pawn != null)
            {
                if (pawn.Faction == Faction.OfPlayer)
                {
                    string str = Pawn.Dead ? StalkerProps.messageEmergedCorpse : StalkerProps.messageEmerged;
                    if (!str.NullOrEmpty())
                    {
                        str = str.Formatted(pawn.Named("PAWN"));
                        Messages.Message(str, pawn, MessageTypeDefOf.NeutralEvent);
                    }
                }
            }
            
            Pawn.Drawer.renderer.SetAllGraphicsDirty();
            if (stalkerState == StalkerState.Digesting)
            {
                stalkerState = StalkerState.Stalking;
            }
        }

        #endregion

        private Pawn DropPawn()
        {
            if (!Swallowed)
            {
                return null;
            }

            var map = Pawn.MapHeld;
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
                corpse.Destroy();
                return null;
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
            Scribe_Values.Look(ref startedDigest, "startedDigest");
            Scribe_Values.Look(ref biosignature, "biosignature");
            Scribe_Values.Look(ref lastFurClumpTick, "lastFurClumpTick");
            Scribe_Values.Look(ref becomeInvisibleTick, "becomeInvisibleTick");
            Scribe_Values.Look(ref wasDrafted, "wasDrafted", defaultValue: false);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }
    }
}