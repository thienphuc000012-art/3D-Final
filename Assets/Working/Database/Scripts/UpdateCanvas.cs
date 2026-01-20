using TMPro;
using UnityEngine;

public class UpdateCanvas : MonoBehaviour
{
    GameObject canvas;
    GameObject InputIngameNamePanel;
    GameObject MenuPanel;
    public GameObject PlayerCam;

    void Awake()
    {
        
        
    }
    void Start()
    {
        canvas = GameObject.Find("Canvas").gameObject;
        InputIngameNamePanel = canvas.transform.Find("IngameNameInputPanel").gameObject;
        MenuPanel = canvas.transform.Find("MenuPanel").gameObject;
        Debug.Log("PlayerRuntime.Instance.name: " + PlayerRuntime.Instance.Player.Name);
        if (PlayerRuntime.Instance.Player.Name != "NewPlayer")
        {
            InputIngameNamePanel.SetActive(false);
            PlayerCam.SetActive(true);
        }
        else
        {
            InputIngameNamePanel.SetActive(true);
        }
    }

    void Update()
    {
        
    }


    public void InputIngameNameButtonClick()
    {
        PlayerRuntime.Instance.Player.Name = InputIngameNamePanel.transform.Find("InputIngameName").GetComponent<TMP_InputField>().text;
        InputIngameNamePanel.SetActive(false);
        _ = PlayerRuntime.Instance.SavePlayer();
        Debug.Log("name after input: " + PlayerRuntime.Instance.Player.Name);
        MenuPanel.SetActive(true);
    }

    public void PlayButtonClick()
    {
        MenuPanel.SetActive(false);
        PlayerCam.SetActive(true);
    }
}
