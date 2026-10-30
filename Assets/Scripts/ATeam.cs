using UnityEngine;

public enum Team
{
    player,
    enemy
}

public class ATeam : MonoBehaviour
{
    [SerializeField] public Team team;
    [SerializeField] public BlackHole blackhole;
    public Team GetTeam()
    {
        return team;
    }
    public LayerMask GetMask()
    {
        return (1 << gameObject.layer);
    }
    public void SetTeam(Team t)
    {
        team = t;
        
        if (team == Team.player)
        {
            gameObject.layer = LayerMask.NameToLayer("TeamPlayer");
        } else
        {
            gameObject.layer = LayerMask.NameToLayer("TeamEnemy");
        }
    }
}

