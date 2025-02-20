using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverScript : WorldObject, ISignalReceiver
{
    public HitBroadcasterScript hbs;
    public ISignalReceiver target;
    public GameObject targetGO;
    public int targetSignalActive;
    public int targetSignalInactive;

    public bool leverActive;

    public float leverChangeDuration = 0.2f;

    public float leverCooldown;
    public float leverCooldownDuration = 0.5f;

    public GameObject leverMiddle;

    public MeshRenderer bottomLight;
    public MeshRenderer leverLight;
    public Material materialLight_Inactive;
    public Material materialLight_Active;


    public string globalFlag = "";
    public string areaFlag = "";

    public bool moving;

    // Start is called before the first frame update
    void Start()
    {
        if (targetGO != null)
        {
            target = targetGO.GetComponent<ISignalReceiver>();
        }

        hbs.signalReceiver = this;

        bool activate = false;
        if (globalFlag.Length > 0)
        {
            activate = MainManager.Instance.GetGlobalFlag(globalFlag);
        }
        else if (areaFlag.Length > 0)
        {
            activate = MainManager.Instance.GetAreaFlag(areaFlag);
        }
        if (activate)
        {
            InstantActivate();
        } else
        {
            InstantDeactivate();
        }
    }

    public override void WorldUpdate()
    {
        if (leverCooldown > 0)
        {
            leverCooldown -= Time.deltaTime;
        }
    }

    public void ReceiveSignal(int signal)
    {
        if (signal == -1)
        {
            DeactivateLever();
        }
        if (signal == 2)
        {
            ActivateLever();
        }

        if (moving)
        {
            return;
        }

        if (leverActive)
        {
            DeactivateLever();
        }
        else
        {
            //hit
            ActivateLever();
        }
    }

    public void ActivateLever()
    {
        if (leverCooldown > 0)
        {
            return;
        }

        if (!leverActive)
        {
            StartCoroutine(ActivateLeverCoroutine());
        }
        leverActive = true;

        if (globalFlag.Length > 0)
        {
            MainManager.Instance.SetGlobalFlag(globalFlag, true);
        }
        else if (areaFlag.Length > 0)
        {
            MainManager.Instance.SetAreaFlag(areaFlag, true);
        }
        leverCooldown = leverCooldownDuration;

        if (target != null)
        {
            target.ReceiveSignal(targetSignalActive);
        }
    }
    public void DeactivateLever()
    {
        if (leverCooldown > 0)
        {
            return;
        }

        if (leverActive)
        {
            StartCoroutine(DeactivateLeverCoroutine());
        }
        leverActive = false;

        if (globalFlag.Length > 0)
        {
            MainManager.Instance.SetGlobalFlag(globalFlag, false);
        }
        else if (areaFlag.Length > 0)
        {
            MainManager.Instance.SetAreaFlag(areaFlag, false);
        }
        leverCooldown = leverCooldownDuration;

        if (target != null)
        {
            target.ReceiveSignal(targetSignalInactive);
        }
    }
    public void InstantActivate()
    {
        leverMiddle.transform.localEulerAngles = Vector3.right * 30;
        bottomLight.material = materialLight_Active;
        leverLight.material = materialLight_Active;
    }
    public void InstantDeactivate()
    {
        leverMiddle.transform.localEulerAngles = Vector3.right * -30;
        bottomLight.material = materialLight_Inactive;
        leverLight.material = materialLight_Inactive;
    }

    public IEnumerator ActivateLeverCoroutine()
    {
        while (moving)
        {
            yield return null;
        }
        moving = true;

        float time = 0;
        while (time < leverChangeDuration)
        {
            leverMiddle.transform.localEulerAngles = Vector3.Lerp(Vector3.right * -30, Vector3.right * 30, (time / leverChangeDuration));

            time += Time.deltaTime;
            yield return null;
        }
        leverMiddle.transform.localEulerAngles = Vector3.right * 30;

        bottomLight.material = materialLight_Active;
        leverLight.material = materialLight_Active;

        moving = false;
    }

    public IEnumerator DeactivateLeverCoroutine()
    {
        while (moving)
        {
            yield return null;
        }
        moving = true;

        float time = 0;
        while (time < leverChangeDuration)
        {
            leverMiddle.transform.localEulerAngles = Vector3.Lerp(Vector3.right * 30, Vector3.right * -30, (time / leverChangeDuration));

            time += Time.deltaTime;
            yield return null;
        }
        leverMiddle.transform.localEulerAngles = Vector3.right * -30;

        bottomLight.material = materialLight_Inactive;
        leverLight.material = materialLight_Inactive;

        moving = false;
    }
}
