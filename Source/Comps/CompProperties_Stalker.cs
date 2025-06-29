using RimWorld;
using Verse;

namespace EbonRiseV2.Comps
{
    public class CompProperties_Stalker : CompProperties_Stunnable
    {
        [MustTranslate] public string jobString;
        [MustTranslate] public string messageSwallowed;
        [MustTranslate] public string messageDigested;
        [MustTranslate] public string messageEmerged;

        [MustTranslate] public string messageEmergedCorpse;

        [MustTranslate] public string messageDigestionCompleted;

        [MustTranslate] public string digestingInspector;
        
        [MustTranslate] public string furClumpDroppedLabel;
        [MustTranslate] public string furClumpDroppedDesc;

        [MustTranslate] public string slainLetterLabel;
        [MustTranslate] public string slainLetterDesc;

        [MustTranslate] public string stalkerSpottedLabel;
        [MustTranslate] public string stalkerSpottedDesc;
        
        [MustTranslate] public string stalkerSensedLabel;
        [MustTranslate] public string stalkerSensedDesc;

        public int completeDigestionDamage = 125;

        public SimpleCurve bodySizeDigestTimeCurve = new()
        {
            new CurvePoint(0.2f, 10f),
            new CurvePoint(1f, 60f),
            new CurvePoint(3.5f, 90f)
        };

        public CompProperties_Stalker()
        {
            compClass = typeof(Comp_Stalker);
        }
    }
}