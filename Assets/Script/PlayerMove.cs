using UnityEngine;

public class CrusaderControl : MonoBehaviour {
    Animator anim;
    CharacterController controller;

    [Header("Thông số di chuyển")]
    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    public float gravity = 20f; 
    public float jumpForce = 8f;
    
    private float verticalVelocity; 
    private Vector3 moveDir = Vector3.zero;

    void Start() {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update() {
        // 1. Lấy hướng từ Camera để nhân vật đi đúng hướng đang nhìn
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 2. Lấy Input phím di chuyển
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = (camForward * v + camRight * h).normalized;

        // 3. Xử lý Tốc độ & Animator
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        // Speed cho Animator: 0 (nghỉ), 1 (đi bộ), 2.5 (chạy)
        float animSpeed = inputDir.magnitude * (isRunning ? 2.5f : 1f);
        anim.SetFloat("Speed", animSpeed);

        // 4. Xử lý Trọng lực và Nhảy
        if (controller.isGrounded) {
            verticalVelocity = -1f; // Ghì nhân vật xuống mặt đất
            if (Input.GetKeyDown(KeyCode.Space)) {
                anim.SetTrigger("Jump");
                verticalVelocity = jumpForce;
            }
        } else {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // 5. Thực hiện di chuyển
        Vector3 finalMove = inputDir * currentSpeed;
        finalMove.y = verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        // 6. Xoay mặt nhân vật theo hướng Camera
        // Chỉ xoay khi có di chuyển hoặc xoay dần theo hướng Camera Y
        float targetAngle = Camera.main.transform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.15f);

        // 7. Các nút chức năng khác
        if (Input.GetMouseButtonDown(0)) anim.SetTrigger("Attack"); // Chuột trái
        
        if (Input.GetKeyDown(KeyCode.LeftControl)) anim.SetBool("isCrouching", true);
        if (Input.GetKeyUp(KeyCode.LeftControl)) anim.SetBool("isCrouching", false);
    }
}