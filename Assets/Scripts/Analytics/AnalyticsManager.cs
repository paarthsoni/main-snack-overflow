//using System;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.SceneManagement;

///// <summary>
///// Tracks per-attempt analytics metrics and posts one row to Google Forms
///// through SendToGoogle.cs.
///// Attach this to a persistent GameObject in your first scene.
///// </summary>
//[DefaultExecutionOrder(-1000)]
//public class AnalyticsManager : MonoBehaviour
//{
//    public static AnalyticsManager I { get; private set; }

//    [Header("Sender (auto-assigned if missing)")]
//    [SerializeField] private SendToGoogle sender;

//    // === Metrics collected per game attempt ===
//    private string sessionId;         // unique per player runtime
//    private string levelId;           // current Scene name
//    private int shotsFired;           // number of player shots
//    private int correctHits;          // number of correct hits
//    private float timeTakenSec;       // time taken in this attempt
//    private int completed;            // 1 = success, 0 = fail
//    private int pauseClicks;          // number of pause button presses

//    // Internals
//    private float attemptStartTime;
//    private bool attemptRunning;

//    // ----------------------------------------------------------
//    //  UNITY LIFECYCLE
//    // ----------------------------------------------------------
//    private void Awake()
//    {
//        // Singleton pattern
//        if (I != null && I != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        I = this;
//        DontDestroyOnLoad(gameObject);

//        // Generate session ID once per game runtime
//        sessionId = Guid.NewGuid().ToString("N");

//        // Initialize with current scene name
//        levelId = SceneManager.GetActiveScene().name;

//        // Auto-attach SendToGoogle if missing
//        if (sender == null)
//            sender = GetComponent<SendToGoogle>() ?? gameObject.AddComponent<SendToGoogle>();

//        // Update levelId automatically when scenes change
//        SceneManager.activeSceneChanged += (_, newScene) => levelId = newScene.name;
//    }

//    // ----------------------------------------------------------
//    //  PUBLIC HOOKS – called from gameplay scripts
//    // ----------------------------------------------------------

//    /// <summary>Called when the clue/memory phase ends and gameplay begins.</summary>
//    public void OnAttemptStart()
//    {
//        shotsFired = 0;
//        correctHits = 0;
//        timeTakenSec = 0f;
//        completed = 0;
//        // Pause clicks can either be session-wide or per-attempt.
//        // Uncomment this next line if you want them reset per attempt:
//        // pauseClicks = 0;

//        levelId = SceneManager.GetActiveScene().name;
//        attemptStartTime = Time.time;
//        attemptRunning = true;
//        Debug.Log("[Analytics] Attempt started for " + levelId);
//    }

//    /// <summary>Call every time a player fires (mouse click or beam).</summary>
//    public void OnShotFired()
//    {
//        if (!attemptRunning) return;
//        shotsFired++;
//    }

//    /// <summary>Call when a correct impostor is hit.</summary>
//    public void OnCorrectHit()
//    {
//        if (!attemptRunning) return;
//        correctHits++;
//    }

//    /// <summary>Call when Pause is clicked.</summary>
//    public void RegisterPauseClicked()
//    {
//        pauseClicks++;
//    }

//    // Optional alias if other scripts use a different name
//    public void OnPauseClicked() => RegisterPauseClicked();

//    /// <summary>Call when the player wins.</summary>
//    public void EndAttemptSuccess()
//    {
//        if (!attemptRunning) return;
//        timeTakenSec = Time.time - attemptStartTime;
//        completed = 1;
//        attemptRunning = false;
//        StartCoroutine(SendRow());
//    }

//    /// <summary>Call when the player fails (e.g., time out or all lives lost).</summary>
//    public void EndAttemptFail()
//    {
//        if (!attemptRunning) return;
//        timeTakenSec = Time.time - attemptStartTime;
//        completed = 0;
//        attemptRunning = false;
//        StartCoroutine(SendRow());
//    }

//    // ----------------------------------------------------------
//    //  DATA SENDER
//    // ----------------------------------------------------------
//    private IEnumerator SendRow()
//    {
//        float accuracyPercent =
//            (shotsFired > 0) ? (correctHits / (float)shotsFired) * 100f : 0f;

//        Debug.Log($"[Analytics] Sending row → session={sessionId}, level={levelId}, " +
//                  $"shots={shotsFired}, correct={correctHits}, acc={accuracyPercent:0.##}%, " +
//                  $"time={timeTakenSec:0.##}s, completed={completed}, pauses={pauseClicks}");

//        yield return sender.SendAttemptRow(
//            sessionId: sessionId,
//            levelId: levelId,
//            shotsFired: shotsFired,
//            correctHits: correctHits,
//            accuracyPercent: accuracyPercent,
//            timeTakenSec: timeTakenSec,
//            completed: completed,
//            pauseClicks: pauseClicks
//        );
//    }
//}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks per-attempt analytics metrics and posts one row to Google Forms
/// (or an Apps Script relay) through SendToGoogle.cs.
/// Attach this to a persistent GameObject in your first scene.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager I { get; private set; }

    [Header("Sender (auto-assigned if missing)")]
    [SerializeField] private SendToGoogle sender;

    // === Metrics collected per game attempt ===
    private string sessionId;         // unique per player runtime
    private string levelId;           // current Scene name
    private int shotsFired;           // number of player shots
    private int correctHits;          // number of correct hits
    private float timeTakenSec;       // time taken in this attempt
    private int completed;            // 1 = success, 0 = fail
    private int pauseClicks;          // number of pause button presses

    // Internals
    private float attemptStartTime;
    private bool attemptRunning;

    private void Awake()
    {
        // Singleton
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        sessionId = Guid.NewGuid().ToString("N");
        levelId = SceneManager.GetActiveScene().name;

        // Ensure sender
        if (sender == null)
            sender = GetComponent<SendToGoogle>() ?? gameObject.AddComponent<SendToGoogle>();

        // Keep levelId in sync if you add more scenes later
        SceneManager.activeSceneChanged += (_, newScene) => levelId = newScene.name;
    }

    // -------- Public hooks --------

    /// <summary>Call when clues close and actual gameplay starts.</summary>
    public void OnAttemptStart()
    {
        shotsFired = 0;
        correctHits = 0;
        timeTakenSec = 0f;
        completed = 0;
        // If you want pause clicks per attempt (not per session), uncomment:
        // pauseClicks = 0;

        levelId = SceneManager.GetActiveScene().name;
        attemptStartTime = Time.time;
        attemptRunning = true;
        Debug.Log("[Analytics] Attempt started for " + levelId);
    }

    /// <summary>Call every time the player fires.</summary>
    public void OnShotFired()
    {
        if (!attemptRunning) return;
        shotsFired++;
    }

    /// <summary>Call when a correct impostor is hit.</summary>
    public void OnCorrectHit()
    {
        if (!attemptRunning) return;
        correctHits++;
    }

    /// <summary>Call when Pause is clicked.</summary>
    public void RegisterPauseClicked()
    {
        pauseClicks++;
    }
    public void OnPauseClicked() => RegisterPauseClicked();

    /// <summary>Call on win.</summary>
    public void EndAttemptSuccess()
    {
        if (!attemptRunning) return;
        timeTakenSec = Time.time - attemptStartTime;
        completed = 1;
        attemptRunning = false;
        StartCoroutine(SendRow());
    }

    /// <summary>Call on fail/timeout.</summary>
    public void EndAttemptFail()
    {
        if (!attemptRunning) return;
        timeTakenSec = Time.time - attemptStartTime;
        completed = 0;
        attemptRunning = false;
        StartCoroutine(SendRow());
    }

    // -------- Sender --------
    private IEnumerator SendRow()
    {
        float accuracyPercent = (shotsFired > 0)
            ? (correctHits / (float)shotsFired) * 100f
            : 0f;

        Debug.Log($"[Analytics] Sending row → session={sessionId}, level={levelId}, " +
                  $"shots={shotsFired}, correct={correctHits}, acc={accuracyPercent:0.##}%, " +
                  $"time={timeTakenSec:0.##}s, completed={completed}, pauses={pauseClicks}");

        yield return sender.SendAttemptRow(
            sessionId: sessionId,
            levelId: levelId,
            shotsFired: shotsFired,
            correctHits: correctHits,
            accuracyPercent: accuracyPercent,
            timeTakenSec: timeTakenSec,
            completed: completed,
            pauseClicks: pauseClicks
        );
    }
}

