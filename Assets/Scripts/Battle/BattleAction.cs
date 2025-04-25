using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static PlayerMove;
using static UnityEngine.GraphicsBuffer;

//things that interrupt the battle system that aren't regular moves

public abstract class BattleAction : MonoBehaviour, IEntityHighlighter
{
    public abstract int GetBaseCost();
    public int cost;

    //How do you pay for this move?
    public virtual BattleHelper.MoveCurrency GetCurrency(BattleEntity caller = null)
    {
        return BattleHelper.MoveCurrency.Energy;
    }

    public virtual int GetCost(BattleEntity caller)
    {
        if (GetBaseCost() == 0)
        {
            return 0;
        }

        int modifiedCost = (int)GetBaseCost();
        
        if (UseBurst())
        {
            modifiedCost += -caller.GetEffectEnduranceBonus() - caller.GetBadgeEnduranceBonus();
        }
        if (UseFlow())
        {
            modifiedCost += -caller.GetEffectFlowBonus() - caller.GetBadgeFlowBonus();
        }

        if (modifiedCost < 1)
        {
            modifiedCost = 1;
        }
        return modifiedCost;
    }

    public BattleAction()
    {
        cost = (int)GetBaseCost();
    }


    //does this cost stamina?
    public virtual bool UseStamina()
    {
        return true;
    }

    //does this use burst, endurance, etc?
    public virtual bool UseBurst()
    {
        return true;
    }

    //does this use awaken, flow, etc?
    public virtual bool UseFlow()
    {
        return true;
    }
    
    //copies stuff from the move code
    public virtual bool CanChoose(BattleEntity caller)
    {
        if (!PlayerTurnController.Instance.CanChoose(this, caller))
        {
            return false;
        }

        if (GetBaseCost() == 0)
        {
            return true;
        }

        if (!MainManager.Instance.Cheat_EnergyAnarchy)
        {
            switch (GetCurrency(caller))
            {
                case BattleHelper.MoveCurrency.Energy:
                    if (BattleControl.Instance.ep < GetCost(caller))
                    {
                        return false;
                    }
                    break;
                case BattleHelper.MoveCurrency.Health:  //Note: you are not allowed to pay with all your hp (leaving you with 0)
                    if (caller.hp - 1 < GetCost(caller))
                    {
                        return false;
                    }
                    break;
                case BattleHelper.MoveCurrency.Stamina:
                    if (caller.stamina < GetCost(caller) - caller.GetEffectHasteBonus())
                    {
                        return false;
                    }
                    break;
                case BattleHelper.MoveCurrency.Soul:
                    if (BattleControl.Instance.se < GetCost(caller))
                    {
                        return false;
                    }
                    break;
                case BattleHelper.MoveCurrency.Coins:
                    if (BattleControl.Instance.GetCoins(caller) < GetCost(caller))
                    {
                        return false;
                    }
                    break;
            }
        }


        int staminaCost = GetCost(caller);
        if (caller is PlayerEntity pcaller)
        {
            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Coins && pcaller.BadgeEquipped(Badge.BadgeType.GoldenEnergy))
            {
                staminaCost = cost / 5;
            }

            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Stamina && pcaller.BadgeEquipped(Badge.BadgeType.StaminaEnergy))
            {
                //the actual cost check was earlier in this case so this check doesn't really matter
                staminaCost = 0;
            }

            if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.IonizedSand)
            {
                staminaCost /= 2;
            }
            if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.TrialOfHaste)
            {
                staminaCost = 0;
            }
        }

        if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.SacredGrove)
        {
            if (UseStamina() && (staminaCost - caller.GetEffectHasteBonus() >= 8))
            {
                return false;
            }
        }
        if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.TrialOfSimplicity)
        {
            if (UseStamina() && (staminaCost - caller.GetEffectHasteBonus() >= 5))
            {
                return false;
            }
        }

        if (!MainManager.Instance.Cheat_StaminaAnarchy && (UseStamina() && (caller.stamina < staminaCost - caller.GetEffectHasteBonus())))
        {
            return false;
        }

        return true;
    }

    public virtual CantMoveReason GetCantMoveReason(BattleEntity caller)
    {
        if (BattleControl.IsPlayerControlled(caller, true) && !PlayerTurnController.Instance.CanChoose(this, caller))
        {
            return CantMoveReason.Unknown;
        }

        if (GetBaseTarget().range != TargetArea.TargetAreaType.None && BattleControl.Instance.GetEntities(caller, GetBaseTarget()).Count == 0)
        {
            return CantMoveReason.NoTargets;
        }

        int cost = GetCost(caller);

        switch (GetCurrency(caller))
        {
            case BattleHelper.MoveCurrency.Energy:
                if (BattleControl.Instance.GetEP(caller) < cost)
                {
                    return CantMoveReason.NotEnoughEnergy;
                }
                break;
            case BattleHelper.MoveCurrency.Health:  //Note: you are not allowed to pay with all your hp (leaving you with 0)
                if (caller.hp - 1 < cost)
                {
                    return CantMoveReason.NotEnoughHealth;
                }
                break;
            case BattleHelper.MoveCurrency.Stamina:
                if (caller.stamina < cost - caller.GetEffectHasteBonus())
                {
                    return CantMoveReason.NotEnoughStamina;
                }
                break;
            case BattleHelper.MoveCurrency.Soul:
                if (BattleControl.Instance.GetSE(caller) < cost)
                {
                    return CantMoveReason.NotEnoughSoul;
                }
                break;
            case BattleHelper.MoveCurrency.Coins:
                if (BattleControl.Instance.playerData.coins < cost)
                {
                    return CantMoveReason.NotEnoughCoins;
                }
                break;
        }

        int staminaCost = cost;
        if (caller is PlayerEntity pcaller)
        {
            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Coins && pcaller.BadgeEquipped(Badge.BadgeType.GoldenEnergy))
            {
                staminaCost = cost / 5;
            }

            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Stamina && pcaller.BadgeEquipped(Badge.BadgeType.StaminaEnergy))
            {
                //the actual cost check was earlier in this case so this check doesn't really matter
                staminaCost = 0;
            }

            if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.IonizedSand)
            {
                staminaCost /= 2;
            }
            if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.TrialOfHaste)
            {
                staminaCost = 0;
            }
        }

        if (UseStamina() && (caller.stamina < staminaCost - caller.GetEffectHasteBonus()))
        {
            return CantMoveReason.NotEnoughStamina;
        }

        return CantMoveReason.Unknown;
    }

    public virtual void ChooseAction(BattleEntity caller)
    {
        //base.ChooseAction(caller);

        int cost = GetCost(caller);

        switch (GetCurrency(caller))
        {
            case BattleHelper.MoveCurrency.Energy:
                BattleControl.Instance.AddEP(caller, -cost);
                break;
            case BattleHelper.MoveCurrency.Health:  //Note: you are not allowed to pay with all your hp (leaving you with 0)
                caller.hp -= cost;
                if (caller.hp < 0)
                {
                    caller.hp = 0;
                }
                break;
            case BattleHelper.MoveCurrency.Stamina:
                caller.stamina -= cost;
                if (caller.stamina < 0)
                {
                    caller.stamina = 0;
                }
                break;
            case BattleHelper.MoveCurrency.Soul:
                BattleControl.Instance.AddSE(caller, -cost);
                break;
            case BattleHelper.MoveCurrency.Coins:
                BattleControl.Instance.AddCoins(caller, -cost);
                break;
        }

        int staminaCost = cost;
        if (caller is PlayerEntity pcaller)
        {
            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Coins && pcaller.BadgeEquipped(Badge.BadgeType.GoldenEnergy))
            {
                staminaCost = cost / 5;
            }

            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Stamina && pcaller.BadgeEquipped(Badge.BadgeType.StaminaEnergy))
            {
                //the actual cost check was earlier in this case so this check doesn't really matter
                staminaCost = 0;
            }


            if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.IonizedSand)
            {
                staminaCost /= 2;
            }
            if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.TrialOfHaste)
            {
                staminaCost = 0;
            }

            //stamina block
            if (UseStamina() && staminaCost > caller.GetRealAgility())
            {
                pcaller.staminaBlock = true;
            }
        }

        if (UseStamina() && GetCurrency(caller) != BattleHelper.MoveCurrency.Stamina)
        {
            caller.stamina -= staminaCost;
            if (caller.stamina < 0)
            {
                caller.stamina = 0;
            }
        }

        //Remove burst and haste tokens if move costs stuff
        if (GetBaseCost() > 0)
        {
            if (UseBurst())
            {
                caller.TokenRemove(Effect.EffectType.Burst);
                caller.TokenRemove(Effect.EffectType.Enervate);
            }

            if (UseFlow())
            {
                caller.TokenRemove(Effect.EffectType.Awaken);
                caller.TokenRemove(Effect.EffectType.Disorient);
            }

            if (UseStamina())
            {
                caller.TokenRemove(Effect.EffectType.Haste);
                caller.TokenRemove(Effect.EffectType.Hamper);
            }
        }

        if (GetCurrency(caller) == BattleHelper.MoveCurrency.Energy && cost > 0)
        {
            BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.PayEnergy);
        }
        BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Tactic);
    }

    public abstract TargetArea GetBaseTarget();
    public abstract string GetName();
    public abstract string GetDescription();
    public abstract bool ForfeitTurn();
    public string GetActionCommandDesc()
    {
        return "";
    }

    public virtual IEnumerator Execute(BattleEntity caller)
    {
        //BattleControl.Instance.interrupt = true;
        yield return null;
        //BattleControl.Instance.interrupt = false;
    }

    public virtual string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        return "";
    }
}

