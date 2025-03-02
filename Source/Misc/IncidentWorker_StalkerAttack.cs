using EbonRiseV2.Comps;
using EbonRiseV2.Util;
using RimWorld;
using Verse;

namespace EbonRiseV2.Misc
{
    public class IncidentWorker_StalkerAttack : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map target = (Map) parms.target;
            if (!RCellFinder.TryFindRandomSpotJustOutsideColony(parms.spawnCenter, target, out var result))
                return false;
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(MiscDefOf.SF_RiftStalker, Faction.OfEntities, tile: target.Tile));
            GenSpawn.Spawn(pawn, result, target);
            pawn.GetComp<Comp_Stalker>().Invisibility.BecomeInvisible();
            return true;
        }
    }
}