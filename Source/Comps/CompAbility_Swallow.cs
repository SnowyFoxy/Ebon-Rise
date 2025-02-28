using RimWorld;
using Verse;


namespace EbonRiseV2.Comps
{
    public class CompAbility_Swallow : CompAbilityEffect, ICompAbilityEffectOnJumpCompleted
    {
        private new CompAbilityProperties_Swallow Props => (CompAbilityProperties_Swallow)props;

        public void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target) //AHHHHHHHH WHY IS THIS A LOCATION!?
        {
            Log.Message("OnJumpCompleted called, origin: " + origin + "LocalTargetInfo: " + target.Label + target.Cell);
            if (parent.pawn.TryGetComp<Comp_Stalker>(out var comp))
            {
                comp.StartSwallow(target);
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            Pawn pawn = target.Pawn;
            if (pawn == null)
            {
                return false;
            }

            return pawn.BodySize <= Props.maxBodySize;
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return !(target.Pawn.BodySize > Props.maxBodySize) && base.Valid(target, throwMessages);
        }
    }
}
