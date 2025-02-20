using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonkBox : MonoBehaviour, IDashHopTrigger, ISlashTrigger, ISmashTrigger, IStompTrigger, IHeadHitTrigger
{
    public Rigidbody rb;

    public float power = 4;

    public void Bonk(Vector3 kickpos, Vector3 kicknormal)
    {
        //Debug.Log("Bonk");
        //rb.AddForce(-8 * (kicknormal + Vector3.down * 0.2f), ForceMode.Impulse);
        rb.velocity = power * (-kicknormal + Vector3.up * 0.5f);
    }

    public bool Slash(Vector3 slashvector, Vector3 playerpos)
    {
        rb.velocity = power * (slashvector + Vector3.up * 0.5f);
        return true;
    }

    public bool Smash(Vector3 smashvector, Vector3 playerpos)
    {
        rb.velocity = power * (smashvector + Vector3.up * 0.5f);
        return true;
    }

    public void Stomp(WorldPlayer.StompType stompType)
    {
        rb.velocity = power * (Vector3.down * 1f);
    }

    public void HeadHit(WorldPlayer.StompType stompType)
    {
        rb.velocity = power * (Vector3.up * 1f);
    }
}
