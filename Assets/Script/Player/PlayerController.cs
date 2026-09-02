using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Options")]
    public float speed = 5.0f;
    public float jumpForce = 5.0f;
    public float gravity = -9.81f;
    public CharacterController controller;

    private float verticalVelocity = 0.0f;

    void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        if (controller == null) return;

        float moveZ = Input.GetAxis("Vertical");
        float moveX = Input.GetAxis("Horizontal");

        // Calculate movement relative to character's local rotation (First-Person direction)
        Vector3 moveDirection = (transform.right * moveX) + (transform.forward * moveZ);
        if (moveDirection.magnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        // Handle grounded state, gravity, and jump physics
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2.0f; // Small constant downward force to stay grounded
            }

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpForce * -2.0f * gravity);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalVelocity = (moveDirection * speed) + (Vector3.up * verticalVelocity);
        controller.Move(finalVelocity * Time.deltaTime);
    }
}

