using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class WorldCameraSettings    //contains all the info something that messes with the world camera will need (Note: battle camera is special and needs special handling, but the script may end up shared)
{
    public WorldCamera.CameraMode mode;
    public WorldCamera.CameraEffect effect;
    public float distance;
    public Vector3 directionVector;
    public Vector3 cameraEulerAngles;
    public float worldspaceYaw;
    public float movementHalflife;

    public float distanceB;
    public Vector3 directionVectorB;
    public Vector3 cameraEulerAnglesB;   //note: separate from worldspace yaw
    public float worldspaceYawB;
    public Vector3 pointA;
    public Vector3 pointB;
    public int priority;    //lower priority zones get ignored

    public static WorldCameraSettings GetDefault()
    {
        WorldCameraSettings wcs = new WorldCameraSettings();

        wcs.mode = WorldCamera.CameraMode.FollowPlayer;
        wcs.effect = WorldCamera.CameraEffect.None;
        wcs.cameraEulerAngles = Vector3.zero;
        wcs.directionVector = new Vector3(0, 1.2f, -3f);
        wcs.distance = 4.75f;
        wcs.worldspaceYaw = 0;
        wcs.movementHalflife = 0.05f;

        return wcs;
    }
}

public class WorldCameraSnapshot
{
    public WorldCameraSettings settings;
    public Vector3 position;
    public Quaternion rotation;
}

public class WorldCamera : MonoBehaviour
{
    public enum CameraMode
    {
        ManualSettings,          //settings don't change automatically
        FollowPlayer,            //ray between camera and point that has the XZ coords of player but has Y of the last grounded pos stays constant
        FollowPlayerNoVertical, //FollowPlayer but no vertical change
        HardFollowPlayer,        //ray between camera and player stays constant
        RestrictAlongLine,       //camera restricted to be along a line of 2 points positionally
        RestrictBetweenPoints,   //camera restricted to be along a line of 2 points positionally (This version clamps)
        RestrictBetweenPlanes,  //clamps the player position between the 2 planes formed by finding the perpendicular planes to the line between A and B (that contain A and B)
        RestrictOnPlane,        //point is stuck onto a plane defined by point A and point B (point A is on plane and line between A and B defines normal to plane)
        FollowPlayerInterpolate,            //interpolates between the a and b parameters (using player position between pointA and B)
        RestrictAlongLineInterpolate,       //interpolates between the a and the b parameters
        RestrictBetweenPointsInterpolate,   //(useful for transitioning between other camera zones)
        RestrictBetweenPlanesInterpolate,
        RadialFollow,            //camera rotates as you go around a point (based on camera XZ pos)
        RadialFollowReverse,     //points outward from center (so 180 degrees from radial follow)
        RadialFollowFixedDistance,            //fixed distance is the xz distance from point a to point b (focus distance)
        RadialFollowReverseFixedDistance,     //fixed distance is the xz distance from point a to point b (focus distance)
        FirstPerson_DoNotUse,           //For fun (makes the camera on top of you) (Mouse controls camera angling, though this can be independent of your real facing angle)
    }

    public CameraMode mode;

    //One for each unique material used
    //(Mode is used to determine what variables need to be set for each camera effect material)
    public enum CameraEffect
    {
        None,
        VignetteWeakSpark,
        VignetteWeakFire,
        VignetteWeakWater,
        VignetteWeakDark,
        VignetteWeakIce,
        VignetteAetherRays,
        VignetteAcid,
        VignetteSpark,
        VignetteLeafy,
        VignetteFire,
        VignetteWater,
        VignetteDark,
        VignetteIce,
        VignettePrismatic
    }

    //to do: some way to allow for the settings of the material to be changed
    //to do: way to toggle orthographic

    //future thing to do: assign materials to each enum value somehow
    //but also trying to check the material every frame seems like a bad idea
    public CameraEffect effect;

    //public float targetDistance;
    public float targetYaw;
    public Vector3 targetEulerAngles;

    public float distance;
    public Vector3 focus;
    public Vector3 directionVector;
    public Vector3 cameraEulerAngles;   //note: separate from worldspace yaw
    public float worldspaceYaw;
    public float movementHalflife;