public class BA_Rest : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.None, false);
    public override string GetName() => "Rest";
    public override string GetDescription() => "Do nothing until the next turn. Gives you one turn worth of Stamina and 3 Soul Energy.";
    public override bool ForfeitTurn() => true;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        /*
        caller.stamina += caller.agility;
        if (caller.stamina >= BattleControl.Instance.GetMaxStamina(caller))
        {
            caller.stamina = BattleControl.Instance.GetMaxStamina(caller);
        }
        */
        caller.HealStamina(caller.GetRealAgility());

        Ribbon.RibbonType rt = Ribbon.RibbonType.None;
        bool ribbonPower = false;
        int ribbonMult = 1;

        float restBoost = 1;

        //BattleControl.Instance.AddSE(caller, 3);
        int value = 3;
        if (caller is PlayerEntity pcaller)
        {
            if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Sloth))
            {
                value = 9 + 9 * pcaller.BadgeEquippedCount(Badge.BadgeType.DarkConcentration);
                restBoost = 3;
            }
            else
            {
                value = 3 + 3 * pcaller.BadgeEquippedCount(Badge.BadgeType.DarkConcentration);
            }


            rt = pcaller.GetVisualRibbon().type;
            ribbonPower = pcaller.BadgeEquipped(Badge.BadgeType.RibbonPower);
            ribbonMult = pcaller.BadgeEquippedCount(Badge.BadgeType.RibbonPower);
            if (pcaller.BadgeEquipped(Badge.BadgeType.LongRest) && pcaller.lastRestTurn >= BattleControl.Instance.turnCount - 1)
            {
                restBoost *= 1 + 0.5f * pcaller.BadgeEquippedCount(Badge.BadgeType.LongRest);
            }
            pcaller.lastRestTurn = BattleControl.Instance.turnCount;
        }
        caller.HealSoulEnergy(value);



        switch (rt)
        {
            case Ribbon.RibbonType.BeginnerRibbon:
                if (ribbonPower)
                {
                    caller.HealStamina((int)(restBoost * (3 * ribbonMult) * (caller.GetRealAgility() / 2)));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Defocus, (sbyte)(1 * ribbonMult), Effect.INFINITE_DURATION));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Sunder, (sbyte)(1 * ribbonMult), Effect.INFINITE_DURATION));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Enervate, (sbyte)(1 * ribbonMult), Effect.INFINITE_DURATION));
                }
                else
                {
                    caller.HealStamina((int)(restBoost  * (caller.GetRealAgility() / 2)));
                }
                RibbonEffect(caller, new Color(0.7f, 0.3f, 0), ribbonPower);
                break;
            case Ribbon.RibbonType.ExpertRibbon:
                if (ribbonPower)
                {
                    caller.CureEffects(false);
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Seal, 1, (sbyte)(3 * ribbonMult)));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Immunity, 1, (sbyte)(restBoost * ribbonMult * 3)));
                }
                else
                {
                    caller.CureEffects(false);
                    //secret
                    if (restBoost > 1)
                    {
                        caller.InflictEffect(caller, new Effect(Effect.EffectType.Immunity, 1, (sbyte)(restBoost - 0.5f)));
                    }
                }
                RibbonEffect(caller, new Color(0.7f, 0.8f, 1f), ribbonPower);
                break;
            case Ribbon.RibbonType.ChampionRibbon:
                bool alone = true;
                List<BattleEntity> playerParty = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));
                alone = playerParty.Count == 1;
                if (ribbonPower)
                {
                    if (alone)
                    {
                        caller.HealHealth((int)(restBoost * ribbonMult * 15));
                        caller.HealEnergy((int)(restBoost * ribbonMult * 15));
                    }
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackDown, (sbyte)(1 * ribbonMult), 3));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.DefenseDown, (sbyte)(1 * ribbonMult), 3));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.EnduranceDown, (sbyte)(1 * ribbonMult), 3));
                } else
                {
                    if (alone)
                    {
                        caller.HealHealth((int)(restBoost * 5));
                        caller.HealEnergy((int)(restBoost * 5));
                    }
                }
                RibbonEffect(caller, new Color(1f, 0.7f, 0), ribbonPower);
                break;
            case Ribbon.RibbonType.StaticRibbon:
                if (ribbonPower) 
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Paralyze, 1, (sbyte)(3 * ribbonMult)));
                    caller.HealEnergy((int)(restBoost * ribbonMult * 6));
                } else
                {
                    caller.HealEnergy((int)(restBoost * 2));
                }
                RibbonEffect(caller, new Color(1f, 1f, 0.2f), ribbonPower);
                break;
            case Ribbon.RibbonType.SlimyRibbon:
                if (ribbonPower)
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Poison, 1, (sbyte)(3 * ribbonMult)));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackUp, (sbyte)(3 * ribbonMult), (sbyte)(restBoost * 3)));
                }
                else
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackUp, 1, (sbyte)(restBoost * 3)));
                }
                RibbonEffectDark(caller, new Color(1f, 0.3f, 1f), ribbonPower);
                break;
            case Ribbon.RibbonType.FlashyRibbon:
                if (ribbonPower)
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Dizzy, 1, (sbyte)(3 * ribbonMult)));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.DefenseUp, (sbyte)(3 * ribbonMult), (sbyte)(restBoost * 3)));
                }
                else
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.DefenseUp, 1, (sbyte)(restBoost * 3)));
                }
                RibbonEffect(caller, new Color(0.5f, 1f, 0.5f), ribbonPower);
                break;
            case Ribbon.RibbonType.SoftRibbon:
                if (ribbonPower)
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Sleep, 1, (sbyte)(3 * ribbonMult)));
                    caller.HealHealth((int)(restBoost * ribbonMult * 6));
                }
                else
                {
                    caller.HealHealth((int)(restBoost * 2));
                }
                RibbonEffect(caller, new Color(0.2f, 0.2f, 1f), ribbonPower);
                break;
            case Ribbon.RibbonType.ThornyRibbon:
                if (ribbonPower)
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Berserk, 1, (sbyte)(3 * ribbonMult)));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, (sbyte)(restBoost * ribbonMult * 2), Effect.INFINITE_DURATION));
                    caller.TakeDamageStatus(3 * ribbonMult);
                } else
                {
                    caller.TakeDamageStatus(1);
                }
                RibbonEffect(caller, new Color(1f, 0.2f, 1f), ribbonPower);
                break;
            case Ribbon.RibbonType.DiamondRibbon:
                if (ribbonPower)
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Freeze, 1, (sbyte)(3 * ribbonMult)));
                    caller.HealCoins((int)(restBoost * ribbonMult * 45));
                } else
                {
                    caller.HealCoins((int)(restBoost * 15));
                }
                RibbonEffectDiamond(caller, new Color(0.6f, 1f, 1f), ribbonPower);
                break;
        }

        if (caller is PlayerEntity pcallerB)
        {
            if (pcallerB.agilityRush < pcallerB.BadgeEquippedCount(Badge.BadgeType.AgilityRush) && pcallerB.stamina >= BattleControl.Instance.GetMaxStamina(pcallerB))
            {
                pcallerB.agilityRush++;
                pcallerB.InflictEffect(pcallerB, new Effect(Effect.EffectType.BonusTurns, (sbyte)(pcallerB.BadgeEquippedCount(Badge.BadgeType.AgilityRush)), Effect.INFINITE_DURATION));
            }
        }

        BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Rest);
        yield return null;
    }

    public void RibbonEffect(BattleEntity caller, Color color, bool ribbonPower)
    {
        Vector3 position = caller.transform.position + Vector3.up * (caller.height / 2);
        GameObject eo;
        if (ribbonPower)
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfettiTriple"), gameObject.transform);
        } else
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfetti"), gameObject.transform);
        }
        eo.transform.position = position;
        EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(color, 1, 1);
    }

    public void RibbonEffectDiamond(BattleEntity caller, Color color, bool ribbonPower)
    {
        Vector3 position = caller.transform.position + Vector3.up * (caller.height / 2);
        GameObject eo;
        if (ribbonPower)
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfettiDiamondTriple"), gameObject.transform);
        }
        else
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfettiDiamond"), gameObject.transform);
        }
        eo.transform.position = position;
        EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(color, 1, 1);
    }

    public void RibbonEffectDark(BattleEntity caller, Color color, bool ribbonPower)
    {
        Vector3 position = caller.transform.position + Vector3.up * (caller.height / 2);
        GameObject eo;
        if (ribbonPower)
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfettiDarkTriple"), gameObject.transform);
        }
        else
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfettiDark"), gameObject.transform);
        }
        eo.transform.position = position;
        EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(color, 1, 1);
    }
}

