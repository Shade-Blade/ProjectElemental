using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformScript : WorldObject
{
    Vector3 startPos;
    Quaternion startRot;

    public float timeSinceContact;
    public float timeToFall;

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
            } else
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
