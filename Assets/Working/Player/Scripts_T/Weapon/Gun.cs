using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    // ===================== DATA =====================
    public GunData gunData;

    [Header("AIM")]
    public Camera aimCamera;

    [Header("VIEWMODEL")]
    public Transform firePointVM;
    public ParticleSystem muzzleFlashVM;
    public Transform modelHolderVM;

    [Header("FULL BODY")]
    public Transform modelHolderFB;

    [Header("ANIMATOR")]
    public Animator viewModelAnimator;
    public Animator fullBodyAnimator;

    [Header("BULLET (VISUAL)")]
    public GameObject bulletPrefab;

    [Header("LEVEL VISUAL")]
    public GameObject[] gunLevelModels;

    // ===================== LEVEL =====================
    public int level = 1;
    public int maxLevel = 5;
    public int currentExp = 0;
    public int expToNextLevel = 10;

    // ===================== INTERNAL =====================
    int currentAmmo;
    float nextFireTime;
    bool isReloading;
    bool requireReleaseFire;

    Color currentBulletColor = Color.white;

    // ===================== BASE STATS =====================
    float baseDamage;
    float baseFireCooldown;
    float baseReloadTime;
    float baseBulletSpeed;
    int baseMagazineSize;

    // ===================== RUNTIME STATS =====================
    float damage;
    float fireCooldown;
    float reloadTime;
    float bulletSpeed;
    int magazineSize;

    // ===================== ANIM HASH =====================
    int reloadStateHash;

    // ===================== INIT =====================
    void Start()
    {
        reloadStateHash = Animator.StringToHash("Reloading"); // KHỚP TÊN STATE

        CacheBaseStats();
        ResetGunToLevel1();
        UpdateGunVisual();
        LogStats();
    }

    void CacheBaseStats()
    {
        baseDamage = gunData.damage;
        baseFireCooldown = gunData.fireRate;
        baseReloadTime = gunData.reloadTime;
        baseBulletSpeed = gunData.bulletSpeed;
        baseMagazineSize = gunData.magazineSize;
    }

    void ResetGunToLevel1()
    {
        level = 1;
        currentExp = 0;
        expToNextLevel = 10;

        ApplyStatsByLevel();
    }

    // ===================== UPDATE =====================
    void Update()
    {
        // ===== TEST LEVEL =====
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("PRESS L");
            LevelUp();
        }

        // ===== RELOAD BẰNG R (LUÔN HOẠT ĐỘNG) =====
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        // ===== KHÓA BẮN KHI ĐANG RELOAD =====
        if (IsReloadAnimationPlaying())
            return;

        if (requireReleaseFire && !Input.GetButton("Fire1"))
            requireReleaseFire = false;

        if (isReloading || requireReleaseFire)
            return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
                Shoot();
            else
                StartCoroutine(Reload());
        }
    }

    // ===================== SHOOT =====================
    void Shoot()
    {
        if (isReloading) return;

        nextFireTime = Time.time + fireCooldown;
        currentAmmo--;

        PlayAnim(viewModelAnimator, "Shoot");
        PlayAnim(fullBodyAnimator, "Shoot");
        muzzleFlashVM?.Play();

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Vector3 dir = ray.direction;

        // HITSCAN
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Debug.Log($"Bullet hit: {hit.collider.name}");
            // hit.collider.GetComponent<IDamageable>()?.TakeDamage(damage);
        }

        // VISUAL BULLET
        if (bulletPrefab && firePointVM)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                firePointVM.position,
                Quaternion.LookRotation(dir)
            );

            Bullet b = bullet.GetComponent<Bullet>();
            if (b)
            {
                b.damage = damage;
                b.SetDirection(dir);
                b.SetBulletSpeed(bulletSpeed);
                b.SetColor(currentBulletColor);
            }
        }
    }

    // ===================== RELOAD =====================
    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;
        requireReleaseFire = true;

        PlayAnim(viewModelAnimator, "Reload");
        PlayAnim(fullBodyAnimator, "Reload");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
    }


    // ===================== LEVEL =====================
    public void AddExp(int amount)
    {
        if (level >= maxLevel) return;

        currentExp += amount;

        if (currentExp >= expToNextLevel)
        {
            currentExp = 0;
            LevelUp();
        }
    }

    void LevelUp()
    {
        if (level >= maxLevel) return;

        level++;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f);

        ApplyStatsByLevel();
        UpdateGunVisual();
        LogStats();
    }

    void ApplyStatsByLevel()
    {
        int lv = level - 1;

        damage = baseDamage * (1f + 0.2f * lv);
        fireCooldown = baseFireCooldown * Mathf.Pow(0.9f, lv);
        reloadTime = baseReloadTime * Mathf.Pow(0.9f, lv);
        bulletSpeed = baseBulletSpeed * (1f + 0.15f * lv);
        magazineSize = baseMagazineSize + lv * 5;

        currentAmmo = magazineSize;
    }

    // ===================== ANIM CHECK =====================
    bool IsReloadAnimationPlaying()
    {
        if (!viewModelAnimator) return false;

        AnimatorStateInfo state =
            viewModelAnimator.GetCurrentAnimatorStateInfo(0);

        return state.shortNameHash == reloadStateHash &&
               state.normalizedTime < 1f;
    }

    // ===================== VISUAL =====================
    void UpdateGunVisual()
    {
        if (gunLevelModels.Length == 0) return;

        int index = Mathf.Clamp(level - 1, 0, gunLevelModels.Length - 1);
        ReplaceModel(modelHolderVM, gunLevelModels[index]);
        ReplaceModel(modelHolderFB, gunLevelModels[index]);
    }

    void ReplaceModel(Transform holder, GameObject prefab)
    {
        if (!holder || !prefab) return;

        foreach (Transform c in holder)
            Destroy(c.gameObject);

        Instantiate(prefab, holder).transform.localPosition = Vector3.zero;
    }

    // ===================== DEBUG =====================
    void LogStats()
    {
        Debug.Log(
            $"[GUN]\n" +
            $"Lv {level}\n" +
            $"DMG: {damage}\n" +
            $"FireCooldown: {fireCooldown}\n" +
            $"Reload: {reloadTime}\n" +
            $"Mag: {magazineSize}\n" +
            $"EXP: {currentExp}/{expToNextLevel}"
        );
    }

    // ===================== UTILS =====================
    void PlayAnim(Animator anim, string trigger)
    {
        if (!anim || !anim.isActiveAndEnabled || !anim.runtimeAnimatorController)
            return;

        anim.ResetTrigger(trigger);
        anim.SetTrigger(trigger);
    }
}
