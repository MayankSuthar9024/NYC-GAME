using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public CharacterController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float ZMovement  = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float XMovement  = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        Vector3 movement = new Vector3(XMovement, 0, ZMovement);
        movement = movement.normalized * speed * Time.deltaTime;

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            movement.y = jumpForce * Time.deltaTime;
        }
        else
        {
            movement.y = -9.81f * Time.deltaTime;
        }
        controller.Move(movement);

    }
}
