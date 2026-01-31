using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMap : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public Transform roomsRoot;
    public Camera mapCamera;

    [Header("Input")]
    public float holdTime = 0.4f; // tap vs hold threshold

    private Dictionary<Transform, bool> roomDiscovered = new();
    private float tabDownTime;
    private bool mapVisible;

    void Start()
    {
        InitRooms();
        CenterAndFitMap();
        HideMap();
    }

    void Update()
    {
        HandleInput();
        RevealRoomUnderPlayer();
    }

    // ---------------- INIT ----------------

    void InitRooms()
    {
        roomDiscovered.Clear();

        foreach (Transform room in roomsRoot)
        {
            room.gameObject.SetActive(false); // fog of war
            roomDiscovered.Add(room, false);
        }
    }

    // ---------------- INPUT ----------------

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            tabDownTime = Time.time;
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            if (Time.time - tabDownTime >= holdTime)
            {
                ShowFullMap();
            }
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (Time.time - tabDownTime < holdTime)
            {
                ToggleLocalMap();
            }
            else
            {
                HideMap();
            }
        }
    }

    // ---------------- MAP MODES ----------------

    void ToggleLocalMap()
    {
        mapVisible = !mapVisible;

        if (!mapVisible)
        {
            HideMap();
            return;
        }

        foreach (var kvp in roomDiscovered)
            kvp.Key.gameObject.SetActive(kvp.Value);

        mapCamera.gameObject.SetActive(true);
    }

    void ShowFullMap()
    {
        mapCamera.gameObject.SetActive(true);

        foreach (var kvp in roomDiscovered)
        {
            if (kvp.Value)
                kvp.Key.gameObject.SetActive(true);
        }
    }

    void HideMap()
    {
        mapVisible = false;
        mapCamera.gameObject.SetActive(false);
    }

    // ---------------- FOG OF WAR ----------------

    void RevealRoomUnderPlayer()
    {
        foreach (var kvp in roomDiscovered)
        {
            if (kvp.Value) continue;

            if (Vector3.Distance(player.position, kvp.Key.position) < 1.5f)
            {
                kvp.Key.gameObject.SetActive(true);
                roomDiscovered[kvp.Key] = true;
            }
        }
    }

    // ---------------- AUTO CENTER & FIT ----------------

    void CenterAndFitMap()
    {
        Bounds bounds = new Bounds(roomsRoot.GetChild(0).position, Vector3.zero);

        foreach (Transform room in roomsRoot)
            bounds.Encapsulate(room.position);

        mapCamera.transform.position =
            new Vector3(bounds.center.x, mapCamera.transform.position.y, bounds.center.z);

        mapCamera.orthographicSize =
            Mathf.Max(bounds.size.x, bounds.size.z) * 0.6f;
    }
}
