using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneScript : MonoBehaviour
{
    public WorldCameraSettings wcs;

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, -0.5f, 0f), transform.position + new Vector3(-0.5f, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.5f, 0f), transform.position + new Vector3(0.5f, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.5f, 0f), transform.position + new Vector3(0.5f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.5f, 0f), transform.position + new Vector3(-0.5f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.25f, 0f), transform.position + new Vector3(0.75f, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.25f, 0f), transform.position + new Vector3(0.75f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.75f, 0.5f, 0f), transform.position + new Vector3(0.75f, -0.5f, 0f));

        switch (wcs.mode)
        {
            case WorldCamera.CameraMode.RestrictAlongLine:
            case WorldCamera.CameraMode.RestrictBetweenPoints:
            case WorldCamera.CameraMode.RestrictAlongLineInterpolate:
            case WorldCamera.CameraMode.RestrictBetweenPointsInterpolate:
            case WorldCamera.CameraMode.FollowPlayerInterpolate:
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
                Gizmos.DrawSphere(wcs.pointA, 0.2f);
                Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
                Gizmos.DrawSphere(wcs.pointB, 0.2f);
                Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);
                Gizmos.DrawLine(wcs.pointA, wcs.pointB);
                break;
            case WorldCamera.CameraMode.RestrictBetweenPlanes:
            case WorldCamera.CameraMode.RestrictBetweenPlanesInterpolate:
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
                Gizmos.DrawSphere(wcs.pointA, 0.2f);
                Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
                Gizmos.DrawSphere(wcs.pointB, 0.2f);

                Vector3 sideGuess = Vector3.Cross(wcs.pointA - wcs.pointB, Vector3.up);

                if (sideGuess.magnitude == 0)
                {
                    sideGuess = Vector3.Cross(wcs.pointA - wcs.pointB, Vector3.up + Vector3.forward * 0.05f);
                }
                //guaranteed to have at least one of the guesses be a nonzero vector in the desired plane

                Vector3 upGuess = Vector3.Cross(wcs.pointA - wcs.pointB, sideGuess);

                Vector3 up = upGuess.normalized;
                Vector3 side = sideGuess.normalized;

                Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
                Gizmos.DrawLine(wcs.pointA + up + side, wcs.pointA + up - side);
                Gizmos.DrawLine(wcs.pointA + up - side, wcs.pointA - up - side);
                Gizmos.DrawLine(wcs.pointA - up - side, wcs.pointA - up + side);
                Gizmos.DrawLine(wcs.pointA - up + side, wcs.pointA + up + side);
                Gizmos.DrawLine(wcs.pointA + up + side, wcs.pointA - up - side);
                Gizmos.DrawLine(wcs.pointA + up - side, wcs.pointA - up + side);

                Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
                Gizmos.DrawLine(wcs.pointB + up + side, wcs.pointB + up - side);
                Gizmos.DrawLine(wcs.pointB + up - side, wcs.pointB - up - side);
                Gizmos.DrawLine(wcs.pointB - up - side, wcs.pointB - up + side);
                Gizmos.DrawLine(wcs.pointB - up + side, wcs.pointB + up + side);
                Gizmos.DrawLine(wcs.pointB + up + side, wcs.pointB - up - side);
                Gizmos.DrawLine(wcs.pointB + up - side, wcs.pointB - up + side);
                break;
            case WorldCamera.CameraMode.RestrictOnPlane:
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
                Gizmos.DrawSphere(wcs.pointA, 0.2f);
                Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
                Gizmos.DrawSphere(wcs.pointB, 0.2f);

                Vector3 sideGuessB = Vector3.Cross(wcs.pointA - wcs.pointB, Vector3.up);

                if (sideGuessB.magnitude == 0)
                {
                    sideGuessB = Vector3.Cross(wcs.pointA - wcs.pointB, Vector3.up + Vector3.forward * 0.05f);
                }
                //guaranteed to have at least one of the guesses be a nonzero vector in the desired plane

                Vector3 upGuessB = Vector3.Cross(wcs.pointA - wcs.pointB, sideGuessB);

                Vector3 upB = upGuessB.normalized;
                Vector3 sideB = sideGuessB.normalized;

                Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
                Gizmos.DrawLine(wcs.pointA + upB + sideB, wcs.pointA + upB - sideB);
                Gizmos.DrawLine(wcs.pointA + upB - sideB, wcs.pointA - upB - sideB);
                Gizmos.DrawLine(wcs.pointA - upB - sideB, wcs.pointA - upB + sideB);
                Gizmos.DrawLine(wcs.pointA - upB + sideB, wcs.pointA + upB + sideB);
                Gizmos.DrawLine(wcs.pointA + upB + sideB, wcs.pointA - upB - sideB);
                Gizmos.DrawLine(wcs.pointA + upB - sideB, wcs.pointA - upB + sideB);
                break;
            case WorldCamera.CameraMode.RadialFollow:
            case WorldCamera.CameraMode.RadialFollowReverse:
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
                Gizmos.DrawSphere(wcs.pointA, 0.3f);
                break;
            case WorldCamera.CameraMode.RadialFollowFixedDistance:
            case WorldCamera.CameraMode.RadialFollowReverseFixedDistance:
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
                Gizmos.DrawSphere(wcs.pointA, 0.3f);
                UnityEditor.Handles.color = new Color(0.5f, 0.5f, 1f, 0.5f);
                UnityEditor.Handles.DrawWireDisc(wcs.pointA, Vector3.up, (wcs.pointA - wcs.pointB).magnitude);
                break;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        ProcessCollider(other);
    }

    public void OnTriggerStay(Collider other)
    {
        ProcessCollider(other);
    }

    public void OnTriggerExit(Collider other)
    {
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (other.transform.GetComponent<WorldPlayer>() != null)
        {
            if (wcs.priority <= 0)
            {
                MainManager.Instance.mapScript.SetDefaultCamera();
                //MainManager.Instance.Camera.SetCameraSettings(WorldCameraSettings.GetDefault());
            }
        }
    }

    public void ProcessCollider(Collider other)
    {
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (other.transform.GetComponent<WorldPlayer>() != null)
        {
            MainManager.Instance.Camera.SetCameraSettings(wcs);
        }
    }
}
