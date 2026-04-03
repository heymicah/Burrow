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

    void Update()
    {
        float distance = Vector3.Distance(transform.position, rabbit.position);

        // START CHASE (ONLY if not already chasing)
        if (!isChasing && distance <= detectionRange)
        {
            isChasing = true;
            chaseTimer = chaseDuration;
        }

        // CHASING LOGIC
        if (isChasing)
        {
            // countdown timer
            chaseTimer -= Time.deltaTime;

            // STOP if time runs out
            if (chaseTimer <= 0f)
            {
                isChasing = false;
                return;
            }

            // Stay near rabbit but stop moving closer
            if (distance <= stopDistance)
            {
                transform.LookAt(rabbit);
                return;
            }

            // MOVE
            transform.position = Vector3.MoveTowards(
                transform.position,
                rabbit.position,
                chaseSpeed * Time.deltaTime
            );

            transform.LookAt(rabbit);
        }
    }
}