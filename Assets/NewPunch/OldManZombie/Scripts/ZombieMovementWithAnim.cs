using UnityEngine;
using UnityEngine.AI;

public class ZombieMovementWithAnim : MonoBehaviour
{
    public Transform target;
    public float stoppingDistance = 1.5f;

    private Animator animator;
    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.updatePosition = true;   // để agent tự cập nhật vị trí
            agent.updateRotation = true;   // để agent tự quay mặt
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
        }
        else
        {
            Debug.LogWarning("Zombie không đứng trên NavMesh!");
        }
    }
}