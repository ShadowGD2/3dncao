using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public Transform playerTarget;   // Kéo Player vào đây
    public float attackRange = 2.0f; // Khoảng cách để ĐÁNH
    public float attackCooldown = 2.0f;

    [Header("Cài đặt AI Thông minh")]
    public float chaseRange = 10.0f;  // Khoảng cách để bắt đầu ĐUỔI (phải lớn hơn Attack Range)
    public float patrolRadius = 5.0f; // Bán kính vùng đi dạo tự do
    public float patrolWaitTime = 3.0f; // Thời gian đứng nghỉ rồi mới đi tiếp

    [Header("Chỉ số sống còn")]
    public int maxHealth = 100;
    private int currentHealth;

    // Các biến nội bộ
    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;
    
    // Biến cho việc tuần tra
    private Vector3 startPosition; // Vị trí gốc (ổ của quái)
    private float patrolTimer;     // Bộ đếm thời gian nghỉ

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        
        // Lưu lại vị trí ban đầu làm tâm vùng tuần tra
        startPosition = transform.position;
        patrolTimer = patrolWaitTime; // Để nó đi ngay khi vào game
    }

    void Update()
    {
        if (isDead) return;

        // Tính khoảng cách tới Player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // --- TRƯỜNG HỢP 1: PHÁT HIỆN PLAYER (ĐUỔI THEO) ---
        if (distanceToPlayer < chaseRange)
        {
            EngagePlayer(distanceToPlayer);
        }
        // --- TRƯỜNG HỢP 2: KHÔNG THẤY PLAYER (TUẦN TRA) ---
        else
        {
            PatrolLogic();
        }

        // Cập nhật Animation chạy/đứng
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    // Hàm xử lý việc đuổi và đánh
    void EngagePlayer(float distance)
    {
        // Nếu chưa tới tầm đánh -> Chạy tới
        if (distance > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }
        else // Đã tới tầm đánh -> Dừng lại và Bem
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

    // Hàm xử lý đi dạo ngẫu nhiên
    void PatrolLogic()
    {
        // Nếu đang đi dạo và chưa đến nơi -> cứ đi tiếp
        if (agent.remainingDistance > agent.stoppingDistance) 
        {
            return; 
        }

        // Nếu đã đến nơi (hoặc đang đứng yên) -> Bắt đầu đếm giờ nghỉ
        patrolTimer += Time.deltaTime;

        if (patrolTimer >= patrolWaitTime)
        {
            // Hết giờ nghỉ -> Tìm điểm mới để đi
            Vector3 newPos = RandomNavSphere(startPosition, patrolRadius, -1);
            agent.SetDestination(newPos);
            agent.isStopped = false;
            
            patrolTimer = 0; // Reset bộ đếm
        }
    }

    // Hàm tìm điểm ngẫu nhiên trên NavMesh (Copy công thức chuẩn của Unity)
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    // --- CÁC HÀM CŨ (Attack, Die, TakeDamage...) GIỮ NGUYÊN ---
    void PerformAttack()
    {
        int randomIndex = Random.Range(1, 4);
        animator.SetInteger("AttackIndex", randomIndex);
        animator.SetTrigger("Attack");
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
        else animator.SetTrigger("Hit");
        
        // Mẹo hay: Bị đánh cái là quay ra đuổi luôn, khỏi cần check tầm nhìn
        chaseRange = 100f; // Tăng tầm đuổi lên cực đại tạm thời (Aggro)
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

    // Vẽ vòng tròn trong Scene để bạn dễ chỉnh (Màu vàng: Tuần tra, Màu đỏ: Tầm đuổi)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}