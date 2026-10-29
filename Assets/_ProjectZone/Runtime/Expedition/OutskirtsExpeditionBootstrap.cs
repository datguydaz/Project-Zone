using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 5 — spawn Settlement contract board + death-cache claim terminal;
    /// ensure wallet / death / contract / loop services for PZ_Outskirts.
    /// </summary>
    [DefaultExecutionOrder(-44)]
    [DisallowMultipleComponent]
    public sealed class OutskirtsExpeditionBootstrap : MonoBehaviour
    {
        private const string RootName = "Outskirts_Expedition";
        private const string ClaimTerminalName = "DeathCache_Claim_WatchPost";
        private const string BoardName = "ContractBoard_Courtyard";
        private const int ContentVersion = 1;

        private static readonly Vector3 ClaimTerminalPos = new(10f, 5.8f, -10f);
        private static readonly Vector3 ContractBoardPos = new(-2f, 2.2f, 6.5f);

        [SerializeField]
        private bool _spawnOnStart = true;

        [SerializeField]
        private bool _ensureServices = true;

        [SerializeField]
        private bool _ensureValidator = true;

        [SerializeField]
        private int _builtContentVersion;

        private void Start()
        {
            if (_ensureServices)
                EnsureServices();

            if (_spawnOnStart)
            {
                Transform root = transform.Find(RootName);
                bool incomplete = root == null
                    || root.Find(ClaimTerminalName) == null
                    || root.Find(BoardName) == null
                    || _builtContentVersion != ContentVersion;
                EnsureExpeditionContent(forceRebuild: incomplete);
            }

            if (_ensureValidator && GetComponent<PzExpeditionLoopValidator>() == null)
                gameObject.AddComponent<PzExpeditionLoopValidator>();

            if (GetComponent<PzSafeZoneTracker>() == null)
                gameObject.AddComponent<PzSafeZoneTracker>();

            if (GetComponent<PzHybridDeathController>() == null)
                gameObject.AddComponent<PzHybridDeathController>();
        }

        [ContextMenu("Respawn Outskirts Expedition Content")]
        public void RespawnExpeditionContent() => EnsureExpeditionContent(forceRebuild: true);

        public void EnsureExpeditionContent(bool forceRebuild = true)
        {
            Transform root = transform.Find(RootName);
            if (root != null)
            {
                if (!forceRebuild)
                    return;

                root.name = RootName + "_OLD";
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(root.gameObject);
                else
#endif
                    Destroy(root.gameObject);
            }

            var expeditionRoot = new GameObject(RootName);
            expeditionRoot.transform.SetParent(transform, false);

            CreateClaimTerminal(expeditionRoot.transform);
            CreateContractBoard(expeditionRoot.transform);

            _builtContentVersion = ContentVersion;
            Debug.Log(
                "[ProjectZone][Expedition] Spawned Contract Board (courtyard) + Death Cache claim (WatchPost deck).",
                this);
        }

        private static void EnsureServices()
        {
            if (PzWallet.Instance == null)
                new GameObject("PZ_Wallet").AddComponent<PzWallet>();

            if (PzDeathCacheService.Instance == null)
                new GameObject("PZ_DeathCache").AddComponent<PzDeathCacheService>();

            if (PzContractBoardService.Instance == null)
                new GameObject("PZ_ContractBoard").AddComponent<PzContractBoardService>();

            if (PzExpeditionLoop.Instance == null)
                new GameObject("PZ_ExpeditionLoop").AddComponent<PzExpeditionLoop>();
        }

        private static void CreateClaimTerminal(Transform parent)
        {
            GameObject go = OutskirtsInventoryBootstrap.CreateInteractableMarker(
                parent,
                ClaimTerminalName,
                ClaimTerminalPos,
                new Vector3(1.4f, 1.2f, 1.4f),
                new Color(0.95f, 0.55f, 0.15f),
                "CLAIM");
            var interactable = go.GetComponent<Interactable>();
            interactable.Title = "Lost & Found";
            interactable.Description = "Claim recoverable death cache";
            go.AddComponent<PzDeathCacheInteractable>();
        }

        private static void CreateContractBoard(Transform parent)
        {
            GameObject go = OutskirtsInventoryBootstrap.CreateInteractableMarker(
                parent,
                BoardName,
                ContractBoardPos,
                new Vector3(2.2f, 1.8f, 0.35f),
                new Color(0.2f, 0.45f, 0.95f),
                "JOBS");
            var interactable = go.GetComponent<Interactable>();
            interactable.Title = "Contract Board";
            interactable.Description = "Accept / turn in contracts";
            go.AddComponent<PzContractBoardInteractable>();
        }
    }
}
