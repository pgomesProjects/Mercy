using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SettingsOnStart : MonoBehaviour
{
    [SerializeField]
    internal SettingsController settingsController;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("OnApplicationOpen") == 0)
            BeginSetup();
        else
            CallSettingsValues();

        //Play titlescreen music
        GameData.playingSongName = "Titlescreen";
        FindObjectOfType<AudioManager>().Play("Titlescreen", PlayerPrefs.GetFloat("BGMVolume"));
    }

    private void BeginSetup()
    {
        //Default settings on first run of game
        if (PlayerPrefs.GetInt("FirstRun") == 0)
        {
            PlayerPrefs.SetFloat("BGMVolume", 0.5f);
            PlayerPrefs.SetFloat("SFXVolume", 0.5f);
            PlayerPrefs.SetFloat("MouseSensitivity", 0.15f);
            PlayerPrefs.SetInt("FirstRun", 1);
        }

        GameData.playingSongName = "Titlescreen";

        SetUpFullscreen();
        SetUpVSync();
        SetUpVolume();
        SetUpSensitivity();
        SetUpResolution();
        SetUpQuality();

        PlayerPrefs.SetInt("OnApplicationOpen", 1);
    }//end of BeginSetup

    private void CallSettingsValues()
    {
        settingsController.GetVolumeSliders()[0].value = PlayerPrefs.GetFloat("BGMVolume") * 10;
        settingsController.GetVolumeSliders()[1].value = PlayerPrefs.GetFloat("SFXVolume") * 10;
        settingsController.GetSensitivitySlider().value = PlayerPrefs.GetFloat("MouseSensitivity") * 100;
        settingsController.GetFullScreenToggle().isOn = GameData.currentSettings.GetIsFullScreen();
        settingsController.GetVSyncEnabledToggle().isOn = GameData.currentSettings.GetVSyncEnabled();
        settingsController.GetSettingsDropdowns()[0].value = (int)GameData.currentSettings.GetCurrentResolution();
        settingsController.GetSettingsDropdowns()[0].RefreshShownValue();
        settingsController.GetSettingsDropdowns()[1].value = (int)GameData.currentSettings.GetGraphicsQuality();
        settingsController.GetSettingsDropdowns()[1].RefreshShownValue();
    }//end of CallSettingsValues

    private void SetUpFullscreen()
    {

        switch (PlayerPrefs.GetInt("IsWindowed"))
        {
            //Fullscreen
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            //Windowed
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
        GameData.currentSettings.SetIsFullScreen(Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen);
        if (GameData.currentSettings.GetIsFullScreen())
            settingsController.GetFullScreenToggle().isOn = true;
        else
            settingsController.GetFullScreenToggle().isOn = false;
    }//end of SetUpFullscreen

    private void SetUpVSync()
    {
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSyncEnabled");
        GameData.currentSettings.SetVSyncEnabled(PlayerPrefs.GetInt("VSyncEnabled") == 1);
        if (GameData.currentSettings.GetVSyncEnabled())
            settingsController.GetVSyncEnabledToggle().isOn = true;
        else
            settingsController.GetVSyncEnabledToggle().isOn = false;
    }//end of SetUpVSync

    private void SetUpVolume()
    {
        settingsController.GetVolumeSliders()[0].value = PlayerPrefs.GetFloat("BGMVolume") * 10;
        settingsController.GetVolumeSliders()[1].value = PlayerPrefs.GetFloat("SFXVolume") * 10;
        FindObjectOfType<AudioManager>().ChangeVolume(GameData.playingSongName, PlayerPrefs.GetFloat("BGMVolume"));
    }//end of SetUpVolume

    public void SetUpSensitivity()
    {
        settingsController.GetSensitivitySlider().value = PlayerPrefs.GetFloat("MouseSensitivity") * 100;
    }

    private void SetUpResolution()
    {
        int currentResolutionIndex = -1;

        if (GameData.currentSettings.GetIsFullScreen())
        {
            for (int i = 0; i < settingsController.possibleResolutions.GetLength(0); i++)
            {
                if (Screen.currentResolution.width == settingsController.possibleResolutions[i, 0]
                    && Screen.currentResolution.height == settingsController.possibleResolutions[i, 1])
                    currentResolutionIndex = i;
            }
        }
        else
        {
            for (int i = 0; i < settingsController.possibleResolutions.GetLength(0); i++)
            {
                if (Screen.width == settingsController.possibleResolutions[i, 0]
                    && Screen.height == settingsController.possibleResolutions[i, 1])
                    currentResolutionIndex = i;
            }
        }

        //Set to 1080p if none apply
        if (currentResolutionIndex == -1)
            currentResolutionIndex = 1;

        settingsController.GetSettingsDropdowns()[0].value = currentResolutionIndex;
        GameData.currentSettings.SetCurrentResolution((GameSettings.Resolution)currentResolutionIndex);
        settingsController.GetSettingsDropdowns()[0].RefreshShownValue();
    }//end of SetUpResolution

    private void SetUpQuality()
    {
        int currentQuality = QualitySettings.GetQualityLevel();
        settingsController.GetSettingsDropdowns()[1].value = currentQuality;
        GameData.currentSettings.SetGraphicsQuality((GameSettings.GraphicsQuality)currentQuality);
        settingsController.GetSettingsDropdowns()[1].RefreshShownValue();
    }//end of SetUpQuality

}
