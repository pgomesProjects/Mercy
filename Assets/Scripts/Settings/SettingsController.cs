using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider[] volumeSliders;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle fullScreenToggle;
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
        GameManager.instance.currentSettings.SetIsFullScreen(isFullScreen);

        if (GameManager.instance.currentSettings.GetIsFullScreen())
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

    public void ResolutionDropdownChange(int resIndex)
    {
        GameManager.instance.currentSettings.SetCurrentResolution((GameSettings.Resolution)resIndex);
        Screen.SetResolution(possibleResolutions[(int)GameManager.instance.currentSettings.GetCurrentResolution(), 0],
            possibleResolutions[(int)GameManager.instance.currentSettings.GetCurrentResolution(), 1], Screen.fullScreenMode);

        Debug.Log(GameManager.instance.currentSettings.GetCurrentResolution());
    }//end of ResolutionDropdownChange

    public void QualityDropdownChange(int qualityIndex)
    {
        GameManager.instance.currentSettings.SetGraphicsQuality((GameSettings.GraphicsQuality)qualityIndex);
        QualitySettings.SetQualityLevel((int)GameManager.instance.currentSettings.GetGraphicsQuality());
        Debug.Log(GameManager.instance.currentSettings.GetGraphicsQuality());
    }//end of QualityDropdownChange

    public Slider[] GetVolumeSliders() { return volumeSliders; }
    public Slider GetSensitivitySlider() { return sensitivitySlider; }
    public Toggle GetFullScreenToggle() { return fullScreenToggle; }
    public TMP_Dropdown [] GetSettingsDropdowns() { return settingsDropdowns; }
}
