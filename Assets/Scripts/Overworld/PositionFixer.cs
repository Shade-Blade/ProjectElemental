using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionFixer : MonoBehaviour
{
    public Transform fixPoint;
    // Update is called once per frame
    void Update()
    {
        fixPoint.position = transform.position;
        fixPoint.rotation = transform.rotation;
    }

    void FixedUpdate()
    {
        fixPoint.position = transform.position;
        fixPoint.rotation = transform.rotation;
    }
}