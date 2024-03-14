using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectScript_SingleShockwave : MonoBehaviour
{
    public GameObject frustum;
    //public MeshRenderer shockwave;
    //private MaterialPropertyBlock propertyBlock;

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
        frustum.transform.localPosition = Vector3.up * (startYScale);
    }

    // Update is called once per frame
    void Update()
    {
        float newLifetime = (lifetime / maxLifetime);
        float remainingLifetime = 1 - newLifetime;

        //propertyBlockA.SetFloat("_Cutoff", 0.8f * newLifetime);
        //shockwave.SetPropertyBlock(propertyBlockA);

        float Easing(float f)
        {
            return 1 - Mathf.Pow(1 - f, easingPower);
        }

        //transform.localScale = Vector3.one * (startScale + (maxScale - startScale) * Easing(newLifetime));
        float YScale = Mathf.Lerp(startYScale, maxYScale, newLifetime); // (startYScale + (maxYScale - startYScale) * newLifetime);
        float XZScale = Mathf.Lerp(startXZScale, maxXZScale, Easing(newLifetime)); // (startXZScale + (maxXZScale - startXZScale) * Easing(newLifetime));
        frustum.transform.localScale = Vector3.up * YScale + (Vector3.right + Vector3.forward) * XZScale;
        frustum.transform.localPosition = Vector3.up * (YScale);

        lifetime += Time.deltaTime;

        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
