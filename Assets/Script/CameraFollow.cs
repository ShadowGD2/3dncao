using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform target; // Nhân vật để theo dõi
    public float mouseSensitivity = 100f;
    public Vector3 offset = new Vector3(0, 2f, -5f); // Khoảng cách camera so với nhân vật

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start() {
        // Khóa chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate() {
        if (!target) return;

        // Lấy Input chuột
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -20f, 45f); // Giới hạn góc nhìn lên xuống

        // Xoay Camera xung quanh nhân vật
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.position = target.position + rotation * offset;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}