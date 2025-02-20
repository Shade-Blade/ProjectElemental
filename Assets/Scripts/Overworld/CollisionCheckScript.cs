using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Can't make a proxy script that checks this stuff so I have to make a whole new script to do this one thing
public class CollisionCheckScript : MonoBehaviour
{
    public bool isActive = false;
    
    //may need a buffered check if timing gets weird
    public int antilatency = 0;

    //order of this is weird
    private void OnTriggerStay(Collider other)
    {
        if (!other.isTrigger)
        {
            isActive = true;
            antilatency = 0;
        }
    }

    private void FixedUpdate()
    {
        isActive = false;
        antilatency++;
    }


    public bool CollisionCheck()
    {
        return antilatency > 10;
    }
}
