using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_Trail : MonoBehaviour
{
    public float maxlifetime;
    public float baseWidth;
    private float lifetime;
    private float baseTime = 0;
    TrailRenderer tr;

    // Start is called before the first frame update
    void Start()
    {
        tr = GetComponent<TrailRenderer>();
        baseTime = tr.time;
        lifetime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        lifetime += Time.deltaTime;

        if (lifetime > maxlifetime) {
            Destroy(gameObject);
        }

        float lifetimefactor = lifetime / maxlifetime;
        lifetimefactor *= 2;
        if (lifetimefactor > 1)
        {
            lifetimefactor = 2 - lifetimefactor;
        }

        tr.time = lifetimefactor * baseTime;
        tr.widthMultiplier = lifetimefactor * baseWidth;
    }
}
