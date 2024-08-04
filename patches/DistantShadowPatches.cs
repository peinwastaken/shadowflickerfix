using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using static DistantShadow;

namespace shadowflickerfix.patches
{
    public class GameStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(GameWorld __instance)
        {
            Plugin.OnUpdateSettings(null, null);
        }
    }
}
