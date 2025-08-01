using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

//Generic middle ground class for animating sprites
//This is the basic one (Does not handle shaders)
//It has flipX because it is something that comes up everywhere
public class AnimationController : MonoBehaviour
{
    public Animator animator;
    public string currentAnim;
    public string currentEffect;    //Effect data changes this. (This is useful if you want to copy all the visual effects of one sprite to another, like if you have projectiles)
    public SpriteRenderer sprite;

    //Use for left facing sprites (inverts all the flips so things are consistent)
    public bool leftFacing;

    float targetAnimationSpeed = 1;

    public MaterialPropertyBlock propertyBlock;

    public GameObject subobject;    //used for rotation
    //Note that all sprites will have sub sub objects for sprite offsets and also to allow for a "base" offset

    //6 possible materials
    //0 = normal
    //1 = bigradient
    //2 = bigradient alpha
    //3 = normal + buffdebuff
    //4 = bigradient + buffdebuff
    //5 = bigradient alpha + buffdebuff
    public int materialIndex;

    public bool bigradientMap;
    //colors plugged into the material stuff
    public Color blackColor;
    public Color grayColor;
    public Color whiteColor;
    public float leak;

    public bool occlusion;
    public Color occlusionColor;

    public bool buffdebuff;
    public Color[] buffColors;
    public Color[] debuffColors;

    public int buffIndex;
    public int debuffIndex;

    public float lifetime = 0;
    public float timeSinceLastAnimChange = 0;

    //stuff doesn't necessarily read this, but it's good to have to check consistency in things
    public float height = 0.75f;
    public float width = 0.5f;

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        Vector3 down = transform.up * -1 * height / 2;
        Vector3 left = transform.right * -1 * width / 2;

