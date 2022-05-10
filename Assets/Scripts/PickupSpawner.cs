using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    //Static:
    /// <summary>
    /// List of all pickupspawners in scene.
    /// </summary>
    public static List<PickupSpawner> spawners = new List<PickupSpawner>();
    private static readonly float minSpawnDistance = 100;   //Minimum distance between player and valid spawner
    private static readonly float minSpawnAngle = 90;       //Minimum angle between player facing angle and direction of valid spawner from player
    private static readonly float playerViewDistance = 800; //Distance at which player cannot see anything through fog (approximated)

    //Objects & Components:
    [SerializeField, Tooltip("Prefabs for pickups to spawn")] private List<GameObject> pickupPrefabs = new List<GameObject>();
    internal List<GameObject> spawnedPickups = new List<GameObject>(); //List of pickups in scene spawned by this spawner

    //Settings:
    [SerializeField, Tooltip("Radius of pickup spawn area")]                             private float spawnRadius;
    [SerializeField, Tooltip("Maximum number of pickups allowed to spawn in this area")] private int maxPickups;
    [SerializeField, Tooltip("Objects to spawn on start of game")]                       private int spawnOnStart;

    //Runtime vars:

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        spawners.Add(this); //Add this spawners to list of active spawners in scene
    }
    private void Start()
    {
        for (int i = 0; i < spawnOnStart; i++) LocalSpawnPickup();
    }
    private void OnDestroy()
    {
        spawners.Clear();
    }

    //STATIC METHODS:
    /// <summary>
    /// Spawns a pickup in a pickup spawn volume which player is not currently looking at (if possible)
    /// </summary>
    public static void SpawnPickup()
    {
        //Find spawner to use:
        List<PickupSpawner> validSpawners = new List<PickupSpawner>();                                                                              //Container for spawners which may be used to spawn a new pickup
        Vector2 playerPos = new Vector2(PlayerController.main.transform.position.x, PlayerController.main.transform.position.z);                      //Get player position on a 2D plane
        Vector2 playerFacingDirection = new Vector2(PlayerController.main.transform.forward.x, PlayerController.main.transform.forward.z).normalized; //Get direction player is facing on 2D plane
        foreach (PickupSpawner spawner in spawners) //Iterate through list of spawners in scene
        {
            //Check spawner capacity:
            if (spawner.spawnedPickups.Count > spawner.maxPickups) continue; //Skip spawner if it has too many pickups in its area already

            //Check spawner distance:
            Vector2 spawnerPos = new Vector2(spawner.transform.position.x, spawner.transform.position.z); //Get spawner position on a 2D plane
            float spawnerDistance = Vector2.Distance(playerPos, spawnerPos);                              //Find distance between spawner and player
            if (spawnerDistance < minSpawnDistance) continue;                                             //Skip spawner if it is too close to player

            //Check spawner angle:
            if (spawnerDistance < playerViewDistance) //Spawner is not too close to player, but not far enough to guarantee player won't see spawn
            {
                Vector2 spawnerDirection = (spawnerPos - playerPos).normalized;              //Find Vector2 direction from spawner to player
                float spawnerAngle = Vector2.Angle(playerFacingDirection, spawnerDirection); //Get angle between spawner and player facing direction
                if (spawnerAngle < minSpawnAngle) continue;                                  //Skip spawner if it is within player's field of view
            }

            validSpawners.Add(spawner); //Add spawner if it passes checks
        }
        if (validSpawners.Count == 0) return; //Do not attempt to spawn a pickup if there are no valid spawners

        //Spawn pickup:
        validSpawners[Random.Range(0, validSpawners.Count)].LocalSpawnPickup(); //Pick active spawner from list of random spawners
        
    }
    public void LocalSpawnPickup()
    {
        Vector2 relSpawnPoint = Random.insideUnitCircle * spawnRadius;              //Pick spawn point inside spawner's radius
        Vector3 worldSpawnPoint = new Vector3(relSpawnPoint.x, 0, relSpawnPoint.y); //Construct Vector3 version of spawnpoint
        worldSpawnPoint += transform.position;                                      //Move spawnpoint to spawner position

        Ray spawnRay = new Ray(worldSpawnPoint, Vector3.down);  //Create ray which points down from found spawn point
        Physics.Raycast(spawnRay, out RaycastHit spawnSurface); //Raycast to find point in terrain at spawnpoint
        worldSpawnPoint = spawnSurface.point;                   //Get actual world spawn point based on raycast

        GameObject pickup = Instantiate(pickupPrefabs[Random.Range(0, pickupPrefabs.Count)]); //Spawn pickup
        spawnedPickups.Add(pickup);                                                           //Add pickup to spawner list
        pickup.GetComponent<PickupController>().spawner = this;                               //Give pickup a reference to its spawner

        pickup.transform.parent = transform;                                           //Child pickup to spawner
        pickup.transform.position = worldSpawnPoint;                                   //Place pickup at spawn point
        Vector3 rotationRand = pickup.GetComponent<PickupController>().randomRotation; //Get rotation randomness from pickup prefab
        if (rotationRand != Vector3.zero) //Pickup is being given a random rotation modifier
        {
            //Random pickup rotation:
            Vector3 newRotation = pickup.transform.eulerAngles;             //Get initial rotation of pickup on spawn
            newRotation.x += Random.Range(-rotationRand.x, rotationRand.x); //Add random X rotation within range
            newRotation.y += Random.Range(-rotationRand.y, rotationRand.y); //Add random Y rotation within range
            newRotation.z += Random.Range(-rotationRand.z, rotationRand.z); //Add random Z rotation within range
            pickup.transform.eulerAngles = newRotation;                     //Set randomized pickup rotation
        }

        print("Pickup spawned");
    }

    //EDITOR METHODS:
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
    #endif
}
