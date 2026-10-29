#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Unity.AI.Navigation;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Rebuilds Assets/_ProjectZone/Scenes/PZ_Outskirts.unity for Milestone 3 / issue #14.
    /// Menu: Project Zone / Scenes / Build Outskirts Region Scene
    /// </summary>
    public static class OutskirtsSceneBuilder
    {
        private const string ScenePath = "Assets/_ProjectZone/Scenes/PZ_Outskirts.unity";
        private const string GameModePrefabPath = "Assets/PolymindGames/HQFPS/Prefabs/Core/HQFPS_GameMode.prefab";

        [MenuItem("Project Zone/Scenes/Build Outskirts Region Scene")]
        public static void BuildFromMenu() => Build();

        /// <summary>Batch: Unity -batchmode -executeMethod ProjectZone.Editor.OutskirtsSceneBuilder.Build</summary>
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.48f, 0.52f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.55f, 0.6f, 0.65f);
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 180f;

            var region = new GameObject("Outskirts Region");
            region.AddComponent<OutskirtsGreyboxBootstrap>().BuildLayout();
            var surface = region.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameModePrefabPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = "GameMode";
                instance.transform.position = new Vector3(0f, 2.2f, -8f);
            }
            else
            {
                Debug.LogError($"[ProjectZone] Missing GameMode prefab at {GameModePrefabPath}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!.Replace('\\', '/'));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            AddSceneToBuildSettings();
            Debug.Log($"[ProjectZone] Built {ScenePath}. Bake NavMesh if needed, then Play Mode walk for #14.");
        }

        private static void AddSceneToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == ScenePath)
                    return;
            }

            var list = new EditorBuildSettingsScene[scenes.Length + 1];
            for (int i = 0; i < scenes.Length; i++)
                list[i] = scenes[i];
            list[scenes.Length] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = list;
        }
    }
}
#endif
