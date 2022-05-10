using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public string restartLevel, titlescreenLevel;
    [SerializeField] private Image gameOverImage;
    [SerializeField] private Sprite[] gameOverScreens;

    private void Start()
    {
        //Set the game over image to the most recent cause of death
        gameOverImage.sprite = gameOverScreens[(int)GameData.currentCauseOfDeath];
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(restartLevel);
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene(titlescreenLevel);
    }

}
