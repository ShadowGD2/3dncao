using UnityEngine;

public class QuestItem : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        // Kiểm tra nếu Player chạm vào vật phẩm
        if (other.CompareTag("Player")) {
            // Tìm script NPCQuest trong Scene và gọi hàm AddItem
            NPCQuest npc = Object.FindFirstObjectByType<NPCQuest>();
            if (npc != null) {
                npc.AddItem();
                Destroy(gameObject); // Nhặt xong thì xóa vật phẩm
            }
        }
    }
}