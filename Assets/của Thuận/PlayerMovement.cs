using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private InputSystem_Actions playerControls;
    private Transform cameraTransform;

    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float sprintSpeed = 6.0f;
    
    [Tooltip("Tốc độ di chuyển khi đang giữ chuột phải (Aim)")]
    public float aimMoveSpeed = 2.0f; 

    [Tooltip("Độ mượt khi xoay nhân vật theo Camera")]
    public float rotationSpeed = 15.0f; 

    // Biến trạng thái để Animation đọc
    [HideInInspector] public bool isSprinting = false; 
    [HideInInspector] public bool isAiming = false;
    
    // --- MỚI: Biến trạng thái Ngồi ---
    [HideInInspector] public bool isCrouching = false; 

    [Header("Crouch Settings (Mới)")]
    public float crouchSpeed = 1.5f;     // Tốc độ khi ngồi
    public float crouchHeight = 1.0f;    // Chiều cao khi ngồi
    public float standingHeight = 2.0f;  // Chiều cao khi đứng
    public float crouchTransitionSpeed = 10f; // Tốc độ co/giãn người

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;

    // --- ĐÃ XÓA HEADER JUMP SETTINGS ---

    [Header("Buffer/Coyote Time")]
    public float groundCheckBuffer = 0.2f;

    private float timeSinceGrounded = 0f;
    [HideInInspector] public bool isGrounded_Reliable;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    [HideInInspector] public bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public Vector2 moveInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        // Đặt chiều cao mặc định lúc đầu
        controller.height = standingHeight;
        controller.center = new Vector3(0, standingHeight / 2f, 0);

        playerControls = new InputSystem_Actions();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Lỗi: Không tìm thấy Main Camera (Tag 'MainCamera')!");
        }
    }

    private void OnEnable() => playerControls.Player.Enable();
    private void OnDisable() => playerControls.Player.Disable();

    void Update()
    {
        // 1. ĐỌC INPUT
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        bool dashPressed = playerControls.Player.Dash.WasPressedThisFrame();
        bool sprintHeld = playerControls.Player.Sprint.IsPressed(); 

        // --- SỬA: Dùng nút Jump (Space) để làm nút Ngồi (Hold) ---
        // Giữ nút Space là Ngồi, thả ra là Đứng
        bool crouchHeld = playerControls.Player.Jump.IsPressed();

        // Đọc nút Aim (Chuột phải)
        if (playerControls.Player.Aim != null)
        {
             isAiming = playerControls.Player.Aim.IsPressed();
        }

        // Logic Sprint: Chỉ chạy nhanh khi có di chuyển và KHÔNG NGỒI, KHÔNG NGẮM
        isSprinting = sprintHeld && moveInput.magnitude > 0.1f && !isAiming && !crouchHeld;
        
        // Logic Crouch:
        isCrouching = crouchHeld;

        // 2. GROUND CHECK (Vẫn giữ để tính trọng lực)
        bool isGrounded_Controller = controller.isGrounded;
        if (isGrounded_Controller)
        {
            if (velocity.y < 0) velocity.y = -0.5f; // Ép nhân vật dính xuống đất
            timeSinceGrounded = 0f;
            isGrounded_Reliable = true;
        }
        else
        {
            timeSinceGrounded += Time.deltaTime;
            isGrounded_Reliable = (timeSinceGrounded < groundCheckBuffer);
        }

        // 3. XỬ LÝ CHIỀU CAO (NGỒI/ĐỨNG)
        // Tính toán chiều cao mục tiêu
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        
        // Dùng Lerp để thay đổi chiều cao mượt mà
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        
        // Chỉnh lại tâm (Center) để chân không bị thụt xuống đất khi co người lại
        Vector3 targetCenter = new Vector3(0, targetHeight / 2f, 0);
        controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * crouchTransitionSpeed);


        // 4. DASH (Lướt) - Không cho lướt khi đang ngồi
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        if (dashPressed && dashCooldownTimer <= 0 && !isDashing && !isCrouching)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            velocity.y = 0f;
            
            Vector3 camFwd = cameraTransform.forward;
            Vector3 camRgt = cameraTransform.right;
            camFwd.y = 0; camRgt.y = 0;
            camFwd.Normalize(); camRgt.Normalize();

            dashDirection = camFwd * moveInput.y + camRgt * moveInput.x;
            if (dashDirection.sqrMagnitude < 0.01f) dashDirection = transform.forward;
            dashDirection.Normalize();
        }

        // 5. TRỌNG LỰC (Vẫn cần để rơi nếu đi ra khỏi mép vực)
        if (!isDashing)
        {
            // Bỏ logic fallMultiplier của nhảy, chỉ dùng gravity thường
            velocity.y += gravity * Time.deltaTime;
        }

        // --- ĐÃ XÓA LOGIC NHẢY ---

        // 6. DI CHUYỂN & XOAY NGƯỜI
        Vector3 finalMove;
        
        if (isDashing)
        {
            finalMove = dashDirection * dashSpeed;
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) { isDashing = false; if (!controller.isGrounded) velocity.y = -0.1f; }
        }
        else
        {
            // A. Tính hướng di chuyển theo Camera
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();

            Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

            // B. Xoay nhân vật
            if (moveDirection.sqrMagnitude > 0.01f || isAiming)
            {
                Quaternion targetRotation;

                if (isAiming)
                {
                    targetRotation = Quaternion.LookRotation(camForward);
                }
                else
                {
                    if (moveDirection.sqrMagnitude > 0.01f)
                    {
                        targetRotation = Quaternion.LookRotation(moveDirection);
                    }
                    else
                    {
                        targetRotation = transform.rotation;
                    }
                }
                
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // C. Áp dụng tốc độ (Logic ưu tiên: Ngồi < Ngắm < Đi bộ < Chạy)
            float targetSpeed = walkSpeed;

            if (isCrouching)
            {
                targetSpeed = crouchSpeed; // Ưu tiên chậm nhất khi ngồi
            }
            else if (isAiming)
            {
                targetSpeed = aimMoveSpeed;
            }
            else if (isSprinting) 
            {
                targetSpeed = sprintSpeed;
            }

            finalMove = moveDirection.normalized * targetSpeed;
        }

        finalMove.y = velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }
}