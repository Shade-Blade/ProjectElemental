using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNPC_SingleCook : WorldNPCEntity
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
        testTextFile[0][0] = "Hey, you want me to cook something up with my cooking pot for you? I can only cook one thing at a time, but there's still a lot of good recipes I can make.<prompt,Cook 1 Ingredient,1,Cancel,2,1>";
        testTextFile[1][0] = "Ok, what do you want me to cook?<itemMenu,overworld>";
        testTextFile[2][0] = "That's alright. Come back if you have something you want me to cook.<set,arg,1>";
        testTextFile[3][0] = "So you want me to cook <var,0>?<prompt,Yes,1,No,0,1>";
        testTextFile[4][0] = "So you want me to cook <var,0>? That seems like it isn't going to work, but you never know.<prompt,Yes,1,No,0,1>";
        testTextFile[5][0] = "Alright, it looks like we made <var,0>. Come back if you want to use my cooking pot again.";
        testTextFile[6][0] = "Alright, it looks like we made <var,0>. It smells good! Almost took a bite of it myself.";
        testTextFile[7][0] = "Alright, it looks like we made <var,0>. A little weird for my taste, but it's always good to try new things.";
        testTextFile[8][0] = "Looks like it didn't turn out right. Don't worry about it, I've messed things up all the time.";
        testTextFile[9][0] = "Looks like you don't have anything I can cook right now.";

        int text_start = 0;
        int text_chooseitem = 1;
        int text_cancel = 2;
        int text_cook = 3;
        int text_cookmistake = 4;
        int text_resultsuccess = 5;
        int text_resultgood = 6;
        int text_resultweird = 7;
        int text_resultmistake = 8;
        int text_noitems = 9;

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
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_chooseitem, this));
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
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cook, this, tempVars));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cookmistake, this, tempVars));
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

                Item resItem = new Item(outputType, itemA.modifier, Item.ItemOrigin.CookGourmand, 0, 0);
                MainManager.Instance.SetRecipeFlag(resItem.type);
                string[] tempResVars = new string[] { Item.GetName(resItem), quality.ToString() };
                switch (quality)
                {
                    case Item.ItemQuality.Recipe:
                    case Item.ItemQuality.GridFillRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_resultsuccess, this, tempResVars));
                        break;
                    case Item.ItemQuality.WeirdRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_resultweird, this, tempResVars));
                        break;
                    case Item.ItemQuality.Mistake:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_resultmistake, this, tempResVars));
                        break;
                    case Item.ItemQuality.GoodRecipe:
                    case Item.ItemQuality.SpecialtyRecipe:
                    case Item.ItemQuality.SupremeRecipe:
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_resultgood, this, tempResVars));
                        break;
                }

                yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(resItem)));
                yield break;
            }

            //False = go back up through the loop
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_chooseitem, this));
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