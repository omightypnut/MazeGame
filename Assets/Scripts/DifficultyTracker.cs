using UnityEngine;

public class DifficultyTracker : MonoBehaviour
{
    public static DifficultyTracker Instance;

    [Range(0f, 1f)]
    public float difficultyScore = 0.3f;

    [Header("Difficulty Limits")]
    [SerializeField] private float minDifficulty = 0.1f;
    [SerializeField] private float maxDifficulty = 1f;

    [Header("Smoothing")]
    [SerializeField] private float smoothing = 0.35f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateDifficulty(int roundsSurvived, int itemsCollected, int timesCaught, int timesSpotted, float roundLength)
    {
        float itemScore = itemsCollected * 0.15f;
        float surviveScore = 0.2f;
        float caughtPenalty = timesCaught * 0.25f;
        float spottedPenalty = timesSpotted * 0.025f;

        float rawScore = itemScore + surviveScore - caughtPenalty - spottedPenalty;

        float targetDifficulty = Mathf.Clamp01(0.5f + rawScore);
        targetDifficulty = Mathf.Clamp(targetDifficulty, minDifficulty, maxDifficulty);

        difficultyScore = Mathf.Lerp(difficultyScore, targetDifficulty, smoothing);

        Debug.Log($"Difficulty updated. Items: {itemsCollected}, Caught: {timesCaught}, Spotted: {timesSpotted}, New Difficulty: {difficultyScore:F2}");
    }
}