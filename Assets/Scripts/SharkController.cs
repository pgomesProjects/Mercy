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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Always move the shark forward
        transform.position += transform.forward * speed * Time.deltaTime;
        rotTarget = Quaternion.LookRotation(targetPos - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTarget, rotSpeed * Time.deltaTime);
        Debug.DrawLine(transform.position, targetPos, Color.red);

        if (!nodeGenerated)
        {
            transform.LookAt(targetPos);
            targetPos = GetPoint.instance.SpawnNodePoint();
            nodeGenerated = true;
        }
    }
    public void SetNodeGenerated(bool isGenerated) { nodeGenerated = isGenerated; }
}
