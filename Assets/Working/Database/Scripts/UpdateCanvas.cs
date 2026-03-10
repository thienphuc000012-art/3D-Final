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
    public int magazineSize;

    int level = 1;

    [Header("Upgrade")]
    [SerializeField] TextMeshProUGUI curDamageText;
    [SerializeField] TextMeshProUGUI curFireRateText;
    [SerializeField] TextMeshProUGUI curMagazineText;
    [SerializeField] TextMeshProUGUI nextDamageText;
    [SerializeField] TextMeshProUGUI nextFireRateText;
    [SerializeField] TextMeshProUGUI nextMagazineText;

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
        if (player != null)
        {
            currentAmmo = player.transform.Find("GunLogic").GetComponent<Gun>().currentAmmo;
            maxAmmo = player.transform.Find("GunLogic").GetComponent<Gun>().magazineSize;
        }

        Ammotext.text = currentAmmo + "/" + maxAmmo;

    }

    void UpdateGunStats()
    {
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

        damage = gunData.damage * (1f + 0.5f * lv);
        fireCooldown = gunData.fireRate * (1f - 0.15f * lv);
        bulletSpeed = gunData.bulletSpeed * (1f + 0.1f * lv);
        magazineSize = gunData.magazineSize + lv * 5;

        currentAmmo = magazineSize;
    }

        //_ = PlayerRuntime.Instance.SavePlayer();



    //damage = gunData.damage;
    //    fireCooldown = gunData.fireRate;
    //    reloadTime = gunData.reloadTime;
    //    bulletSpeed = gunData.bulletSpeed;
    //    magazineSize = gunData.magazineSize;
    //    currentAmmo = magazineSize;
    }
