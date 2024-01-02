using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectScript_Generic : MonoBehaviour
{
    public ParticleSystem ps;
    public float baseEmission;
    public float baseWidth;

    protected bool playing = false;

    //By convention it should go to a height above 1.5
    //(and also roughly 0.75 - 1 in both x and z directions)
    //but these aren't hard and fast rules, just make sure it looks good

    public virtual void Setup(float scale = 1, float power = 1)
    {
        //particle system code is sus
        ParticleSystem.MainModule mm = ps.main;
        ParticleSystem.EmissionModule emm = ps.emission;
        //mm.startColor = color;

        ParticleSystem.Burst b = ps.emission.GetBurst(0);
        int k = (int)(baseEmission * power);
        if (k < 1)
        {
            k = 1;
        }
        b.count = k;
        emm.SetBurst(0, b);

        float width = baseWidth;
        transform.localScale = Vector3.one * scale;
        mm.startSize = width / scale;

        ps.Play();
        playing = true;
    }

    public virtual void Start()
    {
        if (!playing)
        {
            Setup();
        }
    }
}
