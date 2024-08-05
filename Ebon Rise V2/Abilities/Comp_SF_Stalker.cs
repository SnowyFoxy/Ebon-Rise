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

    private int ticksToDigestFully;

    private int ticksToSwallow;

    private bool wasDrafted;

    public bool Swallowed => SwallowedThing != null;

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

    public Pawn Pawn => parent as Pawn;

    public CompProperties_SF_Stalker Props => (CompProperties_SF_Stalker)props;

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
    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public void StartSwallow(IntVec3 origin, LocalTargetInfo target)
    {
        if (target.HasThing && target.Thing is Pawn { Spawned: not false } pawn)
        {
            pawn.DeSpawn();
            innerContainer.TryAdd(pawn);
            Pawn.jobs.StartJob(JobMaker.MakeJob(SF_DefOf.SF_Stalker_swallow), JobCondition.InterruptForced);
            if (!Props.messageSwallowed.NullOrEmpty() && pawn.Faction == Faction.OfPlayer)
            {
                Messages.Message(Props.messageSwallowed.Formatted(pawn.Named("PAWN")), Pawn, MessageTypeDefOf.NegativeEvent);
            }
            Pawn.Drawer.renderer.SetAllGraphicsDirty();
            Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_DevourerConsumeLeap, Pawn));
        }
        else
        {
            Pawn.abilities.GetAbility(SF_AbilityDefOf.SF_Stalker_Swallow).ResetCooldown();
        }
    }
    public void StartDigesting(IntVec3 origin, LocalTargetInfo target)
    {
        if (target.HasThing && target.Thing is Pawn { Spawned: not false } pawn)
        {
            DamageInfo dinfo = new DamageInfo(DamageDefOf.AcidBurn,);
        }
    }
    public void CompleteSwallow()
    {
        if (Swallowed)
        {
            
        }
    }
    
}







