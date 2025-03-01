﻿using EbonRiseV2.Jobs;
using RimWorld;
using Verse;

namespace EbonRiseV2.Util
{
    [DefOf]
    public class MiscDefOf
    {
        public static HediffDef SF_AcidResist;

        public static DamageDef SF_DigestiveAcid_Injury;

        static MiscDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MiscDefOf));
        }
    }
}