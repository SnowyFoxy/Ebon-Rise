using RimWorld;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using System;
using Verse;
using JetBrains.Annotations;


namespace EbonRiseV2.Comps
{
    public class CompAbilityProperties_Swallow : CompProperties_AbilityEffect
    {
        public float maxBodySize = 9f;
        public string texPath;

        public CompAbilityProperties_Swallow()
        {
            compClass = typeof(CompAbility_Swallow);
        }
    }
}
