using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class JumpTool : MonoBehaviour
{   
    public enum JumpType
    {
        DashVertical,
        LunaVertical,
        WilexVertical,
        DoubleVertical,
        SuperVertical,
        LunaHorizontal,
        WilexHorizontal,
        DashHorizontal,
        DoubleHorizontal,
        DashDoubleHorizontal,
    }

    public JumpType jumpType;
    public int steps;
    public bool precomputeK;
    public float newK;

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.25f);
        Vector3 facing = transform.rotation * Vector3.right;

        float impulse = 1;
        float lift = 1;

        float impulseB = 1;
        float liftB = 1;

        int stages = 1;
        float horizontalVelocity = 3.3f;
        float horizontalVelocityB = 3.3f;

        float grav = 13;

        switch (jumpType)
        {
            case JumpType.DashVertical:
                impulse = 2.75f;
                lift = 2.5f;
                horizontalVelocity = 0f;
                stages = 1;
                break;
            case JumpType.LunaVertical:
                impulse = 3.5f;
                lift = 1.5f;
                horizontalVelocity = 0f;
                stages = 1;
                break;
            case JumpType.WilexVertical:
                impulse = 4f;
                lift = 2.5f;
                horizontalVelocity = 0f;
                stages = 1;
                break;
            case JumpType.DoubleVertical:
                impulse = 4f;
                lift = 2.5f;
                impulseB = 4.5f;
                liftB = 1.25f;
                horizontalVelocity = 0f;
                horizontalVelocityB = 0f;
                stages = 2;
                break;
            case JumpType.SuperVertical:
                impulse = 9.25f;
                lift = -5f;
                horizontalVelocity = 0f;
                stages = 1;
                break;
            case JumpType.DoubleHorizontal:
                impulse = 4f;
                lift = 2.5f;
                impulseB = 4.5f;
                liftB = 1.25f;
                horizontalVelocity = 3.3f;
                horizontalVelocityB = 3.3f;
                stages = 3;
                break;
            case JumpType.LunaHorizontal:
                impulse = 3.5f;
                lift = 1.5f;
                horizontalVelocity = 3.3f;
                stages = 2;
                break;
            case JumpType.WilexHorizontal:
                impulse = 4f;
                lift = 2.5f;
                horizontalVelocity = 3.3f;
                stages = 2;
                break;
            case JumpType.DashHorizontal:
                impulse = 2.75f;
                lift = 2.5f;
                horizontalVelocity = 6f;
                stages = 2;
                break;
            case JumpType.DashDoubleHorizontal:
                impulse = 2.75f;
                lift = 2.5f;
                impulseB = 4.5f;
                liftB = 1.25f;
                horizontalVelocity = 6f;
                horizontalVelocityB = 3.3f;
                stages = 3;
                break;
        }

        float xpoint = (impulse / (grav - lift));

        float a = (-grav / 2);
        float b = -grav * (impulse / (grav - lift));
        float c = (-grav / 2) * (impulse / (grav - lift)) * (impulse / (grav - lift)) + ((impulse * impulse) / (2 * (grav - lift)));
        float end = -((-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a));

        float f1(float x)
        {
            return (impulse * x) - ((grav - lift) / 2) * (x * x);
        }
        float f2(float x)
        {
            return a * (x - xpoint) * (x - xpoint) + ((impulse * impulse) / (2 * (grav - lift)));
        }

        float f3(float x)
        {
            if (x < xpoint)
            {
                return f1(x);
            }
            else
            {
                return f2(x);
            }
        }

        float k = (impulse / (grav - lift));
        if (!precomputeK)
        {
            k = newK;
        }

        float u = f3(k);  //height at time = k


        float xpointB = k + (impulseB) / (grav - liftB);

        float u2 = u + ((impulseB * impulseB) / (grav - liftB)) - ((grav - liftB) / 2) * ((impulseB / (grav - liftB))) * ((impulseB / (grav - liftB)));

        float a2 = a;
        float b2 = -grav * (-k - (impulseB / (grav - liftB)));
        float c2 = a * (-k - (impulseB / (grav - liftB))) * (-k - (impulseB / (grav - liftB))) + u2;
        float end2 = ((-b2 - Mathf.Sqrt(b2 * b2 - 4 * a2 * c2)) / (2 * a2));

        float f4(float x)
        {
            return (impulseB * (x - k)) - ((grav - liftB) / 2) * (x - k) * (x - k) + u;
        }
        float f5(float x)
        {
            return a * (x - k - ((impulseB) / (grav - liftB))) * (x - k - ((impulseB) / (grav - liftB))) + u2;
        }

        float f6(float x)
        {
            if (x < xpointB)
            {
                return f4(x);
            }
            else
            {
                return f5(x);
            }
        }

        //stage 1
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

        Vector3 start = transform.position;
        Vector3 curPoint = start;
        Vector3 nextPoint = curPoint;

        Vector3 xForward = transform.rotation * Vector3.right;
        //up is just up

        //Gizmos.DrawLine(curPoint, curPoint);
        if (stages == 3)
        {
            for (int i = 0; i < steps; i++)
            {
                float eval = k * ((i + 1) / (float)steps);

                nextPoint = start + xForward * horizontalVelocity * eval + Vector3.up * f3(eval);

                Gizmos.DrawLine(curPoint, nextPoint);
                curPoint = nextPoint;
            }
        }
        else
        {
            for (int i = 0; i < steps; i++)
            {
                float eval = xpoint * ((i + 1) / (float)steps);

                nextPoint = start + xForward * horizontalVelocity * eval + Vector3.up * f3(eval);

                Gizmos.DrawLine(curPoint, nextPoint);
                curPoint = nextPoint;
            }
        }

        if (stages == 2 && horizontalVelocity != 0)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            for (int i = 0; i < steps; i++)
            {
                float eval = xpoint + (end - xpoint) * ((i + 1) / (float)steps);

                nextPoint = start + xForward * horizontalVelocity * eval + Vector3.up * f3(eval);

                Gizmos.DrawLine(curPoint, nextPoint);
                curPoint = nextPoint;
            }
        }

        if (stages == 2 && horizontalVelocity == 0)
        {
            //use the other jump
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            for (int i = 0; i < steps; i++)
            {
                float eval = xpoint + (xpointB - xpoint) * ((i + 1) / (float)steps);

                nextPoint = start + xForward * horizontalVelocity * eval + Vector3.up * f6(eval);

                Gizmos.DrawLine(curPoint, nextPoint);
                curPoint = nextPoint;
            }
        }

        if (stages == 3)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

            Vector3 diff = xForward * k * horizontalVelocity - xForward * k * horizontalVelocityB;

            for (int i = 0; i < steps; i++)
            {
                float eval = k + (xpointB - k) * ((i + 1) / (float)steps);

                nextPoint = start + diff + xForward * horizontalVelocityB * eval + Vector3.up * f6(eval);

                Gizmos.DrawLine(curPoint, nextPoint);
                curPoint = nextPoint;
            }

            for (int i = 0; i < steps; i++)
            {
                float eval = xpointB + (end2 - xpointB) * ((i + 1) / (float)steps);

                nextPoint = start + diff + xForward * horizontalVelocityB * eval + Vector3.up * f6(eval);

                Gizmos.DrawLine(curPoint, nextPoint);
                curPoint = nextPoint;
            }
        }

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawSphere(curPoint, 0.25f);
    }

    void Start()
    {
        Debug.LogWarning("Jump tool in play mode (remove when done with it)");
        //Destroy(gameObject);
    }
}
