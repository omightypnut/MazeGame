using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private float roundLength = 60f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundsSurvivedText;

    private float timeRemaining;
    private int roundsSurvived = 0;

    public int RoundsSurvived => roundsSurvived;
    public float TimeRemaining => timeRemaining;

    private void Start()
    {
        ResetTimer();
        UpdateUI();
    }

    private void Update()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            roundsSurvived++;
            ResetTimer();
        }

        UpdateUI();
    }

    private void ResetTimer()
    {
        timeRemaining = roundLength;
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = "Time: " + seconds;
        }

        if (roundsSurvivedText != null)
        {
            roundsSurvivedText.text = "Rounds Survived: " + roundsSurvived;
        }
    }
}