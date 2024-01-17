using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapExitSkyScript : MapExitScript
{
    public override IEnumerator DoExit()
    {
        //Debug.Log("Exit " + exitID);
        yield return StartCoroutine(DoFlyExit());
        (Vector3 playerPos, float playerYaw) = GetRelativeOffset(WorldPlayer.Instance.transform.position, WorldPlayer.Instance.GetTrueFacingRotation());

        MainManager.MapID mid = MainManager.MapID.None;
        Enum.TryParse(nextMap, out mid);
        yield return MainManager.Instance.StartCoroutine(MainManager.Instance.ChangeMap(mid, nextExitID, playerPos, playerYaw));
    }

    public IEnumerator DoFlyExit()
    {
        if (MainManager.Instance.Camera.mode == WorldCamera.CameraMode.FollowPlayer)
        {
            MainManager.Instance.Camera.mode = WorldCamera.CameraMode.FollowPlayerNoVertical;
        }
        WorldPlayer wp = WorldPlayer.Instance;

        bool fadeoutDone = false;
        IEnumerator Fadeout()
        {
            yield return StartCoroutine(MainManager.Instance.FadeToBlack());
            fadeoutDone = true;
        }

        float fly = wp.rb.velocity.y;
        if (fly < 5)
        {
            fly = 5;
        }

        StartCoroutine(Fadeout());
        while (!fadeoutDone)
        {
            wp.rb.velocity = Vector3.up * fly;
            for (int i = 0; i < wp.followers.Count; i++)
            {
                wp.followers[i].rb.velocity = Vector3.up * fly;
            }
            yield return null;
        }
    }


    //Called by the entrance script
    public override IEnumerator DoEntrance(Vector3 offset, float yawOffset)
    {
        //Debug.Log("Start entering");
        yield return StartCoroutine(DoFallEntrance(offset, yawOffset));
        //Debug.Log("End entering");
    }

    public IEnumerator DoFallEntrance(Vector3 offset, float yawOffset)
    {
        if (MainManager.Instance.Camera.mode == WorldCamera.CameraMode.FollowPlayer)
        {
            MainManager.Instance.Camera.mode = WorldCamera.CameraMode.FollowPlayerNoVertical;
        }
        //bool fadeinDone = false;
        IEnumerator Fadein()
        {
            yield return StartCoroutine(MainManager.Instance.UnfadeToBlack());
            //fadeinDone = true;
        }

        float deltaDistance = 1.5f;

        WorldPlayer wp = WorldPlayer.Instance;

        (Vector3 newPos, float newYaw) = ApplyRelativeOffsetInverse(offset, yawOffset);
        newPos += exitPoint.transform.rotation * Vector3.left * deltaDistance;

        wp.transform.position = newPos;
        wp.SetTrueFacingRotation(newYaw);

        wp.FollowerWarpSetState();

        wp.lastGroundedHeight = exitPoint.transform.position.y - 2;
        MainManager.Instance.Camera.SnapToTargets();        

        StartCoroutine(Fadein());

        while (GetPlaneDistance(wp.transform.position) < 0.3f) //(!fadeinDone || !timer)
        {
            yield return null;
        }

        MainManager.Instance.mapScript.SetDefaultCamera();

        yield return null;
    }
}
