#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectZone.Editor
{
    /// <summary>
    /// #55 foliage workflow helpers — Toby primary + Prefab Paint palette seed + TTFE controller.
    /// </summary>
    public static class PzFoliageMenus
    {
        private const string LogPrefix = "[ProjectZone][Foliage][#55]";

        private const string VegetationRoot =
            "Assets/05_Environments/Toby Fredson/Rocky Hills Environment - Whitebark Pine/RHEWP_Demo/Prefabs/Prefabs_Vegetation";

        private const string TtfeControllerPath =
            "Assets/05_Environments/Toby Fredson/The Toby Foliage Engine/(TTFE)_Core/Resources/(TTFE) GLOBAL CONTROLLER/(TTFE) GLOBAL CONTROLLER.prefab";

        [MenuItem("Tools/PROJECT ZONE/Foliage/Seed Paint Palette (Toby Rocky Hills)", priority = 90)]
        public static void SeedPaintPalette()
        {
            PzPrefabPaintPalette palette = PzPrefabPaintPalette.GetOrCreate();
            int before = palette.Entries.Count;
            int added = 0;

            added += AddPrefabsFromFolder(palette, VegetationRoot + "/PV_Trees", PzPaintCategory.Tree, max: 14);
            added += AddPrefabsFromFolder(palette, VegetationRoot + "/PV_Bushes", PzPaintCategory.Bush, max: 12);
            added += AddPrefabsFromFolder(palette, VegetationRoot + "/PV_Grass", PzPaintCategory.Grass, max: 12,
                nameFilter: PreferDryGrass);

            EditorUtility.SetDirty(palette);
            AssetDatabase.SaveAssets();

            Debug.Log($"{LogPrefix} Palette seed: +{added} (was {before}, now {palette.Entries.Count}). " +
                      "Open Prefab Paint Tool to place under zone Art/.");
            EditorUtility.DisplayDialog(
                "PROJECT ZONE Foliage",
                $"Seeded Toby Rocky Hills into Prefab Paint.\nAdded {added} prefabs.\n\n" +
                "Use Tools → PROJECT ZONE → Prefab Paint Tool.\nSee docs/environment/FOLIAGE_WORKFLOW.md",
                "OK");
        }

        [MenuItem("Tools/PROJECT ZONE/Foliage/Ensure TTFE Global Controller", priority = 91)]
        public static void EnsureTtfeController()
        {
            GameObject existing = GameObject.Find("(TTFE) GLOBAL CONTROLLER");
            if (existing != null)
            {
                Selection.activeGameObject = existing;
                EditorGUIUtility.PingObject(existing);
                Debug.Log($"{LogPrefix} TTFE Global Controller already in scene.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TtfeControllerPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("PROJECT ZONE", $"Missing TTFE controller prefab:\n{TtfeControllerPath}", "OK");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "(TTFE) GLOBAL CONTROLLER";
            Undo.RegisterCreatedObjectUndo(instance, "Ensure TTFE Controller");
            EditorSceneManager.MarkSceneDirty(instance.scene);
            Selection.activeGameObject = instance;
            Debug.Log($"{LogPrefix} Spawned TTFE Global Controller for wind/foliage shaders.");
        }

        [MenuItem("Tools/PROJECT ZONE/Foliage/Open Foliage Workflow Doc", priority = 92)]
        public static void OpenDoc()
        {
            string path = Path.GetFullPath("docs/environment/FOLIAGE_WORKFLOW.md");
            if (File.Exists(path))
                EditorUtility.OpenWithDefaultApp(path);
            else
                Debug.LogWarning($"{LogPrefix} Doc missing: docs/environment/FOLIAGE_WORKFLOW.md");
        }

        private static bool PreferDryGrass(string prefabName)
        {
            string n = prefabName.ToLowerInvariant();
            if (n.Contains("(td)"))
                return false;
            // Prefer dry / tall dry for Stalzone murk; still allow simple clumps.
            return n.Contains("dry") || n.Contains("tall") || n.Contains("wheat") || n.Contains("weat")
                   || n.Contains("simplegrass") || n.Contains("clump");
        }

        private static int AddPrefabsFromFolder(
            PzPrefabPaintPalette palette,
            string folder,
            PzPaintCategory category,
            int max,
            System.Func<string, bool> nameFilter = null)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"{LogPrefix} Folder missing: {folder}");
                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            int added = 0;
            foreach (string guid in guids)
            {
                if (added >= max)
                    break;

                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IndexOf("/Inst Terrain Details/", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;
                if (nameFilter != null && !nameFilter(prefab.name))
                    continue;

                if (palette.Entries.Any(e => e.PrefabGuid == guid))
                {
                    // Ensure category lock for workflow consistency.
                    PzPaintPrefabEntry existing = palette.Entries.First(e => e.PrefabGuid == guid);
                    existing.Category = category;
                    existing.Enabled = true;
                    existing.CategoryLocked = true;
                    continue;
                }

                palette.Entries.Add(new PzPaintPrefabEntry
                {
                    PrefabGuid = guid,
                    Category = category,
                    Enabled = true,
                    CategoryLocked = true
                });
                added++;
            }

            return added;
        }
    }
}
#endif
