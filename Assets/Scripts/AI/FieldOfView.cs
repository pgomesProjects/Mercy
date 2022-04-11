using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float blindSpotAngle, sightAngle;

    [HideInInspector]
    public PlayerController playerObject;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeePlayer;

    private SharkController sharkObject;

    private void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        sharkObject = GetComponent<SharkController>();
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        //Checks if they can see the player every 0.2 seconds
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        //Checks to see if there's anything overlapping the radius
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;


            //If the target is in the blindspot range, they cannot see the player
            if ((Vector3.Angle(transform.forward, directionToTarget) < blindSpotAngle / 2) && sharkObject.currentThreatLevel != SharkController.ThreatLevel.THREATENED)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeePlayer = false;
                    //Debug.Log("Player Is In Blindspot!");
                }
            }
            //If the target is in the sight range, they can see the player
            else if (Vector3.Angle(transform.forward, directionToTarget) < sightAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeePlayer = true;
                    //If they can see the player, stop any alarm or cooldown coroutines and keep at threatened
                    StopCoroutine(sharkObject.raiseAlarmCoroutine);
                    StopCoroutine(sharkObject.threatenedCooldownCoroutine);
                    sharkObject.SetThreatLevel(SharkController.ThreatLevel.THREATENED);
                    //Debug.Log("Shark Can See Player!");
                }
                else
                    canSeePlayer = false;
            }
            else
                canSeePlayer = false;
        }
        //If they lose sight of the player, start a cooldown before they go back to wandering
        else if (canSeePlayer)
        {
            canSeePlayer = false;
            //Debug.Log("Shark Lost Sight Of Player!");
            sharkObject.ResetThreatenedCooldown();
            StartCoroutine(sharkObject.threatenedCooldownCoroutine);
        }
    }
}