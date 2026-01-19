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
                    hitbox.ApplyDamage(damage);
                    Debug.Log("Bắn trúng " + hitbox.hitboxType);
                }
            }
        }
    }
}