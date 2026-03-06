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

    public void ApplyDamage(int baseDamage, Vector3 hitPoint)
    {
        if (zombie == null) return;

        int finalDamage = baseDamage;

        if (hitboxType == HitboxType.Head)
        {
            finalDamage = Mathf.RoundToInt(baseDamage * 2f); // headshot x2 damage
        }

        zombie.TakeDamage(finalDamage, hitPoint); // ✅ truyền hitPoint vào zombie
       // Debug.Log($"{gameObject.name} nhận {finalDamage} damage ({hitboxType}) tại {hitPoint}");
    }

}
