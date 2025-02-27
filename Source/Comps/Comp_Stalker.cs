using System.Collections.Generic;
using EbonRiseV2.Jobs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using AbilityDefOf = EbonRiseV2.Abilities.AbilityDefOf;

namespace EbonRiseV2.Comps
{
    public class Comp_Stalker : ThingComp, IThingHolder
    {
        private ThingOwner<Thing> innerContainer;

        private int ticksDigesting;
        private int ticksToSwallowed;

        private int ticksToDigestFully;

        private int ticksToSwallowFully;

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


        #region Swallow

        public void StartSwallow(IntVec3 origin, LocalTargetInfo target)
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

            //Attempted to handle image processing, TBD how to properly handle this, perhaps anim def? 
            /*
            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>( Props + "_middle_");
            Log.Message("UpdatedGraphics");
            if (ContentFinder<Texture2D>.Get(Props + "_middle_"))
            {
                if (graphic == null)
                {
                    Log.Warning("Unable to find graphic..." + Props + "_middle_");
                }
                else
                {
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                }
            } */
        }


        //Swallow is still a direct rip off Devourer, it will currently randomly apply damages based on tick rate. instead it should initiate StartDigestion() and apply either no damage or small amounts of 
        //blunt damage to the pawn whos swallowed
        private void CompleteSwallow()
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

            Pawn.Drawer.renderer.SetAllGraphicsDirty();
            if (Pawn.Drawer.renderer.CurAnimation == AnimationDefOf.DevourerDigesting)
            {
                Pawn.Drawer.renderer.SetAnimation(null);
            }

            StartDigesting(pawn);
        }

        private void AbortSwallow(Map map)
        {
            if (!Swallowed)
            {
                return;
            }

            Pawn pawn = DropPawn(map);
            Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerDigestionAborted, Pawn));
            float amount = Props.timeDamageCurve.Evaluate(ticksDigesting / 60f);
            DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, amount, 0f, -1f, Pawn);
            dinfo.SetApplyAllDamage(value: true);
            pawn.TakeDamage(dinfo);
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

        public void SwallowJobFinished()
        {
            if (ticksDigesting >= ticksToDigestFully)
            {
                CompleteSwallow();
            }
            else
            {
                AbortSwallow(Pawn.MapHeld);
            }
        }

        private void EndSwallowedJob()
        {
            if (!Pawn.Dead && Pawn.CurJobDef == JobDefOf.DevourerDigest && Pawn.jobs.curDriver != null &&
                !Pawn.jobs.curDriver.ended)
            {
                Pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        #endregion

        #region Digestion

        //future reference for job removal. if youre having the bug of repeating swallowing, this is why
        public void DigestJobFinished()
        {
            if (1 >= 1)
            {
                CompleteSwallow(); //remember to change this when finished with the method
            }
            else
            {
                AbortSwallow(Pawn.MapHeld);
            }
        }

        private void StartDigesting(LocalTargetInfo target)
        {
            if (target is not { HasThing: true, Thing: Pawn { Spawned: false } pawn })
            {
                Pawn.abilities.GetAbility(AbilityDefOf.SF_Swallow).ResetCooldown();
                return;
            }
            
            DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, 99f, 0f, -1f, parent);
            pawn.GetLord()?.Notify_PawnDamaged(pawn, dinfo);
            ticksDigesting = 0;
            ticksToDigestFully = 30;
            Pawn.jobs.StartJob(JobMaker.MakeJob(JobsDefOf.SF_Stalker_Digest), JobCondition.InterruptForced);
            //notification for digestion start
        }

        #endregion

        private Pawn DropPawn(Map map)
        {
            if (!Swallowed)
            {
                return null;
            }

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
            Scribe_Values.Look(ref ticksDigesting, "ticksDigesting");
            Scribe_Values.Look(ref ticksToDigestFully, "ticksToDigestFully");
            Scribe_Values.Look(ref wasDrafted, "wasDrafted", defaultValue: false);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }
    }
}