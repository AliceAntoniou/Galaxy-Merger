using UnityEngine;

public class StellarNursery: CelestialObject
{
    [SerializeField] private float productionRate;

    private void Production()
    {
        blackhole.protoStars += 1;
    }
    void Start()
    {
        InvokeRepeating("Production", productionRate, productionRate);
    }

    private new void Update()
    {
        base.Update();
    }
}
