using PolymindGames.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectZone
{
    /// <summary>
    /// Applies lighting to RenderSettings / sun, and drives the HQFPS PostProcessing-layer volume
    /// (Polymind ActiveVolume) with our shared profile — no mute, no second competing grade.
    /// </summary>
    public static class PzLightingApplicator
    {
        public const string VolumeChildName = "Outskirts_Atmosphere";
        public const string VolumeProfileResource = "Atmosphere/PZ_Outskirts_VolumeProfile";
        public const string VolumeProfileAssetPath =
            "Assets/_ProjectZone/Resources/Atmosphere/PZ_Outskirts_VolumeProfile.asset";
        public const string PostProcessingLayerName = "PostProcessing";
        public const float VolumePriority = 50f;
        public const string HqfpsPostRootName = "HQFPS_PostProcessing";
        public const string HqfpsPostPrefabPath =
            "Assets/01_System/PolymindGames/HQFPS/Prefabs/Core/HQFPS_PostProcessing.prefab";

        private static VolumeProfile _playModeProfile;

        public static void Apply(PzLightingProfile profile, Transform volumeParent = null)
        {
            if (profile == null)
            {
                Debug.LogWarning("[ProjectZone][M7] Lighting profile missing — cannot apply.");
                return;
            }

            ApplyRenderSettings(profile);
            ApplySkybox(profile);
            Light sun = EnsureSun(volumeParent);
            ApplySun(sun, profile);

            VolumeProfile grade = GetWorkingProfile();
            RepairVolumeProfile(grade);
            ApplyToProfile(grade, profile);

            // Outskirts scene never contained HQFPS_PostProcessing — spawn/ensure it so Polymind
            // pause FX and our grade share one ActiveVolume.
            Volume hqfps = EnsureHqfpsPostVolume();
            Volume ours = EnsurePzVolume(volumeParent, grade);
            Volume fogVolume;

            if (hqfps != null)
            {
                hqfps.isGlobal = true;
                hqfps.weight = 1f;
                // Assign once when different — reassigning every Apply destroys Polymind's
                // runtime .profile clone and can leave destroyed ColorAdjustments refs.
                if (hqfps.sharedProfile != grade)
                    hqfps.sharedProfile = grade;
                ours.weight = 0f;
                fogVolume = hqfps;
            }
            else
            {
                ours.weight = 1f;
                ours.priority = VolumePriority;
                if (ours.sharedProfile != grade)
                    ours.sharedProfile = grade;
                fogVolume = ours;
            }

            BindPolymindActiveVolume(fogVolume);

            PzProjectSettings.AtmosphereSettings atmosphere = PzProjectSettings.Get()?.Atmosphere;
            PzFogApplicator.Apply(profile, fogVolume, atmosphere);

            ApplyCameraSharpness(profile);

            PzFogMode fogMode = atmosphere != null ? atmosphere.FogBackend : PzFogMode.SimpleLinear;
            Debug.Log($"[ProjectZone][M7] Applied lighting preset: {profile.DisplayName} ({profile.PresetId}), fog={fogMode}" +
                      (hqfps != null ? " via HQFPS volume." : " via Outskirts volume (HQFPS prefab missing)."));
        }

        /// <summary>
        /// Ensures Polymind pause/damage post FX have an ActiveVolume (fixes "No active volume active").
        /// </summary>
        public static void BindPolymindActiveVolume(Volume volume)
        {
            if (volume == null)
                return;

            if (volume.sharedProfile != null)
                RepairVolumeProfile(volume.sharedProfile);

            GlobalVolume bridge = volume.GetComponent<GlobalVolume>();
            if (bridge == null)
                bridge = volume.gameObject.AddComponent<GlobalVolume>();
            if (!bridge.enabled)
                bridge.enabled = true;

            if (PostProcessingManager.Instance != null)
                PostProcessingManager.Instance.ActiveVolume = volume;
        }

        /// <summary>
        /// Play Mode uses a runtime clone so we never corrupt the Resources asset, and so
        /// Polymind's Volume.profile Instantiate always clones healthy components.
        /// </summary>
        public static VolumeProfile GetWorkingProfile()
        {
            VolumeProfile source = EnsureSharedProfile();
            if (!Application.isPlaying)
                return source;

            if (_playModeProfile == null)
            {
                _playModeProfile = ScriptableObject.Instantiate(source);
                _playModeProfile.name = source.name + " (Runtime)";
            }

            return _playModeProfile;
        }

        public static void RepairVolumeProfile(VolumeProfile vp)
        {
            if (vp == null)
                return;

            for (int i = vp.components.Count - 1; i >= 0; i--)
            {
                if (vp.components[i] == null)
                    vp.components.RemoveAt(i);
            }
        }

        public static void ApplyRenderSettings(PzLightingProfile profile)
        {
            // Fog is owned by PzFogApplicator (#54) — do not set RenderSettings.fog here.
            RenderSettings.ambientMode = profile.AmbientMode;
            RenderSettings.ambientSkyColor = profile.AmbientSky;
            RenderSettings.ambientEquatorColor = profile.AmbientEquator;
            RenderSettings.ambientGroundColor = profile.AmbientGround;
            RenderSettings.ambientIntensity = profile.AmbientIntensity;
            RenderSettings.subtractiveShadowColor = new Color(0.22f, 0.2f, 0.24f);
            // Kill shiny “happy” environment reflections from a bright default sky.
            RenderSettings.reflectionIntensity = profile.PresetId == PzLightingPresetId.Night ? 0.25f : 0.35f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }

        /// <summary>
        /// Replaces Unity’s default blue procedural sky with Zone overcast / night greys.
        /// </summary>
        public static void ApplySkybox(PzLightingProfile profile)
        {
            string resource = profile != null && profile.PresetId == PzLightingPresetId.Night
                ? "Atmosphere/PZ_Sky_Night"
                : "Atmosphere/PZ_Sky_Overcast";

            Material sky = Resources.Load<Material>(resource);
#if UNITY_EDITOR
            if (sky == null)
            {
                // Editor first-load: create mats then retry.
                System.Type skyMenus = System.Type.GetType("ProjectZone.Editor.PzSkyboxMenus, Assembly-CSharp-Editor");
                skyMenus?.GetMethod("EnsureSkyboxMaterials", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    ?.Invoke(null, null);
                sky = Resources.Load<Material>(resource);
            }
#endif
            if (sky == null)
            {
                Debug.LogWarning($"[ProjectZone][M7] Skybox material missing: Resources/{resource}");
                return;
            }

            RenderSettings.skybox = sky;
            DynamicGI.UpdateEnvironment();
        }

        public static Light EnsureSun(Transform preferredParent)
        {
            Light sun = RenderSettings.sun;
            if (sun != null && sun.type == LightType.Directional)
                return sun;

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                    return lights[i];
            }

            var go = new GameObject("Directional Light");
            if (preferredParent != null)
                go.transform.SetParent(preferredParent, false);
            sun = go.AddComponent<Light>();
            sun.type = LightType.Directional;
            return sun;
        }

        public static void ApplySun(Light sun, PzLightingProfile profile)
        {
            if (sun == null)
                return;

            sun.color = profile.SunColor;
            sun.intensity = profile.SunIntensity;
            sun.shadows = profile.SunShadows;
            sun.shadowStrength = profile.ShadowStrength;
            sun.transform.rotation = Quaternion.Euler(profile.SunEuler);
            RenderSettings.sun = sun;
        }

        /// <summary>
        /// Finds or instantiates Polymind's HQFPS_PostProcessing root (not part of the player prefab).
        /// </summary>
        public static Volume EnsureHqfpsPostVolume()
        {
            Volume existing = FindHqfpsPostVolume();
            if (existing != null)
                return existing;

            GameObject prefab = LoadHqfpsPostPrefab();
            if (prefab == null)
            {
                Debug.LogWarning(
                    $"[ProjectZone][M7] Missing HQFPS post prefab at {HqfpsPostPrefabPath} — using Outskirts volume.");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = HqfpsPostRootName;
            Volume volume = instance.GetComponentInChildren<Volume>(true);
            if (volume == null)
            {
                Debug.LogWarning("[ProjectZone][M7] HQFPS_PostProcessing prefab has no Volume child.");
                return null;
            }

            volume.isGlobal = true;
            volume.weight = 1f;
            int ppLayer = LayerMask.NameToLayer(PostProcessingLayerName);
            if (ppLayer >= 0)
                volume.gameObject.layer = ppLayer;

            Debug.Log("[ProjectZone][M7] Spawned HQFPS_PostProcessing for ActiveVolume / grade.");
            return volume;
        }

        public static Volume FindHqfpsPostVolume()
        {
            GameObject root = GameObject.Find(HqfpsPostRootName);
            if (root != null)
            {
                Volume child = root.GetComponentInChildren<Volume>(true);
                if (child != null)
                    return child;
            }

            // Any GlobalVolume that is not our Outskirts fallback.
            GlobalVolume[] bridges = Object.FindObjectsByType<GlobalVolume>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < bridges.Length; i++)
            {
                GlobalVolume bridge = bridges[i];
                if (bridge == null || bridge.gameObject.name == VolumeChildName)
                    continue;
                Volume vol = bridge.GetComponent<Volume>();
                if (vol != null)
                    return vol;
            }

            int ppLayer = LayerMask.NameToLayer(PostProcessingLayerName);
            Volume[] volumes = Object.FindObjectsByType<Volume>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
            {
                Volume v = volumes[i];
                if (v == null || v.gameObject.name == VolumeChildName)
                    continue;

                string path = GetHierarchyPath(v.transform);
                if (path.IndexOf(HqfpsPostRootName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return v;

                if (v.isGlobal && ppLayer >= 0 && v.gameObject.layer == ppLayer)
                    return v;
            }

            return null;
        }

        private static GameObject LoadHqfpsPostPrefab()
        {
#if UNITY_EDITOR
            GameObject editorPrefab =
                UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(HqfpsPostPrefabPath);
            if (editorPrefab != null)
                return editorPrefab;
#endif
            return Resources.Load<GameObject>("Atmosphere/HQFPS_PostProcessing");
        }

        public static Volume EnsureVolume(Transform preferredParent)
        {
            VolumeProfile shared = EnsureSharedProfile();
            return EnsurePzVolume(preferredParent, shared);
        }

        private static Volume EnsurePzVolume(Transform preferredParent, VolumeProfile shared)
        {
            Transform searchRoot = preferredParent != null ? preferredParent : null;
            Transform existing = searchRoot != null ? searchRoot.Find(VolumeChildName) : null;
            if (existing == null)
            {
                GameObject found = GameObject.Find(VolumeChildName);
                if (found != null)
                    existing = found.transform;
            }

            GameObject root;
            if (existing != null)
            {
                root = existing.gameObject;
            }
            else
            {
                root = new GameObject(VolumeChildName);
                if (preferredParent != null)
                    root.transform.SetParent(preferredParent, false);
            }

            int ppLayer = LayerMask.NameToLayer(PostProcessingLayerName);
            if (ppLayer >= 0)
                root.layer = ppLayer;

            Volume volume = root.GetComponent<Volume>();
            if (volume == null)
                volume = root.AddComponent<Volume>();

            volume.isGlobal = true;
            volume.priority = VolumePriority;
            if (volume.sharedProfile != shared)
                volume.sharedProfile = shared;
            return volume;
        }

        public static VolumeProfile EnsureSharedProfile()
        {
            VolumeProfile profile = Resources.Load<VolumeProfile>(VolumeProfileResource);
            if (profile != null)
                return profile;

#if UNITY_EDITOR
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/_ProjectZone/Resources"))
            {
                if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/_ProjectZone"))
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "_ProjectZone");
                UnityEditor.AssetDatabase.CreateFolder("Assets/_ProjectZone", "Resources");
            }

            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/_ProjectZone/Resources/Atmosphere"))
                UnityEditor.AssetDatabase.CreateFolder("Assets/_ProjectZone/Resources", "Atmosphere");

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            UnityEditor.AssetDatabase.CreateAsset(profile, VolumeProfileAssetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            return profile;
#else
            return ScriptableObject.CreateInstance<VolumeProfile>();
#endif
        }

        public static void ApplyVolume(Volume volume, PzLightingProfile profile)
        {
            if (volume == null || profile == null)
                return;

            VolumeProfile vp = volume.sharedProfile != null ? volume.sharedProfile : EnsureSharedProfile();
            volume.sharedProfile = vp;
            ApplyToProfile(vp, profile);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(vp);
#endif
        }

        public static void ApplyToProfile(VolumeProfile vp, PzLightingProfile profile)
        {
            if (vp == null || profile == null)
                return;

            if (!vp.TryGet(out ColorAdjustments color))
                color = vp.Add<ColorAdjustments>(true);

            // Sharpness mainly via TAA CAS; keep contrast boost mild so midtones stay open.
            float contrast = profile.Contrast + profile.Sharpness * 10f;

            color.active = true;
            color.postExposure.overrideState = true;
            color.postExposure.value = profile.PostExposure;
            color.contrast.overrideState = true;
            color.contrast.value = contrast;
            color.saturation.overrideState = true;
            color.saturation.value = profile.Saturation;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = profile.ColorFilter;

            if (!vp.TryGet(out Vignette vignette))
                vignette = vp.Add<Vignette>(true);

            vignette.active = true;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = profile.Vignette;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.45f;
            vignette.rounded.overrideState = true;
            vignette.rounded.value = false;

            if (!vp.TryGet(out WhiteBalance whiteBalance))
                whiteBalance = vp.Add<WhiteBalance>(true);

            whiteBalance.active = true;
            whiteBalance.temperature.overrideState = true;
            whiteBalance.temperature.value = profile.WhiteBalanceTemp;

            if (!vp.TryGet(out FilmGrain grain))
                grain = vp.Add<FilmGrain>(true);

            grain.active = profile.FilmGrain > 0.01f;
            grain.type.overrideState = true;
            grain.type.value = FilmGrainLookup.Thin1;
            grain.intensity.overrideState = true;
            grain.intensity.value = profile.FilmGrain;
            grain.response.overrideState = true;
            grain.response.value = 0.6f;

            if (!vp.TryGet(out Bloom bloom))
                bloom = vp.Add<Bloom>(true);

            bloom.active = profile.BloomIntensity > 0.01f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = profile.BloomIntensity;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.9f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.55f;

            if (!vp.TryGet(out ChromaticAberration chromatic))
                chromatic = vp.Add<ChromaticAberration>(true);

            chromatic.active = profile.ChromaticAberration > 0.005f;
            chromatic.intensity.overrideState = true;
            chromatic.intensity.value = profile.ChromaticAberration;

            if (!vp.TryGet(out ShadowsMidtonesHighlights smh))
                smh = vp.Add<ShadowsMidtonesHighlights>(true);

            smh.active = true;
            smh.shadows.overrideState = true;
            smh.shadows.value = new Vector4(1f, 1f, 1f, profile.ShadowsStart);
            smh.midtones.overrideState = true;
            smh.midtones.value = new Vector4(1f, 1f, 1f, 0.05f);
            smh.highlights.overrideState = true;
            smh.highlights.value = new Vector4(1f, 1f, 1f, profile.Highlights);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(vp);
#endif
        }

        public static void ApplyCameraSharpness(PzLightingProfile profile)
        {
            if (profile == null)
                return;

            float cas = Mathf.Clamp01(profile.Sharpness);
            UniversalAdditionalCameraData[] cams =
                Object.FindObjectsByType<UniversalAdditionalCameraData>(FindObjectsInactive.Exclude);

            for (int i = 0; i < cams.Length; i++)
            {
                UniversalAdditionalCameraData data = cams[i];
                if (data == null)
                    continue;

                if ((data.volumeLayerMask.value & (1 << 11)) == 0)
                    continue;

                data.taaSettings.contrastAdaptiveSharpening = cas;
            }
        }

        private static string GetHierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }

            return path;
        }
    }
}
