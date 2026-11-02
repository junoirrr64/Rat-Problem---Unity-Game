using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class CatMovement : MonoBehaviour
{
    [SerializeField] private float rollDuration = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float punchDuration = 0.6f;
    [SerializeField] private float punchDamage = 1f;
    [SerializeField] private float punchRange = 1f;
    [SerializeField] private float punchRadius = 0.7f;
    [SerializeField] private float punchHitDelay = 0.2f;
    [SerializeField] private AudioClip punchHitSound;

    private bool isPunching;
    private bool isRolling;

    // Public properties to check if the cat is currently punching 
    public bool IsPunching => isPunching;
    public bool IsRolling => isRolling;

    private Rigidbody rb;
    private Quaternion targetRotation;
    private Vector3 currentMovement;
    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        targetRotation = transform.rotation;
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Handle input for punching and rolling
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            !isPunching &&
            !isRolling)
        {
            StartCoroutine(Punch());
        }

        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame &&
            !isPunching &&
            !isRolling)
        {
            StartCoroutine(Roll());
        }

        // Update the cat's facing direction based on mouse position
        // Only update facing direction if the cat is not punching or rolling
        if (!isPunching && !isRolling)
        {
            FaceMouse();
        }
    }

    private void FixedUpdate()
    {

        // Handle movement input and update the cat's position
        Move();

        if (isRolling || isPunching)
            return;

        rb.MoveRotation(targetRotation);
        UpdateAnimation();
    }

    private void Move()
    {

        // Get input from the keyboard for movement
        Vector2 input = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1;

        // Calculate movement direction based on camera orientation
        // I did this because I played around with the camera and I wanted the cat to move in the direction of the camera
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        currentMovement =
            cameraForward * input.y +
            cameraRight * input.x;

        if (currentMovement.sqrMagnitude > 1f)
            currentMovement.Normalize();

        rb.MovePosition(
            rb.position +
            currentMovement * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void FaceMouse()
    {
        // Check if the mouse and camera are available before proceeding
        if (Mouse.current == null || Camera.main == null)
            return;

        // Create a ray from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );


        Plane groundPlane =
            new Plane(Vector3.up, Vector3.zero);

        // Check if the ray intersects with the ground plane
        if (groundPlane.Raycast(ray, out float distance))
        {
            // Calculate the point on the ground plane where the ray intersects
            Vector3 mousePosition =
                ray.GetPoint(distance);

            // Calculate the direction from the cat to the mouse position and update the target rotation
            Vector3 direction =
                mousePosition - transform.position;

            direction.y = 0f;

            // Only update the target rotation if the direction is significant to avoid jittering
            if (direction.sqrMagnitude > 0.01f)
            {
                targetRotation =
                    Quaternion.LookRotation(direction);
            }
        }
    }

    private void UpdateAnimation()
    {
        // Check if the animator is available before proceeding
        if (animator == null)
            return;

        // Determine if the cat is moving based on the current movement vector
        bool isMoving =
            currentMovement.sqrMagnitude > 0.01f;

        // Update the animator parameters based on the cat's movement state
        if (!isMoving)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        animator.SetFloat("Speed", 1f);

        // Calculate the facing direction of the cat based on the target rotation and current movement
        // This is used to determine the direction of movement relative to the facing direction (forward or backward)
        Vector3 facingDirection =
            targetRotation * Vector3.forward;

        float forwardAmount =
            Vector3.Dot(
                facingDirection.normalized,
                currentMovement.normalized
            );

        animator.SetFloat(
            "Direction",
            forwardAmount >= 0f ? 1f : -1f
        );
    }

    // Handle the rolling action of the cat
    private IEnumerator Roll()
    {
        isRolling = true;
        // Stop the cat's movement during the roll action
        currentMovement = Vector3.zero;

        // Trigger the roll animation if the animator is available
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Roll");
        }

        // Wait for the roll duration before allowing the cat to roll again
        yield return new WaitForSeconds(rollDuration);

        // Reset the rolling state after the roll is complete
        isRolling = false;
    }

    private IEnumerator Punch()
    {
        isPunching = true;
        // Stop the cat's movement during the punch action
        currentMovement = Vector3.zero;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Punch");
        }

        yield return new WaitForSeconds(punchHitDelay);

        // Perform a punch attack by checking for targets within the punch range and radius
        Vector3 attackCenter =
            transform.position +
            transform.forward * punchRange;

        // Use Physics.OverlapSphere to detect colliders within the punch radius
        Collider[] hits =
            Physics.OverlapSphere(
                attackCenter,
                punchRadius
            );

        // Use a HashSet to keep track of damaged targets to avoid applying damage multiple times to the same target
        HashSet<Health> damagedTargets =
            new HashSet<Health>();

        // Flag to track if at least one rat was hit during the punch attack (used for playing the punch hit sound)
        bool hitAtLeastOneRat = false;

        // Iterate through the detected colliders and apply damage to valid targets
        foreach (Collider hit in hits)
        {
            Health targetHealth =
                hit.GetComponentInParent<Health>();

            if (targetHealth == null)
                continue;

            if (targetHealth.gameObject == gameObject)
                continue;

            if (!targetHealth.gameObject.CompareTag("Rat"))
                continue;

            if (damagedTargets.Contains(targetHealth))
                continue;

            targetHealth.TakeDamage(punchDamage);
            damagedTargets.Add(targetHealth);
            hitAtLeastOneRat = true;
        }

        // Play the punch hit sound if at least one rat was hit and the audio source and sound clip are available
        if (hitAtLeastOneRat &&
            audioSource != null &&
            punchHitSound != null)
        {
            audioSource.PlayOneShot(punchHitSound);
        }

        float remainingTime =
            punchDuration - punchHitDelay;

        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);

        isPunching = false;
    }

    // Check if the cat is facing the attacker
    public bool IsFacing(Transform attacker)
    {
        // Calculate the direction from the cat to the attacker and check if the attacker is in front of the cat
        Vector3 directionToAttacker =
            attacker.position - transform.position;

        // Ignore the vertical component of the direction to focus on horizontal facing
        directionToAttacker.y = 0f;

        // If the direction to the attacker is very small, consider the cat as facing the attacker
        if (directionToAttacker.sqrMagnitude < 0.001f)
            return true;

        directionToAttacker.Normalize();

        float facingAmount =
            Vector3.Dot(transform.forward, directionToAttacker);

        // True when the attacker is in front of the cat.
        return facingAmount >= 0f;
    }
}