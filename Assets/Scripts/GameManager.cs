using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI finalScore; 
    public GameObject endGameMenu;
    public List<GameObject> targets;
    public TextMeshProUGUI scoreText;

    public bool isGameActive;
    private int score;
    private float spawnRate = 1.0f;
    //public TextMeshProUGUI gameOverText;
    //public Button restartButton;
    //public GameObject titleScreen;
    public int divider;
    public bool gameOver;

    // Start is called before the first frame update
    void Start()
    {
        isGameActive = true;
        score = 0;
        //scoreText.text = "Score: " + score;
        //StartCoroutine(SpawnTarget());
        UpdateScore(0);
    }

    // Update is called once per frame
    void Update()
    {
        /*score++;
        scoreText.text = "Score: " + (score / divider);*/
    }
    /*IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            int index = Random.Range(0, targets.Count);
            Instantiate(targets[index]);
        }
    }*/
    public void UpdateScore(int scoreToAdd)
    {
        if (!gameOver)
        {
            score += scoreToAdd;
            scoreText.text = "Score: " + score;
        }
    }
    public void GameOver()
    {
        endGameMenu.SetActive(true);
        finalScore.text = scoreText.text;
        //restartButton.gameObject.SetActive(true);
        //gameOverText.gameObject.SetActive(true);
        gameOver = true;
        isGameActive = false;
    }
    /*public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void StartGame()
    {
        isGameActive = true;
        score = 0;

        StartCoroutine(SpawnTarget());
        UpdateScore(0);

        titleScreen.gameObject.SetActive(false);
    }*/
    
}
