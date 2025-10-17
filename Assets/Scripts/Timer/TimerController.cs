using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimerController : MonoBehaviour
{
    public float startTime = 180f; // 3 minutes
    private float currentTime;

    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;

    private bool isGameOver = false;
    private bool isRunning = false;   // NEW

    void Start()
    {
        currentTime = startTime;
        gameOverPanel.SetActive(false);
        UpdateTimerUI();              // show 03:00 initially
        isRunning = false;            // wait for StartTimer()
    }

    void Update()
    {
        if (isGameOver || !isRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0, startTime);

        UpdateTimerUI();
        if (currentTime <= 0) GameOver();
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void StopTimer()
{
    // Halts countdown without triggering GameOver
    isRunning = false;
}
    void GameOver()
    {
        isGameOver = true;
        isRunning = false;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ForceGameOver()
{
    GameOver();   // calls your existing private GameOver()
}       

    public void StartTimer(float seconds = -1f)  // NEW
    {
        if (seconds > 0f) startTime = seconds;
        currentTime = startTime;
        isGameOver = false;
        gameOverPanel.SetActive(false);
        UpdateTimerUI();
        isRunning = true;
    }

    public void Retry()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}


}
