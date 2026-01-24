using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 60f;
    public float damage = 10f;
    public float lifeTime = 3f;

    [Header("Visual")]
    public Renderer bulletRenderer;

    Material bulletMaterial;
    Rigidbody rb;

    Vector3 moveDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (bulletRenderer != null)
        {
            bulletMaterial = new Material(bulletRenderer.material);
            bulletRenderer.material = bulletMaterial;
            bulletMaterial.EnableKeyword("_EMISSION");
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // GỌI TỪ Gun.cs
    public void SetDirection(Vector3 dir)
    {
        moveDir = dir.normalized;
        rb.linearVelocity = moveDir * speed;
    }

    public void SetBulletSpeed(float newSpeed)
    {
        speed = newSpeed;
        rb.linearVelocity= moveDir * speed;
    }

    public void SetColor(Color color)
    {
        if (bulletMaterial != null)
        {
            bulletMaterial.color = color;
            bulletMaterial.SetColor("_EmissionColor", color * 4f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Nếu collider có ZombieHitbox thì gọi ApplyDamage
        ZombieHitbox hitbox = collision.collider.GetComponent<ZombieHitbox>();
        if (hitbox != null)
        {
            hitbox.ApplyDamage((int)damage);
        }
        else
        {
            // fallback: nếu có Health trực tiếp thì trừ máu
            Health health = collision.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage((int)damage);
            }
        }

        Destroy(gameObject);
    }
}
