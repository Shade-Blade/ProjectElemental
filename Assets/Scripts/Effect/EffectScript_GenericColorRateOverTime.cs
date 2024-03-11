using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectScript_GenericColorRateOverTime : MonoBehaviour
{
    //public ParticleSystem ps;
    bool playing = false;
    //public float basePower;

    //power up to 4 (after that the difference is not noticeable)
    //color should be something like (1,0.3,0.3,0.8)
    public virtual void Setup(Color color, float scale = 1, float power = 1)
    {
        transform.localScale = Vector3.one * scale;
        //don't care about the width, big enemies get big particles I guess

        ParticleSystem.MainModule mm;// = ps.main;
        ParticleSystem.EmissionModule emm;// = ps.emission;
        int k;// = 0;
        /*
        //particle system code is sus
        ParticleSystem.MainModule mm = ps.main;
        ParticleSystem.EmissionModule emm = ps.emission;
        
        if (color.a != 0)
        {
            mm.startColor = color;
        }


        int k = (int)(basePower * power);
        if (k < 1)
        {
            k = 1;
        }
        if (k > basePower * 4)
        {
            k = (int)(basePower * 4);
        }
        emm.rateOverTime = k;
        */

        //hacky setup for children
        ParticleSystem[] psB = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < psB.Length; i++)
        {
            mm = psB[i].main;
            emm = psB[i].emission;
            if (color.a != 0)
            {
                mm.startColor = color;
            }

            k = (int)(emm.rateOverTime.constant * 0.25f * power);
            if (k < 1)
            {
                k = 1;
            }
            if (k > emm.rateOverTime.constant)
            {
                k = (int)(emm.rateOverTime.constant);
            }
            emm.rateOverTime = k;
            psB[i].Play();
        }

        playing = true;
    }

    public void Start()
    {
        if (!playing)
        {
            Setup(new Color(0.65f, 0.65f, 0.25f, 0.0f));
        }
    }
}
