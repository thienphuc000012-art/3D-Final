using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform yawTransform;
    public Transform pitchTransform;
    public Camera cam;

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

    bool isCursorLocked = true;

    void Start()
    {
        LockCursor(true);

        yaw = yawTransform.localEulerAngles.y;

        if (yaw > 180f)
            yaw -= 360f;
    }

    void Update()
    {
        // ===== TOGGLE CURSOR =====
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    LockCursor(!isCursorLocked);
        //}

        // Nếu chuột đang mở thì không xoay camera
        if (!isCursorLocked)
            return;

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

    public void LockCursor(bool state)
    {
        isCursorLocked = state;

        if (state)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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


    public bool IsCursorLocked()
    {
        return isCursorLocked;
    }
}