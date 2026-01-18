using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public GunData gunData;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public ParticleSystem muzzleFlash;
    public Color bulletColor = Color.red; // màu đạn của súng này

    int currentAmmo;
    int reserveAmmo;
    float nextFireTime;
    bool isReloading;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxAmmo;
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        nextFireTime = Time.time + gunData.fireRate;
        currentAmmo--;

        if (muzzleFlash) muzzleFlash.Play();

        // Tạo đạn
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.damage = gunData.damage;
            b.SetColor(bulletColor); // set màu phát sáng
        }
    }

    IEnumerator Reload()
    {
        if (reserveAmmo <= 0 || currentAmmo == gunData.magazineSize)
            yield break;

        isReloading = true;
        yield return new WaitForSeconds(gunData.reloadTime);

        int need = gunData.magazineSize - currentAmmo;
        int load = Mathf.Min(need, reserveAmmo);

        currentAmmo += load;
        reserveAmmo -= load;
        isReloading = false;
    }
}
