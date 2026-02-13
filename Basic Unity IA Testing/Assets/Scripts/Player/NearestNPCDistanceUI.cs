using UnityEngine;
using UnityEngine.UI;

public class NearestNPCDistanceUI : MonoBehaviour
{
    [Header("UI")]
    public Text uiText; // arrastra aquí NearestNPCText

    [Header("NPC filtering")]
    public string npcTag = "NPC"; // pon el tag que uséis (si no tenéis, deja vacío y usará SteeringManager)

    [Header("Update rate")]
    public float updateInterval = 0.2f;

    private float timer;

    void Awake()
    {
        if (uiText == null)
        {
            Debug.LogError("NearestNPCDistanceUI: asigna uiText (el Text del Canvas).");
        }
    }

    void Update()
    {
        if (uiText == null) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = updateInterval;

        float nearest = FindNearestNPCDistance();
        if (nearest < 0f)
        {
            uiText.text = "NPC más cercano: --";
        }
        else
        {
            uiText.text = $"NPC más cercano: {nearest:0.0} m";
        }
    }

    private float FindNearestNPCDistance()
    {
        Vector3 p = transform.position;
        float bestSqr = float.PositiveInfinity;
        bool found = false;

        // 1) Si tenéis tag NPC, esto es lo más rápido
        if (!string.IsNullOrEmpty(npcTag))
        {
            GameObject[] npcs = GameObject.FindGameObjectsWithTag(npcTag);
            for (int i = 0; i < npcs.Length; i++)
            {
                Vector3 d = npcs[i].transform.position - p;
                float sqr = d.sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    found = true;
                }
            }
        }
        else
        {
            // 2) Si no tenéis tag, usamos un componente “típico” del NPC:
            // SteeringManager está en todos vuestros NPC.
            SteeringManager[] npcs = FindObjectsOfType<SteeringManager>();
            for (int i = 0; i < npcs.Length; i++)
            {
                Vector3 d = npcs[i].transform.position - p;
                float sqr = d.sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    found = true;
                }
            }
        }

        return found ? Mathf.Sqrt(bestSqr) : -1f;
    }
}
