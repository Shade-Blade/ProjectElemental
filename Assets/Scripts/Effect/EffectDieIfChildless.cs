using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//prevent object leak where effect objects persist
public class EffectDieIfChildless : MonoBehaviour
{
    void Update()
    {
        if (transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
