using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ZombieUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject zombieListPanel; 
    public GameObject infoPanel;       
    public TMP_Text nameText;
    public TMP_Text statsText;
    public TMP_Text descriptionText;
    public Image zombieImage;

    [Header("Background")]
    public GameObject backgroundImage; 

    [Header("Icon Prefab")]
    public GameObject zombieIconPrefab; 

    private List<GameObject> spawnedIcons = new List<GameObject>();

    void Start()
    {
        zombieListPanel.SetActive(false);
        infoPanel.SetActive(false);
    }
    void Update()
    {
        
        if (zombieListPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseZombieList();
        }
    }
    public void ToggleZombieList()
    {
        bool isActive = !zombieListPanel.activeSelf;
        zombieListPanel.SetActive(isActive);
        backgroundImage.SetActive(isActive); 

        if (isActive)
        {
            Time.timeScale = 0f; 
        }
        else
        {
            Time.timeScale = 1f; 
        }
    }



    public void AddZombieIcon(ZombieData data)
    {
        foreach (var icon in spawnedIcons)
        {
            ZombieIcon iconScript = icon.GetComponent<ZombieIcon>();
            if (iconScript != null && iconScript.zombieData == data)
                return; 
        }

       
        GameObject newIcon = Instantiate(zombieIconPrefab, zombieListPanel.transform);
        Image iconImage = newIcon.GetComponent<Image>();
        iconImage.sprite = data.zombieSprite;
        iconImage.color = Color.white;

       
        ZombieIcon iconScriptNew = newIcon.GetComponent<ZombieIcon>();
        iconScriptNew.zombieData = data;
        iconScriptNew.uiManager = this;

        spawnedIcons.Add(newIcon);
    }

    public void ShowZombieInfoData(ZombieData data)
    {
        infoPanel.SetActive(true);
        nameText.text = data.zombieName;
        statsText.text = $"Máu: {data.maxHealth}\nTốc độ: {data.moveSpeed}\nSát thương: {data.damage}";
        descriptionText.text = data.description;
        zombieImage.sprite = data.zombieSprite;
    }

    public void HideZombieInfo()
    {
        infoPanel.SetActive(false);
    }

    public void CloseZombieList()
    {
        zombieListPanel.SetActive(false);
        infoPanel.SetActive(false);
        backgroundImage.SetActive(false);
        Time.timeScale = 1f;
    }

}