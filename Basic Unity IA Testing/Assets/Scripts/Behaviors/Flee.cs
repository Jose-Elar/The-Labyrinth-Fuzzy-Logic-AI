using UnityEngine;

public class Flee : BaseBehavior
{

    public Transform threat;
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float fleeRadius = 10f;
    private Vector3 velocity;
    [SerializeField] private Vector3 desired;

   // private NPCRaycast npcRaycast;
   // private HpNPC hpNPC;

    void Start()
    {
   //     npcRaycast = GetComponent<NPCRaycast>();
  //      hpNPC = GetComponent<HpNPC>();
    }

    public override Vector3 DesiredDirectionNormalized()
    {
        desired = Vector3.zero;
        Vector3 toThreat = threat.position - transform.position;
        if (toThreat.magnitude < fleeRadius)
        {
            desired = (-toThreat.normalized) * maxSpeed;
        }

        return desired;

    }

    /*
        if (npcRaycast.isPlayerSeen() && hpNPC.getHP() < 21f)
        {
            desired = Vector3.zero;
            Vector3 toThreat = threat.position - transform.position;
            if (toThreat.magnitude < fleeRadius)
            {
                desired = (-toThreat.normalized) * maxSpeed;
            }

            return desired;
        }
        else
        {
            return Vector3.zero;
        }

    */
}
