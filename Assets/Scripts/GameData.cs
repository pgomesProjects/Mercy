using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    internal static int finalScore;
    internal static string playingSongName = "";
    internal static GameSettings currentSettings = new GameSettings();

    public enum CAUSEOFDEATH { SHARKATTACK, SUFFOCATION};
    internal static CAUSEOFDEATH currentCauseOfDeath;

#if UNITY_STANDALONE
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
    static void Init()
    {
        Debug.Log("Initialize Called!");
        Cursor.lockState = CursorLockMode.Confined; //Confine cursor
        Application.quitting += ApplicationQuit; //Call function whenever game quits
    }

    static void ApplicationQuit()
    {
        Debug.Log("Quitting Game...");
        PlayerPrefs.SetInt("OnApplicationOpen", 0);
    }

}
