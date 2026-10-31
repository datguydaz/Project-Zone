using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectZone
{
    /// <summary>
    /// Central PROJECT ZONE tuning hub. Edit via <c>Tools / PROJECT ZONE / Settings</c>.
    /// Loaded at runtime from Resources (<see cref="ResourcePath"/>).
    /// </summary>
    [CreateAssetMenu(menuName = "Project Zone/Project Settings", fileName = "PZ_ProjectSettings")]
    public sealed class PzProjectSettings : ScriptableObject
    {
        public const string ResourcePath = "PZ_ProjectSettings";
        public const string AssetPath = "Assets/_ProjectZone/Resources/PZ_ProjectSettings.asset";

        private static PzProjectSettings _cached;

        [SerializeField]
        private ZombieSettings _zombies = new();

        [SerializeField]
        private PlayerSettings _player = new();

        [SerializeField]
        private AtmosphereSettings _atmosphere = new();

        [SerializeField]
        private AudioSettings _audio = new();

        [SerializeField]
        private WorldSettings _world = new();

        [SerializeField]
        private ExpeditionSettings _expedition = new();

        public ZombieSettings Zombies => _zombies;
        public PlayerSettings Player => _player;
        public AtmosphereSettings Atmosphere => _atmosphere;
        public AudioSettings Audio => _audio;
        public WorldSettings World => _world;
        public ExpeditionSettings Expedition => _expedition;

        public static PzProjectSettings Get()
        {
            if (_cached != null)
                return _cached;

            _cached = Resources.Load<PzProjectSettings>(ResourcePath);
            return _cached;
        }

        public static void SetEditorCache(PzProjectSettings settings)
        {
            _cached = settings;
        }

        [System.Serializable]
        public sealed class ZombieSettings
        {
            [Header("Prefabs")]
            public GameObject FemaleZombiePrefab;
            public GameObject EmeraldRangedFallbackPrefab;

            [Header("Animation clips")]
            public AnimationClip IdleClip;
            public AnimationClip WalkClip;
            public AnimationClip RunClip;
            public AnimationClip AttackClip;
            public AnimationClip DeathClip;

            [Header("Melee AI")]
            public float DetectRange = 28f;
            public float MeleeRange = 1.85f;
            public float MeleeDamage = 12f;
            public float MeleeCooldown = 1.35f;
            public float WalkSpeed = 2.1f;
            public float RunSpeed = 3.6f;
            public float RunDistance = 10f;
        }

        [System.Serializable]
        public sealed class PlayerSettings
        {
            [Header("Starting loadout")]
            public GameObject StartingWeaponPickupPrefab;
            public bool GiveStartingPistol = true;

            [Header("Notes")]
            [TextArea(2, 4)]
            public string DesignerNotes =
                "HQ FPS loadout only. Camera / post FX live under Atmosphere (the volume the FPS camera actually samples).";
        }

        [System.Serializable]
        public sealed class AtmosphereSettings
        {
            [Header("Active preset")]
            [Tooltip("Overcast Day uses the sliders below. Night uses the Zone Night profile (#53).")]
            public PzLightingPresetId ActivePreset = PzLightingPresetId.OvercastDay;

            [Header("Fog backend (#54 — HAZE primary; never stack with RenderSettings fog)")]
            [Tooltip("HazeVolumetric = primary. SimpleLinear = fallback / A/B compare.")]
            public PzFogMode FogBackend = PzFogMode.HazeVolumetric;

            [Tooltip("HAZE global density (Stalzone murk — tune in Atmosphere Settings).")]
            [Range(0f, 40f)]
            public float HazeDensity = 10f;

            [Header("Fog / ambient / sun (RenderSettings) — Overcast Day / Stalzone ref")]
            public bool FogEnabled = true;
            public FogMode FogMode = FogMode.Linear;
            public Color FogColor = new(0.62f, 0.63f, 0.60f);
            public float FogStartDistance = 28f;
            public float FogEndDistance = 145f;

            public AmbientMode AmbientMode = AmbientMode.Trilight;
            public Color AmbientSky = new(0.62f, 0.63f, 0.60f);
            public Color AmbientEquator = new(0.46f, 0.45f, 0.42f);
            public Color AmbientGround = new(0.28f, 0.27f, 0.24f);
            public float AmbientIntensity = 1.55f;

            public Color SunColor = new(0.95f, 0.93f, 0.88f);
            public float SunIntensity = 0.72f;
            public Vector3 SunEuler = new(58f, -20f, 0f);

            [Header("Camera / Post FX (writes into HQFPS PostProcessing volume)")]
            public float PostExposure = 0.18f;
            public float Contrast = 6f;
            [Range(-100f, 100f)]
            public float Saturation = -22f;
            public Color ColorFilter = new(0.94f, 0.93f, 0.90f);
            [Range(0f, 1f)]
            public float Vignette = 0.12f;
            public float WhiteBalanceTemp = 2f;
            [Range(0f, 1f)]
            public float FilmGrain = 0.12f;
            [Range(0f, 1f)]
            public float BloomIntensity = 0.06f;
            [Range(0f, 1f)]
            public float ChromaticAberration = 0.012f;
            [Range(0f, 1f)]
            public float Sharpness = 0.4f;
            public float ShadowsLift = 0.22f;
            public float Highlights = -0.12f;

            [Header("Space transitions (#57 — indoor / outdoor look)")]
            [Tooltip("Lerp HAZE / ambient / exposure / sun when entering Sheltered or Indoor volumes.")]
            public bool EnableSpaceTransitions = true;

            [Range(0.05f, 4f)]
            public float SpaceBlendSeconds = 1.25f;

            [Tooltip("Extra HAZE outside Settlement so Outdoor reads thicker than the yard.")]
            [Range(1f, 2.5f)]
            public float OutdoorHazeBoost = 1.45f;

            [Tooltip("HAZE density scale inside Settlement compound (Sheltered). Lower = clearer yard.")]
            [Range(0f, 1f)]
            public float ShelteredHazeMultiplier = 0.22f;

            [Tooltip("HAZE density scale inside Hab / warehouse / tunnel (Indoor).")]
            [Range(0f, 1f)]
            public float IndoorHazeMultiplier = 0.08f;

            [Range(0.2f, 1.5f)]
            public float ShelteredAmbientMultiplier = 0.7f;

            [Range(0.2f, 1.5f)]
            public float IndoorAmbientMultiplier = 0.55f;

            [Tooltip("Added to post-exposure when Sheltered / Indoor.")]
            [Range(-1f, 1f)]
            public float IndoorExposureBias = 0.28f;

            [Range(0.1f, 1f)]
            public float ShelteredSunMultiplier = 0.55f;

            [Range(0.05f, 1f)]
            public float IndoorSunMultiplier = 0.3f;

            [Header("Weather (#58 — light rain A/B only)")]
            [Tooltip("Off = no weather. LightRain = camera-local streaks for FPS A/B. No wetness/decals.")]
            public PzWeatherMode WeatherMode = PzWeatherMode.Off;

            [Range(0f, 1f)]
            public float RainIntensity = 0.55f;
        }

        [System.Serializable]
        public sealed class AmbientBedSettings
        {
            [Tooltip("Leave empty to use the built-in procedural bed for this zone.")]
            public AudioClip Clip;

            [Tooltip("Per-bed volume (multiplied by master).")]
            [Range(0f, 1f)]
            public float Volume = 0.28f;

            [Tooltip("0 = bright/open air. 1 = heavy muffling (closed room / bunker). Higher = more muffled.")]
            [Range(0f, 1f)]
            public float Muffle = 0f;

            [Tooltip("Uncheck to silence this bed when its zone is active.")]
            public bool Enabled = true;

            /// <summary>Maps Muffle 0→1 to low-pass cutoff (~16 kHz → ~300 Hz).</summary>
            public float ResolveCutoffHz()
            {
                // Curve biased so mid Muffle is already clearly dull.
                float t = Mathf.Clamp01(Muffle);
                t = t * t; // ease-in: small values still open, high values crush highs hard
                return Mathf.Lerp(16000f, 300f, t);
            }
        }

        [System.Serializable]
        public sealed class AudioSettings
        {
            [Header("Master (#56)")]
            [Tooltip("Outdoor / indoor bed crossfade via PzAudioZone volumes.")]
            public bool EnableAmbientAudio = true;

            [Range(0f, 1f)]
            public float AmbientMasterVolume = 1f;

            [Tooltip("Seconds to crossfade when entering/leaving an audio zone.")]
            [Range(0.05f, 4f)]
            public float AmbientCrossfadeSeconds = 1.25f;

            [Header("Beds — assign clips to replace procedural placeholders")]
            public AmbientBedSettings Outdoor = new()
            {
                Volume = 0.34f,
                Muffle = 0.02f
            };

            [Tooltip("Anywhere inside the Settlement compound (simplified #56).")]
            public AmbientBedSettings Settlement = new()
            {
                Volume = 0.32f,
                Muffle = 0.25f
            };

            [Header("Optional (later)")]
            public AmbientBedSettings SettlementIndoor = new()
            {
                Volume = 0.3f,
                Muffle = 0.65f
            };

            public AmbientBedSettings IndustrialIndoor = new()
            {
                Volume = 0.3f,
                Muffle = 0.45f
            };

            public AmbientBedSettings Underground = new()
            {
                Volume = 0.28f,
                Muffle = 0.8f
            };

            public AmbientBedSettings GetBed(PzAmbientKind kind)
            {
                EnsureBedDefaults();
                return kind switch
                {
                    // SettlementYard = whole compound (current simple setup).
                    PzAmbientKind.SettlementYard => Settlement,
                    PzAmbientKind.SettlementIndoor => SettlementIndoor,
                    PzAmbientKind.IndustrialIndoor => IndustrialIndoor,
                    PzAmbientKind.Underground => Underground,
                    _ => Outdoor
                };
            }

            /// <summary>
            /// Fills missing nested beds after Unity deserializes an older settings asset.
            /// </summary>
            public void EnsureBedDefaults()
            {
                Outdoor ??= new AmbientBedSettings { Volume = 0.34f, Muffle = 0.02f };
                Settlement ??= new AmbientBedSettings { Volume = 0.32f, Muffle = 0.25f };
                SettlementIndoor ??= new AmbientBedSettings { Volume = 0.3f, Muffle = 0.65f };
                IndustrialIndoor ??= new AmbientBedSettings { Volume = 0.3f, Muffle = 0.45f };
                Underground ??= new AmbientBedSettings { Volume = 0.28f, Muffle = 0.8f };
            }
        }

        [System.Serializable]
        public sealed class WorldSettings
        {
            public bool SpawnNpcsOnStart = true;
            public bool BakeNavMeshOnStart = true;
            public bool WirePlayerTargetAdapter = true;
            public bool SpawnEmeraldRangedAtIndustrial;

            [Header("Foliage (#55 — Toby + Prefab Paint only)")]
            [Tooltip("Primary stack = Toby Rocky Hills / Juniper via Prefab Paint. No Terrain-detail dual stack.")]
            public bool TobyPrimaryFoliage = true;

            public float MinTreeSpacing = 2.5f;
            public float MinBushSpacing = 1.0f;
            public float MinGrassSpacing = 0.45f;
            public float MinFlowerSpacing = 0.6f;

            [Tooltip("Soft cap painted props per zone before a FPS review.")]
            public int MaxPaintedPropsPerZone = 800;
        }

        [System.Serializable]
        public sealed class ExpeditionSettings
        {
            [TextArea(2, 5)]
            public string DesignerNotes =
                "Contract / wallet / death-cache tuning will land here as M5 systems stabilize.";
        }
    }
}
