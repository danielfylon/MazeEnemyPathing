
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonSpawnTrigger : MonoBehaviour
{
    [Header("Skeleton")]
    public GameObject skeletonPrefab;
    public Transform spawnPoint;

    [Header("Spawn Delay")]
    public float spawnDelay = 15f;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Only activate when the player enters
        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null || triggered)
            return;

        triggered = true;

        StartCoroutine(SpawnAfterDelay(player.transform));
    }

    IEnumerator SpawnAfterDelay(Transform player)
    {
        // Wait before spawning
        yield return new WaitForSeconds(spawnDelay);

        if (skeletonPrefab == null || spawnPoint == null)
        {
            Debug.LogError(
                "Skeleton prefab or spawn point missing!"
            );
            yield break;
        }

        // Make sure spawn location is walkable
        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            spawnPoint.position,
            out hit,
            1.5f,
            NavMesh.AllAreas))
        {
            Debug.LogError(
                "Spawn point is not near a NavMesh!"
            );
            yield break;
        }

        // Spawn skeleton
        GameObject skeleton = Instantiate(
            skeletonPrefab,
            hit.position,
            spawnPoint.rotation
        );

        // Give skeleton its player target
        SkeletonMovement chase =
            skeleton.GetComponent<SkeletonMovement>();

        if (chase != null)
        {
            chase.SetPlayer(player);
        }

        Debug.Log("Skeleton spawned!");
    }
}
