using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro; // Thư viện cho chữ TextMeshPro

// Class phụ để lưu: Món đồ + Số lượng
[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int stackSize; // Số lượng đang có

    public InventoryItem(ItemData _data)
    {
        data = _data;
        stackSize = 1;
    }

    public void AddToStack()
    {
        stackSize++;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Cấu hình UI")]
    public GameObject inventoryPanel;
    public Transform itemContent;
    public GameObject inventoryItemPrefab;

    // Danh sách lưu trữ (Lưu cả số lượng)
    public List<InventoryItem> inventory = new List<InventoryItem>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        // Bấm I bật/tắt túi
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (inventoryPanel != null) ToggleInventory();
        }
    }

    // --- HÀM THÊM ĐỒ (LOGIC XẾP CHỒNG) ---
    public void AddItem(ItemData itemData)
    {
        // 1. Kiểm tra xem món này có cho xếp chồng không?
        if (itemData.isStackable)
        {
            bool itemAlreadyInInventory = false;

            // Quét túi xem có món này chưa
            foreach (InventoryItem item in inventory)
            {
                if (item.data == itemData) // Tìm thấy món trùng!
                {
                    item.AddToStack(); // Cộng số lượng lên
                    itemAlreadyInInventory = true;
                    Debug.Log("Đã gộp " + itemData.itemName + " -> Số lượng: " + item.stackSize);
                    break;
                }
            }

            // Nếu quét hết túi mà chưa có -> Tạo ô mới
            if (!itemAlreadyInInventory)
            {
                inventory.Add(new InventoryItem(itemData));
            }
        }
        else
        {
            // Nếu không cho xếp chồng -> Luôn tạo ô mới
            inventory.Add(new InventoryItem(itemData));
        }

        RefreshInventoryUI();
    }

    void RefreshInventoryUI()
    {
        if (itemContent == null || inventoryItemPrefab == null) return;

        // Xóa hết ô cũ để vẽ lại
        foreach (Transform child in itemContent) Destroy(child.gameObject);

        // Vẽ từng món trong túi
        foreach (InventoryItem item in inventory)
        {
            GameObject obj = Instantiate(inventoryItemPrefab, itemContent);
            
            // 1. Gán Icon
            var iconImage = obj.transform.Find("Icon").GetComponent<Image>();
            if (iconImage != null) iconImage.sprite = item.data.icon;

            // 2. Gán Số Lượng (Tìm cái Text có tên "AmountText")
            // Nếu bạn dùng Text thường thì đổi TextMeshProUGUI thành Text
            var txtAmount = obj.transform.Find("AmountText").GetComponent<TextMeshProUGUI>();
            
            if (txtAmount != null)
            {
                if (item.stackSize > 1)
                    txtAmount.text = item.stackSize.ToString(); // Hiện số (vd: 5)
                else
                    txtAmount.text = ""; // Nếu có 1 cái thì ẩn số đi cho đẹp
            }
        }
    }

    void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}