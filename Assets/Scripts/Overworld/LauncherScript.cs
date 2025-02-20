using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LauncherScript : WorldObject
{
    public bool scripted;
    public float momentumStrength;
    public bool multiDirection;
    public Vector3 launchVelocity;
    public float heightThreshold;
    public Vector3 altLaunchVelocity; // if last grounded height is higher than the height threshold, do alt launch instead

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);

        if (multiDirection)
        {
            void Draw(Vector3 hdir)
            {
                float vel = (launchVelocity - launchVelocity.y * Vector3.up).magnitude;
                Vector3 newDir = hdir.normalized * vel + launchVelocity.y * Vector3.up;
                float velB = (altLaunchVelocity - altLaunchVelocity.y * Vector3.up).magnitude;
                Vector3 newDirB = hdir.normalized * vel + altLaunchVelocity.y * Vector3.up;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + newDir.normalized * 1.5f);
                Gizmos.color = new Color(0.6f, 0.6f, 0.2f);
                Gizmos.DrawLine(transform.position + Vector3.left * 0.1f, transform.position + Vector3.left * 0.1f + newDirB.normalized);
                Gizmos.DrawLine(transform.position + Vector3.right * 0.1f, transform.position + Vector3.right * 0.1f + newDirB.normalized);
                Gizmos.DrawLine(transform.position + Vector3.left * 0.1f + newDirB.normalized, transform.position + Vector3.right * 0.1f + newDirB.normalized);
            }
            Draw(Vector3.right);
            Draw(Vector3.left);
            Draw(Vector3.forward);
            Draw(Vector3.back);
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + launchVelocity.normalized * 1.5f);
            Gizmos.color = new Color(0.6f, 0.6f, 0.2f);
            Gizmos.DrawLine(transform.position + Vector3.left * 0.1f, transform.position + Vector3.left * 0.1f + altLaunchVelocity.normalized);
            Gizmos.DrawLine(transform.position + Vector3.right * 0.1f, transform.position + Vector3.right * 0.1f + altLaunchVelocity.normalized);
            Gizmos.DrawLine(transform.position + Vector3.left * 0.1f + altLaunchVelocity.normalized, transform.position + Vector3.right * 0.1f + altLaunchVelocity.normalized);
            Gizmos.color = Color.yellow;
        }

        if (heightThreshold > float.MinValue)
        {
            Gizmos.DrawSphere(transform.position - transform.position.y * Vector3.up + Vector3.up * heightThreshold, 0.1f);
        }
    }
    public override void ProcessCollision(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            if (collision.transform.GetComponent<WorldEntity>() != null)
            {
                WorldEntity w = collision.transform.GetComponent<WorldEntity>();
                Vector3 launchVel;
                if (w.lastGroundedHeight > heightThreshold)
                {
                    launchVel = altLaunchVelocity;
                }
                else
                {
                    launchVel = launchVelocity;
                }

                if (scripted)
                {
                    if (w is WorldPlayer wp)
                    {
                        if (multiDirection)
                        {
                            float vel = (launchVel - launchVel.y * Vector3.up).magnitude;
                            Vector3 oldDir = w.rb.velocity - w.rb.velocity.y * Vector3.up;
                            Vector3 newDir = oldDir.normalized * vel + launchVel.y * Vector3.up;
                            launchVel = newDir;
                        }

                        foreach (WorldFollower wf in wp.followers)
                        {
                            wp.FollowerWarp(wp.FacingVector() * 0.075f);
                            wf.ScriptedLaunch(launchVel, momentumStrength);
                        }
                        w.ScriptedLaunch(launchVel, momentumStrength);
                    }
                    else
                    {
                        if (multiDirection)
                        {
                            float vel = (launchVel - launchVel.y * Vector3.up).magnitude;
                            Vector3 oldDir = w.rb.velocity - w.rb.velocity.y * Vector3.up;
                            Vector3 newDir = oldDir.normalized * vel + launchVel.y * Vector3.up;
                            launchVel = newDir;
                        }
                        w.ScriptedLaunch(launchVel, momentumStrength);
                    }
                }
                else
                {
                    if (multiDirection)
                    {
                        float vel = (launchVel - launchVel.y * Vector3.up).magnitude;
                        Vector3 oldDir = w.rb.velocity - w.rb.velocity.y * Vector3.up;
                        Vector3 newDir = oldDir.normalized * vel + launchVel.y * Vector3.up;
                        launchVel = newDir;
                    }
                    w.Launch(launchVel, momentumStrength);
                }
            }
        }
    }
}
