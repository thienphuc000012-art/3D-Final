using TMPro;
using UnityEngine;

public class UpdateCanvas : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI Ammotext;
    [SerializeField] GameObject player;

    public GunData gunData;
    int currentAmmo;
    int maxAmmo;

    float damage, fireCooldown, reloadTime, bulletSpeed;
    int magazineSize, level;    

    [Header("Upgrade")]
    [SerializeField] TextMeshProUGUI curDamageText;
    [SerializeField] TextMeshProUGUI curFireRateText;
    [SerializeField] TextMeshProUGUI curMagazineText;
    [SerializeField] TextMeshProUGUI nextDamageText;
    [SerializeField] TextMeshProUGUI nextFireRateText;
    [SerializeField] TextMeshProUGUI nextMagazineText;


    [SerializeField] GameObject UpgradePanel;
    void Awake()
    {
        player = GameObject.Find("Player_T");

    }
    void Start()
    {
        Ammotext.text = currentAmmo + "/" + maxAmmo;
    }

    void Update()
    {
        UpdateGunStats();
        CurGunStatsText();
        NextGunStatsText();
        if (player != null)
        {
            currentAmmo = player.transform.Find("GunLogic").GetComponent<Gun>().currentAmmo;
            maxAmmo = player.transform.Find("GunLogic").GetComponent<Gun>().magazineSize;
        }

        Ammotext.text = currentAmmo + "/" + maxAmmo;    
        
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleUpgradePanel();
        }

    }

    void UpdateGunStats()
    {
        level = gunData.level;
        damage = gunData.damage;
        fireCooldown = gunData.fireRate;
        reloadTime = gunData.reloadTime;
        bulletSpeed = gunData.bulletSpeed;
        magazineSize = gunData.magazineSize;
        currentAmmo = magazineSize;
    }
    void CurGunStatsText()
    {
        curDamageText.text = damage.ToString("F1");
        curFireRateText.text = fireCooldown.ToString("F2") + "s";
        curMagazineText.text = magazineSize.ToString();
    }
    void NextGunStatsText()
    {
        int lv = level - 1;

        nextDamageText.text = (gunData.damage * (1f + 0.5f * lv)).ToString("F1");
        nextFireRateText.text = (gunData.fireRate * (1f - 0.15f * lv)).ToString("F2") + "s";
        nextMagazineText.text = (gunData.magazineSize + lv * 5).ToString();

        
    }

    public void ToggleUpgradePanel()
    {
        UpgradePanel.SetActive(!UpgradePanel.activeSelf);
    }


        //_ = PlayerRuntime.Instance.SavePlayer();

}