public class BA_Check : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Enemy, false);
    public override string GetName() => "Check";
    public override string GetDescription() => "(0 turns) Learn more about an enemy and reveal its max HP.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        GameObject bestiaryCard = Instantiate(Resources.Load<GameObject>("Menu/BestiaryCard"), MainManager.Instance.Canvas.transform);

        BestiaryCardScript bcs = bestiaryCard.GetComponent<BestiaryCardScript>();

        MainManager.Instance.SetBestiaryFlag(caller.curTarget.entityID);

        string name = BestiaryOrderEntry.GetBestiaryOrderNumberString(caller.curTarget.entityID) + ". " + BattleEntity.GetNameStatic(caller.curTarget.entityID);
        //Get sprite somehow
        Sprite sp = BattleEntity.GetBestiarySprite(caller.curTarget.entityID);
        string stat = BattleEntity.GetBestiarySideText(caller.curTarget.entityID);

        bcs.Setup(sp, name, stat);

        string tattle = caller.curTarget.GetTattle(caller.entityID);
        yield return StartCoroutine(MainManager.Instance.DisplayTextBox(tattle, caller));
        BattleControl.Instance.BroadcastEvent(caller.curTarget, BattleHelper.Event.Check);

        bcs.Fadeout();
    }
}

public class BA_Scan : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Enemy, false);
    public override string GetName() => "Scan";
    public override string GetDescription() => "Learn more about an enemy and reveal its max HP as well as all of its moves. <descriptionnoticecolor>Unlike Check, this does cost your turn.</descriptionnoticecolor>";
    public override bool ForfeitTurn() => true;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        BattleEntity target = caller.curTarget;
        string tattle = "<tail,k>" + target.GetTattle(BattleHelper.EntityID.Keru);

        GameObject bestiaryCard = Instantiate(Resources.Load<GameObject>("Menu/BestiaryCard"), MainManager.Instance.Canvas.transform);

        BestiaryCardScript bcs = bestiaryCard.GetComponent<BestiaryCardScript>();

        MainManager.Instance.SetBestiaryFlag(target.entityID);

        string name = BestiaryOrderEntry.GetBestiaryOrderNumberString(caller.curTarget.entityID) + ". " + BattleEntity.GetNameStatic(caller.curTarget.entityID);
        //Get sprite somehow
        Sprite sp = BattleEntity.GetBestiarySprite(caller.curTarget.entityID);
        string stat = BattleEntity.GetBestiarySideText(caller.curTarget.entityID);

        bcs.Setup(sp, name, stat);

        /*
        //debug: append all the move description stuff after
        for (int i = 0; i < target.moveset.Count; i++)
        {
            tattle += "<next>";
            tattle += target.moveset[i].GetName();
            tattle += ": ";
            tattle += target.moveset[i].GetDescription();
        }
        */

        //Moveset check

        if (caller.curTarget.GetEntityProperty(BattleHelper.EntityProperties.ScanHideMoves))
        {
            yield break;
        }

        string movesetViewer = "<tail,k>Here is their moveset. (<buttonsprite,A> to exit, <buttonsprite,B> to go back)<dataget," + caller.posId + ",arg,0><genericmenu,arg,4>";

        Debug.Log(target.moveset.Count);

        //name, right, usage, desc
        List<string> nameList = new List<string>();
        List<string> descList = new List<string>();

        for (int i = 0; i < target.moveset.Count; i++)
        {
            nameList.Add(FormattedString.InsertEscapeSequences(target.moveset[i].GetName()));
            descList.Add(FormattedString.InsertEscapeSequences(target.moveset[i].GetDescription()));
        }

        string scanTable = GenericBoxMenu.PackMenuString(null, null, nameList, null, null, descList);
        Debug.Log(scanTable);
        ((PlayerEntity)caller).SetScanTable(scanTable);

        int index = -1;
        while (index == -1)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBox(tattle, caller));

            yield return StartCoroutine(MainManager.Instance.DisplayTextBox(movesetViewer, caller));

            string menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out index);
        }
        BattleControl.Instance.BroadcastEvent(target, BattleHelper.Event.Check);
        bcs.Fadeout();
    }
}

public class BA_SuperSwapEntities : BattleAction //switches everyone around 1 position
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Ally, false);
    public override string GetName() => "Swap Positions <button,z>";
    public override string GetDescription() => "Switch all character's positions around. Can also be done with the <button,z> button.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override bool CanChoose(BattleEntity caller)
    {
        //reuse this condition from base battle menu
        if (!BaseBattleMenu.ZUsable())
        {
            return false;
        }

        return base.CanChoose(caller);
    }

    public override IEnumerator Execute(BattleEntity caller)
    {
        List<BattleEntity> playerParty = BattleControl.Instance.GetEntities((e) => BattleControl.IsPlayerControlled(e, true));

        for (int i = 0; i < playerParty.Count; i++)
        {
            StartCoroutine(playerParty[i].Spin(Vector3.up * 360, 0.3f));
            if (i == playerParty.Count - 1)
            {
                yield return StartCoroutine(playerParty[i].Spin(Vector3.up * 360, 0.3f));
            }
        }

        //things to swap: pos ID, home positions, positions

        //Store the info for the first entity on the list (for later)
        Vector3 tempHPos = playerParty[0].homePos;
        Vector3 tempPos = playerParty[0].transform.position;
        //Not swapping posIds makes things still work
        int tempID = playerParty[0].posId;

        BattleControl.Instance.SwapEffectCasters(playerParty);

        for (int i = 0; i < playerParty.Count; i++)
        {
            if (i == playerParty.Count - 1)
            {
                playerParty[i].homePos = tempHPos;
                playerParty[i].transform.position = tempPos;
                playerParty[i].posId = tempID;
            }
            else
            {
                playerParty[i].homePos = playerParty[i + 1].homePos;
                playerParty[i].transform.position = playerParty[i + 1].transform.position;
                playerParty[i].posId = playerParty[i + 1].posId;
            }
        }


        //wrath special case
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Wrath))
        {            
            foreach (PlayerEntity pe in playerParty)
            {
                if (pe == playerParty[0])
                {
                    pe.InflictEffectForce(pe, new Effect(Effect.EffectType.Berserk, 1, Effect.INFINITE_DURATION));
                } else
                {
                    if (!pe.BadgeEquipped(Badge.BadgeType.RagesPower) && pe.HasEffect(Effect.EffectType.Berserk) && pe.GetEffectEntry(Effect.EffectType.Berserk).duration == Effect.INFINITE_DURATION)
                    {
                        pe.RemoveEffect(Effect.EffectType.Berserk);
                    }
                }
            }
        }
    }
}

//Remnant of larger party paradigm (with max characters at once being lower than the number of characters you actually have)
/*
//get rid of target, replace with other character
public class SwitchCharacter : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.PlayerCanSwitch, false);
    public override string GetName() => "Switch Characters";
    public override string GetDescription() => GetRealDescription();
    public override bool ForfeitTurn() => true;

    //don't really want to have to make a new object just to get the description
    public static string GetRealDescription() => "Switch in a selected character with another.";

    public override IEnumerator Execute(BattleEntity caller)
    {
        BattleEntity target = caller.curTarget;
        PlayerData.PlayerDataEntry newChar = PlayerTurnController.Instance.toSwitch;
        PlayerData.PlayerDataEntry oldChar = BattleControl.Instance.playerData.GetEntry(target.entityID);
        PlayerData party = BattleControl.Instance.playerData;

        bool oldCharCanMove = target.CanMove() && PlayerTurnController.Instance.movableParty.Contains(target);

        int posid = target.posId;
        Vector3 newHomePos = BattleHelper.GetDefaultPosition(posid);
        Vector3 offscreen = Vector3.left * 10;

        BattleEntity newEntity;

        if (target.hp > 0 && newChar.hp > 0)
        {
            //Normal animation (target leaves, new char enters)
            BattleControl.Instance.RemoveEntityAtId(posid);
            
            yield return StartCoroutine(target.Move(offscreen));
            Destroy(target.gameObject);

            newEntity = BattleControl.Instance.SummonEntity(newChar, posid, offscreen);
            newEntity.homePos = newHomePos;
            yield return StartCoroutine(newEntity.Move(newHomePos));
            
            party.TryRemoveFromCurrent(target.entityID);
            party.TryAddToCurrent(newChar.entityID);
        }
        else if (target.hp > 0 && newChar.hp <= 0)
        {
            //Target leaves, picks up new char, leaves again
            BattleControl.Instance.RemoveEntityAtId(posid);
            yield return StartCoroutine(target.Move(offscreen));

            newEntity = BattleControl.Instance.SummonEntity(newChar, posid, offscreen);
            newEntity.homePos = newHomePos;

            StartCoroutine(newEntity.Move(newHomePos, target.entitySpeed));
            yield return StartCoroutine(target.Move(newHomePos));

            yield return StartCoroutine(target.Move(offscreen));
            Destroy(target.gameObject);

            party.TryRemoveFromCurrent(target.entityID);
            party.TryAddToCurrent(newChar.entityID);
        }
        else if (newChar.hp > 0 && target.hp <= 0)
        {
            //New char enters, picks up target, leaves, comes back
            BattleControl.Instance.RemoveEntityAtId(posid);
            newEntity = BattleControl.Instance.SummonEntity(newChar, posid, offscreen);
            newEntity.homePos = BattleHelper.GetDefaultPosition(posid);
            yield return StartCoroutine(newEntity.Move(newHomePos));

            StartCoroutine(newEntity.Move(offscreen, target.entitySpeed));
            yield return StartCoroutine(target.Move(offscreen, newEntity.entitySpeed));

            Destroy(target.gameObject);
            yield return StartCoroutine(newEntity.Move(newHomePos));

            party.TryRemoveFromCurrent(target.entityID);
            party.TryAddToCurrent(newChar.entityID);
        }
        else
        {
            //Caller picks up target, leaves, comes back with new char, returns to normal position
            BattleControl.Instance.RemoveEntityAtId(posid);
            newEntity = BattleControl.Instance.SummonEntity(newChar, posid, offscreen);
            newEntity.homePos = BattleHelper.GetDefaultPosition(posid);

            yield return StartCoroutine(caller.Move(target.transform.position));

            StartCoroutine(caller.Move(offscreen));
            yield return StartCoroutine(target.Move(offscreen, caller.entitySpeed));

            StartCoroutine(caller.Move(newHomePos));
            yield return StartCoroutine(newEntity.Move(newHomePos, caller.entitySpeed));

            yield return StartCoroutine(caller.Move(caller.homePos));

            Destroy(target.gameObject);

            party.TryRemoveFromCurrent(target.entityID);
            party.TryAddToCurrent(newChar.entityID);
        }


        //quick switch: remove below, make new character mobile if old character could move

        if (!oldCharCanMove)
        {
            //Expend your own turn
            PlayerTurnController.Instance.MakeImmobile(caller);
        }
    }
}
*/

public class BA_SwapEntities : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.AllyNotSelf, false);
    public override string GetName() => "Swap Positions";
    public override string GetDescription() => "Switch the position of two characters.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    //Has same limitations as Z button since you have 2 characters
    public override bool CanChoose(BattleEntity caller)
    {
        //reuse this condition from base battle menu
        if (!BaseBattleMenu.ZUsable())
        {
            return false;
        }

        return base.CanChoose(caller);
    }

    //use caller's target
    public override IEnumerator Execute(BattleEntity caller)
    {
        StartCoroutine(caller.curTarget.Spin(Vector3.up * 360, 0.3f));
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.3f));

        BattleEntity target = caller.curTarget;

        //things to swap: pos ID, home positions, positions
        int tempID = caller.posId;
        Vector3 tempHPos = caller.homePos;
        Vector3 tempPos = caller.transform.position;

        caller.posId = target.posId;
        caller.homePos = target.homePos;
        caller.transform.position = target.transform.position;

        target.posId = tempID;
        target.homePos = tempHPos;
        target.transform.position = tempPos;
    }
}

public class BA_Flee : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, true);
    public override string GetName() => "Flee";
    public override string GetDescription() => "Run from the current battle. Stamina cost is based on the level of the strongest enemy that can move, but is capped at your max Stamina. Some battles may forbid you from fleeing, while others may allow you to flee for free.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost()
    {
        if (BattleControl.Instance.GetProperty(BattleHelper.BattleProperties.FreeFlee))
        {
            return 0;
        }

        int cost = 0;
        //note: only mobile enemies 
        List<BattleEntity> enemyList = BattleControl.Instance.GetEntities((e) => (e.posId >= 0 && e.CanMove()));

        for (int i = 0; i < enemyList.Count; i++)
        {
            if ((enemyList[i].level / 2) > cost)
            {
                cost = (enemyList[i].level) / 2;
            }
        }

        return cost;
    }
    public override int GetCost(BattleEntity caller)
    {
        int c =  base.GetCost(caller);
        //Cap at half max EP so there are no unescapable overworld encounters
        if (c > BattleControl.Instance.GetMaxStamina(caller))
        {
            c = BattleControl.Instance.GetMaxStamina(caller);
        }
        return c;
    }

    public override BattleHelper.MoveCurrency GetCurrency(BattleEntity caller)
    {
        return BattleHelper.MoveCurrency.Stamina;
    }
    public override bool UseBurst()
    {
        return false;
    }

    public bool success = true;

    public override bool CanChoose(BattleEntity caller)
    {
        if (BattleControl.Instance.GetProperty(BattleHelper.BattleProperties.NoFlee))
        {
            return false;
        }

        //if you are in a win state, do not let you run away (this is a weird state)
        if (BattleControl.Instance.GetEntities(e => BattleControl.EntityCountsForBattleEnd(e) && e.posId >= 0).Count == 0)
        {
            return false;
        }

        return base.CanChoose(caller);
    }
    public override CantMoveReason GetCantMoveReason(BattleEntity caller)
    {
        if (BattleControl.Instance.GetProperty(BattleHelper.BattleProperties.NoFlee))
        {
            return CantMoveReason.BlockFlee;
        }

        //if you are in a win state, do not let you run away (this is a weird state)
        if (BattleControl.Instance.GetEntities(e => BattleControl.EntityCountsForBattleEnd(e) && e.posId >= 0).Count == 0)
        {
            return CantMoveReason.Unknown;
        }
        return base.GetCantMoveReason(caller);
    }

    public override IEnumerator Execute(BattleEntity caller)
    {
        //everyone runs, battle ends
        List<BattleEntity> playerParty = BattleControl.Instance.GetEntities((e) => e.posId < 0 && e.CanMove());
        for (int i = 0; i < playerParty.Count; i++)
        {
            StartCoroutine(playerParty[i].Jump(playerParty[i].transform.position, 0.5f, 0.25f));
            if (i == playerParty.Count - 1)
            {
                yield return StartCoroutine(playerParty[i].Jump(playerParty[i].transform.position, 0.5f, 0.25f));
            }
            Debug.Log("loop");
        }
        if (success)
        {
            for (int i = 0; i < playerParty.Count; i++)
            {
                if (i == playerParty.Count - 1)
                {
                    yield return StartCoroutine(playerParty[i].Move(Vector3.left * 10, 5.0f));
                } else
                {
                    StartCoroutine(playerParty[i].Move(Vector3.left * 10, 5.0f));
                }
            }

            yield return StartCoroutine(BattleControl.Instance.EndBattle(BattleHelper.BattleOutcome.Flee));
        } else
        {

        }
        yield return null;
    }
}


