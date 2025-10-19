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
    [Tooltip("Will be rebound automatically each scene load by name/tag below.")]
    public TextMeshProUGUI livesText;

    [Header("Binding (optional)")]
    [Tooltip("Name to search for in the scene. Leave empty to skip name search.")]
    [SerializeField] string livesTextName = "LivesText";
    [Tooltip("Tag to search for in the scene. Leave empty to skip tag search.")]
    [SerializeField] string livesTextTag  = "LivesText";
    [Tooltip("If true, logs a warning when LivesText is not found.")]
    [SerializeField] bool logIfMissing = true;

    [Header("Events")]
    public UnityEvent OnOutOfLives = new UnityEvent();

    int lives;

    void Awake()
    {
       
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[LivesManager] Duplicate detected — destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start()
    {
        
        lives = startingLives;
        RebindLivesText(force: true);
        UpdateUI();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        lives = startingLives;

        
        OnOutOfLives.RemoveAllListeners();
        var tc = FindObjectOfType<TimerController>(true);
        if (tc) OnOutOfLives.AddListener(tc.ForceGameOver);

        
        RebindLivesText(force: true);
        UpdateUI();
    }

    
    public static void SafeLoseLife()
    {
        if (Instance != null) Instance.LoseLife();
        else Debug.LogError("[LivesManager] SafeLoseLife() called but Instance is null.");
    }

    public void LoseLife()
    {
        int before = lives;
        lives = Mathf.Max(0, lives - 1);

        
        if (!IsTextValid(livesText)) RebindLivesText(force: true);

        UpdateUI();

        if (lives == 0)
            OnOutOfLives.Invoke();
    }

    
    public void ResetLives(int to = -1)
    {
        lives = (to >= 0) ? to : startingLives;
        if (!IsTextValid(livesText)) RebindLivesText(force: true);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (!IsTextValid(livesText))
        {
            RebindLivesText(force: true);
            if (!IsTextValid(livesText))
            {
                if (logIfMissing)
                    Debug.LogWarning("[LivesManager] UpdateUI() skipped — LivesText not found. " +
                                     $"Set name='{livesTextName}' or tag='{livesTextTag}', or drag assign in Inspector.");
                return;
            }
        }

        livesText.text = $"Remaining Lives: {lives}";
        if (!livesText.gameObject.activeSelf)
            livesText.gameObject.SetActive(true);
    }

    

    static bool IsTextValid(TextMeshProUGUI t) => t != null && !ReferenceEquals(t, null);

    void RebindLivesText(bool force = false)
    {
        if (!force && IsTextValid(livesText)) return;

        livesText = null;

        
        if (!string.IsNullOrEmpty(livesTextName))
        {
            var byName = GameObject.Find(livesTextName);
            if (byName) livesText = byName.GetComponent<TextMeshProUGUI>();
        }

        
        if (!IsTextValid(livesText) && !string.IsNullOrEmpty(livesTextTag))
        {
            var all = FindObjectsOfType<TextMeshProUGUI>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t && t.CompareTag(livesTextTag))
                {
                    livesText = t;
                    break;
                }
            }
        }

        if (!IsTextValid(livesText) && logIfMissing)
        {
            Debug.LogWarning("[LivesManager] LivesText not found in scene. " +
                             $"(searched name='{livesTextName}', tag='{livesTextTag}').");
        }
    }

    public int Current => lives;
}
