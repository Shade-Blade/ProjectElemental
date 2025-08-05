using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBattleMapScript : BattleMapScript
{
    public enum TutorialState
    {
        Start,
        ChooseCheck,
        ChooseScan,
        ChooseTurnRelay,
        ChooseExpensive,
        DoBlock,
        UseItem,
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

        testTextFile[0][0] = "<system>Welcome to Project Elemental! Battles are turn based. Use <button,start> or <button,A> to open menus and use <button,left> and <button,right> to scroll through them (scroll up and down with <button,up> and <button,down>).";
        testTextFile[0][0] += "<next><system>You'll need to understand how enemies work in order to know how to deal with them. Try using the Check action in the Tactics menu with Luna. (Use <button,b> to switch to her).";
        testTextFile[0][0] += "<next><system>You can also switch positions with <button,z> but this is only available when both of you can move or one of you is dead. The character in the front is targetted more often but gets 33% more Agility (stamina regeneration).";

        testTextFile[1][0] = "<system>You can see that Checking the enemy has revealed their health bar below them. Any other enemies of the same type will also have their health bars shown below them. You can read the entire bestiary text in the Bestiary section of the Journal section of the Pause Menu in the overworld.";
        testTextFile[1][0] += "<next><system>Keru has the ability to scan enemies from afar and tell you about them remotely. There is a seperate action called Scan which is similar to the Check action but it gives you the entire moveset of the target enemy. Try using it with Wilex now (Use <button,b> to switch to him).";

        testTextFile[2][0] = "<system>You can see that the Check action did not use Luna's turn, while the Scan action used up Wilex's turn.";               
        testTextFile[2][0] += "<next><system>Try using the Turn Relay action to give Wilex another turn. Note that skills become 25% more expensive for each additional action after the first.";

        testTextFile[3][0] = "<system>Normally you would not be able to use Turn Relay right now because of the stamina cost but for the purposes of this tutorial you have been given more.";

        testTextFile[4][0] = "<system>Normal skills cost Stamina and Energy. Try using Multi Slash with Wilex now. You can see that it is in <debtcolor>light orange.</color>";
        testTextFile[4][0] += "<next><system><exhaustcolor>Dark orange</color> means that the move will cause you to lose stamina regeneration next turn. <debtcolor>Light orange</color> represents a move that puts you in stamina debt that makes you lose your next turn. Lost turns are equivalent to taking the Rest action in the tactics menu.";
        testTextFile[4][0] += "<next><system>Skills in red are not usable (this tutorial disables many skills).";
        testTextFile[4][0] += "<next><system>Make sure to read the action command description after selecting the move to know what to do to perform it correctly.";

        testTextFile[5][0] = "<system>Now it is the enemy side's turn to attack. Block attacks using <button,a> right before taking damage. This reduces damage taken by 1. The ribbons you are wearing also provide other ways to block but you should block normally for now. (To read more on Ribbons, look at them in the Equip section of the Pause Menu)";

        testTextFile[6][0] = "<system>It looks like you didn't block that attack. Let's try again. Use <button,a> right before taking damage to block.";

        testTextFile[7][0] = "<system>Now that the enemies have all acted, it is your turn again. Now try using an Item in the Item menu to heal yourself. Items can heal HP and EP, or provide any variety of other effects. Make sure to pay attention to the description of each item.";

        testTextFile[8][0] = "<system>Defeating enemies gives you XP, and 100 is required for each level up. Each level up lets you increase max HP, max EP or max SP (SP is the stat used to equip badges but it also controls max Soul Energy). You can learn more about battles by reading the descriptions of moves you get and reading the Journal section of the Pause Menu.";


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
                /*
                if (!BattleControl.Instance.turnRelay.CanChoose(luna))
                {
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[3][0], null));
                    luna.HealStamina(BattleControl.Instance.turnRelay.GetBaseCost());
                }
                */
                break;
            case TutorialState.ChooseTurnRelay:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[4][0], null));
                break;
            case TutorialState.ChooseExpensive:
                if (BattleControl.Instance.GetEntityByID(0) != null)
                {
                    BattleControl.Instance.GetEntityByID(0).hp = 0;
                }                
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[5][0], null));
                break;
            case TutorialState.DoBlock:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[7][0], null));
                BattleControl.Instance.playerData.AddItem(new Item(Item.ItemType.CarrotCake));
                break;
            case TutorialState.UseItem:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[8][0], null));
                yield return BattleControl.Instance.EndBattle(BattleHelper.BattleOutcome.Win);
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
                if (eventID == BattleHelper.Event.Check)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.ChooseScan:
                if (eventID == BattleHelper.Event.Check)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.ChooseTurnRelay:
                if (eventID == BattleHelper.Event.Tactic)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.ChooseExpensive:
                if (eventID == BattleHelper.Event.Hurt)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.DoBlock:
                if (b is PlayerEntity pe && eventID == BattleHelper.Event.Hurt)
                {
                    if (pe.lastHitWasBlocked)
                    {
                        cueStateChange = true;
                    } else
                    {
                        blockFail = true;
                    }
                }
                break;
            case TutorialState.UseItem:
                if (eventID == BattleHelper.Event.UseItem)
                {
                    cueStateChange = true;
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
        if (blockFail)
        {
            string text = "<system>It looks like you didn't block that attack. Let's try again. Use <button,a> right before taking damage to block.";
            yield return StartCoroutine(MainManager.Instance.DisplayTextBox(text, null));
            foreach (PlayerEntity pe in BattleControl.Instance.GetPlayerEntities())
            {
                pe.ReceiveEffectForce(new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }
            blockFail = false;
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

    public bool CanChoose(Move pm, BattleEntity b)
    {
        switch (state)
        {
            case TutorialState.Start:
                return false;
            case TutorialState.ChooseCheck:
                return false;
            case TutorialState.ChooseScan:
                return false;
            case TutorialState.ChooseTurnRelay:
                return false;
            case TutorialState.ChooseExpensive:
                return (pm is WM_MultiSlash);
            case TutorialState.DoBlock:
                return false;
            case TutorialState.UseItem:
                return (pm is ItemMove);
        }

        return true;
    }
    public bool CanChoose(BattleAction pm, BattleEntity b)
    {
        switch (state)
        {
            case TutorialState.Start:
                return false;
            case TutorialState.ChooseCheck:
                return (pm is BA_Check) && b.entityID == BattleHelper.EntityID.Luna;
            case TutorialState.ChooseScan:
                return (pm is BA_Scan) && b.entityID == BattleHelper.EntityID.Wilex;
            case TutorialState.ChooseTurnRelay:
                return (pm is BA_TurnRelay);
            case TutorialState.ChooseExpensive:
                return false;
            case TutorialState.DoBlock:
                return false;
            case TutorialState.UseItem:
                return false;
        }

        return true;
    }
}
