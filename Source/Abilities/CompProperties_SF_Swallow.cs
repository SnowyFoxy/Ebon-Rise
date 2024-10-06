using RimWorld;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using System;
using Verse;
using JetBrains.Annotations;


[DefOf]
public class CompProperties_SF_Swallow : CompProperties_AbilityEffect
{        
    public float maxBodySize = 9f;
    public string texPath;
    public CompProperties_SF_Swallow()
    { 
        compClass = typeof(CompAbilityEffect_SF_Swallow);
    }
}

