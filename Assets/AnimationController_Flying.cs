using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController_Flying : AnimationController_BackSprite
{
    public Vector3 offsetFlying;
    public float heightFlying;
    public float widthFlying;
    public Vector3 offsetGrounded;
    public float heightGrounded;
    public float widthGrounded;

    public override void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.75f, 1f, 1f, 0.5f);
        Vector3 down = transform.up * -1 * heightFlying / 2;
        Vector3 left = transform.right * -1 * widthFlying / 2;

        Gizmos.DrawLine(transform.position + left, transform.position - left);
        Gizmos.DrawLine(transform.position - left, transform.position - 2 * down - left);
        Gizmos.DrawLine(transform.position - 2 * down - left, transform.position - 2 * down + left);
        Gizmos.DrawLine(transform.position - 2 * down + left, transform.position + left);

        Gizmos.color = new Color(1f, 0.75f, 0.75f, 0.5f);
        down = transform.up * -1 * heightGrounded / 2;
        left = transform.right * -1 * widthGrounded / 2;

        Gizmos.DrawLine(transform.position + left, transform.position - left);
        Gizmos.DrawLine(transform.position - left, transform.position - 2 * down - left);
        Gizmos.DrawLine(transform.position - 2 * down - left, transform.position - 2 * down + left);
        Gizmos.DrawLine(transform.position - 2 * down + left, transform.position + left);
    }

    public override void SetAnimation(string name, bool force = false, float time = -1)
    {
        timeSinceLastAnimChange = 0;
        string modifiedName = name;

        //Hardcode talking animations not having back sprites (Note: also need to force this for special cases as well)
        if (showBack && !name.Contains("talk") && !name.Contains("hurt"))
        {
            modifiedName = name + "_back";
        }

        if (name.Contains("flying"))
        {
            height = heightFlying;
            width = widthFlying;
            sprite.transform.localPosition = offsetFlying;
        } else
        {
            height = heightGrounded;
            width = widthGrounded;
            sprite.transform.localPosition = offsetGrounded;
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
            newanim = newanim.Replace("flying", "");
            SetAnimation(newanim, force, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            return;
        }

        SetAnimation(name, force, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }
}
