using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PressurePlateScript : WorldObject, ISignalReceiver
{
    public TriggerBroadcasterScript tbs;
    public ISignalReceiver target;
    public GameObject targetGO;
    public int targetSignalActive;
    public int targetSignalInactive;

    public bool buttonActive;
    public float buttonActiveLevel;

    public float buttonChangeDuration = 0.1f;

    public GameObject buttonMiddle;

    public MeshRenderer buttonMiddleMesh;
    public MeshRenderer bottomLight;
    public Material materialLight_Inactive;
    public Material materialLight_Active;

    public bool receivedSignalLastFixedUpdate;
    public bool pastRSLFU;

    public void Start()
    {
        tbs.signalReceiver = this;

        if (targetGO != null)
        {
            target = targetGO.GetComponent<ISignalReceiver>();
        }
        forceKinematic = true;
    }


    public override void WorldFixedUpdate()
    {
        bool lastActive = buttonActiveLevel > 0.5f;
        float pastActiveLevel = buttonActiveLevel;

        //to prevent "blips" (?)
        //pressure plate only moves if the plate is active or inactive for 2 frames in a row
        //Oscillating between yes and no is pretty rare and probably doesn't matter that the plate becomes half activated
        //  (But that is kind of an expected outcome)
        if (pastRSLFU == receivedSignalLastFixedUpdate)
        {
            if (receivedSignalLastFixedUpdate)
            {
                buttonActiveLevel += Time.fixedDeltaTime / buttonChangeDuration;
            }
            else
            {
                buttonActiveLevel -= Time.fixedDeltaTime / buttonChangeDuration;
            }
        }
        buttonActiveLevel = Mathf.Clamp01(buttonActiveLevel);

        buttonActive = buttonActiveLevel > 0.5f;
        if (lastActive != buttonActive)
        {
            if (target != null)
            {
                target.ReceiveSignal(buttonActive ? targetSignalActive : targetSignalInactive);
            }

            bottomLight.material = buttonActive ? materialLight_Active : materialLight_Inactive;
            buttonMiddleMesh.material = buttonActive ? materialLight_Active : materialLight_Inactive;
        }

        float delta = 0.04f * ((buttonActiveLevel) - (pastActiveLevel));
        rb.velocity = Vector3.down * (delta / Time.fixedDeltaTime);
        rb.MovePosition(transform.position + Vector3.down * 0.04f * (buttonActiveLevel));

        pastRSLFU = receivedSignalLastFixedUpdate;
        receivedSignalLastFixedUpdate = false;
    }

    public void ReceiveSignal(int signal)
    {
        receivedSignalLastFixedUpdate = true;
    }
}
