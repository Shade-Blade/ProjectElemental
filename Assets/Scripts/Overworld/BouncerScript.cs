using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncerScript : WorldObject
{
    public float minLaunch;
    public float firstBounceBonus;
    public override void ProcessCollision(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            if (collision.transform.GetComponent<WorldEntity>() != null)
            {
                WorldEntity w = collision.transform.GetComponent<WorldEntity>();

                Vector3 launchVel = Vector3.up * minLaunch;
                //need a failsafe
                if ((w.lastHighestHeight - w.FeetPosition().y) > 0)
                {
                    if (!w.usedFirstBounce)
                    {
                        w.lastHighestHeight += firstBounceBonus;
                    }

                    launchVel = Vector3.up * Mathf.Sqrt(2 * -Physics.gravity.y * (w.lastHighestHeight - w.FeetPosition().y));

                    //Need to calculate a velocity integral then find the inverse to get velocity from height
                    //but luckily it is just a triangle shaped area

                    //triangle height = (V)
                    //triangle width = (V / -Physics.gravity.y)
                    //triangle area = V^2 / (2 * -Physics.gravity.y)

                    //H = V^2 / (2 * -Physics.gravity.y)
                    //V = sqrt(2 * -Physics.gravity.y * H)
                }
                if (float.IsNaN(launchVel.y) || launchVel.y < minLaunch)
                {
                    launchVel.y = minLaunch;
                }
                w.usedFirstBounce = true;
                w.Launch(launchVel, 0);
            }
        }
    }
}
