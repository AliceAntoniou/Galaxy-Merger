namespace Multi_1
{
    public enum EntityType
    {
        // Celestial Objects (placed on the Galaxy Grid)
        StellarNursery, StarSystem, Nebula, TinyBlackHole,
        // Planets (placed on a Star System)
        AsteroidBelt, FrozenBelt, SlingshotPlanetSpawner, ResearcherPlanet,
        // Units (spawned by planets, server only) / Don't forget to add Network Transform to the prefab
        Comet, Asteroid, SlingshotPlanet
    }
}
