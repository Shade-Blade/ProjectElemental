using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Swoosh : MonoBehaviour
{
    public GameObject swooshObject;
    public MeshRenderer swooshMesh;

    private MaterialPropertyBlock propertyBlock;

    public float lifetime;
    public float scaleMidpoint;  //XScale amount reaches maximum here (then goes down)
    public float rotationMidpoint;  //Rotation stops

    public float scaleMax = 1;

    public bool stopRotation;
    public bool stopped;

    private float time;

    public float startXScale;
    public float endXScale;
    public float startRotation;
    public float endRotation;

    // Start is called before the first frame update
    void Start()
    {       
        if (swooshMesh != null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        swooshObject.transform.localEulerAngles = Vector3.up * startRotation;
        propertyBlock.SetFloat("_XFactor", startXScale);
        swooshMesh.SetPropertyBlock(propertyBlock);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;

        if (time > lifetime)
        {
            Destroy(gameObject);
        }

        float timeB = MainManager.EasingQuadratic(time / lifetime, 1) * lifetime;

        if (!stopRotation)
        {
            swooshObject.transform.localEulerAngles = Vector3.up * Mathf.Lerp(startRotation, endRotation, Mathf.Clamp01(timeB / rotationMidpoint));
        }

        float sc = timeB / scaleMidpoint;
        if (sc > 1)
        {
            sc = ((lifetime - timeB) / (lifetime - scaleMidpoint));
        }

        propertyBlock.SetFloat("_XFactor", Mathf.Lerp(startXScale, endXScale, sc * scaleMax));
        swooshMesh.SetPropertyBlock(propertyBlock);
    }

    public void StopRotation()
    {
        stopRotation = true;
        StopRotationSword();
    }
    public void StopRotationSword()
    {
        if (stopped)
        {
            return;
        }
        stopped = true;

        float timeB = MainManager.EasingQuadratic(time / lifetime, 1) * lifetime;

        //Special thing for sword swoosh
        if (time > scaleMidpoint || timeB/lifetime > (2/3f))
        {

        } else
        {
            scaleMax = Mathf.Lerp(startXScale, endXScale, (timeB / (scaleMidpoint)));
            lifetime = MainManager.InverseEasingQuadratic((timeB/lifetime) * 1.5f, 1) * lifetime;
            scaleMidpoint = timeB;
        }
    }
}
