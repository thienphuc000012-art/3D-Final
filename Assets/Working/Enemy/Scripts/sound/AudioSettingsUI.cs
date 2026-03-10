using UnityEngine;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject settingsPanel;

    [Header("Managers")]
    public AudioSettingsManager audioSettingsManager;
    [Header("Other UI")]
    public ZombieUIManager zombieUIManager;
    public GameObject zombieListButton;

    private bool isOpen = false;

    public void ToggleSettings()
    {
        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);

        if (isOpen)
        {
            if (zombieUIManager != null)
                zombieUIManager.CloseZombieList();
            if (zombieListButton != null)
                zombieListButton.SetActive(false);

            Time.timeScale = 0f;
        }
        else
        {
            if (zombieListButton != null)
                zombieListButton.SetActive(true);

            Time.timeScale = 1f;
        }
    }

    public void ExitSettings()
    {
        isOpen = false;
        settingsPanel.SetActive(false);
        if (zombieListButton != null)
            zombieListButton.SetActive(true);

        Time.timeScale = 1f;
    }
}