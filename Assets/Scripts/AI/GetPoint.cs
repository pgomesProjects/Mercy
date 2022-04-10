using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPoint : MonoBehaviour
{
    public static GetPoint instance;

    private Vector3 center;
    public Vector3 size;
    public GameObject nodeObject;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        center = transform.position;
    }

    public Vector3 SpawnNodePoint()
    {
        Vector3 pos = center + new Vector3(Random.Range(-size.x / 2, size.x / 2), Random.Range(-size.y / 2, size.y / 2), Random.Range(-size.z / 2, size.z / 2));
        Instantiate(nodeObject, pos, nodeObject.transform.rotation);
        return pos;
    }

    public Vector3 SpawnPointCloseToPlayer()
    {
        //Keep the y in bounds
        float minimumY = 0;
        if (PlayerController.main.transform.position.y - PlayerController.main.interestedAreaBox.y < 10)
            minimumY = 10;
        else
            minimumY = -PlayerController.main.interestedAreaBox.y;

        Vector3 pos = PlayerController.main.transform.position + new Vector3(Random.Range(-PlayerController.main.interestedAreaBox.x / 2, PlayerController.main.interestedAreaBox.x / 2), Random.Range(minimumY, PlayerController.main.interestedAreaBox.y / 2), Random.Range(-PlayerController.main.interestedAreaBox.z / 2, PlayerController.main.interestedAreaBox.z / 2 / 2));
        Instantiate(nodeObject, pos, nodeObject.transform.rotation);
        return pos;
    }

    public Vector3 SpawnNodeAtPlayer()
    {
        Vector3 pos = PlayerController.main.transform.position;
        Instantiate(nodeObject, pos, nodeObject.transform.rotation);
        return pos;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, size);
    }
#endif
}
