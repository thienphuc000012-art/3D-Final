using UnityEngine;

public class ViewModelFollowCamera : MonoBehaviour
{
    [Header("References")]
    public Camera aimCamera;

    [Header("Settings")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    void LateUpdate()
    {
        if (aimCamera == null) return;

        // Follow camera rotation
        transform.rotation =
            aimCamera.transform.rotation * Quaternion.Euler(rotationOffset);

        // Optional: giữ position offset
        transform.position =
            aimCamera.transform.position +
            aimCamera.transform.TransformDirection(positionOffset);
    }
}
