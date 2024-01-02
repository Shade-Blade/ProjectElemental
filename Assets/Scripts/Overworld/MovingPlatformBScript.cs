using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformBScript : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 movement;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x > 25 && movement.x > 0)
        {
            movement.x *= -1;
        }
        if (transform.position.x < -25 && movement.x < 0)
        {
            movement.x *= -1;
        }

        rb.velocity = movement;
    }

    void FixedUpdate()
    {
        rb.velocity = movement;
    }
}
