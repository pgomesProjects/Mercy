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

    private bool isMoving, playerDetected;

    // Start is called before the first frame update
    void Start()
    {
        isMoving = true;
        playerDetected = false;
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
            if (!nodeGenerated && !playerDetected)
            {
                transform.LookAt(targetPos);
                targetPos = GetPoint.instance.SpawnNodePoint();
                nodeGenerated = true;
            }
        }
    }
    public void SetNodeGenerated(bool isGenerated) { nodeGenerated = isGenerated; }
}
