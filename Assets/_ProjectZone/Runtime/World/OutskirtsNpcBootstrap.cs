using System.Collections;
using EmeraldAI;
using PolymindGames;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 3 / issue #19 — place Outskirts NPCs and bake/validate NavMesh navigation.
    /// NPCs live under Outskirts_NPCs (not greybox zones) so layout rebuilds do not wipe them.
    /// Hostiles: Female Zombie melee greybox (+ optional Emerald ranged fallback).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class OutskirtsNpcBootstrap : MonoBehaviour
    {
        private const string RootName = "Outskirts_NPCs";
        private const string DefaultZombiePrefabPath =
            "Assets/ZombieFemaleAnimations/Prefabs/FemaleZombie.prefab";
        private const string DefaultEmeraldRangedPrefabPath =
            "Assets/Emerald AI/Demo/Demo Source/Demo Abilities/Ability Source/Other/Example Summoned AI 1.prefab";

        private const string IdleFbxPath =
            "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Idle01.FBX";
        private const string WalkFbxPath =
            "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Walk01Forward.FBX";
        private const string RunFbxPath =
            "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Run01ForwardInPlace.FBX";
        private const string AttackFbxPath =
            "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_AttackInPlace01.FBX";
        private const string DeathFbxPath =
            "Assets/ZombieFemaleAnimations/Art/Animations/ZombieFemale_Death01.FBX";

        [SerializeField]
        private GameObject _zombiePrefab;

        [SerializeField]
        private GameObject _emeraldRangedPrefab;

        [SerializeField]
        private AnimationClip _zombieIdleClip;

        [SerializeField]
        private AnimationClip _zombieWalkClip;

        [SerializeField]
        private AnimationClip _zombieRunClip;

        [SerializeField]
        private AnimationClip _zombieAttackClip;

        [SerializeField]
        private AnimationClip _zombieDeathClip;

        [SerializeField]
        private bool _spawnOnStart = true;

        [SerializeField]
        private bool _bakeNavMeshOnStart = true;

        [SerializeField]
        private bool _wirePlayerTargetAdapter = true;

        [SerializeField]
        private bool _spawnEmeraldRangedAtIndustrial;

        private void Start()
        {
            ResolvePrefabs();

            if (_bakeNavMeshOnStart)
                BakeNavMesh();

            if (_spawnOnStart)
                EnsureNpcs();

            if (_wirePlayerTargetAdapter)
                StartCoroutine(WirePlayerWhenReady());
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolvePrefabs();
        }
#endif

        private void ResolvePrefabs()
        {
#if UNITY_EDITOR
            if (_zombiePrefab == null)
            {
                GameObject zombie = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultZombiePrefabPath);
                if (zombie != null)
                    _zombiePrefab = zombie;
            }

            if (_emeraldRangedPrefab == null)
            {
                GameObject ranged = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultEmeraldRangedPrefabPath);
                if (ranged != null)
                    _emeraldRangedPrefab = ranged;
            }

            if (_zombieIdleClip == null)
                _zombieIdleClip = LoadClip(IdleFbxPath, "ZombieFemale_Idle01");
            if (_zombieWalkClip == null)
                _zombieWalkClip = LoadClip(WalkFbxPath, "ZombieFemale_Walk01Forward");
            if (_zombieRunClip == null)
                _zombieRunClip = LoadClip(RunFbxPath, "ZombieFemale_Run01ForwardInPlace");
            if (_zombieAttackClip == null)
                _zombieAttackClip = LoadClip(AttackFbxPath, "ZombieFemale_AttackInPlace01");
            if (_zombieDeathClip == null)
                _zombieDeathClip = LoadClip(DeathFbxPath, "ZombieFemale_Death01");
#endif
        }

#if UNITY_EDITOR
        private static AnimationClip LoadClip(string fbxPath, string preferredName)
        {
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            AnimationClip fallback = null;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not AnimationClip clip)
                    continue;
                if (clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                    continue;
                if (clip.name == preferredName)
                    return clip;
                fallback ??= clip;
            }

            return fallback;
        }
#endif

        [ContextMenu("Bake Outskirts NavMesh")]
        public void BakeNavMesh()
        {
            NavMeshSurface surface = GetComponent<NavMeshSurface>();
            if (surface == null)
                surface = FindAnyObjectByType<NavMeshSurface>();

            if (surface == null)
            {
                Debug.LogWarning("[ProjectZone][Outskirts] No NavMeshSurface found to bake.", this);
                return;
            }

            surface.BuildNavMesh();
        }

        [ContextMenu("Respawn Outskirts NPCs")]
        public void EnsureNpcs()
        {
            ResolvePrefabs();

            Transform root = transform.Find(RootName);
            if (root != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(root.gameObject);
                else
#endif
                    Destroy(root.gameObject);
            }

            var npcs = new GameObject(RootName);
            npcs.transform.SetParent(transform, false);

            CreateNavPlaceholder(npcs.transform, "Settlement_Civilian_Placeholder",
                new Vector3(-8f, 0.1f, 2f), new Color(0.45f, 0.7f, 0.45f), wander: true);

            if (_zombiePrefab != null)
            {
                SpawnZombie(npcs.transform, "Hostile_Checkpoint_Zombie", new Vector3(45f, 0.1f, 4f));
                SpawnZombie(npcs.transform, "Hostile_Industrial_Zombie", new Vector3(92f, 0.1f, 2f));
            }
            else
            {
                CreateNavPlaceholder(npcs.transform, "Hostile_Checkpoint_Placeholder",
                    new Vector3(45f, 0.1f, 4f), new Color(0.7f, 0.35f, 0.3f), wander: true);
                CreateNavPlaceholder(npcs.transform, "Hostile_Industrial_Placeholder",
                    new Vector3(92f, 0.1f, 2f), new Color(0.7f, 0.35f, 0.3f), wander: true);
                Debug.LogWarning(
                    "[ProjectZone][Outskirts] FemaleZombie prefab missing — using NavMesh placeholders.",
                    this);
            }

            if (_spawnEmeraldRangedAtIndustrial && _emeraldRangedPrefab != null)
                SpawnEmeraldHostile(npcs.transform, _emeraldRangedPrefab, "Hostile_Industrial_Ranged",
                    new Vector3(96f, 0.1f, 6f));
        }

        private void SpawnZombie(Transform parent, string name, Vector3 worldPos)
        {
            GameObject instance;
            try
            {
                Object spawned = Instantiate((Object)_zombiePrefab, worldPos, Quaternion.identity, parent);
                instance = spawned as GameObject;
                if (instance == null)
                {
                    if (spawned != null)
                        Destroy(spawned);
                    throw new System.InvalidCastException(
                        $"Zombie prefab '{_zombiePrefab.name}' did not instantiate as a GameObject.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(
                    $"[ProjectZone][Outskirts] Failed to spawn zombie '{name}': {ex.Message}. Using placeholder.",
                    this);
                CreateNavPlaceholder(parent, name + "_Placeholder", worldPos, new Color(0.7f, 0.35f, 0.3f), wander: true);
                return;
            }

            instance.name = name;
            PzZombieHostileSetup.Configure(
                instance,
                _zombieIdleClip,
                _zombieWalkClip,
                _zombieRunClip,
                _zombieAttackClip,
                _zombieDeathClip);

            if (NavMesh.SamplePosition(worldPos, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                instance.transform.position = hit.position;
        }

        private void SpawnEmeraldHostile(Transform parent, GameObject prefab, string name, Vector3 worldPos)
        {
            GameObject instance;
            try
            {
                Object spawned = Instantiate((Object)prefab, worldPos, Quaternion.identity, parent);
                instance = spawned as GameObject;
                if (instance == null)
                {
                    if (spawned != null)
                        Destroy(spawned);
                    throw new System.InvalidCastException(
                        $"Hostile prefab '{prefab.name}' did not instantiate as a GameObject.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(
                    $"[ProjectZone][Outskirts] Failed to spawn Emerald hostile '{name}': {ex.Message}.",
                    this);
                return;
            }

            instance.name = name;

            if (instance.GetComponent<EmeraldDamageReceiver>() == null)
            {
                EmeraldDamageReceiver receiver = instance.AddComponent<EmeraldDamageReceiver>();
                receiver.Configure();
            }
        }

        private static void CreateNavPlaceholder(
            Transform parent,
            string name,
            Vector3 worldPos,
            Color color,
            bool wander)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                mat.color = color;
                renderer.sharedMaterial = mat;
            }

            var agent = go.AddComponent<NavMeshAgent>();
            agent.height = 2f;
            agent.radius = 0.4f;
            agent.speed = 2.2f;
            agent.acceleration = 6f;
            agent.angularSpeed = 180f;

            if (wander)
                go.AddComponent<OutskirtsNavWanderer>();
        }

        private IEnumerator WirePlayerWhenReady()
        {
            while (Player.AllPlayers.Count == 0)
                yield return null;

            Player player = Player.AllPlayers[0];
            EmeraldHqPlayerTargetAdapter adapter = player.GetComponent<EmeraldHqPlayerTargetAdapter>();
            if (adapter == null)
                adapter = player.gameObject.AddComponent<EmeraldHqPlayerTargetAdapter>();

            adapter.Configure(player, logDiagnostics: false);
        }
    }
}
