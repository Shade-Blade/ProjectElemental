using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//holder script for all the parts of the box menu
//I should probably merge this with the BoxMenu script since this doesn't actually do anything
public class BoxMenuScript : MonoBehaviour
{
    public Image upArrow;
    public Image downArrow;
    public Image selectorArrow;
    public Image menuBox;
    public RectMask2D mask;

    public Image descriptorBox;
    public TextDisplayer descriptorTextBox;

    public GameObject levelChangeIndicator;

    public void SetLevelChangeIndicator(bool isUpgradeable)
    {
        levelChangeIndicator.SetActive(isUpgradeable);
        selectorArrow.color = isUpgradeable ? new Color(1, 0.9f, 0) : Color.blue;
    }

    public static Vector3 GetRelativePosition(float relIndex)
    {
        //for page size = 6
        //  0 has displacement 75
        //  5 has displacement -75
        //  +75 - 30 * ri

        //for page size = N
        //  0 has displacement 75
        //  N - 1 has displacement -75
        //  +75 - (150/(N-1)) * ri

        //don't remember exactly why I did this (probably to hide all the offscreen menu options?)
        //No longer used because I changed the sprite mask to cut off stuff closer (so I could make them scroll fluidly)
        /*
        if (relIndex < 0)
        {
            return Vector3.up * 75 + Vector3.down * (150/(BoxMenu.MENU_SIZE_PER_PAGE - 1)) * (relIndex - 1);
        } else if (relIndex > BoxMenu.MENU_SIZE_PER_PAGE - 1)
        {
            return Vector3.up * 75 + Vector3.down * (150 / (BoxMenu.MENU_SIZE_PER_PAGE - 1)) * (relIndex + 1);
        }
        */

        return Vector3.up * 70 + Vector3.down * (150 / (BoxMenu.MENU_SIZE_PER_PAGE - 1)) * relIndex;
    }
}
