﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

public class Comp_SF_Stalker : ThingComp, IThingHolder
{

    private ThingOwner<Thing> innerContainer;

    private int ticksDigesting;

    private int ticksToDigestFully;

    private int ticksToSwallow;

    private bool wasDrafted;

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
            Thing SwallowedThing = SwallowedThing;
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
        if (target.HasThing && target.Thing is Pawn { Spawned: not false} pawn)
    }
}






