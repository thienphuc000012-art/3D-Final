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
    public Transform modelHolder;          // EMPTY object
    public GameObject[] gunLevelModels;    // prefab gun đã có màu sẵn

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

    // 🔥 màu đạn hiện tại (lấy từ gun)
    Color currentBulletColor = Color.white;

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

        Debug.Log($"[GUN INIT] Lv {level}");
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

        // DEBUG: nhấn L = lên đúng 1 level
        if (Input.GetKeyDown(KeyCode.L))
            DebugLevelUp();
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
    // DEBUG LEVEL UP
    // ==========================
    void DebugLevelUp()
    {
        level++;
        ApplyLevelStats();
        UpdateGunVisual();

        Debug.Log($"[DEBUG] Press L -> Lv {level}");
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
            b.SetColor(currentBulletColor); // ✅ màu đạn theo gun
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
    // EXP & LEVEL (DÙNG KHI BẮN ENEMY)
    // ==========================
    public void AddExp(float amount)
    {
        currentExp += amount;

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        expToNextLevel *= 1.5f;

        ApplyLevelStats();
        UpdateGunVisual();

        Debug.Log($"[GUN LEVEL UP] -> Lv {level}");
    }

    void ApplyLevelStats()
    {
        float dmgMul = 1f + (level - 1) * 1.0f;
        float speedMul = 1f + (level - 1) * 0.2f;
        float reloadMul = Mathf.Pow(0.8f, level - 1);
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

        // clear model cũ
        for (int i = modelHolder.childCount - 1; i >= 0; i--)
            Destroy(modelHolder.GetChild(i).gameObject);

        int index = Mathf.Clamp(level - 1, 0, gunLevelModels.Length - 1);

        Debug.Log($"[GUN MODEL] Level {level} -> index {index}");

        GameObject model = Instantiate(gunLevelModels[index], modelHolder);

        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        // ✅ lấy màu từ gun để gán cho bullet
        CacheBulletColorFromModel(model);
    }

    // ==========================
    // GET COLOR FROM GUN MODEL
    // ==========================
    void CacheBulletColorFromModel(GameObject model)
    {
        Renderer r = model.GetComponentInChildren<Renderer>();
        if (!r) return;

        Material mat = r.material;

        if (mat.HasProperty("_BaseColor"))
            currentBulletColor = mat.GetColor("_BaseColor");
        else if (mat.HasProperty("_Color"))
            currentBulletColor = mat.GetColor("_Color");
        else
            currentBulletColor = Color.white;

        Debug.Log($"[BULLET COLOR] {currentBulletColor}");
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
