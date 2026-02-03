using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Thời gian")]
    public float dayDuration = 60f; // Độ dài 1 ngày (giây)
    [Range(0, 24)] public float timeOfDay = 12f; // Giờ hiện tại

    [Header("Mặt trời (Đèn)")]
    public Light sunLight;
    public float maxIntensity = 1.5f;
    public Gradient sunColor;

    [Header("Bầu trời (Skybox)")]
    public Material skyboxMaterial; // Kéo Material của Skybox vào đây
    public Gradient skyTint; // Màu ám của bầu trời (Trưa xanh, Chiều cam, Tối đen)
    
    // Biến để lưu giá trị gốc của Skybox (để tránh làm hỏng file gốc khi tắt game)
    private float defaultExposure;
    private Color defaultTint;

    void Start()
    {
        // Tự động lấy Skybox đang dùng trong RenderSettings nếu chưa gán
        if (skyboxMaterial == null)
        {
            skyboxMaterial = RenderSettings.skybox;
        }

        // Lưu lại giá trị gốc (nếu là Procedural Skybox)
        if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Exposure"))
        {
            defaultExposure = skyboxMaterial.GetFloat("_Exposure");
            defaultTint = skyboxMaterial.GetColor("_SkyTint");
        }
    }

    void Update()
    {
        // 1. Tính thời gian
        timeOfDay += Time.deltaTime / dayDuration * 24f;
        if (timeOfDay >= 24) timeOfDay = 0;

        // 2. Xoay Mặt trời
        float sunRotationX = (timeOfDay / 24f) * 360f - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunRotationX, 170f, 0);

        // 3. Tính toán độ cao mặt trời (Dot Product)
        // 1 = Đỉnh đầu (Trưa), 0 = Chân trời (Hoàng hôn), -1 = Dưới chân (Đêm)
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);

        UpdateLighting(dotProduct);
        UpdateSkybox(dotProduct);
    }

    void UpdateLighting(float dotProduct)
    {
        // Chỉnh độ sáng đèn
        sunLight.intensity = Mathf.Lerp(0, maxIntensity, dotProduct);
        
        // Chỉnh màu đèn
        if (sunColor != null)
            sunLight.color = sunColor.Evaluate(dotProduct);

        // Bật/Tắt đèn khi đêm xuống để tiết kiệm hiệu năng
        sunLight.enabled = dotProduct > 0.1f;
    }

    void UpdateSkybox(float dotProduct)
    {
        if (skyboxMaterial == null) return;

        // Chỉ chỉnh Skybox nếu nó là loại "Procedural" (Mặc định của Unity)
        if (skyboxMaterial.HasProperty("_Exposure"))
        {
            // A. Chỉnh độ sáng (Exposure)
            // Khi đêm (dotProduct < 0), giảm Exposure về 0 để trời tối đen
            // Mathf.Clamp01 để giữ giá trị từ 0 đến 1
            float exposure = Mathf.Clamp01(dotProduct); 
            skyboxMaterial.SetFloat("_Exposure", exposure);

            // B. Chỉnh màu trời (Tint)
            if (skyTint != null)
            {
                // Dùng dotProduct để lấy màu từ Gradient
                // Cần Clamp giá trị dotProduct về 0-1 vì Gradient không nhận số âm
                Color currentSkyColor = skyTint.Evaluate(Mathf.Clamp01(dotProduct));
                skyboxMaterial.SetColor("_SkyTint", currentSkyColor);
            }
            
            // C. Chỉnh sương mù (Fog) cho đồng bộ (Tùy chọn)
            RenderSettings.fogColor = sunLight.color;
        }
    }

    // Khi tắt game, trả lại giá trị gốc cho Skybox (để không bị tối thui mãi mãi trong Editor)
    void OnApplicationQuit()
    {
        if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Exposure"))
        {
            skyboxMaterial.SetFloat("_Exposure", defaultExposure);
            skyboxMaterial.SetColor("_SkyTint", defaultTint);
        }
    }
}