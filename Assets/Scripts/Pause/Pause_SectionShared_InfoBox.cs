using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static Pause_HandlerShared_InfoBox;

public class Pause_SectionShared_InfoBox : Pause_SectionShared
{    

    public GameObject leftArrow;
    public GameObject rightArrow;

    public TextDisplayer infoBox;

    public string[] infoText;
    public int index;

    public override void ApplyUpdate(object state)
    {
        UpdateObject uo = (UpdateObject)state;

        gameObject.SetActive(true);
        infoText = uo.infoText;
        index = uo.index;

        if (index == 0)
        {
            leftArrow.SetActive(false);
        } else
        {
            leftArrow.SetActive(true);
        }

        if (index == infoText.Length - 1)
        {
            rightArrow.SetActive(false);
        } else
        {
            rightArrow.SetActive(true);
        }

        infoBox.SetText(infoText[index], true, true);

        return;
    }
    public override object GetState()
    {
        UpdateObject uo = new UpdateObject(index, infoText);

        return uo;
    }
}
