using UnityEngine;

[System.Serializable]
public class GunRuntime
{
    public string gunName;

    public float damage;
    public float fireRate;
    public float reloadTime;
    public float bulletSpeed;

    public int magazineSize;
    public int maxAmmo;

    public static GunRuntime FromData(GunData data)
    {
        return new GunRuntime
        {
            gunName = data.gunName,
            damage = data.damage,
            fireRate = data.fireRate,
            reloadTime = data.reloadTime,
            bulletSpeed = data.bulletSpeed,
            magazineSize = data.magazineSize,
            maxAmmo = data.maxAmmo
        };
        
    }
}
