using UnityEngine;

public class ZombieHitbox : MonoBehaviour
{
    public enum HitboxType { Head, Body }
    public HitboxType hitboxType;

    private Health health;

    void Start()
    {
        // Lấy Health từ zombie cha
        health = GetComponentInParent<Health>();
    }

    public void ApplyDamage(int baseDamage)
    {
        if (health == null) return;

        int finalDamage = baseDamage;

        // Nếu trúng head thì nhân damage
        if (hitboxType == HitboxType.Head)
        {
            finalDamage = Mathf.RoundToInt(baseDamage * 2f); // headshot x2 damage
        }

        health.TakeDamage(finalDamage);
        Debug.Log($"{gameObject.name} nhận {finalDamage} damage ({hitboxType})");
    }

    // Ví dụ khi player bắn raycast trúng collider này
    private void OnMouseDown()
    {
        // Test: click chuột vào collider để gây damage
        ApplyDamage(10);
    }
}