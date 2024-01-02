using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericStateTrigger : WorldZone
{
    public WorldPlayer.ActionState action;

    public override void ProcessTrigger(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (wp != null)
        {
            if (wp.GetActionState() == action)
            {
                Destroy(gameObject);
            }
        }
    }
}
