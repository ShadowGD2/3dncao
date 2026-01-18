using UnityEngine;

public class CrusaderControl : MonoBehaviour {
    Animator anim;
    CharacterController controller;

    [Header("Thông số di chuyển")]
    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    public float gravity = 20f; 
    public float jumpForce = 8f;
    
    [Header("Thông số chiến đấu")]
    public Transform attackPoint;   // Điểm tâm của cú chém (thường đặt trước mặt)
    public float attackRange = 1f;  // Bán kính cú chém
    public LayerMask enemyLayers;   // Chỉ đánh vào những gì thuộc lớp Enemy
    public int attackDamage = 20;   // Sát thương gây ra

    private float verticalVelocity; 
    private Vector3 moveDir = Vector3.zero;

    void Start() {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update() {
        // --- 1. XỬ LÝ DI CHUYỂN (GIỮ NGUYÊN) ---
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = (camForward * v + camRight * h).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        float animSpeed = inputDir.magnitude * (isRunning ? 2.5f : 1f);
        anim.SetFloat("Speed", animSpeed);

        if (controller.isGrounded) {
            verticalVelocity = -1f; 
            if (Input.GetKeyDown(KeyCode.Space)) {
                anim.SetTrigger("Jump");
                verticalVelocity = jumpForce;
            }
        } else {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 finalMove = inputDir * currentSpeed;
        finalMove.y = verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        float targetAngle = Camera.main.transform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.15f);

        // --- 2. XỬ LÝ TẤN CÔNG ---
        // Khi bấm chuột, chỉ kích hoạt Animation. 
        // Việc gây sát thương sẽ do Animation Event gọi hàm DealDamage() bên dưới.
        if (Input.GetMouseButtonDown(0)) 
        {
            anim.SetTrigger("Attack"); 
        }
        
        if (Input.GetKeyDown(KeyCode.LeftControl)) anim.SetBool("isCrouching", true);
        if (Input.GetKeyUp(KeyCode.LeftControl)) anim.SetBool("isCrouching", false);
    }

    // --- HÀM MỚI: Gây sát thương (Sẽ được gọi bởi Animation Event) ---
    public void DealDamage()
    {
        // 1. Tạo một hình cầu vô hình tại attackPoint để kiểm tra va chạm
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        // 2. Duyệt qua tất cả kẻ thù trúng đòn
        foreach(Collider enemy in hitEnemies)
        {
            Debug.Log("Đã chém trúng: " + enemy.name);

            // Gọi hàm nhận sát thương bên script EnemyController
            EnemyController enemyScript = enemy.GetComponent<EnemyController>();
            if(enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
            }
        }
    }

    // Vẽ vòng tròn đỏ trong Scene để bạn dễ căn chỉnh tầm đánh (Gizmos)
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}