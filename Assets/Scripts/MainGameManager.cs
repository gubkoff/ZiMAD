using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainGameManager : MonoBehaviour {
    public static MainGameManager instance;
    [SerializeField] private Text stepText;
    [SerializeField] private Text scoresText;
    private static int stepCount;
    private static int scores;

    void Awake() {
        Screen.orientation = ScreenOrientation.Portrait;
        stepCount = 0;
        scores = 0;
        ShowStep();
        if (instance == null) {
            instance = this;
        } else {
            Destroy(instance);
        }
    }

    public void AddStep() {
        stepCount++;
        ShowStep();
    }

    private void ShowStep() {
        if (stepText != null) {
            stepText.text = stepCount.ToString();
        }
    }
    
    public void AddScores(int _scores) {
        scores += _scores;
        ShowScores();
    }

    
    private void ShowScores() {
        if (scoresText != null) {
            scoresText.text = scores.ToString();
        }
    }

    public void ReloadScene() {
        SceneManager.LoadScene("GameScene");
    }
}
