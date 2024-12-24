using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetXPEffectScript : MonoBehaviour
{
    public TextDisplayer getXPText;
    public float xoffset;
    public TextDisplayer perfecttext;
    public TextDisplayer decisivetext;
    public TextDisplayer fortunetext;

    public XPDisplayerScript xpDisp;

    public int state;
    float stateWait;

    public int xpGet;
    public int pdExp;

    public void Setup(int xpGet, bool perfect, bool decisive, int fortunePower)
    {
        state = 0;
        stateWait = 0;

        xpDisp = FindObjectOfType<XPDisplayerScript>();

        pdExp = BattleControl.Instance.playerData.exp;

        this.xpGet = xpGet;

        if (xpGet == 0 && !perfect && !decisive && !(fortunePower > 0))
        {
            getXPText.SetText("", true, true);
        }
        else
        {
            getXPText.SetText("You got " + xpGet + " XP!", true, true);
        }
        getXPText.gameObject.transform.localPosition = Vector3.up * 500;

        int count = 0;
        if (perfect)
        {
            perfecttext.gameObject.SetActive(true);
            perfecttext.SetText("Perfect x1.5", true, true);
            perfecttext.gameObject.transform.localPosition = xoffset * Vector3.right + (1 + count) * -30 * Vector3.up;
            count++;
        } else
        {
            perfecttext.gameObject.SetActive(false);
        }

        if (decisive)
        {
            decisivetext.gameObject.SetActive(true);
            decisivetext.SetText("Decisive x2", true, true);
            decisivetext.gameObject.transform.localPosition = xoffset * Vector3.right + (1 + count) * -30 * Vector3.up;
            count++;
        }
        else
        {
            decisivetext.gameObject.SetActive(false);
        }

        if (fortunePower > 0)
        {
            fortunetext.gameObject.SetActive(true);
            fortunetext.SetText("Fortune x" + MainManager.PrecisionTruncate(CharmEffect.GetFortunePower(fortunePower)), true, true);
            fortunetext.gameObject.transform.localPosition = xoffset * Vector3.right + (1 + count) * -30 * Vector3.up;
            count++;
        }
        else
        {
            fortunetext.gameObject.SetActive(false);
        }

        //Something bad happened
        if (xpDisp == null)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case 0: //Text falls in
                getXPText.gameObject.transform.localPosition = MainManager.EasingQuadraticTime(getXPText.gameObject.transform.localPosition, Vector3.zero, 8000);
                xpDisp.gameObject.transform.localPosition = MainManager.EasingQuadraticTime(xpDisp.gameObject.transform.localPosition, Vector3.down * 125 + Vector3.right * 250, 10000);
                if (getXPText.gameObject.transform.localPosition == Vector3.zero && xpDisp.gameObject.transform.localPosition == Vector3.down * 125 + Vector3.right * 250)
                {
                    stateWait += Time.deltaTime;
                    if (stateWait > 0.5f)
                    {
                        state = 1;
                        stateWait = 0;
                        BattleControl.Instance.battleXP = 0;
                    }
                }
                break;
            case 1: //Wait for xp scroll to be done
                BattleControl.Instance.playerData.exp = pdExp + (xpGet - (int)BattleControl.Instance.visualXP);
                if (BattleControl.Instance.visualXP < 1)
                {
                    stateWait += Time.deltaTime;
                    if (stateWait > 0.25f)
                    {
                        state = 2;
                        stateWait = 0;
                    }
                }
                break;
            case 2: //flying up
                stateWait += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.up * stateWait * 2500;

                if (stateWait > 0.25f)
                {
                    Destroy(gameObject);
                }

                break;
        }
    }
}
