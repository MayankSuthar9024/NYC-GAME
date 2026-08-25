using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Sensitivity")]
    public float mouseSensitivity = 2f;
    public float verticalClamp = 89f;
    public bool invertY = false;

    [Header("Camera Setup")]
    public float eyeHeight = 1.5f;
    public float cameraDistance = 3.5f;

    private float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        AttachCamera();
    }

    void Update()
    {
        if (cameraTransform == null || !cameraTransform.IsChildOf(transform))
            AttachCamera();

        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

        transform.Rotate(0f, mouseX, 0f);

        cameraTransform.SetParent(transform, true);
        cameraTransform.localPosition = new Vector3(0f, eyeHeight, -cameraDistance);

        pitch = Mathf.Clamp(pitch + mouseY, -verticalClamp, verticalClamp);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void AttachCamera()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null)
        {
            Camera[] allCams = FindObjectsOfType<Camera>(true);
            foreach (Camera c in allCams)
            {
                if (c.CompareTag("MainCamera")) { cam = c; break; }
            }
            if (cam == null && allCams.Length > 0) cam = allCams[0];
        }
        if (cam != null) cameraTransform = cam.transform;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
