using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public GunData gunData;

    [Header("CROSSHAIR")]
    public GameObject crosshairUI;

    [Header("AUDIO")]
    public AudioSource shootAudio;
    public AudioSource reloadAudio;
    public AudioClip shootClip;
    public AudioClip reloadClip;

    [Header("AIM CAMERA")]
    public Camera aimCamera;

    [Header("ADS ZOOM")]
    public float hipFOV = 70f;
    public float adsFOV = 45f;
    public float fovLerpSpeed = 10f;

    [Header("VIEWMODEL")]
    public Transform firePointVM;
    public ParticleSystem muzzleFlashVM;

    [Header("MODEL ROOTS")]
    public Transform ADS_Parent;
    public Transform recoilPivot;
    public Transform modelHolderVM;
    public Transform modelHolderFB;

    [Header("ANIMATOR")]
    public Animator viewModelAnimator;
    public Animator fullBodyAnimator;

    [Header("BULLET VISUAL")]
    public GameObject bulletPrefab;

    [Header("LEVEL VISUAL")]
    public GameObject[] gunLevelModels;

    [Header("MAG SYSTEM")]
    public Transform magSocketVM;
    public Transform magSocketFB;
    public GameObject[] magLevelPrefabs;

    GameObject currentMagVM;
    GameObject currentMagFB;

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

    [Header("VIEWMODEL RECOIL")]
    public float hipRecoilAmount = 4f;
    public float hipRecoilBack = 0.07f;
    public float adsRecoilAmount = 1.1f;
    public float adsRecoilBack = 0.02f;
    public float recoilReturnSpeed = 8f;

    Vector3 viewmodelRecoilCurrent;
    Vector3 viewmodelRecoilTarget;

    // LEVEL SYSTEM
    public int level = 1;
    public int maxLevel = 5;
    public int currentExp = 0;
    public int expToNextLevel = 10;

    // INTERNAL
    int currentAmmo;
    float nextFireTime;
    bool isReloading;
    bool requireReleaseFire;

    float postReloadDelay = 1f;
    bool lockFireAfterReload = false;

    Color currentBulletColor = Color.white;

    // BASE STATS
    float baseDamage;
    float baseFireCooldown;
    float baseReloadTime;
    float baseBulletSpeed;
    int baseMagazineSize;

    // RUNTIME
    float damage;
    float fireCooldown;
    float reloadTime;
    float bulletSpeed;
    int magazineSize;

    // BURST
    bool isBurstFiring = false;
    public int burstCount = 3; // ADS bắn 3 viên

    // ADS STATE CONTROL
    bool wantADS;
    bool forceHipByReload = false;

    // RELOAD COOLDOWN
    bool canReload = true;
    public float reloadCooldown = 1f;

    void Start()
    {
        CacheBaseStats();
        ResetGunToLevel1();
        UpdateGunVisual();
        AttachMagByLevel();
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LevelUp();
        }

        // ADS input
        wantADS = Input.GetButton("Fire2");

        // nếu đang reload → ép HIP
        if (forceHipByReload)
        {
            isAiming = false;
        }
        else
        {
            isAiming = wantADS;
        }

        if (viewModelAnimator)
            viewModelAnimator.SetBool("isAiming", isAiming);

        if (crosshairUI)
            crosshairUI.SetActive(!isAiming);

        HandleADS();
        HandleADSZoom();
        HandleViewmodelRecoil();

        if (lockFireAfterReload)
        {
            if (Time.time >= nextFireTime)
                lockFireAfterReload = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && canReload && !isReloading && currentAmmo < magazineSize)
        {
            UnfreezeAnim();
            StartCoroutine(Reload());
            return;
        }

        if (requireReleaseFire && !Input.GetButton("Fire1"))
            requireReleaseFire = false;

        if (isReloading || requireReleaseFire) return;

        // HIP → AUTO
        if (!isAiming && Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo <= 0)
            {
                UnfreezeAnim();
                StartCoroutine(Reload());
                return;
            }
            Shoot();
        }

        // ADS → BURST
        if (isAiming && Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo <= 0)
            {
                UnfreezeAnim();
                StartCoroutine(Reload());
                return;
            }

            if (!isBurstFiring)
                StartCoroutine(BurstFire());
        }
    }

    IEnumerator BurstFire()
    {
        isBurstFiring = true;

        int bulletsToFire = burstCount;
        while (bulletsToFire > 0 && currentAmmo > 0)
        {
            Shoot();
            bulletsToFire--;
            yield return new WaitForSeconds(fireCooldown);
        }

        nextFireTime = Time.time + fireCooldown;
        isBurstFiring = false;
    }

    void Shoot()
    {
        nextFireTime = Time.time + fireCooldown;
        currentAmmo--;

        MouseLook mouseLook = aimCamera.GetComponentInParent<MouseLook>();
        if (mouseLook)
        {
            if (isAiming)
                mouseLook.AddRecoil(adsCamRecoilUp, adsCamRecoilSide);
            else
                mouseLook.AddRecoil(hipCamRecoilUp, hipCamRecoilSide);
        }

        AddViewmodelRecoil();

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
            Vector3 targetPoint = Physics.Raycast(camRay, out RaycastHit hit, 1000f)
                ? hit.point
                : camRay.GetPoint(1000f);

            dir = (targetPoint - firePointVM.position).normalized;
        }

        if (shootAudio && shootClip)
            shootAudio.PlayOneShot(shootClip);

        if (bulletPrefab)
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

    void AddViewmodelRecoil()
    {
        if (!recoilPivot) return;

        float recoil = isAiming ? adsRecoilAmount : hipRecoilAmount;
        float back = isAiming ? adsRecoilBack : hipRecoilBack;

        viewmodelRecoilTarget += new Vector3(
            -recoil,
            Random.Range(-1f, 1f),
            back
        );
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

        if (recoilPivot)
            recoilPivot.localRotation = Quaternion.Euler(viewmodelRecoilCurrent);
    }

    void HandleADS()
    {
        if (!hipPosition || !adsPosition || !ADS_Parent) return;

        Transform target = isAiming ? adsPosition : hipPosition;

        ADS_Parent.localPosition = Vector3.Lerp(
            ADS_Parent.localPosition,
            target.localPosition,
            Time.deltaTime * aimSpeed
        );

        ADS_Parent.localRotation = Quaternion.Slerp(
            ADS_Parent.localRotation,
            target.localRotation,
            Time.deltaTime * aimSpeed
        );
    }

    void HandleADSZoom()
    {
        if (!aimCamera) return;

        float targetFOV = isAiming ? adsFOV : hipFOV;
        aimCamera.fieldOfView = Mathf.Lerp(
            aimCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * fovLerpSpeed
        );
    }

    Vector3 ApplySpread(Vector3 dir, float spread)
    {
        Vector2 circle = Random.insideUnitCircle * spread;
        dir += aimCamera.transform.right * circle.x;
        dir += aimCamera.transform.up * circle.y;
        return dir.normalized;
    }
    IEnumerator Reload()
    {
        if (isReloading || !canReload) yield break;

        isReloading = true;
        canReload = false;
        requireReleaseFire = true;

        // 1️⃣ ÉP HIP KHI RELOAD
        forceHipByReload = true;
        isAiming = false;

        if (viewModelAnimator)
            viewModelAnimator.SetBool("isAiming", false);

        if (crosshairUI)
            crosshairUI.SetActive(true);

        UnfreezeAnim();
        PlayAnim(viewModelAnimator, "Reload");
        PlayAnim(fullBodyAnimator, "Reload");

        yield return new WaitForSeconds(reloadTime);

        // Nạp đạn
        currentAmmo = magazineSize;
        isReloading = false;

        reloadAudio?.PlayOneShot(reloadClip);

        // 2️⃣ SAU RELOAD: TRẢ QUYỀN ADS
        forceHipByReload = false;

        if (wantADS)
            isAiming = true;
        else
            isAiming = false;

        // 3️⃣ LOCK FIRE + COOLDOWN RELOAD
        lockFireAfterReload = true;
        nextFireTime = Time.time + postReloadDelay;

        yield return new WaitForSeconds(reloadCooldown);
        canReload = true;
    }

    void UnfreezeAnim()
    {
        if (!viewModelAnimator) return;
        viewModelAnimator.speed = 1f;
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

    void UpdateGunVisual()
    {
        int index = Mathf.Clamp(level - 1, 0, gunLevelModels.Length - 1);
        ReplaceModel(modelHolderVM, gunLevelModels[index]);
        ReplaceModel(modelHolderFB, gunLevelModels[index]);

        var mesh = gunLevelModels[index].GetComponentInChildren<MeshRenderer>();
        if (mesh)
            currentBulletColor = mesh.sharedMaterial.color;
    }

    void ReplaceModel(Transform holder, GameObject prefab)
    {
        foreach (Transform c in holder)
            Destroy(c.gameObject);

        var obj = Instantiate(prefab, holder);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void AttachMagByLevel()
    {
        int index = Mathf.Clamp(level - 1, 0, magLevelPrefabs.Length - 1);

        if (currentMagVM) Destroy(currentMagVM);
        if (currentMagFB) Destroy(currentMagFB);

        if (magSocketVM)
        {
            currentMagVM = Instantiate(magLevelPrefabs[index], magSocketVM);
            currentMagVM.transform.localPosition = Vector3.zero;
            currentMagVM.transform.localRotation = Quaternion.identity;
        }

        if (magSocketFB)
        {
            currentMagFB = Instantiate(magLevelPrefabs[index], magSocketFB);
            currentMagFB.transform.localPosition = Vector3.zero;
            currentMagFB.transform.localRotation = Quaternion.identity;
        }
    }

    void PlayAnim(Animator anim, string trigger)
    {
        if (!anim || !anim.runtimeAnimatorController) return;
        anim.ResetTrigger(trigger);
        anim.SetTrigger(trigger);
    }

    void LevelUp()
    {
        if (level >= maxLevel) return;

        level++;
        currentExp = 0;

        ApplyStatsByLevel();
        UpdateGunVisual();
        AttachMagByLevel();

        LogStats();
    }

    void LogStats()
    {
        Debug.Log($"[GUN]\nLv {level}\nDMG: {damage}\nFireCooldown: {fireCooldown}\nReload: {reloadTime}\nMag: {magazineSize}");
    }
}