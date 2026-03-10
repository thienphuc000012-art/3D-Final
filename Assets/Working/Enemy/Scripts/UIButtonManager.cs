using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonManager : MonoBehaviour
{

    void Start()
    {

    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Login"); 
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}