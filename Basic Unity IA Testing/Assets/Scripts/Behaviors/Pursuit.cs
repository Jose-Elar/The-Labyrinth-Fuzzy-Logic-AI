using UnityEngine;

public class PursuitAgent : BaseBehavior
{
    public Transform target;
    public Rigidbody targetRb;
    public float maxSpeed = 6f;
    public float maxForce = 10f;
    private Vector3 velocity;

    public override Vector3 DesiredDirectionNormalized()
    {
        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;
        float speed = velocity.magnitude;
        float prediction = (speed <= distance / 2f) ? 1f : distance / speed;
        Vector3 futurePosition = target.position + targetRb.linearVelocity * prediction;
        Vector3 desired = (futurePosition - transform.position).normalized * maxSpeed;

        return desired;
    }
}