using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using CustomEnums;
using UnityEngine.UI;

/// <summary>
/// Processes player input and movement functionality.
/// </summary>
public class PlayerController : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Objects & Components:
    public static PlayerController main;   //Singleton instance of playercontroller
    private PlayerInput playerInput;       //Script handling player input events
    internal Rigidbody rb;                 //Player rigidbody component
    public Camera playerCam;               //Player camera component
    public Slider pickupProgressIndicator; //Player UI element which displays pickup progress

    public static List<PickupController> pickupsInRange = new List<PickupController>(); //Pickups currently in range of player

    //Settings:
    [Header("Swimming:")]
    [SerializeField] [Tooltip("Player's base swim speed (in units per second)")]                                                       private float baseSwimSpeed;
    [SerializeField] [Tooltip("Speed at which player can swim up (in world space")]                                                    private float ascendSpeed;
    [SerializeField] [Tooltip("Speed at which player can swim down (in world space)")]                                                 private float descendSpeed;
    [SerializeField] [Tooltip("How quickly player accelerates to target speed while swimming")] [Range(0, 1)]                          private float swimAccel;
    [SerializeField] [Tooltip("How much force is applied when player dashes")]                                                         private float dashForce;
    [SerializeField] [Tooltip("How long dash acceleration lingers after initial impulse")]                                             private float dashPeriod;
    [SerializeField] [Tooltip("Time player must wait after dashing before they may dash again")]                                       private float dashCooldown;
    [SerializeField] [Tooltip("Swim speed multiplier (value) depending on horizontal swim direction (t0 is forward, t1 is backward)")] private AnimationCurve swimDirModCurve;
    [SerializeField] [Tooltip("Curve describing player velocity modification during dash period")]                                     private AnimationCurve dashCurve;

    [Header("Mouselook:")]
    [SerializeField] [Tooltip("How dramatically camera responds to mouse/joystick movements (along each axis")] private Vector2 lookSensitivity;
    [SerializeField] [Tooltip("Smooths out camera motion, but at the cost of responsiveness")] [Range(1, 0)]    private float lookDamping;
    [SerializeField] [MinMaxSlider(-90, 90)] [Tooltip("How far up or down player can look (in degrees)")]       private Vector2 verticalLookLimits;

    [Header("Other:")]
    [SerializeField] [Tooltip("How long player takes to pick up an item")] private float pickupTime;
    [SerializeField] [Tooltip("How long player takes to lose one percent of oxygen per second normally")] private float defaultOxygenDepletionRate;
    [SerializeField] [Tooltip("How long player takes to lose one percent of oxygen per second when dashing")] private float dashOxygenDepletionRate;
    [SerializeField] [Tooltip("How long player takes to die after losing oxygen")] private float secondsUntilOxygenDeath;

    //Runtime Memory Vars:
    private Vector2 mouseRotation; //Cumulative angular rotation generated from mouse movements
    private Quaternion lookOrigin; //Original look direction used for reference
    private Quaternion lookTarget; //Rotation which player is currently trying to get to

    private Vector3 moveTarget;       //Player's current target velocity
    private Vector3 dashMoveTarget;   //Player target velocity for most recent dash
    private float timeSinceDash = -1; //Time since player last dashed (negative if player is ready to dash again)

    //[SerializeField] private int playerHealth = 100; //The player's health attribute
    public int playerScore; //Score that depends on the items the player picks up
    private bool pickingUpItem = false;  //Whether or not player is currently picking up an item
    private float timeHoldingPickup = 0; //Time player has spent holding pickup button when in range of pickup
    internal float oxygenPercentage = 100; //Player oxygen level
    internal bool oxygenIsDepleting;
    private float currentDepletionRate;
    internal bool playerDyingFromOxygen;
    private IEnumerator playerOxygenBlackoutCoroutine;

    public Vector3 interestedAreaBox = new Vector3(200, 200, 200); //The area in which the shark goes to when interested
    public float teleportAreaMultiplier = 1.25f; //A multiplier that creates a new box for the shark to spawn a little bit farther from the player
    internal Vector3 originalInterestedAreaBox;
    public float playerViewingDistance = 100;


    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialization:
        if (main == null) { main = this; } else { Destroy(this); } //Singleton-ize playerController script

        //Get objects & components:
        if (!TryGetComponent(out playerInput)) Debug.LogError("Player object is missing PlayerInput script");                  //Make sure player has input script
        if (!TryGetComponent(out rb)) Debug.LogError("Player object does not have an attached RigidBody");                     //Make sure player has rigidbody
        if (pickupProgressIndicator == null) Debug.LogError("Make sure to assign PickupProgress UI element to player object"); //Make sure player has pickup progress indicator

    }
    private void Start()
    {
        //Initialization:
        lookOrigin = transform.localRotation; //Get initial player rotation
        lookTarget = transform.localRotation; //Use this for initial rotation target as well

        //Save the Vector3 for the area box, since the shark's aggression will change this
        originalInterestedAreaBox = interestedAreaBox;

        //Set look sensitivity from settings
        lookSensitivity.x = PlayerPrefs.GetFloat("MouseSensitivity");
        lookSensitivity.y = PlayerPrefs.GetFloat("MouseSensitivity");
    }
    private void Update()
    {
        //Item pickup sequence:
        if (pickingUpItem) //Player is currently picking up an item
        {
            //Update pickup progress:
            timeHoldingPickup += Time.deltaTime; //Increment pickup time
            pickupProgressIndicator.value = Mathf.Min(1, timeHoldingPickup / pickupTime); //Display pickup progress on HUD

            //Check for pickup procedure:
            if (timeHoldingPickup >= pickupTime) //Player has held button long enough to pick up item
            {
                if (pickupsInRange.Count > 0) pickupsInRange[0].CollectItem(); //Collect oldest pickup in list
                if (pickupsInRange.Count > 0) StartPickup();                   //Restart pickup counter if there are more objects to pick up
                else CancelPickup();                                           //Stop pickup counter if everything in range of player is picked up
            }
        }

        //Update player oxygen levels if the oxygen is depleting
        if(oxygenIsDepleting)
            OxygenTimer();
    }
    private void FixedUpdate()
    {
        //Mouselook:
        if (transform.localRotation != lookTarget) //Player is not currently aligned with lookTarget
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, lookTarget, lookDamping); //Rotate player object toward target
            if (Vector3.Distance(transform.localRotation.eulerAngles, lookTarget.eulerAngles) < 0.001f) { transform.localRotation = lookTarget; } //Snap to target if close enough
        }

        //Dashing:
        Vector3 actualMoveTarget = Quaternion.LookRotation(transform.forward, transform.up) * moveTarget; //Get vector for actual move target (corrected by player rotation, available to be modified by dash if needed)
        bool forceVelCalc = false; //Janky bool to make dashing work the way I want it
        if (timeSinceDash >= 0) //Player is currently dashing
        {
            timeSinceDash += Time.fixedDeltaTime; //Increment dash time

            //Time triggers:
            if (timeSinceDash <= dashPeriod) //Dash is fully active and moveTarget needs to be modified
            {
                //Adjust move target (to make dash feel smoother):
                float moveTargetBlend = dashCurve.Evaluate(1 - (timeSinceDash / dashPeriod));       //Get interpolant for blending between actual move target velocity and modified dash velocity
                actualMoveTarget = Vector3.Lerp(dashMoveTarget, actualMoveTarget, moveTargetBlend); //Blend between move target decided by inital dash and move target decided by current player input
                forceVelCalc = true;
            }
            if (timeSinceDash >= dashCooldown) timeSinceDash = -1; //Indicate that player is no longer dashing once recharge time has been fulfilled
        }

        //Swimming:
        if (rb.velocity != actualMoveTarget) //Player is not currently traveling at target velocity
        {
            if (moveTarget != Vector3.zero || forceVelCalc) //Player is using input to move in a direction (or is dashing)
            {
                rb.velocity = Vector3.Lerp(rb.velocity, actualMoveTarget, swimAccel); //Lerp velocity (accelerate) toward target
            }
        }
    }

    private void OxygenTimer()
    {
        //If dashing, set the depletion rate to the dash depletion rate
        if (timeSinceDash >= 0 && timeSinceDash <= dashPeriod)
        {
            if (currentDepletionRate != dashOxygenDepletionRate)
                currentDepletionRate = dashOxygenDepletionRate;
        }
        //If not, set the depletion rate to the default depletion rate
        else if(currentDepletionRate != defaultOxygenDepletionRate)
            currentDepletionRate = defaultOxygenDepletionRate;

        //If the oxygen percentage is greater than 0, constantly deplete it
        if(oxygenPercentage > 0)
        {
            oxygenPercentage -= (1 / currentDepletionRate) * Time.deltaTime;

            //Debug.Log("Oxygen At " + oxygenPercentage + "%");

            //Always update the oxygen bar
            LevelManager.main.UpdateOxygenBar(oxygenPercentage);
        }
        //The player is out of oxygen
        else if(!playerDyingFromOxygen)
        {
            Debug.Log("Player Is Out Of Oxygen!");
            StartOxygenDeath();
            playerDyingFromOxygen = true;
        }
    }

    private void StartOxygenDeath()
    {        
        //Wait for the sequence to finish before game over
        StartCoroutine(WaitForGameOver());
    }

    public void StopOxygenDeath()
    {
        //Stop the choking SFX
        if (FindObjectOfType<AudioManager>() != null)
            FindObjectOfType<AudioManager>().Stop("OxygenChokingSFX");

        //Stop the blackout animation coroutine
        StopCoroutine(playerOxygenBlackoutCoroutine);

        //Quickly get rid of the blackout screen
        StartCoroutine(LevelSequencer.main.FadePlayerInOut(0.25f, true));
    }

    private IEnumerator WaitForGameOver()
    {
        //Play choking sound effect
        if (FindObjectOfType<AudioManager>() != null)
            FindObjectOfType<AudioManager>().Play("OxygenChokingSFX", PlayerPrefs.GetFloat("SFXVolume", 0.5f));

        //Slowly fade in the blackout screen
        playerOxygenBlackoutCoroutine = LevelSequencer.main.FadePlayerInOut(secondsUntilOxygenDeath, false);
        StartCoroutine(playerOxygenBlackoutCoroutine);

        //Wait for the death animation to stop before killing the player
        yield return new WaitForSeconds(secondsUntilOxygenDeath);

        //Stop the current threat music
        Cursor.visible = true;
        LevelManager.main.StopThreatMusic((int)SharkController.main.currentThreatLevel);
        if (FindObjectOfType<AudioManager>() != null)
            FindObjectOfType<AudioManager>().Stop("OxygenChokingSFX");
        LevelManager.main.GameOver();
    }

    //INPUT METHODS:
    public void OnMove(InputAction.CallbackContext context)
    {
        //Process input vector:
        Vector2 moveInput = context.ReadValue<Vector2>();                 //Get raw input vector
        float swimAngle = Vector2.Angle(Vector2.up, moveInput) / 180;     //Get float representing degrees off from forward move input is (as interpolant between 0-1)
        moveInput *= baseSwimSpeed * swimDirModCurve.Evaluate(swimAngle); //Apply base swim speed and direction modifier to move input magnitude

        //Compute target movement:
        moveTarget = new Vector3(moveInput.x, moveTarget.y, moveInput.y); //Arrange moveInput values into Vector3 (without erasing existing vertical values)
    }
    public void OnMoveVertical(InputAction.CallbackContext context)
    {
        //Process input value:
        float moveInput = context.ReadValue<float>(); //Get raw input value
        if (moveInput >= 0) moveInput *= ascendSpeed; //Apply ascend speed if moving up
        else moveInput *= descendSpeed;               //Apply descend speed if moving down

        //Compute target movement:
        moveTarget.y = moveInput; //Apply vertical move input to target
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        //Process input vector:
        Vector2 lookInput = context.ReadValue<Vector2>();   //Get raw input vector
        mouseRotation.x += lookInput.x * lookSensitivity.x; //Apply horizontal input to cumulative rotation vector
        mouseRotation.y += lookInput.y * lookSensitivity.y; //Apply vertical input to cumulative rotation vector
        mouseRotation.y = Mathf.Clamp(mouseRotation.y, verticalLookLimits.x, verticalLookLimits.y); //Clamp vertical angle between set limits

        //Compute target rotation:
        Quaternion horizLook = Quaternion.AngleAxis(mouseRotation.x, Vector3.up);  //Get rotation for target horizontal angle
        Quaternion vertLook = Quaternion.AngleAxis(mouseRotation.y, Vector3.left); //Get rotation for target vertical angle
        lookTarget = lookOrigin * horizLook * vertLook;                            //Get new look target
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started) //Dash button has been pushed
        {
            //Validity checks:
            if (timeSinceDash != -1) return;              //Do not allow player to dash before cooldown has ended
            if (CageController.main.playerInside) return; //Do not allow player to dash while inside cage

            //Compute dash impulse:
            Vector3 correctedMoveTarget = moveTarget.normalized;                                        //Get vector for normalized move target (determined by input)
            if (moveTarget == Vector3.zero) correctedMoveTarget = Vector3.forward;                      //Default to dashing straight forward if player gives no specific input
            dashMoveTarget = correctedMoveTarget * dashForce;                                           //Apply dash force to normalized move target (save for velocity calculations later)
            dashMoveTarget = Quaternion.LookRotation(transform.forward, transform.up) * dashMoveTarget; //Rotate move target to align with player's facing direction
            rb.AddForce(dashMoveTarget, ForceMode.Impulse);                                             //Apply dash vector to player rigidbody as instantaneous acceleration

            //Cleanup:
            timeSinceDash = 0; //Indicate that player is now dashing
        }
    }
    public void OnPickUp(InputAction.CallbackContext context)
    {
        //Check for game end:
        if (CageController.main.playerInside &&           //Player is inside cage
            LevelSequencer.main.phase == LevelPhase.Hunt) //Game is in Hunt phase
        {
            LevelSequencer.main.ProgressSequence(); //Progress game sequence
        }

        //Check for item pickup:
        if (context.started && pickupsInRange.Count > 0) StartPickup(); //Indicate that player is picking up item
        if (context.canceled) CancelPickup();                           //Stop picking up item if control is released
    }

    private void StartPickup()
    {
        pickingUpItem = true;                               //Indicate item is being picked up
        timeHoldingPickup = 0;                              //Reset pickup time
        pickupProgressIndicator.gameObject.SetActive(true); //Show progress indicator
    }
    public void CancelPickup()
    {
        pickingUpItem = false;                               //Indicate no items are being picked up
        timeHoldingPickup = 0;                               //Reset pickup time
        pickupProgressIndicator.gameObject.SetActive(false); //Hide progress indicator
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Display the box that the shark spawns nodes at when interested
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, interestedAreaBox);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * playerViewingDistance);
    }
#endif
}
