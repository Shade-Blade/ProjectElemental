using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapExitHoleScript : MapExitScript
{
    public float jumpVelocity;
    public float lateralMovement;

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Vector3 launch = Vector3.up * jumpVelocity + exitPoint.rotation * Vector3.up * lateralMovement;

        Gizmos.color = new Color(0.75f, 0.75f, 0.75f, 0.5f);
        Gizmos.DrawLine(exitPoint.position, exitPoint.position + launch);
    }

    public override IEnumerator DoExit()
    {
        mapScript.OnExit(exitID);
        //Debug.Log("Exit " + exitID);
        yield return StartCoroutine(DoFallExit());
        (Vector3 playerPos, float playerYaw) = GetRelativeOffset(WorldPlayer.Instance.transform.position, WorldPlayer.Instance.GetTrueFacingRotation());

        MainManager.MapID mid = MainManager.MapID.None;
        Enum.TryParse(nextMap, out mid);
        yield return MainManager.Instance.StartCoroutine(MainManager.Instance.ChangeMap(mid, nextExitID, playerPos, playerYaw));
    }

    public IEnumerator DoFallExit()
    {
        if (MainManager.Instance.Camera.mode == WorldCamera.CameraMode.FollowPlayer)
        {
            MainManager.Instance.Camera.mode = WorldCamera.CameraMode.FollowPlayerNoVertical;
        }
        yield return StartCoroutine(MainManager.Instance.FadeToBlack());
        //gravity pulls you down
    }

    //Called by the entrance script
    public override IEnumerator DoEntrance(Vector3 offset, float yawOffset)
    {
        //Debug.Log("Start entering");
        yield return StartCoroutine(DoJumpEntrance(offset, yawOffset));
        //Debug.Log("End entering");
    }

    public IEnumerator DoJumpEntrance(Vector3 offset, float yawOffset)
    {
        bool fadeinDone = false;
        IEnumerator Fadein()
        {
            yield return StartCoroutine(MainManager.Instance.UnfadeToBlack());
            fadeinDone = true;
        }

        float deltaDistance = 1.5f;

        WorldPlayer wp = WorldPlayer.Instance;

        (Vector3 newPos, float newYaw) = ApplyRelativeOffsetInverse(offset, yawOffset);
        newPos += exitPoint.transform.rotation * Vector3.left * deltaDistance;

        wp.transform.position = newPos;
        wp.SetTrueFacingRotation(newYaw);

        wp.FollowerWarpSetState();

        wp.lastGroundedHeight = exitPoint.transform.position.y;
        if (MainManager.Instance.Camera.mode == WorldCamera.CameraMode.FollowPlayer)
        {
            MainManager.Instance.Camera.mode = WorldCamera.CameraMode.FollowPlayerNoVertical;
        }
        MainManager.Instance.Camera.SnapToTargets();


        StartCoroutine(Fadein());

        while (!fadeinDone)
        {
            wp.rb.velocity = Vector3.zero;
            for (int i = 0; i < wp.followers.Count; i++)
            {
                wp.followers[i].rb.velocity = Vector3.zero;
            }
            yield return null;
        }

        wp.transform.position = newPos;
        wp.Launch(Vector3.up * jumpVelocity + exitPoint.rotation * Vector3.up * lateralMovement);        

        while (!wp.IsGrounded()) //(!fadeinDone || !timer)
        {
            wp.scriptedInput = MainManager.XZProject(exitPoint.rotation * Vector3.up * lateralMovement);
            yield return null;
        }

        MainManager.Instance.mapScript.SetDefaultCamera();

        yield return null;
    }
}
