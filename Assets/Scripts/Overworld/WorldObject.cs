using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Very bare bones class for most overworld objects
//Only has support for obeying mapScript.GetHalted()
public class WorldObject : MonoBehaviour
{
    [HideInInspector]
    public MapScript mapScript;
    public Rigidbody rb;
    [HideInInspector]
    public Vector3 bufferVelocity;
    [HideInInspector]
    public bool forceKinematic;

    public virtual void Awake()
    {
        mapScript = FindObjectOfType<MapScript>();
        //rb = FindObjectOfType<Rigidbody>();
    }

    //map init, called by mapscript
    public virtual void WorldInit()
    {

    }

    public virtual void Update()
    {
        if (rb != null)
        {
            bool nextKinematic = mapScript.GetHalted() || forceKinematic;
            if (!rb.isKinematic && nextKinematic)
            {
                bufferVelocity = rb.velocity;
            }
            if (rb.isKinematic && !nextKinematic)
            {
                rb.velocity = bufferVelocity;
            }
            rb.isKinematic = nextKinematic;
        }
        if (!mapScript.GetHalted())
        {
            WorldUpdate();
        }
    }

    public virtual void WorldUpdate()
    {

    }

    public virtual void FixedUpdate()
    {
        if (!mapScript.GetHalted())
        {
            WorldFixedUpdate();
        }
    }

    public virtual void WorldFixedUpdate()
    {

    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        ProcessCollision(collision);
    }

    public virtual void OnCollisionStay(Collision collision)
    {
        ProcessCollision(collision);
    }

    public virtual void ProcessCollision(Collision collision)
    {

    }

    public virtual void OnHazardReset()
    {

    }
}
