using UnityEngine;

public class NPCRaycast : MonoBehaviour
{
    private HpNPC hpNPC;
    public Transform player;
    public float viewDistance;

    [SerializeField] private bool playerSeen = false;

    void Start()
    {
        hpNPC = GetComponent<HpNPC>();       
    }

    void Update()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, viewDistance) )
        {
            if (hit.collider.CompareTag("Player") )
            {
                playerSeen = true;
                hpNPC.reduceHp(10f * Time.deltaTime);
            }
            else
            {
                playerSeen = false;
            }
        }
    }

    public bool isPlayerSeen()
    {
        return playerSeen;
    }   
}
