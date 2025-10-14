using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// public class InstructionsManager : MonoBehaviour
// {
//     [Header("UI References")]
//     public TextMeshProUGUI instructionsText;
//     public GameObject instructionsPanel;
//     public Button startButton;

//     [Header("Typing Settings")]
//     [TextArea(3, 10)]
//     public string fullText = "Welcome, Agent!\n\nMission:\nRemember your targets carefully.\nOnce the clues disappear, eliminate only the correct ones.\n\nYou have 3 minutes. Every bullet counts!";
//     public float typingSpeed = 0.03f;

//     void Start()
//     {
//         Time.timeScale = 0f; // pause gameplay
//         startButton.gameObject.SetActive(false); // hide Start until text finishes
//         StartCoroutine(TypeText());
//         startButton.onClick.AddListener(OnStartClicked);
//     }

//     IEnumerator TypeText()
//     {
//         instructionsText.text = "";
//         foreach (char c in fullText)
//         {
//             instructionsText.text += c;
//             yield return new WaitForSecondsRealtime(typingSpeed); // unaffected by Time.timeScale
//         }
//         yield return new WaitForSecondsRealtime(0.3f);
//         startButton.gameObject.SetActive(true); // show Start after typing completes
//     }

//     void OnStartClicked()
//     {
//         instructionsPanel.SetActive(false);
//         Time.timeScale = 1f; // resume game
//     }
// }


// ... top of file unchanged ...

public class InstructionsManager : MonoBehaviour
{
    // (existing fields)
    public TextMeshProUGUI instructionsText;
    public GameObject instructionsPanel;
    public Button startButton;

    [TextArea(3, 10)]
    public string fullText = "Welcome, Agent!\n\nMission:\nRemember your targets carefully.\nOnce the clues disappear, eliminate only the correct ones.\n\nYou have 3 minutes. Every bullet counts!";
    public float typingSpeed = 0.03f;

    // NEW: references
    [Header("Game Flow")]
    public MemoryBarController memoryBar;   // drag in Inspector
    public TimerController timerController; // drag in Inspector
    public GameObject[] enableOnGameplay;   // optional: spawn managers etc.

    void Start()
    {
        Time.timeScale = 0f; // keep game paused
        startButton.gameObject.SetActive(false);
        StartCoroutine(TypeText());
        startButton.onClick.AddListener(OnStartClicked);

        if (memoryBar != null)
            memoryBar.OnMemoryPhaseComplete += HandleMemoryComplete;
    }

    void OnDestroy()
    {
        startButton.onClick.RemoveListener(OnStartClicked);
        if (memoryBar != null)
            memoryBar.OnMemoryPhaseComplete -= HandleMemoryComplete;
    }

    IEnumerator TypeText()
    {
        instructionsText.text = "";
        foreach (char c in fullText)
        {
            instructionsText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        yield return new WaitForSecondsRealtime(0.3f);
        startButton.gameObject.SetActive(true);
    }

    void OnStartClicked()
    {
        // Hide instructions; DO NOT unpause now.
        instructionsPanel.SetActive(false);

        // Show memory bar (it pauses, blurs, runs intro & countdown internally)
        if (memoryBar != null)
            memoryBar.BeginMemoryPhase();
    }

    void HandleMemoryComplete(System.Collections.Generic.List<PathShape.ShapeType> _)
    {
        // Memory bar has unpaused the game here. Start the 3-minute timer.
        if (timerController != null)
            timerController.StartTimer(180f);

        foreach (var go in enableOnGameplay)
            if (go) go.SetActive(true);
    }
}
