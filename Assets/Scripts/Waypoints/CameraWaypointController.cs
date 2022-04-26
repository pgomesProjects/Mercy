using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWaypointController : CameraWaypointBaseController
{
    private void FixedUpdate()
    {
        if(data.waypoints != null && data.waypoints.Count > 0)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        foreach(WaypointController waypoint in data.waypoints)
        {
            waypoint.controller.data.item.image.transform.position = UIImagePosition(waypoint.controller.data.item);
            waypoint.controller.data.item.message.text = (WaypointDistance(waypoint.controller.data.item) - waypoint.controller.data.interactDistance) + "M";
        }
    }
}
