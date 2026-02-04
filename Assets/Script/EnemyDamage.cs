using UnityEngine;

public class EnemyDamageBridge : MonoBehaviour
{
    public int damageAmount = 10;
    private PlayerHealth pHealth;

    void Start() {
        // Tìm Player trong game thông qua Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) pHealth = player.GetComponent<PlayerHealth>();
    }

    // Hàm này sẽ được gọi từ Animation của Quái
    public void DealDamageToPlayer() {
        if (pHealth != null) {
            // Kiểm tra khoảng cách: chỉ mất máu nếu quái đang ở gần sát Player
            float dist = Vector3.Distance(transform.position, pHealth.transform.position);
            if (dist <= 3.0f) { 
                pHealth.TakeDamage(damageAmount);
            }
        }
    }
}