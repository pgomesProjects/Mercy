using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitlescreenController : MonoBehaviour
{
    [SerializeField] private string levelToLoad;
    [SerializeField] private GameObject[] menuStates;
    [SerializeField] private TextMeshProUGUI highScoreText;
    private enum MenuState { TITLESCREEN, PREFERENCES };
    private MenuState currentMenuState;

    private void Start()
    {
        currentMenuState = MenuState.TITLESCREEN;
        highScoreText.text = "Best Score: " + PlayerPrefs.GetInt("HighScore");
    }

    public void StartGame()
    {
        FindObjectOfType<AudioManager>().Stop(GameManager.instance.playingSongName);
        SceneManager.LoadScene(levelToLoad);
    }

    public void TogglePreferences()
    {
        switch (currentMenuState)
        {
            case MenuState.TITLESCREEN:
                menuStates[0].SetActive(false);
                menuStates[1].SetActive(true);
                currentMenuState = MenuState.PREFERENCES;
                break;
            case MenuState.PREFERENCES:
                menuStates[0].SetActive(true);
                menuStates[1].SetActive(false);
                currentMenuState = MenuState.TITLESCREEN;
                break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
