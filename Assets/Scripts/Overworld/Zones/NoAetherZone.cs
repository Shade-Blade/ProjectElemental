using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoAetherZone : WorldZone
{
    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.7f, 0.3f, 0.3f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, -0.5f, 0f), transform.position + new Vector3(0.5f, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.5f, 0f), transform.position + new Vector3(0.5f, -0.5f, 0f));
    }

    public override void ProcessTrigger(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();
        if (wp != null)
        {
            wp.noAetherZone = true;
        }
    }
}
