using System;
using System.IO;
using PolymindGames;
using PolymindGames.InventorySystem;
using PolymindGames.SaveSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 4 / #23 — permanent Settlement stash outside the carried backpack.
    /// Persists via a Project Zone profile file (HQ has no built-in stash).
    /// </summary>
    [DefaultExecutionOrder(-60)]
    [DisallowMultipleComponent]
    public sealed class PzPermanentStash : MonoBehaviour
    {
        private const string FileName = "pz_permanent_stash.sav";

        [SerializeField]
        private ItemContainerGenerator _generator = new ItemContainerGenerator
        {
            Name = "Stash",
            MaxSlotCount = 40,
            AllowStacking = true,
            Restrictions = System.Array.Empty<ContainerRestriction>(),
            PredefinedItems = System.Array.Empty<ItemGenerator>(),
            MinLootCount = 0,
            MaxLootCount = 0
        };

        [SerializeField]
        private bool _loadOnAwake = true;

        [SerializeField]
        private bool _saveOnDestroy = true;

        private static PzPermanentStash _instance;
        private DefaultInventory _defaultInventory;
        private ItemContainer _container;

        public static PzPermanentStash Instance => _instance;
        public IItemContainer Container
        {
            get
            {
                EnsureCreated();
                return _container;
            }
        }

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
            if (_instance != this)
                return;

            if (_saveOnDestroy && Application.isPlaying && _container != null)
            {
                try
                {
                    SaveToDisk();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ProjectZone][Inventory] Stash save on destroy failed: {ex.Message}", this);
                }
            }

            _instance = null;
        }

        public void EnsureCreated()
        {
            if (_container != null)
                return;

            _defaultInventory ??= new DefaultInventory(gameObject);
            _container = PzItemContainerFactory.Create(_defaultInventory, _generator, populateWithItems: false);
        }

        public void OpenFor(ICharacter character)
        {
            if (character == null)
                return;

            EnsureCreated();
            PzInventoryPanelController.Open(character, Container, "Settlement Stash");
        }

        public void SaveToDisk()
        {
            EnsureCreated();
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var data = new StashSaveData
            {
                Container = _container,
                SavedUtc = DateTime.UtcNow.ToString("o")
            };

            SaveLoadManager.SaveToFile(data, SavePath);
            Debug.Log($"[ProjectZone][Inventory] Stash saved → {SavePath}");
        }

        public bool LoadFromDisk()
        {
            EnsureCreated();

            if (!File.Exists(SavePath))
                return false;

            var data = SaveLoadManager.LoadFromFile<StashSaveData>(SavePath);
            if (data?.Container == null)
                return false;

            _container = data.Container;
            _container.InitializeAfterDeserialization(
                _defaultInventory,
                PzItemContainerFactory.RestrictionsOrEmpty(_generator));
            Debug.Log($"[ProjectZone][Inventory] Stash loaded ← {SavePath}");
            return true;
        }

        [Serializable]
        private sealed class StashSaveData
        {
            public ItemContainer Container;
            public string SavedUtc;
        }

#if UNITY_EDITOR
        [ContextMenu("Save Stash Now")]
        private void ContextSave() => SaveToDisk();

        [ContextMenu("Load Stash Now")]
        private void ContextLoad() => LoadFromDisk();

        [ContextMenu("Clear Stash File")]
        private void ContextClearFile()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
#endif
    }
}
