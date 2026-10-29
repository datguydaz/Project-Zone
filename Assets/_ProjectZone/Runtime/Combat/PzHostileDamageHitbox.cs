using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// HQ raycast damage entry point for <see cref="PzHostileHealth"/> hostiles.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzHostileDamageHitbox : MonoBehaviour, IDamageHandler
    {
        private PzHostileHealth _health;

        public ICharacter Character => null;

        public void Bind(PzHostileHealth health)
        {
            _health = health;
        }

        public DamageResult HandleDamage(float damage, in DamageArgs args)
        {
            if (_health == null)
                _health = GetComponentInParent<PzHostileHealth>();

            if (_health == null || _health.IsDead || damage <= 0f)
                return DamageResult.Ignored;

            int before = _health.Health;
            int amount = Mathf.Max(1, Mathf.CeilToInt(damage));
            _health.ApplyDamage(amount);

            int after = _health.Health;
            DamageResult result = after <= 0
                ? DamageResult.Fatal
                : after < before
                    ? DamageResult.Normal
                    : DamageResult.Ignored;

            if (result != DamageResult.Ignored)
                DamageTracker.RegisterDamage(this, result, amount, in args);

            return result;
        }
    }
}
