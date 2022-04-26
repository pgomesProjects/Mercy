using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StoryController : MonoBehaviour
{
    [SerializeField] private string levelToLoad;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private string[] storyDialogLines;
    private int currentLine;

    [SerializeField] private Button[] storyButtons;

    private void Start()
    {
        //Start the story with the first line
        currentLine = 0;
        storyText.text = storyDialogLines[currentLine];
    }

    public void StartGame()
    {
        //Stop playing the main menu background music
        if(GameManager.instance != null && FindObjectOfType<AudioManager>() != null)
            FindObjectOfType<AudioManager>().Stop(GameManager.instance.playingSongName);

        //Load the necessary game scene
        SceneManager.LoadScene(levelToLoad);
    }

    public void NextPage()
    {
        //Advance to the next line in the story
        currentLine++;
        storyText.text = storyDialogLines[currentLine];

        //Check to see if the current line is the last line in the story
        CheckForLastLine();
    }

    private void CheckForLastLine()
    {
        //If the next line is the end of the dialog
        if(currentLine + 1 >= storyDialogLines.Length)
        {
            //Hide the next page button
            storyButtons[1].gameObject.SetActive(false);

            //Change the text on the skip story button and move to center
            storyButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Start Game";
            storyButtons[0].GetComponent<RectTransform>().anchoredPosition = 
                new Vector2(0, storyButtons[0].GetComponent<RectTransform>().anchoredPosition.y);
        }
    }
}
