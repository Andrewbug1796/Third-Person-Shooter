using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class ZombieController : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] ZombieHealthScript healthScript;

    [Header("Target")]
    [SerializeField] private Transform _player;

    [Header("Movement")]
    public float walkSpeed = 0.8f;
    public float crawlSpeed = 0.3f;
    public float stopDistance = 2;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackRate = 1.5f;
    private float attackTimer;
    private PlayerHealth playerHealth;

    [Header("Boolean States")]
    public bool isCrawling;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        healthScript = GetComponent<ZombieHealthScript>();
        playerHealth = _player.GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (healthScript.isDead)
        {
            StopAndDisableAgent();
            return;
        }

        isCrawling = !healthScript.isStanding;
        agent.speed = isCrawling ?crawlSpeed : walkSpeed;

        float _distance = Vector3.Distance(transform.position, _player.position);
        if(_distance < stopDistance) 
        {
            agent.isStopped = true;

            // Hit the player every attackRate seconds while in range
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0 && playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                attackTimer = attackRate;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(_player.position);
        }
    }

    void StopAndDisableAgent()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
    }
}
