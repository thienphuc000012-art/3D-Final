using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody; // Kéo Player_T vào đây
    public float sensitivity = 200f;
    float xRotation = 0f;

    void Start()
    {
        // Khóa chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
        // Ẩn con trỏ chuột đi
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // 1. Xoay Lên/Xuống: Tác động vào chính CameraHolder (chứa Camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 2. Xoay Trái/Phải: Tác động vào Player_T
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}