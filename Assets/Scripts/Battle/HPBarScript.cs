using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarScript : MonoBehaviour
{
    public BattleEntity entity;

    public SpriteRenderer sprite;
    public TMPro.TMP_Text hpText;
    public TMPro.TMP_Text defText;

    public SpriteRenderer emptybar;
    public SpriteRenderer fullbarA;
    public SpriteRenderer fullbarB;
    public SpriteRenderer bufferBar;

    public const float HP_BAR_WIDTH = 0.22f;
    public const float HP_UP_OFFSET = 0.0075f;

    public const float HP_LIGHTER_HEIGHT = 0.03f;
    public const float HP_DARKER_HEIGHT = 0.015f;

    //scroll per second
    //I want this to be noticeable but not too slow
    public const float HP_SCROLL_SPEED = 4f;
    public const float BUFFER_SCROLL_SPEED = 2f;    

    private float proportion;       //where the bar is
    private float bufferproportion; //lags behind proportion (used to show damage dealt or healed, may be lower or greater than proportion)

    private float bufferTimer;  //how long to keep buffer in place? (0 = buffer can move, reset to BUFFER_TIME when proportion moves)
    public const float BUFFER_TIME = 0.25f;  //after it stops moving, how long to wait?

    private float realproportion;   //hp / max hp (i.e. the true value)

    public int lastHP;
    public int lastDef;

    public void Setup(BattleEntity b)
    {
        int defense = int.MinValue;

        if (BattleControl.Instance.playerData.BadgeEquipped(Badge.BadgeType.DefenseSight))
        {
            defense = b.GetDefense();
        }

        Setup(b.hp, b.maxHP, defense);
        entity = b;

        if (b.GetEntityProperty(BattleHelper.EntityProperties.HideHP))
        {
            Setup(1, 1, defense);
            Color a = BattleControl.Instance.GetHPBarColors()[0];
            Color bc = BattleControl.Instance.GetHPBarColors()[3];
            Color newColor = new Color((a.r + bc.r)/2, (a.g + bc.g) / 2, (a.b + bc.b) / 2, 1);
            fullbarA.color = newColor;
            fullbarB.color = newColor;
            fullbarA.transform.localScale = Vector3.up * HP_LIGHTER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * 1;
            fullbarB.transform.localScale = Vector3.up * HP_DARKER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * 1;
            if (hpText.color == Color.black)
            {
                //Very hacky fix
                hpText.text = "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold White Outline + Overlay\">?</font>";
            }
            else
            {
                hpText.text = "?";
            }
        }
    }
    public void Setup(int hp, int maxhp, int def = int.MinValue)
    {
        realproportion = (hp / (float)maxhp);
        if (float.IsNaN(realproportion))
        {
            realproportion = 0;
        }
        if (realproportion < 0)
        {
            realproportion = 0;
        }
        if (realproportion > 1)
        {
            realproportion = 1;
        }

        proportion = realproportion;
        bufferproportion = proportion;
        bufferTimer = 0;

        //the two parts of the shading are 2 different rectangles
        //they get scaled to the correct width
        fullbarA.transform.localScale = Vector3.up * HP_LIGHTER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * proportion;
        fullbarA.transform.localPosition = Vector3.up * HP_UP_OFFSET + Vector3.right * HP_BAR_WIDTH * (1 - proportion) * (-1 / 2.0f);
        fullbarB.transform.localScale = Vector3.up * HP_DARKER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * proportion;
        fullbarB.transform.localPosition = Vector3.right * HP_BAR_WIDTH * (1 - proportion) * (-1 / 2.0f);

        float bufferCenter = (bufferproportion + proportion) / 2;
        float bufferWidth = Mathf.Abs(bufferproportion - proportion);

        //unnecessary (since local scale is 0 and would be fixed later) but ehh
        bufferBar.transform.localScale = Vector3.up * HP_LIGHTER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * bufferWidth;
        bufferBar.transform.localPosition = Vector3.up * HP_UP_OFFSET + Vector3.right * HP_BAR_WIDTH * (bufferCenter - 0.5f);

        if (hpText.color == Color.black)
        {
            //Very hacky fix
            hpText.text = "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold White Outline + Overlay\">" + hp + "</font>";
        }
        else
        {
            hpText.text = "" + hp;
        }

        if (def != int.MinValue)
        {
            if (def > DefenseTableEntry.IMMUNITY_CONSTANT)
            {
                defText.text = "A";
            }
            else if (def == DefenseTableEntry.IMMUNITY_CONSTANT)
            {
                defText.text = "X";
            }
            else
            {
                defText.text = "" + def;
            }
        } else
        {
            defText.text = "";
        }

        fullbarA.color = BattleControl.Instance.GetHPBarColors()[0];
        fullbarB.color = BattleControl.Instance.GetHPBarColors()[1];
        bufferBar.color = BattleControl.Instance.GetHPBarColors()[2];
        emptybar.color = BattleControl.Instance.GetHPBarColors()[3];
        hpText.color = BattleControl.Instance.GetHPBarColors()[4];

        lastHP = hp;
        lastDef = def;
    }

    public void Update()
    {
        if (entity == null)
        {
            Destroy(this);
        } else
        {
            if (entity.GetEntityProperty(BattleHelper.EntityProperties.HideHP))
            {
                Color a = BattleControl.Instance.GetHPBarColors()[0];
                Color bc = BattleControl.Instance.GetHPBarColors()[3];
                Color newColor = new Color((a.r + bc.r) / 2, (a.g + bc.g) / 2, (a.b + bc.b) / 2, 1);
                fullbarA.color = newColor;
                fullbarB.color = newColor;
                fullbarA.transform.localScale = Vector3.up * HP_LIGHTER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * 1;
                fullbarB.transform.localScale = Vector3.up * HP_DARKER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * 1;
                if (hpText.color == Color.black)
                {
                    //Very hacky fix
                    hpText.text = "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold White Outline + Overlay\">?</font>";
                }
                else
                {
                    hpText.text = "?";
                }

                int def = int.MinValue;

                //presume that you can't equip DefenseSight without passing through Setup
                //This is a safe assumption
                if (lastDef != int.MinValue)
                {
                    def = entity.GetDefense();
                }

                if (lastDef != def)
                {
                    if (def != int.MinValue)
                    {
                        if (def > DefenseTableEntry.IMMUNITY_CONSTANT)
                        {
                            defText.text = "A";
                        }
                        else if (def == DefenseTableEntry.IMMUNITY_CONSTANT)
                        {
                            defText.text = "X";
                        }
                        else
                        {
                            defText.text = "" + def;
                        }
                    }
                    else
                    {
                        defText.text = "";
                    }
                    lastDef = def;
                }

                bufferBar.transform.localScale = Vector3.up * HP_LIGHTER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * 0;
            } else
            {
                //Move the proportion along
                realproportion = (entity.hp / (float)entity.maxHP);
                if (float.IsNaN(realproportion))
                {
                    realproportion = 0;
                }
                if (realproportion < 0)
                {
                    realproportion = 0;
                }
                if (realproportion > 1)
                {
                    realproportion = 1;
                }

                float maxDiff = HP_SCROLL_SPEED * Time.deltaTime;

                //snap proportion if target is within (proportion - max diff) and (proportion + max diff)
                if (realproportion > proportion - maxDiff && realproportion < proportion + maxDiff)
                {
                    if (realproportion != proportion)
                    {
                        bufferTimer = BUFFER_TIME;
                    }
                    proportion = realproportion;
                }
                else
                {
                    //otherwise move it along
                    bufferTimer = BUFFER_TIME;
                    if (realproportion < proportion - maxDiff)
                    {
                        proportion -= maxDiff;
                    }
                    else
                    {
                        proportion += maxDiff;
                    }
                }

                float maxDiffB = BUFFER_SCROLL_SPEED * Time.deltaTime;

                if (bufferTimer > 0)
                {
                    if (!entity.inCombo)
                    {
                        bufferTimer -= Time.deltaTime;
                    }
                }
                else
                {
                    bufferTimer = 0;
                    //Buffer can move with same rules as normal hp bar
                    //snap proportion if target is within (proportion - max diff) and (proportion + max diff)
                    if (realproportion > bufferproportion - maxDiffB && realproportion < bufferproportion + maxDiffB)
                    {
                        bufferproportion = realproportion;
                    }
                    else
                    {
                        //otherwise move it along
                        if (realproportion < bufferproportion - maxDiffB)
                        {
                            bufferproportion -= maxDiffB;
                        }
                        else
                        {
                            bufferproportion += maxDiffB;
                        }
                    }
                }

                fullbarA.transform.localScale = Vector3.up * HP_LIGHTER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * proportion;
                fullbarA.transform.localPosition = Vector3.up * HP_UP_OFFSET + Vector3.right * HP_BAR_WIDTH * (1 - proportion) * (-1 / 2.0f);
                fullbarB.transform.localScale = Vector3.up * HP_DARKER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * proportion;
                fullbarB.transform.localPosition = Vector3.right * HP_BAR_WIDTH * (1 - proportion) * (-1 / 2.0f);

                float bufferCenter = (bufferproportion + proportion) / 2;
                float bufferWidth = Mathf.Abs(bufferproportion - proportion);

                bufferBar.transform.localScale = Vector3.up * HP_LIGHTER_HEIGHT + Vector3.forward * 1 + Vector3.right * HP_BAR_WIDTH * bufferWidth;
                bufferBar.transform.localPosition = Vector3.up * HP_UP_OFFSET + Vector3.right * HP_BAR_WIDTH * (bufferCenter - 0.5f);

                if (lastHP != entity.hp)
                {
                    if (hpText.color == Color.black)
                    {
                        //Very hacky fix
                        hpText.text = "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold White Outline + Overlay\">" + entity.hp + "</font>";
                    }
                    else
                    {
                        hpText.text = "" + entity.hp;
                    }
                    lastHP = entity.hp;
                }

                int def = int.MinValue;

                //presume that you can't equip DefenseSight without passing through Setup
                //This is a safe assumption
                if (lastDef != int.MinValue)
                {
                    def = entity.GetDefense();
                }

                if (lastDef != def)
                {
                    if (def != int.MinValue)
                    {
                        if (def > DefenseTableEntry.IMMUNITY_CONSTANT)
                        {
                            defText.text = "A";
                        }
                        else if (def == DefenseTableEntry.IMMUNITY_CONSTANT)
                        {
                            defText.text = "X";
                        }
                        else
                        {
                            defText.text = "" + def;
                        }
                    }
                    else
                    {
                        defText.text = "";
                    }
                    lastDef = def;
                }
            }
        }
    }
}
