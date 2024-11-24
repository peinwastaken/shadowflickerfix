using EFT;
using EFT.Settings.Graphics;
using shadowflickerfix.Helpers;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine.Rendering.PostProcessing;

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
            // this is odd. sorry.
            // reset component cache on map load
            Util.ResetComponentCache();

            // update all the components
            Plugin.OnUpdateSettings(null, null);
        }
    }

    public class SetAntiAliasingPatch : ModulePatch
    {
        public static PostProcessLayer.Antialiasing lastAntiAliasing;
        public static EAntialiasingMode lastAntiAliasingMode;
        public static EDLSSMode lastDlssMode;
        public static EFSR2Mode lastFSR2Mode;
        public static EFSR3Mode lastFSR3Mode;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(CameraClass).GetMethod(nameof(CameraClass.SetAntiAliasing));
        }

        [PatchPostfix]
        public static void PostPatchfix(CameraClass __instance, EAntialiasingMode quality, EDLSSMode dlssMode, EFSR2Mode fsr2Mode)
        {
            PostProcessLayer ppLayer = Util.GetPostProcessLayer();
            SubpixelMorphologicalAntialiasing smaa = ppLayer.subpixelMorphologicalAntialiasing;

            lastAntiAliasing = ppLayer.antialiasingMode;
            lastAntiAliasingMode = quality;
            lastDlssMode = dlssMode;
            lastFSR2Mode = fsr2Mode;

            if (Plugin.SMAAEnabled.Value == true)
            {
                ppLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                smaa.quality = Plugin.SMAAQuality.Value;
            }
        }
    }
}
