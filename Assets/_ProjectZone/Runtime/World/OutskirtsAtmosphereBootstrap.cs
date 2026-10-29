using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectZone
{
    /// <summary>
    /// Milestone 3 / issue #20 — lighting, fog, and ambient atmosphere for The Outskirts.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    public sealed class OutskirtsAtmosphereBootstrap : MonoBehaviour
    {
        private const string RootName = "Outskirts_Atmosphere";

        [SerializeField]
        private bool _applyOnEnable = true;

        private void OnEnable()
        {
            if (_applyOnEnable)
                ApplyAtmosphere();
        }

        [ContextMenu("Apply Outskirts Atmosphere")]
        public void ApplyAtmosphere()
        {
            ApplyRenderSettings();
            EnsureSun();
            EnsureVolume();
            EnsureAmbientAudio();
            EnsureDustHints();
        }

        private static void ApplyRenderSettings()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.48f, 0.52f, 0.55f);
            RenderSettings.fogStartDistance = 25f;
            RenderSettings.fogEndDistance = 140f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.46f, 0.52f);
            RenderSettings.ambientEquatorColor = new Color(0.28f, 0.3f, 0.32f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.11f, 0.1f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.subtractiveShadowColor = new Color(0.3f, 0.28f, 0.32f);
        }

        private void EnsureSun()
        {
            Light sun = null;
            Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    sun = lights[i];
                    break;
                }
            }

            if (sun == null)
            {
                var go = new GameObject("Directional Light");
                go.transform.SetParent(transform, false);
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.color = new Color(1f, 0.82f, 0.65f);
            sun.intensity = 0.95f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(28f, -35f, 0f);
            RenderSettings.sun = sun;
        }

        private void EnsureVolume()
        {
            Transform existing = transform.Find(RootName);
            GameObject root;
            if (existing != null)
            {
                root = existing.gameObject;
            }
            else
            {
                root = new GameObject(RootName);
                root.transform.SetParent(transform, false);
            }

            Volume volume = root.GetComponent<Volume>();
            if (volume == null)
                volume = root.AddComponent<Volume>();

            volume.isGlobal = true;
            volume.priority = 10f;

            if (volume.profile == null)
                volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            if (!volume.profile.TryGet(out ColorAdjustments color))
                color = volume.profile.Add<ColorAdjustments>(true);

            color.active = true;
            color.postExposure.overrideState = true;
            color.postExposure.value = -0.15f;
            color.contrast.overrideState = true;
            color.contrast.value = 8f;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = new Color(1f, 0.95f, 0.88f);

            if (!volume.profile.TryGet(out Vignette vignette))
                vignette = volume.profile.Add<Vignette>(true);

            vignette.active = true;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.22f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.45f;

            if (!volume.profile.TryGet(out WhiteBalance whiteBalance))
                whiteBalance = volume.profile.Add<WhiteBalance>(true);

            whiteBalance.active = true;
            whiteBalance.temperature.overrideState = true;
            whiteBalance.temperature.value = 8f;
        }

        private void EnsureAmbientAudio()
        {
            Transform root = transform.Find(RootName);
            if (root == null)
                return;

            Transform audioTransform = root.Find("AmbientAudio");
            GameObject audioGo;
            if (audioTransform != null)
            {
                audioGo = audioTransform.gameObject;
            }
            else
            {
                audioGo = new GameObject("AmbientAudio");
                audioGo.transform.SetParent(root, false);
            }

            AudioSource source = audioGo.GetComponent<AudioSource>();
            if (source == null)
                source = audioGo.AddComponent<AudioSource>();

            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.25f;
            // Clip intentionally unset — assign an Outskirts bed when audio assets exist.
        }

        private void EnsureDustHints()
        {
            Transform root = transform.Find(RootName);
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
