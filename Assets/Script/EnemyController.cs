using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public Transform playerTarget;   // Kéo Player vào đây
    public float attackRange = 2.0f; // Khoảng cách để ĐÁNH
    public float attackCooldown = 2.0f;

    [Header("Cài đặt AI Thông minh")]
    public float chaseRange = 10.0f;  // Khoảng cách để bắt đầu ĐUỔI
    public float patrolRadius = 5.0f; // Bán kính vùng đi dạo tự do
    public float patrolWaitTime = 3.0f; // Thời gian đứng nghỉ rồi mới đi tiếp

    [Header("Chỉ số sống còn")]
    public int maxHealth = 100;
    private int currentHealth;

    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;

    private Vector3 startPosition;
    private float patrolTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        startPosition = transform.position;
        patrolTimer = patrolWaitTime;
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < chaseRange)
        {
            EngagePlayer(distanceToPlayer);
        }
        else
        {
            PatrolLogic();
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(30);
        }
    }

    void EngagePlayer(float distance)
    {
        if (distance > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }
        else
        {
            agent.isStopped = true;
            RotateTowards(playerTarget.position);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    void PatrolLogic()
    {
        if (agent.remainingDistance > agent.stoppingDistance) return;

        patrolTimer += Time.deltaTime;

        if (patrolTimer >= patrolWaitTime)
        {
            Vector3 newPos = RandomNavSphere(startPosition, patrolRadius, -1);
            agent.SetDestination(newPos);
            agent.isStopped = false;
            patrolTimer = 0;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    void PerformAttack()
    {
        int randomIndex = Random.Range(1, 4);
        animator.SetInteger("AttackIndex", randomIndex);
        animator.SetTrigger("Attack");

        // --- ĐOẠN CODE THÊM MỚI: Gây sát thương cho Player ---
        if (playerTarget != null)
        {
            PlayerHealth pHealth = playerTarget.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                // Kiểm tra lại khoảng cách để chắc chắn đánh trúng
                float dist = Vector3.Distance(transform.position, playerTarget.position);
                if (dist <= attackRange + 0.5f)
                {
                    pHealth.TakeDamage(10); // Mỗi cú đánh mất 10 máu
                }
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
        else animator.SetTrigger("Hit");
        chaseRange = 100f;
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        agent.isStopped = true;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 5f);
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, patrolRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}