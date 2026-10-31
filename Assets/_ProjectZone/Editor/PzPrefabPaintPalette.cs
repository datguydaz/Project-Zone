#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectZone.Editor
{
    public enum PzPaintCategory
    {
        Tree = 0,
        Bush = 1,
        Grass = 2,
        Flower = 3,
        Rock = 4,
        Clutter = 5,
        Other = 6
    }

    [Serializable]
    public sealed class PzPaintPrefabEntry
    {
        public string PrefabGuid;
        public PzPaintCategory Category = PzPaintCategory.Other;
        public bool Enabled = true;
        public bool CategoryLocked;

        public GameObject LoadPrefab()
        {
            if (string.IsNullOrEmpty(PrefabGuid))
                return null;
            string path = AssetDatabase.GUIDToAssetPath(PrefabGuid);
            if (string.IsNullOrEmpty(path))
                return null;
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }

    /// <summary>
    /// Persistent brush palette for Prefab Paint Tool (project-local).
    /// </summary>
    public sealed class PzPrefabPaintPalette : ScriptableObject
    {
        public const string AssetPath = "Assets/_ProjectZone/Editor/PzPrefabPaintPalette.asset";

        public List<PzPaintPrefabEntry> Entries = new();

        public static PzPrefabPaintPalette GetOrCreate()
        {
            var asset = AssetDatabase.LoadAssetAtPath<PzPrefabPaintPalette>(AssetPath);
            if (asset != null)
                return asset;

            string folder = "Assets/_ProjectZone/Editor";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/_ProjectZone", "Editor");

            asset = CreateInstance<PzPrefabPaintPalette>();
            AssetDatabase.CreateAsset(asset, AssetPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static PzPaintCategory GuessCategory(GameObject prefab)
        {
            if (prefab == null)
                return PzPaintCategory.Other;

            string path = AssetDatabase.GetAssetPath(prefab) ?? string.Empty;
            string hay = (prefab.name + " " + path).ToLowerInvariant();

            if (ContainsAny(hay, "tree", "pine", "fir", "oak", "birch", "spruce", "juniper", "cedar",
                    "willow", "maple", "beech", "poplar", "aspen", "sequoia", "palm", "trunk"))
                return PzPaintCategory.Tree;

            if (ContainsAny(hay, "bush", "shrub", "hedge", "thicket"))
                return PzPaintCategory.Bush;

            if (ContainsAny(hay, "grass", "weed", "fern", "reed", "turf", "lawn", "clover", "moss"))
                return PzPaintCategory.Grass;

            if (ContainsAny(hay, "flower", "bloom", "petal", "daisy", "rose", "tulip", "wildflower"))
                return PzPaintCategory.Flower;

            if (ContainsAny(hay, "rock", "stone", "pebble", "boulder", "cliff", "rubble"))
                return PzPaintCategory.Rock;

            if (ContainsAny(hay, "clutter", "debris", "prop", "crate", "barrel", "trash", "scrap", "log"))
                return PzPaintCategory.Clutter;

            return PzPaintCategory.Other;
        }

        private static bool ContainsAny(string hay, params string[] needles)
        {
            foreach (string n in needles)
            {
                if (hay.Contains(n))
                    return true;
            }
            return false;
        }
    }
}
#endif
