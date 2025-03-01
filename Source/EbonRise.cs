using HarmonyLib;
using Verse;

namespace EbonRiseV2
{
    public class EbonRise : Mod
    {
        public EbonRise(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("SnowFox.EbonRiseV2");
            harmony.PatchAll();
        }
    }
}