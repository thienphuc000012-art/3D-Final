using UnityEngine;
using System; // cần để dùng Action

public class Health : MonoBehaviour
{
    private int _maxHealth;
    private int _currentHealth;

    // ✅ Khai báo sự kiện OnDeath
    public event Action OnDeath;

    public void SetUp(int current, int max)
    {
        _maxHealth = max;
        _currentHealth = current;
    }

    public void TakeDamage(int damageAmount)
    {
        _currentHealth -= damageAmount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => _maxHealth;

    private void Die()
    {
        // ✅ Gọi sự kiện OnDeath khi chết
        OnDeath?.Invoke();

        // Nếu muốn hủy object sau khi chết
        Destroy(gameObject, 3f);
    }
}