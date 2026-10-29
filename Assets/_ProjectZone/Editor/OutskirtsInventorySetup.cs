#if UNITY_EDITOR
using System.IO;
using PolymindGames.SaveSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectZone.Editor
{
    /// <summary>
    /// Wires Milestone 4 inventory/persistence into PZ_Outskirts without rebuilding greybox.
    /// Menu: Project Zone / Inventory / Setup Outskirts Inventory & Persistence
    /// </summary>
    public static class OutskirtsInventorySetup
    {
        private const string ScenePath = "Assets/_ProjectZone/Scenes/PZ_Outskirts.unity";
        private const string LevelDefinitionPath = "Assets/_ProjectZone/Data/PZ_Outskirts_LevelDefinition.asset";
        private const string LootTablePath =
            "Assets/PolymindGames/FPSCore/Data/Inventory/LootTables/FPS_WeaponsAndAmmo.asset";

        [MenuItem("Project Zone/Inventory/Setup Outskirts Inventory & Persistence")]
        public static void SetupFromMenu()
        {
            EnsureLevelDefinition();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject region = GameObject.Find("Outskirts Region");
            if (region == null)
            {
                Debug.LogError("[ProjectZone] Outskirts Region missing — build the Outskirts scene first.");
                return;
            }

            var bootstrap = region.GetComponent<OutskirtsInventoryBootstrap>();
            if (bootstrap == null)
                bootstrap = region.AddComponent<OutskirtsInventoryBootstrap>();

            var so = new SerializedObject(bootstrap);
            so.FindProperty("_lootTable").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(LootTablePath);
            so.FindProperty("_levelDefinition").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelDefinitionPath);
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsureSceneSaveHandler(region.scene);
            bootstrap.EnsureInventoryContent(forceRebuild: true);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectZone] Outskirts inventory & persistence setup saved to PZ_Outskirts.");
        }

        [MenuItem("Project Zone/Inventory/Create Outskirts Level Definition")]
        public static void EnsureLevelDefinitionMenu() => EnsureLevelDefinition();

        private static LevelDefinition EnsureLevelDefinition()
        {
            Directory.CreateDirectory("Assets/_ProjectZone/Data");
            var existing = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelDefinitionPath);
            if (existing != null)
                return existing;

            var definition = ScriptableObject.CreateInstance<LevelDefinition>();
            AssetDatabase.CreateAsset(definition, LevelDefinitionPath);

            var so = new SerializedObject(definition);
            so.FindProperty("_name").stringValue = "The Outskirts";
            so.FindProperty("_description").stringValue =
                "PROJECT ZONE expedition region — Settlement, Checkpoint, Industrial, Underground entrance.";

            SerializedProperty sceneProp = so.FindProperty("_scene");
            if (sceneProp != null)
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
                SerializedProperty sceneRef = sceneProp.FindPropertyRelative("sceneReference");
                if (sceneRef != null)
                    sceneRef.objectReferenceValue = sceneAsset;

                SerializedProperty sceneName = sceneProp.FindPropertyRelative("sceneName");
                if (sceneName != null)
                    sceneName.stringValue = "PZ_Outskirts";

                SerializedProperty scenePathProp = sceneProp.FindPropertyRelative("scenePath");
                if (scenePathProp != null)
                    scenePathProp.stringValue = ScenePath;

                int buildIndex = -1;
                EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    if (buildScenes[i].path == ScenePath)
                    {
                        buildIndex = i;
                        break;
                    }
                }

                SerializedProperty buildIndexProp = sceneProp.FindPropertyRelative("buildIndex");
                if (buildIndexProp != null)
                    buildIndexProp.intValue = buildIndex;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ProjectZone] Created {LevelDefinitionPath}");
            return definition;
        }

        private static void EnsureSceneSaveHandler(Scene scene)
        {
            if (SceneSaveHandler.TryGetHandler(scene, out _))
                return;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.GetComponentInChildren<SceneSaveHandler>(true) != null)
                    return;
            }

            var go = new GameObject("PZ_SceneSaveHandler");
            SceneManager.MoveGameObjectToScene(go, scene);
            var handler = go.AddComponent<SceneSaveHandler>();
            var so = new SerializedObject(handler);
            so.FindProperty("_levelDefinition").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelDefinitionPath);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
