using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script for all the sprites that only have an idle and back sprite
//Debug
public class AnimationController_Stub : AnimationController_BackSprite
{
    public Sprite spriteIdle;
    public Sprite spriteIdleBack;

    public override void SetAnimation(string name, bool force = false)
    {
        name = "idle";

        string modifiedName = name;
        //Hardcode talking animations not having back sprites (Note: also need to force this for special cases as well)
        if (showBack && !name.Contains("talk") && !name.Contains("hurt"))
        {
            modifiedName = name + "_back";
        }

        if (modifiedName.Equals("idle"))
        {
            sprite.sprite = spriteIdle;
        } else
        {
            sprite.sprite = spriteIdleBack;
        }
    }
}
