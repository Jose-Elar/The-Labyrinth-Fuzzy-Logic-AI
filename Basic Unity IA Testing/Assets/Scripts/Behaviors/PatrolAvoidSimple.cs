using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PatrolAvoidSimple : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3.5f;
    public float turnSpeed = 360f; // grados/seg
    public bool keepOnGround = true;

    [Header("Obstacle detection")]
    public float detectDistance = 1.8f;
    public float detectRadius = 0.35f;
    public LayerMask obstacleMask = ~0; // por defecto, todo
    public float directionChangeCooldown = 0.25f;

    [Header("Randomness")]
    [Tooltip("Ángulo máximo de giro cuando detecta obstáculo (en grados).")]
    public float maxTurnAngle = 140f;

    [Tooltip("Pequeño cambio aleatorio cada cierto tiempo para que no se sincronicen todos.")]
    public bool addSmallWander = true;
    public float wanderInterval = 1.5f;
    public float wanderAngle = 15f;

    CharacterController cc;
    Vector3 moveDir;
    float cooldown;
    float wanderTimer;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        // Dirección inicial aleatoria en XZ
        Vector2 rnd = Random.insideUnitCircle.normalized;
        moveDir = new Vector3(rnd.x, 0f, rnd.y);
        wanderTimer = Random.Range(0f, wanderInterval);
    }

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        // Sonda hacia delante: SphereCast para detectar obstáculos
        bool hitObstacle = Physics.SphereCast(
            origin: transform.position + Vector3.up * (cc.height * 0.5f),
            radius: detectRadius,
            direction: transform.forward,
            hitInfo: out RaycastHit hit,
            maxDistance: detectDistance,
            layerMask: obstacleMask,
            queryTriggerInteraction: QueryTriggerInteraction.Ignore
        );

        if (hitObstacle && cooldown <= 0f)
        {
            ChangeDirection(hit.normal);
            cooldown = directionChangeCooldown;
        }
        else if (addSmallWander)
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                wanderTimer = wanderInterval + Random.Range(-0.3f, 0.3f);
                // Pequeña variación aleatoria para que no vayan todos igual
                float a = Random.Range(-wanderAngle, wanderAngle);
                moveDir = Quaternion.Euler(0f, a, 0f) * moveDir;
                moveDir.y = 0f;
                moveDir.Normalize();
            }
        }

        // Rotación suave hacia la dirección de movimiento
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // Movimiento con CharacterController
        Vector3 velocity = transform.forward * speed;

        // Mantener en suelo (simple)
        if (keepOnGround)
        {
            // gravedad ligera para mantener contacto
            velocity.y = -2f;
        }

        cc.Move(velocity * Time.deltaTime);
    }

    void ChangeDirection(Vector3 obstacleNormal)
    {
        // Estrategia: rebotar (reflejar dirección) + giro aleatorio grande
        Vector3 reflect = Vector3.Reflect(transform.forward, obstacleNormal);
        reflect.y = 0f;
        reflect.Normalize();

        float randomTurn = Random.Range(-maxTurnAngle, maxTurnAngle);
        moveDir = Quaternion.Euler(0f, randomTurn, 0f) * reflect;
        moveDir.y = 0f;
        moveDir.Normalize();
    }

    // Gizmos para ver el detector
    void OnDrawGizmosSelected()
    {
        if (!TryGetComponent<CharacterController>(out var c)) return;

        Vector3 origin = transform.position + Vector3.up * (c.height * 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin + transform.forward * detectDistance, detectRadius);
        Gizmos.DrawLine(origin, origin + transform.forward * detectDistance);
    }
}
