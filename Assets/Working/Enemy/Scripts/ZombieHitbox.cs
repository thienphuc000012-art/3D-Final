using UnityEngine;

public class ZombieHitbox : MonoBehaviour
{
    public enum HitboxType { Head, Body }
    public HitboxType hitboxType;

    private ZombieMovementWithAnim zombie;

    void Start()
    {
        // Lấy script zombie cha
        zombie = GetComponentInParent<ZombieMovementWithAnim>();
    }

    public void ApplyDamage(int baseDamage)
    {
        if (zombie == null) return;

        int finalDamage = baseDamage;

        // Nếu trúng head thì nhân damage
        if (hitboxType == HitboxType.Head)
        {
            finalDamage = Mathf.RoundToInt(baseDamage * 2f); // headshot x2 damage
        }

        zombie.TakeDamage(finalDamage); // gọi hàm TakeDamage của zombie
        Debug.Log($"{gameObject.name} nhận {finalDamage} damage ({hitboxType})");
    }
}