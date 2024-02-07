using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectEnviroFollower : WorldObject
{
    //Used for battle map world effects
    public bool disableMovement;

    public override void WorldUpdate()
    {
        if (!disableMovement && WorldPlayer.Instance != null)
        {
            transform.position = WorldPlayer.Instance.transform.position;
        }
    }
}
