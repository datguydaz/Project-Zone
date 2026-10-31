namespace ProjectZone
{
    /// <summary>
    /// Ambient bed kinds for Outskirts audio zones (#56).
    /// Values 0–3 kept stable for existing scene serialization; Yard appended.
    /// </summary>
    public enum PzAmbientKind
    {
        Outdoor = 0,
        SettlementIndoor = 1,
        IndustrialIndoor = 2,
        Underground = 3,
        SettlementYard = 4
    }
}
