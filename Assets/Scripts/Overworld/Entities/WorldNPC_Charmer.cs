using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharmEffect;

public class WorldNPC_Charmer : WorldNPCEntity
{
    public CharmEffect.CharmType charm;
    public int[] charmPrices;
    public int[] totemPrices;

    public override IEnumerator InteractCutscene()
    {
        PlayerData pd = MainManager.Instance.playerData;

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

        //0 = base menu
        //  <prompt,Buy Charm,1,Buy Totem,2,Explain,3,Cancel,-1>
        //1 = charm menu
        //  <prompt,Lesser Charm (30 coins),1,Normal Charm (60 coins),2,Greater Charm (90 coins),3,Cancel,-1,-1>
        //2 = buy charm confirm
        //3 = too poor charm
        //4 = totem can't buy
        //5 = totem menu
        //  <prompt,Lesser Totem (30 coins),1,Normal Totem (60 coins),2,Greater Totem (90 coins),3,Cancel,-1,-1>
        //6 = buy totem confirm
        //7 = too poor totem
        //8 = explain
        //9 = cancel

        testTextFile[0][0] = wed.talkStrings[0];
        testTextFile[1][0] = wed.talkStrings[1];
        testTextFile[2][0] = wed.talkStrings[2];
        testTextFile[3][0] = wed.talkStrings[3];
        testTextFile[4][0] = wed.talkStrings[4];
        testTextFile[5][0] = wed.talkStrings[5];
        testTextFile[6][0] = wed.talkStrings[6];
        testTextFile[7][0] = wed.talkStrings[7];
        testTextFile[8][0] = wed.talkStrings[8];
        testTextFile[9][0] = wed.talkStrings[9];

        int state = 0;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case -1:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 9, this));
                yield break;
            case 1:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
                break;
            case 2:
                bool hasTotem = false;
                for (int i = 0; i < pd.keyInventory.Count; i++)
                {
                    if (charm == CharmEffect.CharmType.Fortune && pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemA)
                    {
                        hasTotem = true;
                    }
                    if (charm == CharmEffect.CharmType.Fortune && pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemB)
                    {
                        hasTotem = true;
                    }
                    if (charm == CharmEffect.CharmType.Fortune && pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemC)
                    {
                        hasTotem = true;
                    }

                    if ((charm == CharmEffect.CharmType.Attack || charm == CharmEffect.CharmType.Defense) && pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemA)
                    {
                        hasTotem = true;
                    }
                    if ((charm == CharmEffect.CharmType.Attack || charm == CharmEffect.CharmType.Defense) && pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemB)
                    {
                        hasTotem = true;
                    }
                    if ((charm == CharmEffect.CharmType.Attack || charm == CharmEffect.CharmType.Defense) && pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemC)
                    {
                        hasTotem = true;
                    }
                }
                if (hasTotem)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                    yield break;
                }
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this));
                break;
            case 3:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this));
                yield break;
        }

        if (state == 1)
        {
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg"), out int charmIndex);

            if (charmIndex == -1)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 9, this));
                yield break;
            }

            Debug.Log(MainManager.Instance.lastTextboxMenuResult + " " + charmIndex);
            int price = charmPrices[charmIndex - 1];
            if (price > pd.coins)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
                yield break;
            }

            //Pay
            pd.coins -= price;

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
            switch (charmIndex)
            {
                case 1:
                    if (charm == CharmType.Fortune)
                    {
                        pd.AddCharmEffect(CharmType.Fortune, 1);
                    }
                    else
                    {
                        pd.AddCharmEffect(CharmType.Attack, 1);
                    }
                    break;
                case 2:
                    if (charm == CharmType.Fortune)
                    {
                        pd.AddCharmEffect(CharmType.Fortune, 2);
                    }
                    else
                    {
                        pd.AddCharmEffect(CharmType.Attack, 2);
                    }
                    break;
                case 3:
                    if (charm == CharmType.Fortune)
                    {
                        pd.AddCharmEffect(CharmType.Fortune, 3);
                    }
                    else
                    {
                        pd.AddCharmEffect(CharmType.Attack, 3);
                    }
                    break;
            }
            yield break;
        }

        if (state == 2)
        {
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg"), out int totemIndex);

            if (totemIndex == -1)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 9, this));
                yield break;
            }

            int price = totemPrices[totemIndex - 1];
            if (price > pd.coins)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                yield break;
            }

            //Pay
            pd.coins -= price;

            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 6, this));

            switch (totemIndex)
            {
                case 1:
                    if (charm == CharmType.Fortune)
                    {
                        yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(new KeyItem(KeyItem.KeyItemType.FortuneTotemA))));
                    }
                    else
                    {
                        yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(new KeyItem(KeyItem.KeyItemType.PowerTotemA))));
                    }
                    break;
                case 2:
                    if (charm == CharmType.Fortune)
                    {
                        yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(new KeyItem(KeyItem.KeyItemType.FortuneTotemB))));
                    }
                    else
                    {
                        yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(new KeyItem(KeyItem.KeyItemType.PowerTotemB))));
                    }
                    break;
                case 3:
                    if (charm == CharmType.Fortune)
                    {
                        yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(new KeyItem(KeyItem.KeyItemType.FortuneTotemC))));
                    }
                    else
                    {
                        yield return StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(new KeyItem(KeyItem.KeyItemType.PowerTotemC))));
                    }
                    break;
            }
            yield break;
        }
    }
}
