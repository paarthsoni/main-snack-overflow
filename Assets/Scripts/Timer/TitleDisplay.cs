using System.Collections;
using TMPro;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public GameObject instructionsPanel;
    public InstructionsManager instructionsManager;

    public float slideSpeed = 500f;      // units per second
    public float pauseDuration = 2f;     // seconds at center

    void Start()
    {
        // Ensure instructions hidden
        instructionsPanel.SetActive(false);
        titleText.gameObject.SetActive(true);

        StartCoroutine(AnimateTitleToCenter());
    }

    IEnumerator AnimateTitleToCenter()
    {
        RectTransform titleRect = titleText.rectTransform;
        Canvas canvas = titleText.canvas;

        // Get top of screen in canvas local space
        Vector2 topScreen = new Vector2(Screen.width / 2f, Screen.height); // top-center in screen coords
        Vector3 topPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, topScreen, canvas.worldCamera, out topPos);

        // Get center of screen in canvas local space
        Vector2 centerScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector3 centerPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, centerScreen, canvas.worldCamera, out centerPos);

        // Set title to top initially
        titleRect.position = topPos;

        // Slide to center
        yield return SlideWorldPosition(titleRect, topPos, centerPos);

        // Pause at center
        yield return new WaitForSecondsRealtime(pauseDuration);

        // Hide title at center
        titleText.gameObject.SetActive(false);

        // Show instructions
        if (instructionsManager != null)
        {
            instructionsPanel.SetActive(true);
            instructionsManager.StartCoroutine(instructionsManager.TypeText());
        }
        else
        {
            Debug.LogError("InstructionsManager not assigned in TitleManager!");
        }
    }

    IEnumerator SlideWorldPosition(RectTransform rect, Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        float duration = Mathf.Max(distance / slideSpeed, 0.5f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        rect.position = to;
    }
}
