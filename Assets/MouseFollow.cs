using System.Collections;
using UnityEngine;

// Oringinally this was supposed to be a mouse but I ended up using a rat instead. The name stuck though.
[RequireComponent(typeof(Rigidbody))]
public class MouseFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float stoppingDistance = 0.3f;
    [SerializeField] private float updateInterval = 3f;

    [Header("Random Timing")]
    [SerializeField] private float randomStartDelay = 2f;
    [SerializeField] private float intervalVariation = 0.5f;

    [Header("Spin Attack")]
    [SerializeField] private float spinDuration = 1f;
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float attackCooldown = 0.5f;

    private Rigidbody rb;
    private Vector3 lastKnownPosition;
    private bool hasTargetPosition;
    private bool isSpinning;
    private bool damagedDuringSpin;
    private float nextAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
    }

    private IEnumerator Start()
    {
        // Wait for a random delay before starting the movement logic to avoid all rats moving at the same time
        yield return new WaitForSeconds(
            Random.Range(0f, randomStartDelay)
        );

        while (true)
        {
            if (target != null)
            {
                lastKnownPosition = target.position;
                lastKnownPosition.y = rb.position.y;
                hasTargetPosition = true;
            }

            float delay = updateInterval +
                          Random.Range(
                              -intervalVariation,
                              intervalVariation
                          );

            yield return new WaitForSeconds(Mathf.Max(0.1f, delay));
        }
    }

    private void FixedUpdate()
    {
        // If the rat is currently spinning (attacking) or doesn't have a target position, it won't move towards the target.
        if (isSpinning || !hasTargetPosition)
            return;

        Vector3 direction = lastKnownPosition - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Vector3 moveDirection = direction.normalized;

        // Face the latest target position.
        rb.MoveRotation(Quaternion.LookRotation(moveDirection));

        // Reached the position it was chasing.
        if (direction.magnitude <= stoppingDistance)
        {
            StartSpin();
            return;
        }

        rb.MovePosition(
            rb.position +
            moveDirection * moveSpeed * Time.fixedDeltaTime
        );
    }

    // When the rat collides with the cat, it starts spinning (attacking).
    private void OnCollisionEnter(Collision collision)
    {
        if (IsCat(collision.transform))
        {
            StartSpin();
        }
    }

    // When the rat is spinning and collides with the cat, it deals damage to the cat.
    private void OnCollisionStay(Collision collision)
    {
        if (!isSpinning || damagedDuringSpin)
            return;

        if (IsCat(collision.transform))
        {
            CatMovement catMovement =
                collision.transform.GetComponentInParent<CatMovement>();

            // Rolling gives the cat temporary immunity.
            if (catMovement != null &&
                (
                catMovement.IsRolling || (catMovement.IsPunching && catMovement.IsFacing(transform))
                ))
            {
                return;
            }

            // If the cat is not rolling or punching, it takes damage from the rat's spin attack.
            Health catHealth =
                collision.transform.GetComponentInParent<Health>();

            if (catHealth != null)
            {
                catHealth.TakeDamage(damage);
                damagedDuringSpin = true;
            }
        }
    }

    // Check if the collided object is the cat by comparing the root transforms.
    private bool IsCat(Transform other)
    {
        return target != null &&
               other.root == target.root;
    }

    // Start the spin attack if the rat is not already spinning and the attack cooldown has passed.
    private void StartSpin()
    {
        if (isSpinning || Time.time < nextAttackTime)
            return;

        StartCoroutine(SpinAttack());
    }

    // Coroutine that handles the spin attack behavior, including rotation and timing.
    private IEnumerator SpinAttack()
    {
        isSpinning = true;
        damagedDuringSpin = false;

        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            rb.MoveRotation(
                rb.rotation *
                Quaternion.Euler(
                    0f,
                    spinSpeed * Time.fixedDeltaTime,
                    0f
                )
            );

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isSpinning = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}