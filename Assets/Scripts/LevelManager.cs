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
