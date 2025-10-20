using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseRestartUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;   // drag PauseButton
    [SerializeField] private Button exitButton;    // drag ExitButton (acts as Restart)

    [Header("Optional Overlay")]
    [SerializeField] private GameObject pauseOverlay; // dim panel; optional

    private TimerController timer;   // found at runtime
    private bool paused;

    void Awake()
    {
        timer = FindObjectOfType<TimerController>(true);
        if (!pauseButton || !exitButton)
        {
            Debug.LogWarning("[PauseRestartUI] Assign Pause & Exit buttons in the inspector.");
        }
    }

    void OnEnable()
    {
        if (pauseButton) pauseButton.onClick.AddListener(TogglePause);
        if (exitButton)  exitButton.onClick.AddListener(RestartGame);
    }

    void OnDisable()
    {
        if (pauseButton) pauseButton.onClick.RemoveListener(TogglePause);
        if (exitButton)  exitButton.onClick.RemoveListener(RestartGame);
    }

    void TogglePause()
    {
        paused = !paused;

        if (paused)
        {
            // stop world + countdown
            Time.timeScale = 0f;
            if (timer) timer.StopTimer();
            if (pauseOverlay) pauseOverlay.SetActive(true);
            SetButtonLabel(pauseButton, "Resume");
        }
        else
        {
            // resume world + countdown
            Time.timeScale = 1f;
            if (timer) timer.ResumeTimer();
            if (pauseOverlay) pauseOverlay.SetActive(false);
            SetButtonLabel(pauseButton, "Pause");
        }
    }

    void RestartGame()
    {
        // Ensure unpaused before reload so the new scene isn't frozen.
        Time.timeScale = 1f;

        // Use TimerController’s retry (scene reload) if present.
        if (timer)
        {
            timer.Retry();  // reloads current scene → title → instructions → memory → gameplay
            return;
        }

        // Fallback: direct reload.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    // helper to change TMP or legacy Text on the button
    void SetButtonLabel(Button b, string text)
    {
        if (!b) return;
        var tmp = b.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (tmp) { tmp.text = text; return; }
        var uText = b.GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (uText) uText.text = text;
    }
}
