using UnityEngine;
using UnityEngine.EventSystems;

public class CrusaderControl : MonoBehaviour 
{
    Animator anim;
    CharacterController controller;

    [Header("Cài đặt Di chuyển")]
    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    public float turnSpeed = 10f;
    public float gravity = 20f; 
    public float jumpForce = 8f;

    [Header("Chiến đấu (Không cần Animation Event)")]
    public float attackDistance = 2.5f; // Khoảng cách chém tới
    public int attackDamage = 25;      // Sát thương mỗi lần chém
    public LayerMask enemyLayer;       // Phải chọn đúng Layer Enemy trong Inspector

    private float verticalVelocity; 
    private Vector3 moveDir;

    void Start() 
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update() 
    {
        HandleMovement();
        
        // Nhấn chuột trái để đánh
        if (Input.GetMouseButtonDown(0)) 
        {
            // Kiểm tra nếu không nhấn đè lên UI (như nút bấm, menu)
            if (!EventSystem.current.IsPointerOverGameObject()) 
            {
                ExecuteAttack();
            }
        }

        // Các phím chức năng khác giữ nguyên
        anim.SetBool("isShielding", Input.GetKey(KeyCode.Q));
        anim.SetBool("isCrouching", Input.GetKey(KeyCode.LeftControl));
    }

    void ExecuteAttack() 
    {
        // 1. Chạy Animation đánh (Giữ nguyên Animator của bạn)
        anim.SetTrigger("Attack"); 

        // 2. Logic gây sát thương bằng Raycast
        RaycastHit hit;
        // Bắn tia từ bụng Player (cao lên 1m), hướng về phía trước
        Vector3 rayOrigin = transform.position + Vector3.up; 
        
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, attackDistance, enemyLayer)) 
        {
            // Nếu trúng vật thể, tìm script EnemyController trên vật thể đó
            EnemyController enemy = hit.collider.GetComponent<EnemyController>();
            if (enemy != null) 
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log("Đã chém trúng: " + hit.collider.name);
            }
        }
    }

    void HandleMovement() 
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = (camForward * v + camRight * h).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;
        
        anim.SetFloat("Speed", inputDir.magnitude * (isRunning ? 2f : 1f), 0.1f, Time.deltaTime);

        if (inputDir.magnitude >= 0.1f) 
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            moveDir = inputDir;
        } 
        else moveDir = Vector3.zero;

        if (controller.isGrounded) 
        {
            verticalVelocity = -2f;
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                anim.SetTrigger("Jump");
                verticalVelocity = jumpForce;
            }
        } 
        else verticalVelocity -= gravity * Time.deltaTime;

        Vector3 finalMove = moveDir * speed;
        finalMove.y = verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    // Vẽ tia màu xanh trong cửa sổ Scene để bạn dễ căn chỉnh khoảng cách
    void OnDrawGizmos() 
    {
        Gizmos.color = Color.blue;
        Vector3 rayOrigin = transform.position + Vector3.up;
        Gizmos.DrawRay(rayOrigin, transform.forward * attackDistance);
    }
}