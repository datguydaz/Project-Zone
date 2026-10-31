#if UNITY_EDITOR
using ProjectZone;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectZone.Editor
{
    /// <summary>
    /// M7 atmosphere — auto-syncs lighting profiles and applies Zone Overcast on Outskirts.
    /// Manual menus remain as overrides; no dialogs on the auto path.
    /// </summary>
    [InitializeOnLoad]
    public static class PzAtmosphereMenus
    {
        private const string ResourcesFolder = "Assets/_ProjectZone/Resources/Atmosphere";
        private const string DayPath = ResourcesFolder + "/PZ_Lighting_OvercastDay.asset";
        private const string NightPath = ResourcesFolder + "/PZ_Lighting_Night.asset";
        private const string LogPrefix = "[ProjectZone][M7]";

        private static bool _bootstrapped;

        static PzAtmosphereMenus()
        {
            EditorApplication.delayCall += AutoBoot;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void AutoBoot()
        {
            if (_bootstrapped)
                return;
            _bootstrapped = true;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            PzSkyboxMenus.EnsureSkyboxMaterials();
            SyncProfilesFromCode();
            if (IsOutskirtsSceneOpen())
                ApplyPreset(GetActivePresetFromSettings(), silent: true);
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // Bake latest code values into assets before Play loads Resources.
                SyncProfilesFromCode();
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.delayCall += () =>
                {
                    if (!IsOutskirtsSceneOpen())
                        return;
                    ApplyPreset(GetActivePresetFromSettings(), silent: true);
                };
            }
        }

        public static PzLightingPresetId GetActivePresetFromSettings()
        {
            PzProjectSettings settings = PzProjectSettingsProvider.GetOrCreate();
            if (settings?.Atmosphere == null)
                return PzLightingPresetId.OvercastDay;
            return settings.Atmosphere.ActivePreset;
        }

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Ensure Lighting Profiles", priority = 70)]
        public static void EnsureProfiles()
        {
            SyncProfilesFromCode();
            Debug.Log($"{LogPrefix} Lighting profiles synced from code.");
        }

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Apply Zone Overcast (Stalker push)", priority = 71)]
        public static void ApplyOvercastDay()
        {
            SyncProfilesFromCode();
            ApplyPreset(PzLightingPresetId.OvercastDay, silent: false);
        }

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Apply Overcast Day (active scene)", priority = 71)]
        public static void ApplyOvercastDayAlias() => ApplyOvercastDay();

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Apply Night (active scene)", priority = 72)]
        public static void ApplyNight()
        {
            SyncProfilesFromCode();
            ApplyPreset(PzLightingPresetId.Night, silent: false);
        }

        /// <summary>
        /// Writes EditorConfigure* into Resources assets, then overlays Atmosphere Settings
        /// (fog/sun/post FX) onto the day profile — Atmosphere tab is source of truth for look.
        /// </summary>
        public static void SyncProfilesFromCode()
        {
            EnsureFolder("Assets/_ProjectZone", "Resources");
            EnsureFolder("Assets/_ProjectZone/Resources", "Atmosphere");

            PzLightingProfile day = LoadOrCreate(DayPath);
            day.EditorConfigureOvercastDay();

            PzLightingProfile night = LoadOrCreate(NightPath);
            night.EditorConfigureNight();

            ApplyAtmosphereSettingsToDayProfile(day);

            EditorUtility.SetDirty(day);
            EditorUtility.SetDirty(night);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Pushes Settings → Atmosphere (fog/sun/post FX) into Overcast day profile.</summary>
        public static void ApplyAtmosphereSettingsToDayProfile(PzLightingProfile day = null)
        {
            if (day == null)
                day = AssetDatabase.LoadAssetAtPath<PzLightingProfile>(DayPath);
            if (day == null)
                return;

            PzProjectSettings settings = PzProjectSettingsProvider.GetOrCreate();
            if (settings == null || settings.Atmosphere == null)
                return;

            // Day sliders only overlay the Overcast profile. Night is owned by EditorConfigureNight.
            if (settings.Atmosphere.ActivePreset != PzLightingPresetId.OvercastDay)
                return;

            Undo.RecordObject(day, "PZ Atmosphere Settings");
            day.ApplyAtmosphereCameraSettings(settings.Atmosphere);
            EditorUtility.SetDirty(day);
        }

        // Compat alias for older call sites.
        public static void ApplyPlayerCameraToDayProfile(PzLightingProfile day = null) =>
            ApplyAtmosphereSettingsToDayProfile(day);

        public static void ApplyPreset(PzLightingPresetId preset, bool silent)
        {
            OutskirtsAtmosphereBootstrap bootstrap = Object.FindFirstObjectByType<OutskirtsAtmosphereBootstrap>();
            if (bootstrap == null)
            {
                GameObject region = GameObject.Find("Outskirts Region")
                                    ?? GameObject.Find("OutskirtsRegion");
                if (region == null)
                {
                    // Only auto-create host in Outskirts scenes.
                    if (!IsOutskirtsSceneOpen())
                    {
                        if (!silent)
                            Debug.LogWarning($"{LogPrefix} No Outskirts scene / atmosphere host — skipped apply.");
                        return;
                    }

                    region = new GameObject("Outskirts_AtmosphereHost");
                    Undo.RegisterCreatedObjectUndo(region, "PZ Atmosphere Host");
                }

                bootstrap = region.GetComponent<OutskirtsAtmosphereBootstrap>();
                if (bootstrap == null)
                    bootstrap = Undo.AddComponent<OutskirtsAtmosphereBootstrap>(region);
            }

            SerializedObject so = new SerializedObject(bootstrap);
            so.FindProperty("_overcastDay").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PzLightingProfile>(DayPath);
            so.FindProperty("_night").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PzLightingProfile>(NightPath);
            so.FindProperty("_activePreset").enumValueIndex = (int)preset;
            so.FindProperty("_applyOnEnable").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            bootstrap.SetPreset(preset, apply: true);
            EditorUtility.SetDirty(bootstrap);

            if (!EditorApplication.isPlaying)
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            string name = preset == PzLightingPresetId.Night ? "Zone Night" : "Zone Overcast";
            Debug.Log($"{LogPrefix} Applied {name}{(silent ? " (auto)" : string.Empty)}.");

            if (!silent)
            {
                EditorUtility.DisplayDialog(
                    "PROJECT ZONE — M7",
                    $"Applied {name}.\n\n(This is optional now — Play Mode auto-applies Zone Overcast on Outskirts.)",
                    "OK");
            }
        }

        private static bool IsOutskirtsSceneOpen()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid())
                    continue;
                if (scene.name.IndexOf("Outskirts", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (scene.path.IndexOf("Outskirts", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static PzLightingProfile LoadOrCreate(string path)
        {
            PzLightingProfile existing = AssetDatabase.LoadAssetAtPath<PzLightingProfile>(path);
            if (existing != null)
                return existing;

            PzLightingProfile created = ScriptableObject.CreateInstance<PzLightingProfile>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
