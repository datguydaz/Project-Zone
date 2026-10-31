using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectZone
{
    public enum PzLightingPresetId
    {
        OvercastDay = 0,
        Night = 1
    }

    /// <summary>
    /// M7 lighting preset — Zone / Stalker-leaning Outskirts looks.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PZ_LightingProfile",
        menuName = "PROJECT ZONE/Atmosphere/Lighting Profile",
        order = 10)]
    public sealed class PzLightingProfile : ScriptableObject
    {
        [SerializeField]
        private PzLightingPresetId _presetId = PzLightingPresetId.OvercastDay;

        [SerializeField]
        private string _displayName = "Zone Overcast";

        [TextArea(2, 6)]
        [SerializeField]
        private string _notes =
            "Desaturated Zone day: dense fog, crushed midtones, film grain. HAZE still #54.";

        [Header("Fog (RenderSettings — HAZE deferred to #54)")]
        [SerializeField]
        private bool _fogEnabled = true;

        [SerializeField]
        private FogMode _fogMode = FogMode.Linear;

        [SerializeField]
        private Color _fogColor = new(0.42f, 0.44f, 0.43f);

        [SerializeField]
        private float _fogStart = 12f;

        [SerializeField]
        private float _fogEnd = 95f;

        [Header("Ambient")]
        [SerializeField]
        private AmbientMode _ambientMode = AmbientMode.Trilight;

        [SerializeField]
        private Color _ambientSky = new(0.32f, 0.34f, 0.36f);

        [SerializeField]
        private Color _ambientEquator = new(0.22f, 0.22f, 0.2f);

        [SerializeField]
        private Color _ambientGround = new(0.1f, 0.09f, 0.08f);

        [SerializeField]
        private float _ambientIntensity = 0.9f;

        [Header("Sun")]
        [SerializeField]
        private Color _sunColor = new(0.85f, 0.78f, 0.62f);

        [SerializeField]
        private float _sunIntensity = 0.55f;

        [SerializeField]
        private Vector3 _sunEuler = new(18f, -25f, 0f);

        [SerializeField]
        private LightShadows _sunShadows = LightShadows.Soft;

        [SerializeField]
        [Range(0f, 1f)]
        private float _shadowStrength = 0.88f;

        [Header("URP Volume grade")]
        [SerializeField]
        private float _postExposure = -0.35f;

        [SerializeField]
        private float _contrast = 18f;

        [SerializeField]
        [Range(-100f, 100f)]
        private float _saturation = -28f;

        [SerializeField]
        private Color _colorFilter = new(0.9f, 0.92f, 0.88f);

        [SerializeField]
        private float _vignette = 0.32f;

        [SerializeField]
        private float _whiteBalanceTemp = -8f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _filmGrain = 0.28f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _bloomIntensity = 0.08f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _chromaticAberration = 0.05f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _sharpness = 0.35f;

        [SerializeField]
        private float _shadowsStart = -0.15f;

        [SerializeField]
        private float _highlights = -0.1f;

        public PzLightingPresetId PresetId => _presetId;
        public string DisplayName => _displayName;
        public string Notes => _notes;

        public bool FogEnabled => _fogEnabled;
        public FogMode FogMode => _fogMode;
        public Color FogColor => _fogColor;
        public float FogStart => _fogStart;
        public float FogEnd => _fogEnd;

        public AmbientMode AmbientMode => _ambientMode;
        public Color AmbientSky => _ambientSky;
        public Color AmbientEquator => _ambientEquator;
        public Color AmbientGround => _ambientGround;
        public float AmbientIntensity => _ambientIntensity;

        public Color SunColor => _sunColor;
        public float SunIntensity => _sunIntensity;
        public Vector3 SunEuler => _sunEuler;
        public LightShadows SunShadows => _sunShadows;
        public float ShadowStrength => _shadowStrength;

        public float PostExposure => _postExposure;
        public float Contrast => _contrast;
        public float Saturation => _saturation;
        public Color ColorFilter => _colorFilter;
        public float Vignette => _vignette;
        public float WhiteBalanceTemp => _whiteBalanceTemp;
        public float FilmGrain => _filmGrain;
        public float BloomIntensity => _bloomIntensity;
        public float ChromaticAberration => _chromaticAberration;
        public float Sharpness => _sharpness;
        public float ShadowsStart => _shadowsStart;
        public float Highlights => _highlights;

        /// <summary>Copies Atmosphere Settings (fog/sun + post FX) grade into this lighting profile.</summary>
        public void ApplyAtmosphereCameraSettings(PzProjectSettings.AtmosphereSettings atmosphere)
        {
            if (atmosphere == null)
                return;

            _postExposure = atmosphere.PostExposure;
            _contrast = atmosphere.Contrast;
            _saturation = atmosphere.Saturation;
            _colorFilter = atmosphere.ColorFilter;
            _vignette = atmosphere.Vignette;
            _whiteBalanceTemp = atmosphere.WhiteBalanceTemp;
            _filmGrain = atmosphere.FilmGrain;
            _bloomIntensity = atmosphere.BloomIntensity;
            _chromaticAberration = atmosphere.ChromaticAberration;
            _sharpness = atmosphere.Sharpness;
            _shadowsStart = atmosphere.ShadowsLift;
            _highlights = atmosphere.Highlights;

            _fogEnabled = atmosphere.FogEnabled;
            _fogMode = atmosphere.FogMode;
            _fogColor = atmosphere.FogColor;
            _fogStart = atmosphere.FogStartDistance;
            _fogEnd = atmosphere.FogEndDistance;
            _ambientMode = atmosphere.AmbientMode;
            _ambientSky = atmosphere.AmbientSky;
            _ambientEquator = atmosphere.AmbientEquator;
            _ambientGround = atmosphere.AmbientGround;
            _ambientIntensity = atmosphere.AmbientIntensity;
            _sunColor = atmosphere.SunColor;
            _sunIntensity = atmosphere.SunIntensity;
            _sunEuler = atmosphere.SunEuler;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Zone / Stalker-leaning overcast — denser fog, desat, low sun, grain.
        /// </summary>
        public void EditorConfigureOvercastDay()
        {
            _presetId = PzLightingPresetId.OvercastDay;
            _displayName = "Zone Overcast";
            // Locked to Stalzone / Stalcraft daytime refs (developer gameplay screenshots):
            // flat grey overcast, soft sun disc in cloud, muddy desat, dim but fully readable world.
            _notes =
                "Zone Overcast v6 — Stalzone day ref: flat overcast, soft sun-in-cloud, muddy grey-brown " +
                "desat, distance haze (not soup), world always readable. HAZE = #54.";
            _fogEnabled = true;
            _fogMode = FogMode.Linear;
            _fogColor = new Color(0.62f, 0.63f, 0.60f);
            _fogStart = 28f;
            _fogEnd = 145f;
            _ambientMode = AmbientMode.Trilight;
            _ambientSky = new Color(0.62f, 0.63f, 0.60f);
            _ambientEquator = new Color(0.46f, 0.45f, 0.42f);
            _ambientGround = new Color(0.28f, 0.27f, 0.24f);
            _ambientIntensity = 1.55f;
            // Soft bright disc through cloud — low shadow punch
            _sunColor = new Color(0.95f, 0.93f, 0.88f);
            _sunIntensity = 0.72f;
            _sunEuler = new Vector3(58f, -20f, 0f);
            _sunShadows = LightShadows.Soft;
            _shadowStrength = 0.22f;
            _postExposure = 0.18f;
            _contrast = 6f;
            _saturation = -22f;
            _colorFilter = new Color(0.94f, 0.93f, 0.90f);
            _vignette = 0.12f;
            _whiteBalanceTemp = 2f;
            _filmGrain = 0.12f;
            _bloomIntensity = 0.06f;
            _chromaticAberration = 0.012f;
            _sharpness = 0.4f;
            _shadowsStart = 0.22f;
            _highlights = -0.12f;
        }

        public void EditorConfigureNight()
        {
            _presetId = PzLightingPresetId.Night;
            _displayName = "Zone Night";
            _notes =
                "Zone Night v2 (#53): cold moonlit murk — dark Stalzone night, but near geometry " +
                "and ground still read (not pure black void). Combat ~40 m.";
            _fogEnabled = true;
            _fogMode = FogMode.Linear;
            _fogColor = new Color(0.14f, 0.16f, 0.20f);
            _fogStart = 10f;
            _fogEnd = 80f;
            _ambientMode = AmbientMode.Trilight;
            _ambientSky = new Color(0.16f, 0.19f, 0.24f);
            _ambientEquator = new Color(0.11f, 0.12f, 0.14f);
            _ambientGround = new Color(0.06f, 0.06f, 0.07f);
            _ambientIntensity = 1.15f;
            _sunColor = new Color(0.60f, 0.70f, 0.90f);
            _sunIntensity = 0.42f;
            _sunEuler = new Vector3(22f, 155f, 0f);
            _sunShadows = LightShadows.Soft;
            _shadowStrength = 0.65f;
            _postExposure = 0.05f;
            _contrast = 12f;
            _saturation = -28f;
            _colorFilter = new Color(0.80f, 0.86f, 0.96f);
            _vignette = 0.24f;
            _whiteBalanceTemp = -12f;
            _filmGrain = 0.2f;
            _bloomIntensity = 0.1f;
            _chromaticAberration = 0.02f;
            _sharpness = 0.5f;
            _shadowsStart = 0.12f;
            _highlights = -0.05f;
        }
#endif
    }
}
