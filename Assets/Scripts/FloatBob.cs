using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component which allows object to float gently up and down in the water
/// </summary>
public class FloatBob : MonoBehaviour
{
    //Settings:
    [SerializeField] [Tooltip("Enables floating behavior as soon as object is loaded")]         private bool startOnAwake;
    [SerializeField] [Tooltip("Time taken between full bobs (how fast the object moves)")]      private float bobTime;
    [SerializeField] [Tooltip("Full range of motion in bob animation")]                         private float bobDistance;
    [SerializeField] [Tooltip("Curve describing relationship between bobTime and bobDistance")] private AnimationCurve bobCurve;

    //Runtime Memory Vars:
    private bool bobbing;       //Whether or not this object is currently bobbing
    private float timeSinceBob; //Time since completion of last full bob
    private float origY;        //Object's original Y position

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialization:
        bobbing = startOnAwake; //Determine whether or not object starts out doing bobbing motion
    }
    private void Start()
    {
        //Initialization:
        Initialize(bobbing); //Perform initialization (preserve selected onAwake behavior)
    }
    private void Update()
    {
        if (bobbing || timeSinceBob != 0) //Bob animation is either active or halting (and incomplete)
        {
            //Incrememnt bob time:
            timeSinceBob += Time.deltaTime; //Add delta time
            if (timeSinceBob >= bobTime) //Time rollover
            {
                if (bobbing) { timeSinceBob -= bobTime; } //Roll animation time over if still active
                else { timeSinceBob = 0; }                //End animation with final (starting) position if inactive
            }

            //Move object:
            Vector3 origPos = transform.position; origPos.y = origY; //Get positional vector representing original position for bob animation
            Vector3 targPos = origPos; targPos.y += bobDistance;     //Get positional vector representing furthest location of bob animation
            float t = bobCurve.Evaluate(timeSinceBob / bobTime);     //Get interpolant value from curve and current time value
            transform.position = Vector3.Lerp(origPos, targPos, t);  //Move object based on calculated values
        }
    }

    //CONTROL METHODS:
    /// <summary>
    /// Toggles bobbing animation on or off.
    /// </summary>
    /// <param name="enable">Pass true to enable bob, false to disable it.</param>
    public void Toggle(bool enable) { bobbing = enable; }
    /// <summary>
    /// Instantly ends bobbing animation.
    /// </summary>
    /// <param name="snapToOrigin">Pass true to force object back to its original Y position</param>
    public void EndImmediate(bool snapToOrigin)
    {
        bobbing = false;  //Indicate that object is no longer bobbing
        timeSinceBob = 0; //Prevent rest of bob animation from finishing naturally
        if (snapToOrigin) //Object is getting sent back to original Y position
        {
            Vector3 newPos = transform.position; newPos.y = origY; //Get vector representing original position
            transform.position = newPos;                           //Send object back to original position
        }
    }
    /// <summary>
    /// Initializes bob animation (call this if object has been moved).
    /// </summary>
    /// <param name="enable">Pass true to enable bob, false to disable it.</param>
    public void Initialize(bool enable)
    {
        origY = transform.position.y; //Record Y position
        timeSinceBob = 0;             //Reset bob time (needed for re-initialization)
        Toggle(enable);               //Enable/disable bob upon initialization
    }
}
