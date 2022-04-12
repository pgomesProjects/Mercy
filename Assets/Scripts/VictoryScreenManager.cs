using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    [SerializeField] [Tooltip("Level which will be loaded if player chooses to continue")] private string reloadLevel;

    private void Start()
    {
        if (FindObjectOfType<AudioManager>() != null)
        {
            GameManager.instance.playingSongName = "VictorySFX";
            FindObjectOfType<AudioManager>().Play(GameManager.instance.playingSongName, PlayerPrefs.GetFloat("SFXVolume"));
        }
    }

    //INPUT METH:
    public void OnRestart() { SceneManager.LoadScene(reloadLevel); }
    public void OnQuit() { Application.Quit(); }
}
