using System;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Lightweight hostile HP for greybox enemies that are not yet on a full Emerald System.
    /// HQ weapons hit through <see cref="PzHostileDamageHitbox"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzHostileHealth : MonoBehaviour
    {
        [SerializeField]
        private int _startingHealth = 80;

        private int _health;

        public int Health => _health;
        public int StartingHealth => _startingHealth;
        public bool IsDead => _health <= 0;

        public event Action Died;

        private void Awake()
        {
            _health = Mathf.Max(1, _startingHealth);
        }

        public void ApplyDamage(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            _health = Mathf.Max(0, _health - amount);
            if (_health <= 0)
                Died?.Invoke();
        }

        public void ResetHealth()
        {
            _health = Mathf.Max(1, _startingHealth);
        }
    }
}
