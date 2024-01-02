using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionItem_Character : Pause_SectionShared
{
    public GameObject subobject;
    public override void ApplyUpdate(object state)
    {
        return;
    }
    public override object GetState()
    {
        return null;
    }


    public override void Init()
    {
        subobject.SetActive(true);
        base.Init();
    }

    public override void Clear()
    {
        subobject.SetActive(false);
        base.Clear();
    }
}
