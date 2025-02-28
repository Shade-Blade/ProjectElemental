using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note: used in battle but can be used in the overworld for healing
public class EffectScript_Damage : MonoBehaviour
{
    public float maxLifetime;
    private float lifetime = 0;

    public float maxScale;
    public float maxScaleTime;

    public float bezierDuration;
    Vector3 startPos;
    public Vector3 bezierOffset = Vector3.up * 1.0f;
    public Vector3 bezierOffsetB = Vector3.up * 1.5f + Vector3.right * 1.5f;
    public Vector3 bezierOffsetC = Vector3.up * 1.5f + Vector3.left * 1.5f;
    public float starOffset = 1.8f; //distance

    private Color[] damageTypeColors = new Color[]
    {
        new Color(0.5f,1.0f,1.0f),      //Light
        new Color(0.0f,0.5f,1.0f),      //Water
        new Color(1.0f,1.0f,0.0f),      //Air
        new Color(0.2f,0.0f,0.4f),      //Dark
        new Color(1.0f,0.5f,0.0f),      //Fire
        new Color(0.0f,1.0f,0.0f),      //Earth
        new Color(1.0f,0.0f,1.0f),      //Prismatic
        new Color(0.5f,0.0f,0.0f),      //Void
    };

    private Color[] damageTypeAdvancedColors = new Color[]
    {
        new Color(0.0f,0.9f,0.9f),      //Light
        new Color(0.0f,0.0f,1.0f),      //Water
        new Color(1.0f,1.0f,1.0f),      //Air
        new Color(0.5f,0.0f,1.0f),      //Dark
        new Color(1.0f,0.0f,0.0f),      //Fire
        new Color(0.75f,1.0f,0.0f),     //Earth
        new Color(0.5f,0.0f,1.0f),      //Prismatic
        new Color(0.5f,0.4f,0.0f),      //Void
    };

    public int minStars = 3;
    public int maxStars = 10;

    public TMPro.TMP_Text text;
    public int number;


    public TMPro.TMP_Text textReduction;
    public TMPro.TMP_Text textBonus;

    public int dir = 0; //-1 = left, 0 = center, 1 = right

    public SpriteRenderer backSpriteA;
    public SpriteRenderer backSpriteB;

    public Material ministarMaterial;

    public GameObject proxy;

    BattleHelper.DamageEffect be;

    private Vector3 backSpriteScale = new Vector3(0.175f, 0.175f, 0.175f);

    private GameObject[] ministars;
    private GameObject[] damageTypeEffects;

    private Color[] numberColors = new Color[]
    {
        new Color(1.0f,0.6f,0.6f),      //unused (damage) (damage uses the hp bar colors)
        new Color(1.0f,1.0f,1.0f),      //heal
        new Color(0.0f,0.0f,0.0f),      //negative heal
        new Color(0.9f,0.4f,0.0f),      //energy
        new Color(1.0f,1.0f,1.0f),      //negative energy
        new Color(0.9f,0.9f,1.0f),      //block
        new Color(0.95f,0.82f,0.56f),   //super block (second color)
        new Color(1.0f,0.6f,0.6f),      //unused (unblockable) (damage uses the hp bar colors)
        new Color(1.0f,0.6f,0.6f),      //unused (max hp) (damage uses the hp bar colors)
        new Color(0.6f,1f,0.6f),      //unused (soft) (damage uses the hp bar colors)
        new Color(1.0f,0.0f,1.0f),      //soul energy heal
        new Color(0.5f,0.0f,0.5f),      //negative soul energy
        new Color(0.2f,0.5f,0.2f),      //stamina
        new Color(0.5f,0.5f,0.8f),      //negative stamina
        new Color(1f,1f,1f),      //coins
        new Color(1f,0.2f,0.3f),      //negative coins
    };

    public void Setup(BattleHelper.DamageEffect b, int number, string bonus = null, string reduction = null, BattleHelper.DamageType type = BattleHelper.DamageType.Default, ulong properties = 0)
    {
        if (bonus != null)
        {
            textBonus.text = bonus;
        } else
        {
            //move it down to be on top of the damage type
            textReduction.transform.localPosition = Vector3.down * 0.09f;
            textBonus.text = "";
        }
        if (reduction != null)
        {
            textReduction.text = reduction;
        } else
        {
            textReduction.text = "";
        }

        textBonus.color = BattleControl.Instance.GetHPBarColors()[4];
        if (textBonus.color == Color.black)
        {
            //Very hacky fix
            textBonus.text = "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold White Outline + Overlay\">" + textBonus.text + "</font>";
        }
        textReduction.color = BattleControl.Instance.GetHPBarColors()[4];
        if (textReduction.color == Color.black)
        {
            //Very hacky fix
            textReduction.text = "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold White Outline + Overlay\">" + textReduction.text + "</font>";
        }


        be = b;

        if (text == null)
            text = GetComponent<TMPro.TMP_Text>();
        this.number = number;
        if (number == 0)
        {
            text.text = "";
        } else
        {
            text.text = number.ToString();
            if (number.ToString().Equals("1"))  //The font I'm using has the number 1 being off center to an extent that I have to manually fix it
            {
                text.transform.localPosition = Vector3.left * 0.005f;
            }
        }

        //set up stuff based on battle effect
        switch (b)
        {
            case BattleHelper.DamageEffect.BlockedDamage:
                text.color = BattleControl.Instance.GetHPBarColors()[3];
                Color blockColor = numberColors[5];

                Color midpoint = new Color(Mathf.Lerp(text.color.r, blockColor.r, 0.5f), Mathf.Lerp(text.color.g, blockColor.g, 0.5f), Mathf.Lerp(text.color.b, blockColor.b, 0.5f));
                TMPro.VertexGradient gradient = new TMPro.VertexGradient(text.color, midpoint, midpoint, blockColor);
                text.colorGradient = gradient;
                text.color = Color.white;
                break;
            case BattleHelper.DamageEffect.SuperBlockedDamage:
                text.color = BattleControl.Instance.GetHPBarColors()[3];
                TMPro.VertexGradient gradient2 = new TMPro.VertexGradient(text.color, numberColors[5], numberColors[5], numberColors[6]);
                text.colorGradient = gradient2;
                text.color = Color.white;
                break;
            case BattleHelper.DamageEffect.SoftDamage:
                text.color = BattleControl.Instance.GetHPBarColors()[3];
                Color softColor = numberColors[9];

                Color smidpoint = new Color(Mathf.Lerp(text.color.r, softColor.r, 0.5f), Mathf.Lerp(text.color.g, softColor.g, 0.5f), Mathf.Lerp(text.color.b, softColor.b, 0.5f));
                TMPro.VertexGradient sgradient = new TMPro.VertexGradient(text.color, smidpoint, smidpoint, softColor);
                text.colorGradient = sgradient;
                text.color = Color.white;
                break;
            case BattleHelper.DamageEffect.Damage:
            case BattleHelper.DamageEffect.UnblockableDamage:
            case BattleHelper.DamageEffect.MaxHPDamage:
                text.color = BattleControl.Instance.GetHPBarColors()[3];
                break;
            default:
                text.color = numberColors[(int)b];
                break;
        }

        bool advanced = BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.AdvancedElementCalc);

        //set up mini stars
        switch (b)
        {
            case BattleHelper.DamageEffect.BlockedDamage:
            case BattleHelper.DamageEffect.SuperBlockedDamage:
            case BattleHelper.DamageEffect.UnblockableDamage:
            case BattleHelper.DamageEffect.MaxHPDamage:
            case BattleHelper.DamageEffect.SoftDamage:
            case BattleHelper.DamageEffect.Damage:
                if (type != BattleHelper.DamageType.Normal && type != BattleHelper.DamageType.Default)
                {
                    List<Color> colorList = new List<Color>();
                    List<Color> advancedColorList = null;

                    if (advanced)
                    {
                        advancedColorList = new List<Color>();
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        if (((int)type & (1 << (j))) != 0)
                        {
                            colorList.Add(damageTypeColors[j]);

                            if (advanced)
                            {
                                advancedColorList.Add(damageTypeAdvancedColors[j]);
                            }
                        }
                    }

                    //at least 1 star per damage type (so the mythical "all types" attack would get all the colors right)
                    ministars = new GameObject[Mathf.Clamp(number, Mathf.Max(minStars, colorList.Count), maxStars)];

                    for (int i = 0; i < ministars.Length; i++)
                    {
                        //note that a lot of the setup happens later but I should do all the initialization stuff here
                        //(some of this prevents weirdness coming from any gap between this code and the update function executing)
                        ministars[i] = new GameObject("Ministar");
                        ministars[i].transform.parent = transform;
                        ministars[i].transform.position = startPos;
                        SpriteRenderer tempsp = ministars[i].AddComponent<SpriteRenderer>();
                        tempsp.sprite = BattleControl.Instance.damageEffectStar;
                        tempsp.color = colorList[i % colorList.Count];
                        tempsp.material = ministarMaterial;
                        ministars[i].transform.localScale = Vector3.zero;

                        if (advanced)
                        {
                            GameObject g = new GameObject("Sub Ministar");
                            g.transform.parent = ministars[i].transform;
                            g.transform.localPosition = Vector3.zero;
                            SpriteRenderer tempspG = g.AddComponent<SpriteRenderer>();
                            tempspG.sprite = BattleControl.Instance.damageEffectStar;
                            tempspG.color = advancedColorList[i % advancedColorList.Count];
                            tempsp.material = ministarMaterial;
                            g.transform.localScale = Vector3.one * 0.4f;
                        }
                    }
                }
                break;
        }

        switch (b)
        {
            case BattleHelper.DamageEffect.BlockedDamage:
            case BattleHelper.DamageEffect.SuperBlockedDamage:
            case BattleHelper.DamageEffect.UnblockableDamage:
            case BattleHelper.DamageEffect.MaxHPDamage:
            case BattleHelper.DamageEffect.SoftDamage:
            case BattleHelper.DamageEffect.Damage:
                backSpriteA.sprite = BattleControl.Instance.damageEffectStar;
                backSpriteB.sprite = BattleControl.Instance.damageEffectStar;
                //text.color = BattleControl.Instance.GetHPBarColors()[3];
                backSpriteA.color = BattleControl.Instance.GetHPBarColors()[0];
                backSpriteB.color = BattleControl.Instance.GetHPBarColors()[3];
                break;
            case BattleHelper.DamageEffect.Heal:
                backSpriteA.sprite = BattleControl.Instance.heartEffect;
                backSpriteB.sprite = BattleControl.Instance.heartEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteA.color = Color.red;
                backSpriteB.color = Color.red;
                break;
            case BattleHelper.DamageEffect.NegativeHeal:
                backSpriteA.sprite = BattleControl.Instance.heartEffect;
                backSpriteB.sprite = BattleControl.Instance.heartEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteA.color = new Color(0.5f, 0f, 1f);
                backSpriteB.color = new Color(0.5f, 0f, 1f);
                break;
            case BattleHelper.DamageEffect.Energize:
                backSpriteA.sprite = BattleControl.Instance.energyEffect;
                backSpriteB.sprite = BattleControl.Instance.energyEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteA.color = new Color(1f, 1f, 0f);
                backSpriteB.color = new Color(1f, 1f, 0f);
                break;
            case BattleHelper.DamageEffect.DrainEnergy:
                backSpriteA.sprite = BattleControl.Instance.energyEffect;
                backSpriteB.sprite = BattleControl.Instance.energyEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteA.color = new Color(0.25f, 0.25f, 0.05f);
                backSpriteB.color = new Color(0.25f, 0.25f, 0.05f);
                break;
            case BattleHelper.DamageEffect.SoulEnergize:
                backSpriteA.sprite = BattleControl.Instance.soulEffect;
                backSpriteB.sprite = BattleControl.Instance.soulEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteA.color = new Color(0.7f, 0f, 0.7f);
                backSpriteB.color = new Color(0.7f, 0f, 0.7f);
                break;
            case BattleHelper.DamageEffect.DrainSoulEnergy:
                backSpriteA.sprite = BattleControl.Instance.soulEffect;
                backSpriteB.sprite = BattleControl.Instance.soulEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteA.color = new Color(0.3f, 0.0f, 0.3f);
                backSpriteB.color = new Color(0.3f, 0.0f, 0.3f);
                break;
            case BattleHelper.DamageEffect.Stamina:
                backSpriteA.sprite = BattleControl.Instance.staminaEffect;
                backSpriteB.sprite = BattleControl.Instance.staminaEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteA.color = new Color(0.6f, 1.0f, 0.5f);
                backSpriteB.color = new Color(0.6f, 1.0f, 0.5f);
                break;
            case BattleHelper.DamageEffect.DrainStamina:
                backSpriteA.sprite = BattleControl.Instance.staminaEffect;
                backSpriteB.sprite = BattleControl.Instance.staminaEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteA.color = new Color(0.25f, 0.3f, 0.2f);
                backSpriteB.color = new Color(0.25f, 0.3f, 0.2f);
                break;
            case BattleHelper.DamageEffect.Coins:
                backSpriteA.sprite = BattleControl.Instance.coinEffect;
                backSpriteB.sprite = BattleControl.Instance.coinEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteScale = Vector3.one * 0.15f;                  //hacky
                backSpriteA.color = new Color(1f, 0.85f, 0.3f);
                backSpriteB.color = new Color(1f, 0.85f, 0.3f);
                break;
            case BattleHelper.DamageEffect.NegativeCoins:
                backSpriteA.sprite = BattleControl.Instance.coinEffect;
                backSpriteB.sprite = BattleControl.Instance.coinEffect;
                backSpriteA.transform.localPosition = Vector3.zero;
                backSpriteB.transform.localPosition = Vector3.zero;
                backSpriteScale = Vector3.one * 0.15f;                  //hacky
                backSpriteA.color = new Color(0.5f, 0.4f, 0.25f);
                backSpriteB.color = new Color(0.5f, 0.4f, 0.25f);
                break;
        }
    }

    //against player: goes left
    //against enemy: goes right
    public void SetDir(bool isPlayer)
    {
        dir = isPlayer ? -1 : 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        text = GetComponent<TMPro.TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        float heaviness = 0.5f;
        float completion = (lifetime / bezierDuration);
        float falsecompletion = completion * (1f + heaviness) + completion * completion * -heaviness;

        float startdegree = 0;
        float degreespread = 0;

        switch (dir)
        {
            case 1:
                proxy.transform.position = MainManager.BezierCurve(falsecompletion, startPos, startPos + bezierOffsetB, startPos);
                startdegree = 45;
                break;
            case -1:
                proxy.transform.position = MainManager.BezierCurve(falsecompletion, startPos, startPos + bezierOffsetC, startPos);
                startdegree = 315;
                break;
            default:
                proxy.transform.position = MainManager.BezierCurve(falsecompletion, startPos, startPos + bezierOffset, startPos);
                break;
        }

        float numScale = Mathf.Min(0.65f + number * 0.025f, 1);
        if (number < 0)
        {
            numScale = Mathf.Min(0.65f - number * 0.025f, 1);
        }
        if (number == 0)
        {
            numScale = 0.4f;
        }

        if (be == BattleHelper.DamageEffect.SoftDamage)
        {
            numScale = 0.45f;
        }

        if (ministars != null)
        {
            //degree spread = numstars * (9)
            degreespread = Mathf.Clamp(number, minStars, maxStars) * 180f / 10;

            float dtr = (Mathf.PI / 180);

            for (int i = 0; i < ministars.Length; i++)
            {
                //Compute some stuff
                float tempdegree = startdegree - degreespread / 2 + i * (degreespread / (ministars.Length - 1));
                if (tempdegree > 360)
                {
                    tempdegree -= 360;
                }
                if (tempdegree < 0)
                {
                    tempdegree += 360;
                }

                Vector3 tempOffset = Vector3.up * Mathf.Cos(tempdegree * dtr) + Vector3.right * Mathf.Sin(tempdegree * dtr);
                tempOffset *= starOffset;

                ministars[i].transform.position = MainManager.BezierCurve(falsecompletion, startPos, startPos + tempOffset, startPos);

                if (lifetime < maxScaleTime)
                {
                    ministars[i].transform.localScale = Vector3.one * maxScale * (lifetime / maxScaleTime) * 0.04f;
                }
                else
                {
                    ministars[i].transform.localScale = Vector3.one * maxScale * 0.04f;
                }
            }
        }

        switch (be)
        {
            case BattleHelper.DamageEffect.SuperBlockedDamage:
            case BattleHelper.DamageEffect.BlockedDamage:
            case BattleHelper.DamageEffect.Damage:
                backSpriteB.transform.eulerAngles = Vector3.forward * (144f + 36f * Mathf.Min(number,15)/15f);
                backSpriteB.transform.localPosition = Vector3.up * 0.01f;
                break;
            case BattleHelper.DamageEffect.UnblockableDamage:
                backSpriteB.transform.eulerAngles = Vector3.forward * (180f + 144f * completion);
                backSpriteB.transform.localPosition = Vector3.up * 0.01f;
                break;
            case BattleHelper.DamageEffect.MaxHPDamage:
                break;
        }

        if (lifetime < maxScaleTime)
        {
            proxy.transform.localScale = Vector3.one * maxScale * (lifetime / maxScaleTime);            
            backSpriteA.transform.localScale = backSpriteScale * numScale;
            if (be == BattleHelper.DamageEffect.MaxHPDamage)
            {
                backSpriteB.transform.localScale = backSpriteScale * numScale + Vector3.one * 0.05f;
            }
            else
            {
                if (number < 0)
                {
                    backSpriteB.transform.localScale = backSpriteScale * numScale * (Mathf.Min(0.7f - 0.02f * number, 1f));
                }
                else
                {
                    backSpriteB.transform.localScale = backSpriteScale * numScale * (Mathf.Min(0.7f + 0.02f * number, 1f));
                }
            }
        }
        else
        {
            proxy.transform.localScale = new Vector3(1, 1, 1) * maxScale;
            backSpriteA.transform.localScale = backSpriteScale * numScale;
            if (be == BattleHelper.DamageEffect.MaxHPDamage)
            {
                backSpriteB.transform.localScale = backSpriteScale * numScale + Vector3.one * 0.05f;
            }
            else
            {
                if (number < 0)
                {
                    backSpriteB.transform.localScale = backSpriteScale * numScale * (Mathf.Min(0.7f - 0.02f * number, 1f));
                }
                else
                {
                    backSpriteB.transform.localScale = backSpriteScale * numScale * (Mathf.Min(0.7f + 0.02f * number, 1f));
                }
            }
        }

        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
