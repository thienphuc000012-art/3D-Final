using TMPro;
using UnityEngine;

public class UpdateCanvas : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI Ammotext;
    [SerializeField] GameObject player;
    int currentAmmo;
    int maxAmmo;
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
        if (player != null)
        {
            currentAmmo = player.transform.Find("GunLogic").GetComponent<Gun>().currentAmmo;
            maxAmmo = player.transform.Find("GunLogic").GetComponent<Gun>().magazineSize;
        }

        Ammotext.text = currentAmmo + "/" + maxAmmo;

    }

    //_ = PlayerRuntime.Instance.SavePlayer();
    
    
}
