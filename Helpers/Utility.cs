using static DistantShadow;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace shadowflickerfix.Helpers
{
    public static class Util
    {
        public static Camera GetCamera() => Camera.main;
        public static DistantShadow GetDistantShadow() => GetCamera().GetComponentInChildren<DistantShadow>();
        public static PostProcessLayer GetPostProcessLayer() => GetCamera().GetComponentInChildren<PostProcessLayer>();
    }
}