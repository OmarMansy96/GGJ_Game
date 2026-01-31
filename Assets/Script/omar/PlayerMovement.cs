
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 3f;
    public float sprintMultiplayer = 3f;
    public float rotationSpeed=5f;
    public float jumpForce = 5;
    public float gravity = 9.8f;

    [SerializeField] private Mask mask;

    public PlayerInputManager inputManager;
    CharacterController controller;

    Vector3 currentMovement;
    Vector3 currentRotation;



    private Health health;
  
    private void Awake()
    {
        if (!health) health = GetComponent<Health>();
        
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible= false;
    }


    private void Update()
    {
        Move();
        Rotate();

    }

    void Move()
    {
        float speed = movementSpeed * (inputManager.sprintInputTrigerd > 0 ? sprintMultiplayer : 1);
    
        Vector3 inputDirection = new Vector3(inputManager.moveInputValue.x, 0, inputManager.moveInputValue.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        worldDirection.Normalize();
        currentMovement.x = worldDirection.x * speed;
        currentMovement.z = worldDirection.z * speed; 

        Jump();
        controller.Move(currentMovement * Time.deltaTime);
    }

    void Rotate()
    {
        currentRotation += new Vector3(-inputManager.lookInputValue.y, inputManager.lookInputValue.x, 0) * rotationSpeed * Time.deltaTime;

        currentRotation.x = Mathf.Clamp(currentRotation.x, -40, 25);
        transform.rotation= Quaternion.Euler(currentRotation );
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            if (inputManager.jumpInputTrigerd)
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y -= gravity * Time.deltaTime;
        }
    }





    public void Hit(int damage)
    {
        int leftover = damage;

        if (mask != null && mask.gameObject.activeSelf)
            mask.TakeDamage(ref leftover);

        if (leftover > 0)
            health.TakeDamage(leftover);
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Mask"))
        {
            mask.Equip();
            Destroy(collision.gameObject);
            Debug.Log("Mask Equipped"); 
        }
    }
}





