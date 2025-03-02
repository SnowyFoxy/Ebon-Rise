using EbonRiseV2.Jobs;
using RimWorld;
using Verse;

namespace EbonRiseV2.Util
{
    [DefOf]
    public class MiscDefOf
    {
        public static HediffDef SF_AcidResist;
        public static HediffDef SF_Invisibility;
        public static DamageDef SF_DigestiveAcid_Injury;
        public static ThingDef SF_FurClump;
        public static PawnKindDef SF_RiftStalker;

        static MiscDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MiscDefOf));
        }
    }
}