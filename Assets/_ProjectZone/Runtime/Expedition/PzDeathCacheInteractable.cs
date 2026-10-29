using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Opens the recoverable death cache (world marker or Settlement claim terminal).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Interactable))]
    public sealed class PzDeathCacheInteractable : MonoBehaviour
    {
        private Interactable _interactable;

        private void Awake()
        {
            _interactable = GetComponent<Interactable>();
            if (string.IsNullOrEmpty(_interactable.Title))
                _interactable.Title = "Death Cache";
            if (string.IsNullOrEmpty(_interactable.Description))
                _interactable.Description = "Recover lost gear";
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
            PzDeathCacheService cache = PzDeathCacheService.Instance;
            if (cache == null)
            {
                Debug.LogWarning("[ProjectZone][DeathCache] No death cache service.", this);
                return;
            }

            if (!cache.HasPendingCache)
            {
                _interactable.Description = "Empty — nothing to recover";
                return;
            }

            cache.OpenFor(character);
            cache.NotifyOpened();
            if (!cache.HasPendingCache)
                PzExpeditionLoop.Instance?.NotifyCacheRecovered();
        }
    }
}
