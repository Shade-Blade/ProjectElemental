using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNPC_InventoryShopkeeper : WorldNPCEntity
{
    public List<ShopItem> inventory;

    public override void Start()
    {
        base.Start();
        ShopInventoryInit();

        //mutate here because start will only be called once
        for (int i = 0; i < inventory.Count; i++)
        {
            inventory[i].pickupUnion.Mutate();
        }
    }

    //Where any flag check should be (or any random inventory generation)
    public virtual void ShopInventoryInit()
    {

    }

    //Where any flag setting should be (i.e. set flag that prevents you from buying item again)
    public virtual void BuyItem(ShopItem item)
    {

    }

    public override IEnumerator InteractCutscene()
    {
        string[][] testTextFile = new string[13][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];
        testTextFile[7] = new string[1];
        testTextFile[8] = new string[1];
        testTextFile[9] = new string[1];
        testTextFile[10] = new string[1];
        testTextFile[11] = new string[1];
        testTextFile[12] = new string[1];

        /*
        testTextFile[0][0] = "Shop NPC main text<prompt,Buy Items,1,Sell Items,2,Cancel,3,-1>";
        testTextFile[1][0] = "Buy menu <dataget,arg,menu><genericmenu,arg,4>";
        testTextFile[2][0] = "Buy <var,0> for <var,1> <var,2>?<prompt,Yes,1,No,2,1>";
        testTextFile[3][0] = "You are poor";
        testTextFile[4][0] = "Inventory full";
        testTextFile[5][0] = "You bought a thing";
        testTextFile[6][0] = "Bye after buying";
        testTextFile[7][0] = "Bye";

        testTextFile[8][0] = "Selling menu<itemmenu,selling>";
        testTextFile[9][0] = "Selling menu again<itemmenu,selling>";
        testTextFile[10][0] = "Sell <var,0> for <var,1> coins?<prompt,Yes,1,No,2,1>";
        testTextFile[11][0] = "Sell no items";
        testTextFile[12][0] = "Bye after selling";
        */
        testTextFile[0][0] = wed.talkStrings[0];
        testTextFile[1][0] = wed.talkStrings[1];
        testTextFile[2][0] = wed.talkStrings[2];
        testTextFile[3][0] = wed.talkStrings[3];
        testTextFile[4][0] = wed.talkStrings[4];
        testTextFile[5][0] = wed.talkStrings[5];
        testTextFile[6][0] = wed.talkStrings[6];
        testTextFile[7][0] = wed.talkStrings[7];
        testTextFile[8][0] = wed.talkStrings[8];
        testTextFile[9][0] = wed.talkStrings[9];
        testTextFile[10][0] = wed.talkStrings[10];
        testTextFile[11][0] = wed.talkStrings[11];
        testTextFile[12][0] = wed.talkStrings[12];

        int state = 0;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case 1:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
                break;
            case 2:
                if (MainManager.Instance.playerData.itemInventory.Count == 0)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 11, this));
                    yield break;
                }
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                break;
            case -1:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                yield break;
        }

        int itemsBought = 0;
        while (state == 1)
        {
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out int itemIndex);

            //Debug.Log(menuResult + " " + itemIndex);

            if (itemIndex == -1)
            {
                if (itemsBought > 0)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 6, this));
                } else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                }
                yield break;
            }

            string[] vars = new string[] { PickupUnion.GetName(inventory[itemIndex].pickupUnion), inventory[itemIndex].cost + "", inventory[itemIndex].ConvertCurrencyToString() };

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this, vars));

            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out int ynstate);

            if (ynstate == 1)
            {
                bool canBuy = true;
                PlayerData pd = MainManager.Instance.playerData;
                switch (inventory[itemIndex].currency)
                {
                    case ShopItem.Currency.Coin:
                        canBuy = pd.coins >= inventory[itemIndex].cost;
                        break;
                    case ShopItem.Currency.Shard:
                        canBuy = pd.shards >= inventory[itemIndex].cost;
                        break;
                    case ShopItem.Currency.HP:
                        canBuy = false;
                        foreach (PlayerData.PlayerDataEntry pde in pd.party)
                        {
                            //only one needs to have the hp to pay the cost (but it gets paid by both still
                            if (pde.hp > inventory[itemIndex].cost)
                            {
                                canBuy = true;
                            }
                        }
                        break;
                    case ShopItem.Currency.EP:
                        canBuy = pd.ep >= inventory[itemIndex].cost;
                        break;
                    case ShopItem.Currency.SE:
                        canBuy = pd.se >= inventory[itemIndex].cost;
                        break;
                    case ShopItem.Currency.AstralToken:
                        canBuy = pd.astralTokens >= inventory[itemIndex].cost;
                        break;
                }

                //buy?
                if (!canBuy)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this, vars));
                } else
                {
                    if (inventory[itemIndex].pickupUnion.type == PickupUnion.PickupType.Item && MainManager.Instance.playerData.itemInventory.Count >= MainManager.Instance.playerData.GetMaxInventorySize())
                    {
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this, vars));
                    } else
                    {
                        //can buy stuff
                        //Pay
                        switch (inventory[itemIndex].currency)
                        {
                            case ShopItem.Currency.Coin:
                                pd.coins -= inventory[itemIndex].cost;
                                break;
                            case ShopItem.Currency.Shard:
                                pd.shards -= inventory[itemIndex].cost;
                                break;
                            case ShopItem.Currency.HP:
                                foreach (PlayerData.PlayerDataEntry pde in pd.party)
                                {
                                    //only one needs to have the hp to pay the cost (but it gets paid by both still)
                                    pde.hp -= inventory[itemIndex].cost;
                                    if (pde.hp < 1)
                                    {
                                        pde.hp = 1;
                                    }
                                }
                                break;
                            case ShopItem.Currency.EP:
                                pd.ep -= inventory[itemIndex].cost;
                                break;
                            case ShopItem.Currency.SE:
                                pd.se -= inventory[itemIndex].cost;
                                break;
                            case ShopItem.Currency.AstralToken:
                                pd.astralTokens -= inventory[itemIndex].cost;
                                break;
                        }

                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this, vars));
                        yield return StartCoroutine(MainManager.Instance.Pickup(inventory[itemIndex].pickupUnion.Copy()));
                        BuyItem(inventory[itemIndex]);
                        itemsBought++;
                    }
                }
            }

            //go back through loop
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
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
                if (itemsBought > 0)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 12, this));
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                }
                yield break;
            }

            //Do the menu thing
            string[] tempVars = new string[] { Item.GetName(item), Item.GetSellPrice(item).ToString() };
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 10, this, tempVars));


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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 12, this));
                yield break;
            }

            //Keep going?
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));

            itemsBought++;
        }

        yield return null;
    }

    public override string RequestTextData(string request)
    {
        if (request.Equals("buymenu"))
        {
            //build inventory shopkeeper table
            List<PickupUnion> items = new List<PickupUnion>();
            List<string> rightTextList = new List<string>();

            for (int i = 0; i < inventory.Count; i++)
            {
                items.Add(inventory[i].pickupUnion);
                rightTextList.Add(inventory[i].cost + " <" + inventory[i].currency + ">");
            }

            return GenericBoxMenu.PackMenuString(items, null, null, rightTextList);
        }

        return "nop";
    }
}
