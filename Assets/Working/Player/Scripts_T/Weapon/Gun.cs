using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public GunData gunData;

    [Header("CROSSHAIR")]
    public GameObject crosshairUI;   // <-- thêm cái này

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

    int reloadStateHash;

    // BURST
    bool isBurstFiring = false;
    public int burstCount = 2; // 2 viên khi aim


    void Start()
    {
        reloadStateHash = Animator.StringToHash("Reloading");

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
        if (Input.GetKeyDown(KeyCode.L)) LevelUp();

        // Ẩn crosshair khi aim
        if (crosshairUI)
            crosshairUI.SetActive(!isAiming);

        if (lockFireAfterReload)
        {
            if (Time.time >= nextFireTime)
                lockFireAfterReload = false;
            return;
        }

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

        // ----------------------------
        // FIRE INPUT
        // ----------------------------
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo <= 0)
            {
                UnfreezeAnim();
                StartCoroutine(Reload());
                return;
            }

            if (isAiming)
            {
                // Bắn burst 2 viên
                if (!isBurstFiring)
                    StartCoroutine(BurstFire());
            }
            else
            {
                // Bắn thường
                Shoot();
            }
        }

        // ADS
        bool aimingState = Input.GetButton("Fire2");
        if (aimingState != isAiming)
            isAiming = aimingState;

        HandleADS();
        HandleViewmodelRecoil();
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

        nextFireTime = Time.time + fireCooldown; // chống spam click
        isBurstFiring = false;
    }

    void UnfreezeAnim()
    {
        if (!viewModelAnimator) return;
        viewModelAnimator.speed = 1f;
    }

    void Shoot()
    {
        if (isReloading) return;

        nextFireTime = Time.time + fireCooldown;
        currentAmmo--;

        MouseLook mouseLook = aimCamera.GetComponentInParent<MouseLook>();
        if (mouseLook && !isAiming)
            mouseLook.AddRecoil(hipCamRecoilUp, hipCamRecoilSide);

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

    void AddViewmodelRecoil()
    {
        if (!recoilPivot || isAiming) return;

        viewmodelRecoilTarget += new Vector3(
            -hipRecoilAmount,
            Random.Range(-2f, 2f),
            hipRecoilBack
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

    Vector3 ApplySpread(Vector3 dir, float spread)
    {
        if (spread <= 0f) return dir;

        Vector2 circle = Random.insideUnitCircle * spread;
        dir += aimCamera.transform.right * circle.x;
        dir += aimCamera.transform.up * circle.y;

        return dir.normalized;
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;
        requireReleaseFire = true;

        UnfreezeAnim();
        PlayAnim(viewModelAnimator, "Reload");
        PlayAnim(fullBodyAnimator, "Reload");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        if (reloadAudio && reloadClip)
            reloadAudio.PlayOneShot(reloadClip);

        lockFireAfterReload = true;
        nextFireTime = Time.time + postReloadDelay;
    }

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
        AttachMagByLevel();
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

    bool IsReloadAnimationPlaying()
    {
        if (!viewModelAnimator) return false;
        AnimatorStateInfo state = viewModelAnimator.GetCurrentAnimatorStateInfo(0);
        return state.IsName("Reload") && state.normalizedTime < 1f;
    }

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

    void ApplyMagColor(Color color)
    {
        if (currentMagVM)
            ApplyColorToMag(currentMagVM, color);

        if (currentMagFB)
            ApplyColorToMag(currentMagFB, color);
    }

    void ApplyColorToMag(GameObject mag, Color color)
    {
        var renderers = mag.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                    mat.color = color;
            }
        }
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

    void AttachMagByLevel()
    {
        if (magLevelPrefabs.Length == 0) return;

        int index = Mathf.Clamp(level - 1, 0, magLevelPrefabs.Length - 1);
        GameObject prefab = magLevelPrefabs[index];

        if (currentMagVM) Destroy(currentMagVM);
        if (currentMagFB) Destroy(currentMagFB);

        if (magSocketVM)
        {
            currentMagVM = Instantiate(prefab, magSocketVM);
            currentMagVM.transform.localPosition = Vector3.zero;
            currentMagVM.transform.localRotation = Quaternion.identity;
            currentMagVM.transform.localScale = Vector3.one;
        }

        if (magSocketFB)
        {
            currentMagFB = Instantiate(prefab, magSocketFB);
            currentMagFB.transform.localPosition = Vector3.zero;
            currentMagFB.transform.localRotation = Quaternion.identity;
            currentMagFB.transform.localScale = Vector3.one;
        }
    }

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