using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform yawTransform;     // Player_T / Yaw
    public Transform pitchTransform;   // Player_T / Yaw / Pitch
    public Camera cam;                 // Main Camera

    [Header("Settings")]
    public float mouseSensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ===== YAW (xoay người) =====
        yawTransform.Rotate(Vector3.up * mouseX);

        // ===== PITCH (nhìn lên / xuống) =====
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // CHỈ XOAY TRỤC X — KHÓA Y & Z
        pitchTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // ===== FIX NGHIÊNG (ROLL) – CỰC KỲ QUAN TRỌNG =====
        Vector3 camEuler = cam.transform.localEulerAngles;
        camEuler.z = 0f;
        cam.transform.localEulerAngles = camEuler;
    }
}
