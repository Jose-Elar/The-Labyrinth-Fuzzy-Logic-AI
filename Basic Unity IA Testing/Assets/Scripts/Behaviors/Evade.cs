using Unity.VisualScripting;
using UnityEngine;

public class EvadeAgent : BaseBehavior 
{
    public Transform threat;
    public Rigidbody threatRb;
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float evadeRadius = 10f;
    private Vector3 velocity;
    private Vector3 desired;

    private SteeringManager manager;

    private void Awake()
    {
        manager = GetComponent<SteeringManager>();
    }

    public override Vector3 DesiredDirectionNormalized()
    {
        desired = Vector3.zero;
        Vector3 toThreat = threatRb.position - transform.position;
        if (toThreat.magnitude < evadeRadius)
        {
            Debug.Log("EVADE");
            float prediction = toThreat.magnitude / (manager.velocity.magnitude + threatRb.linearVelocity.magnitude); //velocity.magnitude + threatRb.linearVelocity.magnitude
            Vector3 futurePosition = threat.position + threatRb.linearVelocity * prediction;
            desired = (transform.position - futurePosition).normalized * maxSpeed;
        }
        return desired;
    }

}

//recordar modificar la velocidad fuera del radio para casos de solo evade para que frene
