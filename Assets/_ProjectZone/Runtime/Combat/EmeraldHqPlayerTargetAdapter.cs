using EmeraldAI;
using EmeraldAI.Utility;
using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Registers an HQ FPS player so Emerald AI can detect, pursue, and combat them.
    /// Does not replace Emerald or HQ systems — only wires required Emerald target components.
    /// Linked to GitHub issue #10 (Enemy detection and pursuit).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EmeraldHqPlayerTargetAdapter : MonoBehaviour
    {
        private const string PlayerFactionName = "Player";

        [SerializeField]
        private bool _logDiagnostics;

        private Player _player;
        private bool _registered;

        public bool IsRegistered => _registered;

        public void Configure(Player player, bool logDiagnostics = false)
        {
            _player = player;
            _logDiagnostics = logDiagnostics;
            RegisterPlayer();
            BootstrapNearbyAi();
        }

        private void RegisterPlayer()
        {
            if (_player == null)
                _player = GetComponent<Player>();

            if (_player == null)
            {
                Debug.LogError("[ProjectZone] EmeraldHqPlayerTargetAdapter requires an HQ Player.", this);
                return;
            }

            GameObject playerObject = _player.gameObject;
            playerObject.layer = LayerConstants.Character;

            // Emerald detection requires IFaction + a collider on the same transform as the faction component.
            EnsureDetectionCollider(playerObject);

            Transform torso = _player.GetTransformOfBodyPoint(BodyPoint.Torso);
            Transform transformSource = torso != null ? torso : playerObject.transform;

            // TargetPositionModifier.OnEnable errors if TransformSource is null. Add/configure while inactive
            // so OnEnable runs only after TransformSource is assigned.
            bool wasActive = playerObject.activeSelf;
            playerObject.SetActive(false);

            TargetPositionModifier modifier = playerObject.GetComponent<TargetPositionModifier>();
            if (modifier == null)
                modifier = playerObject.AddComponent<TargetPositionModifier>();

            modifier.TransformSource = transformSource;
            modifier.PositionModifier = 0.5f;

            FactionExtension faction = playerObject.GetComponent<FactionExtension>();
            if (faction == null)
                faction = playerObject.AddComponent<FactionExtension>();

            faction.CurrentFaction = ResolvePlayerFactionIndex();

            if (playerObject.GetComponent<HqPlayerDamageBridge>() == null)
                playerObject.AddComponent<HqPlayerDamageBridge>();

            playerObject.SetActive(wasActive);

            _registered = true;

            if (_logDiagnostics)
            {
                Debug.Log(
                    $"[ProjectZone] HQ player registered for Emerald targeting. " +
                    $"tag={playerObject.tag}, layer={playerObject.layer}, " +
                    $"factionIndex={faction.CurrentFaction}, " +
                    $"transformSource={modifier.TransformSource.name}, " +
                    $"collider={playerObject.GetComponent<Collider>() != null}",
                    playerObject);
            }
        }

        private void BootstrapNearbyAi()
        {
            int characterLayer = 1 << LayerConstants.Character;
            EmeraldSystem[] systems = FindObjectsByType<EmeraldSystem>(FindObjectsInactive.Exclude);

            for (int i = 0; i < systems.Length; i++)
            {
                EmeraldSystem system = systems[i];
                EmeraldDetection detection = system.DetectionComponent;
                if (detection == null)
                    continue;

                detection.DetectionLayerMask |= characterLayer;
                EnsurePlayerIsEnemy(system, detection);

                if (_logDiagnostics)
                {
                    string relation = EmeraldAPI.Faction.GetTargetFactionRelation(system, _player.transform);
                    Debug.Log(
                        $"[ProjectZone] AI '{system.name}' player relation={relation}, " +
                        $"detectionMask includes Character={((detection.DetectionLayerMask & characterLayer) != 0)}",
                        system);
                }
            }
        }

        private static void EnsurePlayerIsEnemy(EmeraldSystem system, EmeraldDetection detection)
        {
            EmeraldFactionData factionData = Resources.Load<EmeraldFactionData>("Faction Data");
            if (factionData == null || factionData.FactionNameList == null)
                return;

            int playerFactionIndex = factionData.FactionNameList.IndexOf(PlayerFactionName);
            if (playerFactionIndex < 0)
                return;

            bool found = false;
            for (int i = 0; i < detection.FactionRelationsList.Count; i++)
            {
                if (detection.FactionRelationsList[i].FactionIndex != playerFactionIndex)
                    continue;

                found = true;
                if (detection.FactionRelationsList[i].RelationType != RelationTypes.Enemy)
                    EmeraldAPI.Faction.SetFactionLevel(system, PlayerFactionName, RelationTypes.Enemy);
                break;
            }

            if (!found)
            {
                EmeraldAPI.Faction.AddFactionRelation(system, PlayerFactionName, RelationTypes.Enemy);
                // Emerald AddFactionRelation does not rebuild runtime lists; SetupFactions also fails to clear AIFactionsList.
                detection.AIFactionsList.Clear();
                detection.SetupFactions();
            }
            else if (!detection.AIFactionsList.Contains(playerFactionIndex))
            {
                detection.AIFactionsList.Clear();
                detection.SetupFactions();
            }
        }

        private static void EnsureDetectionCollider(GameObject playerObject)
        {
            if (playerObject.GetComponent<Collider>() != null)
                return;

            var capsule = playerObject.AddComponent<CapsuleCollider>();
            capsule.height = 1.8f;
            capsule.radius = 0.35f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.isTrigger = false;
        }

        private static int ResolvePlayerFactionIndex()
        {
            EmeraldFactionData factionData = Resources.Load<EmeraldFactionData>("Faction Data");
            if (factionData == null || factionData.FactionNameList == null)
                return 2;

            int index = factionData.FactionNameList.IndexOf(PlayerFactionName);
            return index >= 0 ? index : 2;
        }
    }
}
