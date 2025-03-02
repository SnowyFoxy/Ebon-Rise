using RimWorld;
using Verse;

namespace EbonRiseV2.Jobs
{
    public class JobGiver_AIFightIgnoringFriendly : JobGiver_AIAbilityFight
    {
        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            return target.Faction == Faction.OfPlayer && (!humanlikesOnly || target is not Pawn pawn1 || pawn1.RaceProps.Humanlike);
        }
    }
}