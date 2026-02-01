using UnityEngine;
using TMPro;

public class KeyCollect : MonoBehaviour
{
    [Header("Key Collection")]
    public int keyCount = 0;

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public LayerMask doorLayer; // Assign layer for doors

    [Header("UI")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI keyCountText; // NEW: Display key count
    public string unlockPrompt = "Press E to Unlock Door";
    public string noKeyPrompt = "Locked - Need Key";
    public string collectKeyPrompt = "Press E to Collect Key";

    [Header("References")]
    public Camera playerCamera;

    private Room currentLookedAtRoom;
    private GameObject currentLookedAtKey;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        CheckForInteractables();
        HandleInput();
        UpdateKeyCountDisplay();
    }

    void UpdateKeyCountDisplay()
    {
        if (keyCountText != null)
        {
            keyCountText.text = $"x {keyCount}";
        }
    }

    void CheckForInteractables()
    {
        // Reset current targets
        currentLookedAtRoom = null;
        currentLookedAtKey = null;

        // Raycast from camera center
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            // Check if looking at a key
            if (hit.collider.CompareTag("Key"))
            {
                currentLookedAtKey = hit.collider.gameObject;
                ShowPrompt(collectKeyPrompt);
                return;
            }

            // Check if looking at any room
            Room room = hit.collider.GetComponentInParent<Room>();
            if (room != null)
            {
                // ONLY show prompt if room type is LOCKED
                if (room.roomType == RoomType.Locked)
                {
                    // Check if looking at any door of this locked room
                    Direction? doorDirection = GetLookedAtDoor(hit.collider.gameObject, room);

                    if (doorDirection.HasValue)
                    {
                        currentLookedAtRoom = room;

                        if (keyCount > 0)
                            ShowPrompt(unlockPrompt);
                        else
                            ShowPrompt(noKeyPrompt);

                        return;
                    }
                }
            }
        }

        // Nothing interactable in range - HIDE PROMPT
        HidePrompt();
    }

    Direction? GetLookedAtDoor(GameObject hitObject, Room room)
    {
        // Check which door object was hit
        if (room.doorUp != null && IsChildOf(hitObject, room.doorUp))
            return Direction.Up;
        if (room.doorDown != null && IsChildOf(hitObject, room.doorDown))
            return Direction.Down;
        if (room.doorLeft != null && IsChildOf(hitObject, room.doorLeft))
            return Direction.Left;
        if (room.doorRight != null && IsChildOf(hitObject, room.doorRight))
            return Direction.Right;

        return null;
    }

    bool IsChildOf(GameObject child, GameObject parent)
    {
        Transform current = child.transform;
        while (current != null)
        {
            if (current.gameObject == parent)
                return true;
            current = current.parent;
        }
        return false;
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            // Try to collect key
            if (currentLookedAtKey != null)
            {
                CollectKey(currentLookedAtKey);
            }
            // Try to unlock door
            else if (currentLookedAtRoom != null && keyCount > 0)
            {
                UnlockDoor();
            }
        }
    }

    void CollectKey(GameObject keyObject)
    {
        keyCount++;
        Debug.Log($"Collected key! Total keys: {keyCount}");

        // Destroy the key object
        Destroy(keyObject);

        // Optional: Play sound effect here
        HidePrompt();
    }

    void UnlockDoor()
    {
        if (currentLookedAtRoom == null || keyCount <= 0)
            return;

        // Check if this room is a Locked room type
        if (currentLookedAtRoom.roomType != RoomType.Locked)
        {
            Debug.Log("This room is not a locked room type!");
            return;
        }

        Debug.Log($"Unlocking room: {currentLookedAtRoom.gameObject.name}, Type BEFORE: {currentLookedAtRoom.roomType}");

        // Unlock ALL doors in this room
        currentLookedAtRoom.UnlockAllDoors();

        // IMPORTANT: Change room type so it stops showing as locked
        currentLookedAtRoom.roomType = RoomType.Empty;

        Debug.Log($"Room type AFTER unlock: {currentLookedAtRoom.roomType}");

        // Use a key
        keyCount--;
        Debug.Log($"Unlocked entire room! Keys remaining: {keyCount}");

        // Optional: Play unlock sound here

        // Force hide prompt
        HidePrompt();
        currentLookedAtRoom = null;
    }

    void ShowPrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }
    }

    void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    // Public method to add keys from other scripts
    public void AddKey(int amount = 1)
    {
        keyCount += amount;
        Debug.Log($"Added {amount} key(s)! Total: {keyCount}");
    }

    // Public method to check if player has keys
    public bool HasKey()
    {
        return keyCount > 0;
    }

    void OnDrawGizmos()
    {
        // Draw interaction range in scene view
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionRange);
        }
    }
}