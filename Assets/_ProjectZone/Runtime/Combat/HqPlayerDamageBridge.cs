using EmeraldAI;
using PolymindGames;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectZone
{
    /// <summary>
    /// Emerald calls this when an AI hits the player. The damage is applied to the HQ health manager.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HqPlayerDamageBridge : EmeraldPlayerBridge
    {
        private IHealthManager _health;
        private Collider _playerCollider;

        public override void Awake()
        {
            if (OnTakeDamage == null)
                OnTakeDamage = new UnityEvent();
            if (OnDeath == null)
                OnDeath = new UnityEvent();

            base.Awake();
            _playerCollider = GetComponent<Collider>();
        }

        public override void Start()
        {
            var character = GetComponent<Character>();
            _health = character != null ? character.HealthManager : null;
            SyncFromPlayer();

            if (_health != null)
            {
                _health.Respawn += HandleRespawn;
                _health.Death += HandleHqDeath;
            }
        }

        private void OnDestroy()
        {
            if (_health == null)
                return;

            _health.Respawn -= HandleRespawn;
            _health.Death -= HandleHqDeath;
        }

        public override void DamageCharacterController(int damageAmount, Transform attacker)
        {
            if (Immortal)
                return;

            if (_health != null && !_health.IsDead())
            {
                // Let HQ own lethal handling (PlayerDeathHandler). Pass attacker when available.
                var args = new DamageArgs(DamageType.Blunt, null, transform.position, Vector3.zero);
                _health.ReceiveDamage(damageAmount, in args);
                SyncFromPlayer();
            }
            else if (_health == null)
            {
                Health = Mathf.Max(0, Health - damageAmount);
            }

            OnTakeDamage.Invoke();

            // If HQ health is absent, fall back to Emerald bridge death.
            if (_health == null && Health <= 0)
            {
                if (_playerCollider != null)
                    _playerCollider.enabled = false;

                OnDeath.Invoke();
            }
        }

        private void HandleHqDeath(in DamageArgs args)
        {
            SyncFromPlayer();
            if (_playerCollider != null)
                _playerCollider.enabled = false;

            OnDeath.Invoke();
        }

        private void HandleRespawn()
        {
            if (_playerCollider != null)
                _playerCollider.enabled = true;

            ResetTarget();
            SyncFromPlayer();
        }

        private void SyncFromPlayer()
        {
            if (_health == null)
                return;

            StartHealth = Mathf.Max(1, Mathf.RoundToInt(_health.MaxHealth));
            Health = Mathf.Max(0, Mathf.RoundToInt(_health.Health));
        }
    }
}
