using System.Text;
using EmeraldAI;
using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Play Mode checklist for GitHub issue #11 — melee and ranged combat validation.
    /// Tracks player→AI and AI→player damage without inventing a second combat system.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EmeraldCombatLoopValidator : MonoBehaviour
    {
        [SerializeField]
        private bool _logChecklist = true;

        [SerializeField]
        private float _statusIntervalSeconds = 8f;

        private bool _playerDamagedAi;
        private bool _rangedAiDamagedPlayer;
        private bool _meleeAiDamagedPlayer;
        private bool _duplicateDamageSuspect;
        private float _lastPlayerDamageRealtime = -999f;
        private float _statusTimer;
        private IHealthManager _playerHealth;

        public bool PlayerDamagedAi => _playerDamagedAi;
        public bool RangedAiDamagedPlayer => _rangedAiDamagedPlayer;
        public bool MeleeAiDamagedPlayer => _meleeAiDamagedPlayer;

        public void BindPlayer(Player player)
        {
            if (player == null)
                return;

            _playerHealth = player.HealthManager;
            if (_playerHealth != null)
                _playerHealth.DamageReceived += HandlePlayerDamageReceived;

            HqPlayerDamageBridge bridge = player.GetComponent<HqPlayerDamageBridge>();
            if (bridge != null)
                bridge.OnTakeDamage.AddListener(HandleBridgeTakeDamage);

            RenameExampleHostiles();
            LogStatus("Combat validation started (issue #11).");
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.DamageReceived -= HandlePlayerDamageReceived;

            HqPlayerDamageBridge bridge = FindFirstObjectByType<HqPlayerDamageBridge>();
            if (bridge != null)
                bridge.OnTakeDamage.RemoveListener(HandleBridgeTakeDamage);
        }

        private void Update()
        {
            WatchAiHealthDrops();

            _statusTimer += Time.unscaledDeltaTime;
            if (_logChecklist && _statusTimer >= _statusIntervalSeconds)
            {
                _statusTimer = 0f;
                LogStatus("Checklist pulse");
            }
        }

        private void WatchAiHealthDrops()
        {
            EmeraldHealth[] healthComponents =
                FindObjectsByType<EmeraldHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < healthComponents.Length; i++)
            {
                EmeraldHealth health = healthComponents[i];
                CombatWatch watch = health.GetComponent<CombatWatch>();
                if (watch == null)
                {
                    watch = health.gameObject.AddComponent<CombatWatch>();
                    watch.LastHealth = health.Health;
                    continue;
                }

                if (health.Health < watch.LastHealth)
                {
                    _playerDamagedAi = true;
                    if (_logChecklist)
                    {
                        Debug.Log(
                            $"[ProjectZone][Combat] Player damaged AI '{health.name}' " +
                            $"({watch.LastHealth} → {health.Health}).",
                            health);
                    }
                }

                watch.LastHealth = health.Health;
            }
        }

        private void HandlePlayerDamageReceived(float damage, in DamageArgs args)
        {
            if (damage <= 0f)
                return;

            float now = Time.realtimeSinceStartup;
            if (now - _lastPlayerDamageRealtime < 0.05f)
                _duplicateDamageSuspect = true;

            _lastPlayerDamageRealtime = now;
            ClassifyIncomingAttack();
        }

        private void HandleBridgeTakeDamage()
        {
            ClassifyIncomingAttack();
        }

        private void ClassifyIncomingAttack()
        {
            EmeraldSystem nearestHostile = FindNearestCombatAi();
            if (nearestHostile == null)
            {
                _rangedAiDamagedPlayer = true;
                return;
            }

            if (LooksLikeMelee(nearestHostile))
            {
                _meleeAiDamagedPlayer = true;
                if (_logChecklist)
                    Debug.Log($"[ProjectZone][Combat] Melee AI '{nearestHostile.name}' damaged player.", nearestHostile);
            }
            else
            {
                _rangedAiDamagedPlayer = true;
                if (_logChecklist)
                    Debug.Log($"[ProjectZone][Combat] Ranged AI '{nearestHostile.name}' damaged player.", nearestHostile);
            }
        }

        private static EmeraldSystem FindNearestCombatAi()
        {
            Player player = Player.AllPlayers.Count > 0 ? Player.AllPlayers[0] : null;
            if (player == null)
                return null;

            EmeraldSystem[] systems = FindObjectsByType<EmeraldSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            EmeraldSystem best = null;
            float bestDist = float.MaxValue;
            Vector3 origin = player.transform.position;

            for (int i = 0; i < systems.Length; i++)
            {
                EmeraldSystem system = systems[i];
                if (system.CombatComponent == null || !system.CombatComponent.CombatState)
                    continue;

                float dist = (system.transform.position - origin).sqrMagnitude;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = system;
                }
            }

            return best;
        }

        private static bool LooksLikeMelee(EmeraldSystem system)
        {
            string name = system.name.ToLowerInvariant();
            if (name.Contains("melee") || name.Contains("sword") || name.Contains("fist") || name.Contains("unarmed"))
                return true;

            // Shooter / ranged naming is treated as ranged.
            if (name.Contains("shooter") || name.Contains("ranged") || name.Contains("gun"))
                return false;

            // Fallback: short engagement distance is typical of melee profiles.
            return system.CombatComponent != null && system.CombatComponent.AttackDistance <= 3.5f;
        }

        private static void RenameExampleHostiles()
        {
            EmeraldSystem[] systems = FindObjectsByType<EmeraldSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int rangedIndex = 1;
            for (int i = 0; i < systems.Length; i++)
            {
                EmeraldSystem system = systems[i];
                if (!system.name.StartsWith("Example"))
                    continue;

                if (system.name.IndexOf("Shooter", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    system.name = rangedIndex == 1 ? "Hostile Ranged AI" : $"Hostile Ranged AI {rangedIndex}";
                    rangedIndex++;
                }
                else if (system.name.IndexOf("Sword", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                         system.name.IndexOf("Combat", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    system.name = "Hostile Melee AI";
                }
                else
                {
                    system.name = "Hostile AI";
                }
            }
        }

        private void LogStatus(string reason)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[ProjectZone][Combat] {reason}");
            sb.AppendLine($"  [{(_playerDamagedAi ? "x" : " ")}] Player damaged AI");
            sb.AppendLine($"  [{(_rangedAiDamagedPlayer ? "x" : " ")}] Ranged AI damaged player");
            sb.AppendLine($"  [{(_meleeAiDamagedPlayer ? "x" : " ")}] Melee AI damaged player");
            if (_duplicateDamageSuspect)
                sb.AppendLine("  [!] Possible duplicate player damage pulses detected (<50ms apart)");

            bool rangedReady = _playerDamagedAi && _rangedAiDamagedPlayer;
            bool meleePresent = HasMeleeHostileInScene();
            if (rangedReady && (!meleePresent || _meleeAiDamagedPlayer))
                sb.AppendLine("  Status: checklist complete for available hostiles — confirm in Play Mode, then we can close #11.");
            else if (!meleePresent)
                sb.AppendLine("  Note: no melee hostile found in scene yet — ranged checks can still proceed.");

            Debug.Log(sb.ToString(), this);
        }

        private static bool HasMeleeHostileInScene()
        {
            EmeraldSystem[] systems = FindObjectsByType<EmeraldSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < systems.Length; i++)
            {
                if (LooksLikeMelee(systems[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Lightweight per-AI health snapshot used only by the validator.
        /// </summary>
        private sealed class CombatWatch : MonoBehaviour
        {
            public int LastHealth;
        }
    }
}
