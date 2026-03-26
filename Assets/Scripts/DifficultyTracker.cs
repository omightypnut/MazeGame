using UnityEngine;

[System.Serializable]
public class PlayerSessionData
{
    public float timeHidden;
    public int timesFound;
    public int timesEscaped;
}

public class DifficultyTracker : MonoBehaviour
{
    public static DifficultyTracker Instance;

    public PlayerSessionData session = new PlayerSessionData();
    public float difficultyScore = 0.5f;

    private float trackTimer = 0f;
    public float updateInterval = 10f;

    void Awake() => Instance = this;

    void Update()
    {
        session.timeHidden += Time.deltaTime;

        trackTimer += Time.deltaTime;
        if (trackTimer >= updateInterval)
        {
            RecalculateDifficulty();
            trackTimer = 0f;
        }
    }

    public void PlayerWasFound() => session.timesFound++;
    public void PlayerEscaped() => session.timesEscaped++;

    void RecalculateDifficulty()
    {
        float escapeRatio = session.timesEscaped /
            Mathf.Max(1, session.timesFound + session.timesEscaped);

        difficultyScore = Mathf.Clamp01(
            difficultyScore + (escapeRatio > 0.6f ? 0.1f : -0.1f)
        );

        Debug.Log($"Difficulty: {difficultyScore}");
    }
}