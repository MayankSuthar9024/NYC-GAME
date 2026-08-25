using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    [Range(0.1f, 1f)]
    public float strafeSpeedFactor = 0.7f;
    public float rotationSpeed = 8f;
    public float acceleration = 10f;

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -19.62f;
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask = ~0;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 horizontalVelocity;
    public bool isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            groundCheck = gc.transform;
        }
    }

    void Update()
    {
        Collider[] groundHits = Physics.OverlapSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
        isGrounded = controller.isGrounded;
        for (int i = 0; i < groundHits.Length && !isGrounded; i++)
        {
            if (!groundHits[i].transform.IsChildOf(transform))
                isGrounded = true;
        }

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (isGrounded)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(x, 0f, z);

            float yaw = transform.eulerAngles.y;
            Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

            Vector3 wishDir = forward * input.z + right * (input.x * strafeSpeedFactor);
            wishDir = Vector3.ClampMagnitude(wishDir, 1f);
            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

            Vector3 targetVelocity = wishDir * speed;
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalVelocity + new Vector3(0f, velocity.y, 0f);
        controller.Move(finalMove * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
