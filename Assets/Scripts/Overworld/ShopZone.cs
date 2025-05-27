using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopZone : WorldZone
{
    bool active = false;
    bool lastActive = false;

    public bool coinCounter;

    public GameObject descriptionO;
    public DescriptionBoxScript descriptionS;

    public List<ShopItem> activeShopItems;

    public void OnDrawGizmos()
    {
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.color = new Color(0.2f, 0.6f, 0.2f, 0.5f); //this is gray, could be anything
        Gizmos.DrawSphere(transform.position, 0.125f);

        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.5f, 0f), transform.position + new Vector3(-0.5f, 0.25f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.25f, 0f), transform.position + new Vector3(-0.5f, 0.25f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.25f, 0f), transform.position + new Vector3(-0.5f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0f, -0.5f, 0f), transform.position + new Vector3(0f, 0.5f, 0f));
    }

    public void FixedUpdate()
    {
        if (!lastActive && active)
        {
            if (coinCounter)
            {
                MainManager.Instance.ShowHUD();
            }
        }
        if (lastActive && !active)
        {
            if (coinCounter)
            {
                MainManager.Instance.HideHUD();
            }
            activeShopItems = null;
            if (descriptionO != null)
            {
                Destroy(descriptionO);
                descriptionO = null;
                descriptionS = null;
            }
        }

        lastActive = active;
        active = false;
    }

    public override void ProcessTrigger(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();
        if (wp != null)
        {
            active = true;
        }
    }

    public bool IsActive()
    {
        return lastActive || active;
    }

    public void AddShopItemToList(ShopItem si)
    {
        if (!IsActive())
        {
            return;
        }

        if (activeShopItems == null)
        {
            activeShopItems = new List<ShopItem>();
        }

        activeShopItems.Remove(si);

        activeShopItems.Insert(0, si);

        if (descriptionO == null)
        {
            descriptionO = Instantiate(MainManager.Instance.descriptionBoxBase, MainManager.Instance.Canvas.transform);
            descriptionS = descriptionO.GetComponent<DescriptionBoxScript>();
        }
        descriptionS.SetText(PickupUnion.GetDescription(activeShopItems[0].pickupUnion));
    }
    public void RemoveShopItemFromList(ShopItem si)
    {
        if (!IsActive())
        {
            return;
        }

        if (activeShopItems == null)
        {
            activeShopItems = new List<ShopItem>();
        }
        activeShopItems.Remove(si);

        if (activeShopItems.Count == 0 && descriptionO != null)
        {
            Destroy(descriptionO);
            descriptionO = null;
            descriptionS = null;
        }
    }
}
