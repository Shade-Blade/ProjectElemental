using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_SpiralOutIn : MonoBehaviour
{
    public ParticleSystem ps;

    //Note: there are no color constants so I can't make all the parameters have defaults
    //time is full duration of effect
    //  Note: this effect is a lot faster than SpiralOut because the particles have to go twice the distance in half the time
    //  (1 time is probably better than the default 0.5)
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
        mm.startSpeed = 6.4f / time;
        volm.orbitalOffsetZ = 12.8f / time;

        ps.Play();
    }
}
