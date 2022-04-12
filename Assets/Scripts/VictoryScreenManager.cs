using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    [SerializeField] [Tooltip("Level which will be loaded if player chooses to continue")] private string reloadLevel;

    //INPUT METH:
    public void OnRestart() { SceneManager.LoadScene(reloadLevel); }
    public void OnQuit() { Application.Quit(); }
}
