using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_RadialFlow : MonoBehaviour
{
    public MeshRenderer frustum;

    private MaterialPropertyBlock propertyBlockA;

    public float maxLifetime;
    private float lifetime = 0;

    public float startCutoffA;
    public float endCutoffA;
    public float startCutoffB;
    public float endCutoffB;
    public float startCutoffC;
    public float endCutoffC;

    public float XZScale;       //minimum = 0.4 (or else something goes wrong and it doesn't encompass the camera anymore)

    private Vector3 targetPos;

    public GameObject frustumObj;

    // Start is called before the first frame update
    void Start()
    {
        propertyBlockA = new MaterialPropertyBlock();

        propertyBlockA.SetFloat("_Cutoff", startCutoffA);
        propertyBlockA.SetFloat("_CutoffB", startCutoffB);
        propertyBlockA.SetFloat("_CutoffC", startCutoffC);
        frustum.SetPropertyBlock(propertyBlockA);
        Orient();
    }

    void Orient()
    {
        //position it at the target location
        //(though we'll just assume we are at that target position I guess)
        targetPos = transform.position;

        //need to encompass the camera inside the thing
        //step 1: rotate

        Transform cameraTransform = MainManager.Instance.Camera.transform;

        //look rotation moves the z forward vector towards the target
        //but the frustum points upward
        transform.rotation = Quaternion.LookRotation(cameraTransform.position) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);

        //orient it with respect to the camera
        float scale = (transform.position - cameraTransform.position).magnitude / 2;

        frustumObj.transform.localPosition = Vector3.up * scale;
        frustumObj.transform.localScale = (Vector3.right + Vector3.forward) * (XZScale) + (Vector3.up * (scale));
        //
    }

    // Update is called once per frame
    void Update()
    {
        Orient();
        float newLifetime = (lifetime / maxLifetime);

        propertyBlockA.SetFloat("_Cutoff", startCutoffA + (endCutoffA - startCutoffA) * newLifetime);
        propertyBlockA.SetFloat("_CutoffB", startCutoffB + (endCutoffB - startCutoffB) * newLifetime);
        propertyBlockA.SetFloat("_CutoffC", startCutoffC + (endCutoffC - startCutoffC) * newLifetime);
        frustum.SetPropertyBlock(propertyBlockA);


        lifetime += Time.deltaTime;

        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
