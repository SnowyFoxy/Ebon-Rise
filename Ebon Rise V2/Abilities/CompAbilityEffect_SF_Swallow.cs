using RimWorld;
using Verse;


class CompAbilityEffect_SF_Swallow : CompAbilityEffect, ICompAbilityEffectOnJumpCompleted
{
    private new CompProperties_SF_Swallow Props => (CompProperties_SF_Swallow)props;

    public void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target)
    {
        if (parent.pawn.TryGetComp<Comp_SF_Stalker>(out var comp))
        {
            comp.StarSwallow
        }
    }
}

