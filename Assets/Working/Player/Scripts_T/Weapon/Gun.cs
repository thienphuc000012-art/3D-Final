using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public GunData gunData;

    [Header("AUDIO")]
    public AudioSource shootAudio;
    public AudioSource reloadAudio;

    public AudioClip shootClip;
    public AudioClip reloadClip;

    [Header("AIM CAMERA")]
    public Camera aimCamera;

    [Header("VIEWMODEL")]
    public Transform firePointVM;
    public ParticleSystem muzzleFlashVM;

    [Header("MODEL ROOTS")]
    public Transform ADS_Parent;
    public Transform recoilPivot;    // << NEW
    public Transform modelHolderVM;
    public Transform modelHolderFB;

    [Header("ANIMATOR")]
    public Animator viewModelAnimator;
    public Animator fullBodyAnimator;

    [Header("BULLET VISUAL")]
    public GameObject bulletPrefab;

    [Header("LEVEL VISUAL")]
    public GameObject[] gunLevelModels;

    [Header("ADS SETTINGS")]
    public bool isAiming;
    public float hipSpread = 0.015f;
    public float adsSpread = 0f;
    public float aimSpeed = 12f;

    [Header("IRON-SIGHT POSITIONS")]
    public Transform hipPosition;
    public Transform adsPosition;

    [Header("CAMERA RECOIL")]
    public float hipCamRecoilUp = 1.2f;
    public float hipCamRecoilSide = 0.6f;
    public float adsCamRecoilUp = 0.15f;
    public float adsCamRecoilSide = 0.1f;

    // ============= NEW: VIEWMODEL RECOIL =================
    [Header("VIEWMODEL RECOIL")]
    public float hipRecoilAmount = 4f;
    public float hipRecoilBack = 0.07f;

    public float adsRecoilAmount = 1.1f;
    public float adsRecoilBack = 0.02f;

    public float recoilReturnSpeed = 8f;

    Vector3 viewmodelRecoilCurrent;
    Vector3 viewmodelRecoilTarget;

    // ===================== LEVEL SYSTEM =====================
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

    // ===================== RUNTIME =====================
    float damage;
    float fireCooldown;
    float reloadTime;
    float bulletSpeed;
    int magazineSize;

    int reloadStateHash;

    // ===================== INIT =====================
    void Start()
    {
        reloadStateHash = Animator.StringToHash("Reloading");

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
        if (Input.GetKeyDown(KeyCode.L)) LevelUp();

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize)
        {
            UnfreezeAnim();
            StartCoroutine(Reload());
            return;
        }

        if (IsReloadAnimationPlaying()) return;

        if (requireReleaseFire && !Input.GetButton("Fire1"))
            requireReleaseFire = false;

        if (isReloading || requireReleaseFire) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
                Shoot();
            else
            {
                UnfreezeAnim();
                StartCoroutine(Reload());
            }
        }

        bool aimingState = Input.GetButton("Fire2");

        if (aimingState != isAiming)
        {
            isAiming = aimingState;

            if (isAiming)
                FreezeAimingIdle();
            else
                UnfreezeAnim();
        }

        HandleADS();
        HandleViewmodelRecoil();   // << NEW
    }

    // ===================== Freeze animation =====================
    void FreezeAimingIdle()
    {
        if (!viewModelAnimator) return;

        viewModelAnimator.Play("Rifle_AimingIdle", 0, 0f);
        viewModelAnimator.speed = 0f;
    }

    void UnfreezeAnim()
    {
        if (!viewModelAnimator) return;
        viewModelAnimator.speed = 1f;
    }

    // ===================== SHOOT =====================
    void Shoot()
    {
        if (isReloading) return;

        nextFireTime = Time.time + fireCooldown;
        currentAmmo--;

        // CAMERA RECOIL
        MouseLook mouseLook = aimCamera.GetComponentInParent<MouseLook>();
        if (mouseLook)
        {
            if (!isAiming)
                mouseLook.AddRecoil(hipCamRecoilUp, hipCamRecoilSide);
            else
                mouseLook.AddRecoil(adsCamRecoilUp, adsCamRecoilSide);
        }

        // VIEWMODEL RECOIL
        AddViewmodelRecoil();

        // Anim
        PlayAnim(viewModelAnimator, "Shoot");
        PlayAnim(fullBodyAnimator, "Shoot");

        muzzleFlashVM?.Play();

        Vector3 dir;

        if (!isAiming)
        {
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            dir = ApplySpread(ray.direction, hipSpread);
        }
        else
        {
            Ray camRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            Vector3 targetPoint =
                Physics.Raycast(camRay, out RaycastHit hit, 1000f)
                ? hit.point
                : camRay.GetPoint(1000f);

            dir = (targetPoint - firePointVM.position).normalized;
        }

        if (shootAudio && shootClip)
        {
            shootAudio.pitch = Random.Range(0.95f, 1.05f);
            shootAudio.PlayOneShot(shootClip);
        }

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

    // ===================== VIEWMODEL RECOIL =====================
    void AddViewmodelRecoil()
    {
        if (!recoilPivot) return;

        if (!isAiming)
        {
            viewmodelRecoilTarget += new Vector3(-hipRecoilAmount, Random.Range(-2f, 2f), hipRecoilBack);
        }
        else
        {
            viewmodelRecoilTarget += new Vector3(-adsRecoilAmount, Random.Range(-0.4f, 0.4f), adsRecoilBack);
        }
    }

    void HandleViewmodelRecoil()
    {
        viewmodelRecoilCurrent = Vector3.Lerp(
            viewmodelRecoilCurrent,
            viewmodelRecoilTarget,
            Time.deltaTime * 25f
        );

        viewmodelRecoilTarget = Vector3.Lerp(
            viewmodelRecoilTarget,
            Vector3.zero,
            Time.deltaTime * recoilReturnSpeed
        );

        // APPLY TO RECOIL PIVOT — NOT ADS PARENT
        if (recoilPivot)
            recoilPivot.localRotation = Quaternion.Euler(viewmodelRecoilCurrent);
    }

    // ===================== ADS HANDLER =====================
    void HandleADS()
    {
        if (!hipPosition || !adsPosition || !ADS_Parent) return;

        if (isAiming)
        {
            ADS_Parent.localPosition = Vector3.Lerp(
                ADS_Parent.localPosition,
                adsPosition.localPosition,
                Time.deltaTime * aimSpeed
            );

            ADS_Parent.localRotation = Quaternion.Slerp(
                ADS_Parent.localRotation,
                adsPosition.localRotation,
                Time.deltaTime * aimSpeed
            );
        }
        else
        {
            ADS_Parent.localPosition = Vector3.Lerp(
                ADS_Parent.localPosition,
                hipPosition.localPosition,
                Time.deltaTime * aimSpeed
            );

            ADS_Parent.localRotation = Quaternion.Slerp(
                ADS_Parent.localRotation,
                hipPosition.localRotation,
                Time.deltaTime * aimSpeed
            );
        }
    }

    // ===================== SPREAD =====================
    Vector3 ApplySpread(Vector3 dir, float spread)
    {
        if (spread <= 0f) return dir;

        Vector2 circle = Random.insideUnitCircle * spread;

        dir += aimCamera.transform.right * circle.x;
        dir += aimCamera.transform.up * circle.y;

        return dir.normalized;
    }

    // ===================== RELOAD =====================
    IEnumerator Reload()
    {
        UnfreezeAnim();

        if (isReloading) yield break;

        isReloading = true;
        requireReleaseFire = true;

        PlayAnim(viewModelAnimator, "Reload");
        PlayAnim(fullBodyAnimator, "Reload");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        reloadAudio?.PlayOneShot(reloadClip);
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

        AnimatorStateInfo state = viewModelAnimator.GetCurrentAnimatorStateInfo(0);

        return state.IsName("Reload") && state.normalizedTime < 1f;
    }

    // ===================== VISUAL =====================
    void UpdateGunVisual()
    {
        if (gunLevelModels.Length == 0) return;

        int index = Mathf.Clamp(level - 1, 0, gunLevelModels.Length - 1);
        ReplaceModel(modelHolderVM, gunLevelModels[index]);
        ReplaceModel(modelHolderFB, gunLevelModels[index]);

        var mesh = gunLevelModels[index].GetComponentInChildren<MeshRenderer>();
        if (mesh)
            currentBulletColor = mesh.sharedMaterial.color;
    }

    void ReplaceModel(Transform holder, GameObject prefab)
    {
        if (!holder || !prefab) return;

        foreach (Transform c in holder)
            Destroy(c.gameObject);

        var obj = Instantiate(prefab, holder);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    // ===================== UTIL =====================
    void PlayAnim(Animator anim, string trigger)
    {
        if (!anim || !anim.runtimeAnimatorController) return;

        anim.ResetTrigger(trigger);
        anim.SetTrigger(trigger);
    }

    void LogStats()
    {
        Debug.Log($"[GUN]\nLv {level}\nDMG: {damage}\nFireCooldown: {fireCooldown}\nReload: {reloadTime}\nMag: {magazineSize}\nEXP: {currentExp}/{expToNextLevel}");
    }
}