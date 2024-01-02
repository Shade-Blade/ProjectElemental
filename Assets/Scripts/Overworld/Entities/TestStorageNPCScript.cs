using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class TestStorageNPCScript : WorldNPCEntity
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
        testTextFile[0][0] = "Storage NPC start text<prompt,Store stuff,1,Take out stuff,2,Swap Item Order,3,Swap Storage Order,4,Cancel,5,4>";
        testTextFile[1][0] = "Choose thing to store. Inventory: <var,0>/<var,1>. Storage: <var,2>/<var,3><itemMenu,overworld>";
        testTextFile[2][0] = "Choose thing to take out. Inventory: <var,0>/<var,1>. Storage: <var,2>/<var,3><itemMenu,storage>";
        testTextFile[3][0] = "Choose item to swap<itemMenu>";
        testTextFile[4][0] = "Choose item to swap that first item with<dataget,arg,1><itemMenu,arg,overworldhighlightedblock>";
        testTextFile[5][0] = "Choose item to swap<itemMenu,storage>";
        testTextFile[6][0] = "Choose item to swap that first item with<dataget,arg,2><itemMenu,arg,storagehighlightedblock>";
        testTextFile[7][0] = "Nothing to store";
        testTextFile[8][0] = "Storage full";
        testTextFile[9][0] = "Nothing to take out";
        testTextFile[10][0] = "Inventory full";
        testTextFile[11][0] = "Nothing to swap (items)";
        testTextFile[12][0] = "Nothing to swap (storage)";
        testTextFile[13][0] = "Storage cancel";
        testTextFile[14][0] = "Storage done";

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
