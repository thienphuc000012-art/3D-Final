using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public GunData gunData;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public ParticleSystem muzzleFlash;

    public Color bulletColor = Color.red;
    public Color[] waveBulletColors;

    int currentAmmo;
    int reserveAmmo;
    float nextFireTime;
    bool isReloading;

    // Runtime stats (không thay đổi gunData gốc)
    float currentDamage;
    float currentFireRate;
    float currentReloadTime;
    float currentBulletSpeed;

    void Start()
    {
        ResetGunStats(); // set stats base
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxAmmo;

        if (muzzleFlash) { muzzleFlash.Stop(); muzzleFlash.Clear(); }
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            Shoot();

        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Reload());
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        nextFireTime = Time.time + currentFireRate;
        currentAmmo--;

        if (muzzleFlash) muzzleFlash.Play();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.damage = currentDamage;
            b.SetBulletSpeed(currentBulletSpeed);
            b.SetColor(bulletColor);
        }
    }

    IEnumerator Reload()
    {
        if (reserveAmmo <= 0 || currentAmmo == gunData.magazineSize)
            yield break;

        isReloading = true;
        yield return new WaitForSeconds(currentReloadTime);

        int need = gunData.magazineSize - currentAmmo;
        int load = Mathf.Min(need, reserveAmmo);

        currentAmmo += load;
        reserveAmmo -= load;
        isReloading = false;
    }

    // Buff stats theo wave
    public void ApplyWaveUpgrade(int wave)
    {
        if (wave % 5 != 0) return;
        int waveMultiplier = wave / 5;

        currentDamage = gunData.damage + 5f * waveMultiplier;
        currentBulletSpeed = gunData.bulletSpeed + 20f * waveMultiplier;
        currentFireRate = gunData.fireRate * Mathf.Pow(0.9f, waveMultiplier);
        currentReloadTime = gunData.reloadTime * Mathf.Pow(0.9f, waveMultiplier);

        currentAmmo = gunData.magazineSize;

        if (waveBulletColors != null && waveBulletColors.Length > 0)
        {
            int colorIndex = (waveMultiplier - 1) % waveBulletColors.Length;
            bulletColor = waveBulletColors[colorIndex];
        }

        Debug.Log($"Gun upgraded! Wave: {wave} | Damage: {currentDamage} | BulletSpeed: {currentBulletSpeed} | FireRate: {currentFireRate} | ReloadTime: {currentReloadTime}");
    }

    // Reset về stats base
    public void ResetGunStats()
    {
        currentDamage = gunData.damage;
        currentFireRate = gunData.fireRate;
        currentReloadTime = gunData.reloadTime;
        currentBulletSpeed = gunData.bulletSpeed;

        bulletColor = Color.red;
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxAmmo;
    }
}
