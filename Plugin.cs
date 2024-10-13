using BepInEx;
using BepInEx.Configuration;
using shadowflickerfix.patches;
using shadowflickerfix.Helpers;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using EFT.Settings.Graphics;
using EFT.EnvironmentEffect;
using System.Reflection;

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
        // Distant Shadows
        public static ConfigEntry<DistantShadow.ResolutionState> ResolutionState { get; set; }

        // Shadows
        public static ConfigEntry<bool> ShadowChangesEnabled { get; set; }
        public static ConfigEntry<EShadowCascades> ShadowCascades { get; set; }
        public static ConfigEntry<ShadowResolution> ShadowResolution { get; set; }
        public static ConfigEntry<float> ShadowDecreaseFactor { get; set; }
        public static ConfigEntry<float> ShadowMinimumDistance { get; set; }
        public static ConfigEntry<Vector2> ShadowIntervalFirst { get; set; }
        public static ConfigEntry<Vector2> ShadowIntervalSecond { get; set; }

        // Anti-Aliasing
        public static ConfigEntry<bool> SMAAEnabled { get; set; }
        public static ConfigEntry<SubpixelMorphologicalAntialiasing.Quality> SMAAQuality { get; set; }

        public static void OnUpdateSettings(object sender, EventArgs args)
        {
            DistantShadow distantShadow = Util.GetDistantShadow();
            PostProcessLayer ppLayer = Util.GetPostProcessLayer();
            EnvironmentManager envManager = Util.GetEnvironmentManager();
            SubpixelMorphologicalAntialiasing smaa = ppLayer.subpixelMorphologicalAntialiasing;
            Camera camera = Util.GetCamera();

            if (distantShadow != null)
            {
                distantShadow.CurrentMaskResolution = ResolutionState.Value;
            }

            if (envManager != null && ShadowChangesEnabled.Value == true)
            {
                FieldInfo shadowMin = typeof(EnvironmentManager).GetField("ShadowMinDistance", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo shadowInterval1 = typeof(EnvironmentManager).GetField("ShadowInterval1", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo shadowInterval2 = typeof(EnvironmentManager).GetField("ShadowInterval2", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo shadowDecreaseFactor = typeof(EnvironmentManager).GetField("ShadowDecreaseFactor", BindingFlags.Instance | BindingFlags.NonPublic);

                QualitySettings.shadowCascades = (int)ShadowCascades.Value;
                QualitySettings.shadowResolution = ShadowResolution.Value;
                shadowMin.SetValue(envManager, ShadowMinimumDistance.Value);
                shadowInterval1.SetValue(envManager, ShadowIntervalFirst.Value);
                shadowInterval2.SetValue(envManager, ShadowIntervalSecond.Value);
                shadowDecreaseFactor.SetValue(envManager, ShadowDecreaseFactor.Value);
            }

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
            string distantShadows = "1. Distant Shadows";
            string shadows = "2. Shadows";
            string antialias = "3. Anti-Aliasing";

            // Distant Shadows
            ResolutionState = Config.Bind(distantShadows, "Distant Shadow Resolution", DistantShadow.ResolutionState.FULL, new ConfigDescription(
                    "Changes the resolution of distant shadows. Higher values reduce flickering but come with a small (barely noticeable) performance impact.",
                    null,
                    new ConfigurationManagerAttributes { Order = 1000 }
                ));

            // Shadows
            ShadowChangesEnabled = Config.Bind(shadows, "Enable Shadow Changes", false, new ConfigDescription(
                    "SOME OF THESE SETTINGS WILL REDUCE FPS NOTICEABLY! Enables shadow config changes. These settings can either make shadows look really nice or really bad. Map change required after disabling.",
                    null,
                    new ConfigurationManagerAttributes { Order = 999 }
                ));

            ShadowCascades = Config.Bind(shadows, "Shadow Cascades", EShadowCascades.High, new ConfigDescription(
                    "Changes the amount of shadow cascades. Didn't see a noticeable performance impact.",
                    null,
                    new ConfigurationManagerAttributes { Order = 990 }
                ));

            ShadowResolution = Config.Bind(shadows, "Shadow Quality", UnityEngine.ShadowResolution.VeryHigh, new ConfigDescription(
                    "Changes the shadowmap resolution. Will reduce FPS.",
                    null,
                    new ConfigurationManagerAttributes { Order = 990 }
                ));

            ShadowDecreaseFactor = Config.Bind(shadows, "Shadow Decrease Factor", 2f, new ConfigDescription(
                    "Changes the shadow decrease factor. Lowering the value sharpens the shadows considerably but also reduces FPS.",
                    new AcceptableValueRange<float>(0.01f, 5f),
                    new ConfigurationManagerAttributes { Order = 990 }
                ));

            ShadowMinimumDistance = Config.Bind(shadows, "Minimum Shadow Distance", 20f, new ConfigDescription(
                    "Changes the minimum shadow distance. At least that's my very best guess. No clue what it actually does, but it's here if you want to play around with it.",
                    null,
                    new ConfigurationManagerAttributes { Order = 990 }
                ));

            ShadowIntervalFirst = Config.Bind(shadows, "Shadow Interval 1", new Vector2(25, 45), new ConfigDescription(
                    "Changes the distance at which shadows start fading from one quality to another. I think.",
                    null,
                    new ConfigurationManagerAttributes { Order = 990 }
                ));

            ShadowIntervalSecond = Config.Bind(shadows, "Shadow Interval 2", new Vector2(50, 100), new ConfigDescription(
                    "Changes the distance at which shadows start fading from one quality to another. I think.",
                    null,
                    new ConfigurationManagerAttributes { Order = 990 }
                ));

            // Anti-Aliasing
            SMAAEnabled = Config.Bind(antialias, "SMAA", false, new ConfigDescription(
                    "Enables SMAA. TAA and Tarkov's FXAA suck. Simple as. DO NOT use with upscaling because it makes the image really blurry.",
                    null,
                    new ConfigurationManagerAttributes { Order = 980 }
                ));

            SMAAQuality = Config.Bind(antialias, "SMAA Quality", SubpixelMorphologicalAntialiasing.Quality.High, new ConfigDescription(
                    "Changes SMAA Quality.",
                    null,
                    new ConfigurationManagerAttributes { Order = 970 }
                ));

            // Method binds
            ResolutionState.SettingChanged += OnUpdateSettings;

            ShadowCascades.SettingChanged += OnUpdateSettings;
            ShadowChangesEnabled.SettingChanged += OnUpdateSettings;
            ShadowCascades.SettingChanged += OnUpdateSettings;
            ShadowResolution.SettingChanged += OnUpdateSettings;
            ShadowDecreaseFactor.SettingChanged += OnUpdateSettings;
            ShadowMinimumDistance.SettingChanged += OnUpdateSettings;
            ShadowIntervalFirst.SettingChanged += OnUpdateSettings;
            ShadowIntervalSecond.SettingChanged += OnUpdateSettings;

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
