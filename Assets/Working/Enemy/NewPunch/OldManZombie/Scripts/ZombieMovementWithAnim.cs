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
    private Health health;

    private ZombieSoundManager soundManager;
    [Header("Blood Effect")]
    public GameObject bloodEffectPrefab;

    [Header("Zombie Type")]
    public int zombieTypeIndex;
    public ZombieData zombieData;

    private bool isDead = false;
    private bool isAttacking = false;

    public PlayerHealth sharedPlayerHealth;
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        soundManager = GetComponent<ZombieSoundManager>();

        if (health != null)
        {
            health.SetUp(startHealth, maxHealth);
        }


        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }

        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.speed = moveSpeed;
        }
        if (soundManager != null) soundManager.PlayWalk();

    }

    void Update()
    {
        if (isDead) return;
        if (target == null || agent == null || animator == null) return;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);

            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Run", currentSpeed);

            if (health != null && health.GetCurrentHealth() <= 0)
            {
                Die();
            }

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= stoppingDistance && !isAttacking)
            {
                AttackPlayer();
            }
        }
    }

    public void TakeDamage(int damage,  Vector3 hitPoint)
    {
        if (health != null && !isDead)
        {
            health.TakeDamage(damage);
            if (bloodEffectPrefab != null)
            {
                GameObject blood = Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
                Destroy(blood, 1f);
            }


            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("Hit"))
            {
                animator.SetTrigger("Hit");
                if (soundManager != null) soundManager.PlayHit();
            }

            if (health.GetCurrentHealth() <= 0)
            {
                Die();
            }
        }
    }

    private void AttackPlayer()
    {
        isAttacking = true;
        agent.isStopped = true;
        animator.SetTrigger("Attack");
        if (soundManager != null) soundManager.PlayAttack();


        //Health playerHealth = target.GetComponent<Health>();
        if (sharedPlayerHealth != null)
        {
            sharedPlayerHealth.TakeDamage(10); // ✅ tất cả zombie gọi chung PlayerHealth
        }


        StartCoroutine(ResetAttack());
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(2f);
        isAttacking = false;
        if (!isDead && agent != null)
        {
            agent.isStopped = false;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " đã chết!");

        if (agent != null)
        {
            agent.enabled = false;
        }

        animator.SetTrigger("Die");
        if (soundManager != null) soundManager.PlayDeath();

        Destroy(gameObject, 3f);
    }
}