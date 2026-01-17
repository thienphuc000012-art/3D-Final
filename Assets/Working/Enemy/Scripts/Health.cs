using UnityEngine;

public class Health : MonoBehaviour
{
    private int _maxHealth;
    private int _currentHealth;
    public void SetUp(int current, int max)
    {
        _maxHealth = max;
        _currentHealth = current;

    }

    public void TakeDamage(int damageAmount)
    {
        _currentHealth -= damageAmount;
        if (_currentHealth < 0)
        {
            _currentHealth = 0;
            // Handle death logic here
        }
    }
    public void Heal(int healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
    }
    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => _maxHealth;

}
