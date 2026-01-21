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

    [Header("Aim / Look Control")]
    public Transform GunHolder;   // Object cha trực tiếp của mô hình súng
    public Transform CameraHolder;

    [Header("Spine & Neck Rotation")]
    public Transform spineBone;    // Xương mixamorig:Spine2
    public Transform neckBone;     // Xương mixamorig:Neck
    [Range(0f, 1f)]
    public float spineWeight = 0.4f;

    [Header("Visual & Stats")]
    public Color bulletColor = Color.red;
    public Color[] waveBulletColors;
    public float aimRange = 200f;

    // State nội bộ
    int currentAmmo;
    int reserveAmmo;
    float nextFireTime;
    bool isReloading;
    float currentDamage, currentFireRate, currentReloadTime, currentBulletSpeed;
    float animSpeedMultiplier = 1f;

    void Start()
    {
        ResetGunStats();
        if (muzzleFlash) { muzzleFlash.Stop(); muzzleFlash.Clear(); }

        // Đảm bảo súng bắt đầu với đầy đạn
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxAmmo;
    }

    void Update()
    {
        if (isReloading) return;

        // 1. LUÔN HƯỚNG SÚNG VỀ CROSSHAIR (Lia súng)
        HandleAiming();

        // 2. LOGIC BẮN
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0) Shoot();
            else StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < gunData.magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    void LateUpdate()
    {
        if (spineBone == null || CameraHolder == null) return;

        float xRot = CameraHolder.localEulerAngles.x;
        if (xRot > 180) xRot -= 360f;

        float targetAngle = xRot * spineWeight;

        spineBone.localRotation = Quaternion.Euler(0, 0, targetAngle);

        SyncGunToCrosshair();
    }

    void SyncGunToCrosshair()
    {
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, aimRange))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(aimRange);

        if (GunHolder != null)
        {
            // Súng sẽ nhìn về mục tiêu ngay trong LateUpdate để khớp với xương người
            GunHolder.LookAt(targetPoint);
        }
    }

    void HandleAiming()
    {
        // Xác định điểm mục tiêu từ giữa màn hình
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, aimRange))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(aimRange);

        // Ép GunHolder lia theo điểm này (Điều này giúp tay bám theo nếu bạn dùng IK)
        if (GunHolder != null)
        {
            GunHolder.LookAt(targetPoint);
        }
    }

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

        if (muzzleFlash) muzzleFlash.Play();

        // Vì GunHolder đã LookAt ở Update, firePoint.forward luôn chuẩn xác
        Vector3 shootDir = firePoint.forward;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDir));

        // Bỏ qua va chạm với Player
        Collider playerCol = GetComponentInParent<Collider>();
        Collider bulletCol = bullet.GetComponent<Collider>();
        if (playerCol && bulletCol) Physics.IgnoreCollision(bulletCol, playerCol);

        Bullet b = bullet.GetComponent<Bullet>();
        if (b)
        {
            b.damage = currentDamage;
            b.SetDirection(shootDir);
            b.SetBulletSpeed(currentBulletSpeed);
            b.SetColor(bulletColor);
        }
    }

    IEnumerator Reload()
    {
        if (reserveAmmo <= 0 || currentAmmo == gunData.magazineSize || isReloading) yield break;
        isReloading = true;

        if (animator)
        {
            animator.ResetTrigger("Shoot");
            animator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(currentReloadTime);

        int load = Mathf.Min(gunData.magazineSize - currentAmmo, reserveAmmo);
        currentAmmo += load;
        reserveAmmo -= load;
        isReloading = false;
    }

    public void ApplyWaveUpgrade(int wave)
    {
        if (wave % 5 != 0) return;
        int mult = wave / 5;
        currentDamage = gunData.damage + 5f * mult;
        currentBulletSpeed = gunData.bulletSpeed + 20f * mult;
        currentFireRate = gunData.fireRate * Mathf.Pow(0.9f, mult);
        currentReloadTime = gunData.reloadTime * Mathf.Pow(0.9f, mult);

        if (waveBulletColors?.Length > 0)
            bulletColor = waveBulletColors[(mult - 1) % waveBulletColors.Length];

        animSpeedMultiplier = 1f + 0.1f * mult;
        if (animator) animator.SetFloat("AnimSpeed", animSpeedMultiplier);
    }

    public void ResetGunStats()
    {
        currentDamage = gunData.damage;
        currentFireRate = gunData.fireRate;
        currentReloadTime = gunData.reloadTime;
        currentBulletSpeed = gunData.bulletSpeed;
        currentAmmo = gunData.magazineSize;
        reserveAmmo = gunData.maxAmmo;
    }
}