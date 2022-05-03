using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    [SerializeField] private int scoreValue;
    [SerializeField] private float massInPounds;
    [SerializeField] private Material litMat;

    private MeshRenderer rnd;
    internal bool canBeCollected;
    private PlayerActionsMap playerActions;
    private Material origMat;
    internal PickupSpawner spawner; //This pickup's spawner (if it was spawned, may be null)
    private void Awake()
    {
        playerActions = new PlayerActionsMap();
        //playerActions.Player.PickUp.performed += _ => CollectItem();
    }

    // Start is called before the first frame update
    void Start()
    {
        canBeCollected = false;
        rnd = GetComponent<MeshRenderer>();
        origMat = rnd.material;
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Player is in pick up radius, can pick up item
        if (other.CompareTag("Player"))
        {
            canBeCollected = true;
            //selectionLight.gameObject.SetActive(true);
            PlayerController.pickupsInRange.Add(this);
            rnd.material = litMat;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Player left pick up radius, cannot pick up item
        if (other.CompareTag("Player"))
        {
            canBeCollected = false;
            //selectionLight.gameObject.SetActive(false);
            PlayerController.pickupsInRange.Remove(this);
            rnd.material = origMat;
            if (PlayerController.pickupsInRange.Count == 0) PlayerController.main.CancelPickup(); //Make sure to cancel pickup procedure if this was the only item in range of player
        }
    }

    public void CollectItem()
    {
        //If the item can be collected, tell the player that the item has been picked up and add to score
        if (canBeCollected)
        {
            PlayerController.main.playerScore += scoreValue;
            Debug.Log("Score: " + PlayerController.main.playerScore);
            LevelManager.main.UpdateScore(PlayerController.main.playerScore);
            SharkController.main.UpdateAggression();
            PlayerController.pickupsInRange.Remove(this);
            PickupSpawner.SpawnPickup();
            spawner?.spawnedPickups.Remove(gameObject);
            Destroy(gameObject);
        }
    }
}
