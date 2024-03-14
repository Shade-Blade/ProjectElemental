using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectScript_TripleShockwave : MonoBehaviour
{
    public GameObject frustum;
    public GameObject frustumB;
    public GameObject frustumC;
    //public MeshRenderer shockwave;
    //private MaterialPropertyBlock propertyBlock;

    public float offset;
    public float offsetXZ;
    public float offsetY;
    public float maxLifetime;
    private float lifetime = 0;

    public float maxXZScale;
    public float maxYScale;

    public float startXZScale;
    public float startYScale;

    public float easingPower;

    // Start is called before the first frame update
    void Start()
    {
        //propertyBlock = new MaterialPropertyBlock();
        frustum.transform.localScale = Vector3.up * startYScale + (Vector3.right + Vector3.forward) * startXZScale;
        frustumB.transform.localScale = Vector3.up * startYScale + (Vector3.right + Vector3.forward) * startXZScale;
        frustumC.transform.localScale = Vector3.up * startYScale + (Vector3.right + Vector3.forward) * startXZScale;
        frustum.transform.localPosition = Vector3.up * (startYScale);
        frustum.transform.localPosition = Vector3.up * (startYScale);
        frustum.transform.localPosition = Vector3.up * (startYScale);
        frustumB.SetActive(false);
        frustumC.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        float newLifetimeA = (lifetime / (maxLifetime - offset * 2));
        float newLifetimeB = ((lifetime - offset) / (maxLifetime - offset * 2));
        float newLifetimeC = ((lifetime - offset * 2) / (maxLifetime - offset * 2));

        //propertyBlockA.SetFloat("_Cutoff", 0.8f * newLifetime);
        //shockwave.SetPropertyBlock(propertyBlockA);

        float Easing(float f)
        {
            return 1 - Mathf.Pow(1 - f, easingPower);
        }

        //transform.localScale = Vector3.one * (startScale + (maxScale - startScale) * Easing(newLifetime));
        float YScaleA = Mathf.Lerp(startYScale, maxYScale, newLifetimeA); // (startYScale + (maxYScale - startYScale) * newLifetimeA);
        float XZScaleA = Mathf.Lerp(startXZScale, maxXZScale, Easing(newLifetimeA)); // (startXZScale + (maxXZScale - startXZScale) * Easing(newLifetimeA));
        frustum.transform.localScale = Vector3.up * YScaleA + (Vector3.right + Vector3.forward) * XZScaleA;
        frustum.transform.localPosition = Vector3.up * (YScaleA);

        if (newLifetimeB > 0)
        {
            frustumB.SetActive(true);
            float YScaleB = Mathf.Lerp(startYScale, offsetY + maxYScale, newLifetimeB);  //(startYScale + (offsetY + maxYScale - startYScale) * newLifetimeB);
            float XZScaleB = Mathf.Lerp(startXZScale, offsetXZ + maxXZScale, Easing(newLifetimeA)); //(startXZScale + (offsetXZ + maxXZScale - startXZScale) * Easing(newLifetimeB));
            frustumB.transform.localScale = Vector3.up * YScaleB + (Vector3.right + Vector3.forward) * XZScaleB;
            frustumB.transform.localPosition = Vector3.up * (YScaleB);
        }

        if (newLifetimeC > 0)
        {
            frustumC.SetActive(true);
            float YScaleC = Mathf.Lerp(startYScale, 2 * offsetY + maxYScale, newLifetimeC); // (startYScale + (2 * offsetY + maxYScale - startYScale) * newLifetimeC);
            float XZScaleC = Mathf.Lerp(startXZScale, 2 * offsetXZ + maxXZScale, Easing(newLifetimeC));  //(startXZScale + (2 * offsetXZ + maxXZScale - startXZScale) * Easing(newLifetimeC));
            frustumC.transform.localScale = Vector3.up * YScaleC + (Vector3.right + Vector3.forward) * XZScaleC;
            frustumC.transform.localPosition = Vector3.up * (YScaleC);
        }


        lifetime += Time.deltaTime;

        if (newLifetimeA > 1)
        {
            frustum.SetActive(false);
            //Destroy(frustum);
        }
        if (newLifetimeB > 1)
        {
            frustumB.SetActive(false);
        }
        if (newLifetimeC > 1)
        {
            frustumC.SetActive(false);
        }

        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
