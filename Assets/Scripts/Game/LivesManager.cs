
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Settings")]
    [Min(0)] public int startingLives = 3;

    [Header("UI (scene object)")]
    public TextMeshProUGUI livesText;   // this will be re-bound per scene

    [Header("Events")]
    public UnityEvent OnOutOfLives = new UnityEvent();

    int lives;

    void Awake()
    {
        // Singleton + persist
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start()
    {
        // First scene boot
        if (lives <= 0) lives = startingLives;
        UpdateUI();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Always unpause after reload
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        // Re-wire the Game Over target (TimerController in this scene)
        OnOutOfLives.RemoveAllListeners();
        var tc = FindObjectOfType<TimerController>();
        if (tc) OnOutOfLives.AddListener(tc.ForceGameOver);

        // Re-bind the scene's LivesText (by name or tag)
        if (livesText == null)
        {
            // Try by name first (rename your UI text to "LivesText")
            var go = GameObject.Find("LivesText");
            if (go) livesText = go.GetComponent<TextMeshProUGUI>();

            // Fallback: search by tag "LivesText" (create this tag and assign to the text)
            if (!livesText)
            {
                var all = FindObjectsOfType<TextMeshProUGUI>(true);
                foreach (var t in all)
                    if (t.CompareTag("LivesText")) { livesText = t; break; }
            }
        }

        // New round = reset lives & UI
        ResetLives();
    }

    public void LoseLife()
    {
        lives = Mathf.Max(0, lives - 1);
        UpdateUI();
        if (lives == 0) OnOutOfLives.Invoke();
    }

    public void ResetLives(int to = -1)
    {
        lives = (to >= 0) ? to : startingLives;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (livesText) livesText.text = $"Remaining Lives: {lives}";
    }

    public int Current => lives;
}
