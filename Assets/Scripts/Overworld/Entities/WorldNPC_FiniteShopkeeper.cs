using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Sells finite objects in the overworld (i.e. you buy it once and it becomes unavailable)
//Also has handling for shuffling the available inventory of items
//Use for badge shop and ribbon shop
//(Future note: needs a flag setting thing)
public class WorldNPC_FiniteShopkeeper : WorldNPC_Shopkeeper, IShopkeeperEntity
{
    public List<ShopItem> items;
    public bool canSell;

    public override void Start()
    {
        base.Start();

        //failsafe
        for (int i = 0; i < itemScripts.Count; i++)
        {
            itemScripts[i].shopkeeperEntity = this;
        }

        ShopInventoryInit();

        //mutate here because start will only be called once
        for (int i = 0; i < items.Count; i++)
        {
            items[i].pickupUnion.Mutate();
        }
        ItemInventoryReset();
    }

    //Where any flag check should be (or any random inventory generation)
    public virtual void ShopInventoryInit()
    {

    }

    //Where any flag setting should be (i.e. set flag that prevents you from buying item again)
    public virtual void BuyItem(ShopItem item)
    {

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

        /*
        testTextFile[0][0] = "Shopkeeper buy text: <var,0> costs <var,1> <var,2>. Buy it?<prompt,Yes,1,No,2,1>";
        testTextFile[1][0] = "You bought a thing";
        testTextFile[2][0] = "You didn't buy a thing";
        testTextFile[3][0] = "Too poor to buy a thing";
        testTextFile[4][0] = "Inventory too full to buy a thing";
        */

        testTextFile[0][0] = wed.talkStrings[9];
        testTextFile[1][0] = wed.talkStrings[10];
        testTextFile[2][0] = wed.talkStrings[11];
        testTextFile[3][0] = wed.talkStrings[12];
        testTextFile[4][0] = wed.talkStrings[13];

        string[] tempVars = new string[] { PickupUnion.GetName(sis.shopItem.pickupUnion), sis.shopItem.cost.ToString(), sis.shopItem.ConvertCurrencyToString() };

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
            case ShopItem.Currency.HP:
                canBuy = false;
                foreach (PlayerData.PlayerDataEntry pde in pd.party)
                {
                    //only one needs to have the hp to pay the cost (but it gets paid by both still
                    if (pde.hp > sis.shopItem.cost)
                    {
                        canBuy = true;
                    }
                }
                break;
            case ShopItem.Currency.EP:
                canBuy = pd.ep >= sis.shopItem.cost;
                break;
            case ShopItem.Currency.SE:
                canBuy = pd.se >= sis.shopItem.cost;
                break;
            case ShopItem.Currency.AstralToken:
                canBuy = pd.astralTokens >= sis.shopItem.cost;
                break;
        }

        if (!canBuy)
        {
            //too poor
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this, tempVars));
            yield break;
        }

        //inventory check
        if (sis.shopItem.pickupUnion.type == PickupUnion.PickupType.Item && MainManager.Instance.playerData.itemInventory.Count >= MainManager.Instance.playerData.GetMaxInventorySize() && !(sis.shopItem.pickupUnion.item.modifier == Item.ItemModifier.Void))
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
            case ShopItem.Currency.HP:
                foreach (PlayerData.PlayerDataEntry pde in pd.party)
                {
                    //only one needs to have the hp to pay the cost (but it gets paid by both still)
                    pde.hp -= sis.shopItem.cost;
                    if (pde.hp < 1)
                    {
                        pde.hp = 1;
                    }
                }
                break;
            case ShopItem.Currency.EP:
                pd.ep -= sis.shopItem.cost;
                break;
            case ShopItem.Currency.SE:
                pd.se -= sis.shopItem.cost;
                break;
            case ShopItem.Currency.AstralToken:
                pd.astralTokens -= sis.shopItem.cost;
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
        BuyItem(sis.shopItem);
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

        //Note: easy way to block selling is to not have the sell menu option in the prompt

        /*
        if (items.Count <= itemScripts.Count)
        {
            testTextFile[0][0] = "Shop NPC main text<prompt,Buy stuff,1,Sell stuff,2,Cancel,-1>";
        }
        else
        {
            testTextFile[0][0] = "Shop NPC main text<prompt,Buy stuff,1,Sell stuff,2,Reset Shop Inventory,3,Cancel,-1>";
        }
        testTextFile[1][0] = "Buy stuff by going up to the item and pressing <buttonsprite,a>";
        testTextFile[2][0] = "Can't buy stuff";
        testTextFile[3][0] = "Selling menu<itemmenu,selling>";
        testTextFile[4][0] = "Selling menu again<itemmenu,selling>";
        testTextFile[5][0] = "Sell <var,0> for <var,1> coins?<prompt,Yes,1,No,2,1>";
        testTextFile[6][0] = "Sell no items";
        testTextFile[7][0] = "Reset items";
        testTextFile[8][0] = "Shop bye";
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

        int state = 0;

        if (items.Count == 0 && !canSell)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
            yield break;
        }

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case -1:
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
                //should not be possible
                if (items.Count > 0)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
                }
                ItemInventoryReset();
                yield break;
            default:
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
