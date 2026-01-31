using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MaskSpawner : MonoBehaviour
{
    [SerializeField] private DungeonGenerator generator;
    [SerializeField] private GameObject[] maskPrefabs;

    [Header("Where")]
    [SerializeField] private int roomsToUse = 3;
    [SerializeField] private int masksPerRoom = 1;

    [Header("Placement")]
    [SerializeField] private float sampleRadius = 4f;
    [SerializeField] private float offsetFactor = 0.25f;

    public void SpawnMasks()
    {
        if (generator == null) generator = GetComponent<DungeonGenerator>();
        if (generator == null || maskPrefabs == null || maskPrefabs.Length == 0) return;

        var candidates = new List<GameObject>();

        foreach (var kv in generator.spawnedRooms)
        {
            var room = kv.Value.GetComponent<Room>();
            if (room == null) continue;

            if (room.roomType == RoomType.Spawn || room.roomType == RoomType.Gate)
                continue;

            candidates.Add(kv.Value);
        }

        if (candidates.Count == 0) return;

        Shuffle(candidates);

        int usedRooms = Mathf.Min(roomsToUse, candidates.Count);

        for (int i = 0; i < usedRooms; i++)
        {
            var roomGO = candidates[i];
            Vector3 center = roomGO.transform.position;

            for (int j = 0; j < masksPerRoom; j++)
            {
                float half = generator.roomSize * offsetFactor;

                Vector3 target = center + new Vector3(
                    Random.Range(-half, half),
                    0f,
                    Random.Range(-half, half)
                );

                if (NavMesh.SamplePosition(target, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                {
                    GameObject prefab = maskPrefabs[Random.Range(0, maskPrefabs.Length)];
                    Instantiate(prefab, hit.position, Quaternion.identity);
                }
            }
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
