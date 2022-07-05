using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    // Variable to store character animator component
    Animator animator;

    // Variables to stooree optimized getter/setter parameter IDs
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;

    PlayerInput input;

    Vector2 currentMovement;
    bool movementPressed;
    bool runPressed;
    bool jumpPressed;

    void Awake() {
        input = new PlayerInput();

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

        // set the ID references
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
    }

    // Update is called once per frame
    void Update()
    {
        handleMovement();
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

    }

    void OnEnable() {
        input.CharacterControls.Enable();
    }

    void onDisable() {
        input.CharacterControls.Disable();
    }
}
