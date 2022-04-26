using UnityEngine;

[System.Serializable]
public class WaypointBaseController
{
    [System.Serializable]
    public struct Data
    {
        public WaypointItem item;
        public float interactDistance;
        public float maxDistance;
        public bool useEffects;
    }

    public Data data;

    public void SetTarget(GameObject target) => data.item.target = target;
    public void SetTransform(Transform transform) => data.item.transform = transform;
    public void SetWaypointEffect(GameObject effect) => data.item.effect = effect;
    public void SetUseEffects(bool value) => data.useEffects = value;
    public float GetDistance(Vector3 startPos, Vector3 endPos) => Vector3.Distance(startPos, endPos);

    public void EnableEffect(bool value)
    {
        if(data.useEffects)
            data.item.effect.SetActive(value);
    }

    public void EnableWaypoint(bool value)
    {
        data.item.image.enabled = value;
        data.item.message.enabled = value;
    }
}