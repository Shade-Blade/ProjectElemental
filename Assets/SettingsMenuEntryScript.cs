using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuEntryScript : BoxMenuEntryScript
{
    public Image leftArrow;
    public Image rightArrow;

    public Image line;

    public override void Setup(BoxMenuEntry p_entry, bool showAtOne, string customLevel, string colorStringA, string colorStringB)
    {
        if (p_entry is PlaceholderMenuEntry)
        {
            line.gameObject.SetActive(true);
            maintext.SetText("", true, true);
            righttext.SetText("", true, true);
            background.gameObject.SetActive(false);
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            return;
        }
        line.gameObject.SetActive(false);

        entry = p_entry;
        //maintext.text = entry.name;
        //righttext.text = entry.rightText + entry.spriteString;

        string mainString = entry.name;
        string rightString = entry.rightText + entry.spriteString;


        if (entry.canUse)
        {
            //maintext.color = enabledColor;
            //righttext.color = enabledColor;

            if (colorStringA != null)
            {
                mainString = "<color," + colorStringA + ">" + mainString + "</color>";
                rightString = "<color," + colorStringA + ">" + rightString + "</color>";
            }
            else
            {

            }
        }
        else
        {
            //maintext.color = disabledColor;
            //righttext.color = disabledColor;

            if (colorStringB != null)
            {
                mainString = "<color," + colorStringB + ">" + mainString + "</color>";
                rightString = "<color," + colorStringB + ">" + rightString + "</color>";
            }
            else
            {
                mainString = "<color,red>" + mainString + "</color>";
                rightString = "<color,red>" + rightString + "</color>";
            }
        }


        maintext.SetText(mainString, true, true);
        righttext.SetText(rightString, true, true);
        background.gameObject.SetActive(false);

        if (entry.level > 0)
        {
            leftArrow.gameObject.SetActive(true);
        } else
        {
            leftArrow.gameObject.SetActive(false);
        }
        if (entry.level < entry.maxLevel)
        {
            rightArrow.gameObject.SetActive(true);
        } else
        {
            rightArrow.gameObject.SetActive(false);
        }
    }


    public override void Setup(BoxMenuEntry p_entry, bool showAtOne = true, string customLevel = null)
    {
        if (p_entry is PlaceholderMenuEntry)
        {
            line.gameObject.SetActive(true);
            maintext.SetText("", true, true);
            righttext.SetText("", true, true);
            background.gameObject.SetActive(false);
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            return;
        }
        line.gameObject.SetActive(false);

        entry = p_entry;

        if (entry == null)
        {
            maintext.SetText("", true, true);
            righttext.SetText("", true, true);
            background.gameObject.SetActive(false);
            background.color = Color.white;
            return;
        }

        //maintext.text = entry.name;
        //righttext.text = entry.rightText + entry.spriteString;

        string mainString = entry.name;
        string rightString = entry.rightText + entry.spriteString;

        if (entry.canUse)
        {
            //maintext.color = enabledColor;
            //righttext.color = enabledColor;
        }
        else
        {
            //maintext.color = disabledColor;
            //righttext.color = disabledColor;

            mainString = "<color,red>" + mainString + "</color>";
            rightString = "<color,red>" + rightString + "</color>";
        }


        maintext.SetText(mainString, true, true);
        righttext.SetText(rightString, true, true);
        background.gameObject.SetActive(p_entry.hasBackground);
        background.color = p_entry.backgroundColor;

        if (entry.level > 0)
        {
            leftArrow.gameObject.SetActive(true);
        }
        else
        {
            leftArrow.gameObject.SetActive(false);
        }
        if (entry.level < entry.maxLevel)
        {
            rightArrow.gameObject.SetActive(true);
        }
        else
        {
            rightArrow.gameObject.SetActive(false);
        }
    }
}
