using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("References")]
    public GunData gunData;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public ParticleSystem muzzleFlash;
    public Camera aimCamera;

    [Header("Animation")]
    public Animator animator;

    [Header("Spine Aim (IMPORTANT)")]
    public Transform spineBone;        // mixamorig:Spine2
    public Transform cameraHolder;     // object xoay pitch
    [Range(0f, 1f)]
    public float spineWeight = 0.4f;
    public float maxSpinePitch = 40f;

    [Header("Visual")]
    public Color bulletColor = Color.red;
    public Color[] waveBulletColors;
    public float aimRange = 200f;

    [Header("Magazine (future reload)")]
    public Transform magazineSocket;
    public Transform leftHand;

    // ===== Internal State =====
    int currentAmmo;
    int reserveAmmo;
    float nextFireTime;
    bool isReloading;

    float currentDamage;
    float currentFireRate;
    float currentReloadTime;
    float currentBulletSpeed;

    float animSpeedMultiplier = 1f;

    // ==========================

    void Start()
    {
        ResetGunStats();

        if (muzzleFlash)
        {
            muzzleFlash.Stop();
            muzzleFlash.Clear();
        }
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
                Shoot();
            else
                StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < gunData.magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    // ==========================
    // AIM SPINE (FIX LỆCH)
    // ==========================
    void LateUpdate()
    {
        if (!spineBone || !cameraHolder) return;

        float pitch = cameraHolder.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        pitch = Mathf.Clamp(pitch, -maxSpinePitch, maxSpinePitch);

        // chỉ bẻ X (pitch), không đụng yaw
        spineBone.localRotation =
            Quaternion.Euler(pitch * spineWeight, 0f, 0f);
    }

    // ==========================
    // SHOOT
    // ==========================
    void Shoot()
    {
        nextFireTime = Time.time + currentFireRate;
        currentAmmo--;

        if (animator)
        {
            animator.ResetTrigger("Reload");
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);
            animator.SetTrigger("Shoot");
        }

        if (muzzleFlash)
            muzzleFlash.Play();

        // === ĐẠN BAY THEO CAMERA ===
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Vector3 shootDir = ray.direction;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDir)
        );

        Bullet b = bullet.GetComponent<Bullet>();
        if (b)
        {
            b.damage = currentDamage;
            b.SetDirection(shootDir);
            b.SetBulletSpeed(currentBulletSpeed);
            b.SetColor(bulletColor);
        }
    }

    // ==========================
    // RELOAD
    // ==========================
    IEnumerator Reload()
    {
        if (isReloading) yield break;
        if (reserveAmmo <= 0) yield break;
        if (currentAmmo >= gunData.magazineSize) yield break;

        isReloading = true;

        if (animator)
        {
            animator.ResetTrigger("Shoot");
            animator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(currentReloadTime);

        int load = Mathf.Min(
            gunData.magazineSize - currentAmmo,
            reserveAmmo
        );

        currentAmmo += load;
        reserveAmmo -= load;

        isReloading = false;
    }

    // ==========================
    // WAVE UPGRADE
    // ==========================
    public void ApplyWaveUpgrade(int wave)
    {
        if (wave % 5 != 0) return;

        int mult = wave / 5;

        currentDamage = gunData.damage + 5f * mult;
        currentBulletSpeed = gunData.bulletSpeed + 20f * mult;
        currentFireRate = gunData.fireRate * Mathf.Pow(0.9f, mult);
        currentReloadTime = gunData.reloadTime * Mathf.Pow(0.9f, mult);

        if (waveBulletColors != null && waveBulletColors.Length > 0)
            bulletColor = waveBulletColors[(mult - 1) % waveBulletColors.Length];

        animSpeedMultiplier = 1f + 0.1f * mult;

        if (animator)
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);
    }

    // ==========================
    // RESET
    // ==========================
    public void ResetGunStats()
    {
        currentDamage = gunData.damage;
        currentFireRate = gunData.fireRate;
        currentReloadTime = gunData.reloadTime;
        currentBulletSpeed = gunData.bulletSpeed;

        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxAmmo;
    }

    // ==========================
    // ANIMATION EVENTS (DÙNG SAU)
    // ==========================
    public void GrabMagazine() { }
    public void InsertMagazine() { }
}
