using System;
using System.IO;
using PolymindGames;
using PolymindGames.InventorySystem;
using PolymindGames.SaveSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 5 / #28 — recoverable death cache. Backpack spilled on expedition death
    /// lives here until claimed (world marker and/or Settlement claim terminal).
    /// </summary>
    [DefaultExecutionOrder(-54)]
    [DisallowMultipleComponent]
    public sealed class PzDeathCacheService : MonoBehaviour
    {
        private const string FileName = "pz_death_cache.sav";
        private const string BackpackName = "Backpack";

        [SerializeField]
        private ItemContainerGenerator _generator = new ItemContainerGenerator
        {
            Name = "Death Cache",
            MaxSlotCount = 40,
            AllowStacking = true,
            Restrictions = Array.Empty<ContainerRestriction>(),
            PredefinedItems = Array.Empty<ItemGenerator>(),
            MinLootCount = 0,
            MaxLootCount = 0
        };

        [SerializeField]
        private bool _loadOnAwake = true;

        private static PzDeathCacheService _instance;
        private DefaultInventory _defaultInventory;
        private ItemContainer _container;
        private Vector3 _deathWorldPosition;
        private bool _hasPendingCache;
        private GameObject _worldMarker;

        public static PzDeathCacheService Instance => _instance;
        public IItemContainer Container
        {
            get
            {
                EnsureCreated();
                return _container;
            }
        }

        public bool HasPendingCache => _hasPendingCache && HasAnyItems();
        public Vector3 DeathWorldPosition => _deathWorldPosition;

        public static string SavePath =>
            Path.Combine(Application.persistentDataPath, "ProjectZone", FileName);

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureCreated();

            if (_loadOnAwake)
                LoadFromDisk();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public void EnsureCreated()
        {
            if (_container != null)
                return;

            _defaultInventory ??= new DefaultInventory(gameObject);
            PzItemContainerFactory.Normalize(_generator);
            _container = PzItemContainerFactory.Create(_defaultInventory, _generator, populateWithItems: false);
        }

        /// <summary>
        /// Move all backpack stacks into the death cache and clear the backpack.
        /// </summary>
        public int SpillBackpackFrom(Player player, Vector3 deathPosition)
        {
            EnsureCreated();
            if (player?.Inventory == null)
                return 0;

            IItemContainer backpack = player.Inventory.FindContainer(ItemContainerFilters.WithName(BackpackName));
            if (backpack == null)
                return 0;

            int moved = 0;
            for (int i = 0; i < backpack.SlotsCount; i++)
            {
                SlotReference slot = backpack.GetSlot(i);
                if (!slot.HasItem())
                    continue;

                ItemStack stack = slot.GetStack();
                (int added, _) = _container.AddItem(stack);
                if (added > 0)
                {
                    slot.AdjustStack(-added);
                    moved += added;
                }
            }

            _deathWorldPosition = deathPosition;
            _hasPendingCache = moved > 0 || HasAnyItems();
            SaveToDisk();
            SpawnOrMoveWorldMarker(deathPosition);

            Debug.Log(
                $"[ProjectZone][DeathCache] Spilled {moved} item units at {deathPosition}. Pending={_hasPendingCache}",
                this);
            return moved;
        }

        public void OpenFor(ICharacter character)
        {
            if (character == null)
                return;

            EnsureCreated();
            PzInventoryPanelController.Open(character, Container, "Death Cache");
            // After panel close we can't hook easily — save whenever slots may have changed.
            SaveToDisk();
            if (!HasAnyItems())
            {
                _hasPendingCache = false;
                DestroyWorldMarker();
                SaveToDisk();
            }
        }

        public void NotifyOpened()
        {
            SaveToDisk();
            if (!HasAnyItems())
            {
                _hasPendingCache = false;
                DestroyWorldMarker();
                SaveToDisk();
            }
        }

        public void SpawnOrMoveWorldMarker(Vector3 worldPosition)
        {
            if (!HasPendingCache)
                return;

            if (_worldMarker == null)
            {
                _worldMarker = OutskirtsInventoryBootstrap.CreateInteractableMarker(
                    null,
                    "DeathCache_WorldMarker",
                    worldPosition,
                    new Vector3(1.2f, 1f, 1.2f),
                    new Color(0.95f, 0.45f, 0.1f),
                    "CACHE");
                DontDestroyOnLoad(_worldMarker);
                var interactable = _worldMarker.GetComponent<Interactable>();
                interactable.Title = "Death Cache";
                interactable.Description = "Recover lost gear";
                _worldMarker.AddComponent<PzDeathCacheInteractable>();
            }
            else
            {
                _worldMarker.transform.position = worldPosition;
                _worldMarker.SetActive(true);
            }
        }

        public void DestroyWorldMarker()
        {
            if (_worldMarker == null)
                return;

            Destroy(_worldMarker);
            _worldMarker = null;
        }

        public void SaveToDisk()
        {
            EnsureCreated();
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            SaveLoadManager.SaveToFile(new DeathCacheSaveData
            {
                Container = _container,
                DeathPosition = _deathWorldPosition,
                HasPending = _hasPendingCache,
                SavedUtc = DateTime.UtcNow.ToString("o")
            }, SavePath);
        }

        public bool LoadFromDisk()
        {
            EnsureCreated();
            if (!File.Exists(SavePath))
                return false;

            var data = SaveLoadManager.LoadFromFile<DeathCacheSaveData>(SavePath);
            if (data?.Container == null)
                return false;

            _container = data.Container;
            _container.InitializeAfterDeserialization(
                _defaultInventory,
                PzItemContainerFactory.RestrictionsOrEmpty(_generator));
            _deathWorldPosition = data.DeathPosition;
            _hasPendingCache = data.HasPending && HasAnyItems();

            if (_hasPendingCache)
                SpawnOrMoveWorldMarker(_deathWorldPosition);

            return true;
        }

        private bool HasAnyItems()
        {
            EnsureCreated();
            for (int i = 0; i < _container.SlotsCount; i++)
            {
                if (_container.GetSlot(i).HasItem())
                    return true;
            }

            return false;
        }

        [Serializable]
        private sealed class DeathCacheSaveData
        {
            public ItemContainer Container;
            public Vector3 DeathPosition;
            public bool HasPending;
            public string SavedUtc;
        }
    }
}
