using UnityEngine;
using UnityEngine.AI;

public class ZombieMovementWithAnim : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float stoppingDistance = 1.5f;
    public float moveSpeed = 2f;

    [Header("Health Settings")]
    public int maxHealth = 50;       
    public int startHealth = 50;     

    private Animator animator;
    private NavMeshAgent agent;
    private Health health;   // biến health cho enemy

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();   // lấy component Health

        // Setup máu cho enemy theo giá trị Inspector
        if (health != null)
        {
            health.SetUp(startHealth, maxHealth);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }

        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.updatePosition = true;   // để agent tự cập nhật vị trí
            agent.updateRotation = true;   // để agent tự quay mặt
            agent.speed = moveSpeed;
        }
    }

    void Update()
    {
        if (target == null || agent == null || animator == null) return;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);

            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Run", currentSpeed);

            // Kiểm tra máu
            if (health != null && health.GetCurrentHealth() <= 0)
            {
                Die();
            }
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= stoppingDistance)
            {
                Debug.Log(gameObject.name + " đã chạm vào Player!");
                Destroy(gameObject); // Xóa zombie khi chạm Player
            }

        }
    }

    public void TakeDamage(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " đã chết!");
        Destroy(gameObject);
    }
}