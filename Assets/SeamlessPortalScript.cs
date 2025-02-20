using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Note: Particle effects will make the seam obvious (Mostly the trail effects that aren't directly parented to the player)
//This is not really a fixable problem?

//How to mitigate:
//  Dig -> make the floor under the seam non diggable
//  Super Jump trail -> make the portal short and wide?
//  Dash Hop -> idk

public class SeamlessPortalScript : MonoBehaviour
{
    public Transform exitPoint;
    public SeamlessPortalScript connected;

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.2f, 1f, 0.5f);
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
        //This only targets the world player because other objects won't be warped seamlessly properly
        WorldPlayer wo = other.transform.GetComponent<WorldPlayer>();
        if (wo != null)
        {
            wo.transform.position = connected.ApplyRelativeOffset(GetRelativeOffset(wo.transform.position));
            wo.rb.velocity = (connected.exitPoint.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(exitPoint.rotation) * wo.rb.velocity);
            wo.SetTrueFacingRotation(180 + wo.GetTrueFacingRotation() - exitPoint.rotation.eulerAngles.y + connected.exitPoint.rotation.eulerAngles.y);

            wo.lastGroundedHeight = (wo.lastGroundedHeight + connected.exitPoint.transform.position.y - exitPoint.position.y);

            //warp the followers
            foreach (WorldFollower wf in wo.followers)
            {
                wf.transform.position = connected.ApplyRelativeOffset(GetRelativeOffset(wf.transform.position));
                wf.rb.velocity = (connected.exitPoint.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(exitPoint.rotation) * wf.rb.velocity);
                wf.SetTrueFacingRotation(180 + wf.GetTrueFacingRotation() - exitPoint.rotation.eulerAngles.y + connected.exitPoint.rotation.eulerAngles.y);
            }

            //warp the camera
            GameObject camera = MainManager.Instance.Camera.gameObject;
            camera.transform.position = connected.ApplyRelativeOffset(GetRelativeOffset(camera.transform.position));
            camera.transform.rotation = connected.exitPoint.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(exitPoint.rotation) * camera.transform.rotation; //is this right?
            MainManager.Instance.Camera.DefaultFocusCalculation();  //recalculate this because position shifted
            MainManager.Instance.Camera.focusIgnoreHeightFrames = 2;

            //warp the worldspace yaw
            MainManager.Instance.SetWorldspaceYaw(180 + MainManager.Instance.GetWorldspaceYaw() - exitPoint.rotation.eulerAngles.y + connected.exitPoint.rotation.eulerAngles.y);
        }
    }

    public Vector3 GetRelativeOffset(Vector3 position)
    {
        //get facing vector
        Vector3 facing = exitPoint.rotation * Vector3.right; //note: inverted
        Vector3 delta = position - exitPoint.position;

        //Don't project onto exit point plane because world follower may have some other relative offset
        Vector3 projected = delta;// - Vector3.Project(delta, facing);
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