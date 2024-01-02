using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_SpiralIn : MonoBehaviour
{
    public ParticleSystem ps;

    //Note: there are no color constants so I can't make all the parameters have defaults
    //time is duration
    //scale: normal is roughly 2 units wide (scale changes that but keeps width)
    public void Setup(Color color, float time = 0.5f, int count = 15, float rotation = 0, float width = 0.1f, float scale = 1)
    {
        //particle system code is sus
        ParticleSystem.MainModule mm = ps.main;
        ParticleSystem.ShapeModule sm = ps.shape;
        ParticleSystem.EmissionModule emm = ps.emission;
        ParticleSystem.VelocityOverLifetimeModule volm = ps.velocityOverLifetime;

        transform.localScale = Vector3.one * scale;

        mm.startColor = color;
        mm.startSize = width / scale;

        sm.rotation = Vector3.forward * rotation;

        emm.SetBurst(0, new ParticleSystem.Burst(0, count));

        mm.startLifetime = time / 2;
        mm.startSpeed = -2 / time;
        volm.orbitalOffsetZ = 3 / time;
        volm.radial = -2 / time;

        ps.Play();
    }
}
