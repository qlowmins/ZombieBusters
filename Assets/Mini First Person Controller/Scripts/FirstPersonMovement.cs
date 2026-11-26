using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;
    public float MovementMultiplier { get; set; } = 1f;

    Rigidbody rigidbody;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();



    void Awake()
    {
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
        //Debug.Log("movement load");
    }

/*void Update()
{
    if (Input.GetKeyDown(KeyCode.T))
    {
        Debug.Log("TimeScale before: " + Time.timeScale);

        Time.timeScale = 1f;

        Debug.Log("TimeScale restored to: " + Time.timeScale);
    }
}*/

bool hasLogged = false;

void Update()
{
    if (!hasLogged)
    {
        Debug.Log("Game TimeScale: " + Time.timeScale);
        Debug.Log("Movement Update is running!");
        hasLogged = true;
    }

    if (Input.GetKeyDown(KeyCode.W))
    {
        Debug.Log("W KEY DETECTED!");
        Debug.Log("Vertical Input: " + Input.GetAxis("Vertical"));
    }
}
    void FixedUpdate()
    {
        // Update IsRunning from input.
        IsRunning = canRun && Input.GetKey(runningKey);

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }
        targetMovingSpeed *= MovementMultiplier;

        // Get targetVelocity from input.
        Vector2 targetVelocity =new Vector2( Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

        // Apply movement.
        rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, rigidbody.velocity.y, targetVelocity.y);
    }
}
