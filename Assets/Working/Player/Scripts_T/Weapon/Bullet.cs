using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 60f;
    public float damage = 10f;
    public float lifeTime = 3f;

    public Renderer bulletRenderer;
    private Material bulletMaterial;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (bulletRenderer != null)
        {
            bulletMaterial = new Material(bulletRenderer.material);
            bulletRenderer.material = bulletMaterial;
            bulletMaterial.EnableKeyword("_EMISSION");
        }
    }

    void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        IDamageable dmg = collision.collider.GetComponent<IDamageable>();
        if (dmg != null) dmg.TakeDamage(damage);

        Destroy(gameObject);
    }

    public void SetColor(Color color)
    {
        if (bulletMaterial != null)
        {
            bulletMaterial.color = color;
            bulletMaterial.SetColor("_EmissionColor", color * 4f);
        }
    }

    public void SetBulletSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb != null)
            rb.linearVelocity = transform.forward * speed;
    }
}
