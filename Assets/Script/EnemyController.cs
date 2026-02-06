using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public Transform playerTarget;   
    public float attackRange = 2.0f; 
    public float attackCooldown = 2.0f;

    [Header("Cài đặt AI")]
    public float chaseRange = 10.0f;  
    public float patrolRadius = 5.0f; 
    public float patrolWaitTime = 3.0f; 

    [Header("Chỉ số")]
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

        if(animator != null) animator.SetFloat("Speed", agent.velocity.magnitude);

        // Phím H để test sát thương
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
        if(animator != null) {
            animator.SetInteger("AttackIndex", randomIndex);
            animator.SetTrigger("Attack");
        }

        if (playerTarget != null)
        {
            // Giả sử script máu của người chơi là PlayerHealth
            var pHealth = playerTarget.GetComponent<PlayerHealth>();
            if (pHealth != null && Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 0.5f)
            {
                pHealth.TakeDamage(10);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
        else if(animator != null) animator.SetTrigger("Hit");
        chaseRange = 100f; // Bị đánh thì đuổi theo xa hơn
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if(animator != null) animator.SetTrigger("Die");
        agent.isStopped = true;
        
        Collider col = GetComponent<Collider>();
        if(col != null) col.enabled = false;

        // KIỂM TRA NẾU LÀ BOSS THÌ XỬ LÝ ĐỂ HIỆN MÀN WIN
        if (gameObject.CompareTag("Boss")) 
        {
            // Hủy sau 3 giây để NPCQuest.cs thấy activeBoss == null và gọi WinGame()
            Destroy(gameObject, 3.0f); 
        }
        else 
        {
            Destroy(gameObject, 5f);
        }
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}