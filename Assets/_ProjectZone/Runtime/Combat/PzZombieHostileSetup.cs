using PolymindGames;
using PolymindGames.SurfaceSystem;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZone
{
    /// <summary>
    /// Attaches greybox melee AI + HQ damage volumes to a Female Zombie instance.
    /// </summary>
    public static class PzZombieHostileSetup
    {
        private const string HitVolumeName = "HQ Damage Volume";
        private const string FleshSurfaceName = "Flesh";
        private const float MinVolumeWidth = 0.7f;
        private const float MinVolumeDepth = 0.75f;

        public static void Configure(
            GameObject zombie,
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip attack,
            AnimationClip death)
        {
            if (zombie == null)
                return;

            EnsureRootCapsule(zombie);
            EnsureNavMeshAgent(zombie);

            PzHostileHealth health = zombie.GetComponent<PzHostileHealth>();
            if (health == null)
                health = zombie.AddComponent<PzHostileHealth>();

            PzZombieHostileBrain brain = zombie.GetComponent<PzZombieHostileBrain>();
            if (brain == null)
                brain = zombie.AddComponent<PzZombieHostileBrain>();

            brain.ConfigureClips(idle, walk, run, attack, death);
            EnsureDamageVolume(zombie, health);
        }

        private static void EnsureRootCapsule(GameObject zombie)
        {
            CapsuleCollider capsule = zombie.GetComponent<CapsuleCollider>();
            if (capsule == null)
                capsule = zombie.AddComponent<CapsuleCollider>();

            capsule.height = 1.8f;
            capsule.radius = 0.35f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.direction = 1;
            capsule.isTrigger = false;
        }

        private static void EnsureNavMeshAgent(GameObject zombie)
        {
            NavMeshAgent agent = zombie.GetComponent<NavMeshAgent>();
            if (agent == null)
                agent = zombie.AddComponent<NavMeshAgent>();

            agent.height = 1.8f;
            agent.radius = 0.35f;
            agent.speed = 2.1f;
            agent.acceleration = 8f;
            agent.angularSpeed = 220f;
            agent.stoppingDistance = 1f;
        }

        private static void EnsureDamageVolume(GameObject zombie, PzHostileHealth health)
        {
            Transform existing = zombie.transform.Find(HitVolumeName);
            GameObject volumeObject = existing != null ? existing.gameObject : new GameObject(HitVolumeName);
            if (existing == null)
                volumeObject.transform.SetParent(zombie.transform, false);

            volumeObject.layer = LayerConstants.Hitbox;
            volumeObject.transform.localPosition = Vector3.zero;
            volumeObject.transform.localRotation = Quaternion.identity;
            volumeObject.transform.localScale = Vector3.one;

            BoxCollider volume = volumeObject.GetComponent<BoxCollider>();
            if (volume == null)
                volume = volumeObject.AddComponent<BoxCollider>();

            volume.isTrigger = false;
            volume.enabled = true;
            volume.center = new Vector3(0f, 1f, 0f);
            volume.size = new Vector3(MinVolumeWidth, 2f, MinVolumeDepth);
            ApplyFleshSurface(volume);

            PzHostileDamageHitbox hitbox = volumeObject.GetComponent<PzHostileDamageHitbox>();
            if (hitbox == null)
                hitbox = volumeObject.AddComponent<PzHostileDamageHitbox>();

            hitbox.Bind(health);
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
