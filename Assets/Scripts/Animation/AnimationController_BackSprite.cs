using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController_BackSprite : AnimationController
{
    public bool showBack;

    public override void SetAnimation(string name)
    {
        string modifiedName = name;

        if (showBack)
        {
            modifiedName = name + "_back";
        }


        bool update = (!currentAnim.Equals(modifiedName));
        currentAnim = modifiedName;

        if (animator != null && update)
        {
            //Debug.Log(this.name + ": Play " + name);
            animator.Play(modifiedName);
        }
    }

    public override void SendAnimationData(string data)
    {
        //Debug.Log(data);
        if (data.Equals("xflip"))
        {
            //sprite.flipX = true;
            subobject.transform.localEulerAngles = Vector3.up * 180;
        }
        if (data.Equals("xunflip"))
        {
            //sprite.flipX = false;
            subobject.transform.localEulerAngles = Vector3.up * 0;
        }

        if (data.Equals("showback"))
        {
            showBack = true;
        }
        if (data.Equals("unshowback"))
        {
            showBack = false;
        }

        if (data.Equals("aetherize"))
        {
            Aetherize();
        }
        if (data.Equals("illuminate"))
        {
            Illuminate();
        }
        if (data.StartsWith("effect"))
        {
            EffectUpdate(data);
        }
        if (data.Equals("matreset"))
        {
            MaterialReset();
        }
    }
}