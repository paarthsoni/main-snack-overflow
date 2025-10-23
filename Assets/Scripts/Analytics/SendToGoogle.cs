using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// Sends one analytics row (per attempt) to your Google Form.
public class SendToGoogle : MonoBehaviour
{
    [Header("Google Form")]
    [Tooltip("Use the hidden ACTION URL ending in /formResponse (not the public viewform link).")]
    [SerializeField] private string formURL = "https://docs.google.com/forms/d/e/1FAIpQLSeYAPf5Iatz8b0AW7qiql8Y_ayzh9CQJz4IDQOzT1RyadnN5Q/formResponse";
    

    [Header("Form entry.* names (exactly your 8 fields)")]
    [SerializeField] private string e_sessionId = "entry.1330219283";
    [SerializeField] private string e_levelId = "entry.1013061301";
    [SerializeField] private string e_shotsFired = "entry.650746069";
    [SerializeField] private string e_correctHits = "entry.1310355990";
    [SerializeField] private string e_accuracyPct = "entry.792927461";
    [SerializeField] private string e_timeTakenSec = "entry.1684774599";
    [SerializeField] private string e_completed = "entry.1419990853"; // "1" or "0"
    [SerializeField] private string e_pauseClicks = "entry.1105048103";

    public IEnumerator SendAttemptRow(
        string sessionId,
        string levelId,
        int shotsFired,
        int correctHits,
        float accuracyPercent,
        float timeTakenSec,
        int completed,   // 1 or 0
        int pauseClicks
    )
    {
        WWWForm f = new WWWForm();
        f.AddField(e_sessionId, sessionId);
        f.AddField(e_levelId, levelId);
        f.AddField(e_shotsFired, shotsFired.ToString());
        f.AddField(e_correctHits, correctHits.ToString());
        f.AddField(e_accuracyPct, accuracyPercent.ToString("0.##"));
        f.AddField(e_timeTakenSec, timeTakenSec.ToString("0.##"));
        f.AddField(e_completed, completed.ToString());
        f.AddField(e_pauseClicks, pauseClicks.ToString());

        using (UnityWebRequest req = UnityWebRequest.Post(formURL, f))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError("[Analytics] POST failed: " + req.error);
            else
                Debug.Log("[Analytics] Google Sheet row added.");
        }
    }
}
