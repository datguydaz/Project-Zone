using System.Collections.Generic;
using System.Text;
using EmeraldAI;
using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Play Mode checklist for GitHub issue #12 — AI hit reactions, AI death, and player death.
    /// Observes Emerald/HQ events and polls health as a fallback so deaths are not missed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EmeraldDeathHitReactionValidator : MonoBehaviour
    {
        [SerializeField]
        private bool _logChecklist = true;

        [SerializeField]
        private float _statusIntervalSeconds = 8f;

        [Tooltip("Keep the player immortal until AI death is confirmed so #12 can finish without dying mid-fight.")]
        [SerializeField]
        private bool _immortalUntilAiDeath = true;

        private bool _aiHitReaction;
        private bool _aiDeath;
        private bool _playerDeath;
        private bool _postDeathDamageIgnored;
        private float _statusTimer;
        private int _lowestAiHpSeen = int.MaxValue;
        private IHealthManager _playerHealth;
        private HqPlayerDamageBridge _playerDamageBridge;
        private readonly HashSet<EmeraldSystem> _reportedDeadAi = new HashSet<EmeraldSystem>();
        private EmeraldSystem[] _watchedSystems = System.Array.Empty<EmeraldSystem>();

        public void BindPlayer(Player player)
        {
            if (player == null)
                return;

            _playerHealth = player.HealthManager;
            if (_playerHealth != null)
                _playerHealth.Death += HandlePlayerDeath;

            _playerDamageBridge = player.GetComponent<HqPlayerDamageBridge>();
            if (_playerDamageBridge == null)
                _playerDamageBridge = player.GetComponentInChildren<HqPlayerDamageBridge>();

            ApplyImmortalGate();
            SubscribeAiEvents();
            LogStatus(
                "Death/hit reaction validation started (issue #12). " +
                (_immortalUntilAiDeath
                    ? "Player is immortal until AI DEATH logs — finish one AI to 0 HP, then immortality lifts."
                    : "Kill an AI to 0 HP for the AI death log."));
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.Death -= HandlePlayerDeath;

            if (_playerDamageBridge != null && _immortalUntilAiDeath)
                _playerDamageBridge.Immortal = false;
        }

        private void Update()
        {
            PollAiDeathState();
            SilenceDeathboxFactionWarnings();

            _statusTimer += Time.unscaledDeltaTime;
            if (_logChecklist && _statusTimer >= _statusIntervalSeconds)
            {
                _statusTimer = 0f;
                LogStatus("Checklist pulse");
            }
        }

        private void SubscribeAiEvents()
        {
            _watchedSystems = FindObjectsByType<EmeraldSystem>(FindObjectsInactive.Exclude);
            for (int i = 0; i < _watchedSystems.Length; i++)
            {
                EmeraldSystem system = _watchedSystems[i];
                EmeraldHealth health = system.HealthComponent;
                EmeraldAnimation animation = system.AnimationComponent;
                if (health == null)
                    continue;

                EmeraldSystem captured = system;
                health.OnTakeDamage += () => HandleAiTakeDamage(captured, critical: false);
                health.OnTakeCritDamage += () => HandleAiTakeDamage(captured, critical: true);
                health.OnDeath += () => HandleAiDeath(captured, "OnDeath event");

                if (animation != null)
                {
                    animation.OnGetHit += () =>
                    {
                        _aiHitReaction = true;
                        if (_logChecklist)
                            Debug.Log($"[ProjectZone][DeathHit] AI '{captured.name}' hit reaction animation triggered.", captured);
                    };
                }
            }
        }

        private void PollAiDeathState()
        {
            for (int i = 0; i < _watchedSystems.Length; i++)
            {
                EmeraldSystem system = _watchedSystems[i];
                if (system == null || system.HealthComponent == null)
                    continue;

                bool dead = system.HealthComponent.Health <= 0 ||
                            (system.AnimationComponent != null && system.AnimationComponent.IsDead);
                if (dead)
                    HandleAiDeath(system, "health/IsDead poll");
            }
        }

        private void HandleAiTakeDamage(EmeraldSystem system, bool critical)
        {
            _aiHitReaction = true;
            int hp = system.HealthComponent != null ? system.HealthComponent.Health : -1;
            if (hp >= 0 && hp < _lowestAiHpSeen)
                _lowestAiHpSeen = hp;

            if (_logChecklist)
            {
                Debug.Log(
                    $"[ProjectZone][DeathHit] AI '{system.name}' took {(critical ? "critical " : "")}damage (HP={hp}).",
                    system);
            }

            // Emerald invokes OnTakeDamage after subtracting HP, so a killing blow may report HP=0 here.
            if (hp <= 0)
                HandleAiDeath(system, "killing-blow damage callback");

            if (system.AnimationComponent != null && system.AnimationComponent.IsDead)
                _postDeathDamageIgnored = true;
        }

        private void HandleAiDeath(EmeraldSystem system, string source)
        {
            if (system == null)
                return;

            if (!_reportedDeadAi.Add(system))
                return;

            _aiDeath = true;
            _lowestAiHpSeen = 0;
            ApplyImmortalGate();
            Debug.LogWarning(
                $"[ProjectZone][DeathHit] AI DEATH confirmed on '{system.name}' via {source} " +
                $"(HP={system.HealthComponent?.Health}, IsDead={system.AnimationComponent != null && system.AnimationComponent.IsDead}).",
                system);
        }

        private void ApplyImmortalGate()
        {
            if (!_immortalUntilAiDeath || _playerDamageBridge == null)
                return;

            bool shouldBeImmortal = !_aiDeath;
            if (_playerDamageBridge.Immortal == shouldBeImmortal)
                return;

            _playerDamageBridge.Immortal = shouldBeImmortal;
            if (_logChecklist)
            {
                Debug.Log(
                    shouldBeImmortal
                        ? "[ProjectZone][DeathHit] Player immortal ON until AI death is confirmed."
                        : "[ProjectZone][DeathHit] Player immortal OFF — AI death done; you can die for player-death check if still needed.",
                    this);
            }
        }

        private void HandlePlayerDeath(in DamageArgs args)
        {
            if (_playerDeath)
                return;

            _playerDeath = true;
            Debug.LogWarning("[ProjectZone][DeathHit] PLAYER DEATH confirmed (HQ HealthManager.Death).", this);
            SilenceDeathboxFactionWarnings();
        }

        public void NotifyPostDeathDamageIgnored(EmeraldHealth health)
        {
            _postDeathDamageIgnored = true;
            if (_logChecklist && health != null)
                Debug.Log($"[ProjectZone][DeathHit] Post-death damage ignored on '{health.name}'.", health);
        }

        private static void SilenceDeathboxFactionWarnings()
        {
            // HQ death rigidbody ("Deathbox") can sit on Character/Player layers and spam Emerald detection.
            GameObject deathbox = GameObject.Find("Deathbox");
            if (deathbox == null)
                return;

            if (deathbox.GetComponent<FactionExtension>() == null)
            {
                FactionExtension faction = deathbox.AddComponent<FactionExtension>();
                faction.CurrentFaction = ResolvePlayerFactionIndex();
            }

            // Keep Emerald from treating the corpse prop as a fresh combat target search spam source.
            Collider col = deathbox.GetComponent<Collider>();
            if (col != null)
                deathbox.layer = LayerConstants.Hitbox;
        }

        private static int ResolvePlayerFactionIndex()
        {
            EmeraldFactionData factionData = Resources.Load<EmeraldFactionData>("Faction Data");
            if (factionData?.FactionNameList == null)
                return 2;

            int index = factionData.FactionNameList.IndexOf("Player");
            return index >= 0 ? index : 2;
        }

        private void LogStatus(string reason)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[ProjectZone][DeathHit] {reason}");
            sb.AppendLine($"  [{(_aiHitReaction ? "x" : " ")}] AI hit reaction (damage/anim event)");
            sb.AppendLine($"  [{(_aiDeath ? "x" : " ")}] AI death");
            sb.AppendLine($"  [{(_playerDeath ? "x" : " ")}] Player death (HQ health)");
            sb.AppendLine($"  [{(_postDeathDamageIgnored ? "x" : " ")}] Post-death AI damage ignored (optional)");

            if (_aiHitReaction && _aiDeath && _playerDeath)
                sb.AppendLine("  Status: death/hit checklist complete — confirm and we can close #12.");
            else if (!_aiDeath)
            {
                string hpHint = _lowestAiHpSeen < int.MaxValue
                    ? $" Lowest AI HP this run: {_lowestAiHpSeen}."
                    : string.Empty;
                sb.AppendLine(
                    "  Tip: finish killing an AI to 0 HP (yellow AI DEATH warning)." +
                    (_immortalUntilAiDeath ? " Immortal until then." : string.Empty) +
                    hpHint);
            }
            else if (!_playerDeath)
                sb.AppendLine("  Tip: let an AI kill you for PLAYER DEATH confirmation.");

            Debug.Log(sb.ToString(), this);
        }
    }
}
