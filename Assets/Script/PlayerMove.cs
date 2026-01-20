using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc phải có dòng này để kiểm tra UI

public class CrusaderControl : MonoBehaviour {
    Animator anim;
    CharacterController controller;

    [Header("Thông số di chuyển")]
    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    public float gravity = 20f; 
    public float jumpForce = 8f;
    
    [Header("Thông số chiến đấu")]
    public Transform attackPoint;   
    public float attackRange = 1f;  
    public LayerMask enemyLayers;   
    public int attackDamage = 20;   

    private float verticalVelocity; 
    private float currentSpeed;

    void Start() {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update() {
        // --- 1. XỬ LÝ DI CHUYỂN & XOAY MẶT ---
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
        currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        float animSpeed = inputDir.magnitude * (isRunning ? 2.5f : 1f);
        anim.SetFloat("Speed", animSpeed);

        // Xoay mặt theo hướng di chuyển
        if (inputDir != Vector3.zero) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.15f);
        }

        // --- 2. XỬ LÝ ĐỠ KHIÊN (SHIELD) ---
        if (Input.GetKeyDown(KeyCode.Q)) anim.SetBool("isShielding", true);
        if (Input.GetKeyUp(KeyCode.Q)) anim.SetBool("isShielding", false);

        // --- 3. XỬ LÝ NHẢY ---
        if (controller.isGrounded) {
            verticalVelocity = -1f; 
            if (Input.GetKeyDown(KeyCode.Space)) {
                anim.SetTrigger("Jump");
                verticalVelocity = jumpForce;
            }
        } else {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Thực hiện di chuyển
        Vector3 finalMove = inputDir * currentSpeed;
        finalMove.y = verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        // --- 4. TẤN CÔNG (CÓ KIỂM TRA UI) & NGỒI ---
        // Chỉ cho phép đánh nếu KHÔNG nhấn vào các nút UI (Accept/Decline)
        if (Input.GetMouseButtonDown(0)) 
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                anim.SetTrigger("Attack"); 
            }
        }
        
        if (Input.GetKeyDown(KeyCode.LeftControl)) anim.SetBool("isCrouching", true);
        if (Input.GetKeyUp(KeyCode.LeftControl)) anim.SetBool("isCrouching", false);
    }

    public void DealDamage() {
        if (attackPoint == null) return;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach(Collider enemy in hitEnemies) {
            Debug.Log("Đã chém trúng: " + enemy.name);
        }
    }

    void OnDrawGizmosSelected() {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}