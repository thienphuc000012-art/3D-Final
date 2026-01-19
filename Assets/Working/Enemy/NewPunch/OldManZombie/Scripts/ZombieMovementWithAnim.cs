using System.Collections;
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
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

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
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.speed = moveSpeed;
        }
    }

    void Update()
    {
        if (isDead) return; // nếu đã chết thì không update nữa
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
                Destroy(gameObject);
            }
        }
    }
    public void TakeDamage(int damage)
    {
        if (health != null && !isDead)
        {
            health.TakeDamage(damage);

            // Chỉ gọi trigger nếu chưa ở trong state Hit
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("Hit")) // tên state Hit trong Animator
            {
                animator.SetTrigger("Hit");
            }

            if (health.GetCurrentHealth() <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " đã chết!");

        if (agent != null)
        {
            agent.enabled = false; // tắt hẳn NavMeshAgent
        }

        animator.SetTrigger("Die");

        // Xóa zombie sau khi animation Die chạy xong (ví dụ 3 giây)
        Destroy(gameObject, 3f);
    }
}