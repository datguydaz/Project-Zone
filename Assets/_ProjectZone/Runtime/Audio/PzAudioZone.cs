using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Trigger / bounds volume that drives ambient bed selection (#56).
    /// Highest priority overlapping zone wins; none → Outdoor default.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzAudioZone : MonoBehaviour
    {
        [SerializeField]
        private PzAmbientKind _kind = PzAmbientKind.SettlementIndoor;

        [SerializeField]
        private int _priority = 10;

        [SerializeField]
        private Collider _bounds;

        [SerializeField]
        private bool _drawGizmo = true;

        public PzAmbientKind Kind => _kind;
        public int Priority => _priority;

        private void Awake()
        {
            EnsureCollider();
        }

        private void OnEnable()
        {
            PzAmbientAudioDirector.Register(this);
        }

        private void OnDisable()
        {
            PzAmbientAudioDirector.Unregister(this);
        }

        public void Configure(PzAmbientKind kind, int priority)
        {
            _kind = kind;
            _priority = priority;
            EnsureCollider();
        }

        public bool Contains(Vector3 worldPoint)
        {
            if (_bounds == null)
                EnsureCollider();
            if (_bounds == null)
                return false;

            // Expand a bit so feet / camera height still count (avoids "worked then stopped" at edges).
            Bounds b = _bounds.bounds;
            b.Expand(new Vector3(1f, 3f, 1f));
            return b.Contains(worldPoint);
        }

        private void EnsureCollider()
        {
            if (_bounds == null)
                _bounds = GetComponent<Collider>();
            if (_bounds == null)
                _bounds = gameObject.AddComponent<BoxCollider>();

            _bounds.isTrigger = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawGizmo)
                return;
            EnsureCollider();
            if (_bounds == null)
                return;

            Color c = _kind switch
            {
                PzAmbientKind.SettlementYard => new Color(0.45f, 0.7f, 0.4f, 0.12f),
                PzAmbientKind.SettlementIndoor => new Color(0.35f, 0.75f, 0.45f, 0.2f),
                PzAmbientKind.IndustrialIndoor => new Color(0.75f, 0.55f, 0.25f, 0.2f),
                PzAmbientKind.Underground => new Color(0.35f, 0.45f, 0.8f, 0.22f),
                _ => new Color(0.55f, 0.65f, 0.75f, 0.15f)
            };
            Gizmos.color = c;
            Gizmos.matrix = Matrix4x4.TRS(_bounds.bounds.center, Quaternion.identity, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, _bounds.bounds.size);
            Gizmos.color = new Color(c.r, c.g, c.b, 0.85f);
            Gizmos.DrawWireCube(Vector3.zero, _bounds.bounds.size);
        }
#endif
    }
}
