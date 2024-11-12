using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCookNPCScript : WorldNPCEntity
{
    int secondIndex;

    public override IEnumerator InteractCutscene()
    {
        secondIndex = -1;
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
        testTextFile[0][0] = "Hi there! I can cook your items for you if you want. There's a lot I don't know about cooking, but I'm getting better every day!<prompt,Cook 1 Ingredient,1,Cook 2 Ingredients,2,Cancel,3,2>";
        testTextFile[1][0] = "So what do you want me to cook?<itemMenu,overworld>";
        testTextFile[2][0] = "So what do you want me to cook?<itemMenu,overworld>";
        testTextFile[3][0] = "What's the other ingredient you want me to cook?<dataget,arg,3><itemMenu,arg,overworldhighlightedblock>";
        testTextFile[4][0] = "Oh, ok. Come back if you have something I can cook, I'm always trying to learn how to cook new things!<set,arg,1>";
        testTextFile[5][0] = "You want me to cook your <var,0>? Yeah, I think I can do that.<prompt,yes,1,no,0,1>";
        testTextFile[6][0] = "You want me to cook your <var,0>? I'm not sure that's going to work, but I won't know until I try it!<prompt,yes,1,no,0,1>";
        testTextFile[7][0] = "You want me to cook your <var,0> and <var,1>. Yeah, I think I can do that.<prompt,yes,1,no,0,1>";
        testTextFile[8][0] = "You want me to cook your <var,0> and <var,1>. I'm not sure that's going to work, but I won't know until I try it!<prompt,yes,1,no,0,1>";
        testTextFile[9][0] = "It turned out great! Here's the <var,0> I made.";
        testTextFile[10][0] = "That went a little weird, but I think that <var,0> turned out fine.";
        testTextFile[11][0] = "It turned out amazing! Here's the <var,0> I made.";
        testTextFile[12][0] = "Oh... I'm sorry, but it didn't turn out right at all.";
        testTextFile[13][0] = "Sorry, but you don't have anything I can cook with.";
        testTextFile[14][0] = "Sorry, but you don't have enough for me to make a two ingredient recipe.";

        int text_start = 0;
        int text_choosesingle = 1;
        int text_choosedoubleA = 2;
        int text_choosedoubleB = 3;
        int text_cancel = 4;
        int text_cooksingle = 5;
        int text_cooksinglemistake = 6;
        int text_cookdouble = 7;
        int text_cookdoublemistake = 8;
        int text_cookresultsuccess = 9;
        int text_cookresultgood = 10;
        int text_cookresultweird = 11;
        int text_cookresultmistake = 12;
        int text_noitems = 13;
        int text_noitemsdouble = 14;


        int state = 0;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_start, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case 3:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancel, this));
                state = 0;
                break;
            case 1:
                if (MainManager.Instance.playerData.itemInventory.Count < 1)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_noitems, this));
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_choosesingle, this));
                }
                break;
            case 2:
                if (MainManager.Instance.playerData.itemInventory.Count < 2)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_noitemsdouble, this));
                    yield break;
                }
                else
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_choosedoubleA, this));
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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancel, this));
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
            string[] tempVars = new string[] { Item.GetName(itemA), (rde.quality != Item.ItemQuality.Mistake).ToString() };
            if (rde.quality != Item.ItemQuality.Mistake)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cooksingle, this, tempVars));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cooksinglemistake, this, tempVars));
            }

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

                Item resItem = new Item(outputType, itemA.modifier, Item.ItemOrigin.CookStella, 0, 0);
                MainManager.Instance.SetRecipeFlag(resItem.type);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                switch (quality)
                {
                    case Item.ItemQuality.Recipe:
                    case Item.ItemQuality.GridFillRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookresultsuccess, this, tempResVars));
                        break;
                    case Item.ItemQuality.WeirdRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookresultweird, this, tempResVars));
                        break;
                    case Item.ItemQuality.Mistake:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookresultmistake, this, tempResVars));
                        break;
                    case Item.ItemQuality.GoodRecipe:
                    case Item.ItemQuality.SpecialtyRecipe:
                    case Item.ItemQuality.SupremeRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookresultgood, this, tempResVars));
                        break;
                }

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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancel, this));
                yield break;
            }

            secondIndex = itemIndexA;
            //Debug.Log("Second index is " + secondIndex);

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_choosedoubleB, this));
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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancel, this));
                yield break;
            }

            //Debug.Log(menuResult + " " + itemIndexA + " " + itemIndexB);

            //precompute
            RecipeDataEntry rde = GlobalItemScript.Instance.GetRecipeDataFromIngredients(itemA.type, itemB.type);

            //specialty
            if (rde.quality == Item.ItemQuality.SpecialtyRecipe || rde.quality == Item.ItemQuality.SupremeRecipe)
            {
                rde.quality = Item.ItemQuality.Mistake;
                rde.result = Item.ItemType.Mistake;
            }

            //Do the menu thing
            string[] tempVars = new string[] { Item.GetName(itemA), Item.GetName(itemB), (rde.quality != Item.ItemQuality.Mistake).ToString() };
            if (rde.quality != Item.ItemQuality.Mistake)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookdouble, this, tempVars));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookdoublemistake, this, tempVars));
            }

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

                Item resItem = new Item(outputType, itemA.modifier, Item.ItemOrigin.CookStella, 0, 0);
                MainManager.Instance.SetRecipeFlag(resItem.type);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                switch (quality)
                {
                    case Item.ItemQuality.Recipe:
                    case Item.ItemQuality.GridFillRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookresultsuccess, this, tempResVars));
                        break;
                    case Item.ItemQuality.WeirdRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookresultweird, this, tempResVars));
                        break;
                    case Item.ItemQuality.Mistake:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookresultmistake, this, tempResVars));
                        break;
                    case Item.ItemQuality.GoodRecipe:
                    case Item.ItemQuality.SpecialtyRecipe:
                    case Item.ItemQuality.SupremeRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookresultgood, this, tempResVars));
                        break;
                }

                yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                yield break;
            }


            /*
            //False = cancel
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
            yield break;
            */

            //False = go back up through the loop
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_choosedoubleA, this));
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
