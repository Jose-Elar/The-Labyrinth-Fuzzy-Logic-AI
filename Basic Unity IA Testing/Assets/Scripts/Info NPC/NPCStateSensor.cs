using System.Collections.Generic;
using UnityEngine;

public class NPCStateSensor : MonoBehaviour
{
    public enum NPCState { PATROL, ATTACK, FLEE }

    [Header("Estado actual (solo lectura)")]
    [SerializeField] private NPCState currentState = NPCState.PATROL;
    public NPCState CurrentState => currentState;

    public string CurrentStateLabel
    {
        get
        {
            switch (currentState)
            {
                case NPCState.PATROL: return "PATRULLA";
                case NPCState.ATTACK: return "ATAQUE";
                case NPCState.FLEE:   return "HUIDA";
                default: return currentState.ToString();
            }
        }
    }

    public enum DetectMode { Auto, WeightsOnly, RulesOnly }

    [Header("Modo de detección")]
    [Tooltip("Auto = intenta Activity (behaviors) y si no puede usa Rules. WeightsOnly aquí significa 'activity por behaviors' (NO solo weightFactor).")]
    public DetectMode mode = DetectMode.Auto;

    [Header("Actualización")]
    [Tooltip("Cada cuántos segundos se recalcula el estado (para no gastar CPU).")]
    public float updateInterval = 0.2f;

    [Header("Rules fallback (si no hay behaviors)")]
    public float fleeHpThreshold = 20f;   // coherente con vuestros scripts
    public float attackHpThreshold = 20f; // >20 ataca

    [Header("Debug (solo lectura)")]
    public bool playerSeen;
    public float hpValue;
    public float arriveMag;
    public float fleeMag;
    public string decisionSource;

    private float timer;

    private HpNPC hp;
    private NPCRaycast sight;

    private BaseBehavior arriveB;
    private BaseBehavior fleeB;
    private BaseBehavior patrolB; // opcional

    private const float EPS = 0.0001f;

    void Awake()
    {
        // Buscar en el propio GO y en hijos (muy típico que NPCRaycast esté en un child)
        hp = GetComponent<HpNPC>() ?? GetComponentInChildren<HpNPC>(true) ?? GetComponentInParent<HpNPC>();
        sight = GetComponent<NPCRaycast>() ?? GetComponentInChildren<NPCRaycast>(true) ?? GetComponentInParent<NPCRaycast>();

        ResolveBehaviors();
        timer = Random.Range(0f, updateInterval); // desincroniza NPCs
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = updateInterval;

        // Inputs
        playerSeen = (sight != null) && sight.isPlayerSeen();
        hpValue = (hp != null) ? hp.getHP() : 100f;

        // Reintento si no se pillaron behaviors (por orden de carga)
        if (arriveB == null && fleeB == null && patrolB == null)
            ResolveBehaviors();

        bool canUseBehaviors = (arriveB != null) || (fleeB != null);

        if (mode == DetectMode.RulesOnly || (!canUseBehaviors && mode != DetectMode.WeightsOnly))
        {
            currentState = GetStateFromRules();
            decisionSource = "Rules";
            arriveMag = 0f;
            fleeMag = 0f;
            return;
        }

        // === CLAVE: estado por ACTIVIDAD REAL ===
        // Flee y Arrive en vuestro proyecto devuelven Vector3.zero cuando “no aplican”.
        Vector3 fleeVec = (fleeB != null) ? fleeB.DesiredDirectionNormalized() : Vector3.zero;
        Vector3 arriveVec = (arriveB != null) ? arriveB.DesiredDirectionNormalized() : Vector3.zero;

        fleeMag = fleeVec.sqrMagnitude;
        arriveMag = arriveVec.sqrMagnitude;

        if (fleeMag > EPS)
        {
            currentState = NPCState.FLEE;
            decisionSource = "Behaviors (Flee active)";
            return;
        }

        if (arriveMag > EPS)
        {
            currentState = NPCState.ATTACK;
            decisionSource = "Behaviors (Arrive active)";
            return;
        }

        // Si ninguno está activo, patrulla
        currentState = NPCState.PATROL;
        decisionSource = "Behaviors (none active => Patrol)";
    }

    private NPCState GetStateFromRules()
    {
        // Reglas coherentes con vuestros scripts:
        // - Si no ve al player => Patrulla
        // - Si ve y HP <= 20 => Huye
        // - Si ve y HP > 20 => Ataca
        if (!playerSeen) return NPCState.PATROL;
        if (hpValue <= fleeHpThreshold) return NPCState.FLEE;
        return NPCState.ATTACK;
    }

    private void ResolveBehaviors()
    {
        // Pillar todos los BaseBehavior aunque estén en hijos
        var list = new List<BaseBehavior>(GetComponentsInChildren<BaseBehavior>(true));

        arriveB = null;
        fleeB = null;
        patrolB = null;

        for (int i = 0; i < list.Count; i++)
        {
            var b = list[i];
            if (b == null) continue;

            string typeName = b.GetType().Name;

            // ArriveAgent / Arrive... => ATAQUE
            if (arriveB == null && typeName.Contains("Arrive")) arriveB = b;

            // Flee => HUIDA
            else if (fleeB == null && typeName.Contains("Flee")) fleeB = b;

            // WanderPatrolBehavior => PATRULLA (opcional, no hace falta para decidir)
            else if (patrolB == null && (typeName.Contains("Wander") || typeName.Contains("Patrol"))) patrolB = b;
        }
    }
}
