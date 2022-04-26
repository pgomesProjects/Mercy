using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassController : MonoBehaviour
{
    [SerializeField] private Transform cageTransform;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = transform.InverseTransformDirection(cageTransform.position - PlayerController.main.transform.position);
        float angle = Mathf.Atan2(-dir.x, dir.z) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }
}
