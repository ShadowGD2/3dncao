using UnityEngine;

public class MiniMapIconController : MonoBehaviour
{
    public Transform player; // Kéo nhân vật vào đây

    void LateUpdate()
    {
        if (player == null) return;

        // 1. BÁM THEO VỊ TRÍ (Position)
        // Lấy đúng vị trí của player, không cộng thêm độ cao nữa
        transform.position = player.position;

        // 2. XOAY THEO HƯỚNG (Rotation)
        // Lấy góc xoay Y của nhân vật (xoay trái phải)
        float playerRotationY = player.eulerAngles.y;

        // Ép icon luôn nằm ngửa 90 độ (X=90) và xoay theo Y của nhân vật
        transform.rotation = Quaternion.Euler(90f, playerRotationY, 0f);
    }
}