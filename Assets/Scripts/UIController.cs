using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI totalScoreText;

    [SerializeField]
    private TextMeshProUGUI roundTimer;

    [SerializeField]
    private TextMeshProUGUI packagesLeft;

    [SerializeField]
    private TextMeshProUGUI gameOverPackagesToDeliver;

    [SerializeField]
    private TextMeshProUGUI gameOverPackagesDelivered;

    [SerializeField]
    private TextMeshProUGUI gameOverScore;

    [SerializeField]
    private TextMeshProUGUI subtractPackagesScore;

    [SerializeField]
    private TextMeshProUGUI gameOverFinalScore;

    [SerializeField]
    private TextMeshProUGUI highScoreText;

    [SerializeField]
    private TextMeshProUGUI newHighScoreText;

    [SerializeField]
    private TextMeshProUGUI fullDeliveryBonus;

    [SerializeField]
    private TextMeshProUGUI onTimeBonus;

    // Start is called before the first frame update
    void Start()
    {
        Game.Instance.OnUpdateTotalScore += updateScore;

        Game.Instance.OnRoundTimerTick += updateRoundTimer;

        Game.Instance.OnPackageDelivered += updatePackagesLeft;

        Game.Instance.OnGameOver += updateGameOverScreen;

        Game.Instance.OnNewHighScore += updateHighScore;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void updateScore(int gameScore)
    {
        totalScoreText.SetText(gameScore.ToString());
    }

    private void updateRoundTimer(float current)
    {
        roundTimer.SetText(current.ToString());
    }

    private void updatePackagesLeft(Package package, int leftCount)
    {
        packagesLeft.SetText(leftCount.ToString());
    }

    private void updateGameOverScreen(bool allDeliveredBonus, bool deliveredOnTimeBonus, int packagesToDeliver, int subtractPackages, int packagesLeft, int packagesDelivered, int score, int finalScore)
    {
        gameOverPackagesToDeliver.SetText(packagesLeft.ToString());
        gameOverPackagesDelivered.SetText(packagesDelivered.ToString());
        gameOverScore.SetText(score.ToString());

        subtractPackagesScore.SetText("-" + subtractPackages.ToString());
        
        fullDeliveryBonus.SetText(allDeliveredBonus ? "x2" : "-");
        onTimeBonus.SetText(deliveredOnTimeBonus ? "x2" : "-");

        gameOverFinalScore.SetText(finalScore.ToString());


    }

    private void updateHighScore(int highScore, bool newHigh)
    {
        if(newHigh)
        {
            highScoreText.SetText(highScore.ToString());
            newHighScoreText.gameObject.SetActive(true);
        } else
        {
            newHighScoreText.gameObject.SetActive(false);
        }
    }
}
