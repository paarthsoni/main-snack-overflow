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
    public TextMeshProUGUI livesText;   // will be (re)bound per scene if left null

    [Header("Events")]
    public UnityEvent OnOutOfLives = new UnityEvent();

    int lives;
    bool warnedMissingUIText = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start()
    {
        if (lives <= 0) lives = startingLives;
        RebindLivesText(forceFind: true);
        UpdateUI();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        // If someone wires GameOver to this via inspector, re-point it each load:
        OnOutOfLives.RemoveAllListeners();
        var tc = FindObjectOfType<TimerController>(true);
        if (tc) OnOutOfLives.AddListener(tc.ForceGameOver);

        RebindLivesText(forceFind: true);
        ResetLives(); // new round on reload
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
        if (!livesText)
        {
            if (!warnedMissingUIText)
            {
                warnedMissingUIText = true;
                Debug.LogWarning("[LivesManager] LivesText not found in this scene. " +
                                 "Name the TMP object exactly 'LivesText' or assign it in the Inspector.");
            }
            return;
        }

        livesText.text = $"Remaining Lives: {lives}";
    }

    /// <summary>
    /// Finds the TextMeshProUGUI named "LivesText" in the scene (even if inactive),
    /// unless one is already assigned in the Inspector.
    /// </summary>
    void RebindLivesText(bool forceFind)
    {
        if (!forceFind && livesText) return;

        // If something is already assigned in the scene reference, keep it.
        if (livesText && livesText.gameObject.scene.IsValid()) return;

        // Try by exact name (active or inactive)
        var rootCanvases = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (var c in rootCanvases)
        {
            var texts = c.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                if (t && t.name == "LivesText")
                {
                    livesText = t;
                    warnedMissingUIText = false; // we found it, clear the warning gate
                    return;
                }
            }
        }

        // Last fallback: direct active search by name
        var go = GameObject.Find("LivesText");
        if (go) { livesText = go.GetComponent<TextMeshProUGUI>(); warnedMissingUIText = false; }
    }

    public int Current => lives;
}
