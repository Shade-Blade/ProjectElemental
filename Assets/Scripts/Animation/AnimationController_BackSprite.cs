using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController_BackSprite : AnimationController
{
    public bool showBack;

    public override void SetAnimation(string name, bool force = false, float time = -1)
    {
        timeSinceLastAnimChange = 0;
        string modifiedName = name;

        //Hardcode talking animations not having back sprites (Note: also need to force this for special cases as well)
        if (showBack && !name.Contains("talk") && !name.Contains("hurt"))
        {
            modifiedName = name + "_back";
        }


        //bool update = (!currentAnim.Equals(modifiedName));
        //better way
        bool update = !(animator.GetCurrentAnimatorStateInfo(0).IsName(modifiedName)) || force;
        currentAnim = modifiedName;

        //fixes a problem I'm having somehow
        //update = true;
        //Debug.Log("Play " + currentAnim + " " + update);

        //Debug.Log(this + " " + currentAnim);
        if (animator != null && update)
        {
            //Debug.Log(this.name + ": Play " + name);
            if (force)
            {
                animator.Play(modifiedName, -1, 0);
            }
            else
            {
                if (time != -1)
                {
                    animator.Play(modifiedName, -1, time);
                }
                else
                {
                    animator.Play(modifiedName);
                }
            }
        }
    }
    public override void ReplaceAnimation(string name = null, bool force = false)
    {
        if (name == null)
        {
            string newanim = currentAnim.Replace("_back", "");
            SetAnimation(newanim, force, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            return;
        }

        SetAnimation(name, force, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    public override void SendAnimationData(string data)
    {
        //Debug.Log(data);
        if (data.Equals("xflip"))
        {
            sprite.flipX = true ^ leftFacing;
            //subobject.transform.localEulerAngles = Vector3.up * 180;
        }
        if (data.Equals("xunflip"))
        {
            sprite.flipX = false ^ leftFacing;
            //subobject.transform.localEulerAngles = Vector3.up * 0;
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