using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls behavior and visuals for dive cage, used to enter and exit the level.
/// </summary>
public class CageController : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Objects & Components:
    public static CageController main; //Singleton instance of this script
    private Transform door;            //Cage door transform

    //Settings:
    [Header("General Settings:")]
    [SerializeField] [Tooltip("Name of scene that plays when player escapes")] private string exitScene;
    [Header("Door Animation Settings:")]
    [SerializeField] [Tooltip("How fast the cage door opens and closes")] private float doorSpeed;
    [SerializeField] [Tooltip("Max angle door can open to")]              private float doorOpenAngle;

    //Runtime Vars:
    private bool doorOpen = false;      //Whether or not the door is currently open (or in opening animation)
    private bool doorStationary = true; //Indicates that, whatever state door is in, it has finished its animation

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (main == null) { main = this; } else { Destroy(gameObject); } //Singleton-ify (destroy whole object if duplicated)

        //Get objects & components:
        door = transform.Find("Door"); if (door == null) Debug.LogError("Dive cage is missing child named 'Door'"); //Make sure cage has a door
    }
    private void FixedUpdate()
    {
        //Animate door:
        if (!doorStationary) //Door is currently animating
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
        if (other.CompareTag("Player")) ToggleDoor(true); //Open door when player is near
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) ToggleDoor(false); //Close door when player leaves proximity
    }

    //CONTROL METHODS:
    /// <summary>
    /// Begins door opening/closing animation.
    /// </summary>
    /// <param name="open">Pass true to open door, false to close door.</param>
    public void ToggleDoor(bool open)
    {
        //Setup:
        doorOpen = open;        //Determine door animation
        doorStationary = false; //Indicate that door is now animating (if it wasn't already)
    }
}
