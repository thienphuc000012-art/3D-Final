using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    // ==========================
    // DATA
    // ==========================
    [Header("DATA")]
    public GunData gunData;

    [Header("AIM")]
    public Camera aimCamera;

    // ==========================
    // VIEWMODEL (FPS)
    // ==========================
    [Header("VIEWMODEL")]
    public Transform firePointVM;
    public ParticleSystem muzzleFlashVM;
    public Animator viewModelAnimator;
    public Transform modelHolderVM;

    // ==========================
    // FULL BODY (3RD PERSON)
    // ==========================
    [Header("FULL BODY")]
    public Animator fullBodyAnimator;
    public Transform modelHolderFB;

    [Header("SPINE AIM (FULL BODY ONLY)")]
    public Transform spineBone;
    public Transform cameraHolder;
    [Range(0f, 1f)] public float spineWeight = 0.3f;
    public float maxSpinePitch = 40f;

    // ==========================
    // BULLET
    // ==========================
    [Header("BULLET")]
    public GameObject bulletPrefab;

    // ==========================
    // LEVEL / VISUAL
    // ==========================
    [Header("LEVEL VISUAL")]
    public GameObject[] gunLevelModels;

    int currentAmmo;
    float nextFireTime;
    bool isReloading;
    int level = 1;

    Color currentBulletColor = Color.white;

    // ==========================
    // INIT
    // ==========================
    void Start()
    {
        currentAmmo = gunData.magazineSize;
        UpdateGunVisual();
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

        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(Reload());
    }

    // ==========================
    // SPINE AIM (FULL BODY ONLY)
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
        if (!aimCamera || !firePointVM || !bulletPrefab) return;

        nextFireTime = Time.time + gunData.fireRate;
        currentAmmo--;

        // ▶ animation
        if (viewModelAnimator && viewModelAnimator.runtimeAnimatorController)
            viewModelAnimator.SetTrigger("Shoot");

        if (fullBodyAnimator && fullBodyAnimator.runtimeAnimatorController)
            fullBodyAnimator.SetTrigger("Shoot");

        if (muzzleFlashVM)
            muzzleFlashVM.Play();

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Vector3 dir = ray.direction;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePointVM.position,
            Quaternion.LookRotation(dir)
        );

        Bullet b = bullet.GetComponent<Bullet>();
        if (b)
        {
            b.damage = gunData.damage;
            b.SetDirection(dir);
            b.SetBulletSpeed(gunData.bulletSpeed);
            b.SetColor(currentBulletColor);
        }
    }

    // ==========================
    // RELOAD
    // ==========================
    IEnumerator Reload()
    {
        if (isReloading) yield break;
        isReloading = true;

        if (viewModelAnimator && viewModelAnimator.runtimeAnimatorController)
            viewModelAnimator.SetTrigger("Reload");

        if (fullBodyAnimator && fullBodyAnimator.runtimeAnimatorController)
            fullBodyAnimator.SetTrigger("Reload");

        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.magazineSize;
        isReloading = false;
    }

    // ==========================
    // VISUAL
    // ==========================
    void UpdateGunVisual()
    {
        int index = Mathf.Clamp(level - 1, 0, gunLevelModels.Length - 1);

        ReplaceModel(modelHolderVM, gunLevelModels[index]);
        ReplaceModel(modelHolderFB, gunLevelModels[index]);

        CacheBulletColorFromModel(gunLevelModels[index]);
    }

    void ReplaceModel(Transform holder, GameObject prefab)
    {
        if (!holder || !prefab) return;

        foreach (Transform child in holder)
        {
            if (child.name.Contains("Gun") || child.name.Contains("Weapon"))
                Destroy(child.gameObject);
        }

        GameObject m = Instantiate(prefab, holder);
        m.transform.localPosition = Vector3.zero;
        m.transform.localRotation = Quaternion.identity;
        m.transform.localScale = Vector3.one;
    }


    void CacheBulletColorFromModel(GameObject model)
    {
        Renderer r = model.GetComponentInChildren<Renderer>();
        if (!r) return;

        Material mat = r.sharedMaterial;
        if (mat.HasProperty("_BaseColor"))
            currentBulletColor = mat.GetColor("_BaseColor");
        else if (mat.HasProperty("_Color"))
            currentBulletColor = mat.GetColor("_Color");
    }
}
