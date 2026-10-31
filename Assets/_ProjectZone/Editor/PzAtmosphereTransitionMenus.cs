#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectZone.Editor
{
    /// <summary>
    /// #57 indoor/outdoor atmosphere volumes + director.
    /// </summary>
    public static class PzAtmosphereTransitionMenus
    {
        private const string LogPrefix = "[ProjectZone][Atmosphere][#57]";

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Ensure HQFPS Post Processing Volume", priority = 79)]
        public static void EnsureHqfpsPostVolume()
        {
            Volume volume = PzLightingApplicator.EnsureHqfpsPostVolume();
            if (volume == null)
            {
                EditorUtility.DisplayDialog("PROJECT ZONE", "Could not load HQFPS_PostProcessing prefab.", "OK");
                return;
            }

            PzLightingApplicator.BindPolymindActiveVolume(volume);
            Selection.activeGameObject = volume.transform.root.gameObject;
            EditorGUIUtility.PingObject(volume);
            EditorSceneManager.MarkSceneDirty(volume.gameObject.scene);
            Debug.Log($"{LogPrefix} HQFPS_PostProcessing ensured (Polymind ActiveVolume).");
        }

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Ensure Space Transition Director", priority = 80)]
        public static void EnsureDirector()
        {
            PzAtmosphereTransitionDirector director = PzAtmosphereTransitionDirector.EnsureInScene();
            Selection.activeGameObject = director.gameObject;
            EditorGUIUtility.PingObject(director);
            EditorSceneManager.MarkSceneDirty(director.gameObject.scene);
            Debug.Log($"{LogPrefix} AtmosphereTransitionDirector ready.");
        }

        [MenuItem("Tools/PROJECT ZONE/Atmosphere/Seed Space Transition Zones", priority = 81)]
        public static void SeedSpaceZones()
        {
            int added = 0;

            // Whole Settlement = Sheltered (less haze than open road, still outdoor-ish).
            added += EnsureZone(
                "Zone_Settlement",
                "AtmoZone_SettlementSheltered",
                new Vector3(0f, 2f, 0f),
                new Vector3(36f, 6f, 32f),
                PzAtmosphereSpace.Sheltered,
                priority: 10);

            // Building interiors = Indoor (clear air).
            added += EnsureZone(
                "Zone_Settlement",
                "AtmoZone_HabA",
                new Vector3(-10f, 2f, 8f),
                new Vector3(7f, 3.5f, 5f),
                PzAtmosphereSpace.Indoor,
                priority: 20);

            added += EnsureZone(
                "Zone_Settlement",
                "AtmoZone_HabB",
                new Vector3(-10f, 2f, -8f),
                new Vector3(7f, 3.5f, 5f),
                PzAtmosphereSpace.Indoor,
                priority: 20);

            added += EnsureZone(
                "Zone_Industrial",
                "AtmoZone_Warehouse",
                new Vector3(-7f, 3.5f, 0f),
                new Vector3(9f, 6f, 14f),
                PzAtmosphereSpace.Indoor,
                priority: 30);

            added += EnsureZone(
                "Zone_UndergroundEntrance",
                "AtmoZone_Tunnel",
                new Vector3(0f, 1.5f, 10f),
                new Vector3(4f, 2.8f, 7f),
                PzAtmosphereSpace.Indoor,
                priority: 40);

            PzAtmosphereTransitionDirector.EnsureInScene();
            EditorSceneManager.MarkAllScenesDirty();

            Debug.Log($"{LogPrefix} Seeded/refreshed space zones (+{added} new). " +
                      "Open road = Outdoor haze → Settlement = Sheltered → Hab/warehouse = Indoor clear.");
            EditorUtility.DisplayDialog(
                "PROJECT ZONE Atmosphere",
                "Space transition zones ready.\n\n" +
                "• Outside Settlement → Outdoor (full HAZE)\n" +
                "• Settlement courtyard → Sheltered (less haze)\n" +
                "• Hab / Warehouse / Tunnel → Indoor (much clearer)\n\n" +
                "Tune: Settings → Atmosphere → Space transitions.\n" +
                "Console: Space → Outdoor / Sheltered / Indoor",
                "OK");
        }

        private static int EnsureZone(
            string zoneRootName,
            string volumeName,
            Vector3 localPos,
            Vector3 localScale,
            PzAtmosphereSpace space,
            int priority)
        {
            GameObject zoneRoot = GameObject.Find(zoneRootName);
            if (zoneRoot == null)
            {
                Debug.LogWarning($"{LogPrefix} Zone root missing: {zoneRootName}");
                return 0;
            }

            PzZoneMarker marker = zoneRoot.GetComponent<PzZoneMarker>();
            Transform parent = marker != null && marker.VolumesRoot != null
                ? marker.VolumesRoot
                : zoneRoot.transform;

            Transform existing = parent.Find(volumeName);
            if (existing != null)
            {
                existing.localPosition = localPos;
                existing.localScale = localScale;
                PzAtmosphereZone zone = existing.GetComponent<PzAtmosphereZone>();
                if (zone == null)
                    zone = Undo.AddComponent<PzAtmosphereZone>(existing.gameObject);
                zone.Configure(space, priority);
                EditorUtility.SetDirty(existing.gameObject);
                return 0;
            }

            GameObject volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(volume, "Seed Atmosphere Zone");
            volume.name = volumeName;
            volume.transform.SetParent(parent, false);
            volume.transform.localPosition = localPos;
            volume.transform.localScale = localScale;

            Collider col = volume.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            MeshRenderer renderer = volume.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = false;

            PzAtmosphereZone atmoZone = Undo.AddComponent<PzAtmosphereZone>(volume);
            atmoZone.Configure(space, priority);
            return 1;
        }
    }
}
#endif
