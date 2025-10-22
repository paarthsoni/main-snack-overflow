using UnityEngine;
using TMPro;

public class ImpostorTracker : MonoBehaviour
{
    public static ImpostorTracker Instance;

    [Header("Behavior")]
    public bool pauseOnWin = true;
    public bool disableClicksOnWin = true;
    bool winSequenceStarted = false;

    // How long to wait for the beam to fully play out (charge + impact + fade)
    public float winDelayAfterLastHit = 0.82f;

    [Header("UI")]
    public TMP_Text impostorText;     // optional (legacy label)
    public GameObject winPanel;       // WinPanel (optional)

    // ---- NEW: public scoreboard data ----
    public int TotalSpawned { get; private set; } = 0;
    public int Killed       { get; private set; } = 0;
    public int Remaining    => remaining;

    int remaining = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (winPanel) winPanel.SetActive(false);
        UpdateUI();
    }

    // Call when beginning a new round (or on scene load)
    public void ResetCount()
    {
        remaining = 0;
        TotalSpawned = 0;   // reset scoreboard
        Killed = 0;         // reset scoreboard
        UpdateUI();
        if (winPanel) winPanel.SetActive(false);
        winSequenceStarted = false;
    }

    // Call each time an impostor is spawned
    public void RegisterImpostor()
    {
        remaining++;
        TotalSpawned++;     // track total
        UpdateUI();
    }

    // Call when an impostor is successfully killed
    public void OnImpostorKilled()
    {
        if (remaining <= 0) return; // safety
        remaining--;
        Killed++;                    // track kills
        UpdateUI();

        if (remaining <= 0 && !winSequenceStarted)
        {
            winSequenceStarted = true;
            StartCoroutine(WinAfterBeam());
        }
    }

    System.Collections.IEnumerator WinAfterBeam()
    {
        yield return new WaitForSecondsRealtime(winDelayAfterLastHit);

        // Stop the game timer
        var timer = FindObjectOfType<TimerController>(true);
        if (timer) timer.StopTimer();

        if (pauseOnWin) Time.timeScale = 0f;

        if (disableClicksOnWin)
        {
            var click = FindObjectOfType<ClickToSmite>(true);
            if (click) click.enabled = false;
        }

        if (winPanel) winPanel.SetActive(true);
    }

    void UpdateUI()
    {
        if (impostorText) impostorText.text = $"Impostors Left: {remaining}";
    }
}
