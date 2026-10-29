using System.Collections;
using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 5 / #27 — tracks whether the local player is inside Settlement SafeZoneVolume.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzSafeZoneTracker : MonoBehaviour
    {
        private const string SafeZoneName = "SafeZoneVolume";

        [SerializeField]
        private Collider _safeZoneCollider;

        [SerializeField]
        private bool _findOnStart = true;

        private bool _isInSafeZone = true;
        private Player _player;

        public static PzSafeZoneTracker Instance { get; private set; }
        public bool IsInSafeZone => _isInSafeZone;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            if (_findOnStart)
                ResolveSafeZone();
            StartCoroutine(BindPlayerWhenReady());
        }

        private void Update()
        {
            if (_player == null || _safeZoneCollider == null)
                return;

            bool inside = _safeZoneCollider.bounds.Contains(_player.transform.position);
            if (inside == _isInSafeZone)
                return;

            _isInSafeZone = inside;
            Debug.Log(_isInSafeZone
                ? "[ProjectZone][Expedition] Entered safe zone (Settlement)."
                : "[ProjectZone][Expedition] Left safe zone — expedition death rules active.", this);
        }

        [ContextMenu("Resolve Safe Zone")]
        public void ResolveSafeZone()
        {
            if (_safeZoneCollider != null)
                return;

            GameObject found = GameObject.Find(SafeZoneName);
            if (found != null)
                _safeZoneCollider = found.GetComponent<Collider>();
        }

        private IEnumerator BindPlayerWhenReady()
        {
            float elapsed = 0f;
            while (Player.AllPlayers.Count == 0 && elapsed < 8f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (Player.AllPlayers.Count > 0)
                _player = Player.AllPlayers[0];
        }
    }
}
