using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCookNPCScript : WorldNPCEntity
{
    int secondIndex;

    public override IEnumerator InteractCutscene()
    {
        secondIndex = -1;
        string[][] testTextFile = new string[10][];
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
        testTextFile[0][0] = "Cook NPC start text<prompt,Single item,1,Two items,2,Cancel,3,2>";
        testTextFile[1][0] = "Cook single item menu<itemMenu,overworld>";
        testTextFile[2][0] = "Cook double item menu A<itemMenu,overworld>";
        testTextFile[3][0] = "Cook double item menu B<dataget,arg,3><itemMenu,arg,overworldhighlightedblock>";
        testTextFile[4][0] = "Cook canceled<set,arg,1>";
        testTextFile[5][0] = "Cook <var,0>. Recipe works? <var,1><prompt,yes,1,no,0,1>";
        testTextFile[6][0] = "Cook <var,0> and <var,1>. Recipe works? <var,2><prompt,yes,1,no,0,1>";
        testTextFile[7][0] = "Cook result is <var,0> which is quality <var,1>";
        testTextFile[8][0] = "Single item cook no items";
        testTextFile[9][0] = "Double item cook no items";

        int state = 0;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case 3:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                state = 0;
                break;
            case 1:
                if (MainManager.Instance.playerData.itemInventory.Count < 1)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
                }
                break;
            case 2:
                if (MainManager.Instance.playerData.itemInventory.Count < 2)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 9, this));
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
                }
                break;
        }

        if (state == 0)
        {
            yield break;
        }

        Item itemA;
        int itemIndexA = -1;
        
        while (state == 1)
        {
            //yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);
            //Debug.Log(menuResult + " " + itemIndexA);
            //grab the item
            if (itemIndexA != -1)
            {
                itemA = MainManager.Instance.playerData.itemInventory[itemIndexA];
            } else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                yield break;
            }

            //precompute
            RecipeDataEntry rde = GlobalItemScript.Instance.GetRecipeDataFromIngredients(itemA.type);

            //Specialty thing
            //turn rde into MistakeType if wrong specialty
            //If the specialty + quest not done = add special cancel text

            //Do the menu thing
            string[] tempVars = new string[] { Item.GetName(itemA), (rde.quality != Item.ItemQuality.Mistake).ToString() };
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this, tempVars));

            int ynState = 0;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out ynState);

            if (ynState == 1)
            {
                MainManager.Instance.playerData.itemInventory.RemoveAt(itemIndexA);

                //Do the cooking thing
                //RDE is computed in an earlier step so I can check for valid recipes early

                Item.ItemType outputType = rde.result;
                Item.ItemQuality quality = rde.quality;

                Item resItem = new Item(outputType, itemA.modifier, Item.ItemOrigin.CookMagic, 0, 0);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this, tempResVars));

                yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                yield break;
            }

            //False = go back up through the loop
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
        }

        Item itemB;
        int itemIndexB = -1;

        while (state == 2)
        {
            //yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);
            //grab the item
            if (itemIndexA != -1)
            {
                itemA = MainManager.Instance.playerData.itemInventory[itemIndexA];
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                yield break;
            }

            secondIndex = itemIndexA;
            //Debug.Log("Second index is " + secondIndex);

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexB);
            //grab the item
            if (itemIndexB != -1)
            {
                itemB = MainManager.Instance.playerData.itemInventory[itemIndexB];
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                yield break;
            }

            //Debug.Log(menuResult + " " + itemIndexA + " " + itemIndexB);

            //precompute
            RecipeDataEntry rde = GlobalItemScript.Instance.GetRecipeDataFromIngredients(itemA.type, itemB.type);

            //Do the menu thing
            string[] tempVars = new string[] { Item.GetName(itemA), Item.GetName(itemB), (rde.quality != Item.ItemQuality.Mistake).ToString() };
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 6, this, tempVars));

            int ynState = 0;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out ynState);

            if (ynState == 1)
            {
                MainManager.Instance.playerData.itemInventory.RemoveAt(itemIndexA);
                MainManager.Instance.playerData.itemInventory.RemoveAt(itemIndexB);

                //Do the cooking thing
                //RDE is computed in an earlier step so I can check for valid recipes early

                Item.ItemType outputType = rde.result;
                Item.ItemQuality quality = rde.quality;

                Item resItem = new Item(outputType, itemA.modifier, Item.ItemOrigin.CookMagic, 0, 0);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this, tempResVars));

                yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                yield break;
            }


            /*
            //False = cancel
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
            yield break;
            */

            //False = go back up through the loop
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
        }
    }

    public override string RequestTextData(string request)
    {
        //Debug.Log("Requested " + request);

        if (request.Equals("3"))
        {
            //Debug.Log("Second index is " + secondIndex);
            List<Color?> colorList = new List<Color?>();
            for (int i = 0; i < MainManager.Instance.playerData.itemInventory.Count; i++)
            {
                colorList.Add(null);
            }

            if (secondIndex != -1)
            {
                colorList[secondIndex] = new Color(1, 1, 0.5f, 1f);
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
