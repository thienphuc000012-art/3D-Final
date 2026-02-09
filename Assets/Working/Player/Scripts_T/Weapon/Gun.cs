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

    [Header("ADS (Aiming)")]
    public bool isAiming;
    public float hipSpread = 0.015f;
    public float adsSpread = 0f;

    [Header("CAMERA RECOIL")]
    public float hipCamRecoilUp = 1.2f;
    public float hipCamRecoilSide = 0.6f;
    public float adsCamRecoilUp = 0.15f;
    public float adsCamRecoilSide = 0.1f;


    // ===================== IRON-SIGHT ADS =====================
    [Header("IRON-SIGHT POSITIONS")]
    public Transform hipPosition;   // vị trí súng bình thường
    public Transform adsPosition;   // vị trí iron-sight (ngắm thẳng tâm ruồi)
    public float aimSpeed = 10f;    // tốc độ chuyển ADS


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

    // SAVE VỊ TRÍ GỐC (FIX DRIFT)
    Vector3 initialLocalPos;
    Quaternion initialLocalRot;

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
        reloadStateHash = Animator.StringToHash("Reloading");

        CacheBaseStats();
        ResetGunToLevel1();
        UpdateGunVisual();
        LogStats();

        initialLocalPos = modelHolderVM.localPosition;
        initialLocalRot = modelHolderVM.localRotation;
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
                StartCoroutine(Reload());
        }

        isAiming = Input.GetButton("Fire2");

        // 🎯 IRONSIGHT TRANSITION
        HandleADS();
    }



    // ===================== SHOOT =====================
    void Shoot()
    {
        if (isReloading) return;

        nextFireTime = Time.time + fireCooldown;
        currentAmmo--;

        // CAMERA RECOIL (không ảnh hưởng hướng đạn!)
        MouseLook mouseLook = aimCamera.GetComponentInParent<MouseLook>();
        if (mouseLook)
        {
            if (!isAiming)
                mouseLook.AddRecoil(hipCamRecoilUp, hipCamRecoilSide);
            else
                mouseLook.AddRecoil(adsCamRecoilUp, adsCamRecoilSide);
        }

        // Animation + Muzzle
        PlayAnim(viewModelAnimator, "Shoot");
        PlayAnim(fullBodyAnimator, "Shoot");
        muzzleFlashVM?.Play();

        Vector3 dir;

        if (!isAiming)
        {
            // 🎯 HIP-FIRE: random circle spread
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            dir = ApplySpread(ray.direction, hipSpread);
        }
        else
        {
            // 🎯 ADS: đạn đi chính xác vào *nơi crosshair chỉ*
            Ray camRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

            Vector3 targetPoint;

            if (Physics.Raycast(camRay, out RaycastHit hit, 1000f))
                targetPoint = hit.point;
            else
                targetPoint = camRay.GetPoint(1000f);

            // DIRECTION từ nòng → target
            dir = (targetPoint - firePointVM.position).normalized;
        }


        // Sound
        if (shootAudio && shootClip)
        {
            shootAudio.pitch = Random.Range(0.95f, 1.05f);
            shootAudio.PlayOneShot(shootClip);
        }


        // SPAWN BULLET
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



    // ===================== IRON-SIGHT HANDLER =====================
    void HandleADS()
    {
        if (!hipPosition || !adsPosition) return;

        if (isAiming)
        {
            // Move vào iron-sight
            modelHolderVM.localPosition =
                Vector3.Lerp(modelHolderVM.localPosition, adsPosition.localPosition, Time.deltaTime * aimSpeed);

            modelHolderVM.localRotation =
                Quaternion.Slerp(modelHolderVM.localRotation, adsPosition.localRotation, Time.deltaTime * aimSpeed);
        }
        else
        {
            // Move về hip-fire
            modelHolderVM.localPosition =
                Vector3.Lerp(modelHolderVM.localPosition, hipPosition.localPosition, Time.deltaTime * aimSpeed);

            modelHolderVM.localRotation =
                Quaternion.Slerp(modelHolderVM.localRotation, hipPosition.localRotation, Time.deltaTime * aimSpeed);
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
        if (isReloading) yield break;

        isReloading = true;
        requireReleaseFire = true;

        PlayAnim(viewModelAnimator, "Reload");
        PlayAnim(fullBodyAnimator, "Reload");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        if (reloadAudio && reloadClip)
        {
            reloadAudio.pitch = 1f;
            reloadAudio.PlayOneShot(reloadClip);
        }
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



    // ===================== VISUAL MODEL =====================
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