using System;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{

    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private PlayerMovement PlayerController;
     private Health health;

    public bool enteredRoom = false;
    public bool Attacking = false;


    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 4.5f;
    float LastTimeAttacked;

    [Header("chase")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (!enteredRoom)
        {
            agent.isStopped = true;

            animator.SetBool("isRunning", false);

            return;
        }


        float Distance = Vector3.Distance(transform.position, PlayerController.transform.position);
        if (Distance <= chaseRange && enteredRoom && !Attacking)
        {
            agent.SetDestination(PlayerController.transform.position);
            animator.SetBool("isRunning", true);
            agent.speed = chaseSpeed;
        }
        else
        {
            animator.SetBool("isRunning", false);


        }
        if (Distance <= stoppingDistance)
        {
            Attacking = true;
            //agent.isStopped = true;

            

            if (Time.time - LastTimeAttacked >= attackCooldown)
            {
                animator.SetTrigger("Attack");
                PlayerController.Hit(damage);
                Debug.Log("Zombie Attacked Player for " + damage + " damage.");
                LastTimeAttacked = Time.time;
            }
            
        }
        else
        {
            Attacking = false;
            //agent.isStopped = false;
        }

    }



    private void OnEnable()
    {
        health.OnDied += HandleDeath;
    }
    private void OnDisable()
    {
        health.OnDied -= HandleDeath;
    }
    void HandleDeath()
    {

        animator.SetBool("isDead", true);
        agent.isStopped = true;
        this.enabled = false;
        Destroy(gameObject, 3f);

    }
    void Hit(int amount)
    {
        health.TakeDamage(amount);
    }
}