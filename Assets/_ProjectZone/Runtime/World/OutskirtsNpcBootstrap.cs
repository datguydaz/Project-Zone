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
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class OutskirtsNpcBootstrap : MonoBehaviour
    {
        private const string RootName = "Outskirts_NPCs";
        private const string DefaultHostilePrefabPath =
            "Assets/Emerald AI/Demo/Demo Source/Demo Abilities/Ability Source/Other/Example Summoned AI 1.prefab";

        [SerializeField]
        private GameObject _hostilePrefab;

        [SerializeField]
        private bool _spawnOnStart = true;

        [SerializeField]
        private bool _bakeNavMeshOnStart = true;

        [SerializeField]
        private bool _wirePlayerTargetAdapter = true;

        private void Start()
        {
            ResolveHostilePrefab();

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
            ResolveHostilePrefab();
        }
#endif

        private void ResolveHostilePrefab()
        {
            if (_hostilePrefab != null)
                return;
#if UNITY_EDITOR
            _hostilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultHostilePrefabPath);
#endif
        }

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
            ResolveHostilePrefab();

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

            if (_hostilePrefab != null)
            {
                SpawnHostile(npcs.transform, _hostilePrefab, "Hostile_Checkpoint", new Vector3(45f, 0.1f, 4f));
                SpawnHostile(npcs.transform, _hostilePrefab, "Hostile_Industrial", new Vector3(92f, 0.1f, 2f));
            }
            else
            {
                CreateNavPlaceholder(npcs.transform, "Hostile_Checkpoint_Placeholder",
                    new Vector3(45f, 0.1f, 4f), new Color(0.7f, 0.35f, 0.3f), wander: true);
                CreateNavPlaceholder(npcs.transform, "Hostile_Industrial_Placeholder",
                    new Vector3(92f, 0.1f, 2f), new Color(0.7f, 0.35f, 0.3f), wander: true);
                Debug.LogWarning(
                    "[ProjectZone][Outskirts] Emerald hostile prefab missing — using NavMesh placeholders.",
                    this);
            }
        }

        private static void SpawnHostile(Transform parent, GameObject prefab, string name, Vector3 worldPos)
        {
            GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity, parent);
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
