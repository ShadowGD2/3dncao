using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;  // Tên
    public Sprite icon;      // Hình ảnh
    
    public bool isStackable; // [QUAN TRỌNG] Tích vào đây thì mới xếp chồng được
}