using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    [SerializeField] [Tooltip("Level which will be loaded if player chooses to continue")] private string reloadLevel;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            GameManager.instance.playingSongName = "VictorySFX";
            audioManager.Play(GameManager.instance.playingSongName, PlayerPrefs.GetFloat("SFXVolume"));
        }
    }

    //INPUT METH:
    public void OnRestart()
    {
        if (audioManager != null)
        {
            if (audioManager.IsPlaying(GameManager.instance.playingSongName))
            {
                audioManager.Stop(GameManager.instance.playingSongName);
            }
        }
        SceneManager.LoadScene(reloadLevel);
    }
    public void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
