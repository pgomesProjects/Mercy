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
        if(controller.GetDistance(transform.position, controller.data.item.target.transform.position) < controller.data.interactDistance)
        {
            controller.EnableWaypoint(false);
            controller.EnableEffect(true);
        }
        else
        {
            controller.EnableWaypoint(true);
            controller.EnableEffect(false);
        }
    }

}
