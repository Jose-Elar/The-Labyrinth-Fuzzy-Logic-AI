using UnityEngine;
using UnityEngine.UI;

public class NearestNPCStateHUD : MonoBehaviour
{
    public Text uiText;
    public float updateInterval = 0.2f;

    [Header("Debug")]
    public bool logIfNoneFound = false;

    private float timer;

    void Update()
    {
        if (uiText == null) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = updateInterval;

        var sensors = FindObjectsOfType<NPCStateSensor>();
        if (sensors == null || sensors.Length == 0)
        {
            uiText.text = "Estado NPC más cercano: --";

            if (logIfNoneFound)
                Debug.LogWarning("No se encontró ningún NPCStateSensor en la escena. ¿Está añadido al prefab/instancias del NPC?");
            return;
        }

        Vector3 p = transform.position;

        NPCStateSensor best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < sensors.Length; i++)
        {
            var s = sensors[i];
            float sqr = (s.transform.position - p).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = s;
            }
        }

        uiText.text = "Estado NPC más cercano: " + (best != null ? best.CurrentStateLabel : "--");
    }
}
