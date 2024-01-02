using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenVisibilityChecker : MonoBehaviour
{
    public float sphereRadius;

    public float timePerCheck = 0.5f;
    public float checkCountdown = 0;
    public Vector3[] positions;
    public int curPosition = 0;

    public int reteleportRequirement = 1;
    int reteleportCounter = 1;
    bool lastVisibleState = false;

    public Criteria criteria;

    public enum Criteria
    {
        Random,
        Closest,
        ClosestLeading,
        Farthest
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.0f, 0.2f, 0.2f, 0.3f); //this is gray, could be anything

        if (positions != null)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                Gizmos.DrawSphere(positions[i], sphereRadius);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool doCheck = false;
        if (checkCountdown <= 0)
        {
            checkCountdown = timePerCheck;
            doCheck = true;
        } else
        {
            checkCountdown -= Time.deltaTime;
        }

        if (doCheck)
        {
            bool[] legalPositions = new bool[positions.Length];
            legalPositions[curPosition] = false;    //don't try to teleport to yourself

            bool isLegal = !VisibilityCheck(positions[curPosition], sphereRadius);

            if (isLegal ^ lastVisibleState)
            {
                lastVisibleState = isLegal;
                reteleportCounter--;
            }

            if (isLegal && reteleportCounter <= 0)
            {
                int legalCount = 0;
                string tempPrint = "";
                for (int i = 0; i < legalPositions.Length; i++)
                {
                    if (i != curPosition || criteria != Criteria.Random)
                    {
                        legalPositions[i] = !VisibilityCheck(positions[i], sphereRadius);
                        if (legalPositions[i])
                        {
                            tempPrint += i + " ";
                            //Debug.Log(i);
                            legalCount++;
                        }
                    }
                }
                //Debug.Log(tempPrint);

                if (legalCount > 0)
                {
                    int index = -1;
                    Vector3 wpos = WorldPlayer.Instance.transform.position;
                    switch (criteria)
                    {
                        case Criteria.Random:
                            int k = Random.Range(0, legalCount);

                            for (int i = 0; i < legalPositions.Length; i++)
                            {
                                if (!legalPositions[i])
                                {
                                    continue;
                                }

                                if (legalPositions[i] && k == 0)
                                {
                                    index = i;
                                    break;
                                }

                                k--;
                            }
                            break;
                        case Criteria.Closest:
                            float dist = -1;
                            float d;
                            for (int i = 0; i < legalPositions.Length; i++)
                            {
                                if (!legalPositions[i])
                                {
                                    continue;
                                }

                                if (legalPositions[i])
                                {
                                    d = (positions[i] - wpos).magnitude;
                                    if (dist == -1 || d < dist)
                                    {
                                        dist = d;
                                        index = i;
                                    }
                                }
                            }
                            break;
                        case Criteria.ClosestLeading:
                            Vector3 newpos = wpos + WorldPlayer.Instance.rb.velocity.normalized * 4;
                            float distC = -1;
                            float dC;
                            for (int i = 0; i < legalPositions.Length; i++)
                            {
                                if (!legalPositions[i])
                                {
                                    continue;
                                }

                                if (legalPositions[i])
                                {
                                    dC = (positions[i] - newpos).magnitude;
                                    if (distC == -1 || dC < distC)
                                    {
                                        distC = dC;
                                        index = i;
                                    }
                                }
                            }
                            break;
                        case Criteria.Farthest:
                            float distB = -1;
                            float dB;
                            for (int i = 0; i < legalPositions.Length; i++)
                            {
                                if (!legalPositions[i])
                                {
                                    continue;
                                }

                                if (legalPositions[i])
                                {
                                    dB = (positions[i] - wpos).magnitude;
                                    if (distB == -1 || dB > distB)
                                    {
                                        distB = dB;
                                        index = i;
                                    }
                                }
                            }
                            break;
                    }

                    if (index != -1 && index != curPosition)
                    {
                        transform.position = positions[index];
                        checkCountdown = 1;
                        curPosition = index;
                        reteleportCounter = reteleportRequirement;
                    }
                }
            }
        }

        // Debug.Log("Renderer visibility: " + mr.isVisible + " Frustum visibility: "+ ScreenFrustumCheckSphere(transform.position, sphereRadius) + "  Occlusion visibility: " + OcclusionCheck(transform.position));
    }

    public bool VisibilityCheck(Vector3 pos, float radius)
    {
        if (ScreenFrustumCheckSphere(pos, radius))
        {
            return OcclusionCheck(pos, radius);
        }

        return false;
    }

    public bool ScreenFrustumCheckSphere(Vector3 pos, float radius, bool shadowCheck = true)
    {
        if (ScreenFrustumCheck(pos))
        {
            return true;
        }

        //check whatever points should be closest to being on screen
        Quaternion cr = MainManager.Instance.Camera.transform.rotation;
        Vector3 camForward = cr * Vector3.forward;
        Vector3 camUp = cr * Vector3.up;
        Vector3 camRight = cr * Vector3.right;

        Vector2 relpos = RectTransformUtility.WorldToScreenPoint(MainManager.Instance.Camera.camera, pos);
        relpos -= new Vector2(Screen.width / 2, Screen.height / 2);

        //point inward
        Vector3 worldrelpos = -camRight * relpos.x + -camUp * relpos.y;

        worldrelpos = Vector3.Normalize(worldrelpos);

        Vector3 checkpos = pos + radius * worldrelpos;

        if (ScreenFrustumCheck(checkpos))
        {
            return true;
        }

        if (shadowCheck && ScreenFrustumCheck(checkpos + camForward * radius))
        {
            return true;
        }

        return false;
    }

    public bool ScreenFrustumCheck(Vector3 pos)
    {
        //are we on screen based on the camera frustum?
        Vector2 spos = RectTransformUtility.WorldToScreenPoint(MainManager.Instance.Camera.camera, pos);
        spos /= new Vector2(Screen.width, Screen.height);

        if (spos.x < 0 || spos.y < 0 || spos.x > 1 || spos.y > 1)
        {
            return false;
        }

        Transform t = MainManager.Instance.Camera.transform;
        float c = Vector3.Dot((pos - t.position).normalized, t.rotation * Vector3.forward);
        float threshold = -0.25f + Mathf.Cos(MainManager.Instance.Camera.camera.fieldOfView * (Mathf.PI / 360));
        if (c < threshold)
        {
            return false;
        }

        return true;
    }

    public bool OcclusionCheck(Vector3 pos, float radius)
    {
        //a lot harder :(
        //heuristic: use a raycast, assume no invisible walls between
        //(and no visible but colliderless models)

        RaycastHit rch;

        Quaternion cr = MainManager.Instance.Camera.transform.rotation;
        Vector3 camUp = cr * Vector3.up;
        Vector3 camRight = cr * Vector3.right;

        Vector3 cp = MainManager.Instance.Camera.transform.position;

        float threshold = -0.25f + Mathf.Cos(MainManager.Instance.Camera.camera.fieldOfView * (Mathf.PI / 360));
        if (Vector3.Dot(pos - cp, cr * Vector3.forward) < threshold)
        {
            return false;
        }


        Physics.Raycast(cp, pos - cp, out rch, (pos - cp).magnitude - sphereRadius, 311, QueryTriggerInteraction.Ignore);

        if (rch.collider == null)
        {
            return true;
        }

        Ray r = MainManager.Instance.Camera.camera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(MainManager.Instance.Camera.camera, pos + camRight * radius));
        Physics.Raycast(r, out rch, (pos - cp + camRight * sphereRadius).magnitude - 0.25f, 311, QueryTriggerInteraction.Ignore);

        if (rch.collider == null)
        {
            return true;
        } else
        {
            //Debug.DrawRay(r.origin, rch.point - r.origin, new Color(1, 1, 1, 1), 0.1f);
        }

        r = MainManager.Instance.Camera.camera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(MainManager.Instance.Camera.camera, pos - camRight * radius));
        Physics.Raycast(r, out rch, (pos - cp + camRight * sphereRadius).magnitude - 0.25f, 311, QueryTriggerInteraction.Ignore);

        if (rch.collider == null)
        {
            return true;
        }
        else
        {
            //Debug.DrawRay(r.origin, rch.point - r.origin, new Color(1, 1, 1, 1), 0.1f);
        } 

        r = MainManager.Instance.Camera.camera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(MainManager.Instance.Camera.camera, pos + camUp * radius));
        Physics.Raycast(r, out rch, (pos - cp + camRight * sphereRadius).magnitude - 0.25f, 311, QueryTriggerInteraction.Ignore);

        if (rch.collider == null)
        {
            return true;
        }
        else
        {
            //Debug.DrawRay(r.origin, rch.point - r.origin, new Color(1, 1, 1, 1), 0.1f);
        }

        r = MainManager.Instance.Camera.camera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(MainManager.Instance.Camera.camera, pos - camUp * radius));
        Physics.Raycast(r, out rch, (pos - cp + camRight * sphereRadius).magnitude - 0.25f, 311, QueryTriggerInteraction.Ignore);

        if (rch.collider == null)
        {
            return true;
        }
        else
        {
            //Debug.DrawRay(r.origin, rch.point - r.origin, new Color(1, 1, 1, 1), 0.1f);
        }

        return false;
    }
}
