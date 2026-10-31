#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Zone visual style — auto-creates shared URP materials and applies them to
    /// Outskirts greybox (Art untouched). No menu required.
    /// </summary>
    [InitializeOnLoad]
    public static class PzZoneStyleMenus
    {
        private const string LogPrefix = "[ProjectZone][Style]";
        private const string MaterialsFolder = "Assets/_ProjectZone/Art/Materials/Zone";

        private const string MatDirt = "PZ_Zone_Dirt";
        private const string MatAsphalt = "PZ_Zone_Asphalt";
        private const string MatConcrete = "PZ_Zone_Concrete";
        private const string MatPanel = "PZ_Zone_Panel";
        private const string MatRust = "PZ_Zone_RustMetal";
        private const string MatSteel = "PZ_Zone_PaintedSteel";

        private static bool _bootstrapped;

        static PzZoneStyleMenus()
        {
            EditorApplication.delayCall += AutoBoot;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void AutoBoot()
        {
            if (_bootstrapped)
                return;
            _bootstrapped = true;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            AutoApplyIfOutskirts();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            EditorApplication.delayCall += AutoApplyIfOutskirts;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                AutoApplyIfOutskirts();
            else if (state == PlayModeStateChange.EnteredPlayMode)
                EditorApplication.delayCall += AutoApplyIfOutskirts;
        }

        /// <summary>Called after greybox rebuild so new cubes get Zone mats immediately.</summary>
        public static void AutoApplyAfterGreyboxRebuild()
        {
            EditorApplication.delayCall += AutoApplyIfOutskirts;
        }

        private static void AutoApplyIfOutskirts()
        {
            if (!IsOutskirtsSceneOpen())
                return;

            EnsureMaterials();
            int changed = ApplyToAllGreybox(recordUndo: false);
            if (changed > 0)
            {
                Debug.Log($"{LogPrefix} Auto-applied Zone materials to {changed} greybox renderers.");
                if (!Application.isPlaying)
                    EditorSceneManager.MarkAllScenesDirty();
            }
        }

        private static bool IsOutskirtsSceneOpen()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded)
                    continue;
                string name = scene.name ?? string.Empty;
                string path = scene.path ?? string.Empty;
                if (name.IndexOf("Outskirts", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || path.IndexOf("PZ_Outskirts", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return Object.FindAnyObjectByType<OutskirtsGreyboxBootstrap>() != null;
        }

        public static void EnsureMaterials()
        {
            EnsureFolder("Assets/_ProjectZone/Art");
            EnsureFolder("Assets/_ProjectZone/Art/Materials");
            EnsureFolder(MaterialsFolder);

            bool dirty =
                CreateOrUpdateLit(MatDirt, new Color(0.30f, 0.29f, 0.26f), metallic: 0f, smoothness: 0.18f)
                | CreateOrUpdateLit(MatAsphalt, new Color(0.22f, 0.22f, 0.23f), metallic: 0f, smoothness: 0.22f)
                | CreateOrUpdateLit(MatConcrete, new Color(0.42f, 0.41f, 0.39f), metallic: 0f, smoothness: 0.28f)
                | CreateOrUpdateLit(MatPanel, new Color(0.48f, 0.47f, 0.44f), metallic: 0.05f, smoothness: 0.32f)
                | CreateOrUpdateLit(MatRust, new Color(0.38f, 0.28f, 0.22f), metallic: 0.55f, smoothness: 0.35f)
                | CreateOrUpdateLit(MatSteel, new Color(0.36f, 0.38f, 0.40f), metallic: 0.65f, smoothness: 0.42f);

            if (dirty)
                AssetDatabase.SaveAssets();
        }

        private static int ApplyToAllGreybox(bool recordUndo)
        {
            Material dirt = LoadMat(MatDirt);
            Material asphalt = LoadMat(MatAsphalt);
            Material concrete = LoadMat(MatConcrete);
            Material panel = LoadMat(MatPanel);
            Material rust = LoadMat(MatRust);
            Material steel = LoadMat(MatSteel);

            int changed = 0;
            PzZoneMarker[] markers = Object.FindObjectsByType<PzZoneMarker>(FindObjectsInactive.Include);
            foreach (PzZoneMarker marker in markers)
            {
                if (marker == null || marker.GreyboxRoot == null)
                    continue;

                foreach (MeshRenderer renderer in marker.GreyboxRoot.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (renderer == null)
                        continue;

                    Material pick = PickMaterial(renderer.gameObject.name, dirt, asphalt, concrete, panel, rust, steel);
                    if (pick == null || renderer.sharedMaterial == pick)
                        continue;

                    if (recordUndo)
                        Undo.RecordObject(renderer, "Apply Zone Style Material");
                    renderer.sharedMaterial = pick;
                    EditorUtility.SetDirty(renderer);
                    changed++;
                }
            }

            OutskirtsGreyboxBootstrap bootstrap = Object.FindAnyObjectByType<OutskirtsGreyboxBootstrap>();
            if (bootstrap != null)
            {
                Transform ground = bootstrap.transform.Find("Ground");
                if (ground != null)
                {
                    var renderer = ground.GetComponent<MeshRenderer>();
                    if (renderer != null && dirt != null && renderer.sharedMaterial != dirt)
                    {
                        if (recordUndo)
                            Undo.RecordObject(renderer, "Apply Zone Style Material");
                        renderer.sharedMaterial = dirt;
                        EditorUtility.SetDirty(renderer);
                        changed++;
                    }
                }

                // Paths live under Outskirts_Zones root, not Greybox folders.
                Transform zones = bootstrap.transform.Find("Outskirts_Zones");
                if (zones != null)
                {
                    foreach (Transform child in zones)
                    {
                        if (child == null || !child.name.StartsWith("Path_", System.StringComparison.Ordinal))
                            continue;
                        var renderer = child.GetComponent<MeshRenderer>();
                        if (renderer == null || asphalt == null || renderer.sharedMaterial == asphalt)
                            continue;
                        if (recordUndo)
                            Undo.RecordObject(renderer, "Apply Zone Style Material");
                        renderer.sharedMaterial = asphalt;
                        EditorUtility.SetDirty(renderer);
                        changed++;
                    }
                }
            }

            return changed;
        }

        private static Material PickMaterial(
            string objectName,
            Material dirt,
            Material asphalt,
            Material concrete,
            Material panel,
            Material rust,
            Material steel)
        {
            string n = (objectName ?? string.Empty).ToLowerInvariant();

            if (ContainsAny(n, "path_", "road", "lane", "pad", "dockramp", "approach"))
                return asphalt;

            if (ContainsAny(n, "footprint", "courtyard", "yard", "stairmouth", "earth"))
                return dirt;

            if (ContainsAny(n, "tank", "pipe", "fence", "gate", "hatch", "signpost", "boom", "overturned", "roof", "canopy"))
                return rust;

            if (ContainsAny(n, "warehouse", "dock", "office", "steel", "crate", "loading"))
                return steel;

            if (ContainsAny(n, "hab", "wall", "watch", "booth", "barrier", "bunker", "retaining", "jersey", "fallen", "signboard", "tunnel", "ramp", "shed", "pillar"))
                return panel;

            if (ContainsAny(n, "sandbag", "debris", "watchcrate", "interior", "hazard"))
                return concrete;

            return concrete;
        }

        private static bool ContainsAny(string hay, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (hay.Contains(needle))
                    return true;
            }

            return false;
        }

        /// <returns>True if the material asset was created or values changed.</returns>
        private static bool CreateOrUpdateLit(string assetName, Color baseColor, float metallic, float smoothness)
        {
            string path = $"{MaterialsFolder}/{assetName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            bool dirty = false;
            if (mat == null)
            {
                mat = new Material(shader) { name = assetName };
                AssetDatabase.CreateAsset(mat, path);
                dirty = true;
            }
            else if (shader != null && mat.shader != shader)
            {
                mat.shader = shader;
                dirty = true;
            }

            if (mat.HasProperty("_BaseColor") && mat.GetColor("_BaseColor") != baseColor)
            {
                mat.SetColor("_BaseColor", baseColor);
                dirty = true;
            }
            else if (mat.HasProperty("_Color") && mat.GetColor("_Color") != baseColor)
            {
                mat.SetColor("_Color", baseColor);
                dirty = true;
            }

            if (mat.HasProperty("_Metallic") && !Mathf.Approximately(mat.GetFloat("_Metallic"), metallic))
            {
                mat.SetFloat("_Metallic", metallic);
                dirty = true;
            }

            if (mat.HasProperty("_Smoothness") && !Mathf.Approximately(mat.GetFloat("_Smoothness"), smoothness))
            {
                mat.SetFloat("_Smoothness", smoothness);
                dirty = true;
            }
            else if (mat.HasProperty("_Glossiness") && !Mathf.Approximately(mat.GetFloat("_Glossiness"), smoothness))
            {
                mat.SetFloat("_Glossiness", smoothness);
                dirty = true;
            }

            mat.SetFloat("_Surface", 0f);
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetOverrideTag("RenderType", "Opaque");
            mat.SetInt("_ZWrite", 1);
            mat.renderQueue = (int)RenderQueue.Geometry;

            if (dirty)
                EditorUtility.SetDirty(mat);

            return dirty;
        }

        private static Material LoadMat(string assetName)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/{assetName}.mat");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
                return;

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
