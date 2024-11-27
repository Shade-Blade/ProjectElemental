using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNPC_Innkeeper : WorldNPCEntity
{
    public int cost;
    public InnEffect.InnType type;
    public KeyItem.KeyItemType candle;
    public Vector3 respawnSpot;
    public Vector3 respawnOffset;

    public bool wasHere;    //note: talking will reset this so you can stay again if you really want to for some reason

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = new Color(0.6f, 0.6f, 0.2f, 0.5f); //this is gray, could be anything
        Gizmos.DrawSphere(transform.position + respawnSpot, 0.25f);
        Gizmos.DrawLine(transform.position + respawnSpot, transform.position + respawnSpot + respawnOffset);
        Gizmos.DrawSphere(transform.position + respawnSpot + respawnOffset, 0.125f);
    }

    public override IEnumerator InteractCutscene()
    {
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
        testTextFile[10][0] = wed.talkStrings[10];

        /*
        //"Innkeeper main text (Costs <var,0>)<prompt,Stay Here (? coins),1,Buy Candle (? coins),2,Talk,3,Cancel,4,3>";
        //"(At max stats) Innkeeper main text (Costs <var,0>)<prompt,Stay Here (? coins),1,Buy Candle (? coins),2,Talk,3,Cancel,4,3>";
        //"You were here before";
        //"Innkeeper accept";
        //"Too poor";
        //"Talking text (you will get effect <var,1>)";
        //"Afterward text (you got effect <var,1>)";
        //"Bye";
         * */

        int text_main = 0;
        int text_mainMax = 1;
        int text_innkeeperAccept = 2;
        int text_candleBuy = 3;
        int text_candleCantBuy = 4;
        int text_tooPoorStay = 5;
        int text_tooPoorCandle = 6;
        int text_talk = 7;
        int text_after = 8;
        int text_cancel = 9;
        int text_wasHere = 10;

        PlayerData pd = MainManager.Instance.playerData;

        int state = 0;

        string[] vars = new string[] { cost.ToString(), type.ToString() };

        if (wasHere)
        {
            wasHere = false;
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_wasHere, this));
            yield break;
        }

        if (pd.AtMaxStats())
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_mainMax, this, vars));
        }
        else
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_main, this, vars));
        }

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case 4:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_cancel, this));
                yield break;
            case 1:
                if (pd.coins < cost)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_tooPoorStay, this));
                    yield break;
                }
                pd.coins -= cost;
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_innkeeperAccept, this));
                break;
            case 2:
                bool hasCandle = false;
                for (int i = 0; i < pd.keyInventory.Count; i++)
                {
                    if (pd.keyInventory[i].type >= KeyItem.KeyItemType.PlainCandle && pd.keyInventory[i].type <= KeyItem.KeyItemType.RainbowCandle)
                    {
                        hasCandle = true;
                        continue;
                    }
                }
                if (hasCandle)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_candleCantBuy, this));
                    yield break;
                }

                if (pd.coins < cost * 2)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_tooPoorCandle, this));
                    yield break;
                }
                pd.coins -= cost * 2;
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_candleBuy, this));

                MainManager.Instance.Pickup(new PickupUnion(new KeyItem(candle)));

                yield break;
            case 3:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_talk, this, vars));
                yield break;
        }

        yield return MainManager.Instance.FadeToBlack();

        pd.FullHeal();
        pd.AddInnEffect(type);
        WorldPlayer.Instance.transform.position = transform.position + respawnSpot;
        WorldPlayer.Instance.FollowerWarp(respawnOffset);
        wasHere = true;

        yield return MainManager.Instance.UnfadeToBlack();

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, text_after, this, vars));
    }
}
