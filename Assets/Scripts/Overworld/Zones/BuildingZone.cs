using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingZone : WorldZone
{
    public bool active;

    public float max_time = 0.1f;
    public float max_distance = 10f;

    public GameObject frontObject;

    public float activeTime = 0;

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.75f, 0.5f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.4f);

        //house base
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, -0.5f, 0f), transform.position + new Vector3(-0.5f, 0.2f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.2f, 0f), transform.position + new Vector3(0.5f, 0.2f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.2f, 0f), transform.position + new Vector3(0.5f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.5f, 0f), transform.position + new Vector3(-0.5f, -0.5f, 0f));

        //roof
        Gizmos.DrawLine(transform.position + new Vector3(-0.7f, 0.2f, 0f), transform.position + new Vector3(-0.5f, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.5f, 0f), transform.position + new Vector3(0.5f, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.5f, 0f), transform.position + new Vector3(0.7f, 0.2f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.7f, 0.2f, 0f), transform.position + new Vector3(-0.7f, 0.2f, 0f));
    }

    public override void ProcessTrigger(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();
        if (wp != null)
        {
            active = true;
        }
    }

    public void FixedUpdate()
    {
        if (!mapScript.GetHalted())
        {
            if (active)
            {
                if (activeTime >= max_time)
                {
                    activeTime = max_time;
                } else
                {
                    activeTime += Time.fixedDeltaTime;
                }
            }
            else
            {
                if (activeTime <= 0)
                {
                    activeTime = 0;
                } else
                {
                    activeTime -= Time.fixedDeltaTime;
                }
            }

            frontObject.transform.localPosition = Vector3.up * max_distance * (activeTime / max_time);

            active = false;
        }
    }
}
