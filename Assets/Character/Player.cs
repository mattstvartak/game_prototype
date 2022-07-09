using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameObject CinemachineCameraTarget;
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    
    private int _isWalkingHash;
    private int _isRunningHash;
    private int _isJumpingHash;
    private PlayerInput _input;
    private Vector2 mouseLocation;
    private Vector3 motion;
    private CharacterController _player;
    private Vector3 _velocity = Vector3.zero;
    private Vector2 _move;
    private bool _movePressed;
    private bool _runPressed;
    private bool _jumpPressed;
    private bool _mouseLookPressed;
    private bool _canMoveCamera;
    private Vector2 _mouseLookDelta;
    Animator _animator;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;

    void Awake()
    {
        _input = new PlayerInput();
        _player = GetComponent<CharacterController>();

        // Walking input
        _input.Player.Move.performed += ctx => 
        {
            _move = ctx.ReadValue<Vector2>();
            _movePressed = _move.x != 0 || _move.y != 0;
        };

        // Look input
        _input.Player.Look.performed += ctx => 
        {
            mouseLocation = ctx.ReadValue<Vector2>();
            // Look at mouse when not moving
            if(!_movePressed && !_mouseLookPressed) HandleLookRotation();
        };

        // Mouse Look Toggle
        _input.Player.MouseLook.performed += ctx => _mouseLookPressed = ctx.ReadValueAsButton();

        _input.Player.MouseDelta.performed += ctx => _mouseLookDelta = ctx.ReadValue<Vector2>();

        // Run toggle
        _input.Player.Run.performed += ctx => _runPressed = ctx.ReadValueAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _animator = GetComponent<Animator>();

        // set the ID references
        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
    }   

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleAnimation();
    }
    private void LateUpdate()
    {
        HandleCameraRotation();
    }
     void HandleMovement()
    {
        // Get parameter values from the animator
        bool isRunning = _animator.GetBool(_isRunningHash);
        bool isWalking = _animator.GetBool(_isWalkingHash);

        // Movement
        if(_movePressed)
        {
            Vector3 controllerInput = new Vector3(_move.x, 0, _move.y);

            motion = controllerInput;
            motion *= isRunning ? runSpeed : walkSpeed;

            _player.Move(motion * Time.deltaTime);
        }
    }
    void HandleRotation()
    {
        Vector3 currentPosition = transform.position;
        Vector3 newPosition = new Vector3(_move.x,0,_move.y);
        Vector3 positionToLookAt = Vector3.SmoothDamp(currentPosition, currentPosition + newPosition, ref _velocity, 0.25f);

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

    void HandleLookRotation()
    {
        Vector3 pointToLookAt = GetMouseWorldLocation();
        pointToLookAt = new Vector3(pointToLookAt.x, transform.position.y, pointToLookAt.z);
        transform.LookAt(pointToLookAt);
    }   

    void HandleCameraRotation()
    {
        // If look button is held down allow mouse/stick look.
        if(_mouseLookPressed && _mouseLookDelta.sqrMagnitude >= _threshold) {
            float deltaTimeMultiplier = 1.0f;

            _cinemachineTargetYaw += _mouseLookDelta.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _mouseLookDelta.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        Debug.Log(_mouseLookDelta.y);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    void HandleAnimation() 
    {
        // Get parameter values from the animator
        bool isRunning = _animator.GetBool(_isRunningHash);
        bool isWalking = _animator.GetBool(_isWalkingHash);

        // Walking animation
        if(_movePressed && !isWalking)
        {
            _animator.SetBool(_isWalkingHash, value: true);   
        }

        if(!_movePressed && isWalking)
        {
            _animator.SetBool(_isWalkingHash, false);
        }

        // Running animation
        if((_movePressed && _runPressed) && !isRunning)
        {
            _animator.SetBool(_isRunningHash, true);
        }

        if((!_movePressed || !_runPressed) && isRunning)
        {
            _animator.SetBool(_isRunningHash, false);
        }
    }

    void OnEnable()
    {
        _input.Player.Enable();
    }

    void onDisable()
    {
        _input.Player.Disable();
    }
}
