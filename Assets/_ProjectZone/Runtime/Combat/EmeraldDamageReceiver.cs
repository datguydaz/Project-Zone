using EmeraldAI;
using PolymindGames;
using PolymindGames.SurfaceSystem;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Adds a Hitbox-layer volume HQ weapons can hit, without moving the AI off Emerald's detection layer.
    /// Demo AIs live on Water so they can shoot each other; HQ raycasts ignore Water, so a child volume is required.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EmeraldHealth))]
    public sealed class EmeraldDamageReceiver : MonoBehaviour
    {
        private const string HitVolumeName = "HQ Damage Volume";
        private const string FleshSurfaceName = "Flesh";
        private const float MinVolumeDepth = 0.75f;
        private const float MinVolumeWidth = 0.7f;

        [SerializeField]
        private bool _criticalHit;

        private bool _configured;

        private void Awake() => Configure();

        private void Start() => Configure();

        public void Configure()
        {
            if (_configured)
                return;

            var health = GetComponent<EmeraldHealth>();
            if (health == null)
                return;

            Transform existing = transform.Find(HitVolumeName);
            GameObject volumeObject = existing != null ? existing.gameObject : new GameObject(HitVolumeName);
            if (existing == null)
                volumeObject.transform.SetParent(transform, false);

            volumeObject.layer = LayerConstants.Hitbox;
            volumeObject.transform.localPosition = Vector3.zero;
            volumeObject.transform.localRotation = Quaternion.identity;
            volumeObject.transform.localScale = Vector3.one;

            BoxCollider source = GetComponent<BoxCollider>();
            var system = GetComponent<EmeraldSystem>();
            if (system != null && system.AIBoxCollider != null)
                source = system.AIBoxCollider;

            BoxCollider volume = volumeObject.GetComponent<BoxCollider>();
            if (volume == null)
                volume = volumeObject.AddComponent<BoxCollider>();

            volume.isTrigger = false;
            volume.enabled = true;
            ApplyVolumeShape(volume, source);
            ApplyFleshSurface(volume);

            EmeraldDamageHitbox hitbox = volumeObject.GetComponent<EmeraldDamageHitbox>();
            if (hitbox == null)
                hitbox = volumeObject.AddComponent<EmeraldDamageHitbox>();

            hitbox.Bind(health, _criticalHit);
            _configured = true;
        }

        private static void ApplyVolumeShape(BoxCollider volume, BoxCollider source)
        {
            if (source != null)
            {
                Vector3 size = source.size;
                size.x = Mathf.Max(size.x, MinVolumeWidth);
                size.z = Mathf.Max(size.z, MinVolumeDepth);
                volume.center = source.center;
                volume.size = size;
                return;
            }

            volume.center = new Vector3(0f, 1f, 0f);
            volume.size = new Vector3(MinVolumeWidth, 2f, MinVolumeDepth);
        }

        private static void ApplyFleshSurface(BoxCollider volume)
        {
            if (!SurfaceDefinition.TryGetWithName(FleshSurfaceName, out SurfaceDefinition flesh))
                return;

            PhysicsMaterial[] materials = flesh.Materials;
            if (materials != null && materials.Length > 0 && materials[0] != null)
                volume.sharedMaterial = materials[0];
        }
    }
}
