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
            {
                NotifyPostDeathIgnored();
                return DamageResult.Ignored;
            }

            int amount = Mathf.Max(1, Mathf.CeilToInt(damage));
            Transform attacker = ResolveAttackerTransform(args);
            // Emerald hit reactions / attacker detection work best with a living attacker transform.
            int ragdollForce = Mathf.Clamp(Mathf.RoundToInt(args.HitForce.magnitude), 50, 1000);
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

        private void NotifyPostDeathIgnored()
        {
            EmeraldDeathHitReactionValidator validator = FindAnyObjectByType<EmeraldDeathHitReactionValidator>();
            validator?.NotifyPostDeathDamageIgnored(_health);
        }

        private static Transform ResolveAttackerTransform(in DamageArgs args)
        {
            if (args.Source == null)
                return Player.AllPlayers.Count > 0 ? Player.AllPlayers[0].transform : null;

            Transform sourceTransform = args.Source.transform;
            Player player = sourceTransform.GetComponentInParent<Player>();
            if (player != null)
                return player.transform;

            Character character = sourceTransform.GetComponentInParent<Character>();
            if (character != null)
                return character.transform;

            return sourceTransform;
        }
    }
}
