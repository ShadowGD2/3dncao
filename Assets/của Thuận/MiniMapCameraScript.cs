using UnityEngine;
public class MiniMapCameraScript : MonoBehaviour
{
    public Transform player;
    void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y; // giữ nguyên độ cao camera
        transform.position = newPosition;
    }
}