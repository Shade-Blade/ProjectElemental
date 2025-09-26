using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialBattleMapScript : BattleMapScript
{
    public abstract bool CanChoose(Move pm, BattleEntity b);
    public abstract bool CanChoose(BattleAction pm, BattleEntity b);
    public abstract bool CanTarget(BattleEntity user, TargetArea targetArea, BattleEntity target);
}

public class TutorialBattleMapScriptA : TutorialBattleMapScript
{
    public enum TutorialState
    {
        Start,
        ChooseSlash,
        ChooseTurnRelay,
        ChooseStomp,
        DoBlock,
        UseItem,
    }

    public TutorialState state;

    public bool cueStateChange;
    public bool blockFail;
    public bool acFail;


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
        testTextFile[0][0] += "<next><system>You can switch who will act first using <button,b>. This is only possible if both of you can act.";
        testTextFile[0][0] += "<next><system>You can also switch positions with <button,z> but this is also only available when both of you can move but switching positions can also be done if one of you is dead.";
        testTextFile[0][0] +=  "<next><system>The character in the front is targetted more often but gets 33% more Agility (stamina regeneration). (Note that if one character is dead the other character is always considered to be in front.)";
        testTextFile[0][0] += "<next><system>Now try using Slash to attack the enemy.";
        testTextFile[0][0] += "<next><system>Make sure to read the action command description after selecting the move to know what to do to perform it correctly.";

        testTextFile[1][0] = "<system>Now that Wilex has acted, Luna is the only one you can act with right now. However, you can use Turn Relay to give Wilex another turn.";
        testTextFile[1][0] += "<next><system>Try doing that now.";

        testTextFile[2][0] = "<system>Wilex now has another turn. Note that a character having multiple turns will increase the cost of their abilities by 25% per extra action.";
        testTextFile[2][0] += "<next><system>Giving only one character all the actions will also likely make them run out of Stamina, which will be covered in a later tutorial.";
        testTextFile[2][0] += "<next><system>Now try using High Stomp on the enemy. The action command for High Stomp is different than the one for Slash, so pay attention to that.";

        testTextFile[3][0] = "<system>It seems you didn't do the action command correctly. Try again.";

        testTextFile[5][0] = "<system>Now it is the enemy side's turn to attack. Block attacks using <button,a> right before taking damage. This reduces damage taken by 1. Ignore the text that appears below you for now.";

        testTextFile[6][0] = "<system>It looks like you didn't block that attack. Let's try again. Use <button,a> right before taking damage to block.";

        testTextFile[7][0] = "<system>Now that the enemies have all acted, it is your turn again.<next><system>Now try using an Item in the Item menu to heal yourself.";
        testTextFile[7][0] += "<next><system>Items can heal HP and EP, or provide any variety of other effects. Make sure to pay attention to the description of each item to know what it does.";

        testTextFile[8][0] = "<system>Defeating enemies gives you XP, and 100 is required for each level up. Each level up lets you increase max HP, max EP or max SP (SP is the stat used to equip badges but it also controls max Soul Energy).";
        testTextFile[8][0] += "<next><system>You can learn more about battles by reading the descriptions of moves you get and reading the Journal section of the Pause Menu.";
        testTextFile[8][0] += "<next><system>Try finishing the battle however you want.";


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
            case TutorialState.ChooseSlash:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[1][0], null));
                break;
            case TutorialState.ChooseTurnRelay:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[2][0], null));
                break;
            case TutorialState.ChooseStomp:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[5][0], null));
                break;
            case TutorialState.DoBlock:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[7][0], null));
                BattleControl.Instance.playerData.AddItem(new Item(Item.ItemType.CarrotCake));
                break;
            case TutorialState.UseItem:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[8][0], null));
                //force a RunOutOfTurnEvents call so the enemy dies
                PlayerTurnController.Instance.interruptQueued = true;
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
            case TutorialState.ChooseSlash:
            case TutorialState.ChooseStomp:
                if (eventID == BattleHelper.Event.Hurt && b.posId >= 0)
                {
                    if (BattleHelper.GetDamageProperty(b.lastDamageProperties, BattleHelper.DamageProperties.AC_Success))
                    {
                        cueStateChange = true;
                    }
                    else
                    {
                        acFail = true;
                    }
                    if (b.hp < 5)
                    {
                        b.hp = 5;
                        b.TryCancelEvent(BattleHelper.Event.Death);
                    }
                }
                break;
            case TutorialState.ChooseTurnRelay:
                if (eventID == BattleHelper.Event.Tactic)
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
        if (acFail)
        {
            string text = "<system>It looks like you didn't do that action command correctly. Let's try again.";
            yield return StartCoroutine(MainManager.Instance.DisplayTextBox(text, null));
            foreach (PlayerEntity pe in BattleControl.Instance.GetPlayerEntities())
            {
                if (pe.entityID == BattleHelper.EntityID.Wilex)
                {
                    pe.ReceiveEffectForce(new Effect(Effect.EffectType.BonusTurns, 1, Effect.INFINITE_DURATION));
                    pe.actionCounter--;
                }
                //pe.ReceiveEffectForce(new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }
            acFail = false;
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
            case TutorialState.ChooseStomp:
                return (pm is WM_HighStomp);
            case TutorialState.ChooseTurnRelay:
                return false;
            case TutorialState.ChooseSlash:
                return (pm is WM_Slash) && !(pm is WM_ElectroSlash);
            case TutorialState.DoBlock:
                return false;
            case TutorialState.UseItem:
                return (pm is ItemMove);
        }

        return true;
    }
    public override bool CanChoose(BattleAction pm, BattleEntity b)
    {
        if (state == TutorialState.ChooseTurnRelay)
        {
            return (pm is BA_TurnRelay) && b.entityID == BattleHelper.EntityID.Luna;
        }

        if (state <= TutorialState.UseItem)
        {
            return false;
        }

        return true;
    }
    public override bool CanTarget(BattleEntity user, TargetArea targetArea, BattleEntity target)
    {
        return true;
    }
}
