using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverTextMasterScript : MonoBehaviour
{
    public static HoverTextMasterScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<HoverTextMasterScript>(); //this should work
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    private static HoverTextMasterScript instance;

    public GameObject hoverPopupPrototype;

    public HoverPopupScript hoverPopup;


    public string hoverText;
    public string pastHoverText;

    //process: script sets hover text every frame
    public void Update()
    {
        if (hoverText == null)
        {
            if (hoverPopup != null)
            {
                Destroy(hoverPopup.gameObject);
            }
        } else
        {
            if (!hoverText.Equals(pastHoverText))
            {
                //Hover popup must be updated
                if (hoverPopup == null)
                {
                    MakeHoverPopup(hoverText);
                } else
                {
                    hoverPopup.SetText(hoverText, true, true);
                }
            }
        }

        pastHoverText = hoverText;
        hoverText = null;
    }

    public void MakeHoverPopup(string s)
    {
        GameObject o = Instantiate(hoverPopupPrototype, MainManager.Instance.Canvas.transform);
        HoverPopupScript hps = o.GetComponent<HoverPopupScript>();
        hps.SetText(s, true, true);
        hps.PositionUpdate();

        hoverPopup = hps;
    }

    public void SetHoverText(string p_hoverText)
    {
        hoverText = p_hoverText;
    }
}
