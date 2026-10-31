#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectZone.Editor
{
    /// <summary>
    /// #56 ambient audio — simplified: Outdoor vs Settlement compound.
    /// </summary>
    public static class PzAudioMenus
    {
        private const string LogPrefix = "[ProjectZone][Audio][#56]";

        [MenuItem("Tools/PROJECT ZONE/Audio/Ensure Ambient Audio Director", priority = 100)]
        public static void EnsureDirector()
        {
            PzAmbientAudioDirector director = PzAmbientAudioDirector.EnsureInScene();
            Selection.activeGameObject = director.gameObject;
            EditorGUIUtility.PingObject(director);
            EditorSceneManager.MarkSceneDirty(director.gameObject.scene);
            Debug.Log($"{LogPrefix} AmbientAudioDirector ready.");
        }

        [MenuItem("Tools/PROJECT ZONE/Audio/Seed Greybox Audio Zones", priority = 101)]
        public static void SeedGreyboxZones()
        {
            // Drop the old Yard/Hab split — one volume for the whole Settlement.
            RemoveZone("Zone_Settlement", "AudioZone_SettlementYard");
            RemoveZone("Zone_Settlement", "AudioZone_HabA");
            RemoveZone("Zone_Settlement", "AudioZone_HabB");

            int added = 0;
            added += EnsureZone(
                "Zone_Settlement",
                "AudioZone_Settlement",
                new Vector3(0f, 2f, 0f),
                new Vector3(36f, 6f, 32f),
                PzAmbientKind.SettlementYard,
                priority: 10);

            // Keep industrial / underground for later traversal tests.
            added += EnsureZone(
                "Zone_Industrial",
                "AudioZone_Warehouse",
                new Vector3(-7f, 3.5f, 0f),
                new Vector3(9f, 6f, 14f),
                PzAmbientKind.IndustrialIndoor,
                priority: 30);

            added += EnsureZone(
                "Zone_UndergroundEntrance",
                "AudioZone_Tunnel",
                new Vector3(0f, 1.5f, 10f),
                new Vector3(4f, 2.8f, 7f),
                PzAmbientKind.Underground,
                priority: 40);

            PzAmbientAudioDirector.EnsureInScene();
            EditorSceneManager.MarkAllScenesDirty();

            Debug.Log($"{LogPrefix} Simplified zones (+{added} new). Inside Settlement walls = Settlement bed; outside = Outdoor.");
            EditorUtility.DisplayDialog(
                "PROJECT ZONE Audio",
                "Simplified setup:\n\n" +
                "• Inside Settlement compound → Settlement bed\n" +
                "• Outside Settlement → Outdoor bed\n\n" +
                "Save scene, then Play. Console: Ambient → SettlementYard / Outdoor",
                "OK");
        }

        [MenuItem("Tools/PROJECT ZONE/Audio/Open Audio Zones Doc", priority = 102)]
        public static void OpenDoc()
        {
            string path = System.IO.Path.GetFullPath("docs/environment/AUDIO_ZONES.md");
            if (System.IO.File.Exists(path))
                EditorUtility.OpenWithDefaultApp(path);
            else
                Debug.LogWarning($"{LogPrefix} Missing docs/environment/AUDIO_ZONES.md");
        }

        private static void RemoveZone(string zoneRootName, string volumeName)
        {
            GameObject zoneRoot = GameObject.Find(zoneRootName);
            if (zoneRoot == null)
                return;

            PzZoneMarker marker = zoneRoot.GetComponent<PzZoneMarker>();
            Transform parent = marker != null && marker.VolumesRoot != null
                ? marker.VolumesRoot
                : zoneRoot.transform;

            Transform existing = parent.Find(volumeName);
            if (existing == null)
                return;

            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        private static int EnsureZone(
            string zoneRootName,
            string volumeName,
            Vector3 localPos,
            Vector3 localScale,
            PzAmbientKind kind,
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
                PzAudioZone zone = existing.GetComponent<PzAudioZone>();
                if (zone == null)
                    zone = Undo.AddComponent<PzAudioZone>(existing.gameObject);
                zone.Configure(kind, priority);
                EditorUtility.SetDirty(existing.gameObject);
                return 0;
            }

            GameObject volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(volume, "Seed Audio Zone");
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

            PzAudioZone audioZone = Undo.AddComponent<PzAudioZone>(volume);
            audioZone.Configure(kind, priority);
            return 1;
        }
    }
}
#endif
