using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapExit : WorldObject
{
    public int exitID;
    //public MainManager.MapID nextMap;
    public string nextMap;
    public int nextExitID;
    public abstract (Vector3, Quaternion) GetEntrancePoint();

    //These two methods are used to orient the player correctly when going through map transitions
    //(so if they enter at an offset then they exit at the same offset, but oriented correctly)
    //note: player character only has yaw value
    public abstract (Vector3, float) GetRelativeOffset(Vector3 position, float yaw);
    public abstract (Vector3, float) ApplyRelativeOffset(Vector3 posOffset, float yawOffset);
    public abstract (Vector3, float) GetRelativeOffsetInverse(Vector3 position, float yaw);
    public abstract (Vector3, float) ApplyRelativeOffsetInverse(Vector3 posOffset, float yawOffset);

    public abstract IEnumerator DoExit();
    public abstract IEnumerator DoEntrance(Vector3 offset, float yawOffset);
}

public class MapExitScript : MapExit
{
    public Transform exitPoint;

    //public MapExitScript connected;

    bool active = false;
    bool coroutineActive = false;

    public virtual void OnDrawGizmos()
    {
        if (exitPoint == null)
        {
            return;
        }
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
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

    public override (Vector3, Quaternion) GetEntrancePoint()
    {
        return (exitPoint.position, exitPoint.rotation);
    }

    public override (Vector3, float) GetRelativeOffset(Vector3 position, float yaw)
    {
        //get facing vector
        Vector3 facing = exitPoint.rotation * Vector3.right;
        Vector3 delta = position - exitPoint.position;

        Vector3 projected = delta - Vector3.Project(delta, facing);
        projected = Quaternion.Inverse(exitPoint.rotation) * projected; //return to world space

        //yaw rotations are additive
        float exitYaw = exitPoint.rotation.eulerAngles.y;

        float outputYaw = yaw - exitYaw;
        if (outputYaw > 360)
        {
            outputYaw -= 360;
        }
        if (outputYaw < 0)
        {
            outputYaw += 360;
        }

        return (projected, outputYaw);
    }

    public override (Vector3, float) ApplyRelativeOffset(Vector3 posOffset, float yawOffset)
    {
        Vector3 facing = exitPoint.rotation * Vector3.right;
        Vector3 rotated = exitPoint.rotation * posOffset;
        Vector3 outputPos = rotated + exitPoint.position;

        float exitYaw = exitPoint.rotation.eulerAngles.y;

        float outputYaw = yawOffset + exitYaw;
        if (outputYaw > 360)
        {
            outputYaw -= 360;
        }
        if (outputYaw < 0)
        {
            outputYaw += 360;
        }

        return (outputPos, outputYaw);
    }
    public override (Vector3, float) GetRelativeOffsetInverse(Vector3 position, float yaw)
    {
        //get facing vector
        Vector3 facing = exitPoint.rotation * Vector3.right;
        Vector3 delta = position - exitPoint.position;

        Vector3 projected = delta - Vector3.Project(delta, facing);
        projected = Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(exitPoint.rotation) * projected; //return to world space

        //yaw rotations are additive
        float exitYaw = exitPoint.rotation.eulerAngles.y;

        float outputYaw = yaw - exitYaw;
        if (outputYaw > 360)
        {
            outputYaw -= 360;
        }
        if (outputYaw < 0)
        {
            outputYaw += 360;
        }

        return (projected, outputYaw);
    }

    public override (Vector3, float) ApplyRelativeOffsetInverse(Vector3 posOffset, float yawOffset)
    {
        Vector3 facing = exitPoint.rotation * Vector3.right;
        Vector3 rotated = exitPoint.rotation * Quaternion.Euler(0, 180, 0) * posOffset;
        Vector3 outputPos = rotated + exitPoint.position;

        float exitYaw = exitPoint.rotation.eulerAngles.y;

        float outputYaw = yawOffset + exitYaw;
        if (outputYaw > 360)
        {
            outputYaw -= 360;
        }
        if (outputYaw < 0)
        {
            outputYaw += 360;
        }

        return (outputPos, outputYaw);
    }
    public float GetPlaneDistance(Vector3 position)
    {
        Vector3 delta = position - exitPoint.position;
        return Vector3.Dot(delta, exitPoint.rotation * Vector3.right);
    }


    public override void WorldUpdate()
    {
        //For testing
        /*
        (Vector3 wpPos, float wpYaw) = GetRelativeOffset(WorldPlayer.Instance.transform.position, WorldPlayer.Instance.GetTrueFacingRotation());
        (Vector3 wpPosB, float wpYawB) = ApplyRelativeOffset(wpPos, wpYaw);

        Vector3 yawFacing = Quaternion.Euler(0, wpYawB, 0) * Vector3.right;

        Debug.DrawRay(wpPosB, yawFacing, Color.green);

        if (connected != null)
        {
            (Vector3 wpPosC, float wpYawC) = connected.ApplyRelativeOffset(wpPos, wpYaw);
            Vector3 yawFacingC = Quaternion.Euler(0, wpYawC, 0) * Vector3.right;

            Debug.DrawRay(wpPosC, yawFacingC, Color.yellow);
        }
        */

        if (active == true)
        {
            if (!coroutineActive)
            {
                StartCoroutine(MainManager.Instance.ExecuteCutscene(DoExit()));
                coroutineActive = true;
            }
        }
    }

    public override IEnumerator DoExit()
    {
        mapScript.OnExit(exitID);
        //Debug.Log("Exit " + exitID);
        yield return StartCoroutine(DoWalkExit());
        (Vector3 playerPos, float playerYaw) = GetRelativeOffset(WorldPlayer.Instance.transform.position, WorldPlayer.Instance.GetTrueFacingRotation());

        MainManager.MapID mid = MainManager.MapID.None;
        Enum.TryParse(nextMap, out mid);
        yield return MainManager.Instance.StartCoroutine(MainManager.Instance.ChangeMap(mid, nextExitID, playerPos, playerYaw));
    }

    public IEnumerator DoWalkExit()
    {
        bool fadeoutDone = false;
        IEnumerator Fadeout()
        {
            yield return StartCoroutine(MainManager.Instance.FadeToBlack());
            fadeoutDone = true;
        }

        WorldPlayer wp = WorldPlayer.Instance;
        Vector3 facing = exitPoint.rotation * Vector3.right;
        Vector2 scriptedInput = -MainManager.XZProject(facing);
        StartCoroutine(Fadeout());
        while (!fadeoutDone)
        {
            wp.scriptedInput = scriptedInput;
            yield return null;
        }

        MainManager.Instance.mapScript.SetDefaultCamera();

        yield return null;
    }

    //Called by the entrance script
    public override IEnumerator DoEntrance(Vector3 offset, float yawOffset)
    {
        //Debug.Log("Start entering");
        yield return StartCoroutine(DoWalkEntrance(offset, yawOffset));
        //Debug.Log("End entering");
    }

    public IEnumerator DoWalkEntrance(Vector3 offset, float yawOffset)
    {
        //bool fadeinDone = false;
        IEnumerator Fadein()
        {
            yield return StartCoroutine(MainManager.Instance.UnfadeToBlack());
            //fadeinDone = true;
        }

        //bool timer = false;
        IEnumerator RunTimer()
        {
            float time = 0;
            float finalTimer = 0.75f;
            while (time < finalTimer)
            {
                time += Time.deltaTime;
                yield return null;
            }

            //timer = true;
        }

        float deltaDistance = 1.5f;

        WorldPlayer wp = WorldPlayer.Instance;

        (Vector3 newPos, float newYaw) = ApplyRelativeOffsetInverse(offset, yawOffset);
        newPos += exitPoint.transform.rotation * Vector3.left * deltaDistance;

        wp.transform.position = newPos;
        wp.SetTrueFacingRotation(newYaw);

        wp.FollowerWarpSetState();

        MainManager.Instance.Camera.SnapToTargets();

        Vector3 facing = exitPoint.rotation * Vector3.right;
        Vector2 scriptedInput = MainManager.XZProject(facing);
        StartCoroutine(Fadein());
        StartCoroutine(RunTimer());
        while (GetPlaneDistance(wp.transform.position) < 0.3f) //(!fadeinDone || !timer)
        {
            wp.scriptedInput = scriptedInput;
            yield return null;
        }

        yield return null;
    }

    public void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(other);
    }

    public void OnTriggerStay(Collider other)
    {
        ProcessTrigger(other);
    }
    

    public void ProcessTrigger(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();
        if (wp != null && !mapScript.GetHalted() && mapScript.ExitsActive() && !wp.HazardState() && MainManager.Instance.GetControlsEnabled())
        {
            active = true;
        }
    }
}
