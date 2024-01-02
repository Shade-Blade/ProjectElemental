using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZoneScript: WorldZone
{
    public Vector3 movement;
    public float damping;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 0.5f, 0.2f, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.DrawLine(transform.position, transform.position + movement.normalized);
        Gizmos.DrawSphere(transform.position + movement.normalized, 0.1f);

        if (damping != 0)
        {
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(-0.5f, 0f, -0.5f));
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0f, 0.5f));
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0f, -0.5f));
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(-0.5f, 0f, 0.5f));
        }
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
        }
    }
}
