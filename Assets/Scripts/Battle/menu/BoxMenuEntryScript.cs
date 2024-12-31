using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxMenuEntryScript : MonoBehaviour
{
    //public TMPro.TMP_Text maintext;
    //public TMPro.TMP_Text righttext;

    public TextDisplayer maintext;
    public TextDisplayer righttext;
    public Image background;

    public BoxMenuEntry entry;

    //public static Color enabledColor = new Color(0, 0, 0);
    //public static Color disabledColor = new Color(1f, 0, 0);

    public virtual void Setup(BoxMenuEntry p_entry, bool showAtOne, string customLevel, string colorStringA, string colorStringB)
    {
        entry = p_entry;
        //maintext.text = entry.name;
        //righttext.text = entry.rightText + entry.spriteString;

        string mainString = entry.name;
        string rightString = entry.rightText + entry.spriteString;

        if (entry.maxLevel > 1)
        {
            if (showAtOne || entry.level > 1)
            {
                if (customLevel != null)
                {
                    mainString = entry.name + " " + customLevel + entry.level;
                }
                else
                {
                    mainString = entry.name + " Lv." + entry.level;
                }
            }
            //maintext.text = entry.name + " Lv." + entry.level;
        }

        if (entry.canUse)
        {
            //maintext.color = enabledColor;
            //righttext.color = enabledColor;

            if (colorStringA != null)
            {
                mainString = "<color," + colorStringA + ">" + mainString + "</color>";
                rightString = "<color," + colorStringA + ">" + rightString + "</color>";
            } else
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
    }


    public virtual void Setup(BoxMenuEntry p_entry, bool showAtOne = true, string customLevel = null)
    {
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

        if (entry.maxLevel > 1)
        {
            if (showAtOne || entry.level > 1)
            {
                if (customLevel != null)
                {
                    if (customLevel.Length == 0)
                    {
                        mainString = entry.name;
                    }
                    else
                    {
                        mainString = entry.name + " " + customLevel + entry.level;
                    }
                }
                else
                {
                    mainString = entry.name + " Lv." + entry.level;
                }
            }
            //maintext.text = entry.name + " Lv." + entry.level;
        }

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
    }

    public void Setup(string p_maintext, string p_pricetext = null, string p_spriteString = null)
    {
        entry = new BoxMenuEntry(p_maintext, p_pricetext, p_spriteString);
        //maintext.text = p_maintext;
        //righttext.text = p_pricetext + p_spriteString;
        maintext.SetText(p_maintext, true, true);
        righttext.SetText(p_pricetext, true, true);
        background.gameObject.SetActive(false);
    }
}
