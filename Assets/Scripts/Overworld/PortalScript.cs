using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : WorldObject
{
    public Transform exitPoint;
    public PortalScript connected;

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.2f, 1f, 0.5f);
        Gizmos.DrawSphere(exitPoint.position, 0.3f);
        Vector3 facing = exitPoint.rotation * Vector3.right;
        Gizmos.DrawLine(exitPoint.position, exitPoint.position + facing);

        Vector3 up = exitPoint.rotation * Vector3.up * 0.75f;
        Vector3 side = exitPoint.rotation * Vector3.forward * 0.75f;
        Gizmos.DrawLine(exitPoint.position, exitPoint.position + up);

        Gizmos.color = new Color(0f, 0.75f, 0.75f, 0.5f);
        Gizmos.DrawLine(exitPoint.position + up + side, exitPoint.position + up - side);
        Gizmos.DrawLine(exitPoint.position + up - side, exitPoint.position - up - side);
        Gizmos.DrawLine(exitPoint.position - up - side, exitPoint.position - up + side);
        Gizmos.DrawLine(exitPoint.position - up + side, exitPoint.position + up + side);
        Gizmos.DrawLine(exitPoint.position + up + side, exitPoint.position - up - side);
        Gizmos.DrawLine(exitPoint.position + up - side, exitPoint.position - up + side);
    }

    public void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(other);
    }

    /*
    public void OnTriggerStay(Collider other)
    {
        ProcessTrigger(other);
    }
    */

    public void ProcessTrigger(Collider other)
    {
        WorldObject wo = other.transform.GetComponent<WorldObject>();
        if (wo != null)
        {
            wo.transform.position = connected.ApplyRelativeOffset(GetRelativeOffset(wo.transform.position));
            wo.rb.velocity = (connected.exitPoint.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(exitPoint.rotation) * wo.rb.velocity);
            //Debug.Log(wo.rb.velocity);
            
            /*
            Vector3 facing = exitPoint.rotation * Vector3.right;
            if (Vector3.Dot(facing, wo.rb.velocity) < 0)
            {
                wo.rb.velocity *= -1;
            }
            */

            if (wo is WorldEntity we)
            {
                we.SetTrueFacingRotation(180 + we.GetTrueFacingRotation() - exitPoint.rotation.eulerAngles.y + connected.exitPoint.rotation.eulerAngles.y);
            }
        }
    }

    public Vector3 GetRelativeOffset(Vector3 position)
    {
        //get facing vector
        Vector3 facing = exitPoint.rotation * Vector3.right; //note: inverted
        Vector3 delta = position - exitPoint.position;

        Vector3 projected = delta - Vector3.Project(delta, facing);
        projected = Quaternion.Inverse(exitPoint.rotation) * projected; //return to world space

        return projected;
    }

    public Vector3 ApplyRelativeOffset(Vector3 posOffset)
    {
        Vector3 facing = exitPoint.rotation * Vector3.right;
        Vector3 rotated = exitPoint.rotation * Quaternion.Euler(0, 180, 0) * posOffset;
        Vector3 outputPos = rotated + exitPoint.position;
        return outputPos;
    }
}
