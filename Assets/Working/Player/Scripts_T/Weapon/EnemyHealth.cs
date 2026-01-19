using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float maxHP = 50f;
    float currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        WaveManager.Instance.OnEnemyKilled();
        Invoke(nameof(Respawn), 1.5f);
        gameObject.SetActive(false);
    }

    void Respawn()
    {
        currentHP = maxHP;
        gameObject.SetActive(true);
    }
}
