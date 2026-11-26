namespace Multi_1
{
    public enum EntityType
    {
        None = 0,
        // Celestial Objects (placed on the Galaxy Grid)
        StellarNursery, StarSystem, Nebula, TinyBlackHole,
        // Planets (placed on a Star System)
        AsteroidBelt, FrozenBelt, SlingshotPlanetSpawner, ResearcherPlanet,
        // Units (spawned by planets, server only)
        Comet, Asteroid, SlingshotPlanet
    }

    public static class EntityTypeExtensions
    {
        /// <summary>Placed on a free tile of the Galaxy Grid.</summary>
        public static bool IsCelestial(this EntityType t) =>
            t == EntityType.StellarNursery || t == EntityType.StarSystem ||
            t == EntityType.Nebula || t == EntityType.TinyBlackHole;

        /// <summary>Added to a Star System.</summary>
        public static bool IsPlanet(this EntityType t) =>
            t == EntityType.AsteroidBelt || t == EntityType.FrozenBelt ||
            t == EntityType.SlingshotPlanetSpawner || t == EntityType.ResearcherPlanet;

        /// <summary>Spawned by a planet (server only).</summary>
        public static bool IsUnit(this EntityType t) =>
            t == EntityType.Comet || t == EntityType.Asteroid || t == EntityType.SlingshotPlanet;
    }
}
