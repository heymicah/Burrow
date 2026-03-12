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
    // ── Movement ─────────────────────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float walkSpeed     = 5f;
    [SerializeField] private float sprintSpeed   = 10f;
    [SerializeField] private float rotationSpeed = 14f;

    // ── Bounce / Jump ─────────────────────────────────────────────────────────
    [Header("Bounce")]
    [Tooltip("How high the rabbit bounces in world units.")]
    [SerializeField] private float jumpHeight  = 1.8f;

    [Tooltip("Gravity applied per second. Higher = snappier arc.")]
    [SerializeField] private float gravity     = -28f;

    [Tooltip("Grace period (seconds) after walking off a ledge where jumping still works.")]
    [SerializeField] private float coyoteTime  = 0.12f;

    [Tooltip("Seconds before landing where a jump press is remembered and fires on touch-down.")]
    [SerializeField] private float jumpBuffer  = 0.12f;

    // ── Squash & Stretch ──────────────────────────────────────────────────────
    [Header("Squash & Stretch")]
    [Tooltip("The child Transform that holds the mesh (e.g. the 'master' bone or FBX root child). Leave empty to skip the effect.")]
    [SerializeField] private Transform visualRoot;

    [SerializeField] private float squashAmount    = 0.28f;
    [SerializeField] private float stretchAmount   = 0.18f;
    [SerializeField] private float squashRecovery  = 9f;

    // ── Animator Parameters ───────────────────────────────────────────────────
    [Header("Animator")]
    [Tooltip("Float parameter name for horizontal speed (blend trees).")]
    [SerializeField] private string speedParam      = "Speed";

    [Tooltip("Bool parameter name for the Run state (matches the demo animator).")]
    [SerializeField] private string runParam        = "Run";

    [Tooltip("Bool parameter name for whether the rabbit is grounded.")]
    [SerializeField] private string isGroundedParam = "IsGrounded";

    // ── Private State ─────────────────────────────────────────────────────────
    private CharacterController  _cc;
    private Animator             _anim;
    private Camera               _cam;
    private InputSystem_Actions  _input;

    private Vector2 _moveInput;
    private Vector3 _velocity;
    private bool    _isGrounded;
    private bool    _wasGrounded;
    private bool    _isSprinting;

    private float _coyoteTimer;
    private float _jumpBufferTimer;

    // Cached animator hashes (avoids per-frame string lookups)
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

        // Disable root motion so the CharacterController owns all movement
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
        ProcessJump();

        // Build the full motion vector and apply in ONE Move call to avoid
        // double-stepping the CharacterController's internal ground detection.
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
            // Reset vertical velocity when grounded so gravity doesn't accumulate
            if (_velocity.y < 0f)
                _velocity.y = -2f;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
            // Accumulate gravity while airborne
            _velocity.y += gravity * Time.deltaTime;
        }
    }

    // ── Jump / Bounce ─────────────────────────────────────────────────────────
    private void ProcessJump()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            // Classic jump formula: v = sqrt(2 * |g| * h)
            _velocity.y      = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _coyoteTimer     = 0f;
            _jumpBufferTimer = 0f;
        }
    }

    // ── Horizontal Movement ───────────────────────────────────────────────────
    private Vector3 CalcHorizontalMotion()
    {
        if (_moveInput.sqrMagnitude < 0.01f) return Vector3.zero;

        float speed = _isSprinting ? sprintSpeed : walkSpeed;

        // Flatten camera axes to XZ — guards against near-vertical camera angles
        Vector3 camFwd   = _cam.transform.forward;
        Vector3 camRight = _cam.transform.right;
        camFwd.y   = 0f;
        camRight.y = 0f;

        // If camera is looking almost straight up/down, fall back to world axes
        if (camFwd.sqrMagnitude < 0.01f)   camFwd   = Vector3.forward;
        if (camRight.sqrMagnitude < 0.01f) camRight = Vector3.right;

        camFwd.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camFwd * _moveInput.y + camRight * _moveInput.x).normalized;

        // Smoothly rotate rabbit to face movement direction
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
            // Landing: squash downward, spread sideways
            s.y = 1f - squashAmount;
            s.x = 1f + squashAmount * 0.55f;
            s.z = 1f + squashAmount * 0.55f;
        }
        else if (!_isGrounded && _velocity.y > 0f)
        {
            // Rising: stretch upward, compress sideways
            s.y = 1f + stretchAmount;
            s.x = 1f - stretchAmount * 0.4f;
            s.z = 1f - stretchAmount * 0.4f;
        }
        else
        {
            // Recover back to neutral (1, 1, 1)
            s = Vector3.Lerp(s, Vector3.one, squashRecovery * Time.deltaTime);
        }

        visualRoot.localScale = s;
    }

    // ── Animator ──────────────────────────────────────────────────────────────
    private void AnimateRabbit()
    {
        if (_anim == null) return;

        // Horizontal speed only — ignores vertical so jumping doesn't affect run anim
        float horizSpeed = new Vector2(_cc.velocity.x, _cc.velocity.z).magnitude;

        _anim.SetFloat(_speedHash, horizSpeed);         // for blend trees
        _anim.SetBool(_runHash, horizSpeed > 0.1f);     // for the demo-style Run bool
        _anim.SetBool(_isGroundedHash, _isGrounded);
    }
}
