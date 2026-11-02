
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class SkeletonAttack
{
    public string animationTrigger = "AttackStab";

    public float duration = 1f;
    public float hitTime = 0.4f;
    public float swingSoundTime = 0.2f;

    public AudioClip swingSound;
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class SkeletonMovement : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public AudioSource audioSource;

    [Header("Movement")]
    public float chaseSpeed = 0.8f;
    public float pathUpdateInterval = 0.25f;

    [Header("Attacking")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    public SkeletonAttack[] attacks =
    {
        new SkeletonAttack
        {
            animationTrigger = "AttackStab"
        },
        new SkeletonAttack
        {
            animationTrigger = "AttackSlash1"
        },
        new SkeletonAttack
        {
            animationTrigger = "AttackSlash2"
        }
    };

    [Header("Spawning")]
    public float spawnAnimationDuration = 2f;
    public AudioClip spawnSound;

    [Header("Walking Audio")]
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.5f;

    private NavMeshAgent agent;
    private PlayerLives playerLives;

    private bool spawnFinished = false;
    private bool isAttacking = false;

    private float nextPathUpdate = 0f;
    private float nextAttackTime = 0f;
    private float nextFootstepTime = 0f;

    private int lastAttackIndex = -1;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        agent.speed = chaseSpeed;
        agent.stoppingDistance = attackRange * 0.8f;

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.autoRepath = true;

        if (animator != null)
            animator.applyRootMotion = false;
    }

    void Start()
    {
        // Find the Player
        if (player == null)
        {
            GameObject target =
                GameObject.FindGameObjectWithTag("Player");

            if (target != null)
                SetPlayer(target.transform);
        }
        else
        {
            SetPlayer(player);
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(
                "Skeleton is not on the NavMesh!");

            enabled = false;
            return;
        }

        // Stay still during spawn animation
        agent.isStopped = true;
        SetRunning(false);

        StartCoroutine(SpawnSequence());
    }

    // Called by the existing Skeleton spawner
    public void SetPlayer(Transform target)
    {
        player = target;

        playerLives = player != null
            ? player.GetComponentInParent<PlayerLives>()
            : null;

        if (playerLives == null)
            Debug.LogWarning("PlayerLives not found!");
    }

    // SPAWN ANIMATION AND SOUND
    IEnumerator SpawnSequence()
    {
        Debug.Log("Skeleton spawning!");

        if (animator != null)
            animator.SetTrigger("Spawn");

        PlaySound(spawnSound);

        yield return new WaitForSeconds(
            spawnAnimationDuration);

        spawnFinished = true;
        agent.isStopped = false;
        nextPathUpdate = 0f;

        Debug.Log("Skeleton started chasing!");
    }

    void Update()
    {
        if (!spawnFinished || isAttacking)
            return;

        if (player == null || !agent.isOnNavMesh)
        {
            StopAndIdle();
            return;
        }

        // Follow the Player through the maze
        if (Time.time >= nextPathUpdate)
        {
            agent.SetDestination(player.position);

            nextPathUpdate = Time.time +
                Mathf.Max(0.1f, pathUpdateInterval);
        }

        if (agent.pathPending)
        {
            UpdateMovementAnimation();
            return;
        }

        bool validPath =
            agent.hasPath &&
            agent.pathStatus ==
                NavMeshPathStatus.PathComplete;

        if (!validPath)
        {
            StopAndIdle();
            return;
        }

        agent.isStopped = false;

        float distance = Vector3.Distance(
            transform.position, player.position);

        // Only attack when within reachable range
        bool canAttack =
            distance <= attackRange &&
            agent.remainingDistance <=
                attackRange + 0.2f;

        if (canAttack &&
            Time.time >= nextAttackTime &&
            playerLives != null &&
            playerLives.CanBeHit &&
            attacks != null &&
            attacks.Length > 0)
        {
            StartCoroutine(AttackSequence());
            return;
        }

        UpdateMovementAnimation();
    }

    // RANDOM ATTACK SELECTION
    SkeletonAttack ChooseAttack()
    {
        int index = Random.Range(0, attacks.Length);

        // Avoid repeating the same attack consecutively
        if (attacks.Length > 1 &&
            index == lastAttackIndex)
        {
            int offset = Random.Range(1, attacks.Length);
            index = (index + offset) % attacks.Length;
        }

        lastAttackIndex = index;
        return attacks[index];
    }

    // ATTACK ANIMATION, AUDIO AND DAMAGE
    IEnumerator AttackSequence()
    {
        isAttacking = true;

        SkeletonAttack attack = ChooseAttack();

        if (attack == null)
        {
            isAttacking = false;
            yield break;
        }

        float duration = Mathf.Max(0.1f, attack.duration);

        float hitTime = Mathf.Clamp(
            attack.hitTime, 0f, duration);

        float swingTime = Mathf.Clamp(
            attack.swingSoundTime, 0f, hitTime);

        nextAttackTime = Time.time +
            duration + attackCooldown;

        // Stop moving before attacking
        agent.isStopped = true;
        SetRunning(false);

        FacePlayer();

        // Trigger selected attack animation
        if (animator != null)
        {
            animator.SetTrigger(attack.animationTrigger);
        }

        Debug.Log("Skeleton attack: " +
                  attack.animationTrigger);

        // Wait until weapon begins swinging
        yield return new WaitForSeconds(swingTime);

        PlaySound(attack.swingSound);

        // Wait until weapon reaches the Player
        yield return new WaitForSeconds(
            Mathf.Max(0f, hitTime - swingTime));

        bool hitConfirmed = false;

        if (player != null && playerLives != null)
        {
            float distance = Vector3.Distance(
                transform.position, player.position);

            if (distance <= attackRange &&
                playerLives.CanBeHit)
            {
                hitConfirmed = true;

                Debug.Log("Skeleton attack connected!");
            }
        }

        // Let the rest of the attack animation finish
        yield return new WaitForSeconds(
            Mathf.Max(0f, duration - hitTime));

        // Deduct life after the animation
        if (hitConfirmed && playerLives != null)
        {
            playerLives.LoseLife();
        }

        isAttacking = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            nextPathUpdate = 0f;
        }
    }

    // FACE THE PLAYER
    void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    // MOVEMENT AND FOOTSTEPS
    void UpdateMovementAnimation()
    {
        bool moving =
            !agent.isStopped &&
            agent.velocity.sqrMagnitude > 0.02f;

        SetRunning(moving);

        if (moving)
        {
            if (Time.time >= nextFootstepTime &&
                footstepSounds != null &&
                footstepSounds.Length > 0)
            {
                int index = Random.Range(
                    0, footstepSounds.Length);

                PlaySound(footstepSounds[index]);

                nextFootstepTime = Time.time +
                    Mathf.Max(0.1f, footstepInterval);
            }
        }
        else
        {
            nextFootstepTime = Time.time;
        }
    }

    // IDLE
    void StopAndIdle()
    {
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        SetRunning(false);
        nextFootstepTime = Time.time;
    }

    void SetRunning(bool running)
    {
        if (animator != null)
            animator.SetBool("IsRunning", running);
    }

    // AUDIO HELPER
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
