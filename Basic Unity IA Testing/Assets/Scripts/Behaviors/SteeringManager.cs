using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SteeringManager : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float maxForce = 10f;

    [Header("Optimization: Tick")]
    [Tooltip("Cada cu�ntos segundos se recalculan los behaviors. 0 = cada frame.")]
    public float decisionInterval = 0.0f;

    public Vector3 velocity;

    private List<BaseBehavior> steeringBehaviors = new List<BaseBehavior>();
    private CharacterController cc;

    private float decisionTimer = 0f;
    private Vector3 cachedDesiredVelocity = Vector3.zero;



    //Variables de Cambio
    private NPCRaycast npcRaycast;
    private HpNPC hpNPC;

    //Variables de Calculo de Pesos
    private float targetFlee;
    private float targetArrive;
    private float targetWander;

    public float wanderBaseWeight = 0.2f;
    public float weightSmooth = 5f;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        steeringBehaviors = GetComponents<BaseBehavior>().ToList();
        steeringBehaviors.Sort();

        npcRaycast = GetComponent<NPCRaycast>();
        hpNPC = GetComponent<HpNPC>();
    }

    private void Update()
    {
        CalculateNewWeights();
        // Recalcular solo cuando toca (o cada frame si decisionInterval = 0)
        if (decisionInterval <= 0f)
        {
            cachedDesiredVelocity = ComputeDesiredVelocity();
        }
        else
        {
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f)
            {
                decisionTimer = decisionInterval;
                cachedDesiredVelocity = ComputeDesiredVelocity();
            }
        }

        // Mover cada frame usando la �ltima decisi�n
        MoveAgent(cachedDesiredVelocity);
    }

    private void CalculateNewWeights()
    {
        float hpNorm = Mathf.Clamp01(hpNPC.getHP() / hpNPC.getMaxHP());
        float hpLow = 1f - hpNorm;
        float hpHigh = hpNorm;

        float visibility = npcRaycast.isPlayerSeen() ? 1f : 0f;

        targetFlee = hpLow * visibility;
        targetArrive = hpHigh * visibility ;
        targetWander = wanderBaseWeight * (1f - visibility);


        foreach (BaseBehavior b in steeringBehaviors)
        {
            float newWeight;
            if (b is Flee)
            {
                newWeight = Mathf.Lerp(b.weightFactor, targetFlee, Time.deltaTime * weightSmooth);
                b.setWeightFactor(Mathf.Clamp01(newWeight));
            }
            else if (b is ArriveAgent)
            {
                newWeight = Mathf.Lerp(b.weightFactor, targetArrive, Time.deltaTime * weightSmooth);
                b.setWeightFactor(Mathf.Clamp01(newWeight));
            }
            else if (b is WanderPatrolBehavior)
            {
                newWeight = Mathf.Lerp(b.weightFactor, targetWander, Time.deltaTime * weightSmooth);
                b.setWeightFactor(Mathf.Clamp01(newWeight));
            }

        }
    }

    private Vector3 ComputeDesiredVelocity()
    {
        Vector3 result = Vector3.zero;

        foreach (BaseBehavior behavior in steeringBehaviors)
        {
            if (!behavior.enabled) continue;
            result += behavior.DesiredDirectionNormalized() * behavior.weightFactor;
        }

        result.y = 0f;

        return (result.sqrMagnitude > 0.0001f)
            ? result.normalized * maxSpeed
            : Vector3.zero;
    }

    private void MoveAgent(Vector3 desiredVelocity)
    {

        Vector3 steering = desiredVelocity - velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);

        velocity = Vector3.ClampMagnitude(velocity + steering * Time.deltaTime, maxSpeed);
        velocity.y = 0f;

        Vector3 delta = velocity * Time.deltaTime;
        delta.y = -2f * Time.deltaTime;

        if (cc != null)
            cc.Move(delta);
        else
            transform.position += delta;

        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        if (flatVel.sqrMagnitude > 0.0001f)
            transform.forward = flatVel.normalized;
    }
}
