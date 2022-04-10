using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public string restartLevel, titlescreenLevel;

    public void RestartGame()
    {
        SceneManager.LoadScene(restartLevel);
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene(titlescreenLevel);
    }

}
