using UnityEngine;
using TMPro;

public class NPCQuest : MonoBehaviour {
    [Header("Cấu hình UI")]
    public GameObject questPanel;
    public TextMeshProUGUI questText;
    public GameObject questTrackerPanel;
    public TextMeshProUGUI questTrackerText;

    [Header("Cấu hình Spawn Vật phẩm")]
    public GameObject itemPrefab; // Kéo Prefab vật phẩm vào đây
    public Transform[] spawnPoints; // Kéo các Empty Object làm vị trí spawn vào đây
    public int targetItemAmount = 5;

    private int currentItemAmount = 0;
    private bool isQuestActive = false;
    private int currentLine = 0;
    private bool isPlayerInRange = false;
    private bool isTalking = false;

    [Header("Lời thoại")]
    [TextArea(3, 10)] public string[] dialogues;
    public string declineMessage = "Tiếc quá!";

    void Start() {
        if (questPanel != null) questPanel.SetActive(false);
        if (questTrackerPanel != null) questTrackerPanel.SetActive(false);
    }

    void Update() {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F)) {
            if (!isTalking) StartDialogue();
        }
    }

    void StartDialogue() {
        isTalking = true;
        currentLine = 0;
        questPanel.SetActive(true);
        DisplayLine();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void DisplayLine() {
        if (questText != null && currentLine < dialogues.Length) {
            questText.text = dialogues[currentLine];
        }
    }

    public void AcceptQuest() {
        currentLine++; 
        if (currentLine < dialogues.Length) {
            DisplayLine();
        } else {
            StartQuestLogic();
            CloseQuest();
        }
    }

    void StartQuestLogic() {
        isQuestActive = true;
        currentItemAmount = 0;
        if (questTrackerPanel != null) questTrackerPanel.SetActive(true);
        UpdateTrackerUI();

        // GỌI HÀM SPAWN VẬT PHẨM KHI NHẬN NHIỆM VỤ
        SpawnQuestItems();
    }

    // --- HÀM SPAWN VẬT PHẨM MỚI ---
    void SpawnQuestItems() {
        if (itemPrefab == null || spawnPoints.Length == 0) return;

        // Tạo vật phẩm tại các vị trí đã định sẵn
        for (int i = 0; i < spawnPoints.Length; i++) {
            if (i < targetItemAmount) { // Chỉ spawn đủ số lượng cần
                Instantiate(itemPrefab, spawnPoints[i].position, Quaternion.identity);
            }
        }
    }

    public void AddItem() {
        if (!isQuestActive) return;
        currentItemAmount++;
        UpdateTrackerUI();
        if (currentItemAmount >= targetItemAmount) CompleteQuest();
    }

    void UpdateTrackerUI() {
        if (questTrackerText != null) {
            questTrackerText.text = "Thu thập: " + currentItemAmount + "/" + targetItemAmount;
        }
    }

    void CompleteQuest() {
        isQuestActive = false;
        if (questTrackerText != null) {
            questTrackerText.text = "Hoàn thành!";
            questTrackerText.color = Color.green;
        }
        Invoke("HideQuestTracker", 2.0f);
    }

    void HideQuestTracker() {
        if (questTrackerPanel != null) questTrackerPanel.SetActive(false);
    }

    public void CloseQuest() {
        isTalking = false;
        if (questPanel != null) questPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) { isPlayerInRange = false; CloseQuest(); } }
}