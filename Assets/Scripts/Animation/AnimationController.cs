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
    public SpriteRenderer sprite;

    public bool rightDefault;

    public MaterialPropertyBlock propertyBlock;

    public GameObject subobject;

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

    public virtual void SetAnimation(string name)
    {
        bool update = !(animator.GetCurrentAnimatorStateInfo(0).IsName(name));
        currentAnim = name;

        if (animator != null && update)
        {
            animator.Play(name);
        }
    }

    public virtual void SendAnimationData(string data)
    {
        if (data.Equals("xflip"))
        {
            //Flip X breaks lighting in certain cases
            //Rotation does not

            //sprite.flipX = true;
            //sprite.gameObject.transform.localEulerAngles = Vector3.up * 180;
            subobject.transform.localEulerAngles = Vector3.up * 180;
        }
        if (data.Equals("xunflip"))
        {
            //sprite.flipX = false;
            //sprite.gameObject.transform.localEulerAngles = Vector3.up * 0;
            subobject.transform.localEulerAngles = Vector3.up * 0;
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

        sprite.material = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect2");
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

        sprite.material = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteEffect2");
        sprite.SetPropertyBlock(propertyBlock);
    }
    public void EffectUpdate(string data)
    {
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

            //TODO: Find a better way!!!
            //This seems like a bad idea
            //But keeping 6 materials permanently loaded also seems bad
            //I guess I should find a centralized solution that only loads some of them
            Material targetMaterial = sprite.material;
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
        Material targetMaterial = MainManager.Instance.defaultSpriteMaterial;
        sprite.material = targetMaterial;
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        propertyBlock.SetTexture("_MainTex", sprite.sprite.texture);
        propertyBlock.SetVector("_OcclusionColor", new Color(0, 0, 0, 0.2f));
        sprite.SetPropertyBlock(propertyBlock);
    }

    public virtual string GetCurrentAnim()
    {
        return currentAnim;
    }

    public virtual void Update()
    {
        lifetime += Time.deltaTime;

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
