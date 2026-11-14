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
    private void Start()
    {
        InvokeRepeating(nameof(Production), productionRate, productionRate);
    }

    private void Update()
    {

    }
}
