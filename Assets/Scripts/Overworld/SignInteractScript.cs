using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignInteractScript : InteractTrigger
{
    public SignScript ss;

    public override void Interact()
    {
        StartCoroutine(MainManager.Instance.ExecuteCutscene(ss.SignCutscene()));
    }
}
