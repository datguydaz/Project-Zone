using System;
using PolymindGames.InventorySystem;
using PolymindGames.SaveSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Hosts a single HQ <see cref="ItemContainer"/> for world loot or stash without replacing player Inventory.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzContainerHost : MonoBehaviour, ISaveableComponent
    {
        [SerializeField]
        private ItemContainerGenerator _generator = new ItemContainerGenerator
        {
            Name = "Container",
            MaxSlotCount = 12,
            AllowStacking = true,
            Restrictions = System.Array.Empty<ContainerRestriction>(),
            PredefinedItems = System.Array.Empty<ItemGenerator>(),
            MinLootCount = 1,
            MaxLootCount = 3
        };

        [SerializeField]
        private bool _populateOnCreate = true;

        private DefaultInventory _defaultInventory;
        private ItemContainer _container;
        private bool _loadedFromSave;

        public string ContainerName => _generator != null ? _generator.Name : name;
        public IItemContainer Container => _container;
        public ItemContainerGenerator Generator => _generator;

        public void Configure(ItemContainerGenerator generator, bool populateOnCreate)
        {
            if (generator != null)
                _generator = generator;
            _populateOnCreate = populateOnCreate;

            // Allow reconfigure before first open (AddComponent Awake may have run already).
            if (_container == null)
                EnsureCreated();
        }

        public void EnsureCreated()
        {
            if (_container != null)
                return;

            if (_generator == null)
            {
                _generator = new ItemContainerGenerator
                {
                    Name = "Container",
                    MaxSlotCount = 12,
                    AllowStacking = true,
                    Restrictions = System.Array.Empty<ContainerRestriction>(),
                    PredefinedItems = System.Array.Empty<ItemGenerator>(),
                    MinLootCount = 0,
                    MaxLootCount = 1
                };
            }

            PzItemContainerFactory.Normalize(_generator);

            _defaultInventory ??= new DefaultInventory(gameObject);
            bool populate = _populateOnCreate && !_loadedFromSave;
            _container = PzItemContainerFactory.Create(_defaultInventory, _generator, populate);
        }

        public void ClearAndRepopulate()
        {
            _defaultInventory ??= new DefaultInventory(gameObject);
            _container = PzItemContainerFactory.Create(_defaultInventory, _generator, populateWithItems: true);
            _loadedFromSave = false;
        }

        #region Save & Load
        [Serializable]
        private sealed class SaveData
        {
            public ItemContainer Container;
            public string Name;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            if (data is not SaveData saveData || saveData.Container == null)
                return;

            _defaultInventory ??= new DefaultInventory(gameObject);
            _container = saveData.Container;
            _container.InitializeAfterDeserialization(
                _defaultInventory,
                PzItemContainerFactory.RestrictionsOrEmpty(_generator));
            _loadedFromSave = true;
        }

        object ISaveableComponent.SaveMembers()
        {
            EnsureCreated();
            return new SaveData
            {
                Container = _container,
                Name = ContainerName
            };
        }
        #endregion
    }
}
