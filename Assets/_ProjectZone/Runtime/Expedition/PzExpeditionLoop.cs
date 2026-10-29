using UnityEngine;

namespace ProjectZone
{
    public enum PzExpeditionPhase : byte
    {
        AtHub = 0,
        InField = 1,
        DeadPendingRecovery = 2
    }

    /// <summary>
    /// Milestone 5 / #32 — lightweight expedition session state for the Outskirts loop.
    /// </summary>
    [DefaultExecutionOrder(-52)]
    [DisallowMultipleComponent]
    public sealed class PzExpeditionLoop : MonoBehaviour
    {
        private static PzExpeditionLoop _instance;
        private PzExpeditionPhase _phase = PzExpeditionPhase.AtHub;
        private Vector3 _lastDeathPosition;
        private int _lastSpillCount;

        public static PzExpeditionLoop Instance => _instance;
        public PzExpeditionPhase Phase => _phase;
        public Vector3 LastDeathPosition => _lastDeathPosition;
        public int LastSpillCount => _lastSpillCount;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void Update()
        {
            if (PzSafeZoneTracker.Instance == null)
                return;

            if (_phase == PzExpeditionPhase.DeadPendingRecovery)
                return;

            _phase = PzSafeZoneTracker.Instance.IsInSafeZone
                ? PzExpeditionPhase.AtHub
                : PzExpeditionPhase.InField;
        }

        public void NotifySoftDeath()
        {
            _phase = PzExpeditionPhase.AtHub;
        }

        public void NotifyExpeditionDeath(Vector3 deathPosition, int spillCount)
        {
            _lastDeathPosition = deathPosition;
            _lastSpillCount = spillCount;
            _phase = PzExpeditionPhase.DeadPendingRecovery;
        }

        public void NotifyCacheRecovered()
        {
            if (_phase == PzExpeditionPhase.DeadPendingRecovery)
                _phase = PzSafeZoneTracker.Instance != null && PzSafeZoneTracker.Instance.IsInSafeZone
                    ? PzExpeditionPhase.AtHub
                    : PzExpeditionPhase.InField;

            PzContractBoardService.Instance?.NotifyDeathCacheRecovered();
        }

        public string GetStatusLine()
        {
            string wallet = PzWallet.Instance != null ? PzWallet.Instance.Balance.ToString() : "?";
            string contract = PzContractBoardService.Instance?.ActiveContract != null
                ? PzContractBoardService.Instance.ActiveContract.DisplayName
                : "none";
            string cache = PzDeathCacheService.Instance != null && PzDeathCacheService.Instance.HasPendingCache
                ? "YES"
                : "no";
            return $"phase={_phase} credits={wallet} contract={contract} deathCache={cache}";
        }
    }
}
