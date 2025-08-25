using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNPC_CBRCook : WorldNPCEntity
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

        string[][] testTextFile = new string[18][];
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
        testTextFile[15] = new string[1];
        testTextFile[16] = new string[1];
        testTextFile[17] = new string[1];

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
        int text_cookResultWeird = 11;
        int text_cookResultGood = 12;
        int text_noItemsSingle = 13;
        int text_noItemsDouble = 14;
        int text_noNonMistakes = 15;
        int text_tooPoor = 16;
        int text_cancelledLate = 17;

        testTextFile[0][0] = "Have you come for some world-class cooking? Everything I make is a masterpiece, and you just have to provide the ingredients.<next>It also costs 10 <coin> coins. That's a steal compared to what my skills are really worth. You have <const,coins> <coin> coins. <prompt,Cook 1 Ingredient,1,Cook 2 Ingredients,2,Cook By Result,3,Cancel,4,3>";
        testTextFile[1][0] = "What do you want me to cook?<itemMenu,overworld>";
        testTextFile[2][0] = "What's your first ingredient?<itemMenu,overworld>";
        testTextFile[3][0] = "What's your second ingredient?<dataget,arg,3><itemMenu,arg,overworldhighlightedblock>";

        testTextFile[4][0] = "Here's everything I can cook with what you have.<dataget,arg,4><genericmenu,arg,4>";

        testTextFile[5][0] = "Don't waste my time.<set,arg,1>";
        testTextFile[6][0] = "So you want me to cook your <var,0> into <var,1>?<prompt,Yes,1,No,0,1>";
        testTextFile[7][0] = "So you want me to cook your <var,0> and <var,1> into <var,2>?<prompt,Yes,1,No,0,1>";

        testTextFile[8][0] = "Cooking a <var,0> doesn't work. Can you give me something actually usable?";
        testTextFile[9][0] = "Putting <var,0> and <var,1> together obviously doesn't work. Try giving me better ingredients next time.";

        testTextFile[10][0] = "Here is the <var,0> you wanted. A masterpiece, like everything else I make.";
        testTextFile[11][0] = "Here is the <var,0> you wanted. The best I can do with your low quality ingredients.";
        testTextFile[12][0] = "Here is the <var,0> you wanted. Looks like you actually have good taste in ingredients.";
        testTextFile[13][0] = "Hmph. Nothing you have can be cooked alone.";
        testTextFile[14][0] = "Hmph. Nothing you have can be cooked together.";
        testTextFile[15][0] = "Are you mocking me? You clearly have nothing my culinary skill can salvage. Don't expect your money back.";

        testTextFile[16][0] = "You think I work for free? Come back here once you're ready to pay.";
        testTextFile[17][0] = "Great. You've wasted my time. Don't expect your money back.<set,arg,1>";

        int state = 0;

        BuildCBRList();

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_start, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        PlayerData pd = MainManager.Instance.playerData;

        int coinCost = 10;

        switch (state)
        {
            case 0:
            case 4:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelled, this));
                state = 0;
                break;
            case 1:
                if (pd.coins < coinCost)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_tooPoor, this));
                    yield break;
                }
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
                if (pd.coins < coinCost)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_tooPoor, this));
                    yield break;
                }
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
                if (pd.coins < coinCost)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_tooPoor, this));
                    yield break;
                }
                if (cookByResultList.Count < 1)
                {
                    //No refunds >:)
                    //Note that right now not having ingredients for single or double does not result in losing money either
                    pd.coins -= coinCost;
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

        pd.coins -= coinCost;

        //You pay him and you get nothing >:)
        //Torstrum is a real jerk
        if (cookByResultList.Count == 0)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_noNonMistakes, this));
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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelledLate, this));
                yield break;
            }

            //precompute
            RecipeDataEntry rde = GlobalItemScript.Instance.GetRecipeDataFromIngredients(itemA.type);

            //Specialty thing
            //turn rde into MistakeType if wrong specialty
            //If the specialty + quest not done = add special cancel text
            if (rde.quality == Item.ItemQuality.SpecialtyRecipe || rde.quality == Item.ItemQuality.SupremeRecipe)
            {
                rde.quality = Item.ItemQuality.Mistake;
                rde.result = Item.ItemType.Mistake;
            }

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

                Item resItem = new Item(outputType, itemA.modifier, Item.ItemOrigin.CookTorstrum, 0, 0);
                MainManager.Instance.SetRecipeFlag(resItem.type);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                switch (quality)
                {
                    case Item.ItemQuality.Recipe:
                    case Item.ItemQuality.GridFillRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResult, this, tempResVars));
                        break;
                    case Item.ItemQuality.WeirdRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResultWeird, this, tempResVars));
                        break;
                    case Item.ItemQuality.GoodRecipe:
                    case Item.ItemQuality.SpecialtyRecipe:
                    case Item.ItemQuality.SupremeRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResultGood, this, tempResVars));
                        break;
                }

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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelledLate, this));
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
                MainManager.Instance.playerData.itemInventory.Remove(itemA);
                MainManager.Instance.playerData.itemInventory.Remove(itemB);

                //Do the cooking thing
                //RDE is computed in an earlier step so I can check for valid recipes early

                Item.ItemType outputType = rde.result;
                Item.ItemQuality quality = rde.quality;

                Item resItem = new Item(outputType, GlobalItemScript.GetModifierFromRecipe(itemA.modifier, itemB.modifier), Item.ItemOrigin.CookTorstrum, 0, 0);

                //forcibly change the modifier to the correct one
                if (GlobalItemScript.ItemMultiTarget(resItem.type) && resItem.modifier == Item.ItemModifier.Spread)
                {
                    resItem.modifier = Item.ItemModifier.Focus;
                }
                if (!GlobalItemScript.ItemMultiTarget(resItem.type) && resItem.modifier == Item.ItemModifier.Focus)
                {
                    resItem.modifier = Item.ItemModifier.Spread;
                }

                MainManager.Instance.SetRecipeFlag(resItem.type);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                switch (quality)
                {
                    case Item.ItemQuality.Recipe:
                    case Item.ItemQuality.GridFillRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResult, this, tempResVars));
                        break;
                    case Item.ItemQuality.WeirdRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResultWeird, this, tempResVars));
                        break;
                    case Item.ItemQuality.GoodRecipe:
                    case Item.ItemQuality.SpecialtyRecipe:
                    case Item.ItemQuality.SupremeRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResultGood, this, tempResVars));
                        break;
                }

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

                    Item resItem = new Item(outputType, cbrEntry.result.modifier, Item.ItemOrigin.CookTorstrum, 0, 0);

                    //forcibly change the modifier to the correct one
                    if (GlobalItemScript.ItemMultiTarget(resItem.type) && resItem.modifier == Item.ItemModifier.Spread)
                    {
                        resItem.modifier = Item.ItemModifier.Focus;
                    }
                    if (!GlobalItemScript.ItemMultiTarget(resItem.type) && resItem.modifier == Item.ItemModifier.Focus)
                    {
                        resItem.modifier = Item.ItemModifier.Spread;
                    }

                    MainManager.Instance.SetRecipeFlag(resItem.type);
                    string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };

                    switch (quality)
                    {
                        case Item.ItemQuality.Recipe:
                        case Item.ItemQuality.GridFillRecipe:
                            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResult, this, tempResVars));
                            break;
                        case Item.ItemQuality.WeirdRecipe:
                            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResultWeird, this, tempResVars));
                            break;
                        case Item.ItemQuality.GoodRecipe:
                        case Item.ItemQuality.SpecialtyRecipe:
                        case Item.ItemQuality.SupremeRecipe:
                            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookResultGood, this, tempResVars));
                            break;
                    }

                    yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                    yield break;
                }
            }
            else
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancelledLate, this));
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
                cookByResultList.Add(new CBREntry(rdeTest, new Item(rdeTest.result, inv[i].modifier, Item.ItemOrigin.CookTorstrum, 0, 0), inv[i]));
            }

            for (int j = i + 1; j < MainManager.Instance.playerData.itemInventory.Count; j++)
            {
                RecipeDataEntry rdeTestB = GlobalItemScript.Instance.GetRecipeDataFromIngredientsBigTable(inv[i].type, inv[j].type);

                if (rdeTestB.quality != Item.ItemQuality.Mistake)
                {
                    doubleIngredientRecipeCount++;
                    Item resItem = new Item(rdeTestB.result, GlobalItemScript.GetModifierFromRecipe(inv[i].modifier, inv[j].modifier), Item.ItemOrigin.CookTorstrum, 0, 0);

                    //forcibly change the modifier to the correct one
                    if (GlobalItemScript.ItemMultiTarget(resItem.type) && resItem.modifier == Item.ItemModifier.Spread)
                    {
                        resItem.modifier = Item.ItemModifier.Focus;
                    }
                    if (!GlobalItemScript.ItemMultiTarget(resItem.type) && resItem.modifier == Item.ItemModifier.Focus)
                    {
                        resItem.modifier = Item.ItemModifier.Spread;
                    }

                    cookByResultList.Add(new CBREntry(rdeTestB, resItem, inv[i], inv[j]));

                }

            }
        }
        GlobalItemScript.Instance.UnloadBigRecipeDataTable();
    }
}
