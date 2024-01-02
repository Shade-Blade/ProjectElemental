using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class TestCBRCookNPCScript : WorldNPCEntity
{
    int secondIndex;

    public class CBREntry
    {
        public RecipeDataEntry rde;
        public Item result;
        public Item ingredientA;
        public Item ingredientB;

        public CBREntry(RecipeDataEntry rde, Item result, Item ingredientA, Item ingredientB = default)
        {
            this.rde = rde;
            this.result = result;
            this.ingredientA = ingredientA;
            this.ingredientB = ingredientB;
        }
    }

    List<CBREntry> cookByResultList;
    int singleIngredientRecipeCount = 0;
    int doubleIngredientRecipeCount = 0;

    public override IEnumerator InteractCutscene()
    {
        secondIndex = -1;
        cookByResultList = null;
        singleIngredientRecipeCount = 0;
        doubleIngredientRecipeCount = 0;

        string[][] testTextFile = new string[14][];
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

        int text_start = 0;
        int text_singleItemStart = 1;
        int text_doubleItemStartA = 2;
        int text_doubleItemStartB = 3;
        int text_cookByResult = 4;
        int text_cancelled = 5;
        int text_cookSingle = 6;
        int text_cookDouble = 7;
        int text_cookSingleFail = 8;
        int text_cookDoubleFail = 9;
        int text_cookResult = 10;
        int text_noItemsSingle = 11;
        int text_noItemsDouble = 12;
        int text_noNonMistakes = 13;

        testTextFile[0][0] = "Cook NPC start text<prompt,Single item,1,Two items,2,Cook By Result,3,Cancel,4,3>";
        testTextFile[1][0] = "Cook single item menu<itemMenu,overworld>";
        testTextFile[2][0] = "Cook double item menu A<itemMenu,overworld>";
        testTextFile[3][0] = "Cook double item menu B<dataget,arg,3><itemMenu,arg,overworldhighlightedblock>";

        testTextFile[4][0] = "Cook by result<dataget,arg,4><genericmenu,arg,4>";

        testTextFile[5][0] = "Cook canceled<set,arg,1>";
        testTextFile[6][0] = "Cook <var,0>. Recipe creates <var,1><prompt,yes,1,no,0,1>";
        testTextFile[7][0] = "Cook <var,0> and <var,1>. Recipe creates <var,2><prompt,yes,1,no,0,1>";

        testTextFile[8][0] = "Cook <var,0>. Recipe doesn't work";
        testTextFile[9][0] = "Cook <var,0> and <var,1>. Recipe doesn't work";

        testTextFile[10][0] = "Cook result is <var,0> which is quality <var,1>";
        testTextFile[11][0] = "Single item cook no items";
        testTextFile[12][0] = "Double item cook no items";
        testTextFile[13][0] = "No non mistake recipes possible";

        int state = 0;

        BuildCBRList();

        if (cookByResultList.Count == 0)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_noNonMistakes, this));
            yield break;
        }

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_start, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case 4:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelled, this));
                state = 0;
                break;
            case 1:
                if (MainManager.Instance.playerData.itemInventory.Count < 1 || singleIngredientRecipeCount == 0)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_noItemsSingle, this));
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_singleItemStart, this));
                }
                break;
            case 2:
                if (MainManager.Instance.playerData.itemInventory.Count < 2 || doubleIngredientRecipeCount == 0)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_noItemsDouble, this));
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_doubleItemStartA, this));
                }
                break;
            case 3:
                if (cookByResultList.Count < 1)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_noNonMistakes, this));
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookByResult, this));
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
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelled, this));
                yield break;
            }

            //precompute
            RecipeDataEntry rde = GlobalItemScript.Instance.GetRecipeDataFromIngredients(itemA.type);

            //Specialty thing
            //turn rde into MistakeType if wrong specialty
            //If the specialty + quest not done = add special cancel text

            //Do the menu thing
            string[] tempVars = new string[] { Item.GetName(itemA), Item.GetName(rde.result) };
            if (rde.quality != Item.ItemQuality.Mistake)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookSingle, this, tempVars));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookSingleFail, this, tempVars));
            }


            int ynState = 0;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out ynState);

            if (rde.quality != Item.ItemQuality.Mistake && ynState == 1)
            {
                MainManager.Instance.playerData.itemInventory.RemoveAt(itemIndexA);

                //Do the cooking thing
                //RDE is computed in an earlier step so I can check for valid recipes early

                Item.ItemType outputType = rde.result;
                Item.ItemQuality quality = rde.quality;

                Item resItem = new Item(outputType, itemA.modifier, Item.ItemOrigin.CookMagic, 0, 0);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResult, this, tempResVars));

                yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                yield break;
            }

            //False = go back up through the loop
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_singleItemStart, this));
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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelled, this));
                yield break;
            }

            secondIndex = itemIndexA;
            //Debug.Log("Second index is " + secondIndex);

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_doubleItemStartB, this));
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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelled, this));
                yield break;
            }

            //Debug.Log(menuResult + " " + itemIndexA + " " + itemIndexB);

            //precompute
            RecipeDataEntry rde = GlobalItemScript.Instance.GetRecipeDataFromIngredients(itemA.type, itemB.type);

            //Do the menu thing
            string[] tempVars = new string[] { Item.GetName(itemA), Item.GetName(itemB), Item.GetName(rde.result) };
            if (rde.quality != Item.ItemQuality.Mistake)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookDouble, this, tempVars));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookDoubleFail, this, tempVars));
            }

            int ynState = 0;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out ynState);

            if (rde.quality != Item.ItemQuality.Mistake && ynState == 1)
            {
                MainManager.Instance.playerData.itemInventory.RemoveAt(itemIndexA);
                MainManager.Instance.playerData.itemInventory.RemoveAt(itemIndexB);

                //Do the cooking thing
                //RDE is computed in an earlier step so I can check for valid recipes early

                Item.ItemType outputType = rde.result;
                Item.ItemQuality quality = rde.quality;

                Item resItem = new Item(outputType, itemA.modifier, Item.ItemOrigin.CookCrystal, 0, 0);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResult, this, tempResVars));

                yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                yield break;
            }


            /*
            //False = cancel
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelled, this));
            yield break;
            */

            //False = go back up through the loop
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_doubleItemStartA, this));
        }

        if (state == 3)
        {
            //Cook by result menu
            menuResult = MainManager.Instance.lastTextboxMenuResult;

            //note: needed to escape the name so it doesn't mess up what arg1 is
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);

            //grab the item
            if (itemIndexA != -1)
            {
                CBREntry cbrEntry = cookByResultList[itemIndexA];
                if (cbrEntry.ingredientB.type == Item.ItemType.None)
                {
                    string[] tempVars = new string[] { Item.GetName(cbrEntry.ingredientA), Item.GetName(cbrEntry.result) };
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookSingle, this, tempVars));
                }
                else
                {
                    string[] tempVars = new string[] { Item.GetName(cbrEntry.ingredientA), Item.GetName(cbrEntry.ingredientB), Item.GetName(cbrEntry.result) };
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookDouble, this, tempVars));
                }

                int ynState = 0;
                menuResult = MainManager.Instance.lastTextboxMenuResult;
                int.TryParse(menuResult, out ynState);

                if (ynState == 1)
                {
                    MainManager.Instance.playerData.itemInventory.Remove(cbrEntry.ingredientA);
                    if (cbrEntry.ingredientB.type != Item.ItemType.None)
                    {
                        MainManager.Instance.playerData.itemInventory.Remove(cbrEntry.ingredientB);
                    }

                    //Do the cooking thing
                    //RDE is computed in an earlier step so I can check for valid recipes early

                    Item.ItemType outputType = cbrEntry.rde.result;
                    Item.ItemQuality quality = cbrEntry.rde.quality;

                    Item resItem = new Item(outputType, cbrEntry.ingredientA.modifier, Item.ItemOrigin.CookCrystal, 0, 0);
                    string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResult, this, tempResVars));

                    yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                    yield break;
                }
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelled, this));
                yield break;
            }
        }
    }

    public override string RequestTextData(string request)
    {
        if (request.Equals("4"))
        {
            //Build Cook by result menu

            //arg list = name, right, usage, desc,

            List<Item> items = new List<Item>();
            List<string> rightTextList = new List<string>();

            for (int i = 0; i < cookByResultList.Count; i++)
            {
                items.Add(cookByResultList[i].result);
                if (cookByResultList[i].ingredientB.type == Item.ItemType.None)
                {
                    rightTextList.Add("<larrow> " + Item.GetSpriteString(cookByResultList[i].ingredientA));
                }
                else
                {
                    rightTextList.Add("<larrow> " + Item.GetSpriteString(cookByResultList[i].ingredientA) + " + " + Item.GetSpriteString(cookByResultList[i].ingredientB));
                }
            }

            return GenericBoxMenu.PackMenuString(items, null, null, rightTextList);
        }

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

    public void BuildCBRList()
    {
        cookByResultList = new List<CBREntry>();
        singleIngredientRecipeCount = 0;
        doubleIngredientRecipeCount = 0;

        List<Item> inv = MainManager.Instance.playerData.itemInventory;

        for (int i = 0; i < inv.Count; i++)
        {
            //single ingredient recipe

            RecipeDataEntry rdeTest = GlobalItemScript.Instance.GetRecipeDataFromIngredientsBigTable(inv[i].type);

            if (rdeTest.quality != Item.ItemQuality.Mistake)
            {
                singleIngredientRecipeCount++;
                cookByResultList.Add(new CBREntry(rdeTest, new Item(rdeTest.result, inv[i].modifier, Item.ItemOrigin.CookCrystal, 0, 0), inv[i]));
            }

            for (int j = i + 1; j < MainManager.Instance.playerData.itemInventory.Count; j++)
            {
                RecipeDataEntry rdeTestB = GlobalItemScript.Instance.GetRecipeDataFromIngredientsBigTable(inv[i].type, inv[j].type);

                if (rdeTestB.quality != Item.ItemQuality.Mistake)
                {
                    doubleIngredientRecipeCount++;
                    cookByResultList.Add(new CBREntry(rdeTestB, new Item(rdeTestB.result, inv[i].modifier, Item.ItemOrigin.CookCrystal, 0, 0), inv[i], inv[j]));
                }

            }
        }
        GlobalItemScript.Instance.UnloadBigRecipeDataTable();
    }
}
