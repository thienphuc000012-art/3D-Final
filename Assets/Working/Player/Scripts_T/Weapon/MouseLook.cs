using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;   // ModelPlayer
    public float sensitivity = 120f;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

     

        // Y: body xoay trái/phải
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}
