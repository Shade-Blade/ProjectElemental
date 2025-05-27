using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class OverworldHUDScript : MonoBehaviour
{
    //I can just use references I guess?
    public GameObject owStatDisplayer;
    public GameObject epDisplayer;
    public GameObject seDisplayer;
    public GameObject coinDisplayer;
    public GameObject expDisplayer;
    public GameObject itemDisplayer;

    public List<OWHPDisplayerScript> l_owStatDisplayers;
    public EPDisplayerScript l_epDisplayer;
    public SEDisplayerScript l_seDisplayer;
    public CoinDisplayerScript l_coinDisplayer;
    public EXPDisplayerScript l_expDisplayer;
    public ItemDisplayerScript l_itemDisplayer;

    public RectTransform topLeft;
    public RectTransform topRight;
    public RectTransform bottomRight;

    public List<GameObject> statDisplayers;

    public const float FADE_IN_DISTANCE = 150;

    public float settableFadeIn;
    public float fadein;

    public void Build()
    {
        statDisplayers = new List<GameObject>();

        PlayerData pd = MainManager.Instance.playerData;
        l_owStatDisplayers = new List<OWHPDisplayerScript>();
        for (int i = pd.party.Count - 1; i >= 0; i--)
        {
            GameObject g = Instantiate(owStatDisplayer, topLeft);
            OWHPDisplayerScript s = g.GetComponent<OWHPDisplayerScript>();
            l_owStatDisplayers.Add(s);
            s.SetEntity(pd.party[i]);
            if (pd.party.Count == 1)
            {
                s.SetPosition(0.5f);
            }
            else
            {
                s.SetPosition(pd.party.Count - i - 1);
            }
            statDisplayers.Add(g);
        }

        GameObject h = Instantiate(epDisplayer, topLeft);
        EPDisplayerScript d = h.GetComponent<EPDisplayerScript>();
        l_epDisplayer = d;
        d.Setup(pd);
        d.SetPosition();
        statDisplayers.Add(h);

        GameObject k = Instantiate(seDisplayer, topLeft);
        SEDisplayerScript se = k.GetComponent<SEDisplayerScript>();
        l_seDisplayer = se;
        se.Setup(pd);
        se.SetPosition();
        statDisplayers.Add(k);

        //Bad naming
        GameObject ex = Instantiate(expDisplayer, topRight);
        EXPDisplayerScript exp = ex.GetComponent<EXPDisplayerScript>();
        l_expDisplayer = exp;
        exp.Setup(pd);
        //se.SetPosition();
        statDisplayers.Add(ex);

        GameObject cn = Instantiate(coinDisplayer, topRight);
        CoinDisplayerScript cns = cn.GetComponent<CoinDisplayerScript>();
        l_coinDisplayer = cns;
        cns.Setup(pd);
        //se.SetPosition();
        statDisplayers.Add(cn);

        GameObject it = Instantiate(itemDisplayer, topRight);
        ItemDisplayerScript its = it.GetComponent<ItemDisplayerScript>();
        l_itemDisplayer = its;
        its.Setup(pd);
        //se.SetPosition();
        statDisplayers.Add(it);
    }

    public void SetFadeDirectly(float fade)
    {
        fadein = fade;
        //use fadein
        float distance = FADE_IN_DISTANCE * (1 - fadein);

        topLeft.anchoredPosition = Vector2.up * distance;
        topRight.anchoredPosition = Vector2.up * distance;
        bottomRight.anchoredPosition = Vector2.down * distance;
    }

    public void Update()
    {
        if (!MainManager.Instance.isPaused)
        {
            fadein = MainManager.EasingQuadraticTime(fadein, settableFadeIn, 15);

            //use fadein
            float distance = FADE_IN_DISTANCE * (1 - fadein);

            topLeft.anchoredPosition = Vector2.up * distance;
            topRight.anchoredPosition = Vector2.up * distance;
            bottomRight.anchoredPosition = Vector2.down * distance;
        }
    }

    public bool HasDisplayedState()
    {
        foreach (OWHPDisplayerScript ohds in l_owStatDisplayers)
        {
            if (ohds.displayHPCooldown > 0)
            {
                return true;
            }
        }

        if (l_epDisplayer.displayEPCooldown > 0)
        {
            return true;
        }


        if (l_seDisplayer.displaySECooldown > 0)
        {
            return true;
        }

        if (l_coinDisplayer.displayCoinsCooldown > 0)
        {
            return true;
        }

        if (l_itemDisplayer.displayItemsCooldown > 0)
        {
            return true;
        }

        if (l_expDisplayer.displayXPCooldown > 0)
        {
            return true;
        }

        return false;
    }
}
