using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInnkeeperNPCScript : WorldNPCEntity
{
    public int cost;
    public InnEffect.InnType type;
    public Vector3 respawnSpot;
    public Vector3 respawnOffset;

    public bool wasHere;    //note: talking will reset this so you can stay again if you really want to for some reason

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = new Color(0.6f, 0.6f, 0.2f, 0.5f); //this is gray, could be anything
        Gizmos.DrawSphere(respawnSpot, 0.25f);
        Gizmos.DrawLine(respawnSpot, respawnSpot + respawnOffset);
        Gizmos.DrawSphere(respawnSpot + respawnOffset, 0.125f);
    }

    public override IEnumerator InteractCutscene()
    {
        string[][] testTextFile = new string[8][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];
        testTextFile[7] = new string[1];

        testTextFile[0][0] = "Innkeeper main text (Costs <var,0>)<prompt,Pay,1,Talk,2,Cancel,3,2>";
        testTextFile[1][0] = "(At max stats) Innkeeper main text (Costs <var,0>)<prompt,Pay,1,Talk,2,Cancel,3,2>";
        testTextFile[2][0] = "You were here before";
        testTextFile[3][0] = "Innkeeper accept";
        testTextFile[4][0] = "Too poor";
        testTextFile[5][0] = "Talking text (you will get effect <var,1>)";
        testTextFile[6][0] = "Afterward text (you got effect <var,1>)";
        testTextFile[7][0] = "Bye";

        int state = 0;

        string[] vars = new string[] { cost.ToString(), type.ToString() };

        if (wasHere)
        {
            wasHere = false;
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
            yield break;
        }

        if (MainManager.Instance.playerData.AtMaxStats())
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this, vars));
        }
        else
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this, vars));
        }

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        switch (state)
        {
            case 0:
            case 3:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this));
                yield break;
            case 1:
                if (MainManager.Instance.playerData.coins < cost)
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                    yield break;
                }
                MainManager.Instance.playerData.coins -= cost;
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
                break;
            case 2:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this, vars));
                yield break;
        }

        yield return MainManager.Instance.FadeToBlack();

        MainManager.Instance.playerData.FullHeal();
        MainManager.Instance.playerData.AddInnEffect(type);
        WorldPlayer.Instance.transform.position = respawnSpot;
        WorldPlayer.Instance.FollowerWarp(Vector3.right);
        wasHere = true;

        yield return MainManager.Instance.UnfadeToBlack();

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 6, this, vars));
    }
}
