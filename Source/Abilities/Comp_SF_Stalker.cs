using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using SF_DefOF;

public class Comp_SF_Stalker : ThingComp, IThingHolder
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
    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }


    public Thing SwallowedThing
    {
        get
        {
            if (innerContainer.InnerListForReading.Count <= 0) { return null; }
            return innerContainer.InnerListForReading[0];
        }
    }


    public Pawn SwallowedPawn
    {
        get
        {
            if (SwallowedThing == null)
            {
                return null;
            }
            if(SwallowedThing is Corpse corpse)
            {
                return corpse.InnerPawn;
            }
            return SwallowedThing as Pawn;
        }
    }



    public Thing SwallowThing
    {
        get
        {
            if (innerContainer.InnerListForReading.Count <= 0)
            {
                return null;
            }
            return innerContainer.InnerListForReading[0];
        }
    }

    public Thing DigestingThing
    {
        get
        {
            if (innerContainer.InnerListForReading.Count <= 0)
            {
                return null;
            }
            return innerContainer.InnerListForReading[0];
        }
    }
    public Pawn DigestingPawn
    {
        get
        {
            Thing digestingThing = DigestingThing;
            if (digestingThing == null)
            {
                return null;
            }
            if (digestingThing is Corpse corpse)
            {
                return corpse.InnerPawn;
            }
            return digestingThing as Pawn;
        }
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public Comp_SF_Stalker()
    {
        innerContainer = new ThingOwner<Thing>(this);
    }

    public void StartSwallow(IntVec3 origin, LocalTargetInfo target)
    {
        Log.Message("Target.HasThing: " + target.HasThing);
        Log.Message("Target.thing: " + target.Thing);
        if (target.HasThing && target.Thing is Pawn { Spawned: not false } pawn) 
        {
            Log.Message("Past check");
            pawn.DeSpawn();
            innerContainer.TryAdd(pawn);
            //ticksToSwallowed = GetSwallowedTicks() - 30;
            Pawn.jobs.StartJob(JobMaker.MakeJob(SF_DefOf.SF_Stalker_swallow), JobCondition.InterruptForced);
            if (!Props.messageSwallowed.NullOrEmpty() && pawn.Faction == Faction.OfPlayer)
            {
                Messages.Message(Props.messageSwallowed.Formatted(pawn.Named("PAWN")), Pawn, MessageTypeDefOf.NegativeEvent);
            }
            Pawn.Drawer.renderer.SetAllGraphicsDirty();
            Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerConsumeLeap, Pawn));
            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>( Props + "_middle_");
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
            }
        }
        else
        {
            Log.Message("Failed");
            Pawn.abilities.GetAbility(SF_AbilityDefOf.SF_SwallowAbility).ResetCooldown();
        }
    }
    public void StartDigesting(IntVec3 origin, LocalTargetInfo target)
    {
        if (target.HasThing && target.Thing is Pawn pawn)
        {
            //DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn,);
        }
    }
    /*
    public int GetSwallowedTicks()
    {

    }
    */
    public void CompleteSwallow()
    {
        if (Swallowed)
        {
            Pawn pawn = DropPawn(Pawn.MapHeld);
            Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerDigestionCompleted, Pawn));
            DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, Props.completeDigestionDamage, 0f, -1f, Pawn);
            dinfo.SetApplyAllDamage(value: true);
            pawn.TakeDamage(dinfo);
            if (!Props.messageDigestionCompleted.NullOrEmpty() && !pawn.Dead && pawn.Faction == Faction.OfPlayer)
            {
                Messages.Message(Props.messageDigestionCompleted.Formatted(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
            }
            Pawn.Drawer.renderer.SetAllGraphicsDirty();
            if (Pawn.Drawer.renderer.CurAnimation == AnimationDefOf.DevourerDigesting)
            {
                Pawn.Drawer.renderer.SetAnimation(null);
            }
        }
    }
    private void AbortSwallow(Map map)
    {
        if (!Swallowed)
        {
            return;
        }
        Pawn pawn = DropPawn(map);
        Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerDigestionAborted, Pawn));
        float amount = Props.timeDamageCurve.Evaluate((float)ticksDigesting / 60f);
        DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn, amount, 0f, -1f, Pawn);
        dinfo.SetApplyAllDamage(value: true);
        pawn.TakeDamage(dinfo);
        if (pawn.Faction == Faction.OfPlayer)
        {
            string str = (Pawn.Dead ? Props.messageEmergedCorpse : Props.messageEmerged);
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
        if (!Pawn.Dead && Pawn.CurJobDef == JobDefOf.DevourerDigest && Pawn.jobs.curDriver != null && !Pawn.jobs.curDriver.ended)
        {
            Pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
    }
    public void DigestJobFinished()
	{
		if (1 >= 1)
		{
			CompleteSwallow();
		}
		else
		{
			AbortSwallow(Pawn.MapHeld);
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
    private Pawn DropPawn(Map map)
    {
        if (!Swallowed)
        {
            return null;
        }
        if (!innerContainer.TryDrop(DigestingThing, Pawn.PositionHeld, map, ThingPlaceMode.Near, out var lastResultingThing))
        {
            if (!RCellFinder.TryFindRandomCellNearWith(Pawn.PositionHeld, (IntVec3 c) => c.Standable(map), map, out var result, 1))
            {
                Debug.LogError("Could not drop digesting pawn from devourer!");
                return null;
            }
            lastResultingThing = GenSpawn.Spawn(innerContainer.Take(DigestingThing), result, map);
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
        Scribe_Values.Look(ref ticksDigesting, "ticksDigesting", 0);
        Scribe_Values.Look(ref ticksToDigestFully, "ticksToDigestFully", 0);
        Scribe_Values.Look(ref wasDrafted, "wasDrafted", defaultValue: false);
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
    }

}







