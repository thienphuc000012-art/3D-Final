using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    // ===================== Inspector Data =====================
    public GunData gunData;

    [Header("UI")]
    public GameObject crosshairUI;

    [Header("Audio")]
    public AudioSource shootAudio;
    public AudioSource reloadAudio;
    public AudioClip shootClip;
    public AudioClip reloadClip;

    [Header("Camera")]
    public Camera aimCamera;
    public float hipFOV = 70f;
    public float adsFOV = 45f;
    public float fovLerpSpeed = 10f;

    [Header("Viewmodel")]
    public Transform firePointVM;
    public ParticleSystem muzzleFlashVM;
    public Transform ADS_Parent;
    public Transform recoilPivot;
    public Transform modelHolderVM;
    public Transform modelHolderFB;

    [Header("Animator")]
    public Animator viewModelAnimator;
    public Animator fullBodyAnimator;

    [Header("Bullet Visual")]
    public GameObject bulletPrefab;

    [Header("Level Visual")]
    public GameObject[] gunLevelModels;

    [Header("Mag System")]
    public Transform magSocketVM;
    public Transform magSocketFB;
    public GameObject[] magLevelPrefabs;

    [Header("Mag Hand")]
    public Transform leftHandMagHoldVM;
    public Transform leftHandMagHoldFB;

    [Header("Reload Timing (0-1)")]
    public float magDetachTime = 0.20f;
    public float magSpawnTime = 0.30f;
    public float magAttachTime = 1f;

    [Header("ADS Settings")]
    public bool isAiming;
    public float hipSpread = 0.015f;
    public float adsSpread = 0f;
    public float aimSpeed = 12f;
    public Transform hipPosition;
    public Transform adsPosition;

    [Header("Camera Recoil")]
    public float hipCamRecoilUp = 1.2f;
    public float hipCamRecoilSide = 0.6f;
    public float adsCamRecoilUp = 0.15f;
    public float adsCamRecoilSide = 0.1f;

    [Header("Viewmodel Recoil")]
    public float hipRecoilAmount = 4f;
    public float hipRecoilBack = 0.07f;
    public float adsRecoilAmount = 1.1f;
    public float adsRecoilBack = 0.02f;
    public float recoilReturnSpeed = 8f;

    // ===================== Level =====================
    public int level = 1;
    public int maxLevel = 5;
    public int currentExp = 0;
    public int expToNextLevel = 10;

    // ===================== Runtime =====================
    int currentAmmo;
    float nextFireTime;
    bool isReloading;
    bool requireReleaseFire;
    bool lockFireAfterReload;
    bool forceHipByReload;
    bool canReload = true;

    float damage, fireCooldown, reloadTime, bulletSpeed;
    int magazineSize;

    Color currentBulletColor = Color.white;
    Vector3 vmRecoilCur, vmRecoilTarget;
    Vector3 fireModeJiggleRotation;

    float reloadElapsed;
    GameObject currentMagVM, currentMagFB;
    GameObject oldMagVM, oldMagFB;

    public float reloadCooldown = 1f;
    float postReloadDelay = 1f;

    public int burstCount = 3;
    bool isBurstFiring;

    public enum FireMode { Semi, Auto }
    public FireMode fireMode = FireMode.Auto;

    // ===================== Start =====================
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
        damage = gunData.damage;
        fireCooldown = gunData.fireRate;
        reloadTime = gunData.reloadTime;
        bulletSpeed = gunData.bulletSpeed;
        magazineSize = gunData.magazineSize;
        currentAmmo = magazineSize;
    }

    void ResetGunToLevel1()
    {
        level = 1;
        ApplyStatsByLevel();
    }

    // ===================== Update =====================
    void Update()
    {
        HandleInput();
        HandleADSAll();
        HandleViewmodelRecoil();

        if (CheckFireLocked()) return;
        if (TryReload()) return;
        if (HandleEmptyFire()) return;

        HandleFireProcess();
    }

    // ============================================================
    // ===================== INPUT ================================
    // ============================================================
    void HandleInput()
    {
        isAiming = forceHipByReload ? false : Input.GetButton("Fire2");
        viewModelAnimator?.SetBool("isAiming", isAiming);
        crosshairUI?.SetActive(!isAiming);

        if (Input.GetKeyDown(KeyCode.T))
        {
            fireMode = (fireMode == FireMode.Auto) ? FireMode.Semi : FireMode.Auto;
            Debug.Log("Fire Mode: " + fireMode);
            StartCoroutine(FireModeJiggle());
        }
    }

    bool CheckFireLocked()
    {
        if (!lockFireAfterReload) return false;
        if (Time.time >= nextFireTime) lockFireAfterReload = false;
        return lockFireAfterReload;
    }

    bool TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && canReload && !isReloading && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return true;
        }
        return false;
    }

    bool HandleEmptyFire()
    {
        if (currentAmmo > 0) return false;

        if (Input.GetButtonDown("Fire1") || Input.GetButton("Fire1"))
            StartCoroutine(Reload());

        return true;
    }

    // ============================================================
    // ===================== FIRE PROCESS ==========================
    // ============================================================
    void HandleFireProcess()
    {
        bool fireDown = Input.GetButtonDown("Fire1");
        bool fireHeld = Input.GetButton("Fire1");

        if (isReloading || requireReleaseFire)
        {
            if (!fireHeld) requireReleaseFire = false;
            return;
        }

        if (Time.time < nextFireTime) return;

        switch (fireMode)
        {
            case FireMode.Auto:
                if (fireHeld) Shoot();
                break;

            case FireMode.Semi:
                if (fireDown)
                {
                    Shoot();
                    requireReleaseFire = true;
                }
                break;
        }
    }

    // ============================================================
    // ===================== SHOOT ================================
    // ============================================================
    void Shoot()
    {
        nextFireTime = Time.time + fireCooldown;
        currentAmmo--;

        ApplyCameraRecoil();
        AddViewmodelRecoil();

        PlayAnim(viewModelAnimator, "Shoot");
        PlayAnim(fullBodyAnimator, "Shoot");

        muzzleFlashVM?.Play();
        shootAudio?.PlayOneShot(shootClip);

        Vector3 dir = GetShootDirection();

        if (bulletPrefab)
        {
            var obj = Instantiate(bulletPrefab, firePointVM.position, Quaternion.LookRotation(dir));
            var b = obj.GetComponent<Bullet>();
            if (b)
            {
                b.damage = damage;
                b.SetDirection(dir);
                b.SetBulletSpeed(bulletSpeed);
                b.SetColor(currentBulletColor);
            }
        }
    }

    Vector3 GetShootDirection()
    {
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!isAiming) return ApplySpread(ray, hipSpread);

        return Physics.Raycast(ray, out RaycastHit hit, 1000f)
            ? (hit.point - firePointVM.position).normalized
            : (ray.GetPoint(1000f) - firePointVM.position).normalized;
    }

    // ============================================================
    // ===================== ADS SYSTEM ===========================
    // ============================================================
    void HandleADSAll()
    {
        if (!ADS_Parent) return;

        Transform target = isAiming ? adsPosition : hipPosition;

        ADS_Parent.localPosition = Vector3.Lerp(
            ADS_Parent.localPosition, target.localPosition, Time.deltaTime * aimSpeed);

        ADS_Parent.localRotation = Quaternion.Slerp(
            ADS_Parent.localRotation, target.localRotation, Time.deltaTime * aimSpeed);

        float targetFOV = isAiming ? adsFOV : hipFOV;
        aimCamera.fieldOfView = Mathf.Lerp(
            aimCamera.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
    }

    Vector3 ApplySpread(Ray ray, float spread)
    {
        Vector2 c = Random.insideUnitCircle * spread;
        return (ray.direction + aimCamera.transform.right * c.x + aimCamera.transform.up * c.y).normalized;
    }

    // ============================================================
    // ===================== RECOIL ===============================
    // ============================================================
    void ApplyCameraRecoil()
    {
        var mouseLook = aimCamera.GetComponentInParent<MouseLook>();
        if (!mouseLook) return;

        if (isAiming) mouseLook.AddRecoil(adsCamRecoilUp, adsCamRecoilSide);
        else mouseLook.AddRecoil(hipCamRecoilUp, hipCamRecoilSide);
    }

    void AddViewmodelRecoil()
    {
        float r = isAiming ? adsRecoilAmount : hipRecoilAmount;
        float b = isAiming ? adsRecoilBack : hipRecoilBack;
        vmRecoilTarget += new Vector3(-r, Random.Range(-1f, 1f), b);
    }

    void HandleViewmodelRecoil()
    {
        vmRecoilCur = Vector3.Lerp(vmRecoilCur, vmRecoilTarget, Time.deltaTime * 25f);
        vmRecoilTarget = Vector3.Lerp(vmRecoilTarget, Vector3.zero, Time.deltaTime * recoilReturnSpeed);

        recoilPivot.localRotation = Quaternion.Euler(vmRecoilCur + fireModeJiggleRotation);
    }

    IEnumerator FireModeJiggle()
    {
        Vector3 jig = new Vector3(Random.Range(-2f, 2f), Random.Range(-4f, 4f), 0f);
        yield return LerpRot(Vector3.zero, jig, 10);
        yield return LerpRot(jig, Vector3.zero, 8);
        fireModeJiggleRotation = Vector3.zero;
    }

    IEnumerator LerpRot(Vector3 from, Vector3 to, float speed)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            fireModeJiggleRotation = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }

    // ============================================================
    // ===================== RELOAD SYSTEM ========================
    // ============================================================
    IEnumerator Reload()
    {
        if (isReloading || !canReload) yield break;

        isReloading = true;
        canReload = false;
        forceHipByReload = true;
        requireReleaseFire = true;

        reloadElapsed = 0f;
        ResetMagFlags();

        PlayAnim(viewModelAnimator, "Reload");
        PlayAnim(fullBodyAnimator, "Reload");

        while (reloadElapsed < reloadTime)
        {
            reloadElapsed += Time.deltaTime;
            ProcessMagReload(reloadElapsed / reloadTime);
            yield return null;
        }

        ProcessMagReload(1f);
        FinishReload();

        yield return new WaitForSeconds(reloadCooldown);
        canReload = true;
    }

    bool magDetached, magSpawned, magAttached;

    void ResetMagFlags()
    {
        magDetached = magSpawned = magAttached = false;
    }

    void ProcessMagReload(float t)
    {
        if (!magDetached && t >= magDetachTime) { magDetached = true; DetachMag(); }
        if (!magSpawned && t >= magSpawnTime) { magSpawned = true; SpawnMag(); }
        if (!magAttached && t >= magAttachTime) { magAttached = true; AttachNewMag(); }
    }

    void DetachMag()
    {
        oldMagVM = currentMagVM;
        oldMagFB = currentMagFB;

        if (oldMagVM) AttachTo(oldMagVM.transform, leftHandMagHoldVM);
        if (oldMagFB) AttachTo(oldMagFB.transform, leftHandMagHoldFB);
    }

    void SpawnMag()
    {
        Destroy(oldMagVM);
        Destroy(oldMagFB);

        int idx = Mathf.Clamp(level - 1, 0, magLevelPrefabs.Length - 1);

        currentMagVM = Instantiate(magLevelPrefabs[idx]);
        currentMagFB = Instantiate(magLevelPrefabs[idx]);

        AttachTo(currentMagVM.transform, leftHandMagHoldVM);
        AttachTo(currentMagFB.transform, leftHandMagHoldFB);
    }

    void AttachNewMag()
    {
        if (currentMagVM) AttachTo(currentMagVM.transform, magSocketVM);
        if (currentMagFB) AttachTo(currentMagFB.transform, magSocketFB);
    }

    void FinishReload()
    {
        currentAmmo = magazineSize;
        isReloading = false;
        forceHipByReload = false;

        reloadAudio?.PlayOneShot(reloadClip);

        lockFireAfterReload = true;
        nextFireTime = Time.time + postReloadDelay;
    }

    void AttachTo(Transform obj, Transform parent)
    {
        if (!obj || !parent) return;
        obj.SetParent(parent, false);
        obj.localPosition = Vector3.zero;
        obj.localRotation = Quaternion.identity;
        obj.localScale = Vector3.one;
    }

    // ============================================================
    // ===================== LEVEL SYSTEM =========================
    // ============================================================
    void ApplyStatsByLevel()
    {
        int lv = level - 1;
        damage = gunData.damage * (1 + 0.2f * lv);
        fireCooldown = gunData.fireRate * Mathf.Pow(0.9f, lv);
        reloadTime = gunData.reloadTime * Mathf.Pow(0.9f, lv);
        bulletSpeed = gunData.bulletSpeed * (1 + 0.15f * lv);
        magazineSize = gunData.magazineSize + lv * 5;
        currentAmmo = magazineSize;
    }

    void UpdateGunVisual()
    {
        int i = Mathf.Clamp(level - 1, 0, gunLevelModels.Length - 1);
        ReplaceModel(modelHolderVM, gunLevelModels[i]);
        ReplaceModel(modelHolderFB, gunLevelModels[i]);

        var mesh = gunLevelModels[i].GetComponentInChildren<MeshRenderer>();
        if (mesh) currentBulletColor = mesh.sharedMaterial.color;
    }

    void ReplaceModel(Transform holder, GameObject prefab)
    {
        foreach (Transform c in holder) Destroy(c.gameObject);

        var obj = Instantiate(prefab, holder);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void AttachMagByLevel()
    {
        int i = Mathf.Clamp(level - 1, 0, magLevelPrefabs.Length - 1);

        currentMagVM = Instantiate(magLevelPrefabs[i], magSocketVM);
        currentMagFB = Instantiate(magLevelPrefabs[i], magSocketFB);

        AttachTo(currentMagVM.transform, magSocketVM);
        AttachTo(currentMagFB.transform, magSocketFB);
    }

    void PlayAnim(Animator anim, string trigger)
    {
        if (!anim) return;
        anim.ResetTrigger(trigger);
        anim.SetTrigger(trigger);
    }

    void LogStats()
    {
        Debug.Log($"[GUN] Lv {level} | DMG {damage} | FireCD {fireCooldown} | Reload {reloadTime} | Mag {magazineSize}");
    }
}