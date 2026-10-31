using System.Collections.Generic;
using Haze.Runtime;
using PolymindGames;
using PolymindGames.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectZone
{
    /// <summary>
    /// #57 — lerps HAZE density, ambient, exposure between Outdoor / Sheltered / Indoor spaces.
    /// </summary>
    [DefaultExecutionOrder(-15)]
    [DisallowMultipleComponent]
    public sealed class PzAtmosphereTransitionDirector : MonoBehaviour
    {
        private const string ChildName = "AtmosphereTransitionDirector";

        private static readonly List<PzAtmosphereZone> Zones = new();

        private Player _player;
        private float _nextPlayerBind;
        private float _nextZoneRefresh;
        private PzAtmosphereSpace _currentSpace = PzAtmosphereSpace.Outdoor;
        private float _blend = 1f;
        private float _hazeFrom;
        private float _hazeTo;
        private float _ambientFrom;
        private float _ambientTo;
        private float _exposureFrom;
        private float _exposureTo;
        private float _sunFrom;
        private float _sunTo;
        private float _baseHaze = 10f;
        private float _baseAmbient = 1.55f;
        private float _baseExposure = 0.18f;
        private float _baseSun = 0.72f;
        private float _blendSeconds = 1.25f;
        private bool _enabled = true;
        private Light _sun;
        private Volume _volume;

        public static PzAtmosphereTransitionDirector Instance { get; private set; }
        public PzAtmosphereSpace CurrentSpace => _currentSpace;

        public static void Register(PzAtmosphereZone zone)
        {
            if (zone != null && !Zones.Contains(zone))
                Zones.Add(zone);
        }

        public static void Unregister(PzAtmosphereZone zone)
        {
            if (zone != null)
                Zones.Remove(zone);
        }

        public static PzAtmosphereTransitionDirector EnsureInScene(Transform parent = null)
        {
            if (Instance != null)
                return Instance;

            PzAtmosphereTransitionDirector existing =
                Object.FindAnyObjectByType<PzAtmosphereTransitionDirector>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            Transform root = parent;
            if (root == null)
            {
                OutskirtsAtmosphereBootstrap atmo =
                    Object.FindAnyObjectByType<OutskirtsAtmosphereBootstrap>();
                if (atmo != null)
                    root = atmo.transform;
            }

            var go = new GameObject(ChildName);
            if (root != null)
                go.transform.SetParent(root, false);
            return go.AddComponent<PzAtmosphereTransitionDirector>();
        }

        private void Awake()
        {
            Instance = this;
            CacheTargets();
            PullBaseFromSettings();
            ApplySpaceImmediate(PzAtmosphereSpace.Outdoor);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            PullBaseFromSettings();
            if (!_enabled)
                return;

            BindPlayerIfNeeded();
            RefreshZoneCacheIfNeeded();
            CacheTargets();

            PzAtmosphereSpace target = ResolveSpace(GetListenerPosition());
            if (target != _currentSpace)
                BeginTransition(target);

            TickBlend();
        }

        private void PullBaseFromSettings()
        {
            PzProjectSettings.AtmosphereSettings atmo = PzProjectSettings.Get()?.Atmosphere;
            if (atmo == null)
                return;

            _enabled = atmo.EnableSpaceTransitions;
            _blendSeconds = Mathf.Max(0.05f, atmo.SpaceBlendSeconds);
            _baseHaze = atmo.HazeDensity;
            _baseAmbient = atmo.AmbientIntensity;
            _baseExposure = atmo.PostExposure;
            _baseSun = atmo.SunIntensity;
        }

        private void CacheTargets()
        {
            if (_sun == null)
            {
                Light[] lights = Object.FindObjectsByType<Light>(
                    FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null && lights[i].type == LightType.Directional)
                    {
                        _sun = lights[i];
                        break;
                    }
                }
            }

            Volume preferred = null;
            if (PostProcessingManager.Instance != null)
                preferred = PostProcessingManager.Instance.ActiveVolume;
            if (preferred == null)
                preferred = PzLightingApplicator.FindHqfpsPostVolume();

            if (preferred != null)
                _volume = preferred;
            else if (_volume == null)
            {
                Volume[] volumes = Object.FindObjectsByType<Volume>(
                    FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < volumes.Length; i++)
                {
                    Volume v = volumes[i];
                    if (v == null || v.sharedProfile == null)
                        continue;
                    if (v.sharedProfile.TryGet(out HazeGlobalFogVolumeComponent _))
                    {
                        _volume = v;
                        break;
                    }
                }
            }
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

        private void RefreshZoneCacheIfNeeded()
        {
            if (Time.unscaledTime < _nextZoneRefresh)
                return;
            _nextZoneRefresh = Time.unscaledTime + 1f;

            Zones.Clear();
            PzAtmosphereZone[] found = Object.FindObjectsByType<PzAtmosphereZone>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != null && found[i].isActiveAndEnabled)
                    Zones.Add(found[i]);
            }
        }

        private static PzAtmosphereSpace ResolveSpace(Vector3 listener)
        {
            PzAtmosphereZone best = null;
            for (int i = 0; i < Zones.Count; i++)
            {
                PzAtmosphereZone zone = Zones[i];
                if (zone == null || !zone.isActiveAndEnabled)
                    continue;
                if (!zone.Contains(listener))
                    continue;
                if (best == null || zone.Priority > best.Priority)
                    best = zone;
            }

            return best != null ? best.Space : PzAtmosphereSpace.Outdoor;
        }

        private void BeginTransition(PzAtmosphereSpace next)
        {
            SampleCurrent(out _hazeFrom, out _ambientFrom, out _exposureFrom, out _sunFrom);
            TargetsFor(next, out _hazeTo, out _ambientTo, out _exposureTo, out _sunTo);
            _currentSpace = next;
            _blend = 0f;
            Debug.Log(
                $"[ProjectZone][Atmosphere][#57] Space → {next} (haze {_hazeFrom:0.0}→{_hazeTo:0.0})",
                this);
        }

        private void ApplySpaceImmediate(PzAtmosphereSpace space)
        {
            _currentSpace = space;
            _blend = 1f;
            TargetsFor(space, out _hazeTo, out _ambientTo, out _exposureTo, out _sunTo);
            _hazeFrom = _hazeTo;
            _ambientFrom = _ambientTo;
            _exposureFrom = _exposureTo;
            _sunFrom = _sunTo;
            ApplySampled(_hazeTo, _ambientTo, _exposureTo, _sunTo);
        }

        private void TickBlend()
        {
            float speed = 1f / _blendSeconds;
            _blend = Mathf.MoveTowards(_blend, 1f, Time.deltaTime * speed);
            float haze = Mathf.Lerp(_hazeFrom, _hazeTo, _blend);
            float ambient = Mathf.Lerp(_ambientFrom, _ambientTo, _blend);
            float exposure = Mathf.Lerp(_exposureFrom, _exposureTo, _blend);
            float sun = Mathf.Lerp(_sunFrom, _sunTo, _blend);
            ApplySampled(haze, ambient, exposure, sun);
        }

        private void TargetsFor(
            PzAtmosphereSpace space,
            out float haze,
            out float ambient,
            out float exposure,
            out float sun)
        {
            PzProjectSettings.AtmosphereSettings atmo = PzProjectSettings.Get()?.Atmosphere;
            float outdoorBoost = atmo != null ? atmo.OutdoorHazeBoost : 1.45f;
            float shelteredHazeMul = atmo != null ? atmo.ShelteredHazeMultiplier : 0.22f;
            float indoorHazeMul = atmo != null ? atmo.IndoorHazeMultiplier : 0.08f;
            float shelteredAmbMul = atmo != null ? atmo.ShelteredAmbientMultiplier : 0.7f;
            float indoorAmbMul = atmo != null ? atmo.IndoorAmbientMultiplier : 0.55f;
            float indoorExposure = atmo != null ? atmo.IndoorExposureBias : 0.28f;
            float indoorSunMul = atmo != null ? atmo.IndoorSunMultiplier : 0.3f;
            float shelteredSunMul = atmo != null ? atmo.ShelteredSunMultiplier : 0.55f;

            switch (space)
            {
                case PzAtmosphereSpace.Indoor:
                    haze = _baseHaze * indoorHazeMul;
                    ambient = _baseAmbient * indoorAmbMul;
                    exposure = _baseExposure + indoorExposure;
                    sun = _baseSun * indoorSunMul;
                    break;
                case PzAtmosphereSpace.Sheltered:
                    haze = _baseHaze * shelteredHazeMul;
                    ambient = _baseAmbient * shelteredAmbMul;
                    exposure = _baseExposure + indoorExposure * 0.45f;
                    sun = _baseSun * shelteredSunMul;
                    break;
                default:
                    haze = _baseHaze * outdoorBoost;
                    ambient = _baseAmbient;
                    exposure = _baseExposure;
                    sun = _baseSun;
                    break;
            }
        }

        private void SampleCurrent(out float haze, out float ambient, out float exposure, out float sun)
        {
            ambient = RenderSettings.ambientIntensity;
            sun = _sun != null ? _sun.intensity : _baseSun;

            haze = _baseHaze;
            exposure = _baseExposure;
            if (_volume != null && _volume.sharedProfile != null)
            {
                if (_volume.sharedProfile.TryGet(out HazeGlobalFogVolumeComponent hazeComp) && hazeComp != null)
                    haze = hazeComp.GlobalDensityMultiplier.value;
                if (_volume.sharedProfile.TryGet(out ColorAdjustments color) && color != null)
                    exposure = color.postExposure.value;
            }
        }

        private void ApplySampled(float haze, float ambient, float exposure, float sun)
        {
            RenderSettings.ambientIntensity = ambient;

            if (_sun != null)
                _sun.intensity = sun;

            if (_volume == null || _volume.sharedProfile == null)
                return;

            VolumeProfile vp = _volume.sharedProfile;
            if (vp.TryGet(out HazeGlobalFogVolumeComponent hazeComp) && hazeComp != null)
            {
                hazeComp.active = true;
                hazeComp.GlobalDensityMultiplier.overrideState = true;
                hazeComp.GlobalDensityMultiplier.value = Mathf.Max(0f, haze);

                // Slightly clearer / cooler fog tint when sheltered/indoor so the cut reads.
                float clear = 1f - Mathf.Clamp01(haze / Mathf.Max(0.01f, _baseHaze * 1.45f));
                Color outdoorFog = new Color(0.62f, 0.63f, 0.60f);
                Color clearFog = new Color(0.48f, 0.5f, 0.52f);
                hazeComp.AmbientColor.overrideState = true;
                hazeComp.AmbientColor.value = Color.Lerp(outdoorFog, clearFog, clear);
            }

            if (vp.TryGet(out ColorAdjustments color) && color != null)
            {
                color.postExposure.overrideState = true;
                color.postExposure.value = exposure;
            }
        }
    }
}
