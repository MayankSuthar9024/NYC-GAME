using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Options")]
    public float speed = 5.0f;
    public float backwardSpeedMultiplier = 0.6f; // Reduced speed when walking/running backwards
    public float jumpForce = 5.0f;
    public float gravity = -9.81f;
    public CharacterController controller;

    [Header("Animation Options")]
    public Animator animator;
    public float animationDampTime = 0.1f;
    public float maxVelocityZ = 7.0f;          // Matches YBotController Blend Tree Forward Running threshold (7)
    public float maxBackwardVelocityZ = 5.0f;  // Matches YBotController Blend Tree Backward threshold (-5)
    public float maxVelocityX = 3.0f;          // Matches YBotController Blend Tree Strafe threshold (3)

    private float verticalVelocity = -5.0f;

    void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (controller != null)
        {
            // Ensure Character Controller capsule bottom sits exactly at transform position (Y=0)
            controller.center = new Vector3(0f, controller.height / 2.0f, 0f);
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Disable Root Motion so animations don't float or lift the Character Controller off the ground
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    void Update()
    {
        if (controller == null) return;

        float moveZ = Input.GetAxis("Vertical");
        float moveX = Input.GetAxis("Horizontal");

        // Reduce player speed when walking backwards (pressing S)
        float currentSpeed = (moveZ < -0.1f) ? (speed * backwardSpeedMultiplier) : speed;

        // Calculate movement relative to character's local rotation (First-Person direction)
        Vector3 moveDirection = (transform.right * moveX) + (transform.forward * moveZ);
        if (moveDirection.magnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        // Update Animator Blend Tree parameters if Animator exists
        if (animator != null)
        {
            // Scale Velocity Z differently for forward vs backward movement
            float targetVelocityZ = (moveZ >= 0) ? (moveZ * maxVelocityZ) : (moveZ * maxBackwardVelocityZ);
            float targetVelocityX = moveX * maxVelocityX;
            bool isMoving = moveDirection.magnitude > 0.1f;

            // Smoothly blend animation states for Idle, Forward Run, Backward Walk, Left/Right Strafe
            animator.SetFloat("Velocity Z", targetVelocityZ, animationDampTime, Time.deltaTime);
            animator.SetFloat("Velocity X", targetVelocityX, animationDampTime, Time.deltaTime);
            animator.SetBool("Running", isMoving && moveZ > 0.1f);
            animator.SetBool("Walking", isMoving);
            animator.SetBool("idle", !isMoving);
        }

        // Handle grounded state, gravity, and jump physics
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -5.0f; // Firm downward force ensuring character stays snapped to the ground
            }

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpForce * -2.0f * gravity);
                if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (animator != null)
        {
            animator.SetBool("IsGrounded", controller.isGrounded);
        }

        Vector3 finalVelocity = (moveDirection * currentSpeed) + (Vector3.up * verticalVelocity);
        controller.Move(finalVelocity * Time.deltaTime);
    }

    void LateUpdate()
    {
        // Lock child model transform to local origin (0,0,0) so animation Y offsets don't float feet above ground
        if (animator != null && animator.transform != transform)
        {
            animator.transform.localPosition = Vector3.zero;
        }
    }
}