    public float distanceB;
    public Vector3 directionVectorB;
    public Vector3 cameraEulerAnglesB;   //note: separate from worldspace yaw
    public float worldspaceYawB;
    public Vector3 pointA;
    public Vector3 pointB;

    public Vector3 offset;

    public float freeCamHoldTime = 0;
    public bool pastFreeCamMode = false;

    //note: camera zones further down the inspector activate later so a priority system is not really necessary except for the default camera
    //public int priority;

    public new Camera camera;

    public Material material;

    public const float MAX_UPOFFSET = 2.5f;
    public const float MAX_DOWNOFFSET = 1.5f;

    void Awake()
    {
        camera = GetComponent<Camera>();
        mode = CameraMode.FollowPlayer;
        //camera.depthTextureMode = DepthTextureMode.DepthNormals;
    }

    public void SetCameraSettings(WorldCameraSettings settings)
    {
        //Debug.Log("set");

        //Debug.Log("settings has effect " + settings.effect + " vs " + effect);

        bool esetup = false;
        if (effect != settings.effect)
        {
            esetup = true;
        }

        mode = settings.mode;
        effect = settings.effect;
        distance = settings.distance;
        directionVector = settings.directionVector;
        targetEulerAngles = settings.cameraEulerAngles;

        if (!MainManager.Instance.Cheat_FirstPersonCamera || mode != CameraMode.FirstPerson_DoNotUse)
        {
            targetYaw = settings.worldspaceYaw;
        }

        movementHalflife = settings.movementHalflife;

        distanceB = settings.distanceB;
        directionVectorB = settings.directionVectorB;
        cameraEulerAnglesB = settings.cameraEulerAnglesB;
        worldspaceYawB = settings.worldspaceYawB;
        pointA = settings.pointA;
        pointB = settings.pointB;
        //priority = settings.priority;

        if (esetup)
        {
            ProcessEffectChange(effect);
        }
    }

    public WorldCameraSettings GetSettings()
    {
        WorldCameraSettings output = new WorldCameraSettings();

        output.mode = mode;
        output.effect = effect;
        output.distance = distance;
        output.directionVector = directionVector;
        output.cameraEulerAngles = targetEulerAngles;
        output.worldspaceYaw = targetYaw;
        output.movementHalflife = movementHalflife;

        output.distanceB = distanceB;
        output.directionVectorB = directionVectorB;
        output.cameraEulerAnglesB = cameraEulerAnglesB;
        output.worldspaceYawB = worldspaceYawB;
        output.pointA = pointA;
        output.pointB = pointB;

        return output;
    }

    public WorldCameraSnapshot MakeSnapshot()
    {
        WorldCameraSnapshot output = new WorldCameraSnapshot();

        output.settings = GetSettings();

        output.position = transform.position;
        output.rotation = transform.rotation;

        return output;
    }

    public void SetSnapshot(WorldCameraSnapshot wcs)
    {
        //Note that offset is reset
        offset = Vector3.zero;

        if (wcs == null)
        {
            mode = CameraMode.FollowPlayer;
            return;
        }

        SetCameraSettings(wcs.settings);
        transform.position = wcs.position;
        transform.rotation = wcs.rotation;
    }

    public void SetOffset(Vector3 newOffset)
    {
        Vector3 diff = newOffset - offset;
        transform.position += diff;
        offset = newOffset;
    }

    public void ProcessEffectChange(CameraEffect ce)
    {
        //set up the camera material

        effect = ce;

        if (ce == CameraEffect.None)
        {
            material = null;
            return;
        }

        //Lazy way
        material = Resources.Load<Material>("MaterialsShaders/CameraEffects/" + ce.ToString());

        if (material == null)
        {
            Debug.LogWarning("Material did not load in properly");
        }
    }

