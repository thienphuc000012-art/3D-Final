using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] GameObject player;
    void Start()
    {
        player = GameObject.Find("PlayerRuntime");
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            if (player.GetComponent<PlayerRuntime>().sceneIndex == 1)
            {
                StartCoroutine(LoadGameScene());
            }
            else if (player.GetComponent<PlayerRuntime>().sceneIndex == 0)
            {
                StartCoroutine(LoadMenuScene());
            }
        }

    }
    IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator LoadMenuScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Menu");
    }
}
