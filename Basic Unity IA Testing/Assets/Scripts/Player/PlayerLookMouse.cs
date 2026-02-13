using UnityEngine;

public class PlayerLookMouse2Axis : MonoBehaviour
{
    [Header("Auto-find")]
    public string pivotName = "PlayerCameraRig"; // tu nombre real

    [Header("Sensitivity")]
    public float sensitivityX = 180f;
    public float sensitivityY = 180f;

    [Header("Pitch limits")]
    public float minPitch = -75f;
    public float maxPitch = 75f;

    private Transform pivot;
    private float pitch;

    void Awake()
    {
        pivot = transform.Find(pivotName);
        if (pivot == null)
        {
            Debug.LogError($"PlayerLookMouse2Axis: No encuentro un hijo llamado '{pivotName}'. " +
                           $"Asegúrate de que el Player prefab tiene ese hijo.");
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (pivot == null) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        // Yaw: rota el Player
        transform.Rotate(0f, mouseX, 0f);

        // Pitch: rota el rig (arriba/abajo) con clamp
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        pivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
