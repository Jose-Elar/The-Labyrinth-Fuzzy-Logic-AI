using UnityEngine;

public class ArriveAgent : BaseBehavior
{
    public Transform target;
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float slowingRadius = 3f;
    public Vector3 desired = Vector3.zero;

   // private NPCRaycast npcRaycast;
   // private HpNPC hpNPC;


    void Start()
    {
        //npcRaycast = GetComponent<NPCRaycast>();
       // hpNPC = GetComponent<HpNPC>();
    }

    public override Vector3 DesiredDirectionNormalized()
    {
        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;
        float speed = maxSpeed;
        if (distance < slowingRadius)
        {
            speed = maxSpeed * (distance / slowingRadius);
        }

        desired = toTarget.normalized * speed;

        return desired;
    }
}
/*
 if (npcRaycast.isPlayerSeen() && hpNPC.getHP() > 20f )
        {
            Vector3 toTarget = target.position - transform.position;
            float distance = toTarget.magnitude;
            float speed = maxSpeed;
            if (distance < slowingRadius)
            {
                speed = maxSpeed * (distance / slowingRadius);
            }

            desired = toTarget.normalized * speed;

            return desired;
        }
        else
        {

            return Vector3.zero;
        }
*/