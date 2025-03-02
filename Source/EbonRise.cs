using HarmonyLib;
using Verse;

namespace EbonRiseV2
{
    public class EbonRise : Mod
    {
        public EbonRise(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("DeVout.EbonRiseV2");
            harmony.PatchAll();
        }
    }
}