using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomEnums;

/// <summary>
/// Controls behavior and visuals for dive cage, used to enter and exit the level.
/// </summary>
public class CageController : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Objects & Components:
    public static CageController main; //Singleton instance of this script
    private Transform door;            //Cage door transform
    private Collider interiorVolume;   //Collider used to determine whether or not player is inside cage

    //Settings:
    [Header("Door Animation Settings:")]
    [SerializeField] [Tooltip("Max angle door can open to")]                                                       private float doorOpenAngle;
    [SerializeField] [Tooltip("How fast the cage door opens and closes")]                                          private float doorSpeed;
    [SerializeField] [Tooltip("How fast the door closes when player enters the cage while being chased by shark")] private float doorSlamSpeed;
    [Header("Sound Effects:")]
    [SerializeField] [Tooltip("Name of sound made when door opens")]        private string doorOpenSound;
    [SerializeField] [Tooltip("Name of sound made when door closes")]       private string doorCloseSound;
    [SerializeField] [Tooltip("Name of sound made when door slams closed")] private string doorSlamSound;
    [Space]

    //Runtime Vars:
    internal bool playerInside;  //Whether or not player is currently inside cage

    private bool doorOpen = false;      //Whether or not the door is currently open (or in opening animation)
    private bool doorStationary = true; //Indicates that, whatever state door is in, it has finished its animation

    //Delegates:
    public delegate void OnPlayerEntersCage(); public static OnPlayerEntersCage playerEntersCageDelegate;
    public delegate void OnPlayerLeavesCage(); public static OnPlayerLeavesCage playerLeavesCageDelegate;

    [Header("UI:")]
    [SerializeField] [Tooltip("The UI that tells the player when they can leave the game.")] private GameObject exitIndicatorText;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (main == null) { main = this; } else { Destroy(gameObject); } //Singleton-ify (destroy whole object if duplicated)

        //Get objects & components:
        door = transform.Find("Door");                                        //Get door transform
        interiorVolume = transform.Find("Interior").GetComponent<Collider>(); //Get interior collider

        //Check for errors:
        if (door == null) Debug.LogError("Dive cage is missing child named 'Door'");                                       //Make sure cage has a door
        if (interiorVolume == null) Debug.LogError("Dive cage is missing child named 'Interior' with collider component"); //Make sure cage has interior volume
    }
    private void FixedUpdate()
    {
        //Animate door:
        if (!doorStationary) //Door is currently animating and is unlocked
        {
            //Get new rotation:
            float targetAngle = doorOpenAngle;                                                        //Get initial value for target angle (assume door is opening)
            if (!doorOpen) targetAngle = 0;                                                           //Make target angle 0 when door is closing
            Quaternion targetRotation = Quaternion.Euler(-90, 0, targetAngle);                        //Package angle into rotation
            Quaternion newRotation = Quaternion.Slerp(door.localRotation, targetRotation, doorSpeed); //Interpolate new angle for door

            //Check if new rotation is close enough to target:
            if (Mathf.Abs(targetAngle - newRotation.z) < 0.01f) //Angle is close enough to just call it
            {
                newRotation = targetRotation; //Go directly to target rotation
                doorStationary = true;        //Indicate that door is now stationary
            }

            //Set door rotation:
            door.localRotation = newRotation; //Apply newfound rotation
        }
    }

    //PHYSICS METHODS:
    private void OnTriggerEnter(Collider other)
    {
        if (LevelSequencer.main.phase == LevelPhase.Hunt ||      //Game is in hunt phase
            LevelSequencer.main.phase == LevelPhase.GracePeriod) //Game is in grace period
        {
            if (other.CompareTag("Player")) ToggleDoor(true); //Open door when player enters proximity
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (LevelSequencer.main.phase == LevelPhase.Hunt ||      //Game is in hunt phase
            LevelSequencer.main.phase == LevelPhase.GracePeriod) //Game is in grace period
        {
            if (other.CompareTag("Player")) ToggleDoor(false); //Close door when player leaves proximity
        }
    }

    //CONTROL METHODS:
    /// <summary>
    /// Releases player from cage at start of game.
    /// </summary>
    public void ReleasePlayer()
    {
        //PlayerController.main.transform.parent = null; //Unchild player from cage
        ToggleDoor(true);                              //Open cage door
    }

    /// <summary>
    /// Traps player in cage at end of game.
    /// </summary>
    public void CapturePlayer()
    {
        PlayerController.main.transform.parent = transform; //Child player to cage transform
        doorSpeed = doorSlamSpeed;                          //Make door close really fast
        ToggleDoor(false);                                  //Close cage door

        //Play door slam SFX
        if (FindObjectOfType<AudioManager>() != null) //Scene has audio manager
        {
            FindObjectOfType<AudioManager>().Play(doorSlamSound, PlayerPrefs.GetFloat("SFXVolume"));
        }
    }
    /// <summary>
    /// Call when player enters the dive cage.
    /// </summary>
    public void PlayerEnteredCage()
    {
        //Update status:
        playerInside = true;        //Indicate that player is inside the cage
        playerEntersCageDelegate(); //Call player cage entry delegates
        Debug.Log("Player has entered dive cage");

        //If the player meets the conditions to leave, show the indicator
        if(playerInside == true && LevelSequencer.main.phase == LevelPhase.Hunt)
        {
            exitIndicatorText.SetActive(true);
        }
    }
    /// <summary>
    /// Call when player leaves the dive cage.
    /// </summary>
    public void PlayerLeftCage()
    {
        //Update status:
        playerInside = false;       //Indicate that player is not inside the cage
        playerLeavesCageDelegate(); //Call player cage exit delegates
        Debug.Log("Player has left dive cage");

        //If the exit indicator is active, deactivate it
        if (exitIndicatorText.activeInHierarchy)
        {
            exitIndicatorText.SetActive(false);
        }
    }

    public void HideIndicator()
    {
        exitIndicatorText.SetActive(false);
    }

    //FUNCTIONALITY METHODS:
    public void ToggleDoor(bool open)
    {
        //Begins door opening/closing animation.

        //If the door is opening, play the open SFX
        if (open)
        {
            //Play door open SFX
            if (FindObjectOfType<AudioManager>() != null) //Scene has audio manager
            {
                FindObjectOfType<AudioManager>().Play(doorOpenSound, PlayerPrefs.GetFloat("SFXVolume"));
            }
        }
        //If the door is closing and the phase is not at Exit, play the door close SFX
        else
        {
            //Play door close SFX
            if (FindObjectOfType<AudioManager>() != null && LevelSequencer.main.phase != LevelPhase.Exit) //Scene has audio manager
            {
                FindObjectOfType<AudioManager>().Play(doorCloseSound, PlayerPrefs.GetFloat("SFXVolume"));
            }
        }

        //Setup:
        doorOpen = open;        //Determine door animation
        doorStationary = false; //Indicate that door is now animating (if it wasn't already)
    }
}
