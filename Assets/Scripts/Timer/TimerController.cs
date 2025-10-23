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
    [Tooltip("Child TMP name inside the GameOver Panel where the score is written.")]
    public string scoreTextName = "ScoreText";

    [Header("Top-left UI")]
    public GameObject topLeftButtonsRoot;   


    float currentTime;
    bool isGameOver = false;
    bool isRunning = false;
    bool _reloading = false;

    const int TOP_SORT_ORDER = 5000;

    void Awake() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start() => ResetAndShowPaused();

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        _reloading = false;

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

        if (currentTime <= 0f) GameOver();
    }

    public void StartTimer(float seconds = -1f)
    {
        if (seconds > 0f) startTime = seconds;

        currentTime = startTime;
        isGameOver = false;
        isRunning = true;

        if (timerText) timerText.gameObject.SetActive(true);
        if (gameOverPanel) gameOverPanel.SetActive(false);

        UpdateTimerUI();

        SetTopLeftButtonsVisible(true);

    }

    public void StopTimer() => isRunning = false;

    public void ResumeTimer()
    {
        if (!isGameOver) isRunning = true;
    }

    public void ForceGameOver() => GameOver();

    public void Retry()
    {
        if (_reloading) return;
        _reloading = true;

        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    void ResetAndShowPaused()
    {
        isRunning = false;
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
        isRunning = false;

        
        TryFillGameOverScore();

        ShowPanelOnTopAndMakeClickable(gameOverPanel);

        SetTopLeftButtonsVisible(false);

        Time.timeScale = 0f;
    }

    
    void TryFillGameOverScore()
    {
        if (!gameOverPanel) return;

        TextMeshProUGUI scoreTMP = null;
        var tmps = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in tmps)
            if (t.name == scoreTextName) { scoreTMP = t; break; }

        if (!scoreTMP)
        {
            Debug.Log("[TimerController] ScoreText not found under GameOver Panel (optional).");
            return;
        }

        int total = 0, killed = 0, remaining = 0;
        var tracker = ImpostorTracker.Instance ?? FindObjectOfType<ImpostorTracker>(true);
        if (tracker != null)
        {
            killed = tracker.Killed;
            remaining = tracker.Remaining;
            total = tracker.TotalSpawned;

            if (total <= 0) total = killed + remaining; 
        }

        scoreTMP.text = $"You eliminated {killed} out of {total} imposters!";
    }

   
    void EnsureEventSystem()
    {
        if (!FindObjectOfType<EventSystem>())
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(go);
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
        if (!panel) return;

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

        if (retry == null)
        {
            var texts = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
                if (t.text.Trim().ToLower().Contains("retry"))
                {
                    retry = t.GetComponentInParent<Button>(true);
                    if (retry) break;
                }
        }

        if (retry == null) return;

        if (retry.targetGraphic != null)
            retry.targetGraphic.raycastTarget = true;

        retry.onClick.RemoveAllListeners();
        retry.onClick.AddListener(Retry);

        var et = retry.gameObject.GetComponent<EventTrigger>();
        if (et && et.triggers != null)
        {
            et.triggers.RemoveAll(e => e != null && e.eventID == EventTriggerType.PointerClick);
            if (et.triggers.Count == 0) Destroy(et);
        }
    }

    public void SetTopLeftButtonsVisible(bool visible)
{
    var go = topLeftButtonsRoot ? topLeftButtonsRoot : GameObject.Find("TopLeftButtons");
    if (go) go.SetActive(visible);
}

    
public void PreparePanelForClicks(GameObject panel)
{
    if (!panel) return;

    
    var t = panel.transform;
    while (t != null)
    {
        if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
        var cg = t.GetComponent<CanvasGroup>();
        if (cg) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
        t = t.parent;
    }

    
    var c = panel.GetComponent<Canvas>();
    if (!c) c = panel.AddComponent<Canvas>();
    c.overrideSorting = true;
    c.sortingOrder = 5000;         
    c.renderMode = RenderMode.ScreenSpaceOverlay;

    if (!panel.GetComponent<GraphicRaycaster>())
        panel.AddComponent<GraphicRaycaster>();

    panel.transform.SetAsLastSibling();
}


public void WireButtonToRetry(GameObject root, string buttonNameOrText = "Exit")
{
    if (!root) return;

    Button target = null;

    
    foreach (var b in root.GetComponentsInChildren<Button>(true))
        if (b.name == buttonNameOrText) { target = b; break; }

    
    if (!target)
    {
        foreach (var tmp in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
        {
            var txt = tmp.text.Trim().ToLower();
            if (txt.Contains(buttonNameOrText.Trim().ToLower()))
            {
                target = tmp.GetComponentInParent<Button>(true);
                if (target) break;
            }
        }
    }

    if (!target) return;

    if (target.targetGraphic) target.targetGraphic.raycastTarget = true;
    target.onClick.RemoveAllListeners();
    target.onClick.AddListener(Retry);   
}


}
