using UnityEngine;
using UnityEngine.Rendering.Universal;

public class twoDAnimationStateController : MonoBehaviour
{
    Animator animator;
    float velocityX = 0.0f;
    float velocityZ = 0.0f;
    public float acceleration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool run = Input.GetKey("w");
        bool left = Input.GetKey("a");
        bool right = Input.GetKey("d");

        if (run)
        {
            velocityZ += Time.deltaTime * acceleration;
        }
        if (left)
        {
            velocityX -= Time.deltaTime * acceleration;
        }
        if (right)
        {
            velocityX += Time.deltaTime * acceleration;
        }
        
        animator.SetFloat("Velocity Z", velocityZ);
        animator.SetFloat("Velocity X", velocityX);
    }
}
