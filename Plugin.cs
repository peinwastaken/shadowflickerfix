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

    [BepInPlugin("com.pein.shadowflickerfix", "Shadow Flicker Fix", "1.5.2")]
    public class Plugin : BaseUnityPlugin
    {
        // Distant Shadows
        public static ConfigEntry<DistantShadow.ResolutionState> ResolutionState { get; set; }

        // Shadows
        public static ConfigEntry<bool> ShadowChangesEnabled { get; set; }
        public static ConfigEntry<EShadowCascades> ShadowCascades { get; set; }
        public static ConfigEntry<ShadowResolution> ShadowResolution { get; set; }
        public static ConfigEntry<bool> AdvancedShadowChangesEnabled { get; set; }
        public static ConfigEntry<float> ShadowDecreaseFactor { get; set; }
        public static ConfigEntry<float> ShadowMinimumDistance { get; set; }
        public static ConfigEntry<Vector2> ShadowIntervalFirst { get; set; }
        public static ConfigEntry<Vector2> ShadowIntervalSecond { get; set; }

        // Anti-Aliasing
        public static ConfigEntry<bool> SMAAEnabled { get; set; }
        public static ConfigEntry<SubpixelMorphologicalAntialiasing.Quality> SMAAQuality { get; set; }
        public static ConfigEntry<bool> TAAChangesEnabled { get; set; }
        public static ConfigEntry<float> TAAJitterSpread { get; set; }
        public static ConfigEntry<float> TAAMotionBlending { get; set; }
        public static ConfigEntry<float> TAAStationaryBlending { get; set; }
        public static ConfigEntry<float> TAASharpness { get; set; }


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

            if (envManager != null)
            {
                if (ShadowChangesEnabled.Value == true)
                {
                    QualitySettings.shadowCascades = (int)ShadowCascades.Value;
                    QualitySettings.shadowResolution = ShadowResolution.Value;

                    if (AdvancedShadowChangesEnabled.Value == true)
                    {
                        FieldInfo shadowMin = typeof(EnvironmentManager).GetField("ShadowMinDistance", BindingFlags.Instance | BindingFlags.NonPublic);
                        FieldInfo shadowInterval1 = typeof(EnvironmentManager).GetField("ShadowInterval1", BindingFlags.Instance | BindingFlags.NonPublic);
                        FieldInfo shadowInterval2 = typeof(EnvironmentManager).GetField("ShadowInterval2", BindingFlags.Instance | BindingFlags.NonPublic);
                        FieldInfo shadowDecreaseFactor = typeof(EnvironmentManager).GetField("ShadowDecreaseFactor", BindingFlags.Instance | BindingFlags.NonPublic);

                        shadowMin.SetValue(envManager, ShadowMinimumDistance.Value);
                        shadowInterval1.SetValue(envManager, ShadowIntervalFirst.Value);
                        shadowInterval2.SetValue(envManager, ShadowIntervalSecond.Value);
                        shadowDecreaseFactor.SetValue(envManager, ShadowDecreaseFactor.Value);
                    }
                }
            }

            if (ppLayer != null)
            {
                if (TAAChangesEnabled.Value == true)
                {
                    TemporalAntialiasing taa = ppLayer.temporalAntialiasing;

                    taa.jitterSpread = TAAJitterSpread.Value;
                    taa.motionBlending = TAAMotionBlending.Value;
                    taa.stationaryBlending = TAAStationaryBlending.Value;
                    taa.sharpness = TAASharpness.Value;
                }

                if (SMAAEnabled.Value == true)
                {
                    ppLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                    smaa.quality = SMAAQuality.Value;
                }
                else
                {
                    EAntialiasingMode lastAntiAliasing = SetAntiAliasingPatch.lastAntiAliasingMode;
                    EDLSSMode lastDlss = SetAntiAliasingPatch.lastDlssMode;
                    EFSR2Mode lastFSR = SetAntiAliasingPatch.lastFSR2Mode;
                    EFSR3Mode lastFSR3 = SetAntiAliasingPatch.lastFSR3Mode;

                    CameraClass.Instance.SetAntiAliasing(lastAntiAliasing, lastDlss, lastFSR, lastFSR3);
                }
            }
        }

        private void DoConfig()
        {
            string distantShadows = "1. Distant Shadows";
            string shadows = "2. Shadows";
            string shadowsAdvanced = "3. Shadows (Advanced)";
            string antialias = "4. Anti-Aliasing";

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
                    new ConfigurationManagerAttributes { Order = 990 }
                ));

            ShadowCascades = Config.Bind(shadows, "Shadow Cascades", EShadowCascades.High, new ConfigDescription(
                    "Changes the amount of shadow cascades. Didn't see a noticeable performance impact.",
                    null,
                    new ConfigurationManagerAttributes { Order = 980 }
                ));

            ShadowResolution = Config.Bind(shadows, "Shadow Quality", UnityEngine.ShadowResolution.High, new ConfigDescription(
                    "Changes the shadowmap resolution. Will reduce FPS.",
                    null,
                    new ConfigurationManagerAttributes { Order = 970 }
                ));

            // Advanced Shadows
            AdvancedShadowChangesEnabled = Config.Bind(shadowsAdvanced, "Enable Advanced Shadow Changes", false, new ConfigDescription(
                    "Enables advanced shadow settings. Shadow changes have to be enabled for this to work.",
                    null,
                    new ConfigurationManagerAttributes { Order = 965 }
                ));

            ShadowDecreaseFactor = Config.Bind(shadowsAdvanced, "Shadow Decrease Factor", 0.5f, new ConfigDescription(
                    "Changes the shadow decrease factor. Lowering the value sharpens the shadows considerably but also reduces FPS.",
                    new AcceptableValueRange<float>(0.01f, 5f),
                    new ConfigurationManagerAttributes { Order = 960 }
                ));

            ShadowMinimumDistance = Config.Bind(shadowsAdvanced, "Minimum Shadow Distance", 20f, new ConfigDescription(
                    "Changes the minimum shadow distance. At least that's my very best guess. No clue what it actually does, but it's here if you want to play around with it.",
                    null,
                    new ConfigurationManagerAttributes { Order = 950 }
                ));

            ShadowIntervalFirst = Config.Bind(shadowsAdvanced, "Shadow Interval 1", new Vector2(10f, 50f), new ConfigDescription(
                    "Changes the distance at which shadows start fading from one quality to another. I think.",
                    null,
                    new ConfigurationManagerAttributes { Order = 940 }
                ));

            ShadowIntervalSecond = Config.Bind(shadowsAdvanced, "Shadow Interval 2", new Vector2(75f, 100f), new ConfigDescription(
                    "Changes the distance at which shadows start fading from one quality to another. I think.",
                    null,
                    new ConfigurationManagerAttributes { Order = 930 }
                ));

            // Anti-Aliasing
            SMAAEnabled = Config.Bind(antialias, "SMAA", false, new ConfigDescription(
                    "Enables SMAA. TAA and Tarkov's FXAA suck. Simple as. DO NOT use with upscaling because it makes the image really blurry.",
                    null,
                    new ConfigurationManagerAttributes { Order = 920 }
                ));

            SMAAQuality = Config.Bind(antialias, "SMAA Quality", SubpixelMorphologicalAntialiasing.Quality.High, new ConfigDescription(
                    "Changes SMAA Quality.",
                    null,
                    new ConfigurationManagerAttributes { Order = 910 }
                ));

            TAAChangesEnabled = Config.Bind(antialias, "Enable TAA Changes", false, new ConfigDescription(
                    "Enables TAA changes. Reduces TAA blur on 1080p displays but increases aliasing as a trade off. Requires game restart.",
                    null,
                    new ConfigurationManagerAttributes { Order = 905 }
                ));

            TAAJitterSpread = Config.Bind(antialias, "TAA Jitter Spread", 0.5f, new ConfigDescription(
                    "Changes TAA jitter spread.",
                    null,
                    new ConfigurationManagerAttributes { Order = 900 }
                ));

            TAAMotionBlending = Config.Bind(antialias, "TAA Motion Blending", 0.5f, new ConfigDescription(
                    "Changes TAA motion blending.",
                    null,
                    new ConfigurationManagerAttributes { Order = 890 }
                ));

            TAAStationaryBlending = Config.Bind(antialias, "TAA Stationary Blending", 0.5f, new ConfigDescription(
                    "Changes TAA stationary blending.",
                    null,
                    new ConfigurationManagerAttributes { Order = 880 }
                ));

            TAASharpness = Config.Bind(antialias, "TAA Sharpness", 0.5f, new ConfigDescription(
                    "Changes TAA sharpness. This sharpens the image while in motion and is thus very unstable at higher values.",
                    new AcceptableValueRange<float>(0.01f, 1f),
                    new ConfigurationManagerAttributes { Order = 870 }
                ));

            // Method binds
            ResolutionState.SettingChanged += OnUpdateSettings;

            ShadowCascades.SettingChanged += OnUpdateSettings;
            ShadowChangesEnabled.SettingChanged += OnUpdateSettings;
            ShadowCascades.SettingChanged += OnUpdateSettings;
            ShadowResolution.SettingChanged += OnUpdateSettings;
            AdvancedShadowChangesEnabled.SettingChanged += OnUpdateSettings;
            ShadowDecreaseFactor.SettingChanged += OnUpdateSettings;
            ShadowMinimumDistance.SettingChanged += OnUpdateSettings;
            ShadowIntervalFirst.SettingChanged += OnUpdateSettings;
            ShadowIntervalSecond.SettingChanged += OnUpdateSettings;

            SMAAEnabled.SettingChanged += OnUpdateSettings;
            SMAAQuality.SettingChanged += OnUpdateSettings;
            TAAChangesEnabled.SettingChanged += OnUpdateSettings;
            TAAJitterSpread.SettingChanged += OnUpdateSettings;
            TAAMotionBlending.SettingChanged += OnUpdateSettings;
            TAAStationaryBlending.SettingChanged += OnUpdateSettings;
            TAASharpness.SettingChanged += OnUpdateSettings;
        }

        private void Awake()
        {
            DoConfig();

            new GameStartPatch().Enable();
            new SetAntiAliasingPatch().Enable();
        }
    }
}
