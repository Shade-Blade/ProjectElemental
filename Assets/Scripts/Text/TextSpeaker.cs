using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//holder interface for textbox speaker information
//this is versatile so I can make stuff like signs without being stuck with gluing it to an entity script or something
public interface ITextSpeaker
{
    //text can send or receive data through special tags
    //These are used for more modular text stuff that should be handled on the side of the entity and not just be hardcoded
    public string RequestTextData(string request);
    public void SendTextData(string data);

    public void EnableSpeakingAnim();
    public bool SpeakingAnimActive();
    public void DisableSpeakingAnim();

    //Push stuff to the animcontroller
    public void SetAnimation(string animationID, bool force = false, float time = -1);
    public void SendAnimationData(string data);

    public Vector3 GetTextTailPosition();

    public void TextBleep();

    public void SetFacing(Vector3 facingTarget);
    public void EmoteEffect(TagEntry.Emote emote);
}
