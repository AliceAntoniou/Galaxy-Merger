using System.Collections;
using UnityEngine;

public class Gamberge : MonoBehaviour
{
    [Header("Properties")]
    public float moveSpeed;
    [Space]
    [SerializeField] private float attack;

    public Rigidbody2D rb;

    private void Awake()
    {
        //Debug.Log("1");
        //Debug.LogWarning("2");
        //Debug.LogError("3");

        //string feur = "feurrr";
        //Debug.Log($"debug:{feur}");
    }

    void Start()
    {
        //rb = GetComponent<Rigidbody2D>();

        StartCoroutine(Feur());
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    private IEnumerator Feur()
    {
        Debug.Log("caca");
        yield return new WaitForSeconds(1);
        Debug.Log("caca");
    }
}
