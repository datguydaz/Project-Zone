using PolymindGames;
using UnityEngine;
using EmeraldAI;

namespace ProjectZone
{
    /// <summary>
    /// Receives HQ FPS hits on one collider and forwards them to Emerald AI health.
    /// HQ only looks for <see cref="IDamageHandler"/> on the collider that was hit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EmeraldDamageHitbox : MonoBehaviour, IDamageHandler
    {
        private EmeraldHealth _health;
        private bool _criticalHit;

        public ICharacter Character => null;

        public void Bind(EmeraldHealth health, bool criticalHit)
        {
            _health = health;
            _criticalHit = criticalHit;
        }

        public DamageResult HandleDamage(float damage, in DamageArgs args)
        {
            if (_health == null)
                _health = GetComponentInParent<EmeraldHealth>();

            if (_health == null || damage <= 0f)
                return DamageResult.Ignored;

            int healthBefore = _health.Health;
            if (healthBefore <= 0)
                return DamageResult.Ignored;

            int amount = Mathf.Max(1, Mathf.CeilToInt(damage));
            Transform attacker = args.Source != null ? args.Source.transform : null;
            int ragdollForce = Mathf.Clamp(Mathf.RoundToInt(args.HitForce.magnitude), 1, 1000);
            _health.Damage(amount, attacker, ragdollForce, _criticalHit);

            int healthAfter = _health.Health;
            DamageResult result = healthAfter <= 0
                ? DamageResult.Fatal
                : healthAfter < healthBefore
                    ? _criticalHit ? DamageResult.Critical : DamageResult.Normal
                    : DamageResult.Ignored;

            if (result != DamageResult.Ignored)
                DamageTracker.RegisterDamage(this, result, amount, in args);

            return result;
        }
    }
}
