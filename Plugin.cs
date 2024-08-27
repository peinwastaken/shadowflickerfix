using BepInEx;
using BepInEx.Configuration;
using shadowflickerfix.patches;
using shadowflickerfix.Helpers;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using EFT.Settings.Graphics;

namespace shadowflickerfix
{
    public enum EShadowCascades
    {
        Low = 2,
        High = 4,
    }

    [BepInPlugin("com.pein.shadowflickerfix", "Shadow Flicker Fix", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<DistantShadow.ResolutionState> ResolutionState { get; set; }
        public static ConfigEntry<EShadowCascades> ShadowCascades { get; set; }

        public static ConfigEntry<bool> SMAAEnabled { get; set; }
        public static ConfigEntry<SubpixelMorphologicalAntialiasing.Quality> SMAAQuality { get; set; }

        public static void OnUpdateSettings(object sender, EventArgs args)
        {
            DistantShadow distantShadow = Util.GetDistantShadow();
            PostProcessLayer ppLayer = Util.GetPostProcessLayer();
            SubpixelMorphologicalAntialiasing smaa = ppLayer.subpixelMorphologicalAntialiasing;
            Camera camera = Util.GetCamera();

            if (distantShadow != null)
            {
                distantShadow.CurrentMaskResolution = ResolutionState.Value;
            }

            QualitySettings.shadowCascades = (int)ShadowCascades.Value;

            // as much as i would like to have the damn setting in the settings menu
            // idk how to do that so... gg? maybe one day lol
            if (ppLayer != null && SMAAEnabled.Value == true)
            {
                ppLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                smaa.quality = SMAAQuality.Value;
            }
            else
            {
                EAntialiasingMode lastAntiAliasing = SetAntiAliasingPatch.lastAntiAliasingMode;
                EDLSSMode lastDlss = SetAntiAliasingPatch.lastDlssMode;
                EFSR2Mode lastFSR = SetAntiAliasingPatch.lastFSR2Mode;

                CameraClass.Instance.SetAntiAliasing(lastAntiAliasing, lastDlss, lastFSR);
            }
        }

        private void DoConfig()
        {
            string shadows = "1. Shadows";
            string antialias = "2. Anti-Aliasing";

            ResolutionState = Config.Bind(shadows, "Distant Shadow Resolution", DistantShadow.ResolutionState.FULL, new ConfigDescription(
                    "Changes the resolution of distant shadows. Higher values reduce flickering but come with a small (if barely noticeable) performance impact.",
                    null,
                    new ConfigurationManagerAttributes { Order = 1000 }
                ));

            ShadowCascades = Config.Bind("General", "Shadow Cascades", EShadowCascades.Low, new ConfigDescription(
                "Changes the amount of shadow cascades. Doesn't do anything that the graphics settings already don't. Didn't see a noticeable performance impact.",
                null,
                new ConfigurationManagerAttributes { Order = 990 }
            ));

            SMAAEnabled = Config.Bind(antialias, "SMAA", false, new ConfigDescription(
                    "Enables SMAA. TAA and Tarkov's FXAA suck. Simple as.",
                    null,
                    new ConfigurationManagerAttributes { Order = 980 }
                ));

            SMAAQuality = Config.Bind(antialias, "SMAA Quality", SubpixelMorphologicalAntialiasing.Quality.High, new ConfigDescription(
                    "Changes SMAA Quality.",
                    null,
                    new ConfigurationManagerAttributes { Order = 970 }
                ));

            ResolutionState.SettingChanged += OnUpdateSettings;
            ShadowCascades.SettingChanged += OnUpdateSettings;
            SMAAEnabled.SettingChanged += OnUpdateSettings;
            SMAAQuality.SettingChanged += OnUpdateSettings;
        }

        private void Awake()
        {
            DoConfig();

            new GameStartPatch().Enable();
            new SetAntiAliasingPatch().Enable();
        }
    }
}
