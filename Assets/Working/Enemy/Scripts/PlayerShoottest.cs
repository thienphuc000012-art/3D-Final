using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoottest : MonoBehaviour
{
    public Camera cam;
    public int damage = 10;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ZombieHitbox hitbox = hit.collider.GetComponent<ZombieHitbox>();
                if (hitbox != null)
                {
                    // ✅ truyền thêm hit.point để spawn máu đúng chỗ
                    hitbox.ApplyDamage(damage, hit.point);
                    //Debug.Log("Bắn trúng " + hitbox.hitboxType + " tại " + hit.point);
                }
            }
        }
    }
}