public class BA_EasyFlee : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.None, false);
    public override string GetName() => "Easy Flee";
    public override string GetDescription() => "Run from the current battle. Drains up to 5 energy from you but is otherwise free. Some battles may forbid you from fleeing, while others may allow you to flee for free.";
    public override bool ForfeitTurn() => false;


    public override int GetBaseCost() => 0;

    public override BattleHelper.MoveCurrency GetCurrency(BattleEntity caller)
    {
        return BattleHelper.MoveCurrency.Stamina;
    }

    public bool success = true;

    public override bool CanChoose(BattleEntity caller)
    {
        if (BattleControl.Instance.GetProperty(BattleHelper.BattleProperties.NoFlee))
        {
            return false;
        }

        //if you are in a win state, do not let you run away (this is a weird state)
        if (BattleControl.Instance.GetEntities(e => BattleControl.EntityCountsForBattleEnd(e) && e.posId >= 0).Count == 0)
        {
            return false;
        }

        return base.CanChoose(caller);
    }
    public override CantMoveReason GetCantMoveReason(BattleEntity caller)
    {
        if (BattleControl.Instance.GetProperty(BattleHelper.BattleProperties.NoFlee))
        {
            return CantMoveReason.BlockFlee;
        }

        //if you are in a win state, do not let you run away (this is a weird state)
        if (BattleControl.Instance.GetEntities(e => BattleControl.EntityCountsForBattleEnd(e) && e.posId >= 0).Count == 0)
        {
            return CantMoveReason.Unknown;
        }
        return base.GetCantMoveReason(caller);
    }

    public override IEnumerator Execute(BattleEntity caller)
    {
        caller.HealEnergy(-5);
        //everyone runs, battle ends
        List<BattleEntity> playerParty = BattleControl.Instance.GetEntities((e) => e.posId < 0 && e.CanMove());
        for (int i = 0; i < playerParty.Count; i++)
        {
            StartCoroutine(playerParty[i].Jump(playerParty[i].transform.position, 0.5f, 0.25f));
            if (i == playerParty.Count - 1)
            {
                yield return StartCoroutine(playerParty[i].Jump(playerParty[i].transform.position, 0.5f, 0.25f));
            }
        }
        if (success)
        {
            for (int i = 0; i < playerParty.Count; i++)
            {
                if (i == playerParty.Count - 1)
                {
                    yield return StartCoroutine(playerParty[i].Move(Vector3.left * 10, 5.0f));
                }
                else
                {
                    StartCoroutine(playerParty[i].Move(Vector3.left * 10, 5.0f));
                }
            }

            yield return StartCoroutine(BattleControl.Instance.EndBattle(BattleHelper.BattleOutcome.Flee));
        }
        else
        {

        }
        yield return null;
    }
}

public class BA_TurnRelay : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSelfMovable, false);
    public override string GetName() => "Turn Relay";
    public override string GetDescription() => "Pass a bonus turn to the other character. Only usable when the other character can move. <descriptionnoticecolor>(Base cost is equal to half your agility, ignoring the bonus for being in front)</descriptionnoticecolor>";
    public override bool ForfeitTurn() => true;

    public override int GetBaseCost() => 2;

    public override BattleHelper.MoveCurrency GetCurrency(BattleEntity caller = null)
    {
        return BattleHelper.MoveCurrency.Stamina;
    }
    public override int GetCost(BattleEntity caller)
    {
        return Mathf.CeilToInt(caller.GetBoostedAgility() / 2f);
    }
    public override bool UseBurst()
    {
        return false;
    }

    //use caller's target
    public override IEnumerator Execute(BattleEntity caller)
    {
        StartCoroutine(caller.curTarget.Spin(Vector3.up * 360, 0.3f));
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.3f));

        BattleEntity target = caller.curTarget;

        //apply boost
        caller.InflictEffectForce(target, new Effect(Effect.EffectType.BonusTurns, 1, Effect.INFINITE_DURATION));

        if (caller is PlayerEntity pcaller)
        {
            if (pcaller.BadgeEquipped(Badge.BadgeType.UnsteadyStance))
            {
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Dizzy, 1, (sbyte)(2 * pcaller.BadgeEquippedCount(Badge.BadgeType.UnsteadyStance))));
                target.HealStamina(pcaller.BadgeEquippedCount(Badge.BadgeType.UnsteadyStance) * (target.GetRealAgility() + target.GetEffectHasteBonus() + target.GetBadgeAgilityBonus()));
            }
        }
    }
}

public class BA_EffectRelay : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.AllyNotSelf, false);
    public override string GetName() => "Effect Relay";
    public override string GetDescription() => "(0 turns) Pass most effects to the other character. Effects immune to effect removal won't be passed over.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 4;

    //use caller's target
    public override IEnumerator Execute(BattleEntity caller)
    {
        StartCoroutine(caller.curTarget.Spin(Vector3.up * 360, 0.3f));
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.3f));

        BattleEntity target = caller.curTarget;

        //transfer
        for (int i = 0; i < caller.effects.Count; i++)
        {
            if (Effect.IsCurable(caller.effects[i].effect, false) || Effect.IsCleanseable(caller.effects[i].effect, false))
            {
                Effect e = caller.effects[i];
                caller.effects.Remove(e);
                i--;
                caller.InflictEffectForce(target, e, e.casterID);
            }
        }
    }
}


