using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sends a signal to CageController when player enters the dive cage (in order to leave the level)
/// </summary>
public class CageEntryDetector : MonoBehaviour
{
    //RUNTIME METHODS:
    private void Start()
    {
        //Validity check:
        if (CageController.main == null) { Debug.LogWarning("Scene has no cage controller, destroying cage entry detector..."); Destroy(this); } //Make sure dive cage is present in scene
    }

    //PHYSICS METHODS:
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) { CageController.main.PlayerEnteredCage(); } //Indicate that player has entered cage
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) { CageController.main.PlayerLeftCage(); } //Indicate that player has left cage
    }
}
