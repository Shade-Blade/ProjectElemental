using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractScript : InteractTrigger
{
    public DoorScript ds;
    public bool front;

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (front)
        {
            Gizmos.color = new Color(1, 1, 0.25f, 0.5f);
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
        else
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.25f, 0.5f);
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
    }

    public override void Interact()
    {
        if (front)
        {
            StartCoroutine(MainManager.Instance.ExecuteCutscene(ds.FrontSideOpen()));
        }
        else
        {
            StartCoroutine(MainManager.Instance.ExecuteCutscene(ds.BackSideOpen()));
        }
    }
}
