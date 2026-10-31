using Haze.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectZone
{
    /// <summary>
    /// M7 #54 — applies exactly one fog backend. Never stacks HAZE + RenderSettings fog.
    /// </summary>
    public static class PzFogApplicator
    {
        public static void Apply(PzLightingProfile profile, Volume volume, PzProjectSettings.AtmosphereSettings atmosphere)
        {
            PzFogMode mode = atmosphere != null ? atmosphere.FogBackend : PzFogMode.SimpleLinear;
            float hazeDensity = atmosphere != null ? atmosphere.HazeDensity : 10f;

            switch (mode)
            {
                case PzFogMode.HazeVolumetric:
                    ApplyHaze(profile, volume, hazeDensity);
                    break;
                case PzFogMode.SimpleLinear:
                default:
                    ApplySimple(profile, volume);
                    break;
            }
        }

        private static void ApplySimple(PzLightingProfile profile, Volume volume)
        {
            SetHazeRendererActive(false);
            DisableHazeOnVolume(volume);

            if (profile == null)
                return;

            RenderSettings.fog = profile.FogEnabled;
            RenderSettings.fogMode = profile.FogMode;
            RenderSettings.fogColor = profile.FogColor;
            RenderSettings.fogStartDistance = profile.FogStart;
            RenderSettings.fogEndDistance = profile.FogEnd;
        }

        private static void ApplyHaze(PzLightingProfile profile, Volume volume, float density)
        {
            // Hard rule: no RenderSettings fog while HAZE is on.
            RenderSettings.fog = false;
            SetHazeRendererActive(true);

            if (volume == null)
            {
                Debug.LogWarning("[ProjectZone][M7][#54] HAZE mode needs a URP Volume — none found.");
                return;
            }

            // Never use volume.profile (clones sharedProfile; breaks Polymind pause Instantiate).
            VolumeProfile vp = volume.sharedProfile;
            if (vp == null)
                return;

            if (!vp.TryGet(out HazeGlobalFogVolumeComponent haze))
                haze = vp.Add<HazeGlobalFogVolumeComponent>(true);

            haze.active = true;
            haze.GlobalDensityMultiplier.overrideState = true;
            haze.GlobalDensityMultiplier.value = Mathf.Max(0f, density);
            haze.GlobalDensityThreshold.overrideState = true;
            haze.GlobalDensityThreshold.value = 0.35f;

            Color fogColor = profile != null ? profile.FogColor : new Color(0.62f, 0.63f, 0.60f);
            haze.AmbientColor.overrideState = true;
            haze.AmbientColor.value = fogColor;
            haze.MainLightContribution.overrideState = true;
            haze.MainLightContribution.value = new Color(1.2f, 1.15f, 1.05f, 1f);
            haze.MainLightScattering.overrideState = true;
            haze.MainLightScattering.value = 0.2f;
            haze.HeightFogFactor.overrideState = true;
            haze.HeightFogFactor.value = 0.15f;
            haze.MaxFogHeight.overrideState = true;
            haze.MaxFogHeight.value = 45f;
            haze.HeightFogSmoothness.overrideState = true;
            haze.HeightFogSmoothness.value = 12f;
            haze.AdditionalLightContribution.overrideState = true;
            haze.AdditionalLightContribution.value = 0f;
            haze.ProbeVolumeContribution.overrideState = true;
            haze.ProbeVolumeContribution.value = 0f;
            haze.GlobalMainLightDensityBoost.overrideState = true;
            haze.GlobalMainLightDensityBoost.value = 0f;
            haze.GlobalGodRaysContribution.overrideState = true;
            haze.GlobalGodRaysContribution.value = Color.black;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(vp);
#endif
        }

        private static void DisableHazeOnVolume(Volume volume)
        {
            if (volume == null)
                return;

            VolumeProfile vp = volume.sharedProfile;
            if (vp == null)
                return;

            if (vp.TryGet(out HazeGlobalFogVolumeComponent haze))
            {
                haze.active = false;
                haze.GlobalDensityMultiplier.overrideState = true;
                haze.GlobalDensityMultiplier.value = 0f;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(vp);
#endif
            }
        }

        private static void SetHazeRendererActive(bool active)
        {
            HazeRendererFeature[] features = Resources.FindObjectsOfTypeAll<HazeRendererFeature>();
            for (int i = 0; i < features.Length; i++)
            {
                if (features[i] != null)
                    features[i].SetActive(active);
            }
        }
    }
}
