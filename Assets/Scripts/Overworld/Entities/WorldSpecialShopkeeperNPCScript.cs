using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpecialShopkeeperNPCScript : WorldShopkeeperNPCScript, IShopkeeperEntity
{
    public List<ShopItem> items;

    public override void Start()
    {
        base.Start();

        //failsafe
        for (int i = 0; i < itemScripts.Count; i++)
        {
            itemScripts[i].shopkeeperEntity = this;
        }

        //mutate here because start will only be called once
        for (int i = 0; i < items.Count; i++)
        {
            items[i].pickupUnion.Mutate();
        }
        ItemInventoryReset();
    }

    public void ItemInventoryReset()
    {
        items = MainManager.ShuffleList(items);

        for (int i = 0; i < itemScripts.Count; i++)
        {
            if (i <= items.Count - 1)
            {
                itemScripts[i].gameObject.SetActive(true);
                itemScripts[i].shopItem = items[i];
                itemScripts[i].Setup(items[i].pickupUnion);
            }
            else
            {
                itemScripts[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < itemScripts.Count; i++)
        {
            //forces a reset of the menus
            if (itemScripts[i].gameObject.activeSelf)
            {
                itemScripts[i].DestroyMenu();
            }
        }
    }

    public override void Interact(ShopItemScript sis)
    {
        StartCoroutine(InteractShopItem(sis));
    }

    public override IEnumerator InteractShopItem(ShopItemScript sis)
    {
        string[][] testTextFile = new string[5][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];

        testTextFile[0][0] = "Shopkeeper buy text: <var,0> costs <var,1> <var,2>. Buy it?<prompt,Yes,1,No,2,1>";
        testTextFile[1][0] = "You bought a thing";
        testTextFile[2][0] = "You didn't buy a thing";
        testTextFile[3][0] = "Too poor to buy a thing";
        testTextFile[4][0] = "Inventory too full to buy a thing";

        string[] tempVars = new string[] { PickupUnion.GetName(sis.shopItem.pickupUnion), sis.shopItem.cost.ToString(), sis.shopItem.currency.ToString() };

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this, tempVars));

        int state;
        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        if (state == 2)
        {
            //cancel
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this, tempVars));
            yield break;
        }

        PlayerData pd = MainManager.Instance.playerData;
        bool canBuy = true;
        switch (sis.shopItem.currency)
        {
            case ShopItem.Currency.Coin:
                canBuy = pd.coins >= sis.shopItem.cost;
                break;
            case ShopItem.Currency.Shard:
                canBuy = pd.shards >= sis.shopItem.cost;
                break;
        }

        if (!canBuy)
        {
            //too poor
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this, tempVars));
            yield break;
        }

        //inventory check
        if (sis.shopItem.pickupUnion.type == PickupUnion.PickupType.Item && MainManager.Instance.playerData.itemInventory.Count >= MainManager.Instance.playerData.GetMaxInventorySize())
        {
            //inventory full
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this, tempVars));
            yield break;
        }

        //Pay
        switch (sis.shopItem.currency)
        {
            case ShopItem.Currency.Coin:
                pd.coins -= sis.shopItem.cost;
                break;
            case ShopItem.Currency.Shard:
                pd.shards -= sis.shopItem.cost;
                break;
        }

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this, tempVars));

        //delete the thing
        //note that the pointers are shared so this works
        items.Remove(sis.shopItem);

        for (int i = 0; i < itemScripts.Count; i++)
        {
            //forces a reset of the menus
            if (itemScripts[i].gameObject.activeSelf)
            {
                itemScripts[i].DestroyMenu();
            }
        }
        sis.gameObject.SetActive(false);

        yield return StartCoroutine(MainManager.Instance.Pickup(sis.shopItem.pickupUnion.Copy()));
    }

    public override IEnumerator InteractCutscene()
    {

        string[][] testTextFile = new string[9][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];
        testTextFile[7] = new string[1];
        testTextFile[8] = new string[1];

        testTextFile[0][0] = "Shop NPC main text<prompt,Buy stuff,1,Sell stuff,2,Reset stuff,3,Cancel,4,3>";
        testTextFile[1][0] = "Buy stuff by going up to the item and pressing <buttonsprite,a>";
        testTextFile[2][0] = "Can't buy stuff";
        testTextFile[3][0] = "Selling menu<itemmenu,selling>";
        testTextFile[4][0] = "Selling menu again<itemmenu,selling>";
        testTextFile[5][0] = "Sell <var,0> for <var,1> coins?<prompt,Yes,1,No,2,1>";
        testTextFile[6][0] = "Sell no items";
        testTextFile[7][0] = "Reset items";
        testTextFile[8][0] = "Shop bye";

        int state = 0;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case 4:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                yield break;
            case 1:
                if (items.Count > 0)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
                }
                yield break;
            case 2:
                if (MainManager.Instance.playerData.itemInventory.Count == 0)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 6, this));
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
                }
                break;
            case 3:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                ItemInventoryReset();
                yield break;
        }

        while (state == 2)
        {
            //Select an item?
            int itemIndexA = -1;
            Item item = default;

            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);
            //Debug.Log(menuResult + " " + itemIndexA);
            //grab the item
            if (itemIndexA != -1)
            {
                item = MainManager.Instance.playerData.itemInventory[itemIndexA];
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                yield break;
            }

            //Do the menu thing
            string[] tempVars = new string[] { Item.GetName(item), Item.GetSellPrice(item).ToString() };
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this, tempVars));


            int ynState = 0;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out ynState);

            if (ynState == 1)
            {
                MainManager.Instance.playerData.itemInventory.Remove(item);
                MainManager.Instance.playerData.coins += Item.GetSellPrice(item);
                if (MainManager.Instance.playerData.coins > PlayerData.MAX_MONEY)
                {
                    MainManager.Instance.playerData.coins = PlayerData.MAX_MONEY;
                }

                //nothing else to do here
            }

            if (MainManager.Instance.playerData.itemInventory.Count == 0)
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                yield break;
            }

            //Keep going?
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
        }
    }
}
