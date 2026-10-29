using System.Collections;
using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Wires Milestone 2 combat adapters in the integration test scene
    /// and waits for the HQ player spawned by the scene game mode.
    /// Validation checklists default off after M2 sign-off — re-enable toggles only when retesting.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class DamageAdapterTestSetup : MonoBehaviour
    {
        private const string LegacyPlayerName = "Example Player";
        private const string DemoCameraName = "Main Camera";

        [Header("Adapters (keep on)")]
        [Tooltip("Log Emerald target registration / AI relation diagnostics.")]
        [SerializeField]
        private bool _logDetectionDiagnostics = false;

        [Header("Completed M2 validators (off by default)")]
        [Tooltip("Issue #11 combat checklist. Leave off after milestone sign-off to avoid console spam.")]
        [SerializeField]
        private bool _enableCombatValidation = false;

        [Tooltip("Issue #12 death/hit checklist + temporary immortality gate. Leave off after sign-off.")]
        [SerializeField]
        private bool _enableDeathHitValidation = false;

        [Tooltip("Issue #13 one-shot package compatibility report. Leave off after sign-off.")]
        [SerializeField]
        private bool _enablePackageCompatibilityValidation = false;

        private IEnumerator Start()
        {
            DisableNamedObject(LegacyPlayerName);
            DisableNamedObject(DemoCameraName);
            AttachAiDamageReceivers();

            while (Player.AllPlayers.Count == 0)
                yield return null;

            AttachPlayerAdapters(Player.AllPlayers[0]);
        }

        private static void DisableNamedObject(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            if (target != null)
                target.SetActive(false);
        }

        private static void AttachAiDamageReceivers()
        {
            EmeraldAI.EmeraldHealth[] healthComponents =
                FindObjectsByType<EmeraldAI.EmeraldHealth>(FindObjectsInactive.Exclude);

            for (int i = 0; i < healthComponents.Length; i++)
            {
                EmeraldAI.EmeraldHealth health = healthComponents[i];
                EmeraldDamageReceiver receiver = health.GetComponent<EmeraldDamageReceiver>();
                if (receiver == null)
                    receiver = health.gameObject.AddComponent<EmeraldDamageReceiver>();

                receiver.Configure();
            }
        }

        private void AttachPlayerAdapters(Player player)
        {
            EmeraldHqPlayerTargetAdapter targetAdapter = player.GetComponent<EmeraldHqPlayerTargetAdapter>();
            if (targetAdapter == null)
                targetAdapter = player.gameObject.AddComponent<EmeraldHqPlayerTargetAdapter>();

            targetAdapter.Configure(player, _logDetectionDiagnostics);

            if (_enableCombatValidation)
            {
                EmeraldCombatLoopValidator combatValidator = player.GetComponent<EmeraldCombatLoopValidator>();
                if (combatValidator == null)
                    combatValidator = player.gameObject.AddComponent<EmeraldCombatLoopValidator>();

                combatValidator.BindPlayer(player);
            }

            if (_enableDeathHitValidation)
            {
                EmeraldDeathHitReactionValidator deathValidator =
                    player.GetComponent<EmeraldDeathHitReactionValidator>();
                if (deathValidator == null)
                    deathValidator = player.gameObject.AddComponent<EmeraldDeathHitReactionValidator>();

                deathValidator.BindPlayer(player);
            }

            if (_enablePackageCompatibilityValidation)
            {
                PackageCompatibilityValidator compat =
                    player.GetComponent<PackageCompatibilityValidator>();
                if (compat == null)
                    compat = player.gameObject.AddComponent<PackageCompatibilityValidator>();

                compat.RunReport(player);
            }
        }
    }
}