    public void DefaultFocusCalculation()
    {
        WorldPlayer player = WorldPlayer.Instance;
        if (player != null)
        {
            if (player.GetActionState() == WorldPlayer.ActionState.HazardFall)
            {
                float pasty = focus.y;
                focus = player.transform.position;
                focus.y = pasty;
            }
            else
            {
                focus = player.transform.position;
                focus.y = player.lastGroundedHeight;

                if (player.transform.position.y - focus.y > MAX_UPOFFSET)
                {
                    focus.y = player.transform.position.y + MAX_UPOFFSET;
                }
                if (player.transform.position.y - focus.y < -MAX_DOWNOFFSET)
                {
                    focus.y = player.transform.position.y - MAX_DOWNOFFSET;
                }
            }
        }
    }
    public void FocusCalculationNoVertical()
    {
        WorldPlayer player = WorldPlayer.Instance;
        if (player != null)
        {
            float pasty = focus.y;
            focus = player.transform.position;
            focus.y = pasty;
        }
    }

    public Vector3 FollowPlayer(bool interpolate)
    {
        DefaultFocusCalculation();

        Vector3 deltaB = focus - pointA;
        Vector3 lineB = pointB - pointA;

        float realdistance = distance;
        float interpValue = Mathf.Clamp01(Vector3.Dot(deltaB, lineB) / Vector3.Dot(lineB, lineB));
        Vector3 realEA = cameraEulerAngles;
        float realYaw = worldspaceYaw;
        if (interpolate)
        {
            realdistance = distance * (1 - interpValue) + distanceB * interpValue;
            realEA = cameraEulerAngles * (1 - interpValue) + cameraEulerAnglesB * interpValue;
            realYaw = worldspaceYaw * (1 - interpValue) + worldspaceYawB * interpValue;
        }

        //Position camera with parameters specified
        Vector3 offset = directionVector.normalized * realdistance;

        Vector2 newxz = MainManager.Instance.WorldspaceXZTransform(new Vector2(offset.x, offset.z));

        offset.x = newxz.x;
        offset.z = newxz.y;

        //transform.eulerAngles = cameraEulerAngles + MainManager.Instance.GetWorldspaceYaw() * Vector3.up;
        //transform.position = focus + offset;
        targetEulerAngles = realEA + realYaw * Vector3.up;
        return focus + offset;
    }

    public Vector3 FollowPlayerVertical(bool interpolate)
    {
        FocusCalculationNoVertical();

        Vector3 deltaB = focus - pointA;
        Vector3 lineB = pointB - pointA;

        float realdistance = distance;
        float interpValue = Mathf.Clamp01(Vector3.Dot(deltaB, lineB) / Vector3.Dot(lineB, lineB));
        Vector3 realEA = cameraEulerAngles;
        float realYaw = worldspaceYaw;
        if (interpolate)
        {
            realdistance = distance * (1 - interpValue) + distanceB * interpValue;
            realEA = cameraEulerAngles * (1 - interpValue) + cameraEulerAnglesB * interpValue;
            realYaw = worldspaceYaw * (1 - interpValue) + worldspaceYawB * interpValue;
        }

        //Position camera with parameters specified
        Vector3 offset = directionVector.normalized * realdistance;

        Vector2 newxz = MainManager.Instance.WorldspaceXZTransform(new Vector2(offset.x, offset.z));

        offset.x = newxz.x;
        offset.z = newxz.y;

        //transform.eulerAngles = cameraEulerAngles + MainManager.Instance.GetWorldspaceYaw() * Vector3.up;
        //transform.position = focus + offset;
        targetEulerAngles = realEA + realYaw * Vector3.up;
        return focus + offset;
    }

    public Vector3 HardFollowPlayer()
    {
        WorldPlayer player = WorldPlayer.Instance;
        if (player != null)
        {
            focus = player.transform.position;
        }
        //Position camera with parameters specified
        Vector3 offsetB = directionVector.normalized * distance;

        Vector2 newxzB = MainManager.Instance.WorldspaceXZTransform(new Vector2(offsetB.x, offsetB.z));

        offsetB.x = newxzB.x;
        offsetB.z = newxzB.y;

        targetEulerAngles = cameraEulerAngles + worldspaceYaw * Vector3.up;
        return focus + offsetB;
    }

