using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_HelixUp : MonoBehaviour
{
    public ParticleSystem ps;

    //Note: there are no color constants so I can't make all the parameters have defaults
    //time is duration
    //scale: normal is roughly 2 units wide (scale changes that but keeps width)
    //real count may be 1 off
    public void Setup(Color color, float time = 1f, int count = 8, float rotation = 0, float width = 0.15f, float scale = 1)
    {
        //particle system code is sus
        ParticleSystem.MainModule mm = ps.main;
        ParticleSystem.ShapeModule sm = ps.shape;
        ParticleSystem.EmissionModule emm = ps.emission;
        ParticleSystem.VelocityOverLifetimeModule volm = ps.velocityOverLifetime;

        transform.localScale = Vector3.one * scale;

        mm.startColor = color;
        mm.startSize = width / scale;

        sm.rotation = Vector3.right * 90 + Vector3.up * rotation;

        emm.rateOverTime = count / (time / 2);

        mm.startLifetime = time / 2;
        mm.duration = time / 2;
        mm.startSpeed = 0;

        volm.y = 4 / time;
        volm.orbitalOffsetY = 12 / time;

        ps.Play();
    }
}
