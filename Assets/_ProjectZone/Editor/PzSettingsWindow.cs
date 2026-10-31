#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Tabbed PROJECT ZONE settings hub (Hierarchy Designer–inspired navigation).
    /// Menu: Tools / PROJECT ZONE / Settings
    /// </summary>
    public sealed class PzSettingsWindow : EditorWindow
    {
        private enum Tab
        {
            Home,
            Zombies,
            Player,
            Atmosphere,
            Audio,
            World,
            Expedition
        }

        private const string WindowTitle = "PROJECT ZONE";
        private const string VersionLabel = "SETTINGS 1.0";

        private Tab _tab = Tab.Home;
        private Vector2 _scroll;
        private PzProjectSettings _settings;
        private SerializedObject _serialized;
        private Vector2 _headerScroll;
        private bool _atmosphereDirty;

        [MenuItem("Tools/PROJECT ZONE/Settings", priority = 0)]
        public static void Open()
        {
            PzSettingsWindow window = GetWindow<PzSettingsWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(560f, 420f);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            EnsureSettings();
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnFocus()
        {
            EnsureSettings();
        }

        private void OnUndoRedo()
        {
            if (_settings == null)
                return;

            // Ctrl+Z restored the settings asset — refresh UI + push atmosphere again.
            _serialized = new SerializedObject(_settings);
            PzProjectSettings.SetEditorCache(_settings);
            if (_tab == Tab.Atmosphere || _atmosphereDirty)
                PushAtmosphereLive(saveAssets: false);
            Repaint();
        }

        private void EnsureSettings()
        {
            if (_settings == null)
                _settings = PzProjectSettingsProvider.GetOrCreate();

            if (_settings != null)
            {
                PzProjectSettings.SetEditorCache(_settings);
                if (_serialized == null || _serialized.targetObject != _settings)
                    _serialized = new SerializedObject(_settings);
            }
        }

        private void OnGUI()
        {
            PzSettingsGui.Ensure();
            EnsureSettings();

            if (_settings == null || _serialized == null)
            {
                EditorGUILayout.HelpBox(
                    "Could not create or load PZ_ProjectSettings. Check Assets/_ProjectZone/Resources/.",
                    MessageType.Error);
                return;
            }

            _serialized.Update();

            EditorGUILayout.BeginVertical(PzSettingsGui.PrimaryPanelStyle);
            DrawHeader();
            DrawBody();
            EditorGUILayout.EndVertical();

            if (_serialized.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_settings);
                if (_tab == Tab.Atmosphere)
                {
                    _atmosphereDirty = true;
                    // Don't SaveAssets here — that breaks Ctrl+Z. Save button / domain reload persists.
                    PushAtmosphereLive(saveAssets: false);
                }
            }
        }

        private void PushAtmosphereLive(bool saveAssets)
        {
            // Day slider overlay only (skip when Night is active). No SaveAssets on every tweak — keeps Ctrl+Z.
            PzAtmosphereMenus.ApplyAtmosphereSettingsToDayProfile();

            PzProjectSettings.AtmosphereSettings atmo = _settings.Atmosphere;
            if (atmo != null && atmo.FogBackend == PzFogMode.HazeVolumetric)
                PzHazeRendererEnsure.EnsureOnPcRenderer(activate: true);
            else
                PzHazeRendererEnsure.EnsureOnPcRenderer(activate: false);

            if (saveAssets)
                AssetDatabase.SaveAssets();

            if (Object.FindAnyObjectByType<OutskirtsAtmosphereBootstrap>() != null
                || EditorSceneManagerHasOutskirts())
            {
                PzAtmosphereMenus.ApplyPreset(PzAtmosphereMenus.GetActivePresetFromSettings(), silent: true);
                PzSceneViewGameLook.ApplyToAllSceneViews();
                SceneView.RepaintAll();
            }
        }

        private static bool EditorSceneManagerHasOutskirts()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                string name = scene.name ?? string.Empty;
                string path = scene.path ?? string.Empty;
                if (name.IndexOf("Outskirts", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || path.IndexOf("PZ_Outskirts", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(PzSettingsGui.HeaderPanelStyle);

            _headerScroll = EditorGUILayout.BeginScrollView(
                _headerScroll,
                GUI.skin.horizontalScrollbar,
                GUIStyle.none,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(false));

            EditorGUILayout.BeginHorizontal();
            string accentHex = ColorUtility.ToHtmlStringRGB(PzSettingsGui.Accent);
            EditorGUILayout.LabelField(
                $"PROJECT <color=#{accentHex}>ZONE</color>",
                PzSettingsGui.HeaderLabelStyle,
                GUILayout.Width(200f));
            GUILayout.FlexibleSpace();
            DrawTabButton("HOME", Tab.Home);
            DrawTabButton("ZOMBIES", Tab.Zombies);
            DrawTabButton("PLAYER", Tab.Player);
            DrawTabButton("ATMOSPHERE", Tab.Atmosphere);
            DrawTabButton("AUDIO", Tab.Audio);
            DrawTabButton("WORLD", Tab.World);
            DrawTabButton("EXPEDITION", Tab.Expedition);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"▷ {TabTitle(_tab)}", PzSettingsGui.TabLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(VersionLabel, PzSettingsGui.VersionLabelStyle, GUILayout.Height(20f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawTabButton(string label, Tab tab)
        {
            Color prev = GUI.backgroundColor;
            if (_tab == tab)
                GUI.backgroundColor = PzSettingsGui.Accent * new Color(1f, 1f, 1f, 0.85f);

            if (GUILayout.Button(label, PzSettingsGui.HeaderButtonStyle, GUILayout.Height(28f)))
                _tab = tab;

            GUI.backgroundColor = prev;
            GUILayout.Space(4f);
        }

        private void DrawBody()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            switch (_tab)
            {
                case Tab.Home:
                    DrawHome();
                    break;
                case Tab.Zombies:
                    DrawPropertyPanel("_zombies", "Zombies / Hostiles",
                        "Female zombie prefab, clips, and melee AI defaults used by Outskirts NPC bootstrap.");
                    break;
                case Tab.Player:
                    DrawPropertyPanel("_player", "Player",
                        "Starting loadout only. Camera / post FX are under Atmosphere (FPS camera volume).");
                    break;
                case Tab.Atmosphere:
                    DrawAtmosphereTab();
                    break;
                case Tab.Audio:
                    DrawAudioTab();
                    break;
                case Tab.World:
                    DrawPropertyPanel("_world", "World / Outskirts",
                        "Spawn / NavMesh / adapter toggles. Foliage (#55): Toby primary + Prefab Paint spacing caps — " +
                        "no Terrain-detail dual stack. See docs/environment/FOLIAGE_WORKFLOW.md");
                    break;
                case Tab.Expedition:
                    DrawPropertyPanel("_expedition", "Expedition",
                        "Placeholder for contracts, wallet, and death-cache tuning.");
                    break;
                default:
                    break;
            }

            GUILayout.Space(12f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping Settings Asset", GUILayout.Height(26f)))
            {
                EditorGUIUtility.PingObject(_settings);
                Selection.activeObject = _settings;
            }

            if (GUILayout.Button("Save", GUILayout.Height(26f), GUILayout.Width(80f)))
            {
                EditorUtility.SetDirty(_settings);
                if (_tab == Tab.Atmosphere || _atmosphereDirty)
                    PushAtmosphereLive(saveAssets: true);
                else
                    AssetDatabase.SaveAssets();
                _atmosphereDirty = false;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        private void DrawHome()
        {
            PzSettingsGui.DrawSection("Overview");
            EditorGUILayout.LabelField(
                "Central place to park PROJECT ZONE prefabs, clips, and greybox tuning — " +
                "so you do not hunt through scene components later.",
                PzSettingsGui.HelpBoxStyle);

            GUILayout.Space(8f);
            PzSettingsGui.DrawSection("Tabs");
            EditorGUILayout.LabelField("• <b>Zombies</b> — female zombie prefab, anim clips, melee ranges/speeds", PzSettingsGui.HelpBoxStyle);
            EditorGUILayout.LabelField("• <b>Player</b> — starting weapon / loadout", PzSettingsGui.HelpBoxStyle);
            EditorGUILayout.LabelField("• <b>Atmosphere</b> — fog / sun + camera post FX (grain, chromatic, sharpness)", PzSettingsGui.HelpBoxStyle);
            EditorGUILayout.LabelField("• <b>Audio</b> — ambient zones (#56), master volume, seed Hab/warehouse/tunnel", PzSettingsGui.HelpBoxStyle);
            EditorGUILayout.LabelField("• <b>World</b> — NPC spawn, NavMesh, foliage density caps (#55)", PzSettingsGui.HelpBoxStyle);
            EditorGUILayout.LabelField("• <b>Expedition</b> — reserved for M5 loop tuning", PzSettingsGui.HelpBoxStyle);

            GUILayout.Space(8f);
            PzSettingsGui.DrawSection("Asset");
            EditorGUILayout.ObjectField("Settings", _settings, typeof(PzProjectSettings), false);
            EditorGUILayout.LabelField(
                $"Runtime load: <i>Resources/{PzProjectSettings.ResourcePath}</i>",
                PzSettingsGui.HelpBoxStyle);
        }

        private void DrawAtmosphereTab()
        {
            DrawPropertyPanel("_atmosphere", "Atmosphere",
                "Day/Night + HAZE (#54) + post FX. Space transitions (#57): Outdoor / Sheltered / Indoor " +
                "lerp haze, ambient, exposure, sun — tune under Space transitions.");

            GUILayout.Space(10f);
            PzSettingsGui.DrawSection("Space transitions (#57)");
            EditorGUILayout.LabelField(
                "Settlement courtyard = Sheltered. Hab / warehouse / tunnel = Indoor (clearer air).",
                PzSettingsGui.HelpBoxStyle);
            GUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ensure HQFPS Post Volume", GUILayout.Height(28f)))
                PzAtmosphereTransitionMenus.EnsureHqfpsPostVolume();
            if (GUILayout.Button("Ensure Transition Director", GUILayout.Height(28f)))
                PzAtmosphereTransitionMenus.EnsureDirector();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Seed Space Zones", GUILayout.Height(28f)))
                PzAtmosphereTransitionMenus.SeedSpaceZones();
        }

        private void DrawAudioTab()
        {
            DrawPropertyPanel("_audio", "Ambient audio (#56)",
                "Simple split: Outdoor (outside Settlement) vs Settlement (inside the compound). " +
                "Assign Clip / Volume / Muffle per bed. Seed zones after code changes.");

            GUILayout.Space(10f);
            PzSettingsGui.DrawSection("Scene setup");
            EditorGUILayout.LabelField(
                "Creates AmbientAudioDirector + greybox zone volumes under Settlement / Industrial / Underground.",
                PzSettingsGui.HelpBoxStyle);
            GUILayout.Space(4f);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ensure Ambient Director", GUILayout.Height(28f)))
                PzAudioMenus.EnsureDirector();
            if (GUILayout.Button("Seed Greybox Audio Zones", GUILayout.Height(28f)))
                PzAudioMenus.SeedGreyboxZones();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Open AUDIO_ZONES.md", GUILayout.Height(24f)))
                PzAudioMenus.OpenDoc();
        }

        private void DrawPropertyPanel(string propertyName, string title, string help)
        {
            PzSettingsGui.DrawSection(title);
            EditorGUILayout.LabelField(help, PzSettingsGui.HelpBoxStyle);
            GUILayout.Space(4f);

            SerializedProperty prop = _serialized.FindProperty(propertyName);
            if (prop == null)
            {
                EditorGUILayout.HelpBox($"Missing property '{propertyName}'.", MessageType.Warning);
                return;
            }

            EditorGUI.indentLevel++;
            DrawChildren(prop);
            EditorGUI.indentLevel--;
        }

        private static void DrawChildren(SerializedProperty root)
        {
            SerializedProperty iterator = root.Copy();
            SerializedProperty end = root.GetEndProperty();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                EditorGUILayout.PropertyField(iterator, true);
                enterChildren = false;
            }
        }

        private static string TabTitle(Tab tab)
        {
            switch (tab)
            {
                case Tab.Home: return "Home";
                case Tab.Zombies: return "Zombies";
                case Tab.Player: return "Player";
                case Tab.Atmosphere: return "Atmosphere";
                case Tab.Audio: return "Audio";
                case Tab.World: return "World";
                case Tab.Expedition: return "Expedition";
                default: return tab.ToString();
            }
        }
    }

    internal static class PzProjectSettingsProvider
    {
        public static PzProjectSettings GetOrCreate()
        {
            PzProjectSettings existing = AssetDatabase.LoadAssetAtPath<PzProjectSettings>(PzProjectSettings.AssetPath);
            if (existing != null)
            {
                existing.Audio?.EnsureBedDefaults();
                MigrateAtmosphereTransitionDefaults(existing);
                return existing;
            }

            existing = Resources.Load<PzProjectSettings>(PzProjectSettings.ResourcePath);
            if (existing != null)
            {
                existing.Audio?.EnsureBedDefaults();
                MigrateAtmosphereTransitionDefaults(existing);
                return existing;
            }

            string folder = "Assets/_ProjectZone/Resources";
            if (!AssetDatabase.IsValidFolder("Assets/_ProjectZone"))
                AssetDatabase.CreateFolder("Assets", "_ProjectZone");
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/_ProjectZone", "Resources");

            var created = ScriptableObject.CreateInstance<PzProjectSettings>();
            SeedDefaults(created);
            AssetDatabase.CreateAsset(created, PzProjectSettings.AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ProjectZone] Created settings asset at {PzProjectSettings.AssetPath}");
            return created;
        }

        /// <summary>
        /// Old #57 defaults (Sheltered haze 0.65) were too subtle vs Outdoor — nudge once.
        /// </summary>
        private static void MigrateAtmosphereTransitionDefaults(PzProjectSettings settings)
        {
            if (settings?.Atmosphere == null)
                return;

            PzProjectSettings.AtmosphereSettings atmo = settings.Atmosphere;
            bool dirty = false;
            if (Mathf.Abs(atmo.ShelteredHazeMultiplier - 0.65f) < 0.02f)
            {
                atmo.ShelteredHazeMultiplier = 0.22f;
                dirty = true;
            }

            if (Mathf.Abs(atmo.ShelteredSunMultiplier - 0.85f) < 0.02f)
            {
                atmo.ShelteredSunMultiplier = 0.55f;
                dirty = true;
            }

            if (atmo.OutdoorHazeBoost < 1.01f)
            {
                atmo.OutdoorHazeBoost = 1.45f;
                dirty = true;
            }

            if (dirty)
                EditorUtility.SetDirty(settings);
        }

        private static void SeedDefaults(PzProjectSettings settings)
        {
            PzProjectSettings.ZombieSettings z = settings.Zombies;
            z.FemaleZombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/04_Monsters/ZombieFemaleAnimations/Prefabs/FemaleZombie.prefab");
            z.EmeraldRangedFallbackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/01_System/Emerald AI/Demo/Demo Source/Demo Abilities/Ability Source/Other/Example Summoned AI 1.prefab");
            z.IdleClip = LoadClip(
                "Assets/04_Monsters/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Idle01.FBX",
                "ZombieFemale_Idle01");
            z.WalkClip = LoadClip(
                "Assets/04_Monsters/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Walk01Forward.FBX",
                "ZombieFemale_Walk01Forward");
            z.RunClip = LoadClip(
                "Assets/04_Monsters/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Run01ForwardInPlace.FBX",
                "ZombieFemale_Run01ForwardInPlace");
            z.AttackClip = LoadClip(
                "Assets/04_Monsters/ZombieFemaleAnimations/Art/Animations/ZombieFemale_AttackInPlace01.FBX",
                "ZombieFemale_AttackInPlace01");
            z.DeathClip = LoadClip(
                "Assets/04_Monsters/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Death01.FBX",
                "ZombieFemale_Death01");

            settings.Player.StartingWeaponPickupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/01_System/PolymindGames/HQFPS/Prefabs/ItemPickups/HQFPS_Pickup_M1911.prefab");

            settings.Audio.EnsureBedDefaults();
            settings.Audio.Outdoor = new PzProjectSettings.AmbientBedSettings
                { Volume = 0.34f, Muffle = 0.02f };
            settings.Audio.Settlement = new PzProjectSettings.AmbientBedSettings
                { Volume = 0.32f, Muffle = 0.25f };
            settings.Audio.SettlementIndoor = new PzProjectSettings.AmbientBedSettings
                { Volume = 0.3f, Muffle = 0.65f };
            settings.Audio.IndustrialIndoor = new PzProjectSettings.AmbientBedSettings
                { Volume = 0.3f, Muffle = 0.45f };
            settings.Audio.Underground = new PzProjectSettings.AmbientBedSettings
                { Volume = 0.28f, Muffle = 0.8f };
        }

        private static AnimationClip LoadClip(string fbxPath, string preferredName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            AnimationClip fallback = null;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not AnimationClip clip)
                    continue;
                if (clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                    continue;
                if (clip.name == preferredName)
                    return clip;
                fallback ??= clip;
            }

            return fallback;
        }
    }
}
#endif
