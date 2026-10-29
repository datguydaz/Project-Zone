using System.Reflection;
using PolymindGames;
using PolymindGames.InventorySystem;
using PolymindGames.SaveSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 4 — spawn Settlement stash + Outskirts loot containers outside greybox zones.
    /// Uses HQ ItemContainerGenerator / Interactable / LootTable; does not replace HQ inventory.
    /// </summary>
    [DefaultExecutionOrder(-45)]
    [DisallowMultipleComponent]
    public sealed class OutskirtsInventoryBootstrap : MonoBehaviour
    {
        private const string RootName = "Outskirts_Inventory";
        private const string StashName = "Stash_Settlement_Courtyard";
        private const int ContentVersion = 2; // bump when marker layout / prompt anchors change
        private const string DefaultLootTablePath =
            "Assets/PolymindGames/FPSCore/Data/Inventory/LootTables/FPS_WeaponsAndAmmo.asset";
        private const string LevelDefinitionPath =
            "Assets/_ProjectZone/Data/PZ_Outskirts_LevelDefinition.asset";

        // Courtyard in front of SupplyShed (shed itself is a solid 6x3x5 at 8,1.5,9 — do not spawn inside it).
        private static readonly Vector3 StashWorldPos = new(5.5f, 0.85f, 5.5f);

        [SerializeField]
        private int _builtContentVersion;

        [SerializeField]
        private bool _spawnOnStart = true;

        [SerializeField]
        private bool _ensureSceneSaveHandler = true;

        [SerializeField]
        private bool _ensurePermanentStash = true;

        [SerializeField]
        private bool _ensurePersistenceValidator = true;

        [SerializeField]
        private LootTable _lootTable;

        [SerializeField]
        private LevelDefinition _levelDefinition;

        private void Start()
        {
            ResolveAssets();

            if (_ensureSceneSaveHandler)
                EnsureSceneSaveHandler();

            if (_ensurePermanentStash)
                EnsurePermanentStashService();

            if (_spawnOnStart)
            {
                Transform root = transform.Find(RootName);
                bool incomplete = root == null
                    || root.Find(StashName) == null
                    || root.childCount < 4
                    || _builtContentVersion != ContentVersion;
                EnsureInventoryContent(forceRebuild: incomplete);
            }

            if (_ensurePersistenceValidator && GetComponent<PzInventoryPersistenceValidator>() == null)
                gameObject.AddComponent<PzInventoryPersistenceValidator>();

            if (GetComponent<PzPlayerStartingLoadout>() == null)
                gameObject.AddComponent<PzPlayerStartingLoadout>();
        }

#if UNITY_EDITOR
        private void OnValidate() => ResolveAssets();
#endif

        private void ResolveAssets()
        {
#if UNITY_EDITOR
            if (_lootTable == null)
                _lootTable = UnityEditor.AssetDatabase.LoadAssetAtPath<LootTable>(DefaultLootTablePath);

            if (_levelDefinition == null)
                _levelDefinition = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelDefinitionPath);
#endif
        }

        [ContextMenu("Respawn Outskirts Inventory Content")]
        public void RespawnInventoryContent() => EnsureInventoryContent(forceRebuild: true);

        public void EnsureInventoryContent(bool forceRebuild = true)
        {
            ResolveAssets();

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

            var inventoryRoot = new GameObject(RootName);
            inventoryRoot.transform.SetParent(transform, false);

            CreateStashTerminal(inventoryRoot.transform, StashWorldPos);

            TryCreateLoot(inventoryRoot.transform, "Loot_Checkpoint_WatchCrate", new Vector3(51f, 1f, 7f),
                "Watch Crate", new Color(0.85f, 0.55f, 0.2f), 8, 1, 3);
            TryCreateLoot(inventoryRoot.transform, "Loot_Industrial_CrateRow", new Vector3(88f, 0.9f, 0f),
                "Industrial Crates", new Color(0.55f, 0.7f, 0.85f), 10, 1, 4);
            TryCreateLoot(inventoryRoot.transform, "Loot_Industrial_WarehouseStack", new Vector3(83f, 1.2f, 11f),
                "Warehouse Stack", new Color(0.9f, 0.35f, 0.25f), 12, 2, 4);

            _builtContentVersion = ContentVersion;
            Debug.Log(
                $"[ProjectZone][Inventory] Spawned stash at {StashWorldPos} (courtyard, south of SupplyShed) + loot under {RootName}.",
                this);
        }

        private void EnsurePermanentStashService()
        {
            if (PzPermanentStash.Instance != null)
                return;

            var go = new GameObject("PZ_PermanentStash");
            go.AddComponent<PzPermanentStash>();
        }

        private void EnsureSceneSaveHandler()
        {
            if (SceneSaveHandler.TryGetHandler(gameObject.scene, out _))
                return;

            SceneSaveHandler existing = FindAnyObjectByType<SceneSaveHandler>();
            if (existing != null)
                return;

            var go = new GameObject("PZ_SceneSaveHandler");
            var handler = go.AddComponent<SceneSaveHandler>();
            AssignLevelDefinition(handler, _levelDefinition);
            Debug.Log("[ProjectZone][Inventory] Added SceneSaveHandler for Outskirts persistence (#25/#26).", this);
        }

        private static void AssignLevelDefinition(SceneSaveHandler handler, LevelDefinition definition)
        {
            if (handler == null || definition == null)
                return;

            FieldInfo field = typeof(SceneSaveHandler).GetField(
                "_levelDefinition",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(handler, definition);
        }

        private static void CreateStashTerminal(Transform parent, Vector3 worldPosition)
        {
            GameObject go = CreateInteractableMarker(
                parent,
                StashName,
                worldPosition,
                new Vector3(1.6f, 1.4f, 1.6f),
                new Color(0.15f, 0.9f, 0.45f),
                "STASH");
            var interactable = go.GetComponent<Interactable>();
            interactable.Title = "Settlement Stash";
            interactable.Description = "Permanent storage";
            go.AddComponent<PzStashInteractable>();
        }

        private void TryCreateLoot(
            Transform parent,
            string objectName,
            Vector3 worldPosition,
            string title,
            Color color,
            int slots,
            byte minLoot,
            byte maxLoot)
        {
            try
            {
                CreateLoot(parent, objectName, worldPosition, title, color, slots, minLoot, maxLoot);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectZone][Inventory] Failed to create loot '{objectName}': {ex}", this);
            }
        }

        private void CreateLoot(
            Transform parent,
            string objectName,
            Vector3 worldPosition,
            string title,
            Color color,
            int slots,
            byte minLoot,
            byte maxLoot)
        {
            GameObject go = CreateInteractableMarker(
                parent,
                objectName,
                worldPosition,
                new Vector3(1.3f, 1.1f, 1.3f),
                color,
                "LOOT");

            var host = go.AddComponent<PzContainerHost>();
            var generator = new ItemContainerGenerator
            {
                Name = title,
                MaxSlotCount = slots,
                AllowStacking = true,
                Restrictions = System.Array.Empty<ContainerRestriction>(),
                PredefinedItems = System.Array.Empty<ItemGenerator>(),
                LootTable = _lootTable,
                MinLootCount = minLoot,
                MaxLootCount = maxLoot,
                RarityWeight = 1f
            };
            host.Configure(generator, populateOnCreate: _lootTable != null);

            var interactable = go.GetComponent<Interactable>();
            interactable.Title = title;
            interactable.Description = "Search container";

            go.AddComponent<PzWorldLootContainer>();
            go.AddComponent<SaveableObject>();
        }

        /// <summary>Shared greybox interactable marker for inventory / death / contracts terminals.</summary>
        public static GameObject CreateInteractableMarker(
            Transform parent,
            string objectName,
            Vector3 worldPosition,
            Vector3 scale,
            Color color,
            string beaconLabel = null)
        {
            // Root stays unscaled so HQ prompt CenterOffset maps cleanly to world (scaled
            // primitives skew Interactable.Awake bounds → prompt floats away from the object).
            var root = new GameObject(objectName);
            root.transform.SetParent(parent, false);
            root.transform.position = worldPosition;

            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer >= 0)
                root.layer = interactableLayer;

            var anchorCollider = root.AddComponent<BoxCollider>();
            anchorCollider.size = scale;
            anchorCollider.center = Vector3.zero;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = scale;
            ApplyUrpColor(visual, color);
            Object.Destroy(visual.GetComponent<Collider>());

            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "Beacon";
            beacon.transform.SetParent(root.transform, false);
            beacon.transform.localPosition = new Vector3(0f, scale.y * 0.5f + 0.85f, 0f);
            beacon.transform.localScale = new Vector3(0.2f, 0.85f, 0.2f);
            ApplyUrpColor(beacon, Color.Lerp(color, Color.white, 0.35f));
            Object.Destroy(beacon.GetComponent<Collider>());

            var interactable = root.AddComponent<Interactable>();
            interactable.Title = objectName;
            // Pin prompt just above the cube top (HQ UI pivot sits above this point).
            ForceInteractableCenterOffset(interactable, new Vector3(0f, scale.y * 0.5f + 0.15f, 0f));
            return root;
        }

        private static void ForceInteractableCenterOffset(Interactable interactable, Vector3 localOffset)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(Interactable).GetField("_centerOffset", flags)?.SetValue(interactable, localOffset);
            typeof(Interactable).GetField("_adjustedCenterOffset", flags)?.SetValue(interactable, localOffset);
        }

        private static void ApplyUrpColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null)
                return;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
                return;

            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
            renderer.sharedMaterial = mat;
        }
    }
}
