using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Processes player input and movement functionality.
/// </summary>
public class PlayerController : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Objects & Components:
    public static PlayerController main; //Singleton instance of playercontroller
    private PlayerInput playerInput;     //Script handling player input events
    private Rigidbody rb;                //Player rigidbody component

    //Settings:
    [Header("Swimming:")]
    [SerializeField] [Tooltip("Player's base swim speed (in units per second)")]                                                       private float baseSwimSpeed;
    [SerializeField] [Tooltip("Speed at which player can swim up (in world space")]                                                    private float ascendSpeed;
    [SerializeField] [Tooltip("Speed at which player can swim down (in world space)")]                                                 private float descendSpeed;
    [SerializeField] [Tooltip("How quickly player accelerates to target speed while swimming")] [Range(0, 1)]                          private float swimAccel;
    [SerializeField] [Tooltip("Swim speed multiplier (value) depending on horizontal swim direction (t0 is forward, t1 is backward)")] private AnimationCurve swimDirModCurve;

    [Header("Mouselook:")]
    [SerializeField] [Tooltip("How dramatically camera responds to mouse/joystick movements (along each axis")] private Vector2 lookSensitivity;
    [SerializeField] [Tooltip("Smooths out camera motion, but at the cost of responsiveness")] [Range(1, 0)]    private float lookDamping;
    [SerializeField] [MinMaxSlider(-90, 90)] [Tooltip("How far up or down player can look (in degrees)")]       private Vector2 verticalLookLimits;

    //Runtime Memory Vars:
    private Vector2 mouseRotation; //Cumulative angular rotation generated from mouse movements
    private Quaternion lookOrigin; //Original look direction used for reference
    private Quaternion lookTarget; //Rotation which player is currently trying to get to

    private Vector3 moveTarget; //Player's current target velocity

    private int playerScore; //Score that depends on the items the player picks up

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialization:
        if (main == null) { main = this; } else { Destroy(this); } //Singleton-ize playerController script

        //Get objects & components:
        if (!TryGetComponent(out playerInput)) Debug.LogError("Player object is missing PlayerInput script"); //Make sure player has input script
        if (!TryGetComponent(out rb)) Debug.LogError("Player object does not have an attached RigidBody");    //Make sure player has rigidbody

    }
    private void Start()
    {
        //Initialization:
        lookOrigin = transform.localRotation; //Get initial player rotation
        lookTarget = transform.localRotation; //Use this for initial rotation target as well
    }
    private void Update()
    {
        
    }
    private void FixedUpdate()
    {
        //Mouselook:
        if (transform.localRotation != lookTarget) //Player is not currently aligned with lookTarget
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, lookTarget, lookDamping); //Rotate player object toward target
            if (Vector3.Distance(transform.localRotation.eulerAngles, lookTarget.eulerAngles) < 0.001f) { transform.localRotation = lookTarget; } //Snap to target if close enough
        }

        //Swimming:
        if (rb.velocity != moveTarget) //Player is not currently traveling at target velocity
        {
            if (moveTarget != Vector3.zero) //Player is using input to move in a direction
            {
                Vector3 correctedMoveTarget = Quaternion.LookRotation(transform.forward, transform.up) * moveTarget;
                rb.velocity = Vector3.Lerp(rb.velocity, correctedMoveTarget, swimAccel); //Lerp velocity (accelerate) toward target
            }
        }
    }

    public void PickedUpItem(int score)
    {
        //Adds to score counter
        playerScore += score;
        Debug.Log("Score: " + playerScore);
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
        
    }
}
