using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIButtons : MonoBehaviour
{
    private Button button;
    //private GameManager gameManager;

    /*public int difficulty;*/

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        //gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        button.onClick.AddListener(StartGame);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void StartGame()
    {
        SceneManager.LoadScene("HotteScene");
        //gameManager.StartGame(difficulty);
        
    }

    /*void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit!");
    }*/
}
