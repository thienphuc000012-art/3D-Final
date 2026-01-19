using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Gun Data")]
public class GunData : ScriptableObject
{
    public string gunName;

    public float damage = 10f;
    public float fireRate = 0.1f;
    public float reloadTime = 1.5f;
    public float bulletSpeed = 60f;

    public int magazineSize = 30;
    public int maxAmmo = 120;
}
