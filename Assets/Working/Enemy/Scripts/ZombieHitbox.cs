using UnityEngine;

public class ZombieHitbox : MonoBehaviour
{
    public enum HitboxType { Head, Body }
    public HitboxType hitboxType;

    private ZombieMovementWithAnim zombie;

    [Header("Headshot Effects")]
    public ParticleSystem headshotParticles;   // hiệu ứng particle


    void Start()
    {
        zombie = GetComponentInParent<ZombieMovementWithAnim>();
    }

    public void ApplyDamage(int baseDamage, Vector3 hitPoint)
    {
        if (zombie == null) return;

        int finalDamage = baseDamage;

        if (hitboxType == HitboxType.Head)
        {
            finalDamage = Mathf.RoundToInt(baseDamage * 2f); // headshot x2 damage

  
            if (headshotParticles != null)
            {
                ParticleSystem ps = Instantiate(headshotParticles, hitPoint, Quaternion.identity);
                ps.Play();
                Destroy(ps.gameObject, ps.main.duration);
            }
        }

        zombie.TakeDamage(finalDamage, hitPoint);
    }
}