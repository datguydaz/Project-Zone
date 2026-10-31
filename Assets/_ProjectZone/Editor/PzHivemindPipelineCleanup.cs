#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Removes Built-in-ness from Hivemind leftovers when a URP twin already exists.
    /// Does not delete Mi_* files (FBX material search is by name) — retargets them to the URP twin shader.
    /// </summary>
    public static class PzHivemindPipelineCleanup
    {
        private const string LogPrefix = "[ProjectZone][HivemindCleanup]";
        private const string HivemindRoot = "Assets/05_Environments/Hivemind";
        private const string BuiltinShaderGuid = "0000000000000000f000000000000000";

        private static readonly string[] FantasySkip =
        {
            "FantasyCemetery",
            "HauntedFarmHouse"
        };

        [MenuItem("Tools/PROJECT ZONE/URP/Cleanup Hivemind Built-in Leftovers", priority = 55)]
        public static void CleanupMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "PROJECT ZONE — Hivemind Cleanup",
                    "Find Built-in materials under Hivemind.\n" +
                    "Where a URP M_* twin exists, retarget the Built-in Mi_* material to that twin's shader (keeps FBX name refs).\n" +
                    "Skips Fantasy Cemetery / Haunted Farm.\n\n" +
                    "No HDRP shader assets were found in audit — only stale YAML tags on URP mats (left alone).\n\nContinue?",
                    "Cleanup",
                    "Cancel"))
                return;

            CleanupBuiltinLeftovers(skipFantasy: true, showDialog: true);
        }

        public static void CleanupBuiltinLeftovers(bool skipFantasy, bool showDialog)
        {
            if (!AssetDatabase.IsValidFolder(HivemindRoot))
            {
                Debug.LogError($"{LogPrefix} Missing folder: {HivemindRoot}");
                return;
            }

            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { HivemindRoot });
            var builtinMats = new List<Material>();
            var report = new StringBuilder();
            int skippedFantasy = 0;

            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (skipFantasy && FantasySkip.Any(f => path.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    skippedFantasy++;
                    continue;
                }

                if (!IsBuiltinMaterialFile(path))
                    continue;

                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                    builtinMats.Add(mat);
            }

            int retargeted = 0;
            int noTwin = 0;
            int failed = 0;

            try
            {
                for (int i = 0; i < builtinMats.Count; i++)
                {
                    Material builtIn = builtinMats[i];
                    string path = AssetDatabase.GetAssetPath(builtIn);
                    EditorUtility.DisplayProgressBar(
                        "Hivemind Built-in Cleanup",
                        $"{builtIn.name} ({i + 1}/{builtinMats.Count})",
                        (float)(i + 1) / Math.Max(1, builtinMats.Count));

                    Material twin = FindUrpTwin(builtIn, path);
                    if (twin == null || twin == builtIn)
                    {
                        noTwin++;
                        report.AppendLine($"NO TWIN: {path}");
                        continue;
                    }

                    if (twin.shader == null || twin.shader.name == "Hidden/InternalErrorShader")
                    {
                        noTwin++;
                        report.AppendLine($"TWIN BROKEN: {path} -> {AssetDatabase.GetAssetPath(twin)}");
                        continue;
                    }

                    try
                    {
                        // Keep the Mi_* asset (FBX looks up by material name) but use the URP twin shader + shared textures.
                        builtIn.shader = twin.shader;
                        builtIn.CopyPropertiesFromMaterial(twin);
                        EditorUtility.SetDirty(builtIn);
                        retargeted++;
                        report.AppendLine($"OK: {path} -> shader from {AssetDatabase.GetAssetPath(twin)}");
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        report.AppendLine($"FAIL: {path}: {ex.Message}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            string summary =
                $"Built-in leftovers found: {builtinMats.Count}\n" +
                $"Retargeted to URP twin shader: {retargeted}\n" +
                $"No safe URP twin: {noTwin}\n" +
                $"Failed: {failed}\n" +
                $"Fantasy paths skipped: {skippedFantasy}";

            Debug.Log($"{LogPrefix} {summary}\n{report}");
            if (showDialog)
                EditorUtility.DisplayDialog("PROJECT ZONE", summary + "\n\nDetails in Console.", "OK");
        }

        private static bool IsBuiltinMaterialFile(string path)
        {
            try
            {
                string yaml = File.ReadAllText(path);
                return yaml.Contains(BuiltinShaderGuid) || yaml.Contains("m_Shader: {fileID: 46,");
            }
            catch
            {
                return false;
            }
        }

        private static Material FindUrpTwin(Material builtIn, string builtInPath)
        {
            string name = builtIn.name;
            string twinName = name;
            if (name.StartsWith("Mi_", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("MI_", StringComparison.Ordinal))
            {
                twinName = "M_" + name.Substring(3);
            }

            // Prefer twin under the same pack's Art/Materials (AbandonedFactory pattern).
            string packRoot = GetPackRoot(builtInPath);
            if (!string.IsNullOrEmpty(packRoot))
            {
                string[] candidates = AssetDatabase.FindAssets($"{twinName} t:Material", new[] { packRoot });
                foreach (string guid in candidates)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path == builtInPath)
                        continue;
                    if (IsBuiltinMaterialFile(path))
                        continue;
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat != null && mat.name == twinName)
                        return mat;
                }
            }

            // Fallback: any non-Built-in material with twin name under Hivemind.
            string[] global = AssetDatabase.FindAssets($"{twinName} t:Material", new[] { HivemindRoot });
            foreach (string guid in global)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == builtInPath || IsBuiltinMaterialFile(path))
                    continue;
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && mat.name == twinName)
                    return mat;
            }

            return null;
        }

        private static string GetPackRoot(string assetPath)
        {
            // Assets/05_Environments/Hivemind/<Pack>/...
            string norm = assetPath.Replace('\\', '/');
            const string marker = "/Hivemind/";
            int idx = norm.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return null;
            int start = idx + marker.Length;
            int slash = norm.IndexOf('/', start);
            if (slash < 0)
                return norm.Substring(0, start).TrimEnd('/');
            return norm.Substring(0, slash);
        }
    }
}
#endif
