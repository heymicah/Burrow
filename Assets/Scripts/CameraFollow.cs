using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mouse-driven orbit camera that circles the rabbit.
/// Uses the "Look" action from InputSystem_Actions (mouse delta + right stick).
///
/// Setup:
///   1. Unparent Main Camera from the rabbit in the Hierarchy.
///   2. Add this script to the Main Camera.
///   3. Assign the rabbit's root Transform to "Target".
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The rabbit's root Transform.")]
    [SerializeField] private Transform target;

    [Tooltip("Local-space offset from the target's root transform to the orbit pivot. " +
             "Adjust X to fix left/right centering, Y to set height (0 = feet, 0.5 = mid-body).")]
    [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Orbit")]
    [SerializeField] private float distance = 4f;

    [Tooltip("Degrees per pixel for mouse. Tune this to feel right.")]
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Tooltip("Degrees per second for gamepad right stick.")]
    [SerializeField] private float gamepadSensitivity = 120f;

    [SerializeField] private bool invertY = false;

    [Header("Pitch Clamp")]
    [SerializeField] private float minPitch = -15f;
    [SerializeField] private float maxPitch =  60f;

    [Header("Position Smoothing")]
    [Tooltip("How quickly the camera moves to its target position. Higher = snappier.")]
    [SerializeField] private float smoothSpeed = 12f;

    // ── State ─────────────────────────────────────────────────────────────────
    private InputSystem_Actions _input;
    private float _yaw;
    private float _pitch = 20f;
    private Vector3 _smoothVelocity;
    private bool _usingGamepad;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _input = new InputSystem_Actions();

        // Seed yaw/pitch from where the camera starts so it doesn't snap on frame 1
        Vector3 angles = transform.eulerAngles;
        _yaw   = angles.y;
        _pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void OnEnable()  => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    private void LateUpdate()
    {
        if (target == null) return;

        ReadLookInput(out float deltaYaw, out float deltaPitch);

        _yaw   += deltaYaw;
        _pitch += invertY ? deltaPitch : -deltaPitch;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // Orbit position: start from pivot, go backwards along the rotated -Z axis
        Quaternion rotation   = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    pivot      = target.position + pivotOffset;
        Vector3    desiredPos = pivot - rotation * Vector3.forward * distance;

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos,
            ref _smoothVelocity, 1f / smoothSpeed
        );
        transform.rotation = rotation;
    }

    // ── Input ─────────────────────────────────────────────────────────────────
    private void ReadLookInput(out float deltaYaw, out float deltaPitch)
    {
        Vector2 look = _input.Player.Look.ReadValue<Vector2>();

        // Detect whether we're getting mouse delta (large values) or a gamepad stick (–1 to 1).
        // Mouse delta is already in pixels/frame; gamepad needs Time.deltaTime scaling.
        _usingGamepad = Mathf.Abs(look.x) <= 1f && Mathf.Abs(look.y) <= 1f
                        && (look.x != 0f || look.y != 0f);

        if (_usingGamepad)
        {
            deltaYaw   =  look.x * gamepadSensitivity * Time.deltaTime;
            deltaPitch =  look.y * gamepadSensitivity * Time.deltaTime;
        }
        else
        {
            deltaYaw   =  look.x * mouseSensitivity;
            deltaPitch =  look.y * mouseSensitivity;
        }
    }
}
