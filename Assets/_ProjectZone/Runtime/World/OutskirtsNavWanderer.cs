using UnityEngine;
using UnityEngine.AI;

namespace ProjectZone
{
    /// <summary>
    /// Simple NavMesh wander used to validate Outskirts navigation (#19).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class OutskirtsNavWanderer : MonoBehaviour
    {
        [SerializeField]
        private float _radius = 8f;

        [SerializeField]
        private float _repathSeconds = 4f;

        private NavMeshAgent _agent;
        private float _timer;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _timer = 0f;
            TrySetDestination();
        }

        private void Update()
        {
            if (_agent == null || !_agent.isOnNavMesh)
                return;

            _timer += Time.deltaTime;
            if (_timer >= _repathSeconds || (!_agent.pathPending && _agent.remainingDistance <= 0.35f))
            {
                _timer = 0f;
                TrySetDestination();
            }
        }

        private void TrySetDestination()
        {
            Vector3 random = transform.position + Random.insideUnitSphere * _radius;
            random.y = transform.position.y;
            if (NavMesh.SamplePosition(random, out NavMeshHit hit, _radius, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }
    }
}
