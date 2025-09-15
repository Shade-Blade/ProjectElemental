using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TattleZoneScript : WorldZone, ITattleable
{
    public string tattle;

    public virtual string GetTattle()
    {
        return tattle;
    }

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.1f);

        Gizmos.DrawLine(transform.position + new Vector3(-0.2f, 0.7f, 0f), transform.position + new Vector3(0, 0f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0f, 0f), transform.position + new Vector3(0.2f, 0.7f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.2f, 0.7f, 0f), transform.position + new Vector3(-0.2f, 0.7f, 0f));
    }

    public override void ProcessTrigger(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();

        if (wp != null)
        {
            wp.zoneTattleTarget = this;
        }
    }
}
