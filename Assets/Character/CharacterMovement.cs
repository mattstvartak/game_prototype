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
    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    void Awake() {
        input = new PlayerInput();
        controller = GetComponent<CharacterController>();
        cam = Camera.main;

        // Walking input
        input.CharacterControls.Movement.performed += ctx => {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
        };

        // Run toggle
        input.CharacterControls.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();

        // Jump/Roll
        input.CharacterControls.Jump.performed += ctx => jumpPressed = ctx.ReadValueAsButton();
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
        handleMovement();
        handleRotation();
        // handleLookRotation();
    }

    void handleRotation() {
        float AngleBetweenTwoPoints(Vector3 a, Vector3 b) {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }

        Vector3 currentPosition = transform.position;
        if(!movementPressed) {
            Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);
            Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue());
            float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

            transform.rotation =  Quaternion.Euler(new Vector3(0f,-angle-90,0f));
        }
        
        if(movementPressed) {
            Vector3 newPosition = new Vector3(currentMovement.x,0,currentMovement.y);
            Vector3 positionToLookAt = Vector3.SmoothDamp(currentPosition, currentPosition + newPosition, ref velocity, 0.25f);
            transform.LookAt(positionToLookAt);
        }
    }

    // void handleLookRotation() {
    //     Vector3 mousePos = Mouse.current.position.ReadValue();   
    //     // mousePos.z = Camera.main.nearClipPlane;
    //     // Vector3 Worldpos = Camera.main.ScreenToWorldPoint(mousePos);  
    // }

    void handleMovement() {
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
