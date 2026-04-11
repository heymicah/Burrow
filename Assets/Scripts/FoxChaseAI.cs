using UnityEngine;

public class FoxChaseAI : MonoBehaviour
{
    public Transform rabbit;

    public float detectionRange = 5f;
    public float chaseSpeed = 3f;
    public float stopDistance = 1.5f;
    public float chaseDuration = 4f;

    private bool isChasing = false;
    private float chaseTimer = 0f;
    private Animator animator;

    [Header("Terrain Grounding")]
    [SerializeField] private float groundCheckDistance = 5f;
    [SerializeField] private LayerMask groundLayer; // set to "Terrain" in Inspector
    [SerializeField] private float groundOffset = 0f; // tweak if fox floats or sinks

    private void SnapToGround()
    {
        // Cast a ray downward from slightly above the fox
        Vector3 rayOrigin = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + groundOffset;
            transform.position = pos;
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Guard against null rabbit (prefab default)
        if (rabbit == null) return;

        float distance = Vector3.Distance(transform.position, rabbit.position);

        if (!isChasing && distance <= detectionRange)
        {
            isChasing = true;
            chaseTimer = chaseDuration;
        }

        if (isChasing)
        {
            chaseTimer -= Time.deltaTime;

            if (chaseTimer <= 0f)
            {
                isChasing = false;
                SetWalking(false);
                return;
            }

            if (distance <= stopDistance)
            {
                SetWalking(false);
                FaceTarget();
                return;
            }

            // Move toward rabbit
            Vector3 direction = (rabbit.position - transform.position).normalized;
            // Only move on XZ plane so Y doesn't get weird
            direction.y = 0f;
            transform.position += direction * chaseSpeed * Time.deltaTime;
            FaceTarget();
            SetWalking(true);
        }
        else
        {
            SetWalking(false);
        }
        SnapToGround();
    }

    private void FaceTarget()
    {
        Vector3 lookTarget = new Vector3(rabbit.position.x, transform.position.y, rabbit.position.z);
        transform.LookAt(lookTarget);
    }

    private void SetWalking(bool walking)
    {
        if (animator != null)
            animator.SetBool("isWalking", walking); // match your animator param name
    }

    public bool IsChasing => isChasing;
}