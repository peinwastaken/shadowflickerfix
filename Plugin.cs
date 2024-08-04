using BepInEx;
using BepInEx.Configuration;
using shadowflickerfix.patches;
using System;
using UnityEngine;

namespace shadowflickerfix
{
    public enum EShadowCascades
    {
        LOW = 2,
        HIGH = 4,
    }

    [BepInPlugin("com.pein.shadowflickerfix", "Shadow Flicker Fix", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<DistantShadow.ResolutionState> ResolutionState { get; set; }
        public static ConfigEntry<EShadowCascades> ShadowCascades { get; set; }

        public static DistantShadow GetDistantShadow()
        {
            Camera camera = Camera.main;
            return camera.GetComponentInChildren<DistantShadow>();
        }

        public static void OnUpdateSettings(object sender, EventArgs args)
        {
            DistantShadow distantShadow = GetDistantShadow();
            if (distantShadow != null)
            {
                distantShadow.CurrentMaskResolution = ResolutionState.Value;
                QualitySettings.shadowCascades = (int)ShadowCascades.Value;
            }
        }

        private void Awake()
        {
            new GameStartPatch().Enable();

            ResolutionState = Config.Bind("General", "Distant Shadow Resolution", DistantShadow.ResolutionState.FULL, new ConfigDescription(
                "Changes the resolution of distant shadows. Higher values reduce flickering but come with a small performance impact.",
                null,
                new ConfigurationManagerAttributes { Order = 100 }
            ));

            ShadowCascades = Config.Bind("General", "Shadow Cascades", EShadowCascades.LOW, new ConfigDescription(
                "Changes the amount of shadow cascades. Makes nearby shadows look nicer. I didn't notice a performance impact but if you do keep it at the default value.",
                null,
                new ConfigurationManagerAttributes { Order = 95 }
            ));

            ResolutionState.SettingChanged += OnUpdateSettings;
            ShadowCascades.SettingChanged += OnUpdateSettings;
        }
    }
}
