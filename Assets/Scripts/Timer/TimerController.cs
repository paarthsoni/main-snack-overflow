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

    void Start()
    {
        currentTime = startTime;
        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0, startTime);

        UpdateTimerUI();

        if (currentTime <= 0)
        {
            GameOver();
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // pause game
    }

    // Called by Retry button in UI
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
