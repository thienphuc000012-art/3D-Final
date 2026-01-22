using UnityEngine;

public class YawLimiter : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float maxYaw = 60f;

    float currentYaw = 0f;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        currentYaw += mouseX;
        currentYaw = Mathf.Clamp(currentYaw, -maxYaw, maxYaw);

        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
    }
}
