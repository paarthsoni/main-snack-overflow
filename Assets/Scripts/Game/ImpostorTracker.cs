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
// Your Sunbeam defaults total ~0.8s; 0.9s is a safe cushion.
public float winDelayAfterLastHit = 0.82f;

    [Header("UI")]
    public TMP_Text impostorText;     // drag your ImpostorText here
    public GameObject winPanel;       // drag WinPanel here

    int remaining = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (winPanel) winPanel.SetActive(false); // ensure hidden on start
        UpdateUI();
    }

    // Call when beginning a new round (or on scene load if you keep this persistent)
    public void ResetCount()
    {
        remaining = 0;
        UpdateUI();
        if (winPanel) winPanel.SetActive(false);
    }

    // Call each time an impostor is spawned
    public void RegisterImpostor()
    {
        remaining++;
        UpdateUI();
    }

    // Call when an impostor is successfully killed
    public void OnImpostorKilled()
    {
        if (remaining <= 0) return; // safety
        remaining--;
        UpdateUI();

        if (remaining <= 0 && !winSequenceStarted)
        {
            winSequenceStarted = true;
            StartCoroutine(WinAfterBeam());
        }
    }

System.Collections.IEnumerator WinAfterBeam()
{
    // Wait in REAL time so it still runs even if someone pauses Time.timeScale elsewhere
    yield return new WaitForSecondsRealtime(winDelayAfterLastHit);

    // Stop the game timer
    var timer = FindObjectOfType<TimerController>(true);
    if (timer) timer.StopTimer();

    // Optionally pause whole game (like Game Over)
    if (pauseOnWin)
        Time.timeScale = 0f;

    // Optionally block clicks so nothing fires under the panel
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
