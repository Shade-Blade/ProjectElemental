using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController_Luna : AnimationController_Player
{
    public override void SetAnimation(string name, bool force = false)
    {
        //Hacky fix
        sprite.transform.localPosition = Vector3.zero;
        sprite.transform.localRotation = Quaternion.identity;

        string modifiedName = name;

        //Debug.Log(showBack + " " + !name.Contains("smash") + " " + !name.Contains("slash") + " " + name);
        //also making talk animations not have back variants
        if (showBack && !name.Contains("smash") && !name.Contains("slash") && !name.Contains("talk"))
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


        if (animator != null && update)
        {
            //Debug.Log(this.name + ": Play " + name);
            if (force)
            {
                animator.Play(modifiedName, -1, 0);
            }
            else
            {
                animator.Play(modifiedName);
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
        Debug.Log(targetMaterial);
        sprite.material = targetMaterial;
    }
}
