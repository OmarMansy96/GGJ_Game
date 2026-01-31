using UnityEngine;

[DisallowMultipleComponent]
public class Room : MonoBehaviour
{
    [Header("Meta")]
    public RoomType roomType = RoomType.Loot;

    [Header("Door references (assign in prefab)")]
    public GameObject doorUp;
    public GameObject doorDown;
    public GameObject doorLeft;
    public GameObject doorRight;

    [HideInInspector]
    public Vector2Int gridPosition;
    [HideInInspector]
    public DungeonGenerator dungeonGenerator;

    // Track which doors are locked
    private bool upLocked = false;
    private bool downLocked = false;
    private bool leftLocked = false;
    private bool rightLocked = false;

    public void SetDoorState(Direction dir, DoorState state)
    {
        GameObject door = GetDoorObject(dir);
        if (door == null) return;

        Transform wall = door.transform.Find("Wall");
        Transform locked = door.transform.Find("Locked");
        Transform Door = door.transform.Find("Door");

        if (state == DoorState.Open)
        {
            if (wall) wall.gameObject.SetActive(true);
            if (locked) locked.gameObject.SetActive(false);
            if (Door) Door.gameObject.SetActive(false);
            SetLockState(dir, false);
        }
        else if (state == DoorState.Wall)
        {
            if (wall) wall.gameObject.SetActive(false);
            if (locked) locked.gameObject.SetActive(true);
            if (Door) Door.gameObject.SetActive(false);
            SetLockState(dir, false);
        }
        else if (state == DoorState.Locked)
        {
            if (wall) wall.gameObject.SetActive(false);
            if (locked) locked.gameObject.SetActive(false);
            if (Door) Door.gameObject.SetActive(true);
            SetLockState(dir, true);
        }
    }

    [ContextMenu("Unlock All Doors")]
    public void UnlockAllDoors()
    {
        UnlockDoor(Direction.Up);
        UnlockDoor(Direction.Down);
        UnlockDoor(Direction.Left);
        UnlockDoor(Direction.Right);
        Debug.Log($"Unlocked all doors in {gameObject.name}");
    }

    public void UnlockDoor(Direction dir)
    {
        if (!IsLocked(dir))
        {
            Debug.Log($"Door {dir} is not locked");
            return;
        }

        GameObject door = GetDoorObject(dir);
        if (door == null)
        {
            Debug.LogWarning($"No door found for direction {dir}");
            return;
        }

        Transform wall = door.transform.Find("Wall");
        Transform locked = door.transform.Find("Locked");
        Transform doorTransform = door.transform.Find("Door");

        // Change from Locked state to Open state
        if (wall) wall.gameObject.SetActive(true);
        if (locked) locked.gameObject.SetActive(false);
        if (doorTransform) doorTransform.gameObject.SetActive(false);

        SetLockState(dir, false);
        Debug.Log($"Unlocked door {dir} in {gameObject.name}");

        // Unlock the corresponding door in the neighboring room
        UnlockNeighborDoor(dir);
    }

    private void UnlockNeighborDoor(Direction dir)
    {
        if (dungeonGenerator == null || dungeonGenerator.spawnedRooms == null)
        {
            Debug.LogWarning("Cannot unlock neighbor door: dungeonGenerator not set");
            return;
        }

        // Get the neighbor's grid position
        Vector2Int offset = dir switch
        {
            Direction.Up => Vector2Int.up,
            Direction.Down => Vector2Int.down,
            Direction.Left => Vector2Int.left,
            Direction.Right => Vector2Int.right,
            _ => Vector2Int.zero
        };

        Vector2Int neighborPos = gridPosition + offset;

        // Find the neighbor room
        if (dungeonGenerator.spawnedRooms.TryGetValue(neighborPos, out GameObject neighborGO))
        {
            Room neighborRoom = neighborGO.GetComponent<Room>();
            if (neighborRoom != null)
            {
                // Get the opposite direction
                Direction oppositeDir = GetOppositeDirection(dir);

                // Unlock the neighbor's door (without recursion)
                GameObject neighborDoor = neighborRoom.GetDoorObject(oppositeDir);
                if (neighborDoor != null)
                {
                    Transform wall = neighborDoor.transform.Find("Wall");
                    Transform locked = neighborDoor.transform.Find("Locked");
                    Transform doorTransform = neighborDoor.transform.Find("Door");

                    if (wall) wall.gameObject.SetActive(true);
                    if (locked) locked.gameObject.SetActive(false);
                    if (doorTransform) doorTransform.gameObject.SetActive(false);

                    neighborRoom.SetLockState(oppositeDir, false);
                    Debug.Log($"Unlocked neighbor's {oppositeDir} door in {neighborGO.name}");
                }
            }
        }
    }

    private Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => dir
        };
    }

    public bool IsLocked(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return upLocked;
            case Direction.Down: return downLocked;
            case Direction.Left: return leftLocked;
            case Direction.Right: return rightLocked;
            default: return false;
        }
    }

    public void SetLockState(Direction dir, bool locked)
    {
        switch (dir)
        {
            case Direction.Up: upLocked = locked; break;
            case Direction.Down: downLocked = locked; break;
            case Direction.Left: leftLocked = locked; break;
            case Direction.Right: rightLocked = locked; break;
        }
    }

    GameObject GetDoorObject(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return doorUp;
            case Direction.Down: return doorDown;
            case Direction.Left: return doorLeft;
            case Direction.Right: return doorRight;
            default: return null;
        }
    }
}