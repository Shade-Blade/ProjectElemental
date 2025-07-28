using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController_Rainbow : AnimationController_BackSprite
{
    public override void SetMaterial(int id)
    {
        //Debug.Log("material " + id);
        //TODO: Find a better way!!!
        //This seems like a bad idea
        //But keeping 6 materials permanently loaded also seems bad
        //I guess I should find a centralized solution that only loads some of them

        Material targetMaterial = null;
        materialIndex = id;

        //usage
        //0 = no effect
        //1 = color change (opaque)
        //2 = color change (transparent)
        //3 = buff/debuff
        //4 = buff/debuff + color change (opaque)
        //5 = buff/debuff + color change (transparent)
        switch (materialIndex)
        {
            case 0:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteGeneral_Rainbow");
                break;
            case 1:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect1_Rainbow");
                break;
            case 2:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect2_Rainbow");
                break;
            case 3:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect3_Rainbow");
                break;
            case 4:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect4_Rainbow");
                break;
            case 5:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect5_Rainbow");
                break;
        }
        sprite.material = targetMaterial;
    }
}
