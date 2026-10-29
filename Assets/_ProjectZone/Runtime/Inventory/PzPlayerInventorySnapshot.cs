using System;
using System.IO;
using PolymindGames;
using PolymindGames.InventorySystem;
using PolymindGames.SaveSystem;
using PolymindGames.WieldableSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 4 / #25–#26 — profile snapshot for player backpack/holster when validating
    /// persistence without requiring the full HQ pause-menu save flow.
    /// HQ scene save still remains the primary path once SceneSaveHandler is present.
    /// </summary>
    public static class PzPlayerInventorySnapshot
    {
        private const string FileName = "pz_player_inventory.sav";
        private const string BackpackName = "Backpack";
        private const string HolsterName = "Holster";

        public static string SavePath =>
            Path.Combine(Application.persistentDataPath, "ProjectZone", FileName);

        public static bool Save(Player player)
        {
            if (player?.Inventory is not Inventory inventory)
            {
                Debug.LogWarning("[ProjectZone][Inventory] Cannot snapshot — player Inventory missing.");
                return false;
            }

            IItemContainer backpack = inventory.FindContainer(ItemContainerFilters.WithName(BackpackName));
            IItemContainer holster = inventory.FindContainer(ItemContainerFilters.WithName(HolsterName));
            var wieldables = player.GetComponentInChildren<WieldableInventory>(true);

            var data = new SnapshotData
            {
                Backpack = backpack as ItemContainer,
                Holster = holster as ItemContainer,
                SelectedHolsterIndex = wieldables != null ? wieldables.SelectedIndex : -1,
                SavedUtc = DateTime.UtcNow.ToString("o")
            };

            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            SaveLoadManager.SaveToFile(data, SavePath);
            Debug.Log($"[ProjectZone][Inventory] Player inventory snapshot → {SavePath}");
            return true;
        }

        public static bool Load(Player player)
        {
            if (player?.Inventory is not Inventory inventory)
                return false;

            if (!File.Exists(SavePath))
            {
                Debug.LogWarning($"[ProjectZone][Inventory] No snapshot at {SavePath}");
                return false;
            }

            var data = SaveLoadManager.LoadFromFile<SnapshotData>(SavePath);
            if (data == null)
                return false;

            ApplyContainer(inventory, BackpackName, data.Backpack);
            ApplyContainer(inventory, HolsterName, data.Holster);

            var wieldables = player.GetComponentInChildren<WieldableInventory>(true);
            if (wieldables != null && data.SelectedHolsterIndex >= 0)
                wieldables.SelectAtIndex(data.SelectedHolsterIndex);

            Debug.Log($"[ProjectZone][Inventory] Player inventory snapshot ← {SavePath}");
            return true;
        }

        private static void ApplyContainer(Inventory inventory, string name, ItemContainer saved)
        {
            if (saved == null)
                return;

            // Inventory owns containers; swap stacks into the live container rather than replacing the array.
            IItemContainer live = inventory.FindContainer(ItemContainerFilters.WithName(name));
            if (live == null)
                return;

            int count = Mathf.Min(live.SlotsCount, saved.SlotsCount);
            for (int i = 0; i < live.SlotsCount; i++)
            {
                SlotReference liveSlot = live.GetSlot(i);
                if (i < count)
                    liveSlot.SetItem(saved.GetSlot(i).GetStack());
                else
                    liveSlot.SetItem(ItemStack.Null);
            }
        }

        [Serializable]
        private sealed class SnapshotData
        {
            public ItemContainer Backpack;
            public ItemContainer Holster;
            public int SelectedHolsterIndex;
            public string SavedUtc;
        }
    }
}
