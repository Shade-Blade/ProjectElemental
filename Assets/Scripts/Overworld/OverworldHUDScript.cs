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

    public RectTransform topLeft;
    public RectTransform topRight;
    public RectTransform bottomRight;

    public List<GameObject> statDisplayers;

    public const float FADE_IN_DISTANCE = 60;

    public float settableFadeIn;
    public float fadein;

    public void Build()
    {
        statDisplayers = new List<GameObject>();

        PlayerData pd = MainManager.Instance.playerData;
        for (int i = pd.party.Count - 1; i >= 0; i--)
        {
            GameObject g = Instantiate(owStatDisplayer, topLeft);
            OWStatDisplayerScript s = g.GetComponent<OWStatDisplayerScript>();
            s.SetEntity(pd.party[i]);
            s.SetPosition(pd.party.Count - i - 1);
            statDisplayers.Add(g);
        }

        GameObject h = Instantiate(epDisplayer, topRight);
        EPDisplayerScript d = h.GetComponent<EPDisplayerScript>();
        d.Setup(pd);
        d.SetPosition();
        statDisplayers.Add(h);

        GameObject k = Instantiate(seDisplayer, topRight);
        SEDisplayerScript se = k.GetComponent<SEDisplayerScript>();
        se.Setup(pd);
        se.SetPosition();
        statDisplayers.Add(k);

        //Bad naming
        GameObject ex = Instantiate(expDisplayer, bottomRight);
        EXPDisplayerScript exp = ex.GetComponent<EXPDisplayerScript>();
        exp.Setup(pd);
        //se.SetPosition();
        statDisplayers.Add(ex);

        GameObject cn = Instantiate(coinDisplayer, bottomRight);
        CoinDisplayerScript cns = cn.GetComponent<CoinDisplayerScript>();
        cns.Setup(pd);
        //se.SetPosition();
        statDisplayers.Add(cn);

        GameObject it = Instantiate(itemDisplayer, bottomRight);
        ItemDisplayerScript its = it.GetComponent<ItemDisplayerScript>();
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
}
