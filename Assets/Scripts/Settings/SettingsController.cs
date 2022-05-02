using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider[] volumeSliders;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle[] toggleButtons;
    [SerializeField] private TMP_Dropdown [] settingsDropdowns;
    [HideInInspector]
    public int[,] possibleResolutions = new int[,] { { 2560, 1440 }, { 1920, 1080 }, { 1280, 720 } };

    public void ChangeBGMVolume(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value * 0.1f);
        Debug.Log("BGM Volume: " + PlayerPrefs.GetFloat("BGMVolume"));
        FindObjectOfType<AudioManager>().ChangeVolume("Titlescreen", PlayerPrefs.GetFloat("BGMVolume"));
    }

    public void ChangeSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value * 0.1f);
        Debug.Log("SFX Volume: " + PlayerPrefs.GetFloat("SFXVolume"));
    }

    public void ChangeSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value * 0.01f);
        Debug.Log("Mouse Sensitivity: " + PlayerPrefs.GetFloat("MouseSensitivity"));
    }

    public void FullScreenToggle(bool isFullScreen)
    {
        //Toggle fullscreen variable
        GameData.currentSettings.SetIsFullScreen(isFullScreen);

        if (GameData.currentSettings.GetIsFullScreen())
        {
            Debug.Log("Game Is Fullscreen");
            PlayerPrefs.SetInt("IsWindowed", 0);
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            Debug.Log("Game Is Windowed");
            PlayerPrefs.SetInt("IsWindowed", 1);
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }//end of FullScreenToggle

    public void VSyncEnabledToggle(bool isVSyncEnabled)
    {
        //Toggle vsync variable
        GameData.currentSettings.SetVSyncEnabled(isVSyncEnabled);

        if (GameData.currentSettings.GetVSyncEnabled())
        {
            Debug.Log("VSync Enabled");
            PlayerPrefs.SetInt("VSyncEnabled", 1);
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            Debug.Log("VSync Disabled");
            PlayerPrefs.SetInt("VSyncEnabled", 0);
            QualitySettings.vSyncCount = 0;
        }
    }//end of VSyncEnabledToggle

    public void ResolutionDropdownChange(int resIndex)
    {
        GameData.currentSettings.SetCurrentResolution((GameSettings.Resolution)resIndex);
        Screen.SetResolution(possibleResolutions[(int)GameData.currentSettings.GetCurrentResolution(), 0],
            possibleResolutions[(int)GameData.currentSettings.GetCurrentResolution(), 1], Screen.fullScreenMode);

        Debug.Log(GameData.currentSettings.GetCurrentResolution());
    }//end of ResolutionDropdownChange

    public void QualityDropdownChange(int qualityIndex)
    {
        GameData.currentSettings.SetGraphicsQuality((GameSettings.GraphicsQuality)qualityIndex);
        QualitySettings.SetQualityLevel((int)GameData.currentSettings.GetGraphicsQuality());
        Debug.Log(GameData.currentSettings.GetGraphicsQuality());
    }//end of QualityDropdownChange

    public Slider[] GetVolumeSliders() { return volumeSliders; }
    public Slider GetSensitivitySlider() { return sensitivitySlider; }
    public Toggle GetFullScreenToggle() { return toggleButtons[0]; }
    public Toggle GetVSyncEnabledToggle() { return toggleButtons[1]; }
    public TMP_Dropdown [] GetSettingsDropdowns() { return settingsDropdowns; }
}
