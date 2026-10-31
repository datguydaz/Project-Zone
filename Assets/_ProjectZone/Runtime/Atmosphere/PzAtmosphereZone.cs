using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Bounds volume that drives indoor/outdoor atmosphere blend (#57).
    /// Highest priority overlapping zone wins; none → Outdoor.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PzAtmosphereZone : MonoBehaviour
    {
        [SerializeField]
        private PzAtmosphereSpace _space = PzAtmosphereSpace.Indoor;

        [SerializeField]
        private int _priority = 10;

        [SerializeField]
        private Collider _bounds;

        [SerializeField]
        private bool _drawGizmo = true;

        public PzAtmosphereSpace Space => _space;
        public int Priority => _priority;

        private void Awake() => EnsureCollider();

        private void OnEnable() => PzAtmosphereTransitionDirector.Register(this);

        private void OnDisable() => PzAtmosphereTransitionDirector.Unregister(this);

        public void Configure(PzAtmosphereSpace space, int priority)
        {
            _space = space;
            _priority = priority;
            EnsureCollider();
        }

        public bool Contains(Vector3 worldPoint)
        {
            if (_bounds == null)
                EnsureCollider();
            if (_bounds == null)
                return false;

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

            Color c = _space switch
            {
                PzAtmosphereSpace.Sheltered => new Color(0.55f, 0.7f, 0.35f, 0.12f),
                PzAtmosphereSpace.Indoor => new Color(0.35f, 0.55f, 0.85f, 0.2f),
                _ => new Color(0.6f, 0.6f, 0.6f, 0.1f)
            };
            Gizmos.color = c;
            Gizmos.matrix = Matrix4x4.TRS(_bounds.bounds.center, Quaternion.identity, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, _bounds.bounds.size);
            Gizmos.color = new Color(c.r, c.g, c.b, 0.9f);
            Gizmos.DrawWireCube(Vector3.zero, _bounds.bounds.size);
        }
#endif
    }
}
