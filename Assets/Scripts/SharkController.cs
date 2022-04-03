using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkController : MonoBehaviour
{
    [Header("Shark Speeds")]
    [SerializeField] private float speed = 5;
    [SerializeField] private float threatSpeed = 15;
    [SerializeField] private float dashSpeed = 50;

    [Header("Shark Rotation Values")]
    [SerializeField] private float rotSpeed = 20;
    [SerializeField] private float threatenedRotSpeed = 80;

    [Header("Shark Attack Values")]
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

    // Start is called before the first frame update
    void Start()
    {
        isMoving = true;
        alarmStarted = false;
        currentSpeed = speed;
        currentRotSpeed = rotSpeed;
        playerLockedOn = false;
        currentThreatLevel = ThreatLevel.WANDERING;
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
            //Always move the shark forward
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
            rotTarget = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTarget, currentRotSpeed * Time.deltaTime);
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

    private void WanderBehavior()
    {
        //Generate random nodes while the player is not currently detected
        if (!nodeGenerated)
        {
            transform.LookAt(targetPos);
            targetPos = GetPoint.instance.SpawnNodePoint();
            nodeGenerated = true;
        }
    }

    private void InterestedBehavior()
    {

    }

    private void ThreatenedBehavior()
    {
        //If they have not locked onto the player, set their threatened values
        if (!playerLockedOn)
        {
            currentSpeed = threatSpeed;
            currentRotSpeed = threatenedRotSpeed;
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
    }

    IEnumerator SpawnNodesOnPlayer()
    {
        //Constantly have one node spawning at the player
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            transform.LookAt(targetPos);
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

    private void CheckBodySensor()
    {
        //If the player is currently in the shark's body sensor, start raising an alarm
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Player"));
        if(hitColliders.Length != 0  && currentThreatLevel != ThreatLevel.THREATENED)
        {
            Debug.Log("Player Is Too Close!");
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
        currentThreatLevel = threatLevel;
        switch (currentThreatLevel)
        {
            case ThreatLevel.WANDERING:
                Debug.Log("Threat Level: Wandering");
                RemoveThreatSettings();
                break;
            case ThreatLevel.INTEREST:
                Debug.Log("Threat Level: Interested");
                RemoveThreatSettings();
                break;
            case ThreatLevel.THREATENED:
                Debug.Log("Threat Level: Threatened");
                break;
        }
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}
