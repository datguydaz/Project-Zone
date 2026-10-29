using System.Text;
using PolymindGames;
using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZone
{
    /// <summary>
    /// Play Mode checklist for Milestone 4 / issues #25–#26.
    /// Hotkeys (Input System): F5 snapshot, F9 restore, F6 save stash, F7 HQ LevelManager save slot 0.
    /// Backpack UI: HQ Item Wheel on middle mouse — loot/stash open the dual transfer panel.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzInventoryPersistenceValidator : MonoBehaviour
    {
        [SerializeField]
        private bool _logChecklist = true;

        [SerializeField]
        private float _statusIntervalSeconds = 12f;

        private float _statusTimer;
        private bool _sawPlayerInventory;
        private bool _sawBackpackContainer;
        private bool _sawStash;
        private bool _sawLoot;
        private bool _snapshotSaved;
        private bool _snapshotLoaded;
        private bool _stashSaved;
        private bool _transferPanelOpened;

        private void Update()
        {
            ObserveScene();

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.f5Key.wasPressedThisFrame)
            {
                Player player = GetPlayer();
                _snapshotSaved = PzPlayerInventorySnapshot.Save(player);
            }

            if (keyboard.f9Key.wasPressedThisFrame)
            {
                Player player = GetPlayer();
                _snapshotLoaded = PzPlayerInventorySnapshot.Load(player);
            }

            if (keyboard.f6Key.wasPressedThisFrame && PzPermanentStash.Instance != null)
            {
                PzPermanentStash.Instance.SaveToDisk();
                _stashSaved = true;
            }

            if (keyboard.f7Key.wasPressedThisFrame)
                TryHqSave();

            _statusTimer += Time.unscaledDeltaTime;
            if (_logChecklist && _statusTimer >= _statusIntervalSeconds)
            {
                _statusTimer = 0f;
                LogStatus("Checklist pulse");
            }
        }

        private void ObserveScene()
        {
            Player player = GetPlayer();
            if (player?.Inventory != null)
            {
                _sawPlayerInventory = true;
                if (player.Inventory.FindContainer(ItemContainerFilters.WithName("Backpack")) != null)
                    _sawBackpackContainer = true;
            }

            if (PzPermanentStash.Instance != null)
                _sawStash = true;

            if (FindAnyObjectByType<PzWorldLootContainer>() != null)
                _sawLoot = true;

            if (PzInventoryPanelController.IsOpen)
                _transferPanelOpened = true;
        }

        private static Player GetPlayer()
            => Player.AllPlayers.Count > 0 ? Player.AllPlayers[0] : null;

        private static void TryHqSave()
        {
            // Fully qualify — ALP Rustic Environment also defines a global LevelManager.
            PolymindGames.LevelManager hqLevels = PolymindGames.LevelManager.Instance;
            if (hqLevels == null)
            {
                Debug.LogWarning("[ProjectZone][Inventory] LevelManager missing — cannot HQ save.");
                return;
            }

            if (!hqLevels.IsCurrentGameValid())
            {
                Debug.LogWarning("[ProjectZone][Inventory] HQ CurrentGameID invalid. SceneSaveHandler must exist at boot.");
                return;
            }

            bool ok = hqLevels.SaveCurrentGame(0);
            Debug.Log(ok
                ? "[ProjectZone][Inventory] HQ LevelManager.SaveCurrentGame(0) started."
                : "[ProjectZone][Inventory] HQ SaveCurrentGame failed (busy or invalid).");
        }

        private void LogStatus(string context)
        {
            var sb = new StringBuilder();
            sb.Append("[ProjectZone][Inventory] ").Append(context).Append(" | ");
            sb.Append("playerInv=").Append(_sawPlayerInventory ? "OK" : "…").Append(' ');
            sb.Append("backpack=").Append(_sawBackpackContainer ? "OK (MMB wheel)" : "…").Append(' ');
            sb.Append("loot=").Append(_sawLoot ? "OK" : "…").Append(' ');
            sb.Append("stash=").Append(_sawStash ? "OK" : "…").Append(' ');
            sb.Append("xferUI=").Append(_transferPanelOpened ? "OK" : "open loot/stash").Append(' ');
            sb.Append("snapSave=").Append(_snapshotSaved ? "OK" : "F5").Append(' ');
            sb.Append("snapLoad=").Append(_snapshotLoaded ? "OK" : "F9").Append(' ');
            sb.Append("stashSave=").Append(_stashSaved ? "OK" : "F6");
            Debug.Log(sb.ToString(), this);
        }
    }
}
