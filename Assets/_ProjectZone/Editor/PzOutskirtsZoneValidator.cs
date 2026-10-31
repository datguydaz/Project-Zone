#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectZone.Editor
{
    /// <summary>
    /// M6 zone hierarchy checks for PZ_Outskirts (#41 / #46).
    /// </summary>
    [InitializeOnLoad]
    public static class PzOutskirtsZoneValidator
    {
        private const string ScenePath = "Assets/_ProjectZone/Scenes/PZ_Outskirts.unity";
        private const string BakeRequestPath = "Temp/PZ_BakeNavMesh.request"; // BakeRequestToken

        static PzOutskirtsZoneValidator()
        {
            EditorApplication.delayCall += TryConsumeBakeRequest;
        }

        private static void TryConsumeBakeRequest()
        {
            if (!File.Exists(BakeRequestPath))
                return;

            try
            {
                File.Delete(BakeRequestPath);
            }
            catch
            {
                return;
            }

            BakeNavMeshInternal(showDialog: false);
        }

        [MenuItem("Tools/PROJECT ZONE/World/Validate Outskirts Zones", priority = 20)]
        public static void ValidateZones()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.path.EndsWith("PZ_Outskirts.unity"))
            {
                if (!EditorUtility.DisplayDialog(
                        "PROJECT ZONE",
                        "Active scene is not PZ_Outskirts. Open it now?",
                        "Open",
                        "Cancel"))
                    return;

                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            PzZoneMarker[] markers = Object.FindObjectsByType<PzZoneMarker>(FindObjectsInactive.Include);
            int issues = 0;

            if (markers.Length == 0)
            {
                Debug.LogWarning(
                    "[ProjectZone][M6] No PzZoneMarker found. Rebuild greybox: select OutskirtsGreyboxBootstrap → Rebuild Outskirts Greybox.");
                issues++;
            }

            bool hasSettlement = false, hasCheckpoint = false, hasIndustrial = false, hasUnderground = false;
            foreach (PzZoneMarker marker in markers)
            {
                switch (marker.ZoneId)
                {
                    case PzZoneId.Settlement: hasSettlement = true; break;
                    case PzZoneId.Checkpoint: hasCheckpoint = true; break;
                    case PzZoneId.Industrial: hasIndustrial = true; break;
                    case PzZoneId.Underground: hasUnderground = true; break;
                    default: break;
                }

                if (marker.GreyboxRoot == null || marker.ArtRoot == null || marker.CollisionRoot == null
                    || marker.SpawnsRoot == null || marker.VolumesRoot == null)
                {
                    Debug.LogWarning($"[ProjectZone][M6] Zone '{marker.name}' missing hierarchy folders.", marker);
                    issues++;
                }
            }

            if (!hasSettlement) { Debug.LogWarning("[ProjectZone][M6] Missing Settlement zone."); issues++; }
            if (!hasCheckpoint) { Debug.LogWarning("[ProjectZone][M6] Missing Checkpoint zone."); issues++; }
            if (!hasIndustrial) { Debug.LogWarning("[ProjectZone][M6] Missing Industrial zone."); issues++; }
            if (!hasUnderground) { Debug.LogWarning("[ProjectZone][M6] Missing Underground zone."); issues++; }

            if (issues == 0)
            {
                Debug.Log(
                    $"[ProjectZone][M6] Zone validation OK — {markers.Length} markers (Settlement/Checkpoint/Industrial/Underground).");
                EditorUtility.DisplayDialog("PROJECT ZONE", $"Zone validation OK ({markers.Length} markers).", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "PROJECT ZONE",
                    $"Zone validation found {issues} issue(s). See Console.",
                    "OK");
            }
        }

        [MenuItem("Tools/PROJECT ZONE/World/Rebuild Outskirts Greybox", priority = 21)]
        public static void RebuildGreybox()
        {
            OutskirtsGreyboxBootstrap bootstrap = Object.FindAnyObjectByType<OutskirtsGreyboxBootstrap>();
            if (bootstrap == null)
            {
                EditorUtility.DisplayDialog(
                    "PROJECT ZONE",
                    "No OutskirtsGreyboxBootstrap in the open scene. Open PZ_Outskirts first.",
                    "OK");
                return;
            }

            bootstrap.BuildLayout();
            EditorSceneManager.MarkSceneDirty(bootstrap.gameObject.scene);
            Debug.Log("[ProjectZone][M6] Outskirts greybox rebuilt (layout v6).");
        }

        [MenuItem("Tools/PROJECT ZONE/World/Bake Outskirts NavMesh", priority = 22)]
        public static void BakeNavMesh()
        {
            BakeNavMeshInternal(showDialog: true);
        }

        public static bool BakeNavMeshInternal(bool showDialog)
        {
            EnsureOutskirtsSceneLoaded();

            OutskirtsNpcBootstrap npc = Object.FindAnyObjectByType<OutskirtsNpcBootstrap>();
            Unity.AI.Navigation.NavMeshSurface surface = Object.FindAnyObjectByType<Unity.AI.Navigation.NavMeshSurface>();

            if (surface == null && npc != null)
                surface = npc.GetComponent<Unity.AI.Navigation.NavMeshSurface>();

            if (surface == null)
            {
                const string msg =
                    "No NavMeshSurface in the open scene. Open PZ_Outskirts (needs a NavMeshSurface on the region root).";
                Debug.LogWarning("[ProjectZone][M6] " + msg);
                if (showDialog)
                    EditorUtility.DisplayDialog("PROJECT ZONE", msg, "OK");
                return false;
            }

            surface.BuildNavMesh();
            EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[ProjectZone][M6] NavMesh baked on '{surface.gameObject.name}'.", surface);
            if (showDialog)
                EditorUtility.DisplayDialog("PROJECT ZONE", "NavMesh bake finished and scene saved.", "OK");
            return true;
        }

        private static void EnsureOutskirtsSceneLoaded()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path.EndsWith("PZ_Outskirts.unity"))
                return;

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
#endif
