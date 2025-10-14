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

    [Header("Typing Settings")]
    [TextArea(3, 10)]
    public string fullText = "Welcome, Agent!\n\nMission:\nRemember your targets carefully.\nOnce the clues disappear, eliminate only the correct ones.\n\nYou have 3 minutes. Every bullet counts!";
    public float typingSpeed = 0.03f;

    void Start()
    {
        Time.timeScale = 0f; // pause gameplay
        startButton.gameObject.SetActive(false); // hide Start until text finishes
        StartCoroutine(TypeText());
        startButton.onClick.AddListener(OnStartClicked);
    }

    IEnumerator TypeText()
    {
        instructionsText.text = "";
        foreach (char c in fullText)
        {
            instructionsText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed); // unaffected by Time.timeScale
        }
        yield return new WaitForSecondsRealtime(0.3f);
        startButton.gameObject.SetActive(true); // show Start after typing completes
    }

    void OnStartClicked()
    {
        instructionsPanel.SetActive(false);
        Time.timeScale = 1f; // resume game
    }
}
