using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtons : MonoBehaviour
{
    public void OnHoverSFX()
    {
        if (FindObjectOfType<AudioManager>() != null) //Scene has audio manager
        {
            FindObjectOfType<AudioManager>().Play("UIHover", PlayerPrefs.GetFloat("SFXVolume"));
        }
    }

    public void OnClickSFX()
    {
        if (FindObjectOfType<AudioManager>() != null) //Scene has audio manager
        {
            FindObjectOfType<AudioManager>().Play("UIClick", PlayerPrefs.GetFloat("SFXVolume"));
        }
    }

    public void StoryOnClickSFX()
    {
        if (FindObjectOfType<AudioManager>() != null) //Scene has audio manager
        {
            FindObjectOfType<AudioManager>().Play("StoryUIClick", PlayerPrefs.GetFloat("SFXVolume"));
        }
    }
}
