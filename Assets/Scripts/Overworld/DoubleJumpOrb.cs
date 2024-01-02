using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumpOrb : WorldObject
{
    public bool active = true;
    public void Start()
    {
        
    }

    public override void WorldUpdate()
    {
        WorldPlayer wp = WorldPlayer.Instance;

        if (wp.IsGrounded())
        {
            active = true;
        }
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
        if (wp != null)
        {
            if (active && !wp.IsGrounded() && !wp.canDoubleJump)
            {
                wp.canDoubleJump = true;
                active = false;
            }
        }
    }
}
