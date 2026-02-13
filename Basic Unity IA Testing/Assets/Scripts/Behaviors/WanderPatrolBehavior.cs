using UnityEngine;

public class WanderPatrolBehavior : BaseBehavior
{
    [Header("Movement")]
    public float maxSpeed = 5f;

    [Header("Wander (no target)")]
    [Tooltip("Cada cu�ntos segundos elige un nuevo rumbo aleatorio.")]
    public float chooseNewDirectionEvery = 1.2f;

    [Tooltip("Cu�nto puede cambiar el rumbo respecto al actual (grados).")]
    [Range(10f, 180f)]
    public float maxTurnAngle = 140f;

    [Tooltip("Suavizado al girar hacia el nuevo rumbo (m�s alto = m�s r�pido).")]
    public float turnSmoothing = 6f;

    [Tooltip("Si quieres que algunos NPC sean m�s err�ticos, activa esto.")]
    public bool randomizePerAgent = true;

    private Vector3 currentDir;
    private Vector3 targetDir;
    private float timer;

    private void Awake()
    {
        // Direcci�n inicial aleatoria en XZ
        Vector2 r = Random.insideUnitCircle.normalized;
        currentDir = new Vector3(r.x, 0f, r.y);
        targetDir = currentDir;

        if (randomizePerAgent)
        {
            chooseNewDirectionEvery *= Random.Range(0.7f, 4f);
            maxTurnAngle *= Random.Range(0.85f, 1.15f);
            turnSmoothing *= Random.Range(0.85f, 1.15f);
        }

        timer = Random.Range(0f, chooseNewDirectionEvery);
    }

    public override Vector3 DesiredDirectionNormalized()
    {
        Debug.Log("WANDER");
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = chooseNewDirectionEvery;

            // Elegir un nuevo rumbo: giro aleatorio RELATIVO al actual
            float a = Random.Range(-maxTurnAngle, maxTurnAngle);
            targetDir = Quaternion.Euler(0f, a, 0f) * currentDir;
            targetDir.y = 0f;

            if (targetDir.sqrMagnitude < 0.0001f)
                targetDir = Random.insideUnitCircle.normalized.ToXZ();
            else
                targetDir.Normalize();
        }

        // Suavizar hacia el rumbo objetivo
        currentDir = Vector3.Slerp(currentDir, targetDir, turnSmoothing * Time.deltaTime);
        currentDir.y = 0f;

        if (currentDir.sqrMagnitude > 0.0001f)
            currentDir.Normalize();

        return currentDir * maxSpeed;
    }
}

// Helper m�nimo para Vector2 -> Vector3 sin meter librer�as
public static class Vector2Extensions
{
    public static Vector3 ToXZ(this Vector2 v) => new Vector3(v.x, 0f, v.y);
}
