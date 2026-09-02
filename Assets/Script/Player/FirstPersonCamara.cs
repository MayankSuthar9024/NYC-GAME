using UnityEngine;

public class FirstPersonCamara : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform player;
    public Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Look Settings")]
    public float sensitivity = 2.0f;
    public float minY = -80.0f;
    public float maxY = 80.0f;

    private float xRotation = 0.0f;

    void Start()
    {
        // Lock cursor to game window and hide it for FPP control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (player != null)
        {
            xRotation = transform.eulerAngles.x;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Vertical camera pitch tilt (look up/down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);

        // Horizontal player yaw rotation (turn body left/right)
        player.Rotate(Vector3.up * mouseX);

        // Position camera at player's head/eye level
        transform.position = player.position + player.TransformDirection(cameraOffset);

        // Orient camera with vertical pitch while matching player's horizontal heading
        transform.rotation = Quaternion.Euler(xRotation, player.eulerAngles.y, 0f);
    }
}

