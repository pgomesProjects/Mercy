using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private string levelToLoad;
    [SerializeField] private string PageToLoad;
    [SerializeField] private GameObject[] menuStates;
    private enum MenuState { SKIPSTORY, NEXTPAGE };
    //private MenuState currentMenuState;
    public void StartGame()
    {
        SceneManager.LoadScene(levelToLoad);
    }

    public void NextPage()
    {
        SceneManager.LoadScene(PageToLoad);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
