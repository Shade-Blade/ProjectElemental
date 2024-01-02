using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceDirectionZoneScript : WorldZone
{
    public float strength;
    public Vector3 movement;
    public float damping;

    public bool isActive;
    public float activeTime = 0;

    public bool forceYActive;
    public float forceY;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.1f);

        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, 0.5f, 0f));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -0.5f, 0f));

        Gizmos.DrawLine(transform.position, transform.position + new Vector3(-0.5f, 0f, -0.5f));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0f, 0.5f));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0f, -0.5f));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(-0.5f, 0f, 0.5f));
    }

    public override void ProcessTrigger(Collider other)
    {
        WorldEntity we = other.transform.GetComponent<WorldEntity>();
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (we != null)
        {
            we.conveyerVector = (movement + -Physics.gravity) * Time.fixedDeltaTime;
            we.movementDamping = damping;
            //we.SetMomentumVel(movement - movement.y * Vector3.up);

            if (we is WorldPlayer wp)
            {
                if (activeTime <= 0)
                {
                    movement = strength * wp.rb.velocity.normalized;
                    if (forceYActive)
                    {
                        movement.y = forceY;
                    }
                }
                isActive = true;
            }
        }
    }

    public void FixedUpdate()
    {
        if (isActive)
        {
            activeTime += Time.fixedDeltaTime;
        } else
        {
            movement = Vector3.zero;
            activeTime = 0;
        }
        isActive = false;
    }
}