    public Vector3 RestrictAlongLine(bool clamp, bool interpolate)
    {
        DefaultFocusCalculation();

        //Position camera with parameters specified

        Vector3 deltaB = focus - pointA;
        Vector3 lineB = pointB - pointA;
        Vector3 projectB;

        float realdistance = distance;
        float interpValue = Mathf.Clamp01(Vector3.Dot(deltaB, lineB) / Vector3.Dot(lineB, lineB));
        Vector3 realEA = cameraEulerAngles;
        float realYaw = worldspaceYaw;
        if (interpolate)
        {
            realdistance = distance * (1 - interpValue) + distanceB * interpValue;
            realEA = cameraEulerAngles * (1 - interpValue) + cameraEulerAnglesB * interpValue;
            realYaw = worldspaceYaw * (1 - interpValue) + worldspaceYawB * interpValue;
        }
        Vector3 offset = directionVector.normalized * realdistance;

        Vector2 newxz = MainManager.Instance.WorldspaceXZTransform(new Vector2(offset.x, offset.z));

        offset.x = newxz.x;
        offset.z = newxz.y;

        if (clamp)
        {
            projectB = pointA + lineB * interpValue;
        } else
        {
            projectB = pointA + lineB * Vector3.Dot(deltaB, lineB) / Vector3.Dot(lineB, lineB);
        }

        focus = projectB;


        //transform.eulerAngles = cameraEulerAngles + MainManager.Instance.GetWorldspaceYaw() * Vector3.up;
        //transform.position = focus + offset;
        targetEulerAngles = realEA + realYaw * Vector3.up;
        return focus + offset;
    }

    public Vector3 RestrictBetweenPlanes(bool interpolate)
    {
        DefaultFocusCalculation();

        //Position camera with parameters specified

        Vector3 deltaB = focus - pointA;
        Vector3 lineB = pointB - pointA;
        Vector3 projectB;

        float realdistance = distance;
        float interpValue = Mathf.Clamp01(Vector3.Dot(deltaB, lineB) / Vector3.Dot(lineB, lineB));
        Vector3 realEA = cameraEulerAngles;
        float realYaw = worldspaceYaw;
        if (interpolate)
        {
            realdistance = distance * (1 - interpValue) + distanceB * interpValue;
            realEA = cameraEulerAngles * (1 - interpValue) + cameraEulerAnglesB * interpValue;
            realYaw = worldspaceYaw * (1 - interpValue) + worldspaceYawB * interpValue;
        }
        Vector3 offset = directionVector.normalized * realdistance;

        Vector2 newxz = MainManager.Instance.WorldspaceXZTransform(new Vector2(offset.x, offset.z));

        offset.x = newxz.x;
        offset.z = newxz.y;

        //rebuild the vector
        Vector3 perpendicular = focus - (pointA + lineB * (Vector3.Dot(deltaB, lineB) / Vector3.Dot(lineB, lineB)));
        projectB = pointA + lineB * interpValue;


        focus = projectB + perpendicular;


        //transform.eulerAngles = cameraEulerAngles + MainManager.Instance.GetWorldspaceYaw() * Vector3.up;
        //transform.position = focus + offset;
        targetEulerAngles = realEA + realYaw * Vector3.up;
        return focus + offset;
    }

    public Vector3 RestrictOnPlane()
    {
        DefaultFocusCalculation();

        Vector3 planeNormal = pointA - pointB;

        //Position camera with parameters specified

        Vector3 delta = focus - pointA;
        Vector3 project = planeNormal * (Vector3.Dot(delta, planeNormal) / Vector3.Dot(planeNormal, planeNormal));

        Vector3 onPlane = delta - project + pointA;
        focus = onPlane;

        float realdistance = distance;
        Vector3 realEA = cameraEulerAngles;
        float realYaw = worldspaceYaw;
        Vector3 offset = directionVector.normalized * realdistance;

        Vector2 newxz = MainManager.Instance.WorldspaceXZTransform(new Vector2(offset.x, offset.z));

        offset.x = newxz.x;
        offset.z = newxz.y;

        //transform.eulerAngles = cameraEulerAngles + MainManager.Instance.GetWorldspaceYaw() * Vector3.up;
        //transform.position = focus + offset;
        targetEulerAngles = realEA + realYaw * Vector3.up;
        return focus + offset;
    }

