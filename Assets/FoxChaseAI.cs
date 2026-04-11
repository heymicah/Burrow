using UnityEngine;

public class FoxChaseAI : MonoBehaviour
{
    public Transform rabbit;

    public float detectionRange = 5f;
    public float chaseSpeed = 3f;
    public float chaseDuration = 4f;

    private bool isChasing = false;
    private float chaseTimer = 0f;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, rabbit.position);

        // START CHASE
        if (!isChasing && distance <= detectionRange)
        {
            isChasing = true;
            chaseTimer = chaseDuration;
        }

        // ONLY RUN THIS IF CHASING
        if (isChasing)
        {
            chaseTimer -= Time.deltaTime;

            // STOP after time
            if (chaseTimer <= 0f)
            {
                isChasing = false;
                return;
            }

            // MOVE
            transform.position = Vector3.MoveTowards(
                transform.position,
                rabbit.position,
                chaseSpeed * Time.deltaTime
            );
        }
    }
}