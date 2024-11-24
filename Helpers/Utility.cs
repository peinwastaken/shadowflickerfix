using static DistantShadow;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using EFT.EnvironmentEffect;

namespace shadowflickerfix.Helpers
{
    public static class Util
    {
        private static DistantShadow _distantShadow;
        private static PostProcessLayer _ppLayer;
        private static EnvironmentManager _envManager;
        private static Camera _camera;

        public static void ResetComponentCache()
        {
            _distantShadow = null;
            _ppLayer = null;
            _envManager = null;
            _camera = null;
        }

        public static Camera GetCamera()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            return _camera;
        }

        public static DistantShadow GetDistantShadow()
        {
            if (_distantShadow == null)
            {
                _distantShadow = GetCamera().GetComponentInChildren<DistantShadow>();
            }

            return _distantShadow;
        }
        public static PostProcessLayer GetPostProcessLayer()
        {
            if (_ppLayer == null)
            {
                _ppLayer = GetCamera().GetComponentInChildren<PostProcessLayer>();
            }

            return _ppLayer;
        }

        public static EnvironmentManager GetEnvironmentManager()
        {
            if (_envManager == null)
            {
                _envManager = UnityEngine.Component.FindObjectOfType<EnvironmentManager>();
            }

            return _envManager;
        }
    }
}