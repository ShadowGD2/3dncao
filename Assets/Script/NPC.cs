using UnityEngine;
using TMPro; // Bắt buộc phải có để dùng TextMeshPro
using UnityEngine.UI;

public class NPCQuest : MonoBehaviour {
    [Header("Cấu hình UI")]
    public GameObject questPanel;
    public TextMeshProUGUI questText; // Kéo object questText (TMP) vào đây

    [Header("Danh sách lời thoại")]
    [TextArea(3, 10)]
    public string[] dialogues; // Nhập các câu thoại vào danh sách này
    
    [Header("Lời thoại khi Từ chối")]
    public string declineMessage = "Tiếc quá, khi nào rảnh hãy quay lại nhé!";

    private int currentLine = 0;
    private bool isPlayerInRange = false;
    private bool isTalking = false;

    void Start() {
        // Đảm bảo bảng ẩn khi bắt đầu game
        if (questPanel != null) questPanel.SetActive(false);
    }

    void Update() {
        // Nhấn F để bắt đầu hoặc tiếp tục hội thoại bằng phím
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F)) {
            if (!isTalking) {
                StartDialogue();
            } else {
                AcceptQuest(); // Nhấn F tương đương nhấn nút Chấp nhận
            }
        }
    }

    void StartDialogue() {
        if (dialogues.Length == 0) return;
        
        isTalking = true;
        currentLine = 0;
        questPanel.SetActive(true);
        DisplayLine();
        
        // Hiện chuột để tương tác với UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void DisplayLine() {
        if (questText != null && currentLine < dialogues.Length) {
            questText.text = dialogues[currentLine];
        }
    }

    // Gắn hàm này vào nút ACCEPT (Chấp nhận)
    public void AcceptQuest() {
        currentLine++; 
        
        if (currentLine < dialogues.Length) {
            DisplayLine(); // Chuyển sang câu tiếp theo (Mess 2, 3...)
        } else {
            Debug.Log("Hoàn thành hội thoại - Bắt đầu nhiệm vụ!");
            CloseQuest();
        }
    }

    // Gắn hàm này vào nút DECLINE (Từ chối)
    public void DeclineQuest() {
        if (questText != null) {
            questText.text = declineMessage;
            // Tự động đóng sau 1.5 giây
            Invoke("CloseQuest", 1.5f);
        }
    }

    public void CloseQuest() {
        isTalking = false;
        if (questPanel != null) questPanel.SetActive(false);
        
        // Khóa chuột lại để tiếp tục chơi
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Nhận diện người chơi
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = false;
            CloseQuest();
        }
    }
}