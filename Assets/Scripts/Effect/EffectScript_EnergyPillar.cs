using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_EnergyPillar : MonoBehaviour
{
    public MeshRenderer pillar;

    private MaterialPropertyBlock propertyBlock;

    public float maxLifetime;
    public float lifetimeRatio;
    public float lifetimeRatioB;
    public float maxRotation;
    public float easingPower;
    private float lifetime = 0;

    // Start is called before the first frame update
    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetFloat("_CutoffOffset", -0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        float newLifetime = (lifetime / maxLifetime);
        float lifetimeRatioed = newLifetime * (1 / lifetimeRatio);

        if (newLifetime > 0.5f)
        {
            lifetimeRatioed = (1 - newLifetime) * (1 / lifetimeRatioB);
        }

        if (lifetimeRatioed > 1)
        {
            lifetimeRatioed = 1;
        }

        float Easing(float f, float p)
        {
            return 1 - Mathf.Pow(1 - f, p);
        }

        float remainingLifetime = 1 - newLifetime;

        if (pillar != null)
        {
            propertyBlock.SetFloat("_CutoffOffset", -0.2f + (0.7f * Easing(lifetimeRatioed, easingPower)));
            pillar.SetPropertyBlock(propertyBlock);
        }

        transform.eulerAngles = Vector3.up * newLifetime * maxRotation;

        lifetime += Time.deltaTime;

        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
