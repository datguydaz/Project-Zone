using PolymindGames;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZone
{
    /// <summary>
    /// Greybox melee chase AI for Female Zombie visuals.
    /// Plays Idle/Walk/Run/Attack/Death clips through <see cref="PzZombieAnimDriver"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PzHostileHealth))]
    public sealed class PzZombieHostileBrain : MonoBehaviour
    {
        [SerializeField]
        private float _detectRange = 28f;

        [SerializeField]
        private float _meleeRange = 1.85f;

        [SerializeField]
        private float _meleeDamage = 12f;

        [SerializeField]
        private float _meleeCooldown = 1.35f;

        [SerializeField]
        private float _walkSpeed = 2.1f;

        [SerializeField]
        private float _runSpeed = 3.6f;

        [SerializeField]
        private float _runDistance = 10f;

        [SerializeField]
        private AnimationClip _idleClip;

        [SerializeField]
        private AnimationClip _walkClip;

        [SerializeField]
        private AnimationClip _runClip;

        [SerializeField]
        private AnimationClip _attackClip;

        [SerializeField]
        private AnimationClip _deathClip;

        private NavMeshAgent _agent;
        private Animator _animator;
        private PzHostileHealth _health;
        private Transform _target;
        private float _nextMeleeTime;
        private bool _dead;
        private readonly PzZombieAnimDriver _anim = new PzZombieAnimDriver();

        public bool IsMeleeHostile => true;

        private void Awake()
        {
            ApplyProjectSettingsTuning();

            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<PzHostileHealth>();
            _animator = GetComponentInChildren<Animator>();

            _agent.speed = _walkSpeed;
            _agent.stoppingDistance = Mathf.Max(0.6f, _meleeRange * 0.55f);
            _agent.angularSpeed = 220f;
            _agent.acceleration = 8f;
            _agent.height = 1.8f;
            _agent.radius = 0.35f;

            EnsureAnimDriver();

            if (_health != null)
                _health.Died += HandleDeath;
        }

        private void ApplyProjectSettingsTuning()
        {
            PzProjectSettings.ZombieSettings z = PzProjectSettings.Get()?.Zombies;
            if (z == null)
                return;

            _detectRange = z.DetectRange;
            _meleeRange = z.MeleeRange;
            _meleeDamage = z.MeleeDamage;
            _meleeCooldown = z.MeleeCooldown;
            _walkSpeed = z.WalkSpeed;
            _runSpeed = z.RunSpeed;
            _runDistance = z.RunDistance;

            if (_idleClip == null && z.IdleClip != null)
                _idleClip = z.IdleClip;
            if (_walkClip == null && z.WalkClip != null)
                _walkClip = z.WalkClip;
            if (_runClip == null && z.RunClip != null)
                _runClip = z.RunClip;
            if (_attackClip == null && z.AttackClip != null)
                _attackClip = z.AttackClip;
            if (_deathClip == null && z.DeathClip != null)
                _deathClip = z.DeathClip;
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.Died -= HandleDeath;

            _anim.Dispose();
        }

        public void ConfigureClips(
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip attack,
            AnimationClip death)
        {
            _idleClip = idle;
            _walkClip = walk;
            _runClip = run;
            _attackClip = attack;
            _deathClip = death;
            EnsureAnimDriver();
        }

        private void EnsureAnimDriver()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
                return;

            if (_idleClip == null || _walkClip == null || _runClip == null || _attackClip == null || _deathClip == null)
                return;

            _anim.Initialize(_animator, _idleClip, _walkClip, _runClip, _attackClip, _deathClip);
        }

        private void Update()
        {
            _anim.Tick(Time.deltaTime);

            if (_dead)
                return;

            if (_target == null)
                AcquireTarget();

            if (_target == null)
            {
                StopMoving();
                _anim.Play(PzZombieAnimDriver.ClipId.Idle);
                return;
            }

            float distance = Vector3.Distance(transform.position, _target.position);
            if (distance > _detectRange)
            {
                _target = null;
                StopMoving();
                _anim.Play(PzZombieAnimDriver.ClipId.Idle);
                return;
            }

            if (distance <= _meleeRange)
            {
                StopMoving();
                FaceTarget();
                TryMelee();
                return;
            }

            _agent.isStopped = false;
            _agent.speed = distance > _runDistance ? _runSpeed : _walkSpeed;
            _agent.SetDestination(_target.position);
            _anim.Play(distance > _runDistance
                ? PzZombieAnimDriver.ClipId.Run
                : PzZombieAnimDriver.ClipId.Walk);
        }

        private void AcquireTarget()
        {
            if (Player.AllPlayers.Count == 0)
                return;

            Player player = Player.AllPlayers[0];
            if (player == null || player.HealthManager == null || player.HealthManager.IsDead())
                return;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= _detectRange)
                _target = player.transform;
        }

        private void TryMelee()
        {
            // Keep attack pose while in range; restart clip on each swing.
            if (Time.time < _nextMeleeTime)
            {
                if (_anim.Current != PzZombieAnimDriver.ClipId.Attack)
                    _anim.Play(PzZombieAnimDriver.ClipId.Attack, 0.08f);
                return;
            }

            _nextMeleeTime = Time.time + _meleeCooldown;
            _anim.Play(PzZombieAnimDriver.ClipId.Attack, 0.05f, forceRestart: true);

            if (_target == null)
                return;

            Player player = _target.GetComponentInParent<Player>();
            if (player == null || player.HealthManager == null || player.HealthManager.IsDead())
                return;

            var args = new DamageArgs(DamageType.Slash, null, _target.position, Vector3.zero);
            player.HealthManager.ReceiveDamage(_meleeDamage, in args);
        }

        private void FaceTarget()
        {
            Vector3 flat = _target.position - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(flat.normalized, Vector3.up),
                Time.deltaTime * 10f);
        }

        private void StopMoving()
        {
            if (_agent == null || !_agent.isOnNavMesh)
                return;

            _agent.isStopped = true;
            _agent.ResetPath();
        }

        private void HandleDeath()
        {
            if (_dead)
                return;

            _dead = true;
            StopMoving();
            if (_agent != null)
                _agent.enabled = false;

            _anim.Play(PzZombieAnimDriver.ClipId.Death);

            Collider[] colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
        }
    }
}
