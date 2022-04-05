using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;

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
            FindObjectOfType<AudioManager>().Play("InGame", 0.2f);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        isGameOver = true;
        if (audioManager != null)
        {
            FindObjectOfType<AudioManager>().Stop("InGame");
            FindObjectOfType<AudioManager>().Play("GameOverSFX", 1);
        }
        SceneManager.LoadScene("GameOver");
    }
}
