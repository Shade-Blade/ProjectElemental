using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectScript_Fountain : MonoBehaviour
{
    public ParticleSystem ps;
    public float baseEmission;
    public float baseWidth;
    public float baseDuration;
    public float offset;
    
    //Power is number of things emitted
    //Rate over time and time are square rooted
    //color should be something like (1,0.3,0.3,0.8)
    public virtual void Setup(float scale = 1, float power = 1)
    {
        //particle system code is sus
        ParticleSystem.MainModule mm = ps.main;
        ParticleSystem.EmissionModule emm = ps.emission;
        //mm.startColor = color;

        //mm.startColor = color;

        float cut = Mathf.Pow(power, 0.667f);

        mm.duration = baseDuration * (power / cut);
        emm.rateOverTime = baseEmission * cut + (offset / (baseDuration * (power / cut)));

        float newScale = (1 + (power / (6f * cut))) * scale;

        float width = baseWidth;
        transform.localScale = Vector3.one * newScale;
        mm.startSize = width / newScale;

        ps.Play();
    }
}
