using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNPC_SpeedCook : WorldNPCEntity
{
    List<Item> itemList;
    List<int> indexList;
    List<Item> pairItemList;
    List<int> pairIndexList;

    public void ResetState()
    {
        //cleanse the state
        itemList = null;
        indexList = null;
        pairItemList = null;
        pairIndexList = null;
    }

    public override IEnumerator InteractCutscene()
    {
        ResetState();

        string[][] testTextFile = new string[11][];
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
        testTextFile[0][0] = "I've got a big cooking pot here, hungry for ingredients! Just give me everything you wanna cook, and I'll have it done in a flash!<next>Make sure you give me the right recipes, or they'll go bad.<prompt,1 Ingredient Recipes,1,2 Ingredient Recipes,2,Cancel,3,2>";
        testTextFile[1][0] = "So we're cooking items one at a time? (Use <buttonsprite,z> to end selection, <buttonsprite,b> to cancel.) <dataget,arg,1><itemMenu,arg,overworldhighlightedblockz>";
        testTextFile[2][0] = "So we're cooking items two at a time? (Use <buttonsprite,z> to end selection, <buttonsprite,b> to cancel.)<dataget,arg,2><itemMenu,arg,overworldhighlightedblockz>";
        testTextFile[3][0] = "Now give me another item to pair with that one. (Select another item then use <buttonsprite,z> to end selection, <buttonsprite,b> to cancel.)<dataget,arg,2><itemMenu,arg,overworldhighlightedblock>";
        testTextFile[4][0] = "Aww. Guess my pot's gonna go hungry. Come back if you want your stuff cooked.<set,arg,1>";
        testTextFile[5][0] = "So you want me to cook <var,0> items one at a time?<prompt,yes,1,no,0,1>";
        testTextFile[6][0] = "So you want me to cook <var,0> pairs of items?<prompt,yes,1,no,0,1>";
        testTextFile[7][0] = "Looks like some of them went bad. I'll still be here if you want to try again.";
        testTextFile[8][0] = "That's everything you wanted. Come back when you've got more stuff to throw in my pot!";
        testTextFile[9][0] = "Looks like you're all out of food to cook. Unless you wanna jump in the pot yourself...";
        testTextFile[10][0] = "Looks like you don't have enough for that.";

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
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 9, this));
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
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 10, this));
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

        //Item itemA;
        int itemIndexA = -1;

        if (state == 1)
        {
            //fill the list

            bool keepGoing = true;

            while (keepGoing)
            {
                //yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
                menuResult = MainManager.Instance.lastTextboxMenuResult;
                int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);
                //Debug.Log(menuResult + " " + itemIndexA);
                //grab the item
                if (itemIndexA != -1)
                {
                    if (itemIndexA == -2)
                    {
                        keepGoing = false;
                    } else
                    {
                        //itemA = MainManager.Instance.playerData.itemInventory[itemIndexA];
                        if (indexList == null)
                        {
                            indexList = new List<int>();
                            itemList = new List<Item>();
                        }
                        indexList.Add(itemIndexA);
                        itemList.Add(MainManager.Instance.playerData.itemInventory[itemIndexA]);

                        if (itemList.Count < MainManager.Instance.playerData.itemInventory.Count)
                        {
                            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
                        }
                        else
                        {
                            keepGoing = false;
                        }
                    }
                }
                else
                {
                    //cancel
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                    yield break;
                }
            }

            //index list is filled?
            if (indexList == null || indexList.Count == 0)
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                yield break;
            }

            //Do the menu thing
            string[] tempVars = new string[] { itemList.Count + "" };
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this, tempVars));

            int ynState = 0;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out ynState);

            if (ynState == 1)
            {

                bool noMistakes = true;

                //cooking
                for (int i = 0; i < itemList.Count; i++)
                {
                    //note: index list can't be trusted because things might move around?
                    Item targetItem = itemList[i];
                    MainManager.Instance.playerData.itemInventory.Remove(targetItem);

                    RecipeDataEntry rde = GlobalItemScript.Instance.GetRecipeDataFromIngredientsBigTable(targetItem.type);

                    //specialty
                    if (rde.quality == Item.ItemQuality.SpecialtyRecipe || rde.quality == Item.ItemQuality.SupremeRecipe)
                    {
                        rde.quality = Item.ItemQuality.Mistake;
                        rde.result = Item.ItemType.Mistake;
                    }

                    Item.ItemType outputType = rde.result;
                    Item.ItemQuality quality = rde.quality;

                    if (rde.quality == Item.ItemQuality.Mistake)
                    {
                        noMistakes = false;
                    }

                    Item resItem = new Item(outputType, targetItem.modifier, Item.ItemOrigin.CookSizzle, 0, 0);
                    MainManager.Instance.SetRecipeFlag(resItem.type);
                    string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                    //yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this, tempResVars));

                    yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                }

                if (noMistakes)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                }
                GlobalItemScript.Instance.UnloadBigRecipeDataTable();
                yield break;
            }

            //cancel
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
            yield break;
        }

        int itemIndexB = -1;

        if (state == 2)
        {
            //fill the list

            bool keepGoing = true;

            while (keepGoing)
            {
                //yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
                menuResult = MainManager.Instance.lastTextboxMenuResult;
                int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexA);
                //Debug.Log(menuResult + " " + itemIndexA);
                //grab the item
                if (itemIndexA != -1)
                {
                    if (itemIndexA == -2)
                    {
                        keepGoing = false;
                    }
                    else
                    {
                        if (pairIndexList == null)
                        {
                            pairIndexList = new List<int>();
                            pairItemList = new List<Item>();
                        }
                        pairIndexList.Add(itemIndexA);
                        pairItemList.Add(MainManager.Instance.playerData.itemInventory[itemIndexA]);

                        //Second item
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
                        menuResult = MainManager.Instance.lastTextboxMenuResult;
                        int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out itemIndexB);
                        //grab the item
                        if (itemIndexB != -1)
                        {
                            if (pairIndexList == null)
                            {
                                pairIndexList = new List<int>();
                                pairItemList = new List<Item>();
                            }
                            pairIndexList.Add(itemIndexB);
                            pairItemList.Add(MainManager.Instance.playerData.itemInventory[itemIndexB]);
                        }
                        else
                        {
                            //cancel
                            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                            yield break;
                        }

                        if (keepGoing)
                        {
                            //Minus 1 since you need to be able to select 2 items next iteration
                            if (pairItemList.Count < MainManager.Instance.playerData.itemInventory.Count - 1)
                            {
                                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
                            }
                            else
                            {
                                keepGoing = false;
                            }
                        }
                    }
                }
                else
                {
                    //cancel
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                    yield break;
                }
            }


            //index list is filled?
            if (pairIndexList == null || pairIndexList.Count == 0)
            {
                //cancel
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                yield break;
            }

            //Do the menu thing
            string[] tempVars = new string[] { (pairItemList.Count / 2) + "" };
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 6, this, tempVars));

            int ynState = 0;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(menuResult, out ynState);

            if (ynState == 1)
            {
                bool noMistakes = true;

                //cooking
                for (int i = 0; i < pairItemList.Count; i += 2)
                {
                    //note: index list can't be trusted because things might move around?
                    Item targetItem = pairItemList[i];
                    Item targetItemB = pairItemList[i + 1];
                    MainManager.Instance.playerData.itemInventory.Remove(targetItem);
                    MainManager.Instance.playerData.itemInventory.Remove(targetItemB);

                    RecipeDataEntry rde = GlobalItemScript.Instance.GetRecipeDataFromIngredientsBigTable(targetItem.type, targetItemB.type);

                    //specialty
                    if (rde.quality == Item.ItemQuality.SpecialtyRecipe || rde.quality == Item.ItemQuality.SupremeRecipe)
                    {
                        rde.quality = Item.ItemQuality.Mistake;
                        rde.result = Item.ItemType.Mistake;
                    }

                    Item.ItemType outputType = rde.result;
                    Item.ItemQuality quality = rde.quality;

                    if (rde.quality == Item.ItemQuality.Mistake)
                    {
                        noMistakes = false;
                    }

                    Item resItem = new Item(outputType, GlobalItemScript.GetModifierFromRecipe(targetItem.modifier, targetItemB.modifier), Item.ItemOrigin.CookSizzle, 0, 0);

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

                    yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                }

                if (noMistakes)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                }
                GlobalItemScript.Instance.UnloadBigRecipeDataTable();
                yield break;
            }

            //cancel
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
            yield break;
        }
    }

    public override string RequestTextData(string request)
    {
        //Debug.Log("Requested " + request);

        Color ColorByValue(int i)
        {
            return MainManager.ColorMap(1 + 2 * (i / (MainManager.Instance.playerData.itemInventory.Count + 0f)));
        }

        if (request.Equals("1"))
        {
            //Debug.Log("Second index is " + secondIndex);
            List<Color?> colorList = new List<Color?>();
            for (int i = 0; i < MainManager.Instance.playerData.itemInventory.Count; i++)
            {
                colorList.Add(null);
            }

            if (indexList != null)
            {
                for (int j = 0; j < indexList.Count; j++)
                {
                    colorList[indexList[j]] = ColorByValue(j);
                }
            }

            return MainManager.PackColorList(colorList);
        }

        if (request.Equals("2"))
        {
            //Debug.Log("Second index is " + secondIndex);
            List<Color?> colorList = new List<Color?>();
            for (int i = 0; i < MainManager.Instance.playerData.itemInventory.Count; i++)
            {
                colorList.Add(null);
            }

            if (pairIndexList != null)
            {
                for (int j = 0; j < pairIndexList.Count; j++)
                {
                    colorList[pairIndexList[j]] = ColorByValue(j / 2);
                }
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
