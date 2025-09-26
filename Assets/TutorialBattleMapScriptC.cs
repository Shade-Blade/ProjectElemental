using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBattleMapScriptC : TutorialBattleMapScript
{
    public enum TutorialState
    {
        Start,
        ChooseCheck,
        ChooseScan,
        ChooseElectroSlash,
        WaitTurnA,
        ChooseLeafySmash,
        ChooseRest,
        End
    }

    public TutorialState state;

    public bool cueStateChange;
    public bool blockFail;


    public IEnumerator StateChangeDialogue()
    {
        /*
        testTextFile[0][0] += "<next><system>You'll need to understand how enemies work in order to know how to deal with them. Try using the Check action in the Tactics menu with Wilex.";

        testTextFile[1][0] = "<system>You can see that Checking the enemy has revealed their health bar below them. Any other enemies of the same type will also have their health bars shown below them.";
        testTextFile[1][0] += "<next><system>You can read the entire bestiary text for that enemy type in the Bestiary section of the Journal section of the Pause Menu in the overworld.";
        testTextFile[1][0] += "<next><system>Keru has the ability to scan enemies from afar and tell you about them remotely. There is a seperate action called Scan which is similar to the Check action but it gives you the entire moveset of the target enemy.";
        testTextFile[1][0] += "<next><system>While normal enemies have very small movesets, lategame enemies like this one will have a variety of moves with unusual effects and reactions so knowing the entire moveset is more important then.";
        testTextFile[1][0] += "<next><system>Try using it with Luna now (Use <button,b> to switch to her).";
         */

        string[][] testTextFile = new string[9][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];
        testTextFile[7] = new string[1];
        testTextFile[8] = new string[1];

        testTextFile[0][0] = "<system>You'll need to understand how enemies work in order to know how to deal with them. Try using the Check action in the Tactics menu with Wilex.";

        testTextFile[1][0] = "<system>You can see that Checking the enemy has revealed their health bar below them. Any other enemies of the same type will also have their health bars shown below them.";
        testTextFile[1][0] += "<next><system>You can read the entire bestiary text for that enemy type in the Bestiary section of the Journal section of the Pause Menu in the overworld.";
        testTextFile[1][0] += "<next><system>Keru has the ability to scan enemies from afar and tell you about them remotely. There is a seperate action called Scan which is similar to the Check action but it gives you the entire moveset of the target enemy.";
        testTextFile[1][0] += "<next><system>While normal enemies have very small movesets, lategame enemies like this one will have a variety of moves with unusual effects and reactions so knowing the entire moveset is more important then.";
        testTextFile[1][0] += "<next><system>Try using it with Luna now (Use <button,b> to switch to her).";

        testTextFile[2][0] = "<system>Normal skills cost Stamina and Energy. Try using Electro Slash with Wilex now. You can see that it is in <debtcolor>light orange</color> and it has an icon to designate what effect it has.";
        testTextFile[2][0] += "<next><system><exhaustcolor>Darker orange</color> means that the move will cause you to lose stamina regeneration next turn. <debtcolor>Lighter orange</color> represents a move that puts you in stamina debt that makes you lose your next turn.";
        testTextFile[2][0] += "<next><system>Skills in red are not usable (this tutorial disables many skills).";
        testTextFile[2][0] += "<next><system>Make sure to read the action command description after selecting the move to know what to do to perform it correctly.";
        testTextFile[2][0] += "<next><system>Watch out for the <state,StateCounter> symbol above the enemy also, as that indicates an enemy that will counterattack you whenever you attack it.";

        testTextFile[3][0] = "<system>You should see that the enemy is now Shocked because you used an Air/Electric type attack, which reduces its defense but increases its attack. Making proper use of these Elemental Marks is the key to winning battles.";
        testTextFile[3][0] += "<next><system>You can see the exact effect of the Shocked effect by hovering over the icon.";
        testTextFile[3][0] += "<next><system>The potency of the mark is equal to the damage dealt divided by 4, rounded up and will last for 2 turns. However you can stack the duration of marks by using the same element.";

        testTextFile[4][0] = "<system>You should see that the Leafy Smash has removed the Shocked effect because Air and Earth are opposite elements. Also note the increased damage dealt by Leafy Smash." +
            "<next><system>Part of this was because of the Electro Slash, but some enemies can be weak to certain elements which will be an additive increase to damage taken." +
            "<next><system>You can also remove Elemental Marks on yourself using the opposite effect, so if Luna was Shocked you could have used Leafy Smash to remove it from her.";
        testTextFile[4][0] += "<next><system>Try using the Rest action with Wilex to replenish his stamina. Resting will give you bonus stamina which will make it easier to spam high cost moves. Note that using Rest does the same thing as losing your turn from stamina debt.";

        testTextFile[5][0] = "<system>Notice that Wilex's stamina is negative (red in the top left corner). This means he will lose his turn because of it. Stamina is also zeroed out if you die.";
        testTextFile[5][0] += "<next><system>Now try using Leafy Smash with Luna to do Earth damage to the enemy.";

        testTextFile[6][0] = "<system>Try using the Rest action to replenish your stamina. Resting will give you bonus stamina which will make it easier to spam high cost moves. Note that using Rest does the same thing as losing your turn from stamina debt.";

        testTextFile[7][0] = "<system>Note that Turn Relay and Fleeing also both cost a certain amount of stamina. The cost of the Flee action is based on the strength of the enemies but it is capped at your Max Stamina (1/2 max EP)." +
            "<next><system>Try fleeing the battle now.";


        PlayerEntity wilex = null;
        PlayerEntity luna = null;
        foreach (PlayerEntity pe in BattleControl.Instance.GetPlayerEntities())
        {
            if (pe.entityID == BattleHelper.EntityID.Wilex)
            {
                wilex = pe;
            }
            if (pe.entityID == BattleHelper.EntityID.Luna)
            {
                luna = pe;
            }
            if (pe.hp < 1)
            {
                pe.HealHealth(1);
            }

            pe.HealEnergy(12);
        }

        switch (state)
        {
            case TutorialState.Start:                
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[0][0], null));
                break;
            case TutorialState.ChooseCheck:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[1][0], null));
                break;
            case TutorialState.ChooseScan:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[2][0], null));
                break;
            case TutorialState.ChooseElectroSlash:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[3][0], null));
                break;
            case TutorialState.WaitTurnA:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[5][0], null));
                break;
            case TutorialState.ChooseLeafySmash:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[4][0], null));
                break;
            case TutorialState.ChooseRest:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[7][0], null));
                break;
        }
        state++;
    }

    public override void OnBattleStart()
    {
        PlayerTurnController.Instance.tutorial = this;
        cueStateChange = true;
    }
    public override IEnumerator OnPreTurn()
    {
        yield break;
    }
    public override IEnumerator OnPostTurn()
    {
        yield break;
    }
    public override void React(BattleEntity b, BattleHelper.Event eventID)
    {
        switch (state)
        {
            case TutorialState.Start:
                cueStateChange = true;
                break;
            case TutorialState.ChooseCheck:
            case TutorialState.ChooseScan:
                if (eventID == BattleHelper.Event.Tactic)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.ChooseElectroSlash:
            case TutorialState.ChooseLeafySmash:
                if (eventID == BattleHelper.Event.Hurt)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.WaitTurnA:
                if (eventID == BattleHelper.Event.PostAction && b.posId >= 0)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.ChooseRest:
                if (eventID == BattleHelper.Event.Tactic)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.End:
                if (eventID == BattleHelper.Event.Death && b.posId >= 0)
                {
                    foreach (PlayerEntity pel in BattleControl.Instance.GetPlayerEntities())
                    {
                        pel.HealHealth(pel.hp);
                        pel.HealEnergy(12);
                    }
                }
                break;
        }
    }
    public override IEnumerator OnOutOfTurn()
    {
        if (cueStateChange)
        {
            cueStateChange = false;
            yield return StartCoroutine(StateChangeDialogue());
        }
        foreach (PlayerEntity pe in BattleControl.Instance.GetPlayerEntities())
        {
            if (pe.hp < 1)
            {
                pe.HealHealth(1);
            }
        }
        yield break;
    }

    public override bool CanChoose(Move pm, BattleEntity b)
    {
        switch (state)
        {
            case TutorialState.Start:
            case TutorialState.ChooseScan:
            case TutorialState.ChooseCheck:
                return false;
            case TutorialState.ChooseElectroSlash:
                return (pm is WM_ElectroSlash);
            case TutorialState.ChooseLeafySmash:
                return (pm is LM_LeafySmash);
            case TutorialState.ChooseRest:
                return false;
            case TutorialState.WaitTurnA:
                return false;
            case TutorialState.End:
                return false;
        }

        return true;
    }
    public override bool CanChoose(BattleAction pm, BattleEntity b)
    {
        switch (state)
        {
            case TutorialState.Start:
                return false;
            case TutorialState.ChooseScan:
                return (b.entityID == BattleHelper.EntityID.Luna) && (pm is BA_Scan);
            case TutorialState.ChooseCheck:
                return (b.entityID == BattleHelper.EntityID.Wilex) && (pm is BA_Check);
            case TutorialState.ChooseElectroSlash:
                return false;
            case TutorialState.ChooseLeafySmash:
                return false;
            case TutorialState.ChooseRest:
                return (pm is BA_Rest);
            case TutorialState.WaitTurnA:
                return false;
            case TutorialState.End:
                return (pm is BA_Flee) || (pm is BA_Rest);
        }

        return true;
    }
    public override bool CanTarget(BattleEntity user, TargetArea targetArea, BattleEntity target)
    {
        return true;
    }
}
