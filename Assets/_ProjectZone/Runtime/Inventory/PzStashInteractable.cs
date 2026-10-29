using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Settlement interactable that opens <see cref="PzPermanentStash"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Interactable))]
    public sealed class PzStashInteractable : MonoBehaviour
    {
        private Interactable _interactable;

        private void Awake()
        {
            _interactable = GetComponent<Interactable>();
            if (string.IsNullOrEmpty(_interactable.Title))
                _interactable.Title = "Stash";
            if (string.IsNullOrEmpty(_interactable.Description))
                _interactable.Description = "Open permanent stash";
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
            PzPermanentStash stash = PzPermanentStash.Instance;
            if (stash == null)
            {
                Debug.LogWarning("[ProjectZone][Inventory] No PzPermanentStash instance in scene.", this);
                return;
            }

            stash.OpenFor(character);
        }
    }
}
