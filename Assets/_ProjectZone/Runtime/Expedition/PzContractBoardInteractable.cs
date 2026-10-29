using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Interactable))]
    public sealed class PzContractBoardInteractable : MonoBehaviour
    {
        private Interactable _interactable;

        private void Awake()
        {
            _interactable = GetComponent<Interactable>();
            if (string.IsNullOrEmpty(_interactable.Title))
                _interactable.Title = "Contract Board";
            RefreshDescription();
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

        private void Update()
        {
            // Keep prompt text fresh while hovered / nearby.
            if (Time.frameCount % 30 == 0)
                RefreshDescription();
        }

        private void OnInteracted(IInteractable interactable, ICharacter character)
        {
            PzContractBoardService board = PzContractBoardService.Instance;
            if (board == null)
            {
                Debug.LogWarning("[ProjectZone][Contracts] Board service missing.", this);
                return;
            }

            board.Interact(character);
            RefreshDescription();
        }

        private void RefreshDescription()
        {
            if (_interactable == null)
                return;

            PzContractBoardService board = PzContractBoardService.Instance;
            _interactable.Description = board != null
                ? board.GetBoardStatusText()
                : "Contracts offline";
        }
    }
}
