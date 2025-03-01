using EbonRiseV2.Comps;
using EbonRiseV2.Util;
using UnityEngine;
using Verse;

namespace EbonRiseV2.Renderer
{
    public class PawnRenderNode_Swallowing : PawnRenderNode_AnimalPart
    {
        public PawnRenderNode_Swallowing(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
            
        }
        
        public override Graphic GraphicFor(Pawn pawn)
        {
            if (!pawn.TryGetComp<Comp_Stalker>(out var comp)) return base.GraphicFor(pawn);
            
            Graphic graphic = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
            
            if (comp.stalkerState == StalkerState.Swallowing)
            {
                Log.Message("Swallowing! " + GraphicDatabase.Get<Graphic_Multi>(graphic.path + "_middle", graphic.Shader, graphic.drawSize, Color.white));
                return GraphicDatabase.Get<Graphic_Multi>(graphic.path + "_middle", graphic.Shader, graphic.drawSize, Color.white);
            }
            
            Log.Message("Swallowed? " + comp.Swallowed);
            return comp.Swallowed ? GraphicDatabase.Get<Graphic_Multi>(graphic.path + "_Closed", graphic.Shader, graphic.drawSize, Color.white) : 
                base.GraphicFor(pawn);
        }
    }
}