using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomEnums;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CustomEnums
{
    //Classes, Enums & Scripts:
    /// <summary>
    /// Describes game phase determined by LevelSequencer.
    /// </summary>
    public enum LevelPhase { Entry, GracePeriod, Hunt, Exit }
}

/// <summary>
/// Manages timing for systems involved in game flow, such as player entry sequence.
/// </summary>
public class LevelSequencer : MonoBehaviour
{
    //Objects & Components:
    public static LevelSequencer main; //Singleton instance of this script

    [Header("Local Scene Components:")]
    [SerializeField] [Tooltip("Position which dive cage will move toward during entry sequence")] private Transform cageDescendTarget;
    [SerializeField] [Tooltip("Image used to block player's view when necessary")]                private Image blackoutScreen;
    [SerializeField][Tooltip("CanvasGroup used to show / hide the player's HUD")]                 private CanvasGroup playerIndicator;
    [SerializeField][Tooltip("CanvasGroup used to show / hide the waypoint HUD")]                 private CanvasGroup waypointIndicator;

    //Settings:
    [Header("Entry Sequence Settings:")]
    [SerializeField] [Tooltip("Time cage takes to lower from starting position to target position (at which point entry sequence ends)")] private float cageDescendTime;
    [SerializeField] [Tooltip("Describes movement of dive cage (toward target) throughout entry sequence")]                               private AnimationCurve cageDescendCurve;
    [SerializeField] [Tooltip("How long it takes for player vision/hearing to fade in when entering level")]                              private float entryFadeTime;
    [Header("Grace Period Sequence Settings:")]
    [SerializeField] [Tooltip("Max distance from cage player may travel before grace period ends")]            private float graceEndDistance;
    [SerializeField] [Tooltip("Max time (in seconds) before shark begins hunting (after player leaves cage)")] private float maxGraceTime;
    [Header("Exit Sequence Settings:")]
    [SerializeField] [Tooltip("Duration (in seconds) of exit sequence")]                                   private float exitTime;
    [SerializeField] [Tooltip("Describes movement of dive cage (toward origin) throughout exit sequence")] private AnimationCurve cageAscendCurve;
    [SerializeField] [Tooltip("Name of scene loaded at end of exit sequence")]                             private string exitScene;

    //Runtime Vars:
    /// <summary>Level's current sequence.</summary>
    internal LevelPhase phase = LevelPhase.Entry; //Start at entry phase

    private float sequenceTime = 0;  //Time (in seconds) remaining in current sequence.  Zero if timer is not in use
    private Vector3 cageOrigPos;     //Original position of dive cage
    private Vector3 cageTargPos;     //Current target position for dive cage

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (main == null) { main = this; } else { Destroy(this); } //Singleton-ize this script

