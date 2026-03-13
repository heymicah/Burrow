using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person rabbit player controller.
///
/// Requirements:
///   - CharacterController component on this GameObject
///   - A Camera tagged "Main Camera" in the scene
///   - Assign the rabbit mesh child Transform to "Visual Root" for squash & stretch
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class RabbitController : MonoBehaviour
{
    // ── Movement ──────────────────────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float walkSpeed     = 5f;
    [SerializeField] private float sprintSpeed   = 9f;
    [SerializeField] private float rotationSpeed = 14f;

    // ── Natural Hops (auto, while moving) ─────────────────────────────────────
    [Header("Natural Hops")]
    [Tooltip("Height of each automatic hop while walking.")]
    [SerializeField] private float hopHeight         = 0.18f;

    [Tooltip("Height of each automatic hop while sprinting.")]
    [SerializeField] private float sprintHopHeight   = 0.28f;

    [Tooltip("Seconds between hops while walking.")]
    [SerializeField] private float hopInterval       = 0.42f;

    [Tooltip("Seconds between hops while sprinting (faster cadence).")]
    [SerializeField] private float sprintHopInterval = 0.28f;

    // ── Big Jump (manual) ─────────────────────────────────────────────────────
    [Header("Big Jump")]
    [Tooltip("Height of the player-triggered jump.")]
    [SerializeField] private float jumpHeight  = 1.8f;

    [Tooltip("Gravity applied per second. Higher = snappier arc.")]
    [SerializeField] private float gravity     = -28f;

    [Tooltip("Grace period (seconds) after walking off a ledge where jumping still works.")]
    [SerializeField] private float coyoteTime  = 0.12f;

    [Tooltip("Seconds before landing where a jump press is remembered.")]
    [SerializeField] private float jumpBuffer  = 0.12f;

    // ── Squash & Stretch ──────────────────────────────────────────────────────
    [Header("Squash & Stretch")]
    [Tooltip("Child Transform holding the mesh (e.g. 'master' bone). Leave empty to skip.")]
    [SerializeField] private Transform visualRoot;

    [SerializeField] private float squashAmount   = 0.28f;
    [SerializeField] private float stretchAmount  = 0.18f;
    [SerializeField] private float squashRecovery = 9f;

    // ── Animator Parameters ───────────────────────────────────────────────────
    [Header("Animator")]
    [SerializeField] private string speedParam      = "Speed";
    [SerializeField] private string runParam        = "Run";
    [SerializeField] private string isGroundedParam = "IsGrounded";

    // ── Private State ─────────────────────────────────────────────────────────
    private CharacterController _cc;
    private Animator            _anim;
    private Camera              _cam;
    private InputSystem_Actions _input;

    private Vector2 _moveInput;
    private Vector3 _velocity;
    private bool    _isGrounded;
    private bool    _wasGrounded;
    private bool    _isSprinting;

    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _hopTimer;           // counts down to next auto-hop

    private int _speedHash;
    private int _runHash;
    private int _isGroundedHash;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        _cc    = GetComponent<CharacterController>();
        _anim  = GetComponentInChildren<Animator>();
        _cam   = Camera.main;
        _input = new InputSystem_Actions();

        if (_anim != null)
            _anim.applyRootMotion = false;

        _speedHash      = Animator.StringToHash(speedParam);
        _runHash        = Animator.StringToHash(runParam);
        _isGroundedHash = Animator.StringToHash(isGroundedParam);
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        _input.Player.Jump.performed -= OnJump;
        _input.Player.Disable();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        _jumpBufferTimer = jumpBuffer;
    }

    private void Update()
    {
        _moveInput   = _input.Player.Move.ReadValue<Vector2>();
        _isSprinting = _input.Player.Sprint.IsPressed();

        GroundCheck();
        ProcessBigJump();
        ProcessHop();

        Vector3 motion = CalcHorizontalMotion();
        motion.y = _velocity.y;
        _cc.Move(motion * Time.deltaTime);

        SquashStretch();
        AnimateRabbit();
    }

    // ── Ground Detection ──────────────────────────────────────────────────────
    private void GroundCheck()
    {
        _wasGrounded = _isGrounded;
        _isGrounded  = _cc.isGrounded;

        if (_isGrounded)
        {
            _coyoteTimer = coyoteTime;
            if (_velocity.y < 0f)
                _velocity.y = -2f;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
            _velocity.y  += gravity * Time.deltaTime;
        }
    }

    // ── Big Jump ──────────────────────────────────────────────────────────────
    private void ProcessBigJump()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            LaunchVertical(jumpHeight);
            _coyoteTimer     = 0f;
            _jumpBufferTimer = 0f;
            _hopTimer        = _isSprinting ? sprintHopInterval : hopInterval; // prevent immediate hop after landing
        }
    }

    // ── Natural Movement Hops ─────────────────────────────────────────────────
    private void ProcessHop()
    {
        bool isMoving = _moveInput.sqrMagnitude > 0.01f;

        if (!isMoving)
        {
            // Reset timer so the next hop fires promptly when movement resumes
            _hopTimer = 0f;
            return;
        }

        _hopTimer -= Time.deltaTime;

        if (_hopTimer <= 0f && _isGrounded)
        {
            float height   = _isSprinting ? sprintHopHeight   : hopHeight;
            float interval = _isSprinting ? sprintHopInterval : hopInterval;

            LaunchVertical(height);
            _hopTimer = interval;
        }
    }

    // ── Shared launch helper ──────────────────────────────────────────────────
    private void LaunchVertical(float height)
    {
        // v = sqrt(2 * |g| * h)
        _velocity.y = Mathf.Sqrt(height * -2f * gravity);
    }

    // ── Horizontal Movement ───────────────────────────────────────────────────
    private Vector3 CalcHorizontalMotion()
    {
        if (_moveInput.sqrMagnitude < 0.01f) return Vector3.zero;

        float speed = _isSprinting ? sprintSpeed : walkSpeed;

        Vector3 camFwd   = _cam.transform.forward;
        Vector3 camRight = _cam.transform.right;
        camFwd.y   = 0f;
        camRight.y = 0f;

        if (camFwd.sqrMagnitude   < 0.01f) camFwd   = Vector3.forward;
        if (camRight.sqrMagnitude < 0.01f) camRight = Vector3.right;

        camFwd.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camFwd * _moveInput.y + camRight * _moveInput.x).normalized;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(moveDir),
            rotationSpeed * Time.deltaTime
        );

        return moveDir * speed;
    }

    // ── Squash & Stretch ──────────────────────────────────────────────────────
    private void SquashStretch()
    {
        if (visualRoot == null) return;

        Vector3 s = visualRoot.localScale;

        if (!_wasGrounded && _isGrounded)
        {
            // Landing squash
            s.y = 1f - squashAmount;
            s.x = 1f + squashAmount * 0.55f;
            s.z = 1f + squashAmount * 0.55f;
        }
        else if (!_isGrounded && _velocity.y > 0f)
        {
            // Rising stretch
            s.y = 1f + stretchAmount;
            s.x = 1f - stretchAmount * 0.4f;
            s.z = 1f - stretchAmount * 0.4f;
        }
        else
        {
            s = Vector3.Lerp(s, Vector3.one, squashRecovery * Time.deltaTime);
        }

        visualRoot.localScale = s;
    }

    // ── Animator ──────────────────────────────────────────────────────────────
    private void AnimateRabbit()
    {
        if (_anim == null) return;

        float horizSpeed = new Vector2(_cc.velocity.x, _cc.velocity.z).magnitude;

        _anim.SetFloat(_speedHash, horizSpeed);
        _anim.SetBool(_runHash, horizSpeed > 0.1f);
        _anim.SetBool(_isGroundedHash, _isGrounded);
    }
}
