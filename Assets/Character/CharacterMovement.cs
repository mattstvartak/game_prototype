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
        input.CharacterControls.Dodge.performed += ctx => jumpPressed = ctx.ReadValueAsButton();
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
        handleMovement();
        handleRotation();
        // handleLookRotation();
    }

    void handleRotation() {
        Vector3 currentPosition = transform.position;

        Vector3 newPosition = new Vector3(currentMovement.x,0,currentMovement.y);

        Vector3 positionToLookAt = Vector3.SmoothDamp(currentPosition, currentPosition + newPosition, ref velocity, 0.25f);

        transform.LookAt(positionToLookAt);
    }
    
     void OnGUI()
    {
        Vector3 point = new Vector3();
        Event   currentEvent = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = currentEvent.mousePosition.x - (cam.pixelWidth/2);
        mousePos.y = cam.pixelHeight - currentEvent.mousePosition.y;

        point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));

        GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        GUILayout.Label("Screen pixels: " + cam.pixelWidth + ":" + cam.pixelHeight);
        GUILayout.Label("Mouse position: " + mousePos);
        GUILayout.Label("World position: " + point.ToString("F3"));
        GUILayout.Label("Mouse X: " + mousePos.x);
        GUILayout.EndArea();
    }
    void handleLookRotation() {
        Vector3 mousePos = Mouse.current.position.ReadValue();   
        // mousePos.z = Camera.main.nearClipPlane;
        // Vector3 Worldpos = Camera.main.ScreenToWorldPoint(mousePos);  
    }

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
