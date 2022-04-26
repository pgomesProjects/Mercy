using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWaypointBaseController : MonoBehaviour
{
    [System.Serializable]
    public struct WaypointData
    {
        public GameObject worldWaypoints;
        public List<WaypointController> waypoints;
        internal int screenWidth;
        internal int screenHeight;
        internal Camera fpsCam;
    }

    public WaypointData data;

    // Start is called before the first frame update
    void Start()
    {
        data.fpsCam = Camera.main;
        data.screenWidth = Screen.width;
        data.screenHeight = Screen.height;

        if(data.worldWaypoints.transform.childCount > 0)
        {
            for(int i = 0; i < data.worldWaypoints.transform.childCount; i++)
            {
                WaypointController tmpWaypointController = data.worldWaypoints.transform.GetChild(i).GetComponent<WaypointController>();

                if(tmpWaypointController != null)
                    data.waypoints.Add(tmpWaypointController);
            }
        }
    }

    /// <summary>
    /// Gets the position of the waypoint image in the UI and clamps it to the screen
    /// </summary>
    /// <param name="item">Waypoint Item</param>
    /// <returns></returns>
    public Vector2 UIImagePosition(WaypointItem item)
    {
        float itemImageWidth = item.image.GetPixelAdjustedRect().width / 2;
        float itemImageHeight = item.image.GetPixelAdjustedRect().height / 2;

        Vector2 screenPosition = GetItemScreenPosition(item);
        screenPosition.x = ScreenClamp(screenPosition.x, itemImageWidth, data.screenWidth);
        screenPosition.y = ScreenClamp(screenPosition.y, itemImageHeight, data.screenHeight);

        return screenPosition;
    }

    public float WaypointDistance(WaypointItem item)
    {
        return Mathf.Round(Vector3.Distance(item.transform.position, transform.position));
    }

    public float ScreenClamp(float screenPosition, float itemImageWidth, int screenWidth)
    {
        return Mathf.Clamp(screenPosition, itemImageWidth, screenWidth - itemImageWidth);
    }

    /// <summary>
    /// Adjusts the waypoint image's position on the screen based on where the target area for the waypoint is
    /// </summary>
    /// <param name="item">Waypoint Item</param>
    /// <returns></returns>
    public Vector2 GetItemScreenPosition(WaypointItem item)
    {
        float x = item.transform.position.x;
        float y = item.transform.position.y + item.heightOffset;
        float z = item.transform.position.z;
        Vector2 screenPosition = data.fpsCam.WorldToScreenPoint(new Vector3(x, y, z));

        if(Vector3.Dot((item.transform.position - transform.position), transform.forward) < 0)
        {
            if(screenPosition.x < Screen.width / 2)
            {
                screenPosition.x = Screen.width - item.image.GetPixelAdjustedRect().width / 2;
            }
            else
            {
                screenPosition.x = item.image.GetPixelAdjustedRect().width / 2;
            }

            if(screenPosition.y < Screen.height / 2)
            {
                screenPosition.y = Screen.height - item.image.GetPixelAdjustedRect().height / 2;
            }
            else
            {
                screenPosition.y = item.image.GetPixelAdjustedRect().height / 2;
            }
        }

        return screenPosition;
    }
}
