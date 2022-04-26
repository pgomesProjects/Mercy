using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct WaypointItem
{
    public Sprite icon;
    public float heightOffset;

    internal Image image;
    internal TextMeshProUGUI message;
    internal GameObject effect;
    internal GameObject waypointUI;
    internal GameObject target;
    internal Transform transform;
}
