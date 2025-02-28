using RimWorld;
using Verse;

namespace EbonRiseV2.Comps
{
    public class CompProperties_SF_Stalker : CompProperties_Interactable
    {
        [MustTranslate] public string messageSwallowed;
        [MustTranslate] public string messageDigested;
        [MustTranslate] public string messageEmerged;

        [MustTranslate] public string messageEmergedCorpse;

        [MustTranslate] public string messageDigestionCompleted;

        [MustTranslate] public string digestingInspector;

        public int completeDigestionDamage = 125;

        public SimpleCurve bodySizeDigestTimeCurve = new()
        {
            new CurvePoint(0.2f, 10f),
            new CurvePoint(1f, 60f),
            new CurvePoint(3.5f, 90f)
        };

        public CompProperties_SF_Stalker()
        {
            compClass = typeof(Comp_Stalker);
        }
    }
}