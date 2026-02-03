using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    [Header("1. Kéo File dữ liệu (ItemData) vào đây")]
    public ItemData itemData; 

    [Header("2. Kéo chữ UI 'Press F' vào đây")]
    public GameObject pickupMessageUI; 

    private bool playerInRange = false;

    void Start()
    {
        // Tắt chữ lúc đầu
        if (pickupMessageUI != null) pickupMessageUI.SetActive(false);
        
        // Kiểm tra xem đã có Collider chưa
        if (GetComponent<Collider>() == null)
            Debug.LogError("Lỗi: Cục vật phẩm " + gameObject.name + " chưa có Collider!");
    }

    void Update()
    {
        // Nếu đứng gần + Bấm F
        if (playerInRange && Keyboard.current.fKey.wasPressedThisFrame)
        {
            PickUpItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (pickupMessageUI != null)
            {
                pickupMessageUI.SetActive(true); // Hiện chữ
            }
            else
            {
                // Báo lỗi nếu quên kéo UI
                Debug.LogWarning("Chưa kéo cái chữ 'Press F' vào script ItemPickup kìa!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (pickupMessageUI != null) pickupMessageUI.SetActive(false); // Tắt chữ
        }
    }

    void PickUpItem()
    {
        // 1. Thêm vào túi (Kiểm tra xem có Manager chưa)
        if (InventoryManager.Instance != null && itemData != null)
        {
            InventoryManager.Instance.AddItem(itemData);
            Debug.Log("Đã nhặt: " + itemData.itemName);
        }
        else
        {
            if (itemData == null) Debug.LogError("Lỗi: Chưa gắn file ItemData cho vật phẩm này!");
            if (InventoryManager.Instance == null) Debug.LogError("Lỗi: Chưa có InventoryManager trong Scene!");
        }

        // 2. Tắt chữ UI
        if (pickupMessageUI != null) pickupMessageUI.SetActive(false);

        // 3. Xóa vật phẩm
        Destroy(gameObject);
    }
}