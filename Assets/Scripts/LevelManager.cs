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
            StartThreatMusic(0);
        }
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void UpdateThreatUI(int level)
    {
        threatUI.color = threatColors[level];
    }

    public void StartThreatMusic(int level)
    {
        if (audioManager != null)
        {
            Debug.Log("Starting Heartbeat" + (level + 1));
            FindObjectOfType<AudioManager>().Play("Heartbeat" + (level + 1), PlayerPrefs.GetFloat("SFXVolume"));
        }
    }

    public void StopThreatMusic(int level)
    {
        if (audioManager != null)
        {
            FindObjectOfType<AudioManager>().Stop("Heartbeat" + (level + 1));
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
