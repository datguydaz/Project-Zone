using System.Text;
using PolymindGames;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZone
{
    /// <summary>
    /// Play Mode checklist for Milestone 5 / #27–#32.
    /// F8 logs status. Kill self outside safe zone (fall/damage) to test death cache.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzExpeditionLoopValidator : MonoBehaviour
    {
        [SerializeField]
        private bool _logChecklist = true;

        [SerializeField]
        private float _statusIntervalSeconds = 14f;

        private float _statusTimer;
        private bool _sawSafeZone;
        private bool _sawWallet;
        private bool _sawDeathCacheService;
        private bool _sawContractBoard;
        private bool _sawClaimTerminal;
        private bool _sawHybridDeath;

        private void Update()
        {
            Observe();

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f8Key.wasPressedThisFrame)
                LogStatus("Manual F8");

            _statusTimer += Time.unscaledDeltaTime;
            if (_logChecklist && _statusTimer >= _statusIntervalSeconds)
            {
                _statusTimer = 0f;
                LogStatus("Checklist pulse");
            }
        }

        private void Observe()
        {
            if (PzSafeZoneTracker.Instance != null)
                _sawSafeZone = true;
            if (PzWallet.Instance != null)
                _sawWallet = true;
            if (PzDeathCacheService.Instance != null)
                _sawDeathCacheService = true;
            if (FindAnyObjectByType<PzContractBoardInteractable>() != null)
                _sawContractBoard = true;
            if (FindAnyObjectByType<PzDeathCacheInteractable>() != null)
                _sawClaimTerminal = true;
            if (FindAnyObjectByType<PzHybridDeathController>() != null)
                _sawHybridDeath = true;
        }

        private void LogStatus(string context)
        {
            var sb = new StringBuilder();
            sb.Append("[ProjectZone][Expedition] ").Append(context).Append(" | ");
            sb.Append("safeZone=").Append(_sawSafeZone ? "OK" : "…").Append(' ');
            sb.Append("wallet=").Append(_sawWallet ? $"OK({PzWallet.Instance?.Balance})" : "…").Append(' ');
            sb.Append("deathSvc=").Append(_sawDeathCacheService ? "OK" : "…").Append(' ');
            sb.Append("board=").Append(_sawContractBoard ? "OK" : "…").Append(' ');
            sb.Append("claim=").Append(_sawClaimTerminal ? "OK" : "…").Append(' ');
            sb.Append("hybrid=").Append(_sawHybridDeath ? "OK" : "…").Append(' ');
            if (PzExpeditionLoop.Instance != null)
                sb.Append(PzExpeditionLoop.Instance.GetStatusLine());
            if (PzSafeZoneTracker.Instance != null)
                sb.Append(" inSafe=").Append(PzSafeZoneTracker.Instance.IsInSafeZone);
            Debug.Log(sb.ToString(), this);
        }
    }
}
