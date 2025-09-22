using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBattleMapScriptC : TutorialBattleMapScript
{
    public enum TutorialState
    {
        Start,
        ChooseElectroSlash,
        ChooseLeafySmash,
        WaitTurn,
        ChooseRest,
        End
    }

    public TutorialState state;

    public bool cueStateChange;
    public bool blockFail;


    public IEnumerator StateChangeDialogue()
    {
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

        testTextFile[0][0] = "<system>Normal skills cost Stamina and Energy. Try using Electro Slash with Wilex now. You can see that it is in <debtcolor>light orange</color> and it has an icon to designate what effect it has.";
        testTextFile[0][0] += "<next><system><exhaustcolor>Darker orange</color> means that the move will cause you to lose stamina regeneration next turn. <debtcolor>Lighter orange</color> represents a move that puts you in stamina debt that makes you lose your next turn.";
        testTextFile[0][0] += "<next><system>Skills in red are not usable (this tutorial disables many skills).";
        testTextFile[0][0] += "<next><system>Make sure to read the action command description after selecting the move to know what to do to perform it correctly.";
        testTextFile[0][0] += "<next><system>Watch out for the <state,StateCounter> symbol above the enemy also, as that indicates an enemy that will counterattack you whenever you attack it.";

        testTextFile[1][0] = "<system>You should see that the enemy is now Shocked because you used an Air/Electric type attack, which reduces its defense but increases its attack. Making proper use of these Elemental Marks is the key to winning battles.";
        testTextFile[1][0] += "<next><system>You can see the exact effect of the Shocked effect by hovering over the icon.";
        testTextFile[1][0] += "<next><system>The potency of the mark is equal to the damage dealt divided by 4, rounded up and will last for 2 turns. However you can stack the duration of marks by using the same element.";
        testTextFile[1][0] += "<system>Now try using Leafy Smash with Luna to do Earth damage to the enemy.";

        testTextFile[2][0] = "<system>You should see that the Leafy Smash has removed the Shocked effect because Air and Earth are opposite elements. You can also remove Elemental Marks on yourself using the opposite effect, so if Luna was Shocked you could have used Leafy Smash to remove it.";

        testTextFile[2][0] += "<next><system>Notice that your stamina is negative (red in the top left corner). This means you will lose your turn because of it. Your stamina is also zeroed out if you die.";

        testTextFile[4][0] = "<system>Try using the Rest action to replenish your stamina. Resting will give you bonus stamina which will make it easier to spam high cost moves. Note that using Rest does the same thing as losing your turn from stamina debt.";

        testTextFile[5][0] = "<system>Note that Turn Relay and Fleeing also both cost a certain amount of stamina. Try fleeing the battle now.";


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
            case TutorialState.ChooseElectroSlash:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[1][0], null));
                break;
            case TutorialState.ChooseLeafySmash:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[2][0], null));
                break;
            case TutorialState.WaitTurn:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[4][0], null));
                break;
            case TutorialState.ChooseRest:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[5][0], null));
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
            case TutorialState.ChooseElectroSlash:
                if (eventID == BattleHelper.Event.Hurt)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.ChooseLeafySmash:
                if (eventID == BattleHelper.Event.Hurt)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.WaitTurn:
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
                return false;
            case TutorialState.ChooseElectroSlash:
                return (pm is WM_ElectroSlash);
            case TutorialState.ChooseLeafySmash:
                return (pm is LM_LeafySmash);
            case TutorialState.ChooseRest:
                return false;
            case TutorialState.WaitTurn:
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
            case TutorialState.ChooseElectroSlash:
                return false;
            case TutorialState.ChooseLeafySmash:
                return false;
            case TutorialState.ChooseRest:
                return (pm is BA_Rest);
            case TutorialState.WaitTurn:
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
