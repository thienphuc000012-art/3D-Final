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
    public Animator animator;   // Animator Base + Layer override

    [Header("Aim / Look")]
    public Transform GunHolder;       // Parent gun (vị trí tay)
    public Transform CameraHolder;    // Object gắn MouseLook

    [Header("Visual")]
    public Color bulletColor = Color.red;
    public Color[] waveBulletColors;

    [Header("Gun Stats")]
    public float aimRange = 200f;

    [Header("Spine Rotation")]
    public Transform spineBone; // Kéo xương mixamorig:Spine2 vào đây

    // --- Internal state ---
    int currentAmmo;
    int reserveAmmo;
    float nextFireTime;
    bool isReloading;

    float currentDamage;
    float currentFireRate;
    float currentReloadTime;
    float currentBulletSpeed;

    // Tốc độ animation Firing/Reload layer
    float animSpeedMultiplier = 1f;

    void Start()
    {
        ResetGunStats();
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxAmmo;

        if (muzzleFlash)
        {
            muzzleFlash.Stop();
            muzzleFlash.Clear();
        }
    }

    void Update()
    {
        if (isReloading) return;

        // --- Fire ---
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            Shoot();

        // --- Reload ---
        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Reload());
    }

    // Trong LateUpdate của Gun.cs
    void LateUpdate()
    {
        // Ép hướng của súng luôn nhìn về phía trước Camera
        if (firePoint != null && aimCamera != null)
        {
            // Bạn có thể dùng Raycast để xác định điểm giữa màn hình
            // Hoặc đơn giản là ép model súng nhìn theo hướng Camera
        }
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            // Hết đạn mới trigger reload
            StartCoroutine(Reload());
            return;
        }

        nextFireTime = Time.time + currentFireRate;
        currentAmmo--;

        // Trigger animation Shoot
        if (animator != null)
        {
            animator.ResetTrigger("Reload");
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);
            animator.SetTrigger("Shoot");
        }

        if (muzzleFlash) muzzleFlash.Play();

        // Raycast từ camera
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = ray.GetPoint(aimRange);

        if (Physics.Raycast(ray, out RaycastHit hit, aimRange))
            targetPoint = hit.point;

        Vector3 shootDir = (targetPoint - firePoint.position).normalized;
        Quaternion bulletRot = Quaternion.LookRotation(shootDir);

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRot);

        Collider playerCol = GetComponentInParent<Collider>();
        Collider bulletCol = bullet.GetComponent<Collider>();
        if (playerCol != null && bulletCol != null)
            Physics.IgnoreCollision(bulletCol, playerCol);

        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.damage = currentDamage;
            b.SetDirection(shootDir);
            b.SetBulletSpeed(currentBulletSpeed);
            b.SetColor(bulletColor);
        }
    }

    IEnumerator Reload()
    {
        if (reserveAmmo <= 0 || currentAmmo == gunData.magazineSize || isReloading)
            yield break;

        isReloading = true;

        if (animator != null)
        {
            animator.ResetTrigger("Shoot");
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);
            animator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(currentReloadTime);

        int need = gunData.magazineSize - currentAmmo;
        int load = Mathf.Min(need, reserveAmmo);

        currentAmmo += load;
        reserveAmmo -= load;
        isReloading = false;
    }

    // --- Buff stats + animation theo wave ---
    public void ApplyWaveUpgrade(int wave)
    {
        if (wave % 5 != 0) return;
        int waveMultiplier = wave / 5;

        // Stats
        currentDamage = gunData.damage + 5f * waveMultiplier;
        currentBulletSpeed = gunData.bulletSpeed + 20f * waveMultiplier;
        currentFireRate = gunData.fireRate * Mathf.Pow(0.9f, waveMultiplier);
        currentReloadTime = gunData.reloadTime * Mathf.Pow(0.9f, waveMultiplier);
        currentAmmo = gunData.magazineSize;

        // Đổi màu đạn
        if (waveBulletColors != null && waveBulletColors.Length > 0)
        {
            int colorIndex = (waveMultiplier - 1) % waveBulletColors.Length;
            bulletColor = waveBulletColors[colorIndex];
        }

        // Animation speed
        animSpeedMultiplier = 1f + 0.1f * waveMultiplier;
        if (animator != null)
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);

        Debug.Log($"Gun upgraded! Wave: {wave} | FireRate: {currentFireRate:F2} | ReloadTime: {currentReloadTime:F2} | AnimSpeed: {animSpeedMultiplier:F2}");
    }

    // --- Reset stats ---
    public void ResetGunStats()
    {
        currentDamage = gunData.damage;
        currentFireRate = gunData.fireRate;
        currentReloadTime = gunData.reloadTime;
        currentBulletSpeed = gunData.bulletSpeed;

        bulletColor = Color.red;
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxAmmo;

        animSpeedMultiplier = 1f;
        if (animator != null)
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);
    }
}
