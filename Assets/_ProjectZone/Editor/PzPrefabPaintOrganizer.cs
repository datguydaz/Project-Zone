#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectZone;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Sorts painted foliage into Tree/Bush/Grass/... folders.
    /// Handles props parented under Art AND props left at scene root (common paint bug).
    /// </summary>
    [InitializeOnLoad]
    public static class PzPrefabPaintOrganizer
    {
        private const string RequestPath = "Temp/PZ_OrganizePaint.request";
        private const string FallbackRootName = "PZ_PaintedFoliage";
        private const string LogPrefix = "[ProjectZone][PrefabPaint]";

        static PzPrefabPaintOrganizer()
        {
            EditorApplication.delayCall += TryConsumeRequest;
        }

        private static void TryConsumeRequest()
        {
            if (!File.Exists(RequestPath))
                return;
            try { File.Delete(RequestPath); }
            catch { return; }

            OrganizeOpenScenes(showDialog: false);
        }

        [MenuItem("Tools/PROJECT ZONE/Organize Painted Hierarchy (Scene)", priority = 61)]
        public static void OrganizeMenu()
        {
            OrganizeOpenScenes(showDialog: true);
        }

        public static void QueueRequest()
        {
            Directory.CreateDirectory("Temp");
            File.WriteAllText(RequestPath, "organize");
            Debug.Log($"{LogPrefix} Organize queued — runs on next Editor delayCall.");
        }

        public static void OrganizeOpenScenes(bool showDialog)
        {
            PzPrefabPaintPalette palette = PzPrefabPaintPalette.GetOrCreate();
            List<GameObject> palettePrefabs = palette.Entries
                .Select(e => e.LoadPrefab())
                .Where(p => p != null)
                .ToList();

            int moved = 0;
            int renamed = 0;
            int folders = 0;
            int found = 0;
            var touchedParents = new HashSet<Transform>();

            // Prefer active scene; also process all loaded.
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded)
                    continue;

                var props = new HashSet<GameObject>();

                // 1) Marker components (works even at scene root)
                PzPaintedProp[] marked = UnityEngine.Object.FindObjectsByType<PzPaintedProp>(FindObjectsInactive.Include);
                foreach (PzPaintedProp marker in marked)
                {
                    if (marker != null && marker.gameObject.scene == scene)
                        props.Add(marker.gameObject);
                }

                // 2) Name heuristic for root clones without relying on Art parent
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name.IndexOf("(Clone)", StringComparison.Ordinal) >= 0)
                        props.Add(root);

                    foreach (Transform art in FindNamedDescendants(root.transform, "Art"))
                    {
                        for (int i = 0; i < art.childCount; i++)
                        {
                            Transform child = art.GetChild(i);
                            if (IsCategoryFolderName(child.name) || child.name == FallbackRootName)
                                continue;
                            if (MatchesPalette(child.gameObject, palettePrefabs)
                                || child.GetComponent<PzPaintedProp>() != null
                                || child.name.IndexOf("(Clone)", StringComparison.Ordinal) >= 0)
                                props.Add(child.gameObject);
                        }
                    }
                }

                found += props.Count;
                Transform fallbackRoot = null;

                foreach (GameObject go in props.ToList())
                {
                    if (go == null)
                        continue;
                    if (go.name == FallbackRootName)
                        continue;
                    if (IsCategoryFolderName(go.name) && go.GetComponent<PzPaintedProp>() == null)
                        continue;

                    Transform rootParent = ResolveOrganizeRoot(go.transform, scene, ref fallbackRoot, ref folders);
                    if (rootParent == null)
                    {
                        Debug.LogWarning($"{LogPrefix} No parent for '{go.name}' — skipped.");
                        continue;
                    }

                    GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject
                                        ?? PrefabUtility.GetCorrespondingObjectFromOriginalSource(go) as GameObject;
                    PzPaintCategory cat = GuessCategory(source != null ? source : go, palette);
                    Transform folder = EnsureCategoryFolder(rootParent, cat, ref folders);
                    touchedParents.Add(rootParent);

                    string cleanName = go.name;
                    if (cleanName.IndexOf("(Clone)", StringComparison.Ordinal) >= 0)
                    {
                        cleanName = cleanName.Replace("(Clone)", string.Empty).TrimEnd();
                        Undo.RecordObject(go, "PZ Rename Clone");
                        go.name = cleanName;
                        renamed++;
                        EditorUtility.SetDirty(go);
                    }

                    if (go.transform.parent != folder)
                    {
                        // Keep world pose when reparenting from scene root.
                        Vector3 pos = go.transform.position;
                        Quaternion rot = go.transform.rotation;
                        Vector3 scale = go.transform.localScale;
                        Undo.SetTransformParent(go.transform, folder, "PZ Organize Hierarchy");
                        go.transform.SetPositionAndRotation(pos, rot);
                        go.transform.localScale = scale;
                        moved++;
                        EditorUtility.SetDirty(go);
                    }
                }

                if (fallbackRoot != null)
                    touchedParents.Add(fallbackRoot);
            }

            foreach (Transform parent in touchedParents)
            {
                if (parent == null)
                    continue;
                SortChildrenByName(parent);
                foreach (PzPaintCategory cat in Enum.GetValues(typeof(PzPaintCategory)))
                {
                    Transform folder = parent.Find(cat.ToString());
                    if (folder != null)
                        SortChildrenByName(folder);
                }
            }

            if (moved > 0 || renamed > 0 || folders > 0)
            {
                EditorSceneManager.MarkAllScenesDirty();
                // Don't SaveAssets alone — user must Ctrl+S scene; still mark dirty so Hierarchy updates.
            }

            string msg =
                $"Found {found} painted props.\n" +
                $"Moved: {moved}\nRenamed: {renamed}\nFolders created: {folders}\n\n" +
                $"Root folder (if needed): {FallbackRootName}/Tree|Bush|Grass|...\n" +
                "Save the scene (Ctrl+S) after this.";

            Debug.Log($"{LogPrefix} {msg.Replace("\n", " | ")}");
            if (showDialog)
                EditorUtility.DisplayDialog("PROJECT ZONE — Organize", msg, "OK");
        }

        /// <summary>
        /// Art ancestor if present; else category-folder parent; else create/use PZ_PaintedFoliage at scene root.
        /// </summary>
        private static Transform ResolveOrganizeRoot(
            Transform prop,
            Scene scene,
            ref Transform fallbackRoot,
            ref int folders)
        {
            if (prop.parent != null && IsCategoryFolderName(prop.parent.name))
                return prop.parent.parent != null ? prop.parent.parent : prop.parent;

            Transform t = prop.parent;
            while (t != null)
            {
                if (string.Equals(t.name, "Art", StringComparison.OrdinalIgnoreCase))
                    return t;
                if (string.Equals(t.name, FallbackRootName, StringComparison.Ordinal))
                    return t;
                t = t.parent;
            }

            // Scene-root painted props (Father = 0) — park under a dedicated root.
            if (fallbackRoot == null)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == FallbackRootName)
                    {
                        fallbackRoot = root.transform;
                        break;
                    }
                }
            }

            if (fallbackRoot == null)
            {
                var go = new GameObject(FallbackRootName);
                Undo.RegisterCreatedObjectUndo(go, "PZ Painted Foliage Root");
                SceneManager.MoveGameObjectToScene(go, scene);
                fallbackRoot = go.transform;
                folders++;
            }

            return fallbackRoot;
        }

        private static IEnumerable<Transform> FindNamedDescendants(Transform root, string name)
        {
            if (string.Equals(root.name, name, StringComparison.OrdinalIgnoreCase))
                yield return root;
            for (int i = 0; i < root.childCount; i++)
            {
                foreach (Transform hit in FindNamedDescendants(root.GetChild(i), name))
                    yield return hit;
            }
        }

        private static PzPaintCategory GuessCategory(GameObject prefab, PzPrefabPaintPalette palette)
        {
            if (prefab == null)
                return PzPaintCategory.Other;

            string path = AssetDatabase.GetAssetPath(prefab);
            string guid = string.IsNullOrEmpty(path) ? null : AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
            {
                PzPaintPrefabEntry entry = palette.Entries.FirstOrDefault(e => e.PrefabGuid == guid);
                if (entry != null)
                    return entry.Category;
            }

            // Name heuristics for clones without asset path (Instantiate fallback)
            string n = prefab.name.ToLowerInvariant();
            if (n.Contains("leaf") || n.Contains("grass") || n.Contains("fern") || n.Contains("weed") || n.Contains("moss"))
                return PzPaintCategory.Grass;
            if (n.Contains("bush") || n.Contains("shrub"))
                return PzPaintCategory.Bush;
            if (n.Contains("tree") || n.Contains("pine") || n.Contains("fir") || n.Contains("oak"))
                return PzPaintCategory.Tree;
            if (n.Contains("flower") || n.Contains("bloom"))
                return PzPaintCategory.Flower;
            if (n.Contains("rock") || n.Contains("stone"))
                return PzPaintCategory.Rock;

            return PzPrefabPaintPalette.GuessCategory(prefab);
        }

        private static Transform EnsureCategoryFolder(Transform rootParent, PzPaintCategory category, ref int folders)
        {
            string folderName = category.ToString();
            Transform existing = rootParent.Find(folderName);
            if (existing != null)
                return existing;

            var folder = new GameObject(folderName);
            Undo.RegisterCreatedObjectUndo(folder, "PZ Paint Category Folder");
            folder.transform.SetParent(rootParent, false);
            folder.transform.localPosition = Vector3.zero;
            folder.transform.localRotation = Quaternion.identity;
            folder.transform.localScale = Vector3.one;
            folders++;
            return folder.transform;
        }

        private static bool MatchesPalette(GameObject go, List<GameObject> palettePrefabs)
        {
            if (go == null || palettePrefabs == null || palettePrefabs.Count == 0)
                return false;

            UnityEngine.Object source = PrefabUtility.GetCorrespondingObjectFromSource(go)
                                        ?? PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
            string goName = go.name.Replace("(Clone)", string.Empty).TrimEnd();

            foreach (GameObject prefab in palettePrefabs)
            {
                if (prefab == null)
                    continue;
                if (source == prefab || (source != null && source.name == prefab.name))
                    return true;
                if (goName == prefab.name || goName.StartsWith(prefab.name + " ", StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static bool IsCategoryFolderName(string name)
        {
            foreach (PzPaintCategory cat in Enum.GetValues(typeof(PzPaintCategory)))
            {
                if (string.Equals(name, cat.ToString(), StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static void SortChildrenByName(Transform parent)
        {
            var children = new List<Transform>();
            for (int i = 0; i < parent.childCount; i++)
                children.Add(parent.GetChild(i));
            children = children.OrderBy(t => t.name, StringComparer.OrdinalIgnoreCase).ToList();
            for (int i = 0; i < children.Count; i++)
                children[i].SetSiblingIndex(i);
        }
    }
}
#endif
