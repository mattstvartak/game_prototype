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
    
    private bool isHandlingRotation;
    private Vector2 mouseLocation;
    private Vector3 motion;
    private CharacterController controller;
    Vector3 velocity = Vector3.zero;

    void Awake()
    {
        input = new PlayerInput();
        controller = GetComponent<CharacterController>();
        isHandlingRotation = false;

        // Walking input
        input.CharacterControls.Movement.performed += ctx => {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
        };

        // Look input
        input.CharacterControls.Look.performed += ctx => {
            mouseLocation = ctx.ReadValue<Vector2>();
            if(!movementPressed) HandleLookRotation();
        };

        // Run toggle
        input.CharacterControls.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

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
        // if(!isHandlingRotation && !movementPressed) {
        //     isHandlingRotation = true;
        //     Invoke("HandleTimedRotation", 2);
        // }
    }

    // void HandleTimedRotation()
    // {
    //     isHandlingRotation = false;
    //    if(mouseLocation == Mouse.current.position.ReadValue()) HandleLookRotation(1f);
    // }

    void HandMovementRotation()
    {
        Vector3 currentPosition = transform.position;

        Vector3 newPosition = new Vector3(currentMovement.x,0,currentMovement.y);

        Vector3 positionToLookAt = Vector3.SmoothDamp(currentPosition, currentPosition + newPosition, ref velocity, 0.25f);

        transform.LookAt(positionToLookAt);
    }

    Vector3 GetMouseWorldLocation()
    {
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

    void HandleLookRotation(float rotationSpeed = 0.25f)
    {
        Vector3 pointToLookAt = GetMouseWorldLocation();
        pointToLookAt = new Vector3(pointToLookAt.x, transform.position.y, pointToLookAt.z);
        transform.position = Vector3.SmoothDamp(transform.position, pointToLookAt, ref velocity, 0.25f);
        // transform.LookAt(pointToLookAt);

        // if (TargetObject != null && Enabled)
        // {
        //     Vector3 targetPosition = pointToLookAt;
        //     transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 3f);
        //     if (RotationSpeed > 0)
        //     {
        //         Quaternion targetRotation = Quaternion.LookRotation(TargetObject.position - myTransform.position);
        //         this.transform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, Time.deltaTime * RotationSpeed);
        //     }
        // }
    }   

    void HandleMovement()
    {
        // Get parameter values from the animator
        bool isRunning = animator.GetBool(isRunningHash);
        bool isWalking = animator.GetBool(isWalkingHash);

        // Walking animation
        if(movementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, value: true);   
        }

        if(!movementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        // Running animation
        if((movementPressed && runPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }

        if((!movementPressed || !runPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }

        // Movement
        if(movementPressed)
        {
            Vector3 controllerInput = new Vector3(currentMovement.x, 0, currentMovement.y);

            motion = controllerInput;
            motion *= isRunning ? runSpeed : walkSpeed;

            controller.Move(motion * Time.deltaTime);
        }
    }

    void OnEnable()
    {
        input.CharacterControls.Enable();
    }

    void onDisable()
    {
        input.CharacterControls.Disable();
    }
}
