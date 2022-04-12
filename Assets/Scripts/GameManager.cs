using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameSettings currentSettings = new GameSettings();

    [HideInInspector] public int finalScore;
    [HideInInspector]
    public string playingSongName = "";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
        Cursor.lockState = CursorLockMode.Confined; //Confine cursor
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Quitting Game...");
        PlayerPrefs.SetInt("OnApplicationOpen", 0);
    }

}
