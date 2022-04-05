using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitlescreenController : MonoBehaviour
{
    [SerializeField] private string levelToLoad;

    private void Start()
    {
        FindObjectOfType<AudioManager>().Play("Titlescreen", 1);
    }

    public void StartGame()
    {
        FindObjectOfType<AudioManager>().Stop("Titlescreen");
        SceneManager.LoadScene(levelToLoad);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
