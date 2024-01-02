using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingLauncherScript : WorldObject
{
    Vector3 startPos;
    Quaternion startRot;

    public bool scripted;
    public float momentumStrength;
    public bool multiDirection;
    public Vector3 launchVelocity;
    public float heightThreshold;
    public Vector3 altLaunchVelocity; // if last grounded height is higher than the height threshold, do alt launch instead

    public float timeSinceContact;
    public float timeToFall;

    private void OnDrawGizmosSelected()
    {
        if (heightThreshold > float.MinValue)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position - transform.position.y * Vector3.up + Vector3.up * heightThreshold, 0.1f);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        if (rb == null)
        {
            rb = transform.parent.GetComponentInChildren<Rigidbody>();
        }

        timeSinceContact = -1;
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
                    if (multiDirection)
                    {
                        float vel = (launchVel - launchVel.y * Vector3.up).magnitude;
                        Vector3 oldDir = w.rb.velocity - w.rb.velocity.y * Vector3.up;
                        Vector3 newDir = oldDir.normalized * vel + launchVel.y * Vector3.up;
                        launchVel = newDir;
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
                    w.Launch(launchVel, momentumStrength);
                }
            }
        }

        WorldEntity wp = collision.collider.transform.GetComponent<WorldPlayer>();
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (wp != null && timeSinceContact < 0)
        {
            timeSinceContact = 0;
        }
    }

    public override void WorldUpdate()
    {
        if (WorldPlayer.Instance != null)
        {
            if (WorldPlayer.Instance.groundedTime > 0.25f && !WorldPlayer.Instance.Unstable())
            {
                ResetPosition();
            }

            if (timeSinceContact >= 0)
            {
                timeSinceContact += Time.deltaTime;
            }
            else
            {
                rb.transform.position = MainManager.EasingQuadraticTime(transform.position, startPos, 12f);
                rb.transform.rotation = MainManager.EasingQuadraticTime(transform.rotation, startRot, 4f);
            }

            if (timeSinceContact > timeToFall)
            {
                rb.constraints = RigidbodyConstraints.None;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }

    public void ResetPosition()
    {
        /*
        rb.MovePosition(startPos);
        rb.MoveRotation(startRot);
        rb.transform.position = startPos;
        rb.transform.rotation = startRot;
        */
        //transform.position = startPos;
        //transform.rotation = startRot;

        timeSinceContact = -1;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}

