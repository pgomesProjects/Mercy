using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreenManager : MonoBehaviour
{
    [SerializeField] [Tooltip("Level which will be loaded if player chooses to continue")] private string reloadLevel;

    private AudioManager audioManager;

    [SerializeField] private TextMeshProUGUI finalScoreText;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            GameData.playingSongName = "VictorySFX";
            audioManager.Play(GameData.playingSongName, PlayerPrefs.GetFloat("SFXVolume"));
        }
        ShowScore();
    }
    private void ShowScore()
    {
        int finalScore = 0;

        finalScore = GameData.finalScore;

        finalScoreText.text = "Score: <br><size='60'>" + finalScore + "</size>";

        if (PlayerPrefs.GetInt("HighScore") < finalScore)
        {
            PlayerPrefs.SetInt("HighScore", finalScore);
            finalScoreText.text += "<br>New High Score!";
        }
    }

    //INPUT METH:
    public void OnRestart()
    {
        if (audioManager != null)
        {
            if (audioManager.IsPlaying(GameData.playingSongName))
            {
                audioManager.Stop(GameData.playingSongName);
            }
        }
        SceneManager.LoadScene(reloadLevel);
    }
    public void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
