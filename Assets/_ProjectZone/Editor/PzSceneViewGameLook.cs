#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Makes Scene View approximate Game/Play look: fog, Zone skybox, URP post FX,
    /// and PostProcessing volume layer (same mask as HQFPS camera).
    /// </summary>
    [InitializeOnLoad]
    public static class PzSceneViewGameLook
    {
        private const string PrefKey = "ProjectZone.SceneViewGameLook";
        private const int PostProcessingLayer = 11;

        private static bool _bootstrapped;
        private static double _nextRefresh;

        static PzSceneViewGameLook()
        {
            EditorApplication.delayCall += Boot;
            EditorSceneManager.sceneOpened += (_, __) => ScheduleRefresh();
            EditorApplication.playModeStateChanged += _ => ScheduleRefresh();
            SceneView.duringSceneGui += OnDuringSceneGui;
        }

        private static void Boot()
        {
            if (_bootstrapped)
                return;
            _bootstrapped = true;

            // Default ON — Scene View should track atmosphere unless user disables.
            if (!EditorPrefs.HasKey(PrefKey))
                EditorPrefs.SetBool(PrefKey, true);

            ScheduleRefresh();
        }

        private static void ScheduleRefresh()
        {
            _nextRefresh = EditorApplication.timeSinceStartup + 0.15;
            EditorApplication.update -= TickRefresh;
            EditorApplication.update += TickRefresh;
        }

        private static void TickRefresh()
        {
            if (EditorApplication.timeSinceStartup < _nextRefresh)
                return;

            EditorApplication.update -= TickRefresh;
            if (Enabled)
                ApplyToAllSceneViews();
        }

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(PrefKey, true);
            set
            {
                EditorPrefs.SetBool(PrefKey, value);
                if (value)
                    ApplyToAllSceneViews();
                SceneView.RepaintAll();
            }
        }

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Scene View Matches Game Look", priority = 80)]
        public static void ToggleMenu()
        {
            Enabled = !Enabled;
            Debug.Log($"[ProjectZone] Scene View game-look {(Enabled ? "ON" : "OFF")} " +
                      "(fog + Zone skybox + URP post + PostProcessing volume layer).");
        }

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Scene View Matches Game Look", true)]
        public static bool ToggleMenuValidate()
        {
            Menu.SetChecked("Tools/PROJECT ZONE/Atmosphere/Scene View Matches Game Look", Enabled);
            return true;
        }

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Refresh Scene View Look Now", priority = 81)]
        public static void RefreshNow()
        {
            Enabled = true;
            PzSkyboxMenus.EnsureSkyboxMaterials();
            if (IsOutskirtsOpen())
                PzAtmosphereMenus.ApplyPreset(PzAtmosphereMenus.GetActivePresetFromSettings(), silent: true);
            ApplyToAllSceneViews();
            SceneView.RepaintAll();
        }

        private static void OnDuringSceneGui(SceneView view)
        {
            if (!Enabled || view == null)
                return;

            // Keep toggles sticky while editing Outskirts (Scene View resets some flags).
            if (!IsOutskirtsOpen())
                return;

            ConfigureSceneView(view);
        }

        public static void ApplyToAllSceneViews()
        {
            foreach (SceneView view in SceneView.sceneViews)
            {
                if (view != null)
                    ConfigureSceneView(view);
            }
        }

        private static void ConfigureSceneView(SceneView view)
        {
            view.sceneLighting = true;

            SceneView.SceneViewState state = view.sceneViewState;
            state.showFog = true;
            state.showSkybox = true;
            state.showImageEffects = true;

            Camera cam = view.camera;
            if (cam == null)
                return;

            UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
            if (data == null)
                return;

            data.renderPostProcessing = true;
            // HQFPS Game camera uses PostProcessing only; include Default too for edit volumes.
            data.volumeLayerMask = (1 << PostProcessingLayer) | (1 << 0);
            data.requiresDepthOption = CameraOverrideOption.UsePipelineSettings;
            data.requiresColorOption = CameraOverrideOption.UsePipelineSettings;

            // Match CAS sharpness from active profile when possible.
            PzLightingProfile profile = LoadActiveProfile();
            if (profile != null)
                data.taaSettings.contrastAdaptiveSharpening = Mathf.Clamp01(profile.Sharpness);
        }

        private static PzLightingProfile LoadActiveProfile()
        {
            PzLightingPresetId preset = PzAtmosphereMenus.GetActivePresetFromSettings();
            string path = preset == PzLightingPresetId.Night
                ? "Assets/_ProjectZone/Resources/Atmosphere/PZ_Lighting_Night.asset"
                : "Assets/_ProjectZone/Resources/Atmosphere/PZ_Lighting_OvercastDay.asset";
            return AssetDatabase.LoadAssetAtPath<PzLightingProfile>(path);
        }

        private static bool IsOutskirtsOpen()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                string name = scene.name ?? string.Empty;
                string path = scene.path ?? string.Empty;
                if (name.IndexOf("Outskirts", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || path.IndexOf("PZ_Outskirts", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
#endif
