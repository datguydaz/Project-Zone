using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Standard child folders under each Outskirts zone root (M6 modular construction).
    /// </summary>
    public static class PzZoneHierarchy
    {
        public const string Greybox = "Greybox";
        public const string Art = "Art";
        public const string Collision = "Collision";
        public const string Spawns = "Spawns";
        public const string Volumes = "Volumes";

        public static PzZoneMarker CreateZoneRoot(
            Transform parent,
            string objectName,
            PzZoneId zoneId,
            bool isSafeZone,
            string displayName,
            Vector3 worldPosition)
        {
            var zone = new GameObject(objectName);
            zone.transform.SetParent(parent, false);
            zone.transform.position = worldPosition;

            Transform greybox = CreateChild(zone.transform, Greybox);
            Transform art = CreateChild(zone.transform, Art);
            Transform collision = CreateChild(zone.transform, Collision);
            Transform spawns = CreateChild(zone.transform, Spawns);
            Transform volumes = CreateChild(zone.transform, Volumes);

            PzZoneMarker marker = zone.AddComponent<PzZoneMarker>();
            marker.Configure(zoneId, isSafeZone, displayName, greybox, art, collision, spawns, volumes);
            return marker;
        }

        public static Transform CreateChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
                return existing;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        public static void CreateSpawnPoint(Transform spawnsRoot, string name, Vector3 localPosition)
        {
            if (spawnsRoot == null)
                return;

            var go = new GameObject(name);
            go.transform.SetParent(spawnsRoot, false);
            go.transform.localPosition = localPosition;
        }
    }
}
