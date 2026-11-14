using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class QuizBossManager : MonoBehaviour
{
    public static QuizBossManager Instance;

    [Header("UI Canvas")]
    public GameObject quizCanvasPanel; // The main screen for the boss fight

    [Header("Data")]
    [Tooltip("The database containing the questions grouped by species")]
    public QuizDatabase quizDatabase;
    [Tooltip("The zone name this boss belongs to (used for wiping memory on lose)")]
    public string zoneName = "Sunlight Zone";
    [Tooltip("The Species ID of the boss to unlock in the Bestiary upon winning")]
    public string bossSpeciesID = "SunlightBoss";

    [Header("Health UI")]
    public Image playerHpBar;
    public Image bossHpBar;
    
    [Header("Quiz UI")]
    public TMP_Text questionText;
    public Button[] answerButtons = new Button[4];
    private TMP_Text[] answerTexts = new TMP_Text[4];

    [Header("Win/Lose Settings")]
    public int correctAnswersToWin = 5;
    public int mistakesToLose = 2;

    [Header("Feedback")]
    public TMP_Text feedbackText; // E.g., "Correct!" or "Wrong!"

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSfx;
    public AudioClip wrongSfx;

    // State
    private int currentCorrect = 0;
    private int currentMistakes = 0;
    private List<QuizQuestion> currentQuizQueue = new List<QuizQuestion>();
    private QuizQuestion currentQuestion;
    private bool isWaitingForNext = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Get text components from buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerTexts[i] = answerButtons[i].GetComponentInChildren<TMP_Text>();
                
                int index = i; // capture index for closure
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
        }
    }

    private void Start()
    {
        if (quizCanvasPanel != null) quizCanvasPanel.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    public void StartQuiz()
    {
        if (quizDatabase == null || quizDatabase.speciesPools == null || quizDatabase.speciesPools.Count == 0)
        {
            Debug.LogError("Quiz Database is empty or missing! Cannot start quiz.");
            return;
        }

        // Reset state
        currentCorrect = 0;
        currentMistakes = 0;
        UpdateHpBars();
        
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        isWaitingForNext = false;

        // Generate Quiz Queue: 1 random question per species
        currentQuizQueue.Clear();
        foreach (var pool in quizDatabase.speciesPools)
        {
            if (pool.questions != null && pool.questions.Count > 0)
            {
                int randomIndex = Random.Range(0, pool.questions.Count);
                currentQuizQueue.Add(pool.questions[randomIndex]);
            }
        }

        // Shuffle the final queue so species order is random
        ShuffleQueue(currentQuizQueue);

        // Show UI and pause game behind the canvas
        quizCanvasPanel.SetActive(true);
        Time.timeScale = 0f;

        ShowNextQuestion();
    }

    private void ShowNextQuestion()
    {
        if (currentQuizQueue.Count == 0)
        {
            // Ran out of questions? Should theoretically not happen if they hit win/lose first, 
            // but just in case, treat as a draw/retry
            EndQuizLose("Ran out of questions!");
            return;
        }

        currentQuestion = currentQuizQueue[0];
        currentQuizQueue.RemoveAt(0);

        // Update UI
        if (questionText != null) questionText.text = currentQuestion.questionText;

        for (int i = 0; i < 4; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].interactable = true;
                if (answerTexts[i] != null && i < currentQuestion.answers.Length)
                {
                    answerTexts[i].text = currentQuestion.answers[i];
                }
            }
        }
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        if (isWaitingForNext) return;
        isWaitingForNext = true;

        // Disable buttons immediately
        foreach (var btn in answerButtons)
        {
            if (btn != null) btn.interactable = false;
        }

        if (selectedIndex == currentQuestion.correctAnswerIndex)
        {
            // Correct! Boss takes damage
            currentCorrect++;
            ShowFeedback("Correct!", Color.green);
            if (audioSource != null && correctSfx != null) audioSource.PlayOneShot(correctSfx);
        }
        else
        {
            // Wrong! Player takes damage
            currentMistakes++;
            ShowFeedback("Wrong!", Color.red);
            if (audioSource != null && wrongSfx != null) audioSource.PlayOneShot(wrongSfx);
        }

        UpdateHpBars();

        // Check Win/Lose conditions BEFORE moving to next
        if (currentCorrect >= correctAnswersToWin)
        {
            StartCoroutine(WaitAndEndQuiz(true, "Boss Defeated!"));
        }
        else if (currentMistakes >= mistakesToLose)
        {
            StartCoroutine(WaitAndEndQuiz(false, "You Died!"));
        }
        else
        {
            // Continue to next question
            StartCoroutine(WaitAndNextQuestion());
        }
    }

    private void ShowFeedback(string text, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = text;
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);
        }
    }

    private void UpdateHpBars()
    {
        // Player HP is depleted by mistakes (Starts at 1, goes down per mistake)
        if (playerHpBar != null)
        {
            float playerHpPercent = 1f - ((float)currentMistakes / mistakesToLose);
            playerHpBar.fillAmount = Mathf.Clamp01(playerHpPercent);
        }

        // Boss HP is depleted by correct answers
        if (bossHpBar != null)
        {
            float bossHpPercent = 1f - ((float)currentCorrect / correctAnswersToWin);
            bossHpBar.fillAmount = Mathf.Clamp01(bossHpPercent);
        }
    }

    private IEnumerator WaitAndNextQuestion()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        
        isWaitingForNext = false;
        ShowNextQuestion();
    }

    private IEnumerator WaitAndEndQuiz(bool isWin, string finalMessage)
    {
        ShowFeedback(finalMessage, isWin ? Color.cyan : Color.red);
        yield return new WaitForSecondsRealtime(2f);

        if (isWin)
        {
            EndQuizWin();
        }
        else
        {
            EndQuizLose(finalMessage);
        }
    }

    private void EndQuizWin()
    {
        quizCanvasPanel.SetActive(false);
        Time.timeScale = 1f;

        // Player is now allowed to pass into the next zone
        // ZoneGateManager already marked them as hasPassedGate = true.
        Debug.Log("Quiz won! Boss defeated.");
        
        if (BestiaryUI.Instance != null && !string.IsNullOrEmpty(bossSpeciesID))
        {
            BestiaryUI.Instance.UnlockSpecies(bossSpeciesID);
            
            // Optional Notification
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification("Boss added to bestiary!", bossSpeciesID);
            }
        }
        
        // Optional: Show a victory popup
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup("Victory", "You defeated the boss with your knowledge! The path to the Twilight Zone is now open.");
        }
    }

    private void EndQuizLose(string reason)
    {
        quizCanvasPanel.SetActive(false);
        Time.timeScale = 1f;

        Debug.LogWarning($"Quiz lost: {reason}");

        // 1. Wipe Bestiary progress for this zone
        if (BestiaryUI.Instance != null)
        {
            BestiaryUI.Instance.WipeZoneDiscoveries(zoneName);
        }
        
        // 1.5. Reset all species physically in the scene so the player can re-scan them
        DiscoverableSpecies[] allSpecies = FindObjectsOfType<DiscoverableSpecies>();
        foreach (var species in allSpecies)
        {
            species.ResetDiscovery();
        }

        // 2. Reset Player Position back to the surface (Y=0)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(player.transform.position.x, 0f, player.transform.position.z);
            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // 3. Reset ZoneGateManager so they get blocked again at 200m
        ZoneGateManager gate = FindObjectOfType<ZoneGateManager>();
        if (gate != null)
        {
            // Reset state using reflection or just make sure they hit the barrier again
            // For now, they are at Y=0, so when they swim down, they will hit the gate again
            // We need to re-lock the gate. Since `hasPassedGate` and `hasShownWarning` are private,
            // we should expose a ResetGate method in ZoneGateManager.
            gate.ResetGate();
        }

        // Show a defeat popup
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup("Defeat", "Your knowledge was insufficient. Your memory of the Sunlight Zone creatures has faded. You have been washed back to the surface.");
        }
    }

    private void ShuffleQueue(List<QuizQuestion> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            QuizQuestion temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
