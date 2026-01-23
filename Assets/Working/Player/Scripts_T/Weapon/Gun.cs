using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    // ==========================
    // REFERENCES
    // ==========================
    [Header("References")]
    public GunData gunData;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public ParticleSystem muzzleFlash;
    public Camera aimCamera;

    [Header("Animation")]
    public Animator animator;

    // ==========================
    // SPINE AIM
    // ==========================
    [Header("Spine Aim")]
    public Transform spineBone;
    public Transform cameraHolder;
    [Range(0f, 1f)] public float spineWeight = 0.3f;
    public float maxSpinePitch = 40f;

    // ==========================
    // GUN LEVEL SYSTEM
    // ==========================
    [Header("Gun Level System")]
    public int level = 1;
    public float currentExp = 0f;
    public float expToNextLevel = 100f;

    // ==========================
    // VISUAL LEVEL
    // ==========================
    [Header("Gun Visual")]
    public Transform modelHolder;              // EMPTY object
    public GameObject[] gunLevelModels;        // prefab theo level
    public Color[] gunLevelColors;             // màu theo level

    [Header("Bullet Visual")]
    public Color bulletColor = Color.red;

    // ==========================
    // MAGAZINE
    // ==========================
    [Header("Magazine")]
    public Transform magazineSocket;

    // ==========================
    // INTERNAL STATE
    // ==========================
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
    // INIT
    // ==========================
    void Start()
    {
        ResetGunStats();
        ApplyLevelStats();
        UpdateGunVisual();

        if (muzzleFlash)
        {
            muzzleFlash.Stop();
            muzzleFlash.Clear();
        }

        Debug.Log($"[GUN INIT] Lv {level} | EXP {currentExp}/{expToNextLevel}");
    }

    // ==========================
    // UPDATE
    // ==========================
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
            StartCoroutine(Reload());

        // DEBUG TEST
        if (Input.GetKeyDown(KeyCode.L))
            AddExp(999f);
    }

    // ==========================
    // SPINE AIM
    // ==========================
    void LateUpdate()
    {
        if (!spineBone || !cameraHolder) return;

        float pitch = cameraHolder.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -maxSpinePitch, maxSpinePitch);

        spineBone.localRotation = Quaternion.Euler(pitch * spineWeight, 0f, 0f);
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
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);
            animator.SetTrigger("Shoot");
        }

        if (muzzleFlash)
            muzzleFlash.Play();

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Vector3 dir = ray.direction;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(dir)
        );

        Bullet b = bullet.GetComponent<Bullet>();
        if (b)
        {
            b.damage = currentDamage;
            b.SetDirection(dir);
            b.SetBulletSpeed(currentBulletSpeed);
            b.SetColor(bulletColor);
        }
    }

    // ==========================
    // RELOAD
    // ==========================
    IEnumerator Reload()
    {
        if (isReloading || reserveAmmo <= 0 || currentAmmo >= gunData.magazineSize)
            yield break;

        isReloading = true;

        if (animator)
            animator.SetTrigger("Reload");

        yield return new WaitForSeconds(currentReloadTime);

        int load = Mathf.Min(gunData.magazineSize - currentAmmo, reserveAmmo);
        currentAmmo += load;
        reserveAmmo -= load;

        isReloading = false;
    }

    // ==========================
    // EXP & LEVEL
    // ==========================
    public void AddExp(float amount)
    {
        currentExp += amount;

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }

        Debug.Log($"[GUN] Lv {level} | EXP {currentExp}/{expToNextLevel}");
    }

    void LevelUp()
    {
        level++;
        expToNextLevel *= 1.5f;

        ApplyLevelStats();
        UpdateGunVisual();

        Debug.Log($"[GUN LEVEL UP] >>> Lv {level}");
    }

    void ApplyLevelStats()
    {
        float dmgMul = 1f + (level - 1) * 1.0f;        // +100% dmg
        float speedMul = 1f + (level - 1) * 0.2f;      // +20% speed
        float reloadMul = Mathf.Pow(0.8f, level - 1); // -20% reload
        float animMul = 1f + (level - 1) * 0.2f;

        currentDamage = gunData.damage * dmgMul;
        currentBulletSpeed = gunData.bulletSpeed * speedMul;
        currentReloadTime = gunData.reloadTime * reloadMul;
        currentFireRate = gunData.fireRate;

        animSpeedMultiplier = animMul;

        if (animator)
            animator.SetFloat("AnimSpeed", animSpeedMultiplier);
    }

    // ==========================
    // VISUAL UPDATE
    // ==========================
    void UpdateGunVisual()
    {
        if (!modelHolder || gunLevelModels == null || gunLevelModels.Length == 0)
            return;

        foreach (Transform c in modelHolder)
            Destroy(c.gameObject);

        int index = Mathf.Clamp(level - 1, 0, gunLevelModels.Length - 1);

        GameObject model = Instantiate(
            gunLevelModels[index],
            modelHolder.position,
            modelHolder.rotation,
            modelHolder
        );

        ApplyGunColor(model);
    }

    void ApplyGunColor(GameObject model)
    {
        if (gunLevelColors == null || gunLevelColors.Length == 0)
            return;

        int index = Mathf.Clamp(level - 1, 0, gunLevelColors.Length - 1);
        Color color = gunLevelColors[index];

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(block);

            if (r.sharedMaterial.HasProperty("_BaseColor"))
                block.SetColor("_BaseColor", color);
            else
                block.SetColor("_Color", color);

            r.SetPropertyBlock(block);
        }
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
}
