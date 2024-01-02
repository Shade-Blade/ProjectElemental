using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_SphereExplosion : MonoBehaviour
{
    public MeshRenderer outerSphere;
    public MeshRenderer innerSphere;

    private MaterialPropertyBlock propertyBlockA;
    private MaterialPropertyBlock propertyBlockB;

    public float maxLifetime;
    private float lifetime = 0;

    public float maxScale;
    public float startScale;
    public float easingPower;
    public float cutoffPower;

    // Start is called before the first frame update
    void Start()
    {
        if (outerSphere != null)
        {
            propertyBlockA = new MaterialPropertyBlock();
        }

        if (innerSphere != null)
        {
            propertyBlockB = new MaterialPropertyBlock();
        }
        transform.localScale = Vector3.one * startScale;
    }

    // Update is called once per frame
    void Update()
    {
        float newLifetime = (lifetime / maxLifetime);
        float remainingLifetime = 1 - newLifetime;

        if (outerSphere != null)
        {
            propertyBlockA.SetFloat("_Cutoff", 0.8f * Easing(newLifetime, cutoffPower));
            outerSphere.SetPropertyBlock(propertyBlockA);
        }
        if (innerSphere != null)
        {
            propertyBlockB.SetFloat("_Cutoff", 1 * Easing(newLifetime, cutoffPower));
            innerSphere.SetPropertyBlock(propertyBlockB);
        }

        float Easing(float f, float p)
        {
            return 1 - Mathf.Pow(1 - f, p);
        }

        transform.localScale = Vector3.one * (startScale + (maxScale - startScale) * Easing(newLifetime, easingPower));

        lifetime += Time.deltaTime;

        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
