using Multi_1;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Objects/UnitSO")]
public class UnitSO : ScriptableObject
{
    public EntityType unitType;
    public int health;
    public int damage;
    public float spawnSpeed;
    public float accelerationSpeed;
    public float maxSpeed;
    public float detectionRange;
    public float range;
    public float atkSpeed;
}
