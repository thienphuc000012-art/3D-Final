using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Settings")]
    public Slider healthSlider;
    public Image healthBarImage;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    [Header("Audio Settings")]
    public AudioSource musicSource;
    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private void HandleDeath()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // ✅ Tắt nhạc nền
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();

        // ✅ Tắt toàn bộ âm thanh zombie
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopAllZombieSounds();

        Time.timeScale = 0f;
    }
}