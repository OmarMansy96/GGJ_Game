using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public InputActionAsset playerController;


    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction sprintAction;



    public Vector2 moveInputValue {  get; private set; }
    public Vector2 lookInputValue { get; private set; }
    public bool jumpInputTrigerd { get; private set; }
    public float sprintInputTrigerd { get; private set; }

    private void Awake()
    {
        moveAction = playerController.FindActionMap("Player").FindAction("Move");
        lookAction = playerController.FindActionMap("Player").FindAction("Look");
        jumpAction = playerController.FindActionMap("Player").FindAction("Jump");
        sprintAction = playerController.FindActionMap("Player").FindAction("Sprint");
        RegisterInputActions();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();

    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();

    }

    public void RegisterInputActions()
    {
        moveAction.performed += context => moveInputValue = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInputValue = Vector2.zero;

        lookAction.performed += context => lookInputValue = context.ReadValue<Vector2>();
        lookAction.canceled += context =>  lookInputValue = Vector2.zero;

        jumpAction.performed += context => jumpInputTrigerd = true;
        jumpAction.canceled += context => jumpInputTrigerd = false;

        sprintAction.performed += context => sprintInputTrigerd = context.ReadValue<float>();
        sprintAction.canceled += context => sprintInputTrigerd = 0;
    }

}
