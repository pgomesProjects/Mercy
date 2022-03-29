using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkController : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    private bool nodeGenerated = false;
    private Vector3 targetPos;
    private Quaternion rotTarget;
    [SerializeField] private float rotSpeed = 20;

    [SerializeField] private float initialAttackPower = 5; //The amount of damage the shark initially deals
    [SerializeField] private float currentAttackPower; //The amount of damage the shark currently deals
    [SerializeField] private float attackMultiplier = 1.25f; //The longer the player is being damaged by the shark, the more damage is dealt

    [SerializeField] private float detectionRadius = 30;
    [SerializeField] private float timeUntilInterested = 5;
    [SerializeField] private float timeUntilThreatened = 5;
    private Vector3 detectionArea;

    private bool isMoving;

    [HideInInspector]
    public enum ThreatLevel { WANDERING, INTEREST, THREATENED }
    public ThreatLevel currentThreatLevel;

    // Start is called before the first frame update
    void Start()
    {
        isMoving = true;
        detectionArea = new Vector3(detectionRadius, detectionRadius, detectionRadius);
        currentThreatLevel = ThreatLevel.WANDERING;
    }

    // Update is called once per frame
    void Update()
    {
        //If the shark is moving
        if (isMoving)
        {
            //Always move the shark forward
            transform.position += transform.forward * speed * Time.deltaTime;
            rotTarget = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTarget, rotSpeed * Time.deltaTime);
            Debug.DrawLine(transform.position, targetPos, Color.red);

            //Generate random nodes while the player is not currently detected
            if (!nodeGenerated && currentThreatLevel == ThreatLevel.WANDERING)
            {
                transform.LookAt(targetPos);
                targetPos = GetPoint.instance.SpawnNodePoint();
                nodeGenerated = true;
            }
        }

        //Vector3.Distance(PlayerController.main.transform.position, transform.position);
        Debug.DrawLine(PlayerController.main.transform.position, transform.position, Color.cyan);
    }

    public ThreatLevel GetThreatLevel() { return currentThreatLevel; }
    public void SetThreatLevel(ThreatLevel threatLevel) { currentThreatLevel = threatLevel; }
    public void SetNodeGenerated(bool isGenerated) { nodeGenerated = isGenerated; }
}
