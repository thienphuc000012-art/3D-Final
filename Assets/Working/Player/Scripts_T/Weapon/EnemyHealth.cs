using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float maxHP = 50f;
    float currentHP;
    public float expDrop = 25f; // mỗi enemy cho bao nhiêu exp


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
        // cộng EXP cho gun
        Gun gun = FindObjectOfType<Gun>();
        if (gun)
            gun.AddExp(expDrop);

        // giữ lại nếu bạn vẫn muốn test wave
        if (WaveManager.Instance != null)
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
