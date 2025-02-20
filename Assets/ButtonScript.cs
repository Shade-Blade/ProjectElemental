using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;

public class ButtonScript : WorldObject, ISignalReceiver
{
    public HitBroadcasterScript hbs;
    public ISignalReceiver target;
    public GameObject targetGO;
    public int targetSignal;

    public bool buttonActive;

    public float buttonChangeDuration = 0.2f;

    public GameObject buttonMiddle;

    public MeshRenderer bottomLight;
    public Material materialLight_Inactive;
    public Material materialLight_Active;


    public bool moving;

    public string globalFlag = "";
    public string areaFlag = "";

    // Start is called before the first frame update
    void Start()
    {
        hbs.signalReceiver = this;

        if (targetGO != null)
        {
            target = targetGO.GetComponent<ISignalReceiver>();
        }

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
        }
        else
        {
            InstantDeactivate();
        }
    }

    public void ReceiveSignal(int signal)
    {
        if (signal == 0)
        {
            DeactivateButton();
        } else
        {
            //hit
            ActivateButton();
        }
    }

    public void ActivateButton()
    {
        if (moving)
        {
            return;
        }

        if (!buttonActive)
        {
            StartCoroutine(ActivateButtonCoroutine());
        }
        buttonActive = true;

        if (globalFlag.Length > 0)
        {
            MainManager.Instance.SetGlobalFlag(globalFlag, true);
        }
        else if (areaFlag.Length > 0)
        {
            MainManager.Instance.SetAreaFlag(areaFlag, true);
        }

        if (target != null)
        {
            target.ReceiveSignal(targetSignal);
        }
    }
    public void DeactivateButton()
    {
        if (buttonActive)
        {
            StartCoroutine(DeactivateButtonCoroutine());
        }
        buttonActive = false;

        if (globalFlag.Length > 0)
        {
            MainManager.Instance.SetGlobalFlag(globalFlag, false);
        }
        else if (areaFlag.Length > 0)
        {
            MainManager.Instance.SetAreaFlag(areaFlag, false);
        }
    }
    public void InstantActivate()
    {
        buttonMiddle.transform.localPosition = Vector3.down * 0.15f;
        bottomLight.material = materialLight_Active;
    }
    public void InstantDeactivate()
    {
        buttonMiddle.transform.localPosition = Vector3.zero;
        bottomLight.material = materialLight_Inactive;
    }

    public IEnumerator ActivateButtonCoroutine()
    {
        while (moving)
        {
            yield return null;
        }

        moving = true;

        float time = 0;
        while (time < buttonChangeDuration)
        {
            buttonMiddle.transform.localPosition = Vector3.down * 0.15f * (time / buttonChangeDuration);

            time += Time.deltaTime;
            yield return null;
        }
        buttonMiddle.transform.localPosition = Vector3.down * 0.15f;

        bottomLight.material = materialLight_Active;
        moving = false;
    }

    public IEnumerator DeactivateButtonCoroutine()
    {
        while (moving)
        {
            yield return null;
        }

        moving = true;
        float time = 0;
        while (time < buttonChangeDuration)
        {
            buttonMiddle.transform.localPosition = Vector3.down * 0.15f * (1 - (time / buttonChangeDuration));

            time += Time.deltaTime;
            yield return null;
        }
        buttonMiddle.transform.localPosition = Vector3.zero;

        bottomLight.material = materialLight_Inactive;
        moving = false;
    }
}
