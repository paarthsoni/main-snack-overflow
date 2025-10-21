//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections;

//public class InstructionsManager : MonoBehaviour
//{
//    public TextMeshProUGUI instructionsText;
//    public GameObject instructionsPanel;
//    public Button startButton;

//    [TextArea(3, 10)]
//    public string fullText =
//        "Welcome, Agent!\n\n" +
//        "Mission:\nYou will receive clues about imposters — their color and the shape in which they roam the city.\n" +
//        "Memorize these carefully.\nOnce the clues disappear, find and eliminate all imposters by matching both color and shape.\n\n" +
//        "You have 1 minute to kill them all. Good luck!";
//    public float typingSpeed = 0.03f;

//    [Header("Game Flow")]
//    public MemoryBarController memoryBar;
//    public TimerController timerController;
//    public GameObject[] enableOnGameplay;

//    void Start()
//    {
//        Time.timeScale = 0f; 
//        startButton.gameObject.SetActive(false);
//        StartCoroutine(TypeText());
//        startButton.onClick.AddListener(OnStartClicked);

//        if (memoryBar != null)
//            memoryBar.OnMemoryPhaseComplete += HandleMemoryComplete;  // now matches Action
//    }

//    void OnDestroy()
//    {
//        startButton.onClick.RemoveListener(OnStartClicked);
//        if (memoryBar != null)
//            memoryBar.OnMemoryPhaseComplete -= HandleMemoryComplete;
//    }

//    IEnumerator TypeText()
//    {
//        instructionsText.text = "";
//        foreach (char c in fullText)
//        {
//            instructionsText.text += c;
//            yield return new WaitForSecondsRealtime(typingSpeed);
//        }
//        yield return new WaitForSecondsRealtime(0.3f);
//        startButton.gameObject.SetActive(true);
//    }

//    void OnStartClicked()
//    {
//        instructionsPanel.SetActive(false);

//        if (memoryBar != null)
//            memoryBar.BeginMemoryPhase();   // pauses & shows clues, then fires event
//    }

//    // CHANGED: no parameters
//    void HandleMemoryComplete()
//    {
//        if (timerController != null)
//            timerController.StartTimer(60f);
//        FindObjectOfType<SpawnManager>()?.StartSpawning();


//        if (enableOnGameplay != null)
//            foreach (var go in enableOnGameplay)
//                if (go) go.SetActive(true);
//    }
//}

// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;

// public class InstructionsManager : MonoBehaviour
// {
//     public TextMeshProUGUI instructionsText;
//     public GameObject instructionsPanel;
//     public Button startButton;

//     [TextArea(3, 10)]
//     public string fullText =
//         "Welcome, Agent!\n\n" +
//         "Mission:\nYou will receive clues about imposters — their color and the shape in which they roam the city.\n" +
//         "Memorize these carefully.\nOnce the clues disappear, find and eliminate all imposters by matching both color and shape.\n\n" +
//         "You have 1 minute to kill them all. Good luck!";

//     public float typingSpeed = 0.03f;

//     [Header("Game Flow")]
//     public MemoryBarController memoryBar;
//     public TimerController timerController;
//     public GameObject[] enableOnGameplay;

    
//     void Start()
//     {
//         Time.timeScale = 0f;
//         startButton.gameObject.SetActive(false);
//         startButton.onClick.AddListener(OnStartClicked);

//         if (memoryBar != null)
//             memoryBar.OnMemoryPhaseComplete += HandleMemoryComplete;

        
//         instructionsPanel.SetActive(false); 
//     }

//     void OnDestroy()
//     {
//         startButton.onClick.RemoveListener(OnStartClicked);
//         if (memoryBar != null)
//             memoryBar.OnMemoryPhaseComplete -= HandleMemoryComplete;
//     }

    
//     public void StartInstructions()
//     {
//         instructionsPanel.SetActive(true);
//         StartCoroutine(TypeText());
//     }

//     public IEnumerator TypeText()
//     {
//         instructionsText.text = "";
//         foreach (char c in fullText)
//         {
//             instructionsText.text += c;
//             yield return new WaitForSecondsRealtime(typingSpeed);
//         }
//         yield return new WaitForSecondsRealtime(0.6f);
//         startButton.gameObject.SetActive(true);
//     }

//     void OnStartClicked()
//     {
//         instructionsPanel.SetActive(false);

//         if (memoryBar != null)
//             memoryBar.BeginMemoryPhase();   
//     }
// void HandleMemoryComplete()
// {
   
//     if (timerController != null)
//         timerController.StartTimer(60f);

//     FindObjectOfType<SpawnManager>()?.StartSpawning();

//     if (enableOnGameplay != null)
//         foreach (var go in enableOnGameplay)
//             if (go) go.SetActive(true);
// }

// }



using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InstructionsManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI instructionsText;
    public GameObject instructionsPanel;
    public Button startButton;

    [TextArea(3, 10)]
    public string fullText =
        "Memorize the shapes and colors.\n\n" +
        "• The color represents the impostor’s color.\n" +
        "• The shape represents the impostor’s movement path.\n\n" +
        "Find and eliminate impostors that match both color and shape.\n" +
        "You only have 3 beams to catch them!\n\n" +
        "Controls: WASD / Arrow Keys";

    public float typingSpeed = 0.02f;

    [Header("Game Flow")]
    public MemoryBarController memoryBar;
    public TimerController timerController;
    public GameObject[] enableOnGameplay;

    void Start()
    {
        Time.timeScale = 0f;
        startButton.gameObject.SetActive(false);
        startButton.onClick.AddListener(OnStartClicked);

        if (memoryBar != null)
            memoryBar.OnMemoryPhaseComplete += HandleMemoryComplete;

        instructionsPanel.SetActive(false);
    }

    void OnDestroy()
    {
        startButton.onClick.RemoveListener(OnStartClicked);
        if (memoryBar != null)
            memoryBar.OnMemoryPhaseComplete -= HandleMemoryComplete;
    }

    public void StartInstructions()
    {
        instructionsPanel.SetActive(true);
        StartCoroutine(TypeText());
    }

    public IEnumerator TypeText()
    {
        instructionsText.text = "";
        foreach (char c in fullText)
        {
            instructionsText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        startButton.gameObject.SetActive(true);
    }

    void OnStartClicked()
    {
        instructionsPanel.SetActive(false);
        if (memoryBar != null)
            memoryBar.BeginMemoryPhase();
    }

    void HandleMemoryComplete()
    {
        if (timerController != null)
            timerController.StartTimer(60f);

        FindObjectOfType<SpawnManager>()?.StartSpawning();

        if (enableOnGameplay != null)
        {
            foreach (var go in enableOnGameplay)
                if (go) go.SetActive(true);
        }
    }
}


// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;

// public class InstructionsManager : MonoBehaviour
// {
//     public TextMeshProUGUI instructionsText;
//     public GameObject instructionsPanel;
//     public Button startButton;

//     [TextArea(3, 10)]
//     public string fullText =
//         "Memorize the shapes and colors.\n\n" +
//         "• The color represents the impostor’s color.\n" +
//         "• The shape represents the impostor’s movement path.\n\n" +
//         "Find and eliminate impostors that match both color and shape.\n" +
//         "You only have 3 beams to catch them!\n\n" +
//         "Controls: WASD / Arrow Keys";

//     [Header("Game Flow")]
//     public MemoryBarController memoryBar;
//     public TimerController timerController;
//     public GameObject[] enableOnGameplay;

//     void Start()
//     {
//         Time.timeScale = 0f;
//         startButton.gameObject.SetActive(false);
//         startButton.onClick.AddListener(OnStartClicked);

//         if (memoryBar != null)
//             memoryBar.OnMemoryPhaseComplete += HandleMemoryComplete;

//         instructionsPanel.SetActive(false);
//     }

//     void OnDestroy()
//     {
//         startButton.onClick.RemoveListener(OnStartClicked);
//         if (memoryBar != null)
//             memoryBar.OnMemoryPhaseComplete -= HandleMemoryComplete;
//     }

//     public void StartInstructions()
//     {
//         instructionsPanel.SetActive(true);
//         instructionsText.text = fullText; // instantly show text
//         startButton.gameObject.SetActive(true);
//     }

//     void OnStartClicked()
//     {
//         instructionsPanel.SetActive(false);

//         if (memoryBar != null)
//             memoryBar.BeginMemoryPhase();
//     }

//     void HandleMemoryComplete()
//     {
//         if (timerController != null)
//             timerController.StartTimer(60f);

//         FindObjectOfType<SpawnManager>()?.StartSpawning();

//         if (enableOnGameplay != null)
//             foreach (var go in enableOnGameplay)
//                 if (go) go.SetActive(true);
//     }
// }
