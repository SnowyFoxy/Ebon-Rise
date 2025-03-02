using EbonRiseV2.Comps;
using EbonRiseV2.Util;
using Verse;
using Verse.AI;

namespace EbonRiseV2.Misc
{
    public class ThinkNode_ConditionalStalkerState : ThinkNode_Conditional
    {
        public StalkerState state;

        protected override bool Satisfied(Pawn pawn)
        {
            Comp_Stalker comp = pawn.TryGetComp<Comp_Stalker>();
            return comp != null && comp.stalkerState == state;
        }
    }
}