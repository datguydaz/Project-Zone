#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HyTeKGames.SRPMaterialConverter;
using UnityEditor;
using UnityEngine;
using static HyTeKGames.SRPMaterialConverter.SRPMC_Utilities;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Batch URP conversion via HyTeKGames SRP Material Converter + nested URP packages.
    /// Skips Hivemind Fantasy / Haunted / Carnival fantasy packs.
    /// Menu-driven only — no auto-run / remote triggers.
    /// </summary>
    public static class PzUrpBatchConvert
    {
        private const string LogPrefix = "[ProjectZone][URP]";

        private static readonly string[] MaterialFolders =
        {
            "Assets/05_Environments/russian_buildings",
            "Assets/05_Environments/Abandoned_Road",
            "Assets/05_Environments/Hivemind/AbandonedFactory",
            "Assets/05_Environments/Hivemind/IndustrialFactory",
            "Assets/05_Environments/Hivemind/PostApocalypticTown",
            "Assets/05_Environments/Hivemind/TheSewers",
            "Assets/05_Environments/Hivemind/DestructedBuilding",
            "Assets/05_Environments/Hivemind/URP/DestructedBuildings",
            "Assets/05_Environments/ALP_Assets",
            "Assets/05_Environments/PostSovietWorld",
            "Assets/05_Environments/Abandoned World/Abandoned Depot",
            "Assets/05_Environments/Abandoned World/UndergroundTunnels",
            "Assets/05_Environments/Abandoned World/Modular Subway Tunnels",
            "Assets/05_Environments/Transformer_substation_booth",
            "Assets/05_Environments/BridgeSet",
            "Assets/05_Environments/PlaygroundApocalypse"
        };

        /// <summary>Small nested URP packages safe to auto-import. Multi-GB Hivemind URP packs are manual.</summary>
        private static readonly string[] SmallUrpPackages =
        {
            "Assets/05_Environments/Abandoned World/Abandoned Depot/AbandonedDepot_URP.unitypackage",
            "Assets/05_Environments/Abandoned World/Modular Subway Tunnels/Modular Subway Tunnels URP.unitypackage"
        };

        /// <summary>Large official URP re-exports — import one at a time via menu (hundreds of MB–GB each).</summary>
        private static readonly string[] LargeUrpPackages =
        {
            "Assets/05_Environments/Hivemind/AbandonedFactory/URP/AbandonedFactoryURP.unitypackage",
            "Assets/05_Environments/Hivemind/IndustrialFactory/URP/IndustrialFactoryURP.unitypackage",
            "Assets/05_Environments/Hivemind/PostApocalypticTown/URP/PostApocalypticTownURP.unitypackage",
            "Assets/05_Environments/Hivemind/TheSewers/URP/TheSewersURP.unitypackage",
            "Assets/05_Environments/Hivemind/URP/DestructedBuildingURP.unitypackage"
        };

        private static readonly string[] FantasySkip =
        {
            "FantasyCemetery",
            "HauntedFarmHouse",
            "Creepwood_Carnival",
            "WitchVillage",
            "Gothic"
        };

        [MenuItem("Tools/PROJECT ZONE/URP/Convert Pink Env Packs (SRP Converter)", priority = 50)]
        public static void ConvertMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "PROJECT ZONE — URP Convert",
                    "1) Import small nested URP packages (Depot ~8MB, Subway ~4MB).\n" +
                    "2) Override Built-in/broken materials → URP Lit via SRP Material Converter.\n\n" +
                    "Skips Fantasy Cemetery / Haunted Farm / Carnival.\n" +
                    "Does NOT auto-import multi-GB Hivemind URP packages (use that menu separately).\n\nContinue?",
                    "Convert",
                    "Cancel"))
                return;

            RunFullConversion(showDialogs: true);
        }

        [MenuItem("Tools/PROJECT ZONE/URP/Convert Materials Only (no package import)", priority = 51)]
        public static void ConvertMaterialsOnlyMenu()
        {
            ConvertMaterialsWithSrpConverter(showDialogs: true);
        }

        [MenuItem("Tools/PROJECT ZONE/URP/Import Small Nested URP Packages", priority = 52)]
        public static void ImportSmallPackagesMenu()
        {
            ImportUrpPackages(SmallUrpPackages, showDialogs: true);
        }

        [MenuItem("Tools/PROJECT ZONE/URP/Import Large Hivemind URP Packages (slow)", priority = 53)]
        public static void ImportLargePackagesMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "PROJECT ZONE — Large URP Import",
                    "This imports multi-GB Hivemind URP packages one by one.\n" +
                    "Expect a long Editor freeze. Fantasy packs are skipped.\n\nContinue?",
                    "Import",
                    "Cancel"))
                return;

            ImportUrpPackages(LargeUrpPackages, showDialogs: true);
        }

        private static void RunFullConversion(bool showDialogs)
        {
            ImportUrpPackages(SmallUrpPackages, showDialogs: false);
            ConvertMaterialsWithSrpConverter(showDialogs);
        }

        private static void ImportUrpPackages(IEnumerable<string> packages, bool showDialogs)
        {
            int imported = 0;
            foreach (string package in packages)
            {
                if (FantasySkip.Any(f => package.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;
                if (!File.Exists(package))
                {
                    Debug.LogWarning($"{LogPrefix} Package missing: {package}");
                    continue;
                }

                long mb = new FileInfo(package).Length / (1024 * 1024);
                Debug.Log($"{LogPrefix} Importing ({mb} MB): {package}");
                try
                {
                    AssetDatabase.ImportPackage(package, interactive: false);
                    imported++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{LogPrefix} Import failed: {package}\n{ex.Message}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"{LogPrefix} Package imports finished ({imported}).");
            if (showDialogs)
                EditorUtility.DisplayDialog("PROJECT ZONE", $"Imported {imported} URP package(s). Check Console for errors.", "OK");
        }

        private static void ConvertMaterialsWithSrpConverter(bool showDialogs)
        {
            List<Material> builtinTargets = CollectConvertibleMaterials(preferHdrp: false);
            List<Material> hdrpTargets = CollectConvertibleMaterials(preferHdrp: true);

            // Prefer HDRP pass only for mats that look like HDRP; remove overlap from builtin list.
            var hdrpSet = new HashSet<Material>(hdrpTargets);
            builtinTargets = builtinTargets.Where(m => !hdrpSet.Contains(m)).ToList();

            int total = builtinTargets.Count + hdrpTargets.Count;
            if (total == 0)
            {
                Debug.Log($"{LogPrefix} No Built-in/broken materials found in target folders.");
                if (showDialogs)
                    EditorUtility.DisplayDialog("PROJECT ZONE", "No convertible materials found.", "OK");
                return;
            }

            string folderPath = GetHyTeKGamesFolderPath();
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError($"{LogPrefix} SRPMaterialConverter folder not found.");
                if (showDialogs)
                    EditorUtility.DisplayDialog("PROJECT ZONE", "SRP Material Converter folder not found under Assets.", "OK");
                return;
            }

            Shader urpLit = Shader.Find(pipelinesDefaultShader[SRPMC_Pipeline.URP]);
            if (urpLit == null)
            {
                Debug.LogError($"{LogPrefix} URP Lit shader not found. Is URP installed?");
                return;
            }

            Debug.Log($"{LogPrefix} Queued SRP convert: {builtinTargets.Count} Builtin + {hdrpTargets.Count} HDRP → URP (override).");

            // Direct override path is more reliable for large batches than async void StartConvert + UI.
            if (builtinTargets.Count > 0)
                ConvertMaterialsDirect(builtinTargets, folderPath, urpLit, SRPMC_Pipeline.Builtin, showDialogs: false);
            if (hdrpTargets.Count > 0)
                ConvertMaterialsDirect(hdrpTargets, folderPath, urpLit, SRPMC_Pipeline.HDRP, showDialogs: false);

            string msg = $"Converted {total} materials to URP Lit via SRP Material Converter logic (override).";
            Debug.Log($"{LogPrefix} {msg}");
            if (showDialogs)
                EditorUtility.DisplayDialog("PROJECT ZONE", msg + "\n\nCheck pink assets in Scene view.", "OK");
        }

        /// <summary>
        /// Mirrors SRPMC override path (mat.shader = target, PostProcessMaterial) without the UI window.
        /// </summary>
        private static void ConvertMaterialsDirect(
            List<Material> targets,
            string folderPath,
            Shader urpLit,
            SRPMC_Pipeline originalPipeline,
            bool showDialogs)
        {
            SRPMC_Editor.originalPipeline = originalPipeline;
            SRPMC_Editor.targetPipeline = SRPMC_Pipeline.URP;

            // Need a live EditorWindow instance for private PostProcessMaterial.
            SRPMC_Editor helper = EditorWindow.GetWindow<SRPMC_Editor>();
            helper.titleContent = new GUIContent("SRP Material Converter");
            FieldInfo cachedPathField = typeof(SRPMC_Editor).GetField(
                "cachedFolderPath",
                BindingFlags.Instance | BindingFlags.NonPublic);
            cachedPathField?.SetValue(helper, folderPath);

            MethodInfo postProcess = typeof(SRPMC_Editor).GetMethod(
                "PostProcessMaterial",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (postProcess == null)
            {
                Debug.LogError($"{LogPrefix} PostProcessMaterial not found.");
                return;
            }

            int ok = 0;
            int fail = 0;
            string label = originalPipeline.ToString();
            try
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    Material mat = targets[i];
                    if (mat == null)
                        continue;

                    EditorUtility.DisplayProgressBar(
                        $"PROJECT ZONE URP Convert ({label})",
                        $"{mat.name} ({i + 1}/{targets.Count})",
                        (float)(i + 1) / targets.Count);

                    try
                    {
                        ParsedMaterial parsed = ParseMaterial(mat);
                        if (parsed == null)
                        {
                            fail++;
                            continue;
                        }

                        mat.shader = urpLit;
                        foreach (var word in mat.enabledKeywords)
                            mat.DisableKeyword(word);
                        mat.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)4;
                        mat.DisableKeyword("_EMISSION");

                        postProcess.Invoke(helper, new object[] { mat, parsed });
                        EditorUtility.SetDirty(mat);
                        ok++;
                    }
                    catch (Exception ex)
                    {
                        fail++;
                        Debug.LogError($"{LogPrefix} Failed {mat.name}: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            string msg = $"[{label}→URP] Converted {ok}, failed {fail}.";
            Debug.Log($"{LogPrefix} {msg}");
            if (showDialogs)
                EditorUtility.DisplayDialog("PROJECT ZONE", msg, "OK");
        }

        /// <param name="preferHdrp">
        /// When true, only materials that look like HDRP.
        /// When false, Built-in / InternalError / Standard (and yaml Built-in guid), excluding clear HDRP.
        /// </param>
        private static List<Material> CollectConvertibleMaterials(bool preferHdrp)
        {
            var list = new List<Material>();
            const string BuiltinShaderGuid = "0000000000000000f000000000000000";
            const string HdrpOrmGuid = "49c17798665c1184480ab4befe6b629e";

            foreach (string folder in MaterialFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                    continue;
                if (FantasySkip.Any(f => folder.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;

                string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (FantasySkip.Any(f => path.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
                        continue;

                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat == null)
                        continue;

                    bool isHdrp = false;
                    bool isBuiltinBroken = false;

                    if (mat.shader != null)
                    {
                        string sn = mat.shader.name;
                        if (sn.Contains("HDRP") || sn.Contains("HDRenderPipeline") || sn.Contains("High Definition"))
                            isHdrp = true;
                        else if (sn == "Hidden/InternalErrorShader"
                                 || sn == "Standard"
                                 || sn.StartsWith("Standard (", StringComparison.Ordinal)
                                 || sn.Contains("Legacy Shaders"))
                            isBuiltinBroken = true;
                    }
                    else
                    {
                        isBuiltinBroken = true;
                    }

                    try
                    {
                        string yaml = File.ReadAllText(path);
                        if (yaml.Contains(HdrpOrmGuid))
                            isHdrp = true;
                        else if (yaml.Contains(BuiltinShaderGuid))
                            isBuiltinBroken = true;
                    }
                    catch
                    {
                        // ignore read errors
                    }

                    if (preferHdrp)
                    {
                        if (isHdrp)
                            list.Add(mat);
                    }
                    else if (isBuiltinBroken || (mat.shader != null && mat.shader.name == "Hidden/InternalErrorShader"))
                    {
                        if (!isHdrp)
                            list.Add(mat);
                    }
                }
            }

            return list.Distinct().ToList();
        }
    }
}
#endif
