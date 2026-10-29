using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ProjectZone
{
    /// <summary>
    /// Plays Female Zombie AnimationClips through a PlayableGraph so we do not depend on
    /// the pack's one-controller-per-clip assets.
    /// </summary>
    public sealed class PzZombieAnimDriver
    {
        public enum ClipId
        {
            Idle = 0,
            Walk = 1,
            Run = 2,
            Attack = 3,
            Death = 4
        }

        private const int ClipCount = 5;

        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private readonly AnimationClipPlayable[] _clipPlayables = new AnimationClipPlayable[ClipCount];
        private readonly AnimationClip[] _clips = new AnimationClip[ClipCount];
        private ClipId _current = ClipId.Idle;
        private bool _ready;
        private float _blendDuration = 0.15f;
        private float _blendElapsed;
        private ClipId _from;
        private ClipId _to;
        private bool _blending;
        private bool _deathLocked;
        private float _deathElapsed;
        private bool _deathPoseHeld;

        public ClipId Current => _current;
        public bool IsReady => _ready;
        public bool IsDeathLocked => _deathLocked;

        public void Initialize(Animator animator, AnimationClip idle, AnimationClip walk, AnimationClip run, AnimationClip attack, AnimationClip death)
        {
            Dispose();

            if (animator == null || idle == null || walk == null || run == null || attack == null || death == null)
                return;

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController = null;

            _clips[(int)ClipId.Idle] = idle;
            _clips[(int)ClipId.Walk] = walk;
            _clips[(int)ClipId.Run] = run;
            _clips[(int)ClipId.Attack] = attack;
            _clips[(int)ClipId.Death] = death;

            _graph = PlayableGraph.Create("PzZombieAnim");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _mixer = AnimationMixerPlayable.Create(_graph, ClipCount);

            for (int i = 0; i < ClipCount; i++)
            {
                _clipPlayables[i] = AnimationClipPlayable.Create(_graph, _clips[i]);
                _clipPlayables[i].SetApplyFootIK(false);
                _graph.Connect(_clipPlayables[i], 0, _mixer, i);
                _mixer.SetInputWeight(i, 0f);
            }

            _mixer.SetInputWeight((int)ClipId.Idle, 1f);
            _clipPlayables[(int)ClipId.Idle].SetTime(0);
            _clipPlayables[(int)ClipId.Idle].Play();

            var output = AnimationPlayableOutput.Create(_graph, "Zombie", animator);
            output.SetSourcePlayable(_mixer);
            _graph.Play();

            _current = ClipId.Idle;
            _ready = true;
            _deathLocked = false;
            _deathPoseHeld = false;
            _deathElapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!_ready)
                return;

            if (_deathLocked)
            {
                UpdateDeathHold(deltaTime);
                return;
            }

            if (!_blending)
                return;

            _blendElapsed += deltaTime;
            float t = _blendDuration <= 0f ? 1f : Mathf.Clamp01(_blendElapsed / _blendDuration);

            for (int i = 0; i < ClipCount; i++)
                _mixer.SetInputWeight(i, 0f);

            _mixer.SetInputWeight((int)_from, 1f - t);
            _mixer.SetInputWeight((int)_to, t);

            if (t >= 1f)
            {
                _blending = false;
                _current = _to;
                for (int i = 0; i < ClipCount; i++)
                    _mixer.SetInputWeight(i, i == (int)_current ? 1f : 0f);
            }
        }

        public void Play(ClipId id, float blend = 0.15f, bool forceRestart = false)
        {
            if (!_ready)
                return;

            // Vendor death clips are imported with loopTime — lock after first play.
            if (_deathLocked)
                return;

            if (!forceRestart && !_blending && _current == id)
                return;

            if (!forceRestart && _blending && _to == id)
                return;

            if (id == ClipId.Death)
            {
                BeginDeath();
                return;
            }

            _from = _current;
            _to = id;
            _blendDuration = Mathf.Max(0.01f, blend);
            _blendElapsed = 0f;
            _blending = true;

            AnimationClipPlayable playable = _clipPlayables[(int)id];
            playable.SetTime(0);
            playable.SetSpeed(1);
            playable.Play();
        }

        private void BeginDeath()
        {
            _deathLocked = true;
            _deathPoseHeld = false;
            _deathElapsed = 0f;
            _blending = false;
            _current = ClipId.Death;

            for (int i = 0; i < ClipCount; i++)
                _mixer.SetInputWeight(i, i == (int)ClipId.Death ? 1f : 0f);

            AnimationClipPlayable playable = _clipPlayables[(int)ClipId.Death];
            playable.SetTime(0);
            playable.SetSpeed(1);
            playable.Play();
        }

        private void UpdateDeathHold(float deltaTime)
        {
            int death = (int)ClipId.Death;
            AnimationClip clip = _clips[death];
            if (clip == null || !_clipPlayables[death].IsValid())
                return;

            for (int i = 0; i < ClipCount; i++)
                _mixer.SetInputWeight(i, i == death ? 1f : 0f);

            if (_deathPoseHeld)
                return;

            // Drive death by elapsed game time so a looping import cannot wrap GetTime() back to 0.
            _deathElapsed += deltaTime;
            float end = Mathf.Max(0.01f, clip.length - 0.03f);
            AnimationClipPlayable playable = _clipPlayables[death];

            if (_deathElapsed >= end)
            {
                playable.SetTime(end);
                playable.SetSpeed(0);
                playable.Pause();
                _deathPoseHeld = true;
            }
            else
            {
                playable.SetTime(_deathElapsed);
                playable.SetSpeed(0); // We manually scrub time to defeat clip looping.
            }
        }

        public void Dispose()
        {
            if (_graph.IsValid())
                _graph.Destroy();

            _ready = false;
            _blending = false;
            _deathLocked = false;
            _deathPoseHeld = false;
            _deathElapsed = 0f;
        }
    }
}
