using System.Collections.Generic;
using UnityEngine;

public enum RoomType { Spawn, Loot, Key, Locked, Gate, Trap }
public enum Direction { Up, Down, Left, Right }
public enum DoorState { Wall, Open, Locked }

public class DungeonGenerator : MonoBehaviour
{
    public int gridWidth = 25, gridHeight = 25;
    public Vector2Int startPos = new Vector2Int(12, 12);
    public int maxRooms = 40, roomSize = 10;
    public GameObject[] roomPrefabs = new GameObject[6];
    public bool useSeed = false;
    public int seed = -1;

    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
    private List<Vector2Int> visitOrder = new List<Vector2Int>();
    private Dictionary<Vector2Int, GameObject> spawnedRooms = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        // only generate in play mode
        if (Application.isPlaying)
            Generate();
    }

    //[ContextMenu("Generate Dungeon (Editor generation cuz my dumbass is dum)")]
    public void Generate()
    {
        // cleanup old rooms
        Cleanup();

        if (roomPrefabs == null || roomPrefabs.Length < 6)
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
            if (roomComp != null) roomComp.roomType = t;
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
            _ => null
        };
    }

    void RandomWalk()
    {
        Vector2Int pos = startPos;
        occupied.Add(pos);
        visitOrder.Add(pos);

        int attempts = 0;
        while (visitOrder.Count < maxRooms && attempts < maxRooms * 10)
        {
            Vector2Int nxt = pos + RandomDirection();
            if (nxt.x >= 0 && nxt.x < gridWidth && nxt.y >= 0 && nxt.y < gridHeight)
            {
                if (!occupied.Contains(nxt))
                {
                    occupied.Add(nxt);
                    visitOrder.Add(nxt);
                    pos = nxt;
                }
                else if (Random.value < 0.3f) pos = nxt;
            }
            attempts++;
        }
    }

    RoomType[] AssignRoomTypes()
    {
        int n = visitOrder.Count;
        RoomType[] types = new RoomType[n];
        for (int i = 0; i < n; i++) types[i] = RoomType.Loot;
        if (n == 0) return types;

        types[0] = RoomType.Spawn;
        int gateIndex = Mathf.Clamp(visitOrder.IndexOf(GetFurthestInRandomCardinal()), 0, n - 1);
        if (gateIndex <= 0) gateIndex = n - 1;
        types[gateIndex] = RoomType.Gate;

        int pairCount = Mathf.Clamp(n / 12, 1, 4);
        List<int> available = new List<int>();
        for (int i = 1; i < n; i++) if (i != gateIndex) available.Add(i);
        Shuffle(available);

        for (int i = 0; i < pairCount && available.Count >= 2; i++)
        {
            int a = available[0], b = available[1];
            available.RemoveRange(0, 2);
            int keyIndex = Mathf.Min(a, b);
            int lockIndex = Mathf.Max(a, b);
            if (keyIndex == 0 || keyIndex == gateIndex || lockIndex == 0 || lockIndex == gateIndex) continue;

            types[keyIndex] = RoomType.Key;
            types[lockIndex] = RoomType.Locked;
        }

        List<int> traps = new List<int>();
        for (int i = 1; i < n; i++) if (types[i] == RoomType.Loot) traps.Add(i);
        Shuffle(traps);
        int trapCount = Mathf.Clamp(n / 8, 1, 6);
        for (int i = 0; i < trapCount && i < traps.Count; i++) types[traps[i]] = RoomType.Trap;

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
        new Vector3((cell.x - gridWidth / 2f) * roomSize, 0, (cell.y - gridHeight / 2f) * roomSize);

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
