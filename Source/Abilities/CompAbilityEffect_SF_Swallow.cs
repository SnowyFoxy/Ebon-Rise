using RimWorld;
using Verse;


public class CompAbilityEffect_SF_Swallow : CompAbilityEffect, ICompAbilityEffectOnJumpCompleted
{
    private new CompProperties_SF_Swallow Props => (CompProperties_SF_Swallow)props;

    public void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target) //AHHHHHHHH WHY IS THIS A LOCATION!?
    {
        Log.Message("OnJumpCompleted called, origin: " +  origin + "LocalTargetInfo: " + target.Label + ((IntVec3)target));
        if (parent.pawn.TryGetComp<Comp_SF_Stalker>(out var comp))
        {
            comp.StartSwallow(origin, target);
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
        if (target.Pawn.BodySize > Props.maxBodySize)
        {
            return false;
        }
        return base.Valid(target, throwMessages);
    }
}

