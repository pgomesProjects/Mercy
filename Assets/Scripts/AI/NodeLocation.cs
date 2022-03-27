using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeLocation : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shark"))
        {
            SharkController sharkController = other.GetComponent<SharkController>();
            sharkController.SetNodeGenerated(false);
            Destroy(gameObject);
        }
    }
}
