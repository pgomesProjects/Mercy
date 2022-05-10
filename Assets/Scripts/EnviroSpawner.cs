using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class EnviroSpawner : MonoBehaviour
{
    [Header("Settings:")]
    [SerializeField, Tooltip("Radius within which system will spawn environment objects")] private float spawnRadius;
    [SerializeField, Tooltip("Number of environment objects to spawn")]                    private int spawnNumber;
    [SerializeField, Tooltip("")]                                                          private Vector2 randomScaleRange;
    [SerializeField, Tooltip("Prefabs for enviro objects to spawn")]                       private List<GameObject> spawnPrefabs = new List<GameObject>();
    [Space()]
    public bool performSpawn;
    public bool export;

    private void Update()
    {
        if (performSpawn)
        {
            performSpawn = false;
            for (int i = 0; i < spawnNumber; i++)
            {
                Vector2 relSpawnPoint = Random.insideUnitCircle * spawnRadius;
                Vector3 worldSpawnPoint = new Vector3(relSpawnPoint.x, 0, relSpawnPoint.y);
                worldSpawnPoint += transform.position;
                Ray spawnRay = new Ray(worldSpawnPoint, Vector3.down); 
                Physics.Raycast(spawnRay, out RaycastHit spawnSurface);
                worldSpawnPoint = spawnSurface.point; 
                GameObject spawnedObject = Instantiate(spawnPrefabs[Random.Range(0, spawnPrefabs.Count)]);
                spawnedObject.transform.parent = transform;
                spawnedObject.transform.position = worldSpawnPoint;
                
                Vector3 newRotation = spawnedObject.transform.eulerAngles;
                newRotation.y += Random.Range(-180, 180);
                spawnedObject.transform.eulerAngles = newRotation;
                spawnedObject.transform.localScale *= Random.Range(randomScaleRange.x, randomScaleRange.y);
            }
        }

        if (export)
        {
            export = false;
            while (transform.childCount > 0)
            {
                transform.GetChild(0).parent = transform.parent;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
#endif