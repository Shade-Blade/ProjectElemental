using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//even lower than WorldObject as it might hurt performance to have a bunch of useless update scripts running around
//(probably not, but it can't hurt)
public class WorldZone : MonoBehaviour
{
    public MapScript mapScript;
    public virtual void Awake()
    {
        mapScript = FindObjectOfType<MapScript>();
        //rb = FindObjectOfType<Rigidbody>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (mapScript != null && !mapScript.GetHalted())
        {
            ProcessTrigger(other);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (!mapScript.GetHalted())
        {
            ProcessTrigger(other);
        }
    }

    public virtual void ProcessTrigger(Collider other)
    {

    }
}
