using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISignalReceiver
{
    public void ReceiveSignal(int signal);
}

public class HitBroadcasterScript : WorldObject, IDashHopTrigger, ISlashTrigger, ISmashTrigger, IStompTrigger, IHeadHitTrigger
{
    public int signalType;
    public ISignalReceiver signalReceiver;

    public void Bonk(Vector3 kickpos, Vector3 kicknormal)
    {
        Hit();
    }

    public bool Slash(Vector3 slashvector, Vector3 playerpos)
    {
        Hit();
        return true;
    }

    public bool Smash(Vector3 smashvector, Vector3 playerpos)
    {
        Hit();
        return true;
    }

    public void Stomp(WorldPlayer.StompType stompType)
    {
        Hit();
    }

    public void HeadHit(WorldPlayer.StompType stompType)
    {
        Hit();
    }

    public void Hit()
    {
        signalReceiver.ReceiveSignal(signalType);
    }
}
