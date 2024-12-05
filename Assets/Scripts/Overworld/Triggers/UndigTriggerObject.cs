using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndigTriggerObject : MonoBehaviour, IUndigTrigger
{
    bool undig = false;
    public int coinCount;

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, -0.5f, 0f), transform.position + new Vector3(0, 0f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0f, 0f), transform.position + new Vector3(0.5f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.5f, 0f), transform.position + new Vector3(-0.5f, -0.5f, 0f));

        Gizmos.DrawLine(transform.position + new Vector3(-0.2f, -0.5f, 0f), transform.position + new Vector3(0, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0.5f, 0f), transform.position + new Vector3(0.2f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.2f, -0.5f, 0f), transform.position + new Vector3(-0.2f, -0.5f, 0f));
    }

    public void Undig()
    {
        if (!undig)
        {
            undig = true;
            MainManager.Instance.DropCoins(coinCount, transform.position, Vector3.up * 6, 3);
            Destroy(gameObject);
        }
    }
}
