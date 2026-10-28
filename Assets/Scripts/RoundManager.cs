using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    [Header("Round Settings")]
    [SerializeField] private float roundLength = 30f;

    [Header("Lives")]
    [SerializeField] private int startingLives = 3;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundsSurvivedText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;

    private float timeRemaining;
    private int roundsSurvived = 0;
    private int currentLives;

    private int itemsCollectedThisRound = 0;
    private int timesCaughtThisRound = 0;
    private int timesSpottedThisRound = 0;

    private bool isGameOver = false;

    public int RoundsSurvived => roundsSurvived;
    public float TimeRemaining => timeRemaining;
    public int CurrentLives => currentLives;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentLives = startingLives;
        ResetTimer();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
        UpdateUI();
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                RestartGame();
            }
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            EndRound();
        }

        UpdateUI();
    }

    private void EndRound()
    {
        roundsSurvived++;

        if (DifficultyTracker.Instance != null)
        {
            DifficultyTracker.Instance.UpdateDifficulty(
                roundsSurvived,
                itemsCollectedThisRound,
                timesCaughtThisRound,
                timesSpottedThisRound,
                roundLength
            );
        }

        if (CollectibleSpawner.Instance != null)
        {
            CollectibleSpawner.Instance.RespawnForNewRound();
        }

        ResetRoundStats();
        ResetTimer();
        UpdateUI();
    }

    private void ResetTimer()
    {
        timeRemaining = roundLength;
    }

    private void ResetRoundStats()
    {
        itemsCollectedThisRound = 0;
        timesCaughtThisRound = 0;
        timesSpottedThisRound = 0;
    }

    public void RegisterItemCollected()
    {
        itemsCollectedThisRound++;
        UpdateUI();
    }

    public void RegisterCaught()
    {
        timesCaughtThisRound++;
        UpdateUI();
    }

    public void RegisterSpotted()
    {
        timesSpottedThisRound++;
    }

    public void LoseLife()
    {
        if (isGameOver)
            return;

        currentLives--;

        if (currentLives < 0)
            currentLives = 0;

        UpdateUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
        {
            gameOverText.text =
                "GAME OVER\n" +
                $"Rounds Survived: {roundsSurvived}\n" +
                "Press Space or Enter to Play Again";
        }

        Debug.Log("Game Over");
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int totalSeconds = Mathf.CeilToInt(timeRemaining);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }

        if (roundsSurvivedText != null)
        {
            roundsSurvivedText.text = $"Rounds Survived: {roundsSurvived}";
        }

        if (difficultyText != null && DifficultyTracker.Instance != null)
        {
            difficultyText.text = $"Difficulty: {DifficultyTracker.Instance.difficultyScore:F2}";
        }

        if (livesText != null)
        {
            livesText.text = $"Lives: {currentLives}";
        }
    }
}