using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBroadcasterScript : WorldZone
{
    public int signalType;
    public ISignalReceiver signalReceiver;

    public override void ProcessTrigger(Collider other)
    {
        if (!other.isTrigger)
        {
            signalReceiver.ReceiveSignal(signalType);
        }
    }
}
