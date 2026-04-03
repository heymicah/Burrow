using UnityEngine;

[RequireComponent(typeof(FoxChaseAI))]
public class FoxDamage : MonoBehaviour
{
    [SerializeField] private float damageAmount = 20f;
    [SerializeField] private float damageCooldown = 1.5f;

    private float cooldownTimer;
    private FoxChaseAI chaseAI;

    private void Awake()
    {
        chaseAI = GetComponent<FoxChaseAI>();
    }

    private void Update()
    {
        if (chaseAI.rabbit == null) return;

        cooldownTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, chaseAI.rabbit.position);
        if (distance <= chaseAI.stopDistance && cooldownTimer <= 0f)
        {
            GameManager.Instance.TakeDamage(damageAmount);
            cooldownTimer = damageCooldown;
        }
    }
}
