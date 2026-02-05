using UnityEngine;
using TMPro;
using System.Collections;

public class NPCQuest : MonoBehaviour {
    public enum QuestState { NotStarted, Collecting, HuntingBoss, Finished }
    
    [Header("Trạng thái nhiệm vụ")]
    public QuestState currentState = QuestState.NotStarted;

    [Header("Cấu hình UI")]
    public GameObject questPanel;
    public TextMeshProUGUI questText;
    public GameObject questTrackerPanel;
    public TextMeshProUGUI questTrackerText;
    public GameObject winUI; 

    [Header("NV 1: Thu thập")]
    public GameObject itemPrefab; 
    public Transform[] spawnPoints; 
    public int targetItemAmount = 5;
    private int currentItemAmount = 0;

    [Header("NV 2: Đánh Boss")]
    public GameObject bossPrefab; 
    public Transform bossSpawnPoint;
    private GameObject activeBoss;

    [Header("Lời thoại")]
    [TextArea(2, 5)] public string[] startDialogues;
    [TextArea(2, 5)] public string[] bossDialogues;
    public string declineMessage = "Tiếc quá! Hẹn gặp lại bạn sau.";

    private int currentLine = 0;
    private bool isPlayerInRange = false;
    private bool isTalking = false;

    void Start() {
        if (questPanel != null) questPanel.SetActive(false);
        if (questTrackerPanel != null) questTrackerPanel.SetActive(false);
        if (winUI != null) winUI.SetActive(false);
    }

    void Update() {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F) && !isTalking) {
            StartDialogue();
        }

        // Kiểm tra thắng game khi Boss bị tiêu diệt
        if (currentState == QuestState.HuntingBoss && activeBoss == null && questTrackerPanel.activeSelf) {
            WinGame();
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
        string[] currentLines = (currentState == QuestState.NotStarted) ? startDialogues : bossDialogues;
        if (currentLine < currentLines.Length) {
            questText.text = currentLines[currentLine];
        }
    }

    public void AcceptQuest() {
        string[] currentLines = (currentState == QuestState.NotStarted) ? startDialogues : bossDialogues;
        currentLine++;

        if (currentLine < currentLines.Length) {
            DisplayLine();
        } else {
            if (currentState == QuestState.NotStarted) StartCollectQuest();
            else if (currentState == QuestState.Collecting && currentItemAmount >= targetItemAmount) StartBossQuest();
            
            CloseQuest();
        }
    }

    public void DeclineQuest() {
        questText.text = declineMessage;
        Invoke("CloseQuest", 1.5f);
    }

    // --- LOGIC NHIỆM VỤ 1 ---
    void StartCollectQuest() {
        currentState = QuestState.Collecting;
        questTrackerPanel.SetActive(true);
        UpdateTrackerUI("Thu thập: 0/" + targetItemAmount);
        SpawnQuestItems(); // Hàm này đã được thêm bên dưới
    }

    // ĐÂY LÀ HÀM BẠN ĐANG THIẾU
    void SpawnQuestItems() {
        if (itemPrefab == null || spawnPoints.Length == 0) return;
        for (int i = 0; i < spawnPoints.Length; i++) {
            if (i < targetItemAmount) {
                Instantiate(itemPrefab, spawnPoints[i].position, Quaternion.identity);
            }
        }
    }

    public void AddItem() {
        if (currentState != QuestState.Collecting) return;
        currentItemAmount++;
        UpdateTrackerUI("Thu thập: " + currentItemAmount + "/" + targetItemAmount);
        
        if (currentItemAmount >= targetItemAmount) {
            questTrackerText.text = "Xong! Hãy về gặp NPC";
            questTrackerText.color = Color.yellow;
        }
    }

    // --- LOGIC NHIỆM VỤ 2 ---
    void StartBossQuest() {
        currentState = QuestState.HuntingBoss;
        UpdateTrackerUI("Tiêu diệt BOSS!");
        questTrackerText.color = Color.red;
        
        if (bossPrefab != null && bossSpawnPoint != null) {
            activeBoss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
        }
    }

    void WinGame() {
        currentState = QuestState.Finished;
        questTrackerPanel.SetActive(false);
        if (winUI != null) winUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Time.timeScale = 0; // Mở dòng này nếu muốn dừng game khi thắng
    }

    void UpdateTrackerUI(string text) {
        if (questTrackerText != null) questTrackerText.text = text;
    }

    public void CloseQuest() {
        isTalking = false;
        questPanel.SetActive(false);
        if (currentState != QuestState.Finished) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) { isPlayerInRange = false; CloseQuest(); } }
}