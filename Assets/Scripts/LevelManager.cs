using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;

    [SerializeField] private Image threatUI;
    [SerializeField] private Color[] threatColors;
    [SerializeField] private TextMeshProUGUI personalBestText;
    [SerializeField] private Slider oxygenBar;

    public TextMeshProUGUI scoreText;
    public bool isGameOver;

    private AudioManager audioManager;

    private void Awake()
    {
        main = this;
    }

    void Start()
    {
        isGameOver = false;
        audioManager = FindObjectOfType<AudioManager>();
        if(audioManager != null)
        {
            GameManager.instance.playingSongName = "InGame";
            FindObjectOfType<AudioManager>().Play(GameManager.instance.playingSongName, PlayerPrefs.GetFloat("BGMVolume"));
        }

        personalBestText.text = "Personal Best: " + PlayerPrefs.GetInt("HighScore");
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void UpdateOxygenBar(float oxygenPercentage)
    {
        oxygenBar.value = oxygenPercentage;
    }

    public void UpdateThreatUI(int level)
    {
        Debug.Log("Threat UI: " + level);
        threatUI.color = threatColors[level];
    }

    public void StartThreatMusic(int level)
    {
        if (audioManager != null)
        {
            Debug.Log("Starting Heartbeat" + level);
            for(int i = 0; i < (int)SharkController.ThreatLevel.NumberOfThreatLevels; i++)
            {
                if(i != level)
                {
                    //If there's another heartbeat SFX playing, stop it
                    FindObjectOfType<AudioManager>().Stop("Heartbeat" + i);
                }
                //Play the wanted heartbeat SFX
                else
                {
                    FindObjectOfType<AudioManager>().Play("Heartbeat" + level, PlayerPrefs.GetFloat("SFXVolume"));
                }
            }
        }
    }

    public void StopThreatMusic(int level)
    {
        if (audioManager != null)
        {
            FindObjectOfType<AudioManager>().Stop("Heartbeat" + level);
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        if (audioManager != null)
        {
            FindObjectOfType<AudioManager>().Stop(GameManager.instance.playingSongName);
            FindObjectOfType<AudioManager>().Play("GameOverSFX", PlayerPrefs.GetFloat("SFXVolume"));
        }
        SceneManager.LoadScene("GameOver");
    }
}
