using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_Sparkle : MonoBehaviour
{
    public ParticleSystem ps;
    private bool playing = false;

    //Note: there are no color constants so I can't make all the parameters have defaults
    //time is duration
    //scale: normal is roughly 2 units wide (scale changes that but keeps width)
    public void Setup(Color color, float time = 0.5f, int count = 20, float width = 0.25f, float scale = 1)
    {
        //particle system code is sus
        ParticleSystem.MainModule mm = ps.main;
        ParticleSystem.ShapeModule sm = ps.shape;
        ParticleSystem.EmissionModule emm = ps.emission;
        ParticleSystem.VelocityOverLifetimeModule volm = ps.velocityOverLifetime;

        transform.localScale = Vector3.one * scale;

        mm.startColor = color;
        mm.startSize = width / scale;

        emm.SetBurst(0, new ParticleSystem.Burst(0, 1, count, (0.5f * time)/count));

        mm.startLifetime = time;
        mm.startSpeed = 0;

        //volm.radial = 0.25f / time;

        ps.Play();
        playing = true;
    }

    public void Start()
    {
        if (!playing)
        {
            Setup(Color.white);
        }
    }
}
