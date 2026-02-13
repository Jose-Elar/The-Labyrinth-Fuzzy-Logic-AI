using UnityEngine;

public class AILODByDistance : MonoBehaviour
{
    public Transform reference;                 // Camera.main por defecto
    public float activeRadius = 25f;            // Dentro: IA completa

    [Header("Decision intervals")]
    public float nearDecisionInterval = 0.10f;  // Cerca: más reactivo
    public float farDecisionInterval = 0.30f;   // Lejos: más barato

    [Header("Behaviors to disable when far")]
    public MonoBehaviour[] expensiveBehaviors;  // Ej: AvoidTaggedObstaclesBehavior

    private SteeringManager manager;
    private float r2;

    private void Awake()
    {
        manager = GetComponent<SteeringManager>();
        r2 = activeRadius * activeRadius;

        if (reference == null && Camera.main != null)
            reference = Camera.main.transform;
    }

    private void Update()
    {
        if (reference == null || manager == null) return;

        float d2 = (reference.position - transform.position).sqrMagnitude;
        bool near = d2 <= r2;

        manager.decisionInterval = near ? nearDecisionInterval : farDecisionInterval;

        for (int i = 0; i < expensiveBehaviors.Length; i++)
        {
            if (expensiveBehaviors[i] != null)
                expensiveBehaviors[i].enabled = near;
        }
    }
}