public class BA_Cheat_RandomMove : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Anyone, false);
    public override string GetName() => "(Cheat) Random Move";
    public override string GetDescription() => "Perform a random move out of your available moves that can target the chosen target. The move done is at its maximum available level and costs nothing. If none of your moves can target your chosen target, you get your turn back.";
    public override bool ForfeitTurn() => true;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        List<Move> movesetList = new List<Move>();

        for (int i = 0; i < caller.moveset.Count; i++)
        {
            movesetList.Add(caller.moveset[i]);
        }

        while (movesetList.Count > 0)
        {
            //Try
            int randomOffset = Random.Range(0, movesetList.Count - 1);

            Move attempt = movesetList[randomOffset];

            if (attempt.GetTargetArea(caller, attempt.GetMaxLevel(caller)).checkerFunction(caller, caller.curTarget))
            {
                attempt.level = attempt.GetMaxLevel(caller);
                //Success
                caller.currMove = attempt;

                caller.moveExecuting = true;
                caller.moveActive = true;
                StartCoroutine(caller.ExecuteMoveCoroutine(attempt.GetMaxLevel(caller)));
                //BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Skill);
                yield return new WaitUntil(() => caller == null || !caller.moveExecuting);
                yield break;
            } else
            {
                //No
                movesetList.Remove(attempt);
            }
        }

        //Failure
        caller.actionCounter--;
        caller.InflictEffectForce(caller, new Effect(Effect.EffectType.BonusTurns, Effect.INFINITE_DURATION, 1));
        yield return null;
    }
}

public class BA_Cheat_RandomItem : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Anyone, false);
    public override string GetName() => "(Cheat) Random Item";
    public override string GetDescription() => "Use a random item out of all possible item types against the target. This does not use items in your inventory, and items that produce other items will not produce anything with this action.";
    public override bool ForfeitTurn() => true;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        List<Item.ItemType> itemList = new List<Item.ItemType>();

        //possibly a lot of iterations
        //but modern computers are fast so 5000 iterations is still fast?

        bool enemyTarget = caller.curTarget.posId >= 0;

        bool ienemyTarget = false;

        for (int i = 1; i < (int)Item.ItemType.EndOfTable; i++)
        {
            //prefilter because most items are not attacking items
            ienemyTarget = Item.GetItemDataEntry((Item.ItemType)i).isAttackItem;

            if (enemyTarget == ienemyTarget)
            {
                itemList.Add((Item.ItemType)i);
            }
        }

        while (itemList.Count > 0)
        {
            //Try
            int randomOffset = Random.Range(0, itemList.Count - 1);

            TargetArea ta = Item.GetTarget(itemList[randomOffset]);

            if (ta.checkerFunction(caller, caller.curTarget))
            {
                //note: bypassing ChooseMove avoids calling the remove item thing
                Move im = Item.GetItemMoveScript(new Item(itemList[randomOffset]));

                caller.currMove = im;
                caller.moveExecuting = true;
                caller.moveActive = true;
                StartCoroutine(caller.ExecuteMoveCoroutine());
                //BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Skill);
                yield return new WaitUntil(() => caller == null || !caller.moveExecuting);

                yield break;
            }

            //Fail
            itemList.RemoveAt(randomOffset);
        }

        //Failure (somehow)
        caller.actionCounter--;
        caller.InflictEffectForce(caller, new Effect(Effect.EffectType.BonusTurns, Effect.INFINITE_DURATION, 1));
        yield return null;
    }
}

public class BA_Cheat_BonusTurn : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.None, false);
    public override string GetName() => "(Cheat) Bonus Turn";
    public override string GetDescription() => "(0 turns) Gives you a Bonus Turn.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        caller.InflictEffectForce(caller, new Effect(Effect.EffectType.BonusTurns, 1, Effect.INFINITE_DURATION));
        yield return null;
    }
}

public class BA_Cheat_Kill : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Enemy, false);
    public override string GetName() => "(Cheat) Kill";
    public override string GetDescription() => "(0 turns) Deals lethal damage to the target.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        caller.DealDamage(caller.curTarget, caller.curTarget.hp, BattleHelper.DamageType.Normal, (ulong)(BattleHelper.DamageProperties.Hardcode | BattleHelper.DamageProperties.Static));
        yield return null;
    }
}

public class BA_Cheat_Flee : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.None, false);
    public override string GetName() => "(Cheat) Flee";
    public override string GetDescription() => "Run from the current battle. Unlike normal version, this one is free and can always be used.";
    public override bool ForfeitTurn() => false;


    public override int GetBaseCost() => 0;

    public override BattleHelper.MoveCurrency GetCurrency(BattleEntity caller)
    {
        return BattleHelper.MoveCurrency.Stamina;
    }

    public bool success = true;

    public override bool CanChoose(BattleEntity caller)
    {
        return base.CanChoose(caller);
    }
    public override CantMoveReason GetCantMoveReason(BattleEntity caller)
    {
        return base.GetCantMoveReason(caller);
    }

    public override IEnumerator Execute(BattleEntity caller)
    {
        //everyone runs, battle ends
        List<BattleEntity> playerParty = BattleControl.Instance.GetEntities((e) => e.posId < 0 && e.CanMove());
        for (int i = 0; i < playerParty.Count; i++)
        {
            StartCoroutine(playerParty[i].Jump(playerParty[i].transform.position, 0.5f, 0.25f));
            if (i == playerParty.Count - 1)
            {
                yield return StartCoroutine(playerParty[i].Jump(playerParty[i].transform.position, 0.5f, 0.25f));
            }
        }
        if (success)
        {
            for (int i = 0; i < playerParty.Count; i++)
            {
                if (i == playerParty.Count - 1)
                {
                    yield return StartCoroutine(playerParty[i].Move(Vector3.left * 10, 5.0f));
                }
                else
                {
                    StartCoroutine(playerParty[i].Move(Vector3.left * 10, 5.0f));
                }
            }

            yield return StartCoroutine(BattleControl.Instance.EndBattle(BattleHelper.BattleOutcome.Flee));
        }
        else
        {

        }
        yield return null;
    }
}

public class BA_Cheat_Win : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.None, false);
    public override string GetName() => "(Cheat) Win";
    public override string GetDescription() => "Win the current battle. Does not kill all enemies, so will not give you XP from them.";
    public override bool ForfeitTurn() => false;


    public override int GetBaseCost() => 0;

    public override BattleHelper.MoveCurrency GetCurrency(BattleEntity caller)
    {
        return BattleHelper.MoveCurrency.Stamina;
    }

    public bool success = true;

    public override bool CanChoose(BattleEntity caller)
    {
        return base.CanChoose(caller);
    }
    public override CantMoveReason GetCantMoveReason(BattleEntity caller)
    {
        return base.GetCantMoveReason(caller);
    }

    public override IEnumerator Execute(BattleEntity caller)
    {
        yield return StartCoroutine(BattleControl.Instance.EndBattle(BattleHelper.BattleOutcome.Win));
    }
}

public class BA_Cheat_Lose : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.None, false);
    public override string GetName() => "(Cheat) Lose";
    public override string GetDescription() => "Lose the current battle.";
    public override bool ForfeitTurn() => false;


    public override int GetBaseCost() => 0;

    public override BattleHelper.MoveCurrency GetCurrency(BattleEntity caller)
    {
        return BattleHelper.MoveCurrency.Stamina;
    }

    public bool success = true;

    public override bool CanChoose(BattleEntity caller)
    {
        return base.CanChoose(caller);
    }
    public override CantMoveReason GetCantMoveReason(BattleEntity caller)
    {
        return base.GetCantMoveReason(caller);
    }

    public override IEnumerator Execute(BattleEntity caller)
    {
        yield return StartCoroutine(BattleControl.Instance.EndBattle(BattleHelper.BattleOutcome.Death));
    }
}

public class BA_Cheat_SeeDefenseTable : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Enemy, false);
    public override string GetName() => "(Cheat) See Defense Table";
    public override string GetDescription() => "(0 Turns - No effect when used) See the defense table of the target on hover";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {       
        yield return null;
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        string output = "";
        for (int i = 0; i < target.defenseTable.Count; i++)
        {
            if (i > 0)
            {
                output += "\n";
            }
            output += target.defenseTable[i].type;
            output += ": ";
            if (target.defenseTable[i].amount > DefenseTableEntry.IMMUNITY_CONSTANT)
            {
                output += "Absorb";
            }
            else if (target.defenseTable[i].amount == DefenseTableEntry.IMMUNITY_CONSTANT)
            {
                output += "Immunity";
            }
            else
            {
                output += target.defenseTable[i].amount;
            }
        }
        return output;
    }
}

public class BA_Cheat_SeeStatusTable : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Enemy, false);
    public override string GetName() => "(Cheat) See Status Table";
    public override string GetDescription() => "(0 Turns - No effect when used) See the status table of the target on hover";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        yield return null;
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        string output = "";

        //Add in the status max turn info too
        output += "Turn Limit: (" + target.statusMaxTurns + "/" + target.baseStatusMaxTurns + ")\n";

        for (int i = 0; i < target.statusTable.Count; i++)
        {
            if (i > 0)
            {
                output += "\n";
            }
            output += target.statusTable[i].status;
            output += ": ";
            output += target.statusTable[i].susceptibility;
            output += "(x" + target.statusTable[i].turnMod + ")";
        }
        return output;
    }
}

public class BA_Cheat_SeeStatChanges : BattleAction
{
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Anyone, false);
    public override string GetName() => "(Cheat) See Stat Changes";
    public override string GetDescription() => "(0 Turns - No effect when used) See the stat changes of the target on hover";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        yield return null;
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        string output = "";

        output = output + "Attack: E(" + target.GetEffectAttackBonus() + ") B(" + target.GetBadgeAttackBonus() + ")\n";
        output = output + "Defense: E(" + target.GetEffectDefenseBonus() + ") B(" + target.GetBadgeDefenseBonus() + ")\n";
        output = output + "Endurance: E(" + target.GetEffectEnduranceBonus() + ") B(" + target.GetBadgeEnduranceBonus() + ")\n";
        output = output + "Haste: E(" + target.GetEffectHasteBonus() + ") B(" + target.GetBadgeAgilityBonus() + ")\n";
        output = output + "Flow: E(" + target.GetEffectFlowBonus() + ") B(" + target.GetBadgeFlowBonus() + ")\n";
        output = output + "Resistance: E(" + target.GetEffectResistanceBonus() + ") B(" + target.GetBadgeResistanceBonus() + ")";
        return output;
    }
}


public class BA_BadgeSwap : BattleAction
{
    public Badge badge;
    public BadgeMenuEntry.EquipType et;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.None, false);
    public override string GetName() => "Badge Swap Action";
    public override string GetDescription() => "(0 turns) <color,red>DESCRIPTION SHOULD NOT BE VISIBLE</color> Equips or unequips the badge on the target character. User can only unequip badges that they have equipped.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        PlayerData pd = BattleControl.Instance.playerData;

        //Debug.Log("Badge Swap: " + et + " " + badge);

        //what to do?
        switch (et)
        {
            case BadgeMenuEntry.EquipType.Party:
                pd.partyEquippedBadges.Remove(badge);
                pd.equippedBadges.Remove(badge);
                break;
            case BadgeMenuEntry.EquipType.Wilex:
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).equippedBadges.Remove(badge);
                pd.equippedBadges.Remove(badge);
                break;
            case BadgeMenuEntry.EquipType.Luna:
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).equippedBadges.Remove(badge);
                pd.equippedBadges.Remove(badge);
                break;
            case BadgeMenuEntry.EquipType.None:
                BadgeDataEntry bde = Badge.GetBadgeDataEntry(badge);
                if (bde.singleOrParty)
                {
                    pd.partyEquippedBadges.Add(badge);
                }
                else
                {
                    pd.GetPlayerDataEntry(caller.entityID).equippedBadges.Add(badge);
                }
                pd.equippedBadges.Add(badge);
                break;
        }
        pd.usedSP = pd.CalculateUsedSP();
        //Debug.Log("Used SP is now " + pd.usedSP);
        pd.UpdateMaxStats();

        BattleControl.Instance.badgeSwapUses++;

        //update max stats in BattleControl
        //but I need to check for the permanent effects that mess with max stats

        //refresh entire moveset
        List<PlayerEntity> pe = BattleControl.Instance.GetPlayerEntities();
        foreach (PlayerEntity p in pe)
        {
            p.jumpMoves = new List<PlayerMove>();
            p.weaponMoves = new List<PlayerMove>();
            foreach (PlayerMove des in p.GetComponents<PlayerMove>())
            {
                Destroy(des);
            }
            p.AddMoves();
            p.tactics = new List<BattleAction>();
            foreach (BattleAction des in p.GetComponents<BattleAction>())
            {
                Destroy(des);
            }
            p.AddTactics();


            p.ValidateEffects();
        }

        yield return null;
    }
}

public class BA_RibbonSwap : BattleAction
{
    public Ribbon ribbon;
    public BadgeMenuEntry.EquipType et;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.None, false);
    public override string GetName() => "Ribbon Swap Action";
    public override string GetDescription() => "(0 turns) <color,red>DESCRIPTION SHOULD NOT BE VISIBLE</color> Equips or unequips the ribbon on the target character. User can only unequip the ribbon they have equipped.";
    public override bool ForfeitTurn() => false;

    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller)
    {
        PlayerData pd = BattleControl.Instance.playerData;

        //what to do?
        switch (et)
        {
            case BadgeMenuEntry.EquipType.Party:
                //???
                break;
            case BadgeMenuEntry.EquipType.Wilex:
            case BadgeMenuEntry.EquipType.Luna:
                //unequip self
                pd.GetPlayerDataEntry(caller.entityID).ribbon = default;
                break;
            case BadgeMenuEntry.EquipType.None:
                pd.GetPlayerDataEntry(caller.entityID).ribbon = ribbon;
                break;
        }

        yield return null;
    }
}