    public Vector3 RadialFollow(bool reverse, bool fix)
    {
        DefaultFocusCalculation();

        Vector3 offset = directionVector.normalized * distance;

        WorldPlayer player = WorldPlayer.Instance;
        Vector2 newxz = (reverse ? 1 : -1) * distance * (MainManager.XZProject(pointA - player.transform.position)).normalized; //MainManager.Instance.WorldspaceXZTransform(new Vector2(offset.x, offset.z));

        offset.x = newxz.x;
        offset.z = newxz.y;

        //transform.eulerAngles = cameraEulerAngles + MainManager.Instance.GetWorldspaceYaw() * Vector3.up;
        //transform.position = focus + offset;

        targetYaw = -Vector2.SignedAngle(Vector2.down, newxz);
        if (targetYaw < 0)
        {
            targetYaw += 360;
        }
        targetEulerAngles = cameraEulerAngles + worldspaceYaw * Vector3.up;

        float abDist = (MainManager.XZProject(pointA - pointB)).magnitude;

        Vector2 abXZ = MainManager.XZProject(focus - pointA).normalized * abDist;

        focus.x = abXZ.x;
        focus.z = abXZ.y;

        focus += pointA;

        return focus + offset;
    }

    public void SnapToTargets()
    {
        worldspaceYaw = targetYaw;
        transform.position = offset + GetTargetPosition();
        transform.eulerAngles = targetEulerAngles;
    }

    public Vector3 GetTargetPosition()
    {
        Vector3 targetPosition = Vector3.zero;
        switch (mode)
        {
            case CameraMode.ManualSettings:
                targetPosition = pointA; //transform.position;
                break;
            case CameraMode.FollowPlayer:
                targetPosition = FollowPlayer(false);
                break;
            case CameraMode.FollowPlayerNoVertical:
                targetPosition = FollowPlayerVertical(false);
                break;
            case CameraMode.HardFollowPlayer:
                targetPosition = HardFollowPlayer();
                break;
            case CameraMode.RestrictAlongLine:
                //follow player logic just with some stuff added
                //note: the line is where the focus is constrained along, the other camera properties can be changed normally
                //note: worldspace yaw should be perpendicular to the line between point A and B because otherwise the vector projection will look weird
                //player won't be in center of camera anymore
                targetPosition = RestrictAlongLine(false, false);
                break;
            case CameraMode.RestrictBetweenPoints:
                //follow player logic just with some stuff added
                //This version clamps between the points
                //note: the line is where the focus is constrained along, the other camera properties can be changed normally
                //note: worldspace yaw should be perpendicular to the line between point A and B because otherwise the vector projection will look weird
                //player won't be in center of camera anymore
                targetPosition = RestrictAlongLine(true, false);
                break;
            case CameraMode.RestrictBetweenPlanes:
                targetPosition = RestrictBetweenPlanes(false);
                break;
            case CameraMode.RestrictOnPlane:
                targetPosition = RestrictOnPlane();
                break;
            case CameraMode.FollowPlayerInterpolate:
                targetPosition = FollowPlayer(true);
                break;
            case CameraMode.RestrictAlongLineInterpolate:
                targetPosition = RestrictAlongLine(false, true);
                break;
            case CameraMode.RestrictBetweenPointsInterpolate:
                targetPosition = RestrictAlongLine(true, true);
                break;
            case CameraMode.RestrictBetweenPlanesInterpolate:
                targetPosition = RestrictBetweenPlanes(true);
                break;
            case CameraMode.RadialFollow:
                targetPosition = RadialFollow(false, false);
                break;
            case CameraMode.RadialFollowReverse:
                targetPosition = RadialFollow(true, false);
                break;
            case CameraMode.RadialFollowFixedDistance:
                targetPosition = RadialFollow(false, true);
                break;
            case CameraMode.RadialFollowReverseFixedDistance:
                targetPosition = RadialFollow(true, true);
                break;
        }

        return targetPosition;
    }

