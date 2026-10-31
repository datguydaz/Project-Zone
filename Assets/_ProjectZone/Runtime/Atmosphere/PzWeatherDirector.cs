using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// #58 — optional camera-local light rain (particle only). No splash/wetness/decals.
    /// Skips emission while atmosphere space is Indoor.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    [DisallowMultipleComponent]
    public sealed class PzWeatherDirector : MonoBehaviour
    {
        private const string ChildName = "WeatherDirector";
        private const string RainChildName = "LightRain";

        private ParticleSystem _rain;
        private Transform _follow;
        private PzWeatherMode _mode = PzWeatherMode.Off;
        private float _intensity = 0.55f;

        public static PzWeatherDirector Instance { get; private set; }

        public static PzWeatherDirector EnsureInScene(Transform parent = null)
        {
            if (Instance != null)
                return Instance;

            PzWeatherDirector existing = Object.FindAnyObjectByType<PzWeatherDirector>();
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
            return go.AddComponent<PzWeatherDirector>();
        }

        private void Awake()
        {
            Instance = this;
            EnsureRainSystem();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void LateUpdate()
        {
            PullSettings();
            BindFollow();
            if (_follow != null)
                transform.position = _follow.position + Vector3.up * 12f;

            bool wantRain = _mode == PzWeatherMode.LightRain && !IsIndoor();
            ApplyRain(wantRain);
        }

        private void PullSettings()
        {
            PzProjectSettings.AtmosphereSettings atmo = PzProjectSettings.Get()?.Atmosphere;
            if (atmo == null)
                return;
            _mode = atmo.WeatherMode;
            _intensity = Mathf.Clamp01(atmo.RainIntensity);
        }

        private void BindFollow()
        {
            if (_follow != null)
                return;
            if (Camera.main != null)
                _follow = Camera.main.transform;
        }

        private static bool IsIndoor()
        {
            PzAtmosphereTransitionDirector atmo = PzAtmosphereTransitionDirector.Instance;
            return atmo != null && atmo.CurrentSpace == PzAtmosphereSpace.Indoor;
        }

        private void EnsureRainSystem()
        {
            if (_rain != null)
                return;

            Transform existing = transform.Find(RainChildName);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(RainChildName);
                go.transform.SetParent(transform, false);
            }

            _rain = go.GetComponent<ParticleSystem>();
            if (_rain == null)
                _rain = go.AddComponent<ParticleSystem>();

            var main = _rain.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = 1.1f;
            main.startSpeed = 14f;
            main.startSize = 0.035f;
            main.startColor = new Color(0.65f, 0.68f, 0.72f, 0.55f);
            main.maxParticles = 800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1.2f;

            var emission = _rain.emission;
            emission.rateOverTime = 0f;

            var shape = _rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(28f, 1f, 28f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.lengthScale = 4f;
                renderer.velocityScale = 0.08f;
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")
                                            ?? Shader.Find("Particles/Standard Unlit")
                                            ?? Shader.Find("Sprites/Default"));
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", new Color(0.7f, 0.72f, 0.75f, 0.45f));
                renderer.sharedMaterial = mat;
            }
        }

        private void ApplyRain(bool on)
        {
            if (_rain == null)
                return;

            var emission = _rain.emission;
            float rate = on ? Mathf.Lerp(80f, 420f, _intensity) : 0f;
            emission.rateOverTime = rate;

            if (on && !_rain.isPlaying)
                _rain.Play();
            else if (!on && _rain.isPlaying && rate <= 0.01f)
                _rain.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
