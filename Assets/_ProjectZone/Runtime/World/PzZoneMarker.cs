using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Marks a zone root under Outskirts_Zones. Used by validators, spawners, and future streaming.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzZoneMarker : MonoBehaviour
    {
        [SerializeField]
        private PzZoneId _zoneId = PzZoneId.None;

        [SerializeField]
        private bool _isSafeZone;

        [SerializeField]
        private string _displayName = "";

        [SerializeField]
        private Transform _greyboxRoot;

        [SerializeField]
        private Transform _artRoot;

        [SerializeField]
        private Transform _collisionRoot;

        [SerializeField]
        private Transform _spawnsRoot;

        [SerializeField]
        private Transform _volumesRoot;

        public PzZoneId ZoneId => _zoneId;
        public bool IsSafeZone => _isSafeZone;
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
        public Transform GreyboxRoot => _greyboxRoot;
        public Transform ArtRoot => _artRoot;
        public Transform CollisionRoot => _collisionRoot;
        public Transform SpawnsRoot => _spawnsRoot;
        public Transform VolumesRoot => _volumesRoot;

        public void Configure(
            PzZoneId zoneId,
            bool isSafeZone,
            string displayName,
            Transform greybox,
            Transform art,
            Transform collision,
            Transform spawns,
            Transform volumes)
        {
            _zoneId = zoneId;
            _isSafeZone = isSafeZone;
            _displayName = displayName;
            _greyboxRoot = greybox;
            _artRoot = art;
            _collisionRoot = collision;
            _spawnsRoot = spawns;
            _volumesRoot = volumes;
        }
    }
}
