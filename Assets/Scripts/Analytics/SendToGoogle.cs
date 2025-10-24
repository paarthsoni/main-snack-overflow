//using System.Collections;
//using UnityEngine;
//using UnityEngine.Networking;

///// Sends one analytics row (per attempt) to your Google Form.
//public class SendToGoogle : MonoBehaviour
//{
//    [Header("Google Form")]
//    [Tooltip("Use the hidden ACTION URL ending in /formResponse (not the public viewform link).")]
//    [SerializeField] private string formURL = "https://docs.google.com/forms/d/e/1FAIpQLSeYAPf5Iatz8b0AW7qiql8Y_ayzh9CQJz4IDQOzT1RyadnN5Q/formResponse";


//    [Header("Form entry.* names (exactly your 8 fields)")]
//    [SerializeField] private string e_sessionId = "entry.1330219283";
//    [SerializeField] private string e_levelId = "entry.1013061301";
//    [SerializeField] private string e_shotsFired = "entry.650746069";
//    [SerializeField] private string e_correctHits = "entry.1310355990";
//    [SerializeField] private string e_accuracyPct = "entry.792927461";
//    [SerializeField] private string e_timeTakenSec = "entry.1684774599";
//    [SerializeField] private string e_completed = "entry.1419990853"; // "1" or "0"
//    [SerializeField] private string e_pauseClicks = "entry.1105048103";

//    public IEnumerator SendAttemptRow(
//        string sessionId,
//        string levelId,
//        int shotsFired,
//        int correctHits,
//        float accuracyPercent,
//        float timeTakenSec,
//        int completed,   // 1 or 0
//        int pauseClicks
//    )
//    {
//        WWWForm f = new WWWForm();
//        f.AddField(e_sessionId, sessionId);
//        f.AddField(e_levelId, levelId);
//        f.AddField(e_shotsFired, shotsFired.ToString());
//        f.AddField(e_correctHits, correctHits.ToString());
//        f.AddField(e_accuracyPct, accuracyPercent.ToString("0.##"));
//        f.AddField(e_timeTakenSec, timeTakenSec.ToString("0.##"));
//        f.AddField(e_completed, completed.ToString());
//        f.AddField(e_pauseClicks, pauseClicks.ToString());

//        using (UnityWebRequest req = UnityWebRequest.Post(formURL, f))
//        {
//            yield return req.SendWebRequest();
//            if (req.result != UnityWebRequest.Result.Success)
//                Debug.LogError("[Analytics] POST failed: " + req.error);
//            else
//                Debug.Log("[Analytics] Google Sheet row added.");
//        }
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendToGoogle : MonoBehaviour
{
    [Header("URLs")]
    [Tooltip("Direct Google Form /formResponse endpoint (Editor/Standalone)")]
    [SerializeField]
    private string directFormURL =
        "https://docs.google.com/forms/d/e/1FAIpQLSeYAPf5Iatz8b0AW7qiql8Y_ayzh9CQJz4IDQOzT1RyadnN5Q/formResponse";

    [Tooltip("Apps Script Web App URL (WebGL relay; returns CORS headers)")]
    [SerializeField]
    private string webglProxyURL =
        "https://script.google.com/macros/s/AKfycbxyXymUWxHgFTOnetRSzfjaW3TFVetcLdYIURI6rZa2oBaBbch7OMqg3E2xfdMuCseo/exec";

    [SerializeField, Tooltip("Resolved at runtime based on platform")]
    private string formURL;

    [Header("Optional security (for your relay)")]
    [SerializeField] private string sharedSecret = ""; // set to match your Apps Script if used

    [Header("Google Form entry.* field names")]
    [SerializeField] private string e_sessionId = "entry.1330219283";
    [SerializeField] private string e_levelId = "entry.1013061301";
    [SerializeField] private string e_shotsFired = "entry.650746069";
    [SerializeField] private string e_correctHits = "entry.1310355990";
    [SerializeField] private string e_accuracyPct = "entry.792927461";
    [SerializeField] private string e_timeTakenSec = "entry.1684774599";
    [SerializeField] private string e_completed = "entry.1419990853";
    [SerializeField] private string e_pauseClicks = "entry.1105048103";

    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        formURL = webglProxyURL;   // CORS-friendly relay for WebGL
#else
        formURL = directFormURL;   // direct Google Forms endpoint
#endif
    }

    public IEnumerator SendAttemptRow(
        string sessionId,
        string levelId,
        int shotsFired,
        int correctHits,
        float accuracyPercent,
        float timeTakenSec,
        int completed,
        int pauseClicks
    )
    {
        var f = new WWWForm();
        f.AddField(e_sessionId, sessionId);
        f.AddField(e_levelId, levelId);
        f.AddField(e_shotsFired, shotsFired.ToString());
        f.AddField(e_correctHits, correctHits.ToString());
        f.AddField(e_accuracyPct, accuracyPercent.ToString("0.##"));
        f.AddField(e_timeTakenSec, timeTakenSec.ToString("0.##"));
        f.AddField(e_completed, completed.ToString());
        f.AddField(e_pauseClicks, pauseClicks.ToString());

        if (!string.IsNullOrEmpty(sharedSecret))
            f.AddField("secret", sharedSecret); // only your relay cares; ignored by Forms

        using (var req = UnityWebRequest.Post(formURL, f))
        {
            req.timeout = 15;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Analytics] POST failed ({req.responseCode}): {req.error}\nBody: {req.downloadHandler?.text}");
            }
            else
            {
                Debug.Log($"[Analytics] OK ({req.responseCode}).");
            }
        }
    }
}

