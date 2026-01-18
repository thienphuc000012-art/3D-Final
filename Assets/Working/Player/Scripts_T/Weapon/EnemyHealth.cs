using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float hp = 50f;

    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
