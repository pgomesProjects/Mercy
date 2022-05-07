using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    PlayerActionsMap playerActions;

    private bool isPaused;

    [SerializeField] private GameObject pauseMenu;

    private void Awake()
    {
        playerActions = new PlayerActionsMap();
        playerActions.Player.Pause.performed += _ => TogglePause();
    }

    // Start is called before the first frame update
    void Start()
    {
        isPaused = false;
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);

        if (isPaused)
        {
            Cursor.visible = true;
            Time.timeScale = 0;
            if(FindObjectOfType<AudioManager>() != null)
            {
/*                if(FindObjectOfType<AudioManager>().IsPlaying("CageChain"))
                    FindObjectOfType<AudioManager>().Pause("CageChain");
                FindObjectOfType<AudioManager>().Pause(GameData.playingSongName);*/

                if(FindObjectOfType<AudioManager>() != null)
                {
                    FindObjectOfType<AudioManager>().PauseAllSounds();
                }
                LevelManager.main.StopThreatMusic((int)SharkController.main.currentThreatLevel);
            }
        }
        else
        {
            Cursor.visible = false;
            Time.timeScale = 1;
            if (FindObjectOfType<AudioManager>() != null)
            {
                /*                FindObjectOfType<AudioManager>().Resume("CageChain");
                                FindObjectOfType<AudioManager>().Resume(GameData.playingSongName);*/

                if (FindObjectOfType<AudioManager>() != null)
                {
                    FindObjectOfType<AudioManager>().ResumeAllSounds();
                }
                LevelManager.main.StartThreatMusic((int)SharkController.main.currentThreatLevel);
            }
        }
    }

    public void ReturnToMain()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
