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
}
