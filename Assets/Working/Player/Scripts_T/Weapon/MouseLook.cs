using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform yawTransform;     // Player_T / Yaw
    public Transform pitchTransform;   // Player_T / Yaw / Pitch
    public Camera cam;                 // Main Camera

    [Header("Settings")]
    public float mouseSensitivity = 2f;

    [Header("Vertical Clamp")]
    public float minPitch = -45f;
    public float maxPitch = 45f;

    [Header("Horizontal Clamp")]
    public float minYaw = -90f;
    public float maxYaw = 90f;

    float pitch;
    float yaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = yawTransform.localEulerAngles.y;
        pitch = pitchTransform.localEulerAngles.x;

        if (yaw > 180f) yaw -= 360f;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ===== YAW =====
        yaw += mouseX;
        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        yawTransform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        // ===== PITCH =====
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        pitchTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // ===== FIX ROLL =====
        Vector3 camEuler = cam.transform.localEulerAngles;
        camEuler.z = 0f;
        cam.transform.localEulerAngles = camEuler;
    }

    public void AddRecoil(float up, float side)
    {
        pitch -= up;
        yaw += Random.Range(-side, side);

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);

        yawTransform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        pitchTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}