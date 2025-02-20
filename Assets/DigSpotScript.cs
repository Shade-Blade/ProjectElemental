using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigSpotScript : WorldObject, IUndigTrigger
{
    public WorldCollectibleScript collectible;
    //Note: spawns with a collectible to make Item Sight follow it

    bool undig = false;

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

    public void Start()
    {
        collectible.intangible = true;
        collectible.antigravity = true;
        collectible.gameObject.SetActive(false);
    }

    public override void WorldUpdate()
    {
        if (collectible == null)
        {
            //Self destruct because there is nothing to undig
            Destroy(gameObject);
        }
    }

    public void Undig()
    {
        if (!undig)
        {
            undig = true;
            Vector3 velocity = Vector3.up * 6 + WorldPlayer.Instance.FacingVector() * 1.5f;
            MainManager.Instance.ThrowExistingCollectible(collectible, transform.position, velocity);
            //move the collectible up the hierarchy so it doesn't get destroyed
            collectible.gameObject.SetActive(true);
            collectible.transform.parent = transform.parent;
            Destroy(gameObject);
        }
    }
}
