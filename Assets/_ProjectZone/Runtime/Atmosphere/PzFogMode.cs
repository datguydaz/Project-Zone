namespace ProjectZone
{
    /// <summary>M7 #54 — primary fog backend. Never stack HAZE + RenderSettings fog.</summary>
    public enum PzFogMode
    {
        /// <summary>Unity linear fog via RenderSettings (current default).</summary>
        SimpleLinear = 0,

        /// <summary>HAZE volumetric global fog (requires HazeRendererFeature on URP renderer).</summary>
        HazeVolumetric = 1
    }
}
