#if UNITY_EDITOR
using Haze.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Ensures <see cref="HazeRendererFeature"/> exists on PC_Renderer for #54 HAZE A/B.
    /// Feature starts inactive until FogBackend = HazeVolumetric.
    /// </summary>
    public static class PzHazeRendererEnsure
    {
        private const string PcRendererPath = "Assets/01_System/Settings/PC_Renderer.asset";
        private const string LogPrefix = "[ProjectZone][M7][#54]";

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Ensure HAZE Renderer Feature", priority = 85)]
        public static void EnsureMenu()
        {
            bool added = EnsureOnPcRenderer(activate: false);
            Debug.Log(added
                ? $"{LogPrefix} Added HazeRendererFeature to PC_Renderer (inactive until FogBackend=Haze)."
                : $"{LogPrefix} HazeRendererFeature already present on PC_Renderer.");
        }

        public static bool EnsureOnPcRenderer(bool activate)
        {
            UniversalRendererData renderer =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(PcRendererPath);
            if (renderer == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing {PcRendererPath}");
                return false;
            }

            SerializedObject so = new SerializedObject(renderer);
            SerializedProperty featuresProp = so.FindProperty("m_RendererFeatures");
            if (featuresProp == null || !featuresProp.isArray)
                return false;

            HazeRendererFeature existing = null;
            for (int i = 0; i < featuresProp.arraySize; i++)
            {
                Object obj = featuresProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (obj is HazeRendererFeature haze)
                {
                    existing = haze;
                    break;
                }
            }

            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<HazeRendererFeature>();
                existing.name = "HazeRendererFeature";
                AssetDatabase.AddObjectToAsset(existing, renderer);
                featuresProp.InsertArrayElementAtIndex(featuresProp.arraySize);
                featuresProp.GetArrayElementAtIndex(featuresProp.arraySize - 1).objectReferenceValue = existing;

                // Let URP rebuild m_RendererFeatureMap on serialize/save.
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(renderer);
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
            }

            existing.SetActive(activate);
            EditorUtility.SetDirty(existing);
            return existing != null;
        }
    }
}
#endif
