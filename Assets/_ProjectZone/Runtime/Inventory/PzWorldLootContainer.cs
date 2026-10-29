using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 4 / #21 — openable world loot using HQ Interactable + ItemContainer (not a second inventory).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PzContainerHost))]
    [RequireComponent(typeof(Interactable))]
    public sealed class PzWorldLootContainer : MonoBehaviour
    {
        [SerializeField]
        private string _panelTitle = "Loot";

        private PzContainerHost _host;
        private Interactable _interactable;

        public PzContainerHost Host => _host;

        private void Awake()
        {
            _host = GetComponent<PzContainerHost>();
            _interactable = GetComponent<Interactable>();
            _host.EnsureCreated();

            if (string.IsNullOrEmpty(_interactable.Title))
                _interactable.Title = _panelTitle;
            if (string.IsNullOrEmpty(_interactable.Description))
                _interactable.Description = "Open container";
        }

        private void OnEnable()
        {
            if (_interactable == null)
                _interactable = GetComponent<Interactable>();
            _interactable.Interacted += OnInteracted;
        }

        private void OnDisable()
        {
            if (_interactable != null)
                _interactable.Interacted -= OnInteracted;
        }

        private void OnInteracted(IInteractable interactable, ICharacter character)
        {
            if (character == null || character.Inventory == null)
                return;

            _host.EnsureCreated();
            PzInventoryPanelController.Open(
                character,
                _host.Container,
                string.IsNullOrEmpty(_interactable.Title) ? _panelTitle : _interactable.Title);
        }
    }
}
