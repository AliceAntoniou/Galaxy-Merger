using UnityEngine;

public class Nebula : CelestialObject
{
    [SerializeField] private float productionRate;
    [SerializeField] private int productionNbr;

    private void Production()
    {
        if (blackhole.stardust >= blackhole.GetMaxStardust()) return;
        blackhole.stardust += productionNbr;
    }
    void Start()
    {
        InvokeRepeating("Production", productionRate, productionRate);
    }

    void Update()
    {

    }
}
