using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkController : MonoBehaviour
{
    [SerializeField] private float sharkWidth = 20;
    [SerializeField] private float sharkTeleportationBuffer = 40; //The extra amount of distance to spawn outside of the player's vision

    [Header("Shark Speeds")]
    [SerializeField] private float speed = 5; //The speed of the shark's movement
    [SerializeField] private float threatSpeed = 15; //The speed of the shark's movement when threatened
    private float originalThreatSpeed;
    [SerializeField] private float dashSpeed = 50; //The dash speed of the shark
    private float originalDashSpeed;

    private float originalThreatenedRadius;

    [Header("Shark Rotation Values")]
    [SerializeField] private float rotSpeed = 20; //The speed that the shark rotates its body towards its target position
    [SerializeField] private float lookAtSpeed = 1; //The speed that the shark looks at it's target position
    [SerializeField] private float targetLookedAtSpeed = 0.25f; //The speed that the shark updates it's rotation when looking directly at its position
    [SerializeField] private float threatenedRotSpeed = 80; //The speed that the shark rotates its body towards its target position when threatened
    [SerializeField] private float lookingAtTargetRotSpeed = 1; //The speed that the shark rotates its body towards its target position when threatened

    [Header("Shark Float Values")]
    [SerializeField] private float attackRadius = 5; //The radius in front of the shark where the player is attacked in
    [SerializeField] private float threatenedSpeedMultiplier = 0.1f;
    [SerializeField] private float FOVIncreaseRate = 10;
    [SerializeField] private float interestedAreaDecreaseRate = 5;
    [SerializeField] private float aggressionUpdateUnits = 10; //The amount of points that the player needs to collect to update the aggression

    //[SerializeField] private float initialAttackPower = 5; //The amount of damage the shark initially deals
    //[SerializeField] private float currentAttackPower; //The amount of damage the shark currently deals
    //[SerializeField] private float attackMultiplier = 1.25f; //The longer the player is being damaged by the shark, the more damage is dealt

    [SerializeField] private float detectionRadius = 60;

    private FieldOfView[] sharkFOV;

    [Header("Shark Timers")]
    [Range(0, 60)]
    [SerializeField] private float timeUntilUnknown = 5;
    [Range(0, 60)]
    [SerializeField] private float timeUntilInterested = 5;
    [Range(0, 60)]
    [SerializeField] private float timeUntilThreatened = 5;
    [Range(0, 60)]
    [SerializeField] private float timeUntilTeleport = 20;
    [SerializeField] private float dashTimer = 5;
    [Range(0, 60)]
    [SerializeField] private float threatLevelCooldown = 20;

    private float currentSpeed;
    private float currentRotSpeed;
    private float currentLookAtSpeed;

    private Vector3 targetPos;
    private Quaternion rotTarget;

    private bool nodeGenerated = false;
    private bool playerLockedOn;
    internal bool canMove = true;
    private bool isInSensor, alarmStarted;
    private float currentAlarmTimer;
    private Rigidbody sharkRb;

    internal IEnumerator raiseAlarmCoroutine, interestedCooldownCoroutine, threatenedCooldownCoroutine, dashCoroutine, playerNodeSpawnCoroutine, unknownCounterCoroutine, teleportCounterCoroutine;

    [HideInInspector]
    public enum ThreatLevel { WANDERING = 1, INTEREST, THREATENED, HOMICIDAL, NumberOfThreatLevels = 4 }
    public ThreatLevel currentThreatLevel;
    private bool startUnknownThreatCountdown, makeThreatUnknown;
    private bool startedTeleportCountdown;

    public static SharkController main;

    // Start is called before the first frame update
    private void Awake()
    {
        if (main == null) { main = this; } else { Destroy(gameObject); } //Singleton-ize
    }
    void Start()
    {
        sharkRb = GetComponent<Rigidbody>();
        alarmStarted = false;
        currentSpeed = speed;
        originalThreatSpeed = threatSpeed;
        originalDashSpeed = dashSpeed;
        currentRotSpeed = rotSpeed;
        currentLookAtSpeed = lookAtSpeed;
        playerLockedOn = false;
        makeThreatUnknown = false;
        currentThreatLevel = ThreatLevel.WANDERING;
        LevelManager.main.UpdateThreatUI((int)currentThreatLevel);
        raiseAlarmCoroutine = RaiseAlarm();
        interestedCooldownCoroutine = InterestedCooldown();
        threatenedCooldownCoroutine = ThreatenedCooldown();
        dashCoroutine = DashAtSpeed();
        playerNodeSpawnCoroutine = SpawnNodesOnPlayer();
        unknownCounterCoroutine = StartUnknownCountdown();
        teleportCounterCoroutine = StartTeleportCountdown();

        //0 = Interested FOV, 1 = Threatened FOV
        sharkFOV = GetComponents<FieldOfView>();
        originalThreatenedRadius = sharkFOV[1].radius;
    }

    // Update is called once per frame
    void Update()
    {

        //If the shark is moving
        if (canMove)
        {
            if (IsSharkVisibleFromCamera())
            {
                Debug.Log("Player Can See Shark! Holy Heck!");
                UnhideThreatLevel();
                ResetTeleportCounter();
            }
            else
            {
                HideThreatLevel();
                CheckForTeleport();
            }

            //Check attack sensor before everything
            CheckAttackSensor();

            CheckLookAtTarget(rotTarget);
            SmoothLookAt(rotTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTarget, currentRotSpeed * Time.deltaTime);

            //Always move the shark forward
            //transform.position += transform.forward * currentSpeed * Time.deltaTime;
            sharkRb.velocity = transform.forward * currentSpeed;
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
        else
        {
            sharkRb.velocity = Vector3.zero;
        }

        Debug.DrawLine(PlayerController.main.transform.position, transform.position, Color.cyan);
    }

    private bool IsSharkVisibleFromCamera()
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(PlayerController.main.playerCam);
        var point = transform.position;

        foreach(var plane in planes)
        {
            if(plane.GetDistanceToPoint(point) < 0)
            {
                return false;
            }
        }

        //If the shark is visible, return true based on the viewing distance between the player and shark
        return Vector3.Distance(PlayerController.main.transform.position, transform.position) < PlayerController.main.playerViewingDistance;
    }

    private void HideThreatLevel()
    {
        if (currentThreatLevel == ThreatLevel.WANDERING)
        {
            if (!startUnknownThreatCountdown)
            {
                StartCoroutine(unknownCounterCoroutine);
                startUnknownThreatCountdown = true;
            }
        }
        else
        {
            UnhideThreatLevel();
        }
    }

    private void CheckForTeleport()
    {
        //If the threat is unknown
        if (makeThreatUnknown)
        {
            if (!startedTeleportCountdown)
            {
                StartCoroutine(teleportCounterCoroutine);
                startedTeleportCountdown = true;
            }
        }
    }

    private void ResetTeleportCounter()
    {
        StopCoroutine(teleportCounterCoroutine);
        teleportCounterCoroutine = StartTeleportCountdown();

        startedTeleportCountdown = false;
    }

    private void UnhideThreatLevel()
    {
        makeThreatUnknown = false;
        startUnknownThreatCountdown = false;
        //Update the UI
        LevelManager.main.UpdateThreatUI((int)currentThreatLevel);

        //Stop and reset coroutine
        StopCoroutine(unknownCounterCoroutine);
        unknownCounterCoroutine = StartUnknownCountdown();
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

    public void UpdateAggression()
    {
        Debug.Log("Original Threat Speeds: " + threatSpeed + " | " + dashSpeed);

        //Update threat speed and dash speed based on player's score
        threatSpeed = originalThreatSpeed * (1 + ((int)(PlayerController.main.playerScore / aggressionUpdateUnits) * threatenedSpeedMultiplier));
        dashSpeed = originalDashSpeed * (1 + ((int)(PlayerController.main.playerScore / aggressionUpdateUnits) * threatenedSpeedMultiplier));

        Debug.Log("New Threat Speeds: " + threatSpeed + " | " + dashSpeed);

        Debug.Log("Original Threatened FOV: " + sharkFOV[1].radius);

        //Update threatened FOV
        sharkFOV[1].radius = originalThreatenedRadius + ((int)(PlayerController.main.playerScore / aggressionUpdateUnits) * FOVIncreaseRate);

        //Make sure that the threatened radius cannot get bigger than the interested radius
        if (sharkFOV[1].radius > sharkFOV[0].radius)
        {
            sharkFOV[1].radius = sharkFOV[0].radius;
        }

        Debug.Log("New Threatened FOV: " + sharkFOV[1].radius);

        //Update player interested radius by making it smaller

        Debug.Log("Original Interested Area: " + PlayerController.main.interestedAreaBox);

        Vector3 newInterestedAreaBox = PlayerController.main.originalInterestedAreaBox;
        newInterestedAreaBox.x -= (int)(PlayerController.main.playerScore / aggressionUpdateUnits) * interestedAreaDecreaseRate;
        newInterestedAreaBox.z -= (int)(PlayerController.main.playerScore / aggressionUpdateUnits) * interestedAreaDecreaseRate;

        //Make sure the area box cannot become negative
        if (newInterestedAreaBox.x < 0)
            newInterestedAreaBox.x = 0;

        if (newInterestedAreaBox.z < 0)
            newInterestedAreaBox.z = 0;

        PlayerController.main.interestedAreaBox = newInterestedAreaBox;

        Debug.Log("New Interested Area: " + PlayerController.main.interestedAreaBox);
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

    IEnumerator StartUnknownCountdown()
    {
        Debug.Log("Starting Unknown Countdown...");
        float currentTimer = 0;

        while (currentTimer < timeUntilUnknown)
        {
            currentTimer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Hide Threat Level");
        makeThreatUnknown = true;
        LevelManager.main.UpdateThreatUI(0);
        unknownCounterCoroutine = StartUnknownCountdown();
    }

    IEnumerator StartTeleportCountdown()
    {
        Debug.Log("Starting Teleport Countdown...");
        float currentTimer = 0;

        while (currentTimer < timeUntilTeleport)
        {
            currentTimer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Teleport To Player");
        TeleportInFrontOfPlayer();
        teleportCounterCoroutine = StartTeleportCountdown();

        //Start teleport coroutine again
        StartCoroutine(teleportCounterCoroutine);
    }

    private void TeleportInFrontOfPlayer()
    {
        Vector3 teleportPos = PlayerController.main.transform.position + (PlayerController.main.transform.forward * (PlayerController.main.playerViewingDistance + sharkTeleportationBuffer));

        //If the point is outside of the bounds, spawn in the opposite direction
        if(!GetPoint.instance.PointInsideBounds(teleportPos))
            teleportPos = PlayerController.main.transform.position + (-PlayerController.main.transform.forward * (PlayerController.main.playerViewingDistance + sharkTeleportationBuffer));

        //If the shark's teleport position is less than 60, spawn her high enough so she doesn't spawn below the ground or right underneath the player
        if (teleportPos.y < 55)
            teleportPos.y = 200;

        transform.position = teleportPos;

        float randomFloat = Random.value;

        Quaternion sharkRotation = PlayerController.main.transform.rotation;

        //Two-thirds chance of the shark being turned perpendicular the player
        if (randomFloat > 0.333f)
        {
            sharkRotation.y = sharkRotation.y + 90;
        }

        transform.rotation = sharkRotation;
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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * (sharkWidth / 2), attackRadius, LayerMask.GetMask("Player"));
        if (hitColliders.Length != 0 && !CageController.main.playerInside)
        {
            Debug.Log("Player Has Been Attacked! Game Over!");

            //Stop the current threat music
            Cursor.visible = true;
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

    public IEnumerator InterestedCooldown()
    {
        float timer = 0;
        Debug.Log("Interested Cooldown Timer Started...");
        while (timer < threatLevelCooldown)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        SetThreatLevel(ThreatLevel.WANDERING);
    }

    public IEnumerator ThreatenedCooldown()
    {
        float timer = 0;
        Debug.Log("Threatened Cooldown Timer Started...");
        while (timer < threatLevelCooldown)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        SetThreatLevel(ThreatLevel.WANDERING);
    }

    public void ResetInterestedCooldown()
    {
        interestedCooldownCoroutine = InterestedCooldown();
    }

    public void ResetThreatenedCooldown()
    {
        threatenedCooldownCoroutine = ThreatenedCooldown();
    }

    public ThreatLevel GetThreatLevel() { return currentThreatLevel; }
    public void SetThreatLevel(ThreatLevel threatLevel)
    {
        currentThreatLevel = threatLevel;

        //If the correct threat SFX is not playing, play it 
        if (FindObjectOfType<AudioManager>() != null && !FindObjectOfType<AudioManager>().IsPlaying("Heartbeat" + (int)currentThreatLevel))
        {
            LevelManager.main.StartThreatMusic((int)currentThreatLevel);
        }

        switch (currentThreatLevel)
        {
            case ThreatLevel.WANDERING:
                //Debug.Log("Threat Level: Wandering");
                RemoveThreatSettings();
                break;
            case ThreatLevel.INTEREST:
                //Debug.Log("Threat Level: Interested");
                DestroyAllNodes();
                UnhideThreatLevel();
                RemoveThreatSettings();
                break;
            case ThreatLevel.THREATENED:
                //Debug.Log("Threat Level: Threatened");
                UnhideThreatLevel();
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
