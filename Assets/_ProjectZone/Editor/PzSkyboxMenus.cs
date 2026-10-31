#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Ensures Zone overcast/night procedural skybox materials (replaces default blue sky).
    /// </summary>
    [InitializeOnLoad]
    public static class PzSkyboxMenus
    {
        public const string Folder = "Assets/_ProjectZone/Art/Materials/Atmosphere";
        public const string OvercastPath = Folder + "/PZ_Sky_Overcast.mat";
        public const string NightPath = Folder + "/PZ_Sky_Night.mat";
        public const string OvercastResource = "Atmosphere/PZ_Sky_Overcast";
        public const string NightResource = "Atmosphere/PZ_Sky_Night";

        private static bool _bootstrapped;

        static PzSkyboxMenus()
        {
            EditorApplication.delayCall += AutoEnsure;
        }

        private static void AutoEnsure()
        {
            if (_bootstrapped)
                return;
            _bootstrapped = true;
            EnsureSkyboxMaterials();
        }

        public static void EnsureSkyboxMaterials()
        {
            EnsureFolder("Assets/_ProjectZone/Art");
            EnsureFolder("Assets/_ProjectZone/Art/Materials");
            EnsureFolder(Folder);
            // Also expose via Resources for runtime load.
            EnsureFolder("Assets/_ProjectZone/Resources");
            EnsureFolder("Assets/_ProjectZone/Resources/Atmosphere");

            Material overcast = CreateOrUpdateProcedural(
                OvercastPath,
                atmosphereThickness: 1.85f,
                skyTint: new Color(0.55f, 0.56f, 0.54f),
                ground: new Color(0.32f, 0.31f, 0.28f),
                exposure: 0.85f,
                sunSize: 0.035f,
                sunConvergence: 2f);

            Material night = CreateOrUpdateProcedural(
                NightPath,
                atmosphereThickness: 1.35f,
                skyTint: new Color(0.18f, 0.22f, 0.32f),
                ground: new Color(0.08f, 0.08f, 0.1f),
                exposure: 0.45f,
                sunSize: 0.02f,
                sunConvergence: 5f);

            // Mirror into Resources so runtime Resources.Load works without Editor.
            CopyToResources(overcast, "Assets/_ProjectZone/Resources/Atmosphere/PZ_Sky_Overcast.mat");
            CopyToResources(night, "Assets/_ProjectZone/Resources/Atmosphere/PZ_Sky_Night.mat");

            AssetDatabase.SaveAssets();
        }

        private static Material CreateOrUpdateProcedural(
            string path,
            float atmosphereThickness,
            Color skyTint,
            Color ground,
            float exposure,
            float sunSize,
            float sunConvergence)
        {
            Shader shader = Shader.Find("Skybox/Procedural");
            if (shader == null)
            {
                Debug.LogWarning("[ProjectZone][Sky] Skybox/Procedural shader missing.");
                return null;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            if (mat.HasProperty("_AtmosphereThickness"))
                mat.SetFloat("_AtmosphereThickness", atmosphereThickness);
            if (mat.HasProperty("_SkyTint"))
                mat.SetColor("_SkyTint", skyTint);
            if (mat.HasProperty("_GroundColor"))
                mat.SetColor("_GroundColor", ground);
            if (mat.HasProperty("_Exposure"))
                mat.SetFloat("_Exposure", exposure);
            if (mat.HasProperty("_SunSize"))
                mat.SetFloat("_SunSize", sunSize);
            if (mat.HasProperty("_SunSizeConvergence"))
                mat.SetFloat("_SunSizeConvergence", sunConvergence);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void CopyToResources(Material source, string destPath)
        {
            if (source == null)
                return;

            Material existing = AssetDatabase.LoadAssetAtPath<Material>(destPath);
            if (existing == null)
            {
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source), destPath);
                return;
            }

            existing.shader = source.shader;
            existing.CopyPropertiesFromMaterial(source);
            EditorUtility.SetDirty(existing);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
                return;
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
