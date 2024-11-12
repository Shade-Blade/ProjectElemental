using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitObstacleBoxScript : MonoBehaviour, IStompTrigger, IDashHopTrigger, ISlashTrigger, ISmashTrigger, IHeadHitTrigger, IUndigTrigger
{
    public PitObstacleScript pos;

    public void Bonk(Vector3 kickpos, Vector3 kicknormal)
    {
        pos.Bonk(kickpos, kicknormal);
    }

    public void HeadHit(WorldPlayer.StompType stompType)
    {
        pos.HeadHit(stompType);
    }

    public bool Slash(Vector3 slashvector, Vector3 playerpos)
    {
        return pos.Slash(slashvector, playerpos);
    }

    public bool Smash(Vector3 smashvector, Vector3 playerpos)
    {
        return pos.Smash(smashvector, playerpos);
    }

    public void Stomp(WorldPlayer.StompType stompType)
    {
        pos.Stomp(stompType);
    }

    public void Undig()
    {
        pos.Undig();
    }
}
