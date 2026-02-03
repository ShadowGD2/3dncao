using UnityEngine;
using System.Collections;

public class LightningSystem : MonoBehaviour
{
    [Header("Liên kết")]
    public DayNightCycle dayNightController;
    
    [Header("Cấu hình Đèn Sấm Sét")]
    public Light lightningLight; 
    public float minIntensity = 2f;
    public float maxIntensity = 5f;

    [Header("Cấu hình Bầu Trời (MỚI)")]
    public Material skyboxMaterial; // Kéo Material Skybox vào đây
    public float flashExposure = 3f; // Độ sáng bầu trời khi chớp (thường là 2-5)
    private float defaultExposure = 0f; // Độ sáng mặc định ban đêm (thường là 0)

    [Header("Cấu hình Thời gian")]
    public float minTimeBetweenStrikes = 5f;  
    public float maxTimeBetweenStrikes = 20f; 
    public float flashDuration = 0.1f;       

    [Header("Cấu hình Âm thanh")]
    public AudioSource thunderAudio; 
    public AudioClip[] thunderSounds; 

    private void Start()
    {
        if (lightningLight != null) lightningLight.enabled = false;
        
        // Tự lấy skybox nếu chưa gán
        if (skyboxMaterial == null) skyboxMaterial = RenderSettings.skybox;

        StartCoroutine(LightningRoutine());
    }

    IEnumerator LightningRoutine()
    {
        while (true) 
        {
            // 1. Kiểm tra ban ngày (Nếu trời sáng thì không đánh)
            if (dayNightController != null && dayNightController.sunLight.intensity > 0.1f)
            {
                yield return new WaitForSeconds(1f);
                continue; 
            }

            // 2. Chờ ngẫu nhiên
            float waitTime = Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes);
            yield return new WaitForSeconds(waitTime);

            // 3. Đánh sét
            int flashCount = Random.Range(1, 4); 

            for (int i = 0; i < flashCount; i++)
            {
                yield return DoFlash();
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f)); 
            }

            // 4. Âm thanh
            if (thunderAudio != null && thunderSounds.Length > 0)
            {
                AudioClip clip = thunderSounds[Random.Range(0, thunderSounds.Length)];
                thunderAudio.pitch = Random.Range(0.9f, 1.1f);
                thunderAudio.PlayOneShot(clip);
            }
        }
    }

    // Hàm chớp (Đã nâng cấp)
    IEnumerator DoFlash()
    {
        // A. BẬT ĐÈN (Chiếu sáng đất)
        if (lightningLight != null)
        {
            lightningLight.enabled = true;
            lightningLight.intensity = Random.Range(minIntensity, maxIntensity);
        }

        // B. BẬT SKYBOX (Chiếu sáng trời - QUAN TRỌNG)
        if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Exposure"))
        {
            // Lưu lại độ sáng cũ (đang là tối)
            float currentExposure = skyboxMaterial.GetFloat("_Exposure");
            
            // Kích độ sáng lên cao (Flash)
            skyboxMaterial.SetFloat("_Exposure", flashExposure);
            
            // Chờ tích tắc
            yield return new WaitForSeconds(flashDuration);
            
            // Trả lại độ sáng cũ (Tối lại)
            skyboxMaterial.SetFloat("_Exposure", currentExposure);
        }
        else
        {
            // Nếu không có skybox thì chỉ chờ thôi
            yield return new WaitForSeconds(flashDuration);
        }
        
        // C. TẮT ĐÈN
        if (lightningLight != null)
        {
            lightningLight.enabled = false;
        }
    }
}