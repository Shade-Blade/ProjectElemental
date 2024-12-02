using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class ShopItem
{
    public PickupUnion pickupUnion;
    public int cost;
    public enum Currency
    {
        Coin,
        Shard,
        HP,
        EP,
        SE,
        AstralToken,
    }
    public Currency currency;
    
    public ShopItem(PickupUnion p_pickupUnion, int p_cost, Currency p_currency)
    {
        pickupUnion = p_pickupUnion;
        cost = p_cost;
        currency = p_currency;
    }

    public static int ConvertCost(Currency target, int cost)
    {
        switch (target)
        {
            case Currency.Coin:
                return cost;
            case Currency.Shard:
                return Mathf.CeilToInt(cost / 75f);
            case Currency.HP:
                //1 coin = 1 hp
                //50 coins = 50 hp
                return Mathf.CeilToInt(cost / 1.5f);
            case Currency.EP:
                return Mathf.CeilToInt(cost / 1f);
            case Currency.SE:
                return Mathf.CeilToInt(cost / 1f);
            case Currency.AstralToken:  //Permanently losing a level up is a massive cost (But I also chose this so that almost everything that costs Astral Tokens only costs 1)
                return Mathf.CeilToInt(cost / 600f);
        }
        return cost;
    }

    //Todo: reference CommonText lines
    public string ConvertCurrencyToString()
    {
        switch (currency)
        {
            case Currency.Coin:
                return cost == 1 ? "coin" : "coins";
            case Currency.Shard:
                return cost == 1 ? "shard" : "shards";
            case Currency.HP:
                return "HP";
            case Currency.EP:
                return "EP";
            case Currency.SE:
                return "SE";
            case Currency.AstralToken:
                return cost == 1 ? "Astral Token" : "Astral Tokens";
        }
        return "somethings";
    }

    public static string CostToString(Currency currency, int cost)
    {
        switch (currency)
        {
            case Currency.Coin:
                return cost.ToString() + " " + (cost == 1 ? "coin" : "coins");
            case Currency.Shard:
                return cost.ToString() + " " + (cost == 1 ? "shard" : "shards");
            case Currency.HP:
                return cost.ToString() + " HP";
            case Currency.EP:
                return cost.ToString() + " EP";
            case Currency.SE:
                return cost.ToString() + " SE";
            case Currency.AstralToken:
                return cost.ToString() + " " + (cost == 1 ? "Astral Token" : "Astral Tokens");
        }
        return cost.ToString() + "somethings";
    }

    //encounter level to difficulty level
    //(sum level - 12)/4

    //cost to encounter level
    //cost = 2f * sum level
}

public class ShopItemScript : InteractTrigger
{
    public ShopItem shopItem;

    public SpriteRenderer sprite;
    public bool setup = false;

    public InteractTrigger it;

    public ShopZone sz;
    public bool menuActive = false;
    public bool selectedItem = false;

    //note: shopkeeperEntity has a failsafe to make this be set correctly
    public WorldNPC_Shopkeeper shopkeeperEntity;

    public Transform menuPoint;
    public WorldspaceShopEntryScript menu;


    // Start is called before the first frame update
    void Start()
    {
        if (!setup)
        {
            shopItem.pickupUnion.Mutate();
            Setup(shopItem.pickupUnion);
        }
    }

    public void Setup(PickupUnion pu)
    {
        shopItem.pickupUnion = pu.Copy();
        transform.localScale = Vector3.one * PickupUnion.GetScale(pu);
        sprite.sprite = PickupUnion.GetSprite(pu);
        sprite.material = PickupUnion.GetSpriteMaterial(pu);
        setup = true;
    }

    public void Update()
    {
        menuActive = sz.IsActive();

        if (menuActive && (menu == null))
        {
            //setup
            menu = WorldspaceShopEntryScript.BuildMenu(menuPoint, GetCostString(), "<outline,#ffffff>" + PickupUnion.GetName(shopItem.pickupUnion) + "</outline>");
        }

        //force this
        if (sz.IsActive())
        {
            if (!selectedItem && it.active)
            {
                selectedItem = true;
                sz.AddShopItemToList(shopItem);
                if (menu != null)
                {
                    menu.ShowName();
                }
            }
        }
        if (selectedItem && !it.active)
        {
            selectedItem = false;
            sz.RemoveShopItemFromList(shopItem);
            if (menu != null)
            {
                menu.HideName();
            }
        }

        if ((menu != null) && !menuActive)
        {
            //unsetup
            Destroy(menu.gameObject);
            menu = null;
        }
    }

    public override void Interact()
    {
        shopkeeperEntity.Interact(this);
    }

    public void DestroyMenu()
    {
        if (menu != null)
        {
            Destroy(menu.gameObject);
        }
        menu = null;
    }

    public string GetCostString()
    {
        return "<outline,#ffffff>" + shopItem.cost + "</outline> " + "<" + shopItem.currency.ToString() + ">";
    }
}
