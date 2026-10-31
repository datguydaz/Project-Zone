using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectZone
{
    /// <summary>
    /// Outskirts atmosphere entry — applies the active M7 lighting profile on enable.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    public sealed class OutskirtsAtmosphereBootstrap : MonoBehaviour
    {
        private const string OvercastDayResource = "Atmosphere/PZ_Lighting_OvercastDay";
        private const string NightResource = "Atmosphere/PZ_Lighting_Night";

        [SerializeField]
        private bool _applyOnEnable = true;

        [SerializeField]
        private PzLightingPresetId _activePreset = PzLightingPresetId.OvercastDay;

        [SerializeField]
        private PzLightingProfile _overcastDay;

        [SerializeField]
        private PzLightingProfile _night;

        [SerializeField]
        private bool _ensureAmbientAudio = true;

        [SerializeField]
        private bool _ensureDustHints = true;

        public PzLightingPresetId ActivePreset => _activePreset;

        private void OnEnable()
        {
            if (_applyOnEnable)
                ApplyAtmosphere();
        }

        private void Start()
        {
            // HQFPS player camera often spawns after atmosphere OnEnable — re-push CAS/volume.
            if (Application.isPlaying)
                StartCoroutine(ReapplyAfterPlayerCamera());
        }

        private IEnumerator ReapplyAfterPlayerCamera()
        {
            yield return null;
            yield return null;
            // EnsureHqfpsPostVolume runs inside Apply — binds Polymind ActiveVolume for pause FX.
            ApplyAtmosphere();

            // Player camera / UI may appear a moment later — one more push after player exists.
            float elapsed = 0f;
            while (elapsed < 4f && PolymindGames.Player.AllPlayers.Count == 0)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return null;
            ApplyAtmosphere();
        }

        [ContextMenu("Apply Outskirts Atmosphere")]
        public void ApplyAtmosphere()
        {
            ResolveProfiles();
            SyncActivePresetFromSettings();
            PzLightingProfile profile = GetActiveProfile();
            if (profile == null)
            {
                Debug.LogWarning(
                    "[ProjectZone][M7] No lighting profile in Resources/Atmosphere — auto-sync should create them on Editor load.",
                    this);
            }
            else
            {
                PzLightingApplicator.Apply(profile, transform);
            }

            if (_ensureAmbientAudio)
                PzAmbientAudioDirector.EnsureInScene(transform);
            PzAtmosphereTransitionDirector.EnsureInScene(transform);
            if (_ensureDustHints)
                EnsureDustHints();
        }

        private void SyncActivePresetFromSettings()
        {
            PzProjectSettings settings = PzProjectSettings.Get();
            if (settings?.Atmosphere == null)
                return;

            _activePreset = settings.Atmosphere.ActivePreset;
        }

        [ContextMenu("Apply Overcast Day")]
        public void ApplyOvercastDay()
        {
            _activePreset = PzLightingPresetId.OvercastDay;
            ApplyAtmosphere();
        }

        [ContextMenu("Apply Night")]
        public void ApplyNight()
        {
            _activePreset = PzLightingPresetId.Night;
            ApplyAtmosphere();
        }

        public void SetPreset(PzLightingPresetId preset, bool apply = true)
        {
            _activePreset = preset;
            if (apply)
                ApplyAtmosphere();
        }

        public PzLightingProfile GetActiveProfile()
        {
            return _activePreset switch
            {
                PzLightingPresetId.Night => _night != null ? _night : _overcastDay,
                _ => _overcastDay
            };
        }

        private void ResolveProfiles()
        {
            // Always prefer Resources so Editor auto-sync (code → asset) wins over stale refs.
            PzLightingProfile day = Resources.Load<PzLightingProfile>(OvercastDayResource);
            PzLightingProfile night = Resources.Load<PzLightingProfile>(NightResource);
            if (day != null)
                _overcastDay = day;
            if (night != null)
                _night = night;
        }

        private void EnsureDustHints()
        {
            Transform root = transform.Find(PzLightingApplicator.VolumeChildName);
            if (root == null)
                return;

            if (root.Find("DustHint_Settlement") != null)
                return;

            CreateDustHint(root, "DustHint_Settlement", new Vector3(0f, 3f, 0f));
            CreateDustHint(root, "DustHint_Industrial", new Vector3(90f, 4f, 8f));
        }

        private static void CreateDustHint(Transform parent, string name, Vector3 worldPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;

            var particles = go.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 0.08f;
            main.startSize = 0.35f;
            main.startColor = new Color(0.65f, 0.62f, 0.55f, 0.25f);
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.rateOverTime = 4f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(18f, 2f, 18f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")
                                            ?? Shader.Find("Particles/Standard Unlit")
                                            ?? Shader.Find("Sprites/Default"));
                renderer.sharedMaterial = mat;
            }
        }
    }
}
