using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaypointManager : MonoBehaviour
{
    [System.Serializable]
    struct Data
    {
        public int totalWayPoints;
        public GameObject waypointCanvas;
        public GameObject worldWaypoints;
        public GameObject waypointUIPrefab;
        public List<GameObject> waypointPrefab;
    }

    [SerializeField] Data data;

    private void Awake()
    {
        for(int i = 0; i < data.totalWayPoints; i++)
        {
            if(data.waypointPrefab.Count > 0)
            {
                for(int j = 0; j < data.waypointPrefab.Count; j++)
                {
                    GameObject tmpWaypoint = Instantiate(data.waypointPrefab[j]);
                    WaypointController tmpWaypointController = tmpWaypoint.GetComponent<WaypointController>();

                    GameObject tmpWaypointUI = Instantiate(data.waypointUIPrefab);
                    tmpWaypointUI.GetComponent<Image>().sprite = tmpWaypointController.controller.data.item.icon;
                    tmpWaypointUI.transform.SetParent(data.waypointCanvas.transform);

                    tmpWaypointController.controller.data.item.image = tmpWaypointUI.GetComponent<Image>();
                    tmpWaypointController.controller.data.item.message = tmpWaypointUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    tmpWaypointController.controller.data.item.waypointUI = tmpWaypointUI;
                    tmpWaypointController.transform.SetParent(data.worldWaypoints.transform);
                    tmpWaypointController.transform.position = data.waypointPrefab[j].transform.position;
                }
            }
        }
    }
}
