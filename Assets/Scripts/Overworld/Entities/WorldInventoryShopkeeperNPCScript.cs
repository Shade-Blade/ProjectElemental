using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInventoryShopkeeperNPCScript : WorldNPCEntity
{
    public List<ShopItem> inventory;

    public override IEnumerator InteractCutscene()
    {
        string[][] testTextFile = new string[8][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];
        testTextFile[7] = new string[1];

        testTextFile[0][0] = "Shop NPC main text<prompt,Buy stuff,1,Cancel,2,1>";
        testTextFile[1][0] = "Buy menu <dataget,arg,4><genericmenu,arg,4>";
        testTextFile[2][0] = "Buy <var,0> for <var,1> coins?<prompt,Yes,1,No,2,1>";
        testTextFile[3][0] = "You are poor";
        testTextFile[4][0] = "Inventory full";
        testTextFile[5][0] = "You bought a thing";
        testTextFile[6][0] = "Bye after buying";
        testTextFile[7][0] = "Bye";

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

            string[] vars = new string[] { PickupUnion.GetName(inventory[itemIndex].pickupUnion), inventory[itemIndex].cost + "" };

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this, vars));

            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out int ynstate);

            if (ynstate == 1)
            {
                //buy?
                if (MainManager.Instance.playerData.coins < inventory[itemIndex].cost)
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
                        MainManager.Instance.playerData.coins -= inventory[itemIndex].cost;
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this, vars));
                        yield return StartCoroutine(MainManager.Instance.Pickup(inventory[itemIndex].pickupUnion.Copy()));
                        itemsBought++;
                    }
                }
            }

            //go back through loop
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
        }



        yield return null;
    }

    public override string RequestTextData(string request)
    {
        if (request.Equals("4"))
        {
            //Build Cook by result menu

            //arg list = name, right, usage, desc,

            List<PickupUnion> items = new List<PickupUnion>();
            List<string> rightTextList = new List<string>();

            for (int i = 0; i < inventory.Count; i++)
            {
                items.Add(inventory[i].pickupUnion);
                rightTextList.Add(inventory[i].cost + " <coin>");
            }

            return GenericBoxMenu.PackMenuString(items, null, null, rightTextList);
        }

        return "nop";
    }
}
