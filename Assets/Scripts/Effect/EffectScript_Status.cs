using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectScript_Status : MonoBehaviour
{
    public ParticleSystem ps;
    public float baseEmission;
    public float baseWidth;

    //power up to 4 (after that the difference is not noticeable)
    //color should be something like (1,0.3,0.3,0.8)
    public virtual void Setup(Color color, float scale = 1, float power = 1)
    {
        //particle system code is sus
        ParticleSystem.MainModule mm = ps.main;
        ParticleSystem.EmissionModule emm = ps.emission;
        //mm.startColor = color;

        mm.startColor = color;

        emm.rateOverTime = baseEmission * power;

        float width = baseWidth;
        transform.localScale = Vector3.one * scale;
        mm.startSize = width / scale;

        ps.Play();
    }
}
