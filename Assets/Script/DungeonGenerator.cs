using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public enum RoomType { Spawn, Loot, Key, Locked, Gate, Trap, Empty }
public enum Direction { Up, Down, Left, Right }
public enum DoorState { Wall, Open, Locked }

public class DungeonGenerator : MonoBehaviour
{

    [SerializeField] private NavMeshSurface surface;
    public int gridWidth = 25, gridHeight = 25;
    public Vector2Int startPos = new Vector2Int(12, 12); // Center of default 25x25 grid
    public int maxRooms = 40, roomSize = 10;
    public GameObject[] roomPrefabs = new GameObject[7]; // Changed from 6 to 7 to include Empty room
    public bool useSeed = false;
    public int seed = -1;

    [Header("Room Type Ratios (out of 100)")]
    [Tooltip("Spawn is always 1, Gate is always 1")]
    public int emptyRoomPercent = 32;  // 16/50 = 32%
    public int lootRoomPercent = 40;   // 20/50 = 40%
    public int trapRoomPercent = 8;    // 4/50 = 8%
    public int keyLockedPairPercent = 16; // 8/50 = 16% (4 keys + 4 locked)

    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
    public List<Vector2Int> visitOrder = new List<Vector2Int>();
    public Dictionary<Vector2Int, GameObject> spawnedRooms = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        // only generate in play mode
        if (Application.isPlaying)
            Generate();
    }

    //[ContextMenu("Generate Dungeon (Editor generation cuz my dumbass is dum)")]
    public void Generate()
    {
        if (surface == null) surface = GetComponent<NavMeshSurface>();
        // cleanup old rooms
        Cleanup();

        if (roomPrefabs == null || roomPrefabs.Length < 7) // Changed from 6 to 7
            Debug.LogWarning("Prefabs missing");

        Random.InitState(useSeed && seed >= 0 ? seed : System.Environment.TickCount);

        occupied.Clear();
        visitOrder.Clear();
        spawnedRooms.Clear();

        startPos.x = Mathf.Clamp(startPos.x, 0, gridWidth - 1);
        startPos.y = Mathf.Clamp(startPos.y, 0, gridHeight - 1);

        RandomWalk();
        RoomType[] assigned = AssignRoomTypes();

        for (int i = 0; i < visitOrder.Count; i++)
        {
            Vector2Int cell = visitOrder[i];
            RoomType t = assigned[i];
            GameObject prefab = GetPrefabForRoomType(t);
            if (prefab == null) continue;

            GameObject inst = Instantiate(prefab, CellToWorld(cell), Quaternion.identity, transform);
            Room roomComp = inst.GetComponent<Room>();
            if (roomComp != null)
            {
                roomComp.roomType = t;
                roomComp.gridPosition = cell;
                roomComp.dungeonGenerator = this;
            }
            spawnedRooms[cell] = inst;
        }

        var snapshot = new Dictionary<Vector2Int, GameObject>(spawnedRooms);
        foreach (var kv in snapshot)
        {
            Room room = kv.Value.GetComponent<Room>();
            if (room == null) continue;

            TrySetDoorPair(kv.Key, Vector2Int.up, Direction.Up, room);
            TrySetDoorPair(kv.Key, Vector2Int.down, Direction.Down, room);
            TrySetDoorPair(kv.Key, Vector2Int.left, Direction.Left, room);
            TrySetDoorPair(kv.Key, Vector2Int.right, Direction.Right, room);
        }
        if (surface != null) surface.BuildNavMesh();
        GetComponent<ZombieSpawner>().SpawnInOnlyThreeRooms();
        GetComponent<MaskSpawner>()?.SpawnMasks();
    }

    void Cleanup()
    {
        // destroy previously spawned rooms
        var children = new List<Transform>();
        foreach (Transform t in transform) children.Add(t);
        foreach (Transform t in children) DestroyImmediate(t.gameObject);
    }

    GameObject GetPrefabForRoomType(RoomType type)
    {
        return type switch
        {
            RoomType.Spawn => roomPrefabs[0],
            RoomType.Loot => roomPrefabs[1],
            RoomType.Key => roomPrefabs[2],
            RoomType.Locked => roomPrefabs[3],
            RoomType.Gate => roomPrefabs[4],
            RoomType.Trap => roomPrefabs[5],
            RoomType.Empty => roomPrefabs[6],
            _ => null
        };
    }

    void RandomWalk()
    {
        Vector2Int pos = startPos;
        occupied.Add(pos);
        visitOrder.Add(pos);

        int attempts = 0;
        int stuckCounter = 0;
        Vector2Int lastPos = pos;

        while (visitOrder.Count < maxRooms && attempts < maxRooms * 20)
        {
            Vector2Int nxt = pos + RandomDirection();

            // Check if next position is valid
            if (nxt.x >= 0 && nxt.x < gridWidth && nxt.y >= 0 && nxt.y < gridHeight)
            {
                if (!occupied.Contains(nxt))
                {
                    // Found a new position!
                    occupied.Add(nxt);
                    visitOrder.Add(nxt);
                    pos = nxt;
                    stuckCounter = 0;
                }
                else
                {
                    // Position already occupied, randomly move to it anyway (but don't add to visitOrder)
                    if (Random.value < 0.3f)
                    {
                        pos = nxt;
                    }
                    stuckCounter++;
                }
            }
            else
            {
                stuckCounter++;
            }

            // If we're stuck at a boundary or surrounded, jump to a random occupied position
            if (stuckCounter > 20 && visitOrder.Count > 1)
            {
                pos = visitOrder[Random.Range(0, visitOrder.Count)];
                stuckCounter = 0;
            }

            attempts++;
        }

        Debug.Log($"Generated {visitOrder.Count} unique rooms out of {maxRooms} requested");
    }

    RoomType[] AssignRoomTypes()
    {
        int n = visitOrder.Count;
        if (n == 0) return new RoomType[0];

        RoomType[] types = new RoomType[n];
        bool[] assigned = new bool[n];

        // Find spawn room - closest to startPos (should be at edge/beginning)
        int spawnIndex = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < visitOrder.Count; i++)
        {
            float dist = Vector2Int.Distance(visitOrder[i], startPos);
            if (dist < minDist)
            {
                minDist = dist;
                spawnIndex = i;
            }
        }
        types[spawnIndex] = RoomType.Spawn;
        assigned[spawnIndex] = true;

        // Find gate room - furthest from spawn room (should be at opposite edge/end)
        int gateIndex = 0;
        float maxDist = float.MinValue;
        Vector2Int spawnPos = visitOrder[spawnIndex];
        for (int i = 0; i < visitOrder.Count; i++)
        {
            if (i == spawnIndex) continue;
            float dist = Vector2Int.Distance(visitOrder[i], spawnPos);
            if (dist > maxDist)
            {
                maxDist = dist;
                gateIndex = i;
            }
        }
        types[gateIndex] = RoomType.Gate;
        assigned[gateIndex] = true;

        // Calculate room counts based on percentages
        // Spawn and Gate are already assigned (2 rooms used)
        int remainingRooms = n - 2;

        int keyLockedPairs = Mathf.RoundToInt((keyLockedPairPercent / 100f) * n / 2); // Divide by 2 because each pair = 2 rooms
        int trapCount = Mathf.RoundToInt((trapRoomPercent / 100f) * n);
        int emptyCount = Mathf.RoundToInt((emptyRoomPercent / 100f) * n);
        int lootCount = Mathf.RoundToInt((lootRoomPercent / 100f) * n);

        // Ensure we don't exceed available rooms
        int specialRoomsTotal = (keyLockedPairs * 2) + trapCount + emptyCount + lootCount;
        if (specialRoomsTotal > remainingRooms)
        {
            // Scale down proportionally if we have too many
            float scale = (float)remainingRooms / specialRoomsTotal;
            keyLockedPairs = Mathf.Max(1, Mathf.RoundToInt(keyLockedPairs * scale));
            trapCount = Mathf.RoundToInt(trapCount * scale);
            emptyCount = Mathf.RoundToInt(emptyCount * scale);
            lootCount = Mathf.RoundToInt(lootCount * scale);
        }

        List<int> available = new List<int>();
        for (int i = 0; i < n; i++)
            if (!assigned[i])
                available.Add(i);
        Shuffle(available);

        // Assign Key-Locked pairs
        int pairsAssigned = 0;
        for (int i = 0; i < keyLockedPairs && available.Count >= 2; i++)
        {
            int keyIndex = available[0];
            int lockedIndex = available[1];
            available.RemoveRange(0, 2);

            types[keyIndex] = RoomType.Key;
            assigned[keyIndex] = true;
            types[lockedIndex] = RoomType.Locked;
            assigned[lockedIndex] = true;
            pairsAssigned++;
        }

        // Assign Trap rooms
        int trapsAssigned = 0;
        for (int i = 0; i < trapCount && available.Count > 0; i++)
        {
            int idx = available[0];
            available.RemoveAt(0);
            types[idx] = RoomType.Trap;
            assigned[idx] = true;
            trapsAssigned++;
        }

        // Assign Empty rooms
        int emptiesAssigned = 0;
        for (int i = 0; i < emptyCount && available.Count > 0; i++)
        {
            int idx = available[0];
            available.RemoveAt(0);
            types[idx] = RoomType.Empty;
            assigned[idx] = true;
            emptiesAssigned++;
        }

        // Assign Loot rooms
        int lootsAssigned = 0;
        for (int i = 0; i < lootCount && available.Count > 0; i++)
        {
            int idx = available[0];
            available.RemoveAt(0);
            types[idx] = RoomType.Loot;
            assigned[idx] = true;
            lootsAssigned++;
        }

        for (int i = 0; i < n; i++)
        {
            if (!assigned[i])
            {
                types[i] = RoomType.Loot;
                assigned[i] = true;
            }
        }

        // Debug: Log room type distribution
        int[] counts = new int[7];
        foreach (var t in types) counts[(int)t]++;
        Debug.Log($"Room distribution ({n} total): Spawn={counts[0]}, Loot={counts[1]}, Key={counts[2]}, Locked={counts[3]}, Gate={counts[4]}, Trap={counts[5]}, Empty={counts[6]}");
        Debug.Log($"Percentages: Empty={emptyRoomPercent}%, Loot={lootRoomPercent}%, Trap={trapRoomPercent}%, KeyLocked={keyLockedPairPercent}%");

        return types;
    }

    void TrySetDoorPair(Vector2Int cell, Vector2Int offset, Direction dir, Room room)
    {
        Vector2Int neighborPos = cell + offset;
        if (!spawnedRooms.TryGetValue(neighborPos, out GameObject neighborGO))
        {
            room.SetDoorState(dir, DoorState.Wall);
            return;
        }

        Room neighborRoom = neighborGO.GetComponent<Room>();
        if (neighborRoom == null)
        {
            room.SetDoorState(dir, DoorState.Wall);
            return;
        }

        DoorState state = (room.roomType == RoomType.Locked || neighborRoom.roomType == RoomType.Locked)
            ? DoorState.Locked : DoorState.Open;

        room.SetDoorState(dir, state);
        neighborRoom.SetDoorState(Opposite(dir), state);
    }

    Direction Opposite(Direction d) => d switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        _ => d
    };

    Vector3 CellToWorld(Vector2Int cell) =>
        new Vector3(cell.x * roomSize, 0, cell.y * roomSize);
    Vector2Int RandomDirection() =>
        Random.Range(0, 4) switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.down,
            2 => Vector2Int.left,
            _ => Vector2Int.right
        };

    Vector2Int GetFurthestInRandomCardinal()
    {
        if (visitOrder.Count == 0) return startPos;
        int choice = Random.Range(0, 4);
        Vector2Int best = startPos;
        float bestVal = float.NegativeInfinity;

        foreach (var p in visitOrder)
        {
            float val = choice switch
            {
                0 => p.y - startPos.y,
                1 => startPos.y - p.y,
                2 => p.x - startPos.x,
                3 => startPos.x - p.x,
                _ => 0
            };
            if (val > bestVal)
            {
                bestVal = val;
                best = p;
            }
        }
        return best;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var p in visitOrder) Gizmos.DrawWireCube(CellToWorld(p), Vector3.one * roomSize * 0.8f);
    }
}