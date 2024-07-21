using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

class CompSwallow : ThingComp, IThingHolder
{

    private ThingOwner<Thing> innerContainer;

    private int ticksDigesting;

    private int ticksToDigestFully;

    private int ticksToSwallow;

    private bool wasDrafted;

    public CompProperties_Swallow Props => (CompProperties_Swallow)props;

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

