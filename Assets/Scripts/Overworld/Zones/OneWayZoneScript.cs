using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayZoneScript : WorldZone
{
    //forces your velocity to not be against the specified direction
    //(note: implicitly applies some antigravity so that upwards ones work)

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.3f);
        Vector3 facing = transform.up;
        Gizmos.DrawLine(transform.position, transform.position + facing);

        Vector3 up = transform.rotation * Vector3.right * 0.5f;
        Vector3 side = transform.rotation * Vector3.forward * 0.5f;
        Gizmos.DrawLine(transform.position, transform.position + up);

        Gizmos.color = new Color(0.8f, 0.6f, 0.2f, 0.5f);
        Gizmos.DrawLine(transform.position + up + side, transform.position + up - side);
        Gizmos.DrawLine(transform.position + up - side, transform.position - up - side);
        Gizmos.DrawLine(transform.position - up - side, transform.position - up + side);
        Gizmos.DrawLine(transform.position - up + side, transform.position + up + side);
        Gizmos.DrawLine(transform.position + up + side, transform.position - up - side);
        Gizmos.DrawLine(transform.position + up - side, transform.position - up + side);
    }


    public float velocityOffset;    //minimum allowable in the blocked direction (negative = weaker one way, positive = forces you out in the specified direction)
    //Note: positive values will look weird if you can interact with the edge of it (slightly jittery, since one frame it applies then the next you are out and it doesn't)
    //  (*above problem can be avoided by blocking access to that side, or making it a lot lower near that edge, or making the offset 0 once you are outside and approaching from the blocked side)
    // If the one way zone is moving then you need to add the dot product of the zone's velocity and the one way vector
    //  (i.e. so if it moves towards the blocked zone then the offset becomes more positive)
    public override void ProcessTrigger(Collider other)
    {
        WorldEntity we = other.transform.GetComponent<WorldEntity>();
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (we != null)
        {
            we.oneWayVector = transform.up;
            we.oneWayMinAllowable = velocityOffset;
        }
    }
}
