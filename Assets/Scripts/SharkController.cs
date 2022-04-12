using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkController : MonoBehaviour
{
    [SerializeField] private float sharkWidth = 20;

    [Header("Shark Speeds")]
    [SerializeField] private float speed = 5; //The speed of the shark's movement
    [SerializeField] private float threatSpeed = 15; //The speed of the shark's movement when threatened
    [SerializeField] private float dashSpeed = 50; //The dash speed of the shark
     
    [Header("Shark Rotation Values")]
    [SerializeField] private float rotSpeed = 20; //The speed that the shark rotates its body towards its target position
    [SerializeField] private float lookAtSpeed = 1; //The speed that the shark looks at it's target position
    [SerializeField] private float targetLookedAtSpeed = 0.25f; //The speed that the shark updates it's rotation when looking directly at its position
    [SerializeField] private float threatenedRotSpeed = 80; //The speed that the shark rotates its body towards its target position when threatened
    [SerializeField] private float lookingAtTargetRotSpeed = 1; //The speed that the shark rotates its body towards its target position when threatened

    [Header("Shark Attack Values")]
    [SerializeField] private float attackRadius = 5; //The radius in front of the shark where the player is attacked in
    [SerializeField] private float initialAttackPower = 5; //The amount of damage the shark initially deals
    [SerializeField] private float currentAttackPower; //The amount of damage the shark currently deals
    [SerializeField] private float attackMultiplier = 1.25f; //The longer the player is being damaged by the shark, the more damage is dealt

    [SerializeField] private float detectionRadius = 60;

    [Header("Shark Timers")]
    [Range(0, 60)]
    [SerializeField] private float timeUntilInterested = 5;
    [Range(0, 60)]
    [SerializeField] private float timeUntilThreatened = 5;
    [SerializeField] private float dashTimer = 5;
    [Range(0, 60)]
    [SerializeField] private float threatenedCooldown = 20;

    private float currentSpeed;
    private float currentRotSpeed;
    private float currentLookAtSpeed;

    private Vector3 targetPos;
    private Quaternion rotTarget;

    private bool nodeGenerated = false;
    private bool playerLockedOn;
    private bool isMoving, isInSensor, alarmStarted;
    private float currentAlarmTimer;

    internal IEnumerator raiseAlarmCoroutine, threatenedCooldownCoroutine, dashCoroutine, playerNodeSpawnCoroutine;

    [HideInInspector]
    public enum ThreatLevel { WANDERING, INTEREST, THREATENED }
    public ThreatLevel currentThreatLevel;
    public static SharkController main;

    // Start is called before the first frame update
    private void Awake()
    {
        if (main == null) { main = this; } else { Destroy(gameObject); } //Singleton-ize
    }
    void OnEnable()
    {
        isMoving = true;
        alarmStarted = false;
        currentSpeed = speed;
        currentRotSpeed = rotSpeed;
        currentLookAtSpeed = lookAtSpeed;
        playerLockedOn = false;
        currentThreatLevel = ThreatLevel.WANDERING;
        LevelManager.main.UpdateThreatUI((int)currentThreatLevel);
        raiseAlarmCoroutine = RaiseAlarm();
        threatenedCooldownCoroutine = ThreatenedCooldown();
        dashCoroutine = DashAtSpeed();
        playerNodeSpawnCoroutine = SpawnNodesOnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        //If the shark is moving
        if (isMoving)
        {
            //Check attack sensor before everything
            CheckAttackSensor();

            CheckLookAtTarget(rotTarget);
            SmoothLookAt(rotTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTarget, currentRotSpeed * Time.deltaTime);

            //Always move the shark forward
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
            rotTarget = Quaternion.LookRotation(targetPos - transform.position);
            Debug.DrawLine(transform.position, targetPos, Color.red);

            //Change behavior based on threat level
            switch (currentThreatLevel)
            {
                case ThreatLevel.WANDERING:
                    WanderBehavior();
                    break;
                case ThreatLevel.INTEREST:
                    InterestedBehavior();
                    break;
                case ThreatLevel.THREATENED:
                    ThreatenedBehavior();
                    break;
            }

            //Check the body sensor if they are not threatened
            CheckBodySensor();
        }

        Debug.DrawLine(PlayerController.main.transform.position, transform.position, Color.cyan);
    }

    private void SmoothLookAt(Quaternion targetRotation)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentLookAtSpeed * Time.deltaTime);
    }


    private void CheckLookAtTarget(Quaternion targetRotation)
    {
        //If the degree between the current rotation and the target rotation is less than 5 degrees, they have looked at their target
        if (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRotation)) < 5)
        {
            //If the shark is looking at its target, rotate slowly if position updates
            currentLookAtSpeed = targetLookedAtSpeed;
            currentRotSpeed = lookingAtTargetRotSpeed;
        }
        else
        {
            currentLookAtSpeed = lookAtSpeed;
            //Change rotation speed based on threat level
            switch (currentThreatLevel)
            {
                case ThreatLevel.THREATENED:
                    currentRotSpeed = threatenedRotSpeed;
                    break;
                default:
                    currentRotSpeed = rotSpeed;
                    break;
            }
        }
    }

    private void WanderBehavior()
    {
        //Generate random nodes while the player is not currently detected
        if (!nodeGenerated)
        {
            targetPos = GetPoint.instance.SpawnNodePoint();
            nodeGenerated = true;
        }
    }

    private void InterestedBehavior()
    {
        //Generate random nodes while the player is not currently detected
        if (!nodeGenerated)
        {
            targetPos = GetPoint.instance.SpawnPointCloseToPlayer();
            nodeGenerated = true;
        }
    }

    private void ThreatenedBehavior()
    {
        //If they have not locked onto the player, set their threatened values
        if (!playerLockedOn)
        {
            currentSpeed = threatSpeed;
            BeginThreat();
            playerLockedOn = true;

            //Start constantly spawning nodes on the player
            StartCoroutine(playerNodeSpawnCoroutine);
        }
    }


    private void BeginThreat()
    {
        //Destroy any existing nodes and start the dashing coroutine
        DestroyAllNodes();
        StartCoroutine(dashCoroutine);
        //Update the UI and the music
        LevelManager.main.UpdateThreatUI((int)currentThreatLevel);
    }

    IEnumerator SpawnNodesOnPlayer()
    {
        //Constantly have one node spawning at the player
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            DestroyAllNodes();
            targetPos = GetPoint.instance.SpawnNodeAtPlayer();
        }
    }

    IEnumerator DashAtSpeed()
    {
        //Every few seconds, dash for half a second
        while (true)
        {
            yield return new WaitForSeconds(dashTimer);
            Debug.Log("Dash!");
            currentSpeed = dashSpeed;
            yield return new WaitForSeconds(0.5f);
            currentSpeed = threatSpeed;
        }
    }

    private void DestroyAllNodes()
    {
        //Get rid of all existing nodes
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");
        foreach (GameObject node in nodes)
            Destroy(node);
        nodeGenerated = false;
    }

    private void CheckAttackSensor()
    {
        //If the player is currently in the shark's body sensor, start raising an alarm
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 10, attackRadius, LayerMask.GetMask("Player"));
        if (hitColliders.Length != 0)
        {
            Debug.Log("Player Has Been Attacked! Game Over!");

            //Stop the current threat music
            LevelManager.main.StopThreatMusic((int)currentThreatLevel);
            LevelManager.main.GameOver();
        }
    }

    private void CheckBodySensor()
    {
        //If the player is currently in the shark's body sensor, start raising an alarm
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Player"));
        if(hitColliders.Length != 0  && currentThreatLevel != ThreatLevel.THREATENED)
        {
            //Debug.Log("Player Is Too Close!");
            isInSensor = true;
            //Start the alarm raising coroutine if it has not been activated
            if (!alarmStarted)
            {
                StopCoroutine(threatenedCooldownCoroutine);
                currentAlarmTimer = timeUntilInterested;
                ResetRaiseAlarm();
                StartCoroutine(raiseAlarmCoroutine);
                alarmStarted = true;
            }
        }
        //If the player is no longer in the sensor, stop the alarm and start the cooldown
        else if (isInSensor)
        {
            StopCoroutine(raiseAlarmCoroutine);
            isInSensor = false;
            alarmStarted = false;
            if(currentThreatLevel != ThreatLevel.WANDERING)
            {
                ResetThreatenedCooldown();
                StartCoroutine(threatenedCooldownCoroutine);
            }
        }

    }

    public IEnumerator RaiseAlarm()
    {
        yield return new WaitForSeconds(currentAlarmTimer);

        SetThreatLevel(ThreatLevel.INTEREST);

        currentAlarmTimer = timeUntilThreatened;
        yield return new WaitForSeconds(currentAlarmTimer);

        SetThreatLevel(ThreatLevel.THREATENED);
    }

    public void ResetRaiseAlarm()
    {
        raiseAlarmCoroutine = RaiseAlarm();
    }

    public IEnumerator ThreatenedCooldown()
    {
        float timer = 0;
        Debug.Log("Cooldown Timer Started...");
        while (timer < threatenedCooldown)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        SetThreatLevel(ThreatLevel.WANDERING);
    }

    public void ResetThreatenedCooldown()
    {
        threatenedCooldownCoroutine = ThreatenedCooldown();
    }

    public ThreatLevel GetThreatLevel() { return currentThreatLevel; }
    public void SetThreatLevel(ThreatLevel threatLevel)
    {
        if(threatLevel != currentThreatLevel)
        {
            //Stop the current threat music before beginning a new one
            LevelManager.main.StopThreatMusic((int)currentThreatLevel);
            LevelManager.main.StartThreatMusic((int)threatLevel);
        }
        currentThreatLevel = threatLevel;
        switch (currentThreatLevel)
        {
            case ThreatLevel.WANDERING:
                //Debug.Log("Threat Level: Wandering");
                RemoveThreatSettings();
                break;
            case ThreatLevel.INTEREST:
                //Debug.Log("Threat Level: Interested");
                DestroyAllNodes();
                RemoveThreatSettings();
                break;
            case ThreatLevel.THREATENED:
                //Debug.Log("Threat Level: Threatened");
                break;
        }

        //Update the UI
        LevelManager.main.UpdateThreatUI((int)currentThreatLevel);
    }

    private void RemoveThreatSettings()
    {
        //Reset the shark to non-aggressive speeds and behavior
        currentSpeed = speed;
        currentRotSpeed = rotSpeed;
        playerLockedOn = false;
        StopCoroutine(dashCoroutine);
        StopCoroutine(playerNodeSpawnCoroutine);
        DestroyAllNodes();
    }

    public void SetNodeGenerated(bool isGenerated) { nodeGenerated = isGenerated; }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + transform.forward * sharkWidth / 2, attackRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}
