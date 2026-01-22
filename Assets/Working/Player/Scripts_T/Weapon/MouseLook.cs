using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform pitch;
    public float sensitivity = 150f;
    public float minPitch = -60f;
    public float maxPitch = 60f;

    float yaw;
    float pitchRot;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yaw += mouseX;
        pitchRot -= mouseY;
        pitchRot = Mathf.Clamp(pitchRot, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        pitch.localRotation = Quaternion.Euler(pitchRot, 0f, 0f);
    }
}
