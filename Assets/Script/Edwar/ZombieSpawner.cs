using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private DungeonGenerator generator;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int roomsToUse = 3;
    [SerializeField] private int minPerRoom = 2;
    [SerializeField] private int maxPerRoom = 3;
    [SerializeField] private float sampleRadius = 4f;

    public void SpawnInOnlyThreeRooms()
    {
        if (generator == null) generator = GetComponent<DungeonGenerator>();
        if (generator == null || zombiePrefab == null) return;

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

            int count = Random.Range(minPerRoom, maxPerRoom + 1);

            for (int j = 0; j < count; j++)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-generator.roomSize * 0.35f, generator.roomSize * 0.35f), 0f,
                                                   Random.Range(-generator.roomSize * 0.35f, generator.roomSize * 0.35f));

                Vector3 target = center + randomOffset;

                if (NavMesh.SamplePosition(target, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                {
                    Instantiate(zombiePrefab, hit.position, Quaternion.identity);
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
