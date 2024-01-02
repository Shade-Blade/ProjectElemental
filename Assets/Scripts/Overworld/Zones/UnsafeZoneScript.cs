using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Use this to blanket mark an area as unstable
//Useful in cases where making separate colliders would be hard (for example, normally safe floor getting bombarded by stuff from above)
public class UnsafeZoneScript : WorldZone
{
    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0.3f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, -0.5f, 0f), transform.position + new Vector3(0, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0.5f, 0f), transform.position + new Vector3(0.5f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.5f, 0f), transform.position + new Vector3(-0.5f, -0.5f, 0f));
    }

    public override void ProcessTrigger(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();
        if (wp != null)
        {
            wp.unstableZone = true;
        }
    }
}
