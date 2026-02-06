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
    public string declineMessage = "Tiếc quá! Hẹn gặp lại bạn sau nhé.";

    private int currentLine = 0;
    private bool isPlayerInRange = false;
    private bool isTalking = false;

    void Start() {
        if (questPanel != null) questPanel.SetActive(false);
        if (questTrackerPanel != null) questTrackerPanel.SetActive(false);
        if (winUI != null) winUI.SetActive(false);
    }

    void Update() {
        // Nhấn F để nói chuyện
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F) && !isTalking) {
            StartDialogue();
        }

        // KIỂM TRA THẮNG GAME: Nếu đang săn Boss và Boss đã bị Destroy (null)
        if (currentState == QuestState.HuntingBoss && activeBoss == null && questTrackerPanel != null && questTrackerPanel.activeSelf) {
            WinGame();
        }
    }

    void StartDialogue() {
        isTalking = true;
        currentLine = 0;
        if (questPanel != null) questPanel.SetActive(true);
        DisplayLine();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void DisplayLine() {
        string[] currentLines = (currentState == QuestState.NotStarted) ? startDialogues : bossDialogues;
        if (questText != null && currentLine < currentLines.Length) {
            questText.text = currentLines[currentLine];
        }
    }

    public void AcceptQuest() {
        string[] currentLines = (currentState == QuestState.NotStarted) ? startDialogues : bossDialogues;
        currentLine++;

        if (currentLine < currentLines.Length) {
            DisplayLine();
        } else {
            // Quyết định bắt đầu NV1 hoặc NV2 dựa trên trạng thái
            if (currentState == QuestState.NotStarted) {
                StartCollectQuest();
            } 
            else if (currentState == QuestState.Collecting && currentItemAmount >= targetItemAmount) {
                StartBossQuest();
            }
            
            CloseQuest();
        }
    }

    public void DeclineQuest() {
        if (questText != null) questText.text = declineMessage;
        Invoke("CloseQuest", 1.5f);
    }

    // --- LOGIC NHIỆM VỤ 1: THU THẬP ---
    void StartCollectQuest() {
        currentState = QuestState.Collecting;
        if (questTrackerPanel != null) questTrackerPanel.SetActive(true);
        UpdateTrackerUI("Thu thập: 0/" + targetItemAmount);
        SpawnQuestItems();
    }

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
            if (questTrackerText != null) {
                questTrackerText.text = "Xong! Hãy về gặp NPC";
                questTrackerText.color = Color.yellow;
            }
        }
    }

    // --- LOGIC NHIỆM VỤ 2: ĐÁNH BOSS ---
    void StartBossQuest() {
        currentState = QuestState.HuntingBoss;
        UpdateTrackerUI("Tiêu diệt BOSS!");
        if (questTrackerText != null) questTrackerText.color = Color.red;
        
        if (bossPrefab != null && bossSpawnPoint != null) {
            activeBoss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            // Gán Tag Boss để script EnemyController biết đây là Boss
            activeBoss.tag = "Boss"; 
        }
    }

    void WinGame() {
        currentState = QuestState.Finished;
        if (questTrackerPanel != null) questTrackerPanel.SetActive(false);
        if (winUI != null) winUI.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Time.timeScale = 0; // Bỏ comment nếu muốn dừng game khi thắng
    }

    void UpdateTrackerUI(string text) {
        if (questTrackerText != null) questTrackerText.text = text;
    }

    public void CloseQuest() {
        isTalking = false;
        if (questPanel != null) questPanel.SetActive(false);
        if (currentState != QuestState.Finished) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) { isPlayerInRange = false; CloseQuest(); } }
}