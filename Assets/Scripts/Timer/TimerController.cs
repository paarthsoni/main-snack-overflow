using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TimerController : MonoBehaviour
{
    [Header("Config")]
    public float startTime = 60f;

    [Header("Scene refs")]
    public TextMeshProUGUI timerText;          
    public GameObject gameOverPanel;           
    [Tooltip("If not assigned, we search any Canvas (even inactive) for this name.")]
    public string gameOverPanelName = "GameOver Panel";
    [Tooltip("Name of the Retry button inside the GameOver panel.")]
    public string retryButtonName = "Retry";

    float currentTime;
    bool isGameOver = false;
    bool isRunning  = false;

    const int TOP_SORT_ORDER = 5000;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        ResetAndShowPaused(); 
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        EnsureEventSystem();

        if (!timerText)
        {
            var go = GameObject.Find("TimerText");
            if (go) timerText = go.GetComponent<TextMeshProUGUI>();
        }
        if (timerText) timerText.gameObject.SetActive(true);

        if (!gameOverPanel)
            gameOverPanel = FindPanelByNameIncludingInactive(gameOverPanelName);

        if (gameOverPanel) gameOverPanel.SetActive(false);

        BindRetryButton();     
        ResetAndShowPaused();
    }

    void Update()
    {
        if (isGameOver || !isRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0f, startTime);
        UpdateTimerUI();

        if (currentTime <= 0f)
            GameOver();
    }

    public void StartTimer(float seconds = -1f)
    {
        if (seconds > 0f) startTime = seconds;

        currentTime = startTime;
        isGameOver  = false;
        isRunning   = true;

        if (timerText) timerText.gameObject.SetActive(true);
        if (gameOverPanel) gameOverPanel.SetActive(false);

        UpdateTimerUI();
    }

    public void StopTimer()     => isRunning = false;
    public void ForceGameOver() => GameOver();

    public void Retry()
    {
        Debug.Log("[TimerController] Retry() called — reloading scene.");
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    void ResetAndShowPaused()
    {
        isRunning  = false;
        isGameOver = false;
        currentTime = Mathf.Max(0f, startTime);
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (!timerText) return;
        int m = Mathf.FloorToInt(currentTime / 60f);
        int s = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }

    void GameOver()
    {
        isGameOver = true;
        isRunning  = false;

        ShowPanelOnTopAndMakeClickable(gameOverPanel);
        Time.timeScale = 0f;
    }

    void EnsureEventSystem()
    {
        if (!FindObjectOfType<EventSystem>())
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(go);
            Debug.Log("[TimerController] Created EventSystem.");
        }
    }

    GameObject FindPanelByNameIncludingInactive(string panelName)
    {
        if (string.IsNullOrEmpty(panelName)) return null;

        var canvases = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (var canvas in canvases)
        {
            var trs = canvas.GetComponentsInChildren<Transform>(true);
            foreach (var t in trs)
                if (t.name == panelName)
                    return t.gameObject;
        }
        return GameObject.Find(panelName); 
    }

    void ShowPanelOnTopAndMakeClickable(GameObject panel)
    {
        if (!panel)
        {
            Debug.LogWarning("[TimerController] GameOver panel not set/found.");
            return;
        }

        var t = panel.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

            var cg = t.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            t = t.parent;
        }

        var topCanvas = panel.GetComponent<Canvas>();
        if (!topCanvas) topCanvas = panel.AddComponent<Canvas>();
        topCanvas.overrideSorting = true;
        topCanvas.sortingOrder = TOP_SORT_ORDER;
        topCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var gr = panel.GetComponent<GraphicRaycaster>();
        if (!gr) panel.AddComponent<GraphicRaycaster>();

        panel.transform.SetAsLastSibling();

        
        BindRetryButton();
    }

    void BindRetryButton()
{
    if (!gameOverPanel) return;

    Button retry = null;
    var buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
    foreach (var b in buttons)
        if (b.name == retryButtonName) { retry = b; break; }

    if (!retry)
    {
        var texts = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
            if (t.text.Trim().ToLower().Contains("retry"))
            {
                retry = t.GetComponentInParent<Button>(true);
                if (retry) break;
            }
    }
    if (!retry) { Debug.LogWarning("Retry button not found."); return; }

    if (retry.targetGraphic) retry.targetGraphic.raycastTarget = true;

   
    retry.onClick.RemoveAllListeners();
    retry.onClick.AddListener(Retry);


    var et = retry.GetComponent<EventTrigger>();
    if (et) Destroy(et);
}

}
