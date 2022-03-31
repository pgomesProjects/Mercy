using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExitGame : MonoBehaviour
{
    // Start is called before the first frame update
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();
        //gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        button.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void QuitGame()
    {

        Application.Quit();
        Debug.Log("Quit!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
