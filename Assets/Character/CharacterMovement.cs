using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    // Variable to store character animator component
    Animator animator;

    // Variables to store optimized getter/setter parameter IDs
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;

    PlayerInput input;

    Vector2 currentMovement;
    bool movementPressed;
    bool runPressed;
    bool jumpPressed;

    public float walkSpeed = 3f;
    public float runSpeed = 5f;

    private Vector3 motion;
    private CharacterController controller;
    private Vector3 velocity = Vector3.zero;

    void Awake() {
        input = new PlayerInput();
        controller = GetComponent<CharacterController>();

        // Walking input
        input.CharacterControls.Movement.performed += ctx => {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
        };

        // Run toggle
        input.CharacterControls.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        // OnGUI();

        // set the ID references
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandMovementRotation();
        if(!movementPressed) HandleLookRotation();
    }

    void HandMovementRotation() {
        Vector3 currentPosition = transform.position;

        Vector3 newPosition = new Vector3(currentMovement.x,0,currentMovement.y);

        Vector3 positionToLookAt = Vector3.SmoothDamp(currentPosition, currentPosition + newPosition, ref velocity, 0.25f);

        transform.LookAt(positionToLookAt);
    }

    Vector3 GetMouseWorldLocation() {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Vector3 worldPosition;


        if(Physics.Raycast(ray, out RaycastHit hitData))
        {
            worldPosition = hitData.point;
            return worldPosition;
        }

        return new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
    }

    void HandleLookRotation() {
        Vector3 pointToLookAt = GetMouseWorldLocation();
        pointToLookAt = new Vector3(pointToLookAt.x, transform.position.y, pointToLookAt.z);
        pointToLookAt = Vector3.SmoothDamp(transform.position, pointToLookAt, ref velocity, 0.5f);
        transform.LookAt(pointToLookAt);
    }

    void HandleMovement() {
        // Get parameter values from the animator
        bool isRunning = animator.GetBool(isRunningHash);
        bool isWalking = animator.GetBool(isWalkingHash);

        // Walking animation
        if(movementPressed && !isWalking) {
            animator.SetBool(isWalkingHash, value: true);
        }

        if(!movementPressed && isWalking) {
            animator.SetBool(isWalkingHash, false);
        }

        // Running animation
        if((movementPressed && runPressed) && !isRunning) {
            animator.SetBool(isRunningHash, true);
        }

        if((!movementPressed || !runPressed) && isRunning) {
            animator.SetBool(isRunningHash, false);
        }

        // Jumping animation
        if(jumpPressed) animator.SetTrigger(isJumpingHash);

        if(!jumpPressed) animator.ResetTrigger(isJumpingHash);

        // Movement
        if(movementPressed) {
            Vector3 controllerInput = new Vector3(currentMovement.x, 0, currentMovement.y);

            motion = controllerInput;
            motion *= isRunning ? runSpeed : walkSpeed;

            controller.Move(motion * Time.deltaTime);
        }
    }

    void OnEnable() {
        input.CharacterControls.Enable();
    }

    void onDisable() {
        input.CharacterControls.Disable();
    }
}
