using System.Collections;
using UnityEngine;
using TMPro;

public class KillTextController : MonoBehaviour
{
    public static KillTextController Instance { get; private set; }

    [Header("Refs")]
    public TextMeshProUGUI label;

    [Header("Timing")]
    public float holdSeconds = 0.9f;      // time fully visible
    public float fadeSeconds = 0.6f;      // fade out duration

    [Header("Style")]
    public Color textColor = new Color(0.9f, 0.2f, 0.2f, 1f); // dark red-ish
    public Vector3 startScale = new Vector3(0.9f, 0.9f, 1f);
    public Vector3 endScale   = new Vector3(1.05f, 1.05f, 1f);

    Coroutine running;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (label == null) label = GetComponent<TextMeshProUGUI>();
        if (label) label.gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (!label) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShowCR(message));
    }

    IEnumerator ShowCR(string message)
    {
        Debug.Log("Hello world!");
        label.gameObject.SetActive(true);
        label.text = message;

        // start state
        Color c = textColor; c.a = 0f;
        label.color = c;
        transform.localScale = startScale;

        // quick pop-in
        float t = 0f;
        const float popIn = 0.15f;
        while (t < popIn)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / popIn);
            label.color = new Color(textColor.r, textColor.g, textColor.b, k); // fade in
            transform.localScale = Vector3.Lerp(startScale, Vector3.one, k);
            yield return null;
        }

        // hold
        yield return new WaitForSecondsRealtime(holdSeconds);

        // gentle scale + fade out
        t = 0f;
        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeSeconds);
            float a = 1f - k;
            label.color = new Color(textColor.r, textColor.g, textColor.b, a);
            transform.localScale = Vector3.Lerp(Vector3.one, endScale, k);
            yield return null;
        }

        label.gameObject.SetActive(false);
        running = null;
    }
}
