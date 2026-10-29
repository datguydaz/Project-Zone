using System.Collections;
using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 5 / #27 — hybrid death: inside SafeZone soft respawn (no cache);
    /// outside SafeZone spill backpack into recoverable death cache then allow HQ respawn.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzHybridDeathController : MonoBehaviour
    {
        [SerializeField]
        private bool _bindOnStart = true;

        private Player _player;
        private IHealthManager _health;
        private bool _bound;

        private void Start()
        {
            if (_bindOnStart)
                StartCoroutine(BindWhenReady());
        }

        private void OnDestroy() => Unbind();

        private IEnumerator BindWhenReady()
        {
            float elapsed = 0f;
            while (Player.AllPlayers.Count == 0 && elapsed < 8f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (Player.AllPlayers.Count == 0)
                yield break;

            yield return null;
            Bind(Player.AllPlayers[0]);
        }

        public void Bind(Player player)
        {
            Unbind();
            _player = player;
            if (_player == null)
                return;

            _health = _player.HealthManager;
            if (_health == null)
                return;

            _health.Death += OnDeath;
            _bound = true;
            Debug.Log("[ProjectZone][Death] Hybrid death rules bound to player.", this);
        }

        private void Unbind()
        {
            if (_bound && _health != null)
                _health.Death -= OnDeath;

            _bound = false;
            _health = null;
            _player = null;
        }

        private void OnDeath(in DamageArgs args)
        {
            if (_player == null)
                return;

            bool inSafeZone = PzSafeZoneTracker.Instance == null || PzSafeZoneTracker.Instance.IsInSafeZone;
            Vector3 deathPos = _player.transform.position;

            if (inSafeZone)
            {
                Debug.Log("[ProjectZone][Death] Soft death inside safe zone — no death cache.", this);
                PzExpeditionLoop.Instance?.NotifySoftDeath();
                return;
            }

            PzDeathCacheService cache = PzDeathCacheService.Instance;
            if (cache == null)
            {
                Debug.LogWarning("[ProjectZone][Death] Death cache service missing — cannot spill backpack.", this);
                return;
            }

            int moved = cache.SpillBackpackFrom(_player, deathPos);
            PzExpeditionLoop.Instance?.NotifyExpeditionDeath(deathPos, moved);
            Debug.Log(
                $"[ProjectZone][Death] Expedition death at {deathPos} — backpack spilled ({moved}). Recover at orange CACHE or Settlement claim terminal.",
                this);
        }
    }
}
