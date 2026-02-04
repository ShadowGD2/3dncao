using UnityEngine;

public class CrusaderAudioBridge : MonoBehaviour
{
    private Animator anim;
    private AudioSource audioSource;
    private bool wasAttacking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Tự động thêm AudioSource nếu bạn quên chưa gán
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Kiểm tra xem Animator có đang ở trạng thái Attack không
        // Chúng ta kiểm tra tên Tag hoặc tên State trong Animator của bạn
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // Nếu State đang chạy có tên chứa chữ "atack" (giống trong ảnh của bạn)
        if (stateInfo.IsName("atack3")) 
        {
            if (!wasAttacking)
            {
                audioSource.Play();
                wasAttacking = true; // Đánh dấu đã phát âm thanh để không bị lặp
            }
        }
        else
        {
            wasAttacking = false; // Reset khi thoát khỏi trạng thái đánh
        }
    }
}