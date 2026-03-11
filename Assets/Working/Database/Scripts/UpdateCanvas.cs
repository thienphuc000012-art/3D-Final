using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateCanvas : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI Ammotext;
    [SerializeField] private TextMeshProUGUI ExpText;
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
    [SerializeField] TextMeshProUGUI curLvText;
    [SerializeField] TextMeshProUGUI nextDamageText;
    [SerializeField] TextMeshProUGUI nextFireRateText;
    [SerializeField] TextMeshProUGUI nextMagazineText;
    [SerializeField] TextMeshProUGUI nextLvText;

    [SerializeField] Image curGun;
    [SerializeField] Image nextGun;
    [SerializeField] Image mainGun;


    [SerializeField] TextMeshProUGUI feeText;
    [SerializeField] TextMeshProUGUI messageText;


    [SerializeField] GameObject UpgradePanel;
    void Awake()
    {
        player = GameObject.Find("Player_T");

    }
    void Start()
    {
        UpdateGunData();
        StartCoroutine(AutoSaveRoutine());
        Ammotext.text = currentAmmo + "/" + maxAmmo;
        messageText.text = "";
    }

    void Update()
    {
        UpdateGunStats();
        CurGunStatsText();
        NextGunStatsText();
        UpdateGunIcon();
        if (player != null)
        {
            currentAmmo = player.transform.Find("GunLogic").GetComponent<Gun>().currentAmmo;
            maxAmmo = player.transform.Find("GunLogic").GetComponent<Gun>().magazineSize;
            ExpText.text = "Exp: " + PlayerRuntime.Instance.Player.Exp.ToString();
            feeText.text = (PlayerRuntime.Instance.Player.Gun.level *1000).ToString() + " Exp";
        }

        Ammotext.text = currentAmmo + "/" + maxAmmo;    
        
        //if(Input.GetKeyDown(KeyCode.Tab))
        //{
        //    ToggleUpgradePanel();
        //}

    }
    void UpdateGunData()
    {
        if (PlayerRuntime.Instance.Player.Gun.level > 2)
        {
            gunData.level = PlayerRuntime.Instance.Player.Gun.level;
            gunData.damage = PlayerRuntime.Instance.Player.Gun.damage;
            gunData.reloadTime = PlayerRuntime.Instance.Player.Gun.reloadTime;
            gunData.bulletSpeed = PlayerRuntime.Instance.Player.Gun.bulletSpeed;
            gunData.magazineSize = PlayerRuntime.Instance.Player.Gun.magazineSize;
        }
        else
        {            
            gunData.level = 1;
            gunData.damage = 10f;
            gunData.fireRate = 0.1f;
            gunData.reloadTime = 1.5f;
            gunData.bulletSpeed = 60f;
            gunData.magazineSize = 30;
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
        //currentAmmo = magazineSize;        
    }
    void CurGunStatsText()
    {
        curDamageText.text = damage.ToString("F1");
        curFireRateText.text = fireCooldown.ToString("F2") + "s";
        curMagazineText.text = magazineSize.ToString();
        curLvText.text = "Lv. " + level.ToString();
    }
    void NextGunStatsText()
    {
        int lv = level - 1;

        nextDamageText.text = (gunData.damage * (1f + 0.5f * lv)).ToString("F1");
        nextFireRateText.text = (gunData.fireRate * (1f - 0.15f * lv)).ToString("F2") + "s";
        nextMagazineText.text = (gunData.magazineSize + lv * 5).ToString();
        nextLvText.text = "Lv. " + (level + 1).ToString();
    }

    void UpdateGunIcon()
    {
        Sprite gun1 = Resources.Load<Sprite>(gunData.level.ToString());
        Sprite gun2 = Resources.Load<Sprite>((gunData.level + 1).ToString());
        mainGun.sprite = gun1;
        curGun.sprite = gun1;
        nextGun.sprite = gun2;
    }    

    public void ToggleUpgradePanel()
    {
        UpgradePanel.SetActive(!UpgradePanel.activeSelf);
    }


    //_ = PlayerRuntime.Instance.SavePlayer();

    IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            PlayerRuntime.Instance.Player.Gun = GunRuntime.FromData(gunData);
            _ = PlayerRuntime.Instance.SavePlayer();
            yield return new WaitForSeconds(1f);
        }
    }

    public void UpgradeClick()
    {
        if (PlayerRuntime.Instance.Player.Exp >= PlayerRuntime.Instance.Player.Gun.level * 1000)
        {
            player.transform.Find("GunLogic").GetComponent<Gun>().UpgradeGun();
            PlayerRuntime.Instance.Player.Exp -= PlayerRuntime.Instance.Player.Gun.level * 1000;
            messageText.text = "Upgrade successful!";
            StartCoroutine(ClearMessage());
            
        }
        else
        {
            messageText.text = "Not enough!";
            StartCoroutine(ClearMessage());
        }
    }
    IEnumerator ClearMessage()
    {
        yield return new WaitForSeconds(2f);
        messageText.text = "";
    }

}
