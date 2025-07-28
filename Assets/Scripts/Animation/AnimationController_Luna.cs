using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController_Luna : AnimationController_Player
{
    public GameObject offsetter;

    public override void SetAnimation(string name, bool force = false, float time = -1)
    {
        timeSinceLastAnimChange = 0;
        string modifiedName = name;

        //Debug.Log(showBack + " " + !name.Contains("smash") + " " + !name.Contains("slash") + " " + name);
        //also making talk animations not have back variants
        if (showBack && !name.Contains("smash") && !name.Contains("slash") && !name.Contains("talk") && !name.Contains("hurt"))
        {
            modifiedName = name + "_back";
        }

        if (modifiedName.Equals("smash_e") && sprite.flipX)
        {
            modifiedName = "smash_w";
        }


        //bool update = (!currentAnim.Equals(modifiedName));
        //better way
        bool update = !(animator.GetCurrentAnimatorStateInfo(0).IsName(modifiedName)) || force;
        currentAnim = modifiedName;

        //fixes a problem I'm having somehow
        //update = true;


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

    public override void SetMaterial(int id)
    {
        //TODO: Find a better way!!!
        //This seems like a bad idea
        //But keeping 6 materials permanently loaded also seems bad
        //I guess I should find a centralized solution that only loads some of them

        Material targetMaterial = null;
        materialIndex = id;
        switch (materialIndex)
        {
            case 0:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/ProperSpriteGeneral_Luna");
                break;
            case 1:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect1_Luna");
                break;
            case 2:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect2_Luna");
                break;
            case 3:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect3_Luna");
                break;
            case 4:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect4_Luna");
                break;
            case 5:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect5_Luna");
                break;
        }
        sprite.material = targetMaterial;
    }

    public override void Update()
    {
        base.Update();

        //Fix for an offsetting problem I'm having
        //doesn't work for some dumb reason
        /*
        Debug.Log(sprite.transform.position);
        if (sprite.flipX)
        {
            //2x so that it reverses the x offset
            offsetter.transform.localPosition = 2 * Vector3.left * sprite.gameObject.transform.localPosition.x;
        }
        */
    }
}
