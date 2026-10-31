using System.Collections;
using PolymindGames;
using PolymindGames.InventorySystem;
using PolymindGames.WieldableSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Ensures the local player always starts with HQFPS_M1911 (from HQFPS_Pickup_M1911)
    /// in the Holster, without editing vendor player prefabs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzPlayerStartingLoadout : MonoBehaviour
    {
        // HQFPS_M1911.asset Name after prefix strip = "M1911"; Id from asset.
        private const string DefaultItemName = "M1911";
        private const int DefaultItemId = -6247473;
        private const string DefaultAmmoName = "45 ACP";
        private const string HolsterName = "Holster";
        private const string BackpackName = "Backpack";
        private const string DefaultPickupPath =
            "Assets/01_System/PolymindGames/HQFPS/Prefabs/ItemPickups/HQFPS_Pickup_M1911.prefab";

        [SerializeField]
        private ItemDefinition _m1911Item;

        [SerializeField]
        private ItemDefinition _ammoItem;

        [SerializeField]
        private ItemPickup _m1911Pickup;

        [SerializeField]
        private int _ammoCount = 48;

        [SerializeField]
        private bool _selectHolsterSlot = true;

        [SerializeField]
        private bool _grantOnStart = true;

        private void Start()
        {
            ApplyProjectSettings();
            if (_grantOnStart)
                StartCoroutine(GrantWhenPlayerReady());
        }

#if UNITY_EDITOR
        private void OnValidate() => ResolveAssets();
#endif

        private void ApplyProjectSettings()
        {
            PzProjectSettings.PlayerSettings player = PzProjectSettings.Get()?.Player;
            if (player == null)
                return;

            _grantOnStart = player.GiveStartingPistol;

            if (_m1911Pickup == null && player.StartingWeaponPickupPrefab != null)
                _m1911Pickup = player.StartingWeaponPickupPrefab.GetComponent<ItemPickup>();
        }

        private void ResolveAssets()
        {
            ApplyProjectSettings();

#if UNITY_EDITOR
            if (_m1911Pickup == null)
            {
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPickupPath);
                if (prefab != null)
                    _m1911Pickup = prefab.GetComponent<ItemPickup>();
            }
#endif
            if (_m1911Item == null)
            {
                _m1911Item = ItemDefinition.GetWithId(DefaultItemId)
                    ?? ItemDefinition.GetWithName(DefaultItemName);
            }

            if (_ammoItem == null)
                _ammoItem = ItemDefinition.GetWithName(DefaultAmmoName);
        }

        private IEnumerator GrantWhenPlayerReady()
        {
            const float timeout = 8f;
            float elapsed = 0f;
            while (Player.AllPlayers.Count == 0 && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (Player.AllPlayers.Count == 0)
            {
                Debug.LogWarning("[ProjectZone][Loadout] No player found — cannot grant M1911.", this);
                yield break;
            }

            // Wait one frame so Inventory / WieldableInventory finish BehaviourStart.
            yield return null;
            ResolveAssets();
            GrantTo(Player.AllPlayers[0]);
        }

        public void GrantTo(Player player)
        {
            if (player?.Inventory == null)
                return;

            ItemDefinition gun = ResolveM1911Definition();
            if (gun == null)
            {
                Debug.LogWarning(
                    "[ProjectZone][Loadout] HQFPS_M1911 item definition missing — cannot grant starting pistol.",
                    this);
                return;
            }

            IItemContainer holster = player.Inventory.FindContainer(ItemContainerFilters.WithName(HolsterName));
            if (holster == null)
            {
                Debug.LogWarning("[ProjectZone][Loadout] Player Holster container missing.", this);
                return;
            }

            if (holster.GetItemCountById(gun.Id) <= 0)
            {
                var stack = new ItemStack(new Item(gun), 1);
                (int added, string reject) = holster.AddItem(stack);
                if (added <= 0)
                {
                    Debug.LogWarning($"[ProjectZone][Loadout] Could not add M1911 to Holster: {reject}", this);
                    return;
                }
            }

            if (_ammoItem != null && _ammoCount > 0)
            {
                IItemContainer backpack = player.Inventory.FindContainer(ItemContainerFilters.WithName(BackpackName));
                backpack?.AddItem(new ItemStack(new Item(_ammoItem), _ammoCount));
            }

            if (_selectHolsterSlot)
            {
                var wieldables = player.GetComponentInChildren<WieldableInventory>(true);
                wieldables?.SelectAtIndex(0);
            }

            Debug.Log("[ProjectZone][Loadout] Starting loadout: HQFPS_M1911 in Holster.", this);
        }

        private ItemDefinition ResolveM1911Definition()
        {
            if (_m1911Item != null)
                return _m1911Item;

            return ItemDefinition.GetWithId(DefaultItemId)
                ?? ItemDefinition.GetWithName(DefaultItemName);
        }
    }
}
