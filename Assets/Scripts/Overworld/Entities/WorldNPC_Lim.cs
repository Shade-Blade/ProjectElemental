using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;

public class WorldNPC_Lim : WorldNPCEntity
{
    public override IEnumerator InteractCutscene()
    {
        string[][] testTextFile = new string[4][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];

        //0 = I have portals that'll get you around this place, if you have the coin for it. Two of them go further down while one goes back up a few floors.
        //1 = Alright, here we go. Keep your eyes closed for this, my techniques are a trade secret.
        //2 = I thought you two would understand the value of portal transport. Maybe I thought too highly of you.
        //3 = Time is money, so don't be afraid to spend a little to save time.
        //tattle = <tail,w>Lim has portals all over the place. Looks like we can skip ahead by quite a lot.<next><tail,k>But that would mean you wouldn't get anything from the floors you skipped. It might be better to take the portal going back a few floors instead.<next><tail,w>Spending more time in this place? Why would we ever want to do that?<next><tail,l>I think there's some stuff back there we could go back for.<next><tail,w>The floors up there are probably already changed. But I guess going back for whatever's there might be a good idea.

        //Idea: insert correct prompt text into the talk string

        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        }
        int floor = int.Parse(floorNo);

        int backfloor = floor - 6;
        if (backfloor < 1)
        {
            backfloor = 1;
        }
        int forwardfloor = floor + 6;
        if (forwardfloor > 100)
        {
            forwardfloor = 100;
        }
        int forwardfloorB = floor + 12;
        if (forwardfloorB > 100)
        {
            forwardfloorB = 100;
        }

        string promptText = "<prompt,Back To Floor " + backfloor + " (" + (10 + (backfloor)) + " coins),1";
        promptText += ",Go to Floor " + forwardfloor + " (" + (20 + (forwardfloor * 2)) + " coins),2";
        promptText += ",Go to Floor " + forwardfloorB + " (" + (40 + (forwardfloorB * 4)) + " coins),3";
        promptText += ",Cancel,-1,-1>";

        testTextFile[0][0] = wed.talkStrings[0] + promptText;
        testTextFile[1][0] = wed.talkStrings[1];
        testTextFile[2][0] = wed.talkStrings[2];
        testTextFile[3][0] = wed.talkStrings[3];

        int state = 0;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        if (state == -1)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
            yield break;
        }

        PlayerData pd = MainManager.Instance.playerData;

        //Try to pay
        int cost = 0;
        int targetfloor = floor;
        switch (state)
        {
            case 1:
                cost = (10 + (backfloor));
                targetfloor -= 6;
                break;
            case 2:
                cost = (20 + (forwardfloor * 2));
                targetfloor += 6;
                break;
            case 3:
                cost = (40 + (forwardfloorB * 4));
                targetfloor += 12;
                break;
        }

        if (targetfloor < 1)
        {
            targetfloor = 1;
        }
        if (targetfloor > 100)
        {
            targetfloor = 100;
        }

        if (pd.coins < cost)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
            yield break;
        }

        pd.coins -= cost;
        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));

        //Go somewhere
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, targetfloor.ToString());
        if (targetfloor == 100)
        {
            MainManager.Instance.StartCoroutine(MainManager.Instance.ChangeMap(MainManager.MapID.Test_PitFinalFloor, 0));
            yield break;
        }
        if (targetfloor % 10 == 0)
        {
            MainManager.Instance.StartCoroutine(MainManager.Instance.ChangeMap(MainManager.MapID.Test_PitRestFloor, 0));
            yield break;
        }
        MainManager.Instance.StartCoroutine(MainManager.Instance.ChangeMap(MainManager.MapID.Test_PitFloor, 0));
        yield break;
    }
}
