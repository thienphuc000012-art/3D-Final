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

    [Header("Aim / Look")]
    public Transform GunHolder;
    public Transform CameraHolder;

    [Header("Spine & Neck Rotation")]
    public Transform spineBone; // Kéo xương Spine2 vào
    public Transform neckBone;  // Kéo xương Neck vào
    [Range(0f, 1f)]
    public float spineWeight = 0.3f; // Tỉ lệ lưng gập (0.2 - 0.4 để chân bám đất)

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
    }

    void Update()
    {
        if (isReloading) return;
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) Shoot();
        if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(Reload());
    }

    // Xử lý xoay xương sau khi Animation đã chạy để không bị giật
    void LateUpdate()
    {
        if (!spineBone || !CameraHolder || !neckBone || !aimCamera) return;

        // 1. Lấy góc lên/xuống từ CameraHolder
        float xRot = CameraHolder.localEulerAngles.x;
        if (xRot > 180) xRot -= 360;

        // 2. Xoay lưng (Spine): Giới hạn trọng tâm để chân KHÔNG bị nhấc
        // Chỉ dùng một phần góc quay (spineWeight)
        spineBone.localRotation *= Quaternion.Euler(xRot * spineWeight, 0, 0);

        // 3. Xoay cổ (Neck): Ép đầu nhìn thẳng vào Crosshair
        // Bù đắp phần còn lại mà lưng chưa xoay tới
        neckBone.rotation = aimCamera.transform.rotation;

        // Bù đắp góc lệch mặc định của Mixamo (Thử 90 hoặc -90 nếu đầu bị quay ngang)
        neckBone.rotation *= Quaternion.Euler(0, 90, 0);
    }

    void Shoot()
    {
        if (currentAmmo <= 0) { StartCoroutine(Reload()); return; }

        nextFireTime = Time.time + currentFireRate;
        currentAmmo--;

        if (animator)
        {
            animator.ResetTrigger("Reload");
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);
            animator.SetTrigger("Shoot");
        }

        if (muzzleFlash) muzzleFlash.Play();

        // Bắn từ tâm Camera (Crosshair)
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = ray.GetPoint(aimRange);
        if (Physics.Raycast(ray, out RaycastHit hit, aimRange)) targetPoint = hit.point;

        Vector3 shootDir = (targetPoint - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDir));

        // Bỏ qua va chạm với người chơi
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
        bulletColor = Color.red;
        animSpeedMultiplier = 1f;
    }
}