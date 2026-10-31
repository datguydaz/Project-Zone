using System.Collections.Generic;
using PolymindGames;
using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Crossfades outdoor / indoor ambient beds from <see cref="PzAudioZone"/> overlaps (#56).
    /// Beds / volumes / low-pass come from <see cref="PzProjectSettings.Audio"/>.
    /// </summary>
    [DefaultExecutionOrder(-20)]
    [DisallowMultipleComponent]
    public sealed class PzAmbientAudioDirector : MonoBehaviour
    {
        private const string ChildName = "AmbientAudioDirector";

        private static readonly List<PzAudioZone> Zones = new();

        private AudioSource _a;
        private AudioSource _b;
        private AudioLowPassFilter _lpA;
        private AudioLowPassFilter _lpB;
        private bool _aIsActive = true;
        private PzAmbientKind _currentKind = PzAmbientKind.Outdoor;
        private float _blend = 1f;
        private float _fadeOutStartVolume;
        private Player _player;
        private float _nextPlayerBind;
        private float _nextZoneRefresh;
        private AudioClip _activeClip;
        private bool _enabledFromSettings = true;
        private float _masterVolume = 1f;
        private float _crossfadeSeconds = 1.25f;

        public static PzAmbientAudioDirector Instance { get; private set; }
        public PzAmbientKind CurrentKind => _currentKind;

        public static void Register(PzAudioZone zone)
        {
            if (zone != null && !Zones.Contains(zone))
                Zones.Add(zone);
        }

        public static void Unregister(PzAudioZone zone)
        {
            if (zone != null)
                Zones.Remove(zone);
        }

        public static PzAmbientAudioDirector EnsureInScene(Transform parent = null)
        {
            if (Instance != null)
                return Instance;

            PzAmbientAudioDirector existing = Object.FindAnyObjectByType<PzAmbientAudioDirector>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            Transform root = parent;
            if (root == null)
            {
                OutskirtsAtmosphereBootstrap atmo = Object.FindAnyObjectByType<OutskirtsAtmosphereBootstrap>();
                if (atmo != null)
                    root = atmo.transform;
            }

            var go = new GameObject(ChildName);
            if (root != null)
                go.transform.SetParent(root, false);
            return go.AddComponent<PzAmbientAudioDirector>();
        }

        private void Awake()
        {
            Instance = this;
            EnsureSources();
            PullSettings();
            ApplyKindImmediate(PzAmbientKind.Outdoor);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            PullSettings();
            if (!_enabledFromSettings)
            {
                Silence();
                return;
            }

            BindPlayerIfNeeded();
            RefreshZoneCacheIfNeeded();
            Vector3 listener = GetListenerPosition();
            PzAmbientKind target = ResolveKind(listener);
            if (target != _currentKind)
                BeginCrossfade(target);
            else
                RefreshActiveBedIfClipChanged();

            TickCrossfade();
        }

        private void RefreshZoneCacheIfNeeded()
        {
            if (Time.unscaledTime < _nextZoneRefresh)
                return;
            _nextZoneRefresh = Time.unscaledTime + 1f;

            Zones.Clear();
            PzAudioZone[] found = Object.FindObjectsByType<PzAudioZone>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null && found[i].isActiveAndEnabled)
                    Zones.Add(found[i]);
            }
        }

        private void PullSettings()
        {
            PzProjectSettings.AudioSettings audio = PzProjectSettings.Get()?.Audio;
            if (audio == null)
                return;

            _enabledFromSettings = audio.EnableAmbientAudio;
            _masterVolume = audio.AmbientMasterVolume;
            _crossfadeSeconds = Mathf.Max(0.05f, audio.AmbientCrossfadeSeconds);
        }

        private void EnsureSources()
        {
            if (_a != null && _b != null)
                return;

            _a = CreateChannel("Bed_A", out _lpA);
            _b = CreateChannel("Bed_B", out _lpB);
        }

        private AudioSource CreateChannel(string name, out AudioLowPassFilter lowPass)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            AudioSource src = child.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = 0f;
            src.priority = 64;
            lowPass = child.AddComponent<AudioLowPassFilter>();
            lowPass.cutoffFrequency = 16000f;
            lowPass.lowpassResonanceQ = 1.6f;
            return src;
        }

        private void BindPlayerIfNeeded()
        {
            if (_player != null)
                return;
            if (Time.unscaledTime < _nextPlayerBind)
                return;
            _nextPlayerBind = Time.unscaledTime + 0.5f;
            if (Player.AllPlayers.Count > 0)
                _player = Player.AllPlayers[0];
        }

        private Vector3 GetListenerPosition()
        {
            if (_player != null)
                return _player.transform.position;
            if (Camera.main != null)
                return Camera.main.transform.position;
            return transform.position;
        }

        private static PzAmbientKind ResolveKind(Vector3 listener)
        {
            PzAudioZone best = null;
            for (int i = 0; i < Zones.Count; i++)
            {
                PzAudioZone zone = Zones[i];
                if (zone == null || !zone.isActiveAndEnabled)
                    continue;
                if (!zone.Contains(listener))
                    continue;
                if (best == null || zone.Priority > best.Priority)
                    best = zone;
            }

            return best != null ? best.Kind : PzAmbientKind.Outdoor;
        }

        private void BeginCrossfade(PzAmbientKind next)
        {
            AudioSource outgoing = _aIsActive ? _a : _b;
            _fadeOutStartVolume = outgoing != null ? outgoing.volume : 0f;

            _currentKind = next;
            _blend = 0f;
            _aIsActive = !_aIsActive;

            AudioSource incoming = _aIsActive ? _a : _b;
            AudioLowPassFilter incomingLp = _aIsActive ? _lpA : _lpB;
            ConfigureSource(incoming, incomingLp, next);
            incoming.volume = 0f;

            string label = next == PzAmbientKind.SettlementYard ? "Settlement" : next.ToString();
            Debug.Log($"[ProjectZone][Audio][#56] Ambient → {label}", this);
        }

        private void ApplyKindImmediate(PzAmbientKind kind)
        {
            _currentKind = kind;
            _blend = 1f;
            _aIsActive = true;
            ConfigureSource(_a, _lpA, kind);
            _a.volume = BedVolume(kind);
            if (!_a.isPlaying && BedEnabled(kind))
                _a.Play();
            _b.volume = 0f;
            if (_b.isPlaying)
                _b.Stop();
        }

        private void RefreshActiveBedIfClipChanged()
        {
            AudioClip wanted = ResolveClip(_currentKind);
            if (wanted == _activeClip)
            {
                // Still push live low-pass / mute from Settings while staying in-zone.
                AudioSource active = _aIsActive ? _a : _b;
                AudioLowPassFilter lp = _aIsActive ? _lpA : _lpB;
                ApplyFilter(lp, _currentKind);
                if (!BedEnabled(_currentKind))
                {
                    active.volume = 0f;
                    if (active.isPlaying)
                        active.Stop();
                }
                else if (!active.isPlaying && wanted != null)
                {
                    active.clip = wanted;
                    active.Play();
                }

                return;
            }

            AudioSource src = _aIsActive ? _a : _b;
            AudioLowPassFilter filter = _aIsActive ? _lpA : _lpB;
            float vol = src.volume;
            ConfigureSource(src, filter, _currentKind);
            src.volume = vol;
        }

        private void TickCrossfade()
        {
            float speed = 1f / Mathf.Max(0.05f, _crossfadeSeconds);
            _blend = Mathf.MoveTowards(_blend, 1f, Time.deltaTime * speed);

            AudioSource active = _aIsActive ? _a : _b;
            AudioSource fadeOut = _aIsActive ? _b : _a;
            float targetVol = BedVolume(_currentKind);

            active.volume = Mathf.Lerp(0f, targetVol, _blend);
            fadeOut.volume = Mathf.Lerp(_fadeOutStartVolume, 0f, _blend);
            ApplyFilter(_aIsActive ? _lpA : _lpB, _currentKind);

            if (_blend >= 1f && fadeOut.isPlaying && fadeOut.volume <= 0.001f)
            {
                fadeOut.Stop();
                fadeOut.volume = 0f;
            }
        }

        private void ConfigureSource(AudioSource src, AudioLowPassFilter lowPass, PzAmbientKind kind)
        {
            AudioClip clip = ResolveClip(kind);
            _activeClip = clip;
            if (src.clip != clip)
                src.clip = clip;
            ApplyFilter(lowPass, kind);

            if (!BedEnabled(kind) || clip == null)
            {
                if (src.isPlaying)
                    src.Stop();
                return;
            }

            if (!src.isPlaying)
                src.Play();
        }

        private void ApplyFilter(AudioLowPassFilter lowPass, PzAmbientKind kind)
        {
            if (lowPass == null)
                return;
            lowPass.cutoffFrequency = ResolveCutoffHz(kind);
            lowPass.lowpassResonanceQ = 1.6f;
        }

        private AudioClip ResolveClip(PzAmbientKind kind)
        {
            PzProjectSettings.AmbientBedSettings bed = GetBed(kind);
            if (bed?.Clip != null)
                return bed.Clip;
            return PzProceduralAmbientBeds.Get(kind);
        }

        private float ResolveCutoffHz(PzAmbientKind kind)
        {
            PzProjectSettings.AmbientBedSettings bed = GetBed(kind);
            if (bed != null)
                return bed.ResolveCutoffHz();
            return PzProceduralAmbientBeds.DefaultLowPassHz(kind);
        }

        private bool BedEnabled(PzAmbientKind kind)
        {
            PzProjectSettings.AmbientBedSettings bed = GetBed(kind);
            return bed == null || bed.Enabled;
        }

        private float BedVolume(PzAmbientKind kind)
        {
            if (!BedEnabled(kind))
                return 0f;

            PzProjectSettings.AmbientBedSettings bed = GetBed(kind);
            float bedVol = bed != null ? bed.Volume : PzProceduralAmbientBeds.DefaultVolume(kind);
            return Mathf.Clamp01(_masterVolume * bedVol);
        }

        private static PzProjectSettings.AmbientBedSettings GetBed(PzAmbientKind kind)
        {
            return PzProjectSettings.Get()?.Audio?.GetBed(kind);
        }

        private void Silence()
        {
            if (_a != null)
            {
                _a.volume = 0f;
                if (_a.isPlaying)
                    _a.Stop();
            }

            if (_b != null)
            {
                _b.volume = 0f;
                if (_b.isPlaying)
                    _b.Stop();
            }
        }
    }
}
