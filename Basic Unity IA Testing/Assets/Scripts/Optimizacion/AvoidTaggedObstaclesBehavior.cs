using UnityEngine;

public class AvoidTaggedObstaclesBehavior : BaseBehavior
{
    [Header("Movement")]
    public float maxSpeed = 5f;

    [Header("Obstacle tag")]
    public string obstacleTag = "Wall";

    [Header("Detection")]
    public float detectDistance = 2.5f;
    public float detectRadius = 0.45f;
    public float castHeight = 0.8f;
    [Range(0f, 80f)] public float feelerAngle = 35f;

    [Header("Stability (anti-jitter)")]
    [Tooltip("Tiempo mínimo que mantiene una dirección de evitación antes de recalcular (s).")]
    public float holdAvoidTime = 0.18f;

    [Tooltip("Suavizado de cambio de dirección (más alto = más rápido).")]
    public float avoidSmoothing = 10f;

    [Header("Corner / Wall separation")]
    [Tooltip("Si está muy pegado a un muro, empuja hacia fuera.")]
    public float separationRadius = 0.65f;

    [Tooltip("Cuánta componente lateral añade para deslizarse por el muro (0 = solo empujar fuera).")]
    [Range(0f, 1f)] public float slideAlongWall = 0.35f;

    // Buffers para evitar GC
    private readonly RaycastHit[] _hits = new RaycastHit[8];
    private readonly Collider[] _overlaps = new Collider[16];

    private Vector3 _avoidDirCurrent = Vector3.zero;
    private Vector3 _avoidDirTarget = Vector3.zero;
    private float _holdTimer = 0f;

    public override Vector3 DesiredDirectionNormalized()
    {
        // Mantener dirección de evitación un poquito para evitar temblores
        if (_holdTimer > 0f)
        {
            _holdTimer -= Time.deltaTime;
            _avoidDirCurrent = Vector3.Lerp(_avoidDirCurrent, _avoidDirTarget, avoidSmoothing * Time.deltaTime);
            _avoidDirCurrent.y = 0f;
            if (_avoidDirCurrent.sqrMagnitude > 0.0001f) _avoidDirCurrent.Normalize();
            return _avoidDirCurrent * maxSpeed;
        }

        Vector3 computed = ComputeAvoidDirection();

        if (computed.sqrMagnitude <= 0.0001f)
        {
            // No hay muro cerca: este behavior no aporta fuerza
            return Vector3.zero;
        }

        _avoidDirTarget = computed;
        _avoidDirCurrent = Vector3.Lerp(_avoidDirCurrent, _avoidDirTarget, avoidSmoothing * Time.deltaTime);
        _avoidDirCurrent.y = 0f;
        if (_avoidDirCurrent.sqrMagnitude > 0.0001f) _avoidDirCurrent.Normalize();

        _holdTimer = holdAvoidTime;
        return _avoidDirCurrent * maxSpeed;
    }

    private Vector3 ComputeAvoidDirection()
    {
        // 1) Si está muy pegado (esquina), usar separación por overlap (sale de “pegado”)
        Vector3 sep = ComputeSeparationFromWalls();
        if (sep.sqrMagnitude > 0.0001f)
            return sep;

        // 2) Feelers: forward / left / right (spherecasts)
        Vector3 origin = transform.position + Vector3.up * castHeight;

        Vector3 fwd = transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();

        Vector3 leftDir = Quaternion.Euler(0f, -feelerAngle, 0f) * fwd;
        Vector3 rightDir = Quaternion.Euler(0f, feelerAngle, 0f) * fwd;

        bool hit = TryGetClosestTaggedHit(origin, fwd, out RaycastHit hitInfo)
                || TryGetClosestTaggedHit(origin, leftDir, out hitInfo)
                || TryGetClosestTaggedHit(origin, rightDir, out hitInfo);

        if (!hit) return Vector3.zero;

        Vector3 normal = hitInfo.normal;
        normal.y = 0f;
        if (normal.sqrMagnitude < 0.0001f) normal = -fwd;
        normal.Normalize();

        // Dirección “salida”: empujar fuera + un poco de deslizamiento lateral para no clavarse
        Vector3 tangent = Vector3.Cross(Vector3.up, normal);
        tangent.y = 0f;
        if (tangent.sqrMagnitude > 0.0001f) tangent.Normalize();

        // Elegir el lado del tangent que más ayuda a seguir avanzando (reduce rebotes raros)
        if (Vector3.Dot(tangent, fwd) < 0f) tangent = -tangent;

        Vector3 desired = (normal + tangent * slideAlongWall).normalized;
        return desired;
    }

    private Vector3 ComputeSeparationFromWalls()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            separationRadius,
            _overlaps,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (count == 0) return Vector3.zero;

        Vector3 push = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Collider c = _overlaps[i];
            if (c == null) continue;
            if (!c.CompareTag(obstacleTag)) continue;

            // Empujar alejándose del centro del collider (aprox)
            Vector3 away = (transform.position - c.ClosestPoint(transform.position));
            away.y = 0f;

            if (away.sqrMagnitude > 0.0001f)
            {
                push += away.normalized;
            }
        }

        if (push.sqrMagnitude <= 0.0001f) return Vector3.zero;
        push.y = 0f;
        return push.normalized;
    }

    private bool TryGetClosestTaggedHit(Vector3 origin, Vector3 dir, out RaycastHit bestHit)
    {
        bestHit = default;
        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            detectRadius,
            dir,
            _hits,
            detectDistance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        float bestDist = float.PositiveInfinity;
        bool found = false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider c = _hits[i].collider;
            if (c == null) continue;
            if (!c.CompareTag(obstacleTag)) continue;

            float d = _hits[i].distance;
            if (d < bestDist)
            {
                bestDist = d;
                bestHit = _hits[i];
                found = true;
            }
        }

        return found;
    }

    // Plan B: si ya chocó con una pared, fijar una dirección de escape estable
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null) return;
        if (!hit.collider.CompareTag(obstacleTag)) return;

        Vector3 fwd = transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();

        Vector3 normal = hit.normal;
        normal.y = 0f;
        if (normal.sqrMagnitude < 0.0001f) normal = -fwd;
        normal.Normalize();

        Vector3 bounced = Vector3.Reflect(fwd, normal);
        bounced.y = 0f;
        if (bounced.sqrMagnitude > 0.0001f) bounced.Normalize();

        _avoidDirTarget = bounced;
        _holdTimer = holdAvoidTime * 2f;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * castHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin + transform.forward * detectDistance, detectRadius);
        Gizmos.DrawLine(origin, origin + transform.forward * detectDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
