using System.Collections;
using PolymindGames;
using PolymindGames.InputSystem.Behaviours;
using PolymindGames.WieldableSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Always use clothed FP arms (shirt + gloves) and block the demo arm-cycle input.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzForceClothedArms : MonoBehaviour
    {
        private const string DefaultSetName = "Shirt & Gloves";

        [SerializeField]
        private string _armSetName = DefaultSetName;

        [SerializeField]
        private bool _disableArmCycleInput = true;

        [SerializeField]
        private bool _applyOnStart = true;

        private void Start()
        {
            if (_applyOnStart)
                StartCoroutine(ApplyWhenReady());
        }

        private IEnumerator ApplyWhenReady()
        {
            const float timeout = 8f;
            float elapsed = 0f;
            while (Player.AllPlayers.Count == 0 && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (Player.AllPlayers.Count == 0)
                yield break;

            // Arms handler finishes Awake/Enable after player spawn.
            yield return null;
            yield return null;
            ApplyTo(Player.AllPlayers[0]);
        }

        public void ApplyTo(Player player)
        {
            if (player == null)
                return;

            WieldableArmsHandler arms = player.GetComponentInChildren<WieldableArmsHandler>(true);
            if (arms == null)
            {
                Debug.LogWarning("[ProjectZone][Arms] WieldableArmsHandler not found on player.", this);
                return;
            }

            if (!arms.SelectArmSetByName(_armSetName))
            {
                // Fallback: Shirt & Gloves is index 2 in HQFPS_Wieldable_Arms.
                if (arms.ArmSetCount > 2)
                    arms.SelectArmSet(2);
                else
                    Debug.LogWarning($"[ProjectZone][Arms] Arm set '{_armSetName}' not found.", arms);
            }
            else
            {
                Debug.Log($"[ProjectZone][Arms] Forced arm set: {_armSetName}", arms);
            }

            if (_disableArmCycleInput)
            {
                FPSArmsChangeInput cycle = arms.GetComponent<FPSArmsChangeInput>();
                if (cycle != null)
                    cycle.enabled = false;
            }
        }
    }
}
