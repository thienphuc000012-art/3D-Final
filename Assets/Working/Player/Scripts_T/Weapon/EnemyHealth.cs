using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Optional")]
    public GameObject deathVFX;
    public float destroyDelay = 0.2f;

    bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // hiệu ứng chết (nếu có)
        if (deathVFX)
        {
            Instantiate(deathVFX, transform.position, Quaternion.identity);
        }

        // TODO: sau này gọi event cộng exp, tăng score, v.v.

        Destroy(gameObject, destroyDelay);
    }
}