        Gizmos.DrawLine(transform.position + left, transform.position - left);
        Gizmos.DrawLine(transform.position - left, transform.position - 2 * down - left);
        Gizmos.DrawLine(transform.position - 2 * down - left, transform.position - 2 * down + left);
        Gizmos.DrawLine(transform.position - 2 * down + left, transform.position + left);
    }


    public virtual void SetAnimation(string name, bool force = false, float time = -1)
    {
        timeSinceLastAnimChange = 0;
        bool update = !(animator.GetCurrentAnimatorStateInfo(0).IsName(name)) || force;
        currentAnim = name;

        if (animator != null && update)
        {
            if (force)
            {
                animator.Play(name, -1, 0);
            }
            else
            {
                if (time != -1)
                {
                    animator.Play(name, -1, time);
                }
                else
                {
                    animator.Play(name);
                }
            }
        }
    }

    public virtual void ReplaceAnimation(string name = null, bool force = false)
    {
        if (name == null)
        {
            SetAnimation(currentAnim, force, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            return;
        }

        SetAnimation(name, force, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

    public virtual void SendAnimationData(string data)
    {
        if (data.Equals("xflip"))
        {
            //Flip X breaks lighting in certain cases
            //Rotation does not

            //^ Past shade is wrong
            //As it turns out, my shader is set up incorrectly, which means that the exact opposite should be the case (flip should be fine, rotation should not be)

            sprite.flipX = true ^ leftFacing;
            //sprite.gameObject.transform.localEulerAngles = Vector3.up * 180;
            //subobject.transform.localEulerAngles = Vector3.up * 180;
        }
        if (data.Equals("xunflip"))
        {
            sprite.flipX = false ^ leftFacing;
            //sprite.gameObject.transform.localEulerAngles = Vector3.up * 0;
            //subobject.transform.localEulerAngles = Vector3.up * 0;
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

    public virtual void SetMaterial(int id)
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
                targetMaterial = MainManager.Instance.defaultSpriteMaterial;
                break;
            case 1:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect1");
                break;
            case 2:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect2");
                break;
            case 3:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect3");
                break;
            case 4:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect4");
                break;
            case 5:
                targetMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect5");
                break;
        }
        sprite.material = targetMaterial;
    }

    public void Aetherize()
    {
        blackColor = new Color(0.5f, 0f, 0f, 0.75f);
        grayColor = new Color(1f, 0.6f, 0.3f, 0.5f);
        whiteColor = new Color(1f, 0.6f, 0.3f, 0.5f);
        leak = 0.6f;
        bigradientMap = true;

        occlusionColor = new Color(0.5f, 0f, 0f, 0.1f);

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        //Sus
        propertyBlock.SetTexture("_MainTex", sprite.sprite.texture);

        propertyBlock.SetFloat("_Leak", leak);
        propertyBlock.SetVector("_BlackColor", blackColor);
        propertyBlock.SetVector("_GrayColor", grayColor);
        propertyBlock.SetVector("_WhiteColor", whiteColor);

        propertyBlock.SetVector("_OcclusionColor", occlusionColor);

        SetMaterial(2);
        sprite.SetPropertyBlock(propertyBlock);
    }
    public void Illuminate()
    {
        //Hopefully the color struct can handle invalid values
        blackColor = new Color(1.3f, 1f, 0.3f, 1f);
        grayColor = new Color(1.4f, 1.3f, 1f, 1f);
        whiteColor = new Color(2.3f, 2.3f, 1.3f, 1f);
        leak = 0.5f;
        bigradientMap = true;

        occlusionColor = new Color(1f, 0.85f, 0.5f, 0.1f);
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        propertyBlock.SetTexture("_MainTex", sprite.sprite.texture);

        propertyBlock.SetFloat("_Leak", leak);
        propertyBlock.SetVector("_BlackColor", blackColor);
        propertyBlock.SetVector("_GrayColor", grayColor);
        propertyBlock.SetVector("_WhiteColor", whiteColor);

        propertyBlock.SetVector("_OcclusionColor", occlusionColor);

        SetMaterial(2);
        sprite.SetPropertyBlock(propertyBlock);
    }
    public void EffectUpdate(string data)
    {
        currentEffect = data;

        if (data.Length <= 2)
        {
            MaterialReset();
            return;
        }

        //Debug.Log("effect " + data);
        string[] split = data.Split('_');

        if (!split[1].Equals("X"))
        {
            //
            string[] colorsplit = split[1].Split("|");
            blackColor = MainManager.ParseColor(colorsplit[0]).GetValueOrDefault();
            grayColor = MainManager.ParseColor(colorsplit[1]).GetValueOrDefault();
            whiteColor = MainManager.ParseColor(colorsplit[2]).GetValueOrDefault();
            float.TryParse(colorsplit[3], out leak);
            bigradientMap = true;
        }
        else
        {
            //no
            bigradientMap = false;
        }

        bool alpha = false;
        if (blackColor.a < 1 || grayColor.a < 1 || whiteColor.a < 1)
        {
            alpha = true;
        }

        if (!split[2].Equals("X"))
        {
            occlusion = true;
            occlusionColor = MainManager.ParseColor(split[2]).GetValueOrDefault();
        } else
        {
            occlusionColor = new Color(0, 0, 0, 0.2f);
        }

        buffdebuff = false;
        if (!split[3].Equals("X"))
        {
            //Debug.Log("Buff color set");
            //
            string[] colorsplit = split[3].Split("|");
            buffColors = new Color[colorsplit.Length];
            for (int i = 0; i < colorsplit.Length; i++)
            {
                buffColors[i] = MainManager.ParseColor(colorsplit[i]).GetValueOrDefault();
            }

            buffdebuff = true;
            //Debug.Log(buffColors.Length);   
        }
        else
        {
            //no
            buffColors = null;
        }

        if (!split[4].Equals("X"))
        {
            //
            string[] colorsplit = split[4].Split("|");
            debuffColors = new Color[colorsplit.Length];
            for (int i = 0; i < colorsplit.Length; i++)
            {
                debuffColors[i] = MainManager.ParseColor(colorsplit[i]).GetValueOrDefault();
            }

            buffdebuff = true;
        }
        else
        {
            //no
            debuffColors = null;
        }

        //At the end, determine which material to use really
        //Only change material if it is now different
        int newmaterialindex = 0;
        if (bigradientMap)
        {
            newmaterialindex += 1;
            if (alpha)
            {
                newmaterialindex += 1;
            }
        }

        if (buffdebuff)
        {
            newmaterialindex += 3;
        }

        if (newmaterialindex != materialIndex)
        {
            //material change time
            materialIndex = newmaterialindex;

            SetMaterial(materialIndex);
        }

        //In any case, update vars
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        //Sus
        propertyBlock.SetTexture("_MainTex", sprite.sprite.texture);

        propertyBlock.SetFloat("_Leak", leak);
        propertyBlock.SetVector("_BlackColor", blackColor);
        propertyBlock.SetVector("_GrayColor", grayColor);
        propertyBlock.SetVector("_WhiteColor", whiteColor);

        if (occlusion)
        {
            propertyBlock.SetVector("_OcclusionColor", occlusionColor);
        } else
        {
            propertyBlock.SetVector("_OcclusionColor", new Color(0,0,0,0.2f));
        }

        if (buffColors != null && buffColors.Length > 0)
        {
            propertyBlock.SetVector("_OverlayColor", buffColors[0]);
            //Debug.Log("Make color " + buffColors[0].ToString());
        }
        else
        {
            propertyBlock.SetVector("_OverlayColor", new Color(0, 0, 0, 1));
        }
        if (debuffColors != null && debuffColors.Length > 0)
        {
            propertyBlock.SetVector("_OverlayColorB", debuffColors[0]);
        }
        else
        {
            propertyBlock.SetVector("_OverlayColorB", new Color(0, 0, 0, 1));
        }

        sprite.SetPropertyBlock(propertyBlock);
    }
    public virtual void MaterialReset()
    {
        currentEffect = "";
        SetMaterial(0);
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        propertyBlock.SetTexture("_MainTex", sprite.sprite.texture);
        propertyBlock.SetVector("_OcclusionColor", new Color(0, 0, 0, 0.2f));
        sprite.SetPropertyBlock(propertyBlock);
    }

    public virtual void Awake()
    {
        sprite.flipX = leftFacing;
    }

    public virtual string GetCurrentAnim()
    {
        return currentAnim;
    }

    public void StopAnimation()
    {
        targetAnimationSpeed = 0;
    }

    public void ResetAnimationSpeed()
    {
        targetAnimationSpeed = 1;
    }

    //note: standard practice to reset should be to use this
    public float GetAnimationSpeed()
    {
        return targetAnimationSpeed;
    }
    //More useful
    public void MultiplyAnimationSpeed(float set)
    {
        targetAnimationSpeed *= set;
    }
    public void SetAnimationSpeed(float set)
    {
        targetAnimationSpeed = set;
    }

    public virtual void Update()
    {
        if (MainManager.Instance.isPaused)
        {
            animator.speed = 0;
        } else
        {
            animator.speed = targetAnimationSpeed;
        }

        lifetime += Time.deltaTime;
        timeSinceLastAnimChange += Time.deltaTime;

        if (materialIndex >= 3 && materialIndex <= 5)
        {
            float perBuffTime = (buffColors == null || buffColors.Length == 0) ? 1 : 0.5f + 1.5f / buffColors.Length;
            //Debug.Log(perBuffTime);
            int newbuffIndex = (buffColors == null || buffColors.Length == 0) ? 0 : Mathf.CeilToInt(lifetime / perBuffTime) % buffColors.Length;
            float perDebuffTime = (debuffColors == null || debuffColors.Length == 0) ? 1 : 0.5f + 1.5f / debuffColors.Length;
            int newdebuffIndex = (debuffColors == null || debuffColors.Length == 0) ? 0 : Mathf.CeilToInt(lifetime / perDebuffTime) % debuffColors.Length;

            if (newbuffIndex != buffIndex && buffColors != null && buffColors.Length > 0)
            {
                buffIndex = newbuffIndex;
                if (propertyBlock == null)
                {
                    propertyBlock = new MaterialPropertyBlock();
                }
                propertyBlock.SetVector("_OverlayColor", buffColors[buffIndex]);
                //Debug.Log("Make color " + buffColors[buffIndex].ToString());
                //Sus
                propertyBlock.SetTexture("_MainTex", sprite.sprite.texture);
                sprite.SetPropertyBlock(propertyBlock);
            }

            if (newdebuffIndex != debuffIndex && debuffColors != null && debuffColors.Length > 0)
            {
                debuffIndex = newdebuffIndex;
                if (propertyBlock == null)
                {
                    propertyBlock = new MaterialPropertyBlock();
                }
                propertyBlock.SetVector("_OverlayColorB", debuffColors[debuffIndex]);
                //Sus
                propertyBlock.SetTexture("_MainTex", sprite.sprite.texture);
                sprite.SetPropertyBlock(propertyBlock);
            }
        } else
        {
            buffIndex = 0;
            debuffIndex = 0;
        }
    }
}
