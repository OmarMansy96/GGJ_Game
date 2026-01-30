// Room.cs
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

    public void SetDoorState(Direction dir, DoorState state)
    {
        GameObject door = GetDoorObject(dir);
        if (door == null) return;

        Transform wall = door.transform.Find("Wall");
        Transform locked = door.transform.Find("Locked");

        if (state == DoorState.Open)
        {
            if (wall) wall.gameObject.SetActive(false);
            if (locked) locked.gameObject.SetActive(false);
        }
        else if (state == DoorState.Wall)
        {
            if (wall) wall.gameObject.SetActive(true);
            if (locked) locked.gameObject.SetActive(false);
        }
        else if (state == DoorState.Locked)
        {
            if (wall) wall.gameObject.SetActive(false);
            if (locked) locked.gameObject.SetActive(true);
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
