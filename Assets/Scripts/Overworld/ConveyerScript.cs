using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerScript : WorldObject
{
    public Vector3 movement;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 0.5f, 0.2f, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.DrawLine(transform.position, transform.position + movement.normalized);
        Gizmos.DrawSphere(transform.position + movement.normalized, 0.1f);
    }

    public override void ProcessCollision(Collision collision)
    {
        WorldEntity we = collision.collider.transform.GetComponent<WorldEntity>();
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (we != null)
        {
            we.conveyerVector = movement;
            we.SetMomentumVel(movement);
        }
    }
}
