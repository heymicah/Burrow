using UnityEngine;

/// <summary>
/// Smooth third-person camera that follows and orbits a target.
///
/// Setup:
///   1. Unparent the Main Camera from the rabbit in the Hierarchy.
///   2. Add this script to the Main Camera.
///   3. Assign the rabbit's root Transform to "Target".
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The rabbit's root Transform.")]
    [SerializeField] private Transform target;

    [Tooltip("World-space height offset for where the camera looks (mid-body, not feet).")]
    [SerializeField] private float lookHeight = 0.5f;

    [Header("Position")]
    [Tooltip("How far behind and above the target the camera sits.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -4f);

    [Tooltip("How quickly the camera catches up to the target.")]
    [SerializeField] private float positionSmoothing = 8f;

    [Header("Rotation")]
    [Tooltip("How quickly the camera rotates to stay behind the rabbit.")]
    [SerializeField] private float rotationSmoothing = 6f;

    private Vector3 _currentVelocity;

    private void LateUpdate()
    {
        if (target == null) return;

        // Desired camera position: offset is in the rabbit's local space
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Smooth damp is better than Lerp for camera — no oscillation at low speeds
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _currentVelocity,
            1f / positionSmoothing
        );

        // Always look at the rabbit's mid-body point
        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothing * Time.deltaTime);
    }
}
