using UnityEngine;

namespace ProjectZone
{
    /// <summary>
    /// Distinct procedural beds so outdoor / yard / indoor are obvious in Play Mode (#56).
    /// Replace with authored clips via Settings → Audio.
    /// </summary>
    public static class PzProceduralAmbientBeds
    {
        private const int SampleRate = 22050;
        private const float DurationSeconds = 4f;

        private static AudioClip _outdoor;
        private static AudioClip _settlementYard;
        private static AudioClip _settlementIndoor;
        private static AudioClip _industrialIndoor;
        private static AudioClip _underground;

        public static AudioClip Get(PzAmbientKind kind)
        {
            return kind switch
            {
                PzAmbientKind.SettlementYard => _settlementYard ??= BuildYard(),
                PzAmbientKind.SettlementIndoor => _settlementIndoor ??= BuildIndoorHum(),
                PzAmbientKind.IndustrialIndoor => _industrialIndoor ??= BuildIndustrial(),
                PzAmbientKind.Underground => _underground ??= BuildUnderground(),
                _ => _outdoor ??= BuildOutdoorWind()
            };
        }

        public static float DefaultVolume(PzAmbientKind kind)
        {
            return kind switch
            {
                PzAmbientKind.SettlementYard => 0.32f,
                PzAmbientKind.SettlementIndoor => 0.3f,
                PzAmbientKind.IndustrialIndoor => 0.3f,
                PzAmbientKind.Underground => 0.28f,
                _ => 0.34f
            };
        }

        public static float DefaultLowPassHz(PzAmbientKind kind)
        {
            return kind switch
            {
                PzAmbientKind.SettlementYard => 7000f,
                PzAmbientKind.SettlementIndoor => 1800f,
                PzAmbientKind.IndustrialIndoor => 2800f,
                PzAmbientKind.Underground => 900f,
                _ => 14000f
            };
        }

        private static AudioClip BuildOutdoorWind()
        {
            // Bright hissy wind — unmistakable open-air bed.
            return Build("PZ_Amb_Outdoor_v3", (t, white, brown, hiss, rng) =>
            {
                float gust = 0.55f + 0.45f * Mathf.Sin(t * 0.7f * Mathf.PI * 2f + Mathf.Sin(t * 0.23f));
                return (brown * 0.25f + white * 0.4f + hiss * 0.55f) * gust;
            });
        }

        private static AudioClip BuildYard()
        {
            // Sheltered courtyard: less hiss, soft mid drone (fenced / buildings).
            return Build("PZ_Amb_SettlementYard_v3", (t, white, brown, hiss, rng) =>
            {
                float drone = Mathf.Sin(t * 85f * Mathf.PI * 2f) * 0.12f
                              + Mathf.Sin(t * 170f * Mathf.PI * 2f) * 0.05f;
                float gust = 0.75f + 0.25f * Mathf.Sin(t * 0.35f * Mathf.PI * 2f);
                return (brown * 0.45f + white * 0.12f + hiss * 0.12f + drone) * gust;
            });
        }

        private static AudioClip BuildIndoorHum()
        {
            // Clear electrical room hum — totally different from outdoor wind.
            return Build("PZ_Amb_SettlementIndoor_v3", (t, white, brown, hiss, rng) =>
            {
                float hum = Mathf.Sin(t * 60f * Mathf.PI * 2f) * 0.42f
                            + Mathf.Sin(t * 120f * Mathf.PI * 2f) * 0.22f
                            + Mathf.Sin(t * 180f * Mathf.PI * 2f) * 0.08f;
                float room = brown * 0.12f + white * 0.03f;
                return hum + room;
            });
        }

        private static AudioClip BuildIndustrial()
        {
            return Build("PZ_Amb_Industrial_v3", (t, white, brown, hiss, rng) =>
            {
                float pulse = 0.65f + 0.35f * Mathf.Sin(t * 2.2f * Mathf.PI * 2f);
                float machine = Mathf.Sin(t * 48f * Mathf.PI * 2f) * 0.2f
                                + Mathf.Sin(t * 96f * Mathf.PI * 2f) * 0.1f;
                return (brown * 0.4f + white * 0.22f + hiss * 0.15f + machine) * pulse;
            });
        }

        private static AudioClip BuildUnderground()
        {
            return Build("PZ_Amb_Underground_v3", (t, white, brown, hiss, rng) =>
            {
                float boom = Mathf.Sin(t * 28f * Mathf.PI * 2f) * 0.35f
                             + Mathf.Sin(t * 14f * Mathf.PI * 2f) * 0.2f;
                return brown * 0.55f + boom + white * 0.04f;
            });
        }

        private delegate float SampleFn(
            float t, float white, float brown, float hiss, System.Random rng);

        private static AudioClip Build(string name, SampleFn fn)
        {
            int samples = Mathf.RoundToInt(SampleRate * DurationSeconds);
            float[] data = new float[samples];
            float brown = 0f;
            System.Random rng = new(name.GetHashCode());

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                float hiss = (float)(rng.NextDouble() * 2.0 - 1.0);
                brown = Mathf.Clamp(brown + white * 0.02f, -1f, 1f);

                float n = fn(t, white, brown, hiss, rng);

                float edge = Mathf.Min(i, samples - 1 - i) / (SampleRate * 0.08f);
                float envelope = Mathf.Clamp01(edge);
                data[i] = Mathf.Clamp(n * 0.38f * envelope, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
