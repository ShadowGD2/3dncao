using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có để chuyển cảnh

public class MainMenu : MonoBehaviour
{
    // Hàm để bắt đầu game
    public void PlayGame()
    {
        // Chuyển sang scene tiếp theo trong danh sách Build Settings
        // Hoặc bạn có thể điền tên Scene cụ thể: SceneManager.LoadScene("Level1");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Hàm để thoát game
    public void QuitGame()
    {
        Debug.Log("Đã thoát game!"); // Dòng này để kiểm tra trong Editor
        Application.Quit(); // Lệnh này chỉ có tác dụng khi đã xuất file (Build)
    }
}