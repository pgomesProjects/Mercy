using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointController : MonoBehaviour
{

    public WaypointBaseController controller;

    private void Awake()
    {
        controller.SetTarget(GameObject.FindGameObjectWithTag("Player"));
        controller.SetTransform(transform);
        controller.SetUseEffects(false);
        if(transform.childCount > 0)
        {
            controller.SetUseEffects(true);
            controller.SetWaypointEffect(transform.GetChild(0).gameObject);
        }
    }

    private void FixedUpdate()
    {
        //If the player is too close to the waypoint
        if(controller.GetDistance(transform.position, controller.data.item.target.transform.position) < (controller.data.interactDistance * 10))
        {
            controller.EnableWaypoint(false);
            controller.EnableEffect(true);
        }
        //If the player is too far from the waypoint
        else if (controller.GetDistance(transform.position, controller.data.item.target.transform.position) > (controller.data.maxDistance * 10) )
        {
            controller.EnableWaypoint(false);
        }
        else
        {
            controller.EnableWaypoint(true);
            controller.EnableEffect(false);
        }
    }

}
