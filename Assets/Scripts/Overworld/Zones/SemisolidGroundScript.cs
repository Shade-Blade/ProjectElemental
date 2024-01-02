using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemisolidGroundScript : WorldZone
{
    public Vector3 conveyer;
    public Vector3 digConveyer;
    public float damping;
    public bool snapBelow;

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.3f);
        Vector3 facing = transform.up;
        Gizmos.DrawLine(transform.position, transform.position + facing);

        Vector3 up = transform.rotation * Vector3.right * 0.5f;
        Vector3 side = transform.rotation * Vector3.forward * 0.5f;
        Gizmos.DrawLine(transform.position, transform.position + up);

        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        Gizmos.DrawLine(transform.position + up + side, transform.position + up - side);
        Gizmos.DrawLine(transform.position + up - side, transform.position - up - side);
        Gizmos.DrawLine(transform.position - up - side, transform.position - up + side);
        Gizmos.DrawLine(transform.position - up + side, transform.position + up + side);
        Gizmos.DrawLine(transform.position + up + side, transform.position - up - side);
        Gizmos.DrawLine(transform.position + up - side, transform.position - up + side);
    }

    public override void ProcessTrigger(Collider other)
    {
        WorldEntity we = other.transform.GetComponent<WorldEntity>();
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (we != null)
        {
            we.DoSemisolidLanding(transform.position + Vector3.up * 0.25f, transform.up, null, snapBelow);
            we.conveyerVector = conveyer;
            we.movementDamping = damping;

            if (we is WorldPlayer wp)
            {
                if (wp.GetActionState() == WorldPlayer.ActionState.Dig)
                {
                    wp.conveyerVector = digConveyer;
                }
            }
        }
    }
}
