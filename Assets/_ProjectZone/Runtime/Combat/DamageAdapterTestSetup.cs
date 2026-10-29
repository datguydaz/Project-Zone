using System.Collections;
using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Wires Milestone 2 combat/detection adapters in the integration test scene
    /// and waits for the HQ player spawned by the scene game mode.
    /// Issues: #8/#9 (damage), #10 (detection), #11 (combat loop validation).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class DamageAdapterTestSetup : MonoBehaviour
    {
        private const string LegacyPlayerName = "Example Player";
        private const string DemoCameraName = "Main Camera";

        [SerializeField]
        private bool _logDetectionDiagnostics = true;

        [SerializeField]
        private bool _enableCombatValidation = true;

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
                FindObjectsByType<EmeraldAI.EmeraldHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

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

            if (!_enableCombatValidation)
                return;

            EmeraldCombatLoopValidator validator = player.GetComponent<EmeraldCombatLoopValidator>();
            if (validator == null)
                validator = player.gameObject.AddComponent<EmeraldCombatLoopValidator>();

            validator.BindPlayer(player);
        }
    }
}
