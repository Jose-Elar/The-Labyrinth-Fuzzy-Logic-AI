using UnityEngine;
using UnityEngine.UI;

public class NPCHealthBar : MonoBehaviour
{
    [Header("Refs")]
    public HpNPC hpNpc;
    public Image fillImage;

    [Header("Position")]
    public Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

    private Transform cam;

    void Awake()
    {
        if (hpNpc == null)
            hpNpc = GetComponentInParent<HpNPC>();

        cam = Camera.main != null ? Camera.main.transform : null;
    }

    void LateUpdate()
    {
        if (hpNpc == null || fillImage == null) return;

        // Seguir al NPC (este script vive en el canvas/barra)
        Transform npc = hpNpc.transform;
        transform.position = npc.position + worldOffset;

        // Mirar a la cámara
        if (cam != null)
        {
            transform.forward = cam.forward; // billboard simple
        }

        // Actualizar fill
        float max = Mathf.Max(1f, hpNpc.getMaxHP());
        float t = Mathf.Clamp01(hpNpc.getHP() / max);
        fillImage.fillAmount = t;
    }
}
