using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalFinish : MonoBehaviour
{
    [Header("Who can finish?")]
    [Tooltip("Tag del NPC que debe tocar la meta (ej: NPC).")]
    public string npcTag = "NPC";

    [Header("End behavior")]
    public bool pauseGame = true;
    public bool showMessage = true;
    [TextArea] public string message = "¡Objetivo alcanzado!";

    [Header("Optional")]
    [Tooltip("Si quieres reiniciar la escena automáticamente, pon segundos > 0.")]
    public float restartAfterSeconds = 0f;

    private bool finished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (finished) return;
        if (!other.CompareTag(npcTag)) return;

        Finish();
    }

    // Si no usas Trigger (collider normal), puedes usar colisión:
    private void OnCollisionEnter(Collision collision)
    {
        if (finished) return;
        if (!collision.collider.CompareTag(npcTag)) return;

        Finish();
    }

    private void Finish()
    {
        finished = true;

        if (showMessage) Debug.Log(message);

        if (pauseGame) Time.timeScale = 0f;

        if (restartAfterSeconds > 0f)
        {
            // Como Time.timeScale puede estar en 0, usamos un "invoke" con tiempo real
            StartCoroutine(RestartCoroutine());
        }
    }

    private System.Collections.IEnumerator RestartCoroutine()
    {
        float t = 0f;
        while (t < restartAfterSeconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Mensajito simple en pantalla (opcional)
    private void OnGUI()
    {
        if (!finished || !showMessage) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 32;
        style.alignment = TextAnchor.MiddleCenter;

        Rect r = new Rect(0, 0, Screen.width, Screen.height);
        GUI.Label(r, message, style);
    }
}
