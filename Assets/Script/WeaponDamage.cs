using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    public int damage = 25;
    private bool canDamage = false;

    // Hàm này sẽ được gọi từ script CrusaderControl khi nhấn chuột
    public void EnableDamage() { canDamage = true; }
    public void DisableDamage() { canDamage = false; }

    private void OnTriggerEnter(Collider other)
    {
        // Nếu đang chém và chạm phải Enemy
        if (canDamage && other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                // Sau khi trúng 1 lần thì tắt luôn để không bị trừ máu nhiều lần trong 1 cú chém
                canDamage = false; 
            }
        }
    }
}