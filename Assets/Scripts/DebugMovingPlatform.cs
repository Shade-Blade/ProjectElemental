using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DebugMovingPlatform : MonoBehaviour
{
    public Vector3[] positions;
    public float duration;
    public float haltTime;

    private float tempTime = 0;
    private float tempHaltTime = 0;

    public int targetIndex = 1;

    Vector3 moveVector = Vector3.zero;
    Vector3 angularVector = Vector3.zero;

    public Rigidbody rb;

    private void OnDrawGizmosSelected()
    {
        if (positions != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < positions.Length; i++)
            {
                if (i == positions.Length - 1)
                {
                    Gizmos.DrawLine(positions[i], positions[0]);
                }
                else
                {
                    Gizmos.DrawLine(positions[i], positions[i + 1]);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        moveVector = (positions[targetIndex] - transform.position) / duration;

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (tempHaltTime > 0)
        {
            tempHaltTime -= Time.fixedDeltaTime;
        } else
        {
            tempTime += Time.fixedDeltaTime;

            if (tempTime > duration)
            {
                transform.position = positions[targetIndex];
                tempHaltTime = haltTime;
                tempTime = 0;
                targetIndex++;
                targetIndex %= positions.Length;

                moveVector = (positions[targetIndex] - transform.position) / duration;

                angularVector = Vector3.zero;
                if (targetIndex == 2)
                {
                    angularVector = Vector3.up * ((2 * Mathf.PI) / duration);
                }
                if (targetIndex == 3)
                {
                    angularVector = Vector3.down * ((2 * Mathf.PI) / duration);
                }
            } else
            {
                //transform.position += moveVector * Time.fixedDeltaTime;
                //transform.eulerAngles = transform.eulerAngles + angularVector * (180f / (Mathf.PI)) * Time.fixedDeltaTime;
                rb.MovePosition(transform.position + moveVector * Time.fixedDeltaTime);
                rb.MoveRotation(Quaternion.Euler(transform.eulerAngles + angularVector * (180f / (Mathf.PI)) * Time.fixedDeltaTime));

                rb.velocity = moveVector;
                rb.angularVelocity = angularVector;
            }
        }
    }
}
