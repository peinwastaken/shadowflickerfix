using static DistantShadow;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using EFT.EnvironmentEffect;
using UnityEngine.Scripting;

namespace shadowflickerfix.Helpers
{
    public static class Util
    {
        public static Camera GetCamera() => Camera.main;
        public static DistantShadow GetDistantShadow() => GetCamera().GetComponentInChildren<DistantShadow>();
        public static PostProcessLayer GetPostProcessLayer() => GetCamera().GetComponentInChildren<PostProcessLayer>();
        public static EnvironmentManager GetEnvironmentManager() => UnityEngine.Component.FindObjectOfType<EnvironmentManager>();
    }
}