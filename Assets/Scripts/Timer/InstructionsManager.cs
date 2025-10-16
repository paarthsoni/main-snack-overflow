using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InstructionsManager : MonoBehaviour
{
    
    public TextMeshProUGUI instructionsText;
    public GameObject instructionsPanel;
    public Button startButton;

    [TextArea(3, 10)]
    public string fullText = "Welcome, Agent!\n\nMission:\nRemember your targets carefully.\nOnce the clues disappear, eliminate only the correct ones.\n\nYou only have a minute. Every bullet counts!";
    public float typingSpeed = 0.03f;

    [Header("Game Flow")]
    public MemoryBarController memoryBar;   
    public TimerController timerController; 
    public GameObject[] enableOnGameplay;   

    void Start()
    {
        Time.timeScale = 0f; 
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
        instructionsPanel.SetActive(false);

        if (memoryBar != null)
            memoryBar.BeginMemoryPhase();
    }

    void HandleMemoryComplete(System.Collections.Generic.List<PathShape.ShapeType> _)
    {
        if (timerController != null)
            timerController.StartTimer(60f);

        foreach (var go in enableOnGameplay)
            if (go) go.SetActive(true);
    }
}
