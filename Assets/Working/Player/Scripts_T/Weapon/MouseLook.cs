using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform yaw;
    public Transform pitch;

    public float sensitivity = 120f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    float yawValue;
    float pitchValue;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yawValue = yaw.localEulerAngles.y;
        pitchValue = 0f;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yawValue += mouseX;
        pitchValue -= mouseY;
        pitchValue = Mathf.Clamp(pitchValue, minPitch, maxPitch);

        yaw.localRotation = Quaternion.Euler(0f, yawValue, 0f);
        pitch.localRotation = Quaternion.Euler(pitchValue, 0f, 0f);
    }
}
