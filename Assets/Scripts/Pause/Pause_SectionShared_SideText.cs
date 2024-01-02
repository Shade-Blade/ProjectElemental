using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause_SectionShared_SideText : Pause_SectionShared
{
    public Image image;
    public TextDisplayer description;
    public string descriptionText;

    public override void ApplyUpdate(object state)
    {
        InformationMenuEntry ime = (InformationMenuEntry)state;

        if (ime == null || ime.sprite == null)
        {
            if (ime == null || ime.spritePath == null || ime.spritePath.Equals(" "))
            {
                image.sprite = MainManager.Instance.defaultSprite;
            } else
            {
                //try loading it
                image.sprite = Resources.Load<Sprite>(ime.spritePath);
            }
        } else
        {
            image.sprite = ime.sprite;
        }

        if (ime == null)
        {
            descriptionText = "";
        } else
        {
            descriptionText = ime.sideText;
        }
        description.SetText(descriptionText, true, true);
    }

    public override object GetState()
    {
        return new InformationMenuEntry(image.sprite, null, descriptionText, null);
    }
}
