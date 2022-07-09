using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Player Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    // Controls
    private PlayerInput _input;
    private bool _movePressed;
    private bool _runPressed;
    private bool _jumpPressed;
    private bool _mouseLookPressed;

    // Camera
    private Vector2 _mouseLookDelta;
    private Vector2 mouseLocation;

    // Player
    private float _rotationVelocity;
    private Vector3 _move;
    private CharacterController _player;
    private float _targetCameraRotation = 0.0f;
    private float _targetRotation = 0.0f;
    private Vector3 _velocity = Vector3.zero;
    private Vector2 _inputDirection;

    // Animation
    private Animator _animator;
    private int _isWalkingHash;
    private int _isRunningHash;
    private int _isJumpingHash;

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
            _inputDirection = ctx.ReadValue<Vector2>();
            _movePressed = _inputDirection.x != 0 || _inputDirection.y != 0;
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
        _targetRotation = Mathf.Atan2(_inputDirection.x, _inputDirection.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        if(_movePressed)
        {
            _move = targetDirection.normalized;
            _move *= isRunning ? runSpeed : walkSpeed;
            _player.Move(_move * Time.deltaTime);
        }
    }

    void HandleCameraZoom()
    {
        
    }

    void HandleRotation()
    {
        if(_movePressed) {
            _targetCameraRotation = Mathf.Atan2(_inputDirection.x, _inputDirection.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetCameraRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

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
