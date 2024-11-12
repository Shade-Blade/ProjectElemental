using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionMap : Pause_SectionShared
{
    public GameObject mapImage;
    public List<MapDotScript> dots;   //to do: make it use a special script to detect the pointer
    public GameObject pointer;

    public MapDotScript selectedDot;

    public TextDisplayer nameBox;

    //Handler should handle this
    //also works in negative direction
    //public Vector3 maxMapDelta; //120,280
    //public Vector3 maxPointerDelta; //400,400

    //ehh I'll just use localposition for this
    //public Vector3 mapDelta;
    //public Vector3 pointerDelta;

    public override void ApplyUpdate(object state)
    {
        //Input is a position
        Pause_HandlerMap.UpdateObject uo = (Pause_HandlerMap.UpdateObject)state;

        pointer.transform.localPosition = uo.position;

        ApplyBounds();

        selectedDot = dots.Find((e) => (uo.location == e.worldLocation));

        if (selectedDot != null)
        {
            nameBox.SetText(MainManager.GetAreaName(selectedDot.worldLocation), true, true);
            textbox.SetText(MainManager.GetAreaDesc(selectedDot.worldLocation), true, true);
        }
        else
        {
            nameBox.SetText("", true, true);
            textbox.SetText("", true, true);
        }
    }

    public void ApplyBounds()
    {
        //270, 110
        Vector3 delta = pointer.transform.localPosition + mapImage.transform.localPosition;
        //Move map image such that the max pointer delta is 270, 110
        if (delta.x > 265)
        {
            mapImage.transform.localPosition = mapImage.transform.localPosition - Vector3.right * (delta.x - 265);
        }
        if (delta.x < -265)
        {
            mapImage.transform.localPosition = mapImage.transform.localPosition - Vector3.right * (delta.x + 265);
        }
        if (delta.y > 115)
        {
            mapImage.transform.localPosition = mapImage.transform.localPosition - Vector3.up * (delta.y - 115);
        }
        if (delta.y < -115)
        {
            mapImage.transform.localPosition = mapImage.transform.localPosition - Vector3.up * (delta.y + 115);
        }

        //constrain map delta
        if (mapImage.transform.localPosition.x > 115)
        {
            mapImage.transform.localPosition = mapImage.transform.localPosition - Vector3.right * mapImage.transform.localPosition.x + Vector3.right * 115;
        }
        if (mapImage.transform.localPosition.x < -115)
        {
            mapImage.transform.localPosition = mapImage.transform.localPosition - Vector3.right * mapImage.transform.localPosition.x + Vector3.left * 115;
        }
        if (mapImage.transform.localPosition.y > 275)
        {
            mapImage.transform.localPosition = mapImage.transform.localPosition - Vector3.up * mapImage.transform.localPosition.y + Vector3.up * 275;
        }
        if (mapImage.transform.localPosition.y < -275)
        {
            mapImage.transform.localPosition = mapImage.transform.localPosition - Vector3.up * mapImage.transform.localPosition.y + Vector3.down * 275;
        }
    }

    public List<MapDotScript> GetDots()
    {
        return dots;
    }

    public override object GetState()
    {
        return new Pause_HandlerMap.UpdateObject(pointer.transform.localPosition, selectedDot == null ? MainManager.WorldLocation.None : selectedDot.worldLocation);
    }

    public override void Init()
    {
        //move it to the current worldlocation
        MainManager.WorldLocation curLocation = MainManager.WorldLocation.None;
        Enum.TryParse(MainManager.Instance.mapScript.worldLocation, out curLocation);

        //move pointer to right place
        selectedDot = dots.Find((e) => (curLocation == e.worldLocation));

        //TO DO: enable or disable dots based on story progress

        if (selectedDot != null)
        {
            if (!selectedDot.isActiveAndEnabled && selectedDot.worldLocation == MainManager.WorldLocation.None)
            {
                nameBox.SetText("", true, true);
                textbox.SetText("", true, true);
                pointer.transform.localPosition = selectedDot.transform.localPosition;
            }
            else
            {
                nameBox.SetText(MainManager.GetAreaName(selectedDot.worldLocation), true, true);
                textbox.SetText(MainManager.GetAreaDesc(selectedDot.worldLocation), true, true);
                pointer.transform.localPosition = selectedDot.transform.localPosition;
            }
        }
        else
        {
            textbox.SetText("", true, true);
        }

        //Center pointer on map
        mapImage.transform.localPosition = -pointer.transform.localPosition;
        ApplyBounds();

        base.Init();
    }
}