    //Note: player moves in fixed update and so the camera becomes a bit unstable with normal update (*though only if the camera movement is not instant)
    void FixedUpdate()
    {
        if (MainManager.Instance.Cheat_RevolvingCam && !MainManager.Instance.Cheat_FreeCam)
        {
            //Revolving cam mode

            //LR to rotate

            Vector2 inputXY = Vector2.zero;

            inputXY.x = InputManager.GetAxisHorizontal();
            inputXY.y = InputManager.GetAxisVertical();
            if (inputXY.magnitude > 1)
            {
                inputXY = inputXY.normalized;
            }

            Vector3 delta = transform.position - focus;

            float rotationAmount = inputXY.x * 60 * Time.fixedDeltaTime;

            transform.eulerAngles += Vector3.up * rotationAmount;

            Quaternion qrot = Quaternion.Euler(0, rotationAmount, 0);
            delta = qrot * delta;

            worldspaceYaw += rotationAmount;
            if (worldspaceYaw < 0)
            {
                worldspaceYaw += 360;
            }
            if (worldspaceYaw > 360)
            {
                worldspaceYaw -= 360;
            }
            MainManager.Instance.SetWorldspaceYaw(worldspaceYaw);

            transform.position = focus + delta;

            return;
        }

        if (MainManager.Instance.Cheat_FreeCam)
        {
            //do our own stuff
            //Move with directionals
            //A to go up
            //Z to go down
            //B to enter rotation mode

            Vector2 inputXY = Vector2.zero;

            inputXY.x = InputManager.GetAxisHorizontal();
            inputXY.y = InputManager.GetAxisVertical();
            if (inputXY.magnitude > 1)
            {
                inputXY = inputXY.normalized;
            }

            if (inputXY.magnitude == 0)
            {
                freeCamHoldTime = 0;
            } else
            {
                freeCamHoldTime += Time.fixedDeltaTime;
            }

            float speedMult = 1;

            if (freeCamHoldTime > 1.5)
            {
                speedMult = 3;
            }
            else if (freeCamHoldTime > 0.5)
            {
                speedMult = 2;
            }

            float x = transform.eulerAngles.x;
            float y = transform.eulerAngles.y;

            x += Input.GetAxis("MouseY") * Time.fixedDeltaTime * -0.01f;
            y += Input.GetAxis("MouseX") * Time.fixedDeltaTime * 0.01f;

            if (x > 180 && x < 270)
            {
                x = 270;
            }
            if (x > 90 && x < 180)
            {
                x = 90;
            }

            if (y > 360)
            {
                y -= 360;
            }
            if (y < 0)
            {
                y += 360;
            }

            transform.eulerAngles = Vector3.right * x + Vector3.up * y;
            targetEulerAngles = transform.eulerAngles;

            if (pastFreeCamMode)
            {
                freeCamHoldTime = 0;
            }
            pastFreeCamMode = false;
            float moveSpeed = 10 * speedMult;

            transform.position += transform.forward * moveSpeed * Time.fixedDeltaTime * inputXY.y;
            transform.position += transform.right * moveSpeed * Time.fixedDeltaTime * inputXY.x;

            float yMove = 0;
            if (InputManager.GetButton(InputManager.Button.A))
            {
                yMove += 1;
            }
            if (InputManager.GetButton(InputManager.Button.Z))
            {
                yMove -= 1;
            }
            transform.position += transform.up * moveSpeed * Time.fixedDeltaTime * yMove;

            return;
        }

        Vector3 targetPosition = GetTargetPosition();


        //Follow targets

        float tempYaw = targetYaw;
        if (tempYaw - worldspaceYaw > 180)
        {
            tempYaw -= 360;
        }
        if (tempYaw - worldspaceYaw < -180)
        {
            tempYaw += 360;
        }
        worldspaceYaw = MainManager.EasingExponentialFixedTime(worldspaceYaw, tempYaw, movementHalflife);
        if (worldspaceYaw > 360)
        {
            worldspaceYaw -= 360;
        }
        if (worldspaceYaw < 0)
        {
            worldspaceYaw += 360;
        }

        Vector3 pastPos = transform.position;
        transform.position = offset + MainManager.EasingExponentialFixedTime(transform.position - offset, targetPosition, movementHalflife);

        Vector3 tempEulerAngles = targetEulerAngles;
        if (tempEulerAngles.x - transform.eulerAngles.x > 180)
        {
            tempEulerAngles.x -= 360;
        }
        if (tempEulerAngles.x - transform.eulerAngles.x < -180)
        {
            tempEulerAngles.x += 360;
        }
        if (tempEulerAngles.y - transform.eulerAngles.y > 180)
        {
            tempEulerAngles.y -= 360;
        }
        if (tempEulerAngles.y - transform.eulerAngles.y < -180)
        {
            tempEulerAngles.y += 360;
        }
        if (tempEulerAngles.z - transform.eulerAngles.z > 180)
        {
            tempEulerAngles.z -= 360;
        }
        if (tempEulerAngles.z - transform.eulerAngles.z < -180)
        {
            tempEulerAngles.z += 360;
        }
        if (!(MainManager.Instance.worldMode == MainManager.WorldMode.Overworld && (mode == CameraMode.FirstPerson_DoNotUse || MainManager.Instance.Cheat_FirstPersonCamera)))
        {
            transform.eulerAngles = MainManager.EasingExponentialFixedTime(transform.eulerAngles, tempEulerAngles, movementHalflife);
        }
        if (transform.eulerAngles.y < 0)
        {
            transform.eulerAngles += Vector3.up * 360;
        }
        if (transform.eulerAngles.y > 360)
        {
            transform.eulerAngles -= Vector3.up * 360;
        }
        //Debug.Log((transform.position - pastPos).magnitude);


        if (MainManager.Instance.worldMode == MainManager.WorldMode.Overworld && (mode == CameraMode.FirstPerson_DoNotUse || MainManager.Instance.Cheat_FirstPersonCamera))
        {
            WorldPlayer wp = WorldPlayer.Instance;
            if (wp != null)
            {
                transform.position = wp.transform.position + WorldPlayer.EYE_HEIGHT * Vector3.up;

                /*
                float angle = (wp.GetTrueFacingRotation() + 90);
                if (angle > 360)
                {
                    angle -= 360;
                }
                if (angle < 0)
                {
                    angle += 360;
                }
                transform.eulerAngles = (angle) * Vector3.up;
                */

                float x = transform.eulerAngles.x;
                float y = transform.eulerAngles.y;

                x += Input.GetAxis("MouseY") * Time.fixedDeltaTime * -0.01f;
                y += Input.GetAxis("MouseX") * Time.fixedDeltaTime * 0.01f;


                if (x > 180 && x < 270)
                {
                    x = 270;
                }
                if (x > 90 && x < 180)
                {
                    x = 90;
                }

                if (y > 360)
                {
                    y -= 360;
                }
                if (y < 0)
                {
                    y += 360;
                }

                transform.eulerAngles = Vector3.right * x + Vector3.up * y;
                targetEulerAngles = transform.eulerAngles;
                targetYaw = y;

                worldspaceYaw = targetYaw;
            }

            /*
            if (mode == CameraMode.FirstPerson_DoNotUse)
            {
                worldspaceYaw = targetYaw;
            }
            */
        }


        MainManager.Instance.SetWorldspaceYaw(worldspaceYaw);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            Graphics.Blit(src, dest, material);
        } else
        {
            Graphics.Blit(src, dest);
        }
    }

    public void SetMode(CameraMode mode)
    {
        this.mode = mode;
    }
    public void SetManual(Vector3 position, Vector3 rotation, float halfLife = 0.05f)
    {
        mode = CameraMode.ManualSettings;
        transform.position = position;
        transform.eulerAngles = rotation;
        pointA = position;          //Target is pointA
        targetEulerAngles = rotation;
        targetYaw = 0;
        movementHalflife = halfLife;
    }
    //Exact same as SetManual, but does not instantaneously change camera settings so that it just moves to the targets
    //Halflife controls the speed
    public void SetManualDelayed(Vector3 targetPosition, Vector3 targetRotation, float halfLife = 0.05f)
    {
        mode = CameraMode.ManualSettings;
        targetEulerAngles = targetRotation;
        pointA = targetPosition;          //Target is pointA
        targetYaw = 0;
        movementHalflife = halfLife;
    }
}
