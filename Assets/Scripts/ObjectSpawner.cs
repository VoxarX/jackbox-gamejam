using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public int amount = 1;
    public Transform spawnPoint;

    [Header("Optional Spread")]
    [Tooltip("Random offset radius so spawned objects don't all stack on top of each other.")]
    public float spawnRadius = 0.5f;

    public void Spawn()
    {
        if (prefabToSpawn == null || spawnPoint == null)
        {
            Debug.LogWarning("ObjectSpawner: missing prefab or spawn point.", this);
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = spawnRadius > 0f
                ? Random.insideUnitSphere * spawnRadius
                : Vector3.zero;

            // Keep offset roughly on the spawn point's local "surface plane" if it's on a planet
            offset = Vector3.ProjectOnPlane(offset, spawnPoint.up);

            Vector3 spawnPos = spawnPoint.position + offset;
            Quaternion spawnRot = spawnPoint.rotation;

            Instantiate(prefabToSpawn, spawnPos, spawnRot);
        }
    }
}