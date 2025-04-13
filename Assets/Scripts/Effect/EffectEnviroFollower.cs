using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectEnviroFollower : WorldObject
{
    //Used for battle map world effects
    public bool disableMovement;

    //use on map entrance to avoid the position snapping effect
    public void Warp()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        ps.Clear();
        if (!disableMovement && WorldPlayer.Instance != null)
        {
            transform.position = WorldPlayer.Instance.transform.position;
        }
    }


    public override void Update()
    {
        if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
        {
            WorldUpdate();
            return;
        }

        base.Update();
    }

    public override void FixedUpdate()
    {
        if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
        {
            WorldFixedUpdate();
            return;
        }

        base.FixedUpdate();
    }

    public override void WorldUpdate()
    {
        if (!disableMovement && WorldPlayer.Instance != null)
        {
            transform.position = WorldPlayer.Instance.transform.position;
        }
    }
}
