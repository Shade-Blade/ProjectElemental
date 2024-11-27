using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class WorldNPC_Storage : WorldNPCEntity
{
    int itemSwapIndex;

    public override IEnumerator InteractCutscene()
    {
        itemSwapIndex = -1;
        string[][] testTextFile = new string[15][];
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
        testTextFile[13] = new string[1];
        testTextFile[14] = new string[1];
        testTextFile[0][0] = "Hi! I'm Palla. Me and my friends can store your items for you. I can also help you organize your inventory too if that's what you want.<next>If you have any items with us, you'll probably see me more often too, so don't worry about giving me items and then never seeing me again.<prompt,Store Items,1,Take Out Items,2,Swap Item Order,3,Swap Storage Order,4,Cancel,5,4>";
        testTextFile[1][0] = "Which one do you want to give me? You have <var,0>/<var,1> items in your inventory and you have <var,2>/<var,3> items in storage.<itemMenu,overworld>";
        testTextFile[2][0] = "Which one do you want back? You have <var,0>/<var,1> items in your inventory and you have <var,2>/<var,3> items in storage.<itemMenu,storage>";
        testTextFile[3][0] = "Okay! Which item do you want to swap?<itemMenu>";
        testTextFile[4][0] = "What do you want to swap that first item with?<dataget,arg,1><itemMenu,arg,overworldhighlightedblock>";
        testTextFile[5][0] = "Okay! Which item do you want to swap?<itemMenu,storage>";
        testTextFile[6][0] = "What do you want to swap that first item with?<dataget,arg,2><itemMenu,arg,storagehighlightedblock>";
        testTextFile[7][0] = "Oh, you don't have anything I can put in storage right now.";
        testTextFile[8][0] = "Sorry, but we've run out of room for your items.";
        testTextFile[9][0] = "Oh, it looks like you don't have anything stored with us right now.";
        testTextFile[10][0] = "Looks like you have your hands full, I can probably hold onto some of that stuff if you want me to.";
        testTextFile[11][0] = "Your pockets are empty right now, I guess that means they're perfectly organized.";
        testTextFile[12][0] = "Oh, you don't have anything in your storage right now.";
        testTextFile[13][0] = "Thanks for stopping by!";
        testTextFile[14][0] = "Thanks for stopping by!";

        int state = 0;

        PlayerData pd = MainManager.Instance.playerData;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case 5:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 13, this));
                state = 0;
                break;
            case 1:
                if (pd.itemInventory.Count < 1)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                    yield break;
                }
                if (pd.storageInventory.Count >= pd.maxStorageSize)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                    yield break;
                }
                string[] tempVarsA = new string[] { (pd.itemInventory.Count).ToString(),  pd.GetMaxInventorySize().ToString(), (pd.storageInventory.Count).ToString(), pd.maxStorageSize.ToString() };
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this, tempVarsA));
                break;
            case 2:
                if (pd.storageInventory.Count < 1)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 9, this));
                    yield break;
                }
                if (pd.itemInventory.Count >= pd.GetMaxInventorySize())
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 10, this));
                    yield break;
                }

                string[] tempVarsB = new string[] { (pd.itemInventory.Count).ToString(), pd.GetMaxInventorySize().ToString(), (pd.storageInventory.Count).ToString(), pd.maxStorageSize.ToString() };
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this, tempVarsB));
                break;
            case 3:
                if (pd.itemInventory.Count < 1)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 11, this));
                    yield break;
                }
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
                break;
            case 4:
                if (pd.storageInventory.Count < 1)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 12, this));
                    yield break;
                }
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this));
                break;
        }

        if (state == 0)
        {
            yield break;
        }


        while (state == 1)
        {
            int itemIndexA = -1;

            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);

            Item item = default;
            if (itemIndexA != -1)
            {
                item = pd.itemInventory[itemIndexA];

                pd.itemInventory.Remove(item);
                pd.storageInventory.Insert(0, item);
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 13, this));
                yield break;
            }


            //restart the loop?
            if (pd.itemInventory.Count < 1)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 14, this));
                yield break;
            }
            if (pd.storageInventory.Count >= pd.maxStorageSize)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 14, this));
                yield break;
            }
            string[] tempVarsA = new string[] { (pd.itemInventory.Count).ToString(), pd.GetMaxInventorySize().ToString(), (pd.storageInventory.Count).ToString(), pd.maxStorageSize.ToString() };
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this, tempVarsA));
        }


        while (state == 2)
        {
            int itemIndexA = -1;

            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);

            Item item = default;
            if (itemIndexA != -1)
            {
                item = pd.storageInventory[itemIndexA];

                pd.storageInventory.Remove(item);
                pd.itemInventory.Insert(0, item);
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 13, this));
                yield break;
            }


            //restart the loop?
            if (pd.storageInventory.Count < 1)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 14, this));
                yield break;
            }
            if (pd.itemInventory.Count >= pd.GetMaxInventorySize())
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 14, this));
                yield break;
            }

            string[] tempVarsB = new string[] { (pd.itemInventory.Count).ToString(), pd.GetMaxInventorySize().ToString(), (pd.storageInventory.Count).ToString(), pd.maxStorageSize.ToString() };
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this, tempVarsB));
        }


        while (state == 3)
        {
            int itemIndexA = -1;
            int itemIndexB = -1;

            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);

            Item itemA = default;
            Item itemB = default;
            if (itemIndexA != -1)
            {
                itemA = pd.itemInventory[itemIndexA];
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 13, this));
                yield break;
            }

            itemSwapIndex = itemIndexA;

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexB);

            if (itemIndexB != -1)
            {
                itemB = pd.itemInventory[itemIndexB];
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 13, this));
                yield break;
            }

            pd.itemInventory[itemIndexB] = itemA;
            pd.itemInventory[itemIndexA] = itemB;

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
        }

        while (state == 4)
        {
            int itemIndexA = -1;
            int itemIndexB = -1;

            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);

            Item itemA = default;
            Item itemB = default;
            if (itemIndexA != -1)
            {
                itemA = pd.storageInventory[itemIndexA];
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 13, this));
                yield break;
            }

            itemSwapIndex = itemIndexA;

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 6, this));
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexB);

            if (itemIndexB != -1)
            {
                itemB = pd.storageInventory[itemIndexB];
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 13, this));
                yield break;
            }

            pd.storageInventory[itemIndexB] = itemA;
            pd.storageInventory[itemIndexA] = itemB;

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this));
        }
    }

    public override string RequestTextData(string request)
    {
        //Debug.Log("Requested " + request);

        if (request.Equals("1"))
        {
            //Debug.Log("Second index is " + secondIndex);
            List<Color?> colorList = new List<Color?>();
            for (int i = 0; i < MainManager.Instance.playerData.itemInventory.Count; i++)
            {
                colorList.Add(null);
            }

            if (itemSwapIndex != -1)
            {
                colorList[itemSwapIndex] = new Color(1, 0.5f, 0.5f, 1f);
            }

            return MainManager.PackColorList(colorList);
        }

        if (request.Equals("2"))
        {
            //Debug.Log("Second index is " + secondIndex);
            List<Color?> colorList = new List<Color?>();
            for (int i = 0; i < MainManager.Instance.playerData.storageInventory.Count; i++)
            {
                colorList.Add(null);
            }

            if (itemSwapIndex != -1)
            {
                colorList[itemSwapIndex] = new Color(1, 0.5f, 0.5f, 1f);
            }

            return MainManager.PackColorList(colorList);
        }

        return "nop";
    }

    public override void SendTextData(string data)
    {
        Debug.Log("Received data: " + data);
    }
}