        //Check for errors:
        if (cageDescendTarget == null) { Debug.LogError("Descend target field is not assigned, level sequencer cannot run"); Destroy(this); } //Make sure dive cage entry target has been given
        if (blackoutScreen == null) { Debug.LogError("Blackout screen is not assigned, level sequencer cannot run"); Destroy(this); }         //Make sure blackout screen has been given
    }
    private void Start()
    {
        //Contingency checks:
        if (PlayerController.main == null) { Debug.LogError("Player is missing from scene"); Destroy(this); }  //Ensure scene has a player
        if (CageController.main == null) { Debug.LogError("Dive Cage is missing from scene"); Destroy(this); } //Ensure scene has a dive cage

        //Delegate subscriptions:
        CageController.playerEntersCageDelegate += PlayerCageEntry; //Subscribe to player cage entry event
        CageController.playerLeavesCageDelegate += PlayerCageExit;  //Subscribe to player cage exit event

        //Set starting variables:
        sequenceTime = cageDescendTime;                       //Set initial sequence time to cage descend time (beginning first phase)
        cageOrigPos = CageController.main.transform.position; //Get starting position of dive cage
        cageTargPos = cageDescendTarget.transform.position;   //Get starting target for dive cage

        //Entry sequence prep:
        if (FindObjectOfType<AudioManager>() != null)
            FindObjectOfType<AudioManager>().Play("CageChain", PlayerPrefs.GetFloat("SFXVolume", 0.5f));
        StartCoroutine(FadePlayerInOut(entryFadeTime, true)); //Begin fading in player senses
        SharkController.main.canMove = false;                 //Make sure the shark can't move
        Cursor.visible = false;                               //Make cursor invisible
    }
    private void Update()
    {
        //Check for sequence time expiration:
        if (sequenceTime > 0) //Do not update sequence timer if it is set to zero
        {
            sequenceTime -= Time.deltaTime;            //Subtract deltaTime from time tracker
            if (sequenceTime <= 0) ProgressSequence(); //Start next sequence if sequence time expires
        }

        //Phase-specific updates:
        switch (phase) //Determine behavior based on current phase
        {
            case LevelPhase.Entry: //Entry sequence updates
                //Move dive cage:
                float t = 1 - cageDescendCurve.Evaluate(1 - (sequenceTime / cageDescendTime)); //Get interpolant value as sequence time applied to entry curve (use inversion fuckery to make curve make sense)
                Vector3 newPos = Vector3.LerpUnclamped(cageOrigPos, cageTargPos, t);           //Interpolate between cage's original position and cage's position target to get new position
                CageController.main.transform.position = newPos;                               //Move cage to new position
                break;
            case LevelPhase.GracePeriod: //Grace period updates
                if (Vector3.Distance(PlayerController.main.transform.position, CageController.main.transform.position) >= graceEndDistance) //Player has strayed far enough away from the dive cage
                {
                    ProgressSequence(); //Begin next sequence
                }
                break;
            case LevelPhase.Exit: //Exit sequence updates
                float t2 = cageAscendCurve.Evaluate(1 - (sequenceTime / exitTime));    //Get interpolant value as sequence time applied to exit curve
                Vector3 newPos2 = Vector3.LerpUnclamped(cageTargPos, cageOrigPos, t2); //Interpolate between cage's end position and origin to get new position
                CageController.main.transform.position = newPos2;                      //Move cage to new position
                break;
            default: break;
        }
    }
    private void OnDisable()
    {
        //Unsubscribe from delegates:
        CageController.playerEntersCageDelegate -= PlayerCageEntry; //Unsubscribe from player cage entry event
        CageController.playerLeavesCageDelegate -= PlayerCageExit;  //Unsubscribe from player cage exit event
    }

    //EVENTS & DELEGATES:
    /// <summary>
    /// Called when player enters dive cage
    /// </summary>
    public void PlayerCageEntry()
    {
        switch (phase) //Determine behavior based on phase
        {
            case LevelPhase.GracePeriod: //Player enters cage during grace period
                sequenceTime = 0;        //Reset countdown until hunt
                break;
            default: break;
        }
    }
    /// <summary>
    /// Called when player leaves dive cage
    /// </summary>
    public void PlayerCageExit()
    {
        switch (phase) //Determine behavior based on phase
        {
            case LevelPhase.GracePeriod:     //Player exits cage during grace period
                sequenceTime = maxGraceTime; //Begin countdown to hunt
                break;
            default: break;
        }
    }

    //COROUTINES:
    /// <summary>
    /// Fades player senses in or out in a certain number of seconds.
    /// </summary>
    /// <param name="fadeTime">How long fade will take (in seconds)</param>
    /// <param name="fadeType">Whether to fade in (TRUE) or out (FALSE)</param>
    /// <returns></returns>
    IEnumerator FadePlayerInOut(float fadeTime, bool fadeType)
    {
        //Initialize:
        Color newColor = blackoutScreen.color; //Get color from blackout screen

        //Fade process:
        for (float timePassed = 0; timePassed < fadeTime; timePassed += Time.deltaTime) //Run this code every frame for fadeTime seconds
        {
            float fadeAmount = timePassed / fadeTime;  //Get interpolant value based on time pased to use with screen blocker
            if (fadeType) fadeAmount = 1 - fadeAmount; //Reverse fade amount when fading in (make screen more transparent as time progresses)
            newColor.a = fadeAmount;                   //Set new alpha for blackout screen
            blackoutScreen.color = newColor;           //Apply new color
            yield return null;                         //Wait until next frame
        }

        //Finish fade:
        if (fadeType) newColor.a = 0; //Make screen fully transparent if fading in
        else newColor.a = 1;          //Make screen fully opaque if fading out
    }

    /// <summary>
    /// Fades player indicator in or out in a certain number of seconds.
    /// </summary>
    /// <param name="fadeTime">How long fade will take (in seconds)</param>
    /// <param name="fadeType">Whether to fade in (TRUE) or out (FALSE)</param>
    /// <returns></returns>
    IEnumerator FadeIndicatorInOut(float fadeTime, bool fadeType)
    {
        //Initialize:
        float startingAlpha = playerIndicator.alpha;

        //Fade process:
        for (float timePassed = 0; timePassed < fadeTime; timePassed += Time.deltaTime) //Run this code every frame for fadeTime seconds
        {
            float fadeAmount = timePassed / fadeTime;  //Get interpolant value based on time pased to use with indicator blocker
            if (fadeType) fadeAmount = 1 - fadeAmount; //Reverse fade amount when fading in (make indicator more transparent as time progresses)
            startingAlpha = fadeAmount;                   //Set new alpha for indicator
            playerIndicator.alpha = startingAlpha;           //Apply new alpha
            waypointIndicator.alpha = startingAlpha;
            yield return null;                         //Wait until next frame
        }

        //Finish fade:
        if (fadeType)
        {
            playerIndicator.alpha = 0; //Make indicator fully transparent if fading in
            waypointIndicator.alpha = 0;
        }
        else
        {
            playerIndicator.alpha = 1;          //Make indicator fully opaque if fading out
            waypointIndicator.alpha = 1;
        }
    }

    //SEQUENCE METHODS:
    /// <summary>
    /// Progresses game sequence from current phase to next in order.
    /// </summary>
    public void ProgressSequence()
    {
        //Initial stuff:
        sequenceTime = 0; //Reset sequence time by default when starting a new sequence

        //Phase transition behavior:
        switch (phase) //Determine what to do next based on current phase
        {
            case LevelPhase.Entry: phase = LevelPhase.GracePeriod; //Transition from Entry to Grace Period
                //External activations:
                CageController.main.transform.position = cageDescendTarget.position; //Make sure cage is at entry position
                CageController.main.ReleasePlayer();                                 //Release player from dive cage
                CageController.main.GetComponent<FloatBob>().Initialize(true);       //Re-initialize and enable cage bob
                                                                                     
                //Update sequencer:
                if (FindObjectOfType<AudioManager>() != null)
                    FindObjectOfType<AudioManager>().Stop("CageChain");              //Stop cage chain sound effect
                PlayerController.main.oxygenIsDepleting = true;                      //Start depleting the player's oxygen
                LevelManager.main.StartThreatMusic(1);                               //Start the threat music
                StartCoroutine(FadeIndicatorInOut(0.25f, false));                    //Fade in the player HUD
                break;
            case LevelPhase.GracePeriod: phase = LevelPhase.Hunt; //Transition from Grace Period to Hunt
                //External activations:
                SharkController.main.canMove = true; //Activate the shark
                //Update sequencer:

                break;
            case LevelPhase.Hunt: phase = LevelPhase.Exit; //Transition from Hunt to Exit
                //External activations:
                CageController.main.CapturePlayer();                              //Trap player in cage
                CageController.main.GetComponent<FloatBob>().EndImmediate(false); //End cage bobbing animation
                //Update sequencer:
                sequenceTime = exitTime;                              //Set timer to begin exit sequence
                cageTargPos = CageController.main.transform.position; //Get current position of cage to lerp from
                CageController.main.HideIndicator();                  //Remove the exit indicator
                StartCoroutine(FadeIndicatorInOut(0.25f, true));     //Remove the HUD indicators
                break;
            case LevelPhase.Exit: //Transition from Exit to next scene
                //Scene cleanup:
                if (FindObjectOfType<AudioManager>() != null) //Scene has audio manager
                {
                    FindObjectOfType<AudioManager>().Stop(GameData.playingSongName);     //Stop audio manager
                    LevelManager.main.StopThreatMusic((int)SharkController.main.currentThreatLevel); //Stop level music
                }

                GameData.finalScore = PlayerController.main.playerScore;
                Cursor.visible = true;             //Make cursor visible
                SceneManager.LoadScene(exitScene); //Load exit scene
                break;
            default:
                break;
        }

        //Cleanup:
        Debug.Log("New phase entered: " + phase); //Indicate that a new sequence has started
    }
}