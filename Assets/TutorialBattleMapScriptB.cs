using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBattleMapScriptB : TutorialBattleMapScript
{
    public enum TutorialState
    {
        Start,
        ChooseHighStomp,
        ChooseHeavyStomp,
        RibbonBlock,
        ChooseWeapon,
        End
    }

    public TutorialState state;

    public bool cueStateChange;
    public bool blockFail;
    public bool blockFailB;


    public IEnumerator StateChangeDialogue()
    {
        string[][] testTextFile = new string[7][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];

        testTextFile[0][0] = "<system>Notice that the front enemy is flying. You will need to jump high to reach them, your normal weapon attacks won't work.";
        testTextFile[0][0] += "<next><system>Try using High Stomp with Wilex to hit it.";

        testTextFile[1][0] = "<system>Most flying enemies will fall down after getting hit. Luna can also stomp enemies, but she can't jump high enough to reach flying enemies.";
        testTextFile[1][0] += "<next><system>You should also notice the <state,StateContactHazard> symbol above the other enemy. That means that contact moves like stomps will cause you to take damage.";
        testTextFile[1][0] += "<next><system>Try Heavy Stomping one of the enemies now to see that.";

        testTextFile[2][0] = "<system>It is the enemies turn to attack. The ribbons you are wearing give you special ways to block.";
        testTextFile[2][0] += "<next><system>The red ribbon on Wilex lets him do a Sharp Block by holding <button,B> which actually increases damage taken but causes the enemy to take damage if it used a contact attack.";
        testTextFile[2][0] += "<next><system>The green ribbon on Luna lets her do a Safety Block by holding <button,A> which reduces damage taken but causes you to lose some energy. It is useful if you are bad at timing the blocks correctly.";
        testTextFile[2][0] += "<next><system>You can hold both buttons to do both though each ribbon is only on one character. Try using the ribbon blocks now.";

        testTextFile[3][0] = "<system>Weapon moves can only target the frontmost enemy most of the time, but they deal slightly higher damage.";
        testTextFile[3][0] += "<next><system>Try hitting the last enemy with one of your weapon attacks.";

        testTextFile[4][0] = "<system>It looks like you didn't block that attack. Let's try again." +
            "<next><system>The Safety Ribbon block requires you to hold <button,a> while the Sharp Ribbon block requires you to hold <button,b>." +
            "<next><system>Note that you can hold both buttons to do both (though each ribbon is only on one character).";

        testTextFile[5][0] = "<system>It looks like you used a normal block instead of a ribbon block. Let's try again." +
            "<next><system>The Safety Ribbon block requires you to hold <button,a> while the Sharp Ribbon block requires you to hold <button,b>." +
            "<next><system>Note that you can hold both buttons to do both (though each ribbon is only on one character).";

        testTextFile[6][0] = "<system>You can equip ribbons in the Equip section of the Pause menu.";
        testTextFile[6][0] += "<next><system>You will get access to that section when you get your first Badge.";

        if (BattleControl.Instance.GetEntitiesSorted((e) => (e.hp > 0 && e.posId >= 0)).Count > 0)
        {
            testTextFile[6][0] += "<next><system>Now try to defeat the enemy by using whatever moves you want.";
        }

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
            case TutorialState.ChooseHighStomp:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[1][0], null));
                break;
            case TutorialState.ChooseHeavyStomp:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[2][0], null));
                break;
            case TutorialState.RibbonBlock:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[3][0], null));
                break;
            case TutorialState.ChooseWeapon:
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(testTextFile[6][0], null));
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
            case TutorialState.ChooseHighStomp:
                if (eventID == BattleHelper.Event.Hurt)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.ChooseHeavyStomp:
                if (eventID == BattleHelper.Event.Hurt)
                {
                    cueStateChange = true;
                }
                break;
            case TutorialState.RibbonBlock:
                if (b is PlayerEntity pe && eventID == BattleHelper.Event.Hurt)
                {
                    if (pe.lastHitWasSpecialBlocked)
                    {
                        cueStateChange = true;
                    }
                    else
                    {
                        if (pe.lastHitWasBlocked)
                        {
                            blockFailB = true;
                        }
                        blockFail = true;
                    }
                }
                break;
            case TutorialState.ChooseWeapon:
                if (eventID == BattleHelper.Event.Weapon)
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
        if (blockFail)
        {
            string text = "<system>It looks like you didn't block that attack. Let's try again. The Safety Ribbon block requires you to hold <button,a> while the Sharp Ribbon block requires you to hold <button,b>. Note that you can hold both buttons to do both (though each ribbon is only on one character).";
            string textB = "<system>It looks like you used a normal block instead of a ribbon block. Let's try again. The Safety Ribbon block requires you to hold <button,a> while the Sharp Ribbon block requires you to hold <button,b>. Note that you can hold both buttons to do both (though each ribbon is only on one character).";

            if (blockFailB)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(textB, null));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBox(text, null));
            }

            foreach (PlayerEntity pe in BattleControl.Instance.GetPlayerEntities())
            {
                pe.ReceiveEffectForce(new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }
            blockFail = false;
            blockFailB = false;
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
            case TutorialState.ChooseHighStomp:
                return (pm is WM_HighStomp);
            case TutorialState.ChooseHeavyStomp:
                return (pm is LM_HeavyStomp);
            case TutorialState.RibbonBlock:
                return false;
            case TutorialState.ChooseWeapon:
                return (pm is WM_Slash) || (pm is LM_Smash);
        }

        return true;
    }
    public override bool CanChoose(BattleAction pm, BattleEntity b)
    {
        return false;
    }

    public override bool CanTarget(BattleEntity user, TargetArea targetArea, BattleEntity target)
    {
        if (state == TutorialState.ChooseHighStomp)
        {
            return target.GetEntityProperty(BattleHelper.EntityProperties.Airborne);
        }
        return true;
    }
}
