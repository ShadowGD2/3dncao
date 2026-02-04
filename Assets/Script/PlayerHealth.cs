using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Thêm thư viện này để load lại cảnh

public class PlayerHealth : MonoBehaviour 
{
    public int health = 100;
    public TextMeshProUGUI healthText;
    public GameObject gameOverPanel; // Kéo GameOverPanel vào đây

    void Start() 
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // Đảm bảo ẩn lúc đầu
        UpdateUI(); 
    }

    public void TakeDamage(int amount) 
    {
        if (health <= 0) return;

        health -= amount;
        if (health < 0) health = 0;
        UpdateUI();

        if (health <= 0) 
        {
            ShowGameOver();
        }
    }

    void UpdateUI() 
    {
        if (healthText != null) healthText.text = "HP: " + health;
    }

    void ShowGameOver() 
    {
        if (gameOverPanel != null) 
        {
            gameOverPanel.SetActive(true); // Hiện bảng Game Over
            Time.timeScale = 0f; // Dừng toàn bộ game lại
            Cursor.lockState = CursorLockMode.None; // Hiện chuột để bấm nút
            Cursor.visible = true;
        }
    }

    // Hàm để gắn vào nút Restart (nếu bạn có tạo nút)
public void RestartGame() 
{
    // Cực kỳ quan trọng: Phải trả thời gian về 1 trước khi load lại
    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
}
}