using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperScript : WorldObject
{
    public float reflectStrength = 1;
    public float momentumStrength = 1;
    public float minReflectPower;

    public override void ProcessCollision(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            if (collision.transform.GetComponent<WorldEntity>() != null)
            {
                WorldEntity w = collision.transform.GetComponent<WorldEntity>();

                if (w.timeSinceLaunch > 0.1)
                {
                    Vector3 launchVel = w.lastFrameVel;

                    Vector3 normal = Vector3.zero;
                    int normalCount = 0;

                    foreach (ContactPoint contact in collision.contacts)
                    {
                        normal += contact.normal.normalized;
                        normalCount++;
                    }

                    if (normalCount > 0)
                    {
                        normal /= normalCount;
                        normal = -normal;   //this is from the perspective of the platform so we need to reflect it 
                    }

                    launchVel = Vector3.Reflect(launchVel, normal);

                    launchVel *= reflectStrength;

                    if (launchVel.magnitude < minReflectPower)
                    {
                        launchVel = launchVel.normalized * minReflectPower;
                    }

                    w.Launch(launchVel, momentumStrength);
                }
            }
        }
    }
}
