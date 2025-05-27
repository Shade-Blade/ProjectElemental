using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using static Item;

public class Item_GenericConsumable : ItemMove
{
    public bool GetMultiTarget()
    {
        if (forceSingleTarget || GetModifier() == ItemModifier.Focus)
        {
            return false;
        }

        if (forceMultiTarget || GetModifier() == ItemModifier.Spread)
        {
            return true;
        }

        return Item.GetProperty(GetItem(), Item.ItemProperty.TargetAll) != null;
    }
    public override bool CanChoose(BattleEntity caller, int level = 1)
    {
        if (Item.GetProperty(GetItem(), Item.ItemProperty.NoBattle) != null)
        {
            return false;
        }

        bool limited = Item.GetProperty(GetItem(), Item.ItemProperty.Limited) != null;

        if (limited)
        {
            if (BattleControl.Instance.GetUsedItemInventory(caller).Find((e) => (e.type == GetItem().type)).type == GetItem().type)
            {
                return false;
            }
        }

        if ((Item.GetProperty(GetItem(), Item.ItemProperty.Quick) != null || GetModifier() == ItemModifier.Quick) && caller is PlayerEntity pcaller)
        {
            if (pcaller.QuickSupply > 0)
            {
                return false;
            }
        }

        return base.CanChoose(caller, level);
    }

    //public override float GetBasePower() => 15.0f;
    //public override Item.ItemType GetItemType() => Item.ItemType.DebugHeal;

    public Item_GenericConsumable()
    {
    }

    /*
    public override IEnumerator UseAnim(BattleEntity caller)
    {
        Item.ItemType it = GetItemType();

        ItemDataEntry ide = Item.GetItemDataEntry(it);
        Debug.Log("Item use anim: " + GetItemType() + " " + ide.useAnim);
        yield return null;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (GetItemType() == ItemType.CursedStew)
        {
            MainManager.Instance.AwardAchievement(MainManager.Achievement.ACH_CursedStew);
        }

        //BattleEntity target = caller.curTarget;

        caller.SetAnimation("itemuse");
        yield return StartCoroutine(DefaultStartAnim(caller));
        caller.SetIdleAnimation();

        ItemDataEntry ide = GetItemDataEntry(GetItem());

        bool dual = GetMultiTarget();

        TargetArea ta = GetBaseTarget();
        //Use this to build the list of targets
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, ta);
        if (!dual)
        {
            targets = new List<BattleEntity>
            {
                caller.curTarget
            };
        }

        //animation
        switch (GetItemDataEntry(item.type).useAnim)
        {
            case ItemUseAnim.Eat:
            case ItemUseAnim.EatBad:
            case ItemUseAnim.EatGood:
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].CanMove())
                    {
                        targets[i].SetAnimation("itemeat");
                    }
                }
                yield return new WaitForSeconds(1f);
                break;
            case ItemUseAnim.Drink:
            case ItemUseAnim.DrinkBad:
            case ItemUseAnim.DrinkGood:
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].CanMove())
                    {
                        targets[i].SetAnimation("itemdrink");
                    }
                }
                yield return new WaitForSeconds(1f);
                break;
            case ItemUseAnim.None:
                break;
        }
        switch (GetItemDataEntry(item.type).useAnim)
        {
            case ItemUseAnim.EatBad:
            case ItemUseAnim.DrinkBad:
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].CanMove())
                    {
                        targets[i].SetAnimation("itembad");
                    }
                }
                break;
            case ItemUseAnim.EatGood:
            case ItemUseAnim.DrinkGood:
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].CanMove())
                    {
                        targets[i].SetAnimation("itemgood");
                    }
                }
                break;
            case ItemUseAnim.None:
                break;
            default:
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].CanMove())
                    {
                        targets[i].SetIdleAnimation();
                    }
                }
                break;
        }

        //level 1 is default, so level 1 = 1x multiplier (i.e. boost of 0)
        yield return StartCoroutine(ExecuteEffect(caller, ide, Item.GetItemBoost(level - 1)));

        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].SetIdleAnimation();
        }

        //
        yield return StartCoroutine(ProducerAnim(caller));

        if (caller is PlayerEntity pcaller)
        {
            yield return new WaitForSeconds(0.5f);
            if (pcaller.BadgeEquipped(Badge.BadgeType.ItemRebate))
            {
                pcaller.HealCoins((int)(ide.sellPrice * 0.75f * pcaller.BadgeEquippedCount(Badge.BadgeType.ItemRebate)));
            }
        }
    }

    public virtual IEnumerator ExecuteEffect(BattleEntity caller, ItemDataEntry ide, float multiplier)
    {
        TargetArea ta = GetBaseTarget();
        //Use this to build the list of targets
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, ta);
        if (!GetMultiTarget())
        {
            targets = new List<BattleEntity>
            {
                caller.curTarget
            };
        }
        else
        {
            //note: by convention, put the caller first in the target list
            if (targets.IndexOf(caller) > 0)    //(-1 and 0 cause this to be skipped)
            {
                targets.Remove(caller);
                targets.Insert(0, caller);
            }
        }

        float boost = 1;
        if (caller.HasEffect(Effect.EffectType.ItemBoost))
        {
            boost = Item.GetItemBoost(caller.GetEffectEntry(Effect.EffectType.ItemBoost).potency);
        }
        if (GetModifier() == ItemModifier.Glistening)
        {
            boost *= 1.5f;
        }
        if (GetModifier() == ItemModifier.Echo)
        {
            boost *= 0.80001f;
        }
        if (GetModifier() == ItemModifier.Echoed)
        {
            boost *= 0.40001f;
        }
        int powerCount = 1;
        if (Item.GetProperty(ide, Item.ItemProperty.Stack) != null)
        {
            //powerCount = BattleControl.Instance.playerData.CountAllOfType(GetItemType());
            //Note: RemoveItemUsed is called immediately before executing this
            //  (which means there is no way to count stack items here ): )
            powerCount = itemCount;
            if (powerCount < 1)
            {
                powerCount = 1;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.Unity) != null)
        {
            powerCount *= itemCount;
            if (powerCount < 1)
            {
                powerCount = 1;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeBoostTen) != null)
        {
            if (BattleControl.Instance.turnCount > 10)
            {
                boost *= 10;
            }
            else
            {
                boost *= BattleControl.Instance.turnCount;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeBoostTwenty) != null)
        {
            if (BattleControl.Instance.turnCount > 20)
            {
                boost *= 20;
            }
            else
            {
                boost *= BattleControl.Instance.turnCount;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeWeaken) != null)
        {
            if (BattleControl.Instance.turnCount >= 10)
            {
                boost *= 0.100001f;
            }
            else
            {
                boost *= (11 - BattleControl.Instance.turnCount) * 0.100001f;
            }
        }
        boost *= multiplier * powerCount;
        if (Item.GetProperty(ide, Item.ItemProperty.Disunity) != null)
        {
            boost /= itemCount;
        }
        boost *= caller.GetItemUseBonus();    //(effect item boost is already calculated)

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleOnTurn1) != null && BattleControl.Instance.turnCount == 1)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtLowHP) != null && caller.hp <= PlayerData.PlayerDataEntry.GetDangerHP(caller.entityID))
        {
            boost *= 2;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtMaxHP) != null && caller.hp == caller.maxHP)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtLowItems) != null && BattleControl.Instance.GetItemInventory(caller).Count <= 5)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.BoostEnemies) != null)
        {
            int ecount = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveEnemy)).Count;
            if (ecount > 4)
            {
                ecount = 4;
            }
            if (ecount <= 1)
            {
                ecount = 1;
            }
            boost *= ecount;
        }


        //Remove it here in case you receive it later
        caller.TokenRemove(Effect.EffectType.ItemBoost);

        int hpheal = (int)(ide.hp * boost);
        int epheal = (int)(ide.ep * boost);
        int seheal = (int)(ide.se * boost);
        int staminaheal = (int)(ide.stamina * boost);
        Effect[] statusList = (Effect[])(ide.effects.Clone());

        //Apply heal over time
        Effect[] healOverTimeEffects = new Effect[0];
        Effect[] tempHealOverTime = new Effect[3];

        //bool isHealOverTime = false;

        sbyte healOverTimeMult = 0;
        if (Item.GetProperty(ide, Item.ItemProperty.HealOverTime) != null)
        {
            healOverTimeMult = 3;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.SlowHealOverTime) != null)
        {
            healOverTimeMult = 5;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.SixHealOverTime) != null)
        {
            healOverTimeMult = 6;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.EightHealOverTime) != null)
        {
            healOverTimeMult = 8;
        }

        if (healOverTimeMult != 0)
        {
            //un-boost the heals (to not boost twice)
            hpheal = ide.hp;
            epheal = ide.ep;
            seheal = ide.se;

            //isHealOverTime = true;
            int count = 0;
            if (hpheal > 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.HealthRegen, (sbyte)(hpheal / healOverTimeMult), healOverTimeMult);
                count++;
            }
            if (hpheal < 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.HealthLoss, (sbyte)(-hpheal / healOverTimeMult), healOverTimeMult);
                count++;
            }

            if (epheal > 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.EnergyRegen, (sbyte)(epheal / healOverTimeMult), healOverTimeMult);
                count++;
            }
            if (epheal < 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.EnergyLoss, (sbyte)(-epheal / healOverTimeMult), healOverTimeMult);
                count++;
            }

            if (seheal > 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.SoulRegen, (sbyte)(seheal / healOverTimeMult), healOverTimeMult);
                count++;
            }
            if (seheal < 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.SoulLoss, (sbyte)(-seheal / healOverTimeMult), healOverTimeMult);
                count++;
            }

            healOverTimeEffects = new Effect[count];

            for (int i = 0; i < count; i++)
            {
                healOverTimeEffects[i] = tempHealOverTime[i];
                //Debug.Log(healOverTimeEffects[i]);
            }

            //don't use the normal heal code later
            hpheal = 0;
            epheal = 0;
            seheal = 0;
        }

        //Boost the effects (boost of 1 is nothing)
        if (boost != 1)
        {
            for (int i = 0; i < statusList.Length; i++)
            {
                //boost by duration
                //if duration == Effect.INFINITE_DURATION, boost by potency
                //blacklist some effect types to prevent some overpowered things
                //(the permanent stat increases, item boost itself)
                //I am being generous right now, don't make me regret it
                /*
                if (Effect.GetEffectClass(statusList[i].effect) == Effect.EffectClass.Static)
                {
                    continue;
                }
                */

                //yeah no
                if (statusList[i].effect == Effect.EffectType.ItemBoost || statusList[i].effect == Effect.EffectType.Miracle)
                {
                    continue;
                }

                statusList[i] = statusList[i].BoostCopy(boost);
            }

            for (int i = 0; i < healOverTimeEffects.Length; i++)
            {
                healOverTimeEffects[i] = healOverTimeEffects[i].BoostCopy(boost);
            }
        }

        //Apply curing effects here
        bool cure = Item.GetProperty(ide, Item.ItemProperty.Cure) != null;
        bool cureBonus = Item.GetProperty(ide, Item.ItemProperty.CureBonus) != null;

        if (cure || cureBonus)
        {
            int curePower = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                curePower += targets[i].CureEffects(false);
            }

            if (cureBonus)
            {
                for (int i = 0; i < statusList.Length; i++)
                {
                    //yeah no
                    if (statusList[i].effect == Effect.EffectType.ItemBoost || statusList[i].effect == Effect.EffectType.Miracle)
                    {
                        continue;
                    }

                    statusList[i] = statusList[i].BoostCopy(curePower);
                }
            }
        }


        bool delay = false;
        int overheal = 0;

        //these calculate healing with variables or otherwise do weird things
        //note that this list should only include things that make sense to boost by multiplying
        //(so stat swap = not here)
        //(strange salad is not here)
        switch (GetItemType())
        {
            case Item.ItemType.WarpedCookie:
                //heal for other stat
                hpheal = (int)(BattleControl.Instance.GetEP(caller) * boost);
                for (int i = 0; i < targets.Count; i++)
                {
                    epheal += targets[0].hp;
                }
                epheal /= targets.Count;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.GoldenTea:
                hpheal = -caller.hp / 2;
                hpheal = (int)(hpheal * boost);
                break;
            case Item.ItemType.BrightSoup:
                epheal = -BattleControl.Instance.GetEP(caller) / 2;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.PineappleJuice:
                epheal = -BattleControl.Instance.GetEP(caller) / 2;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.MushroomSoup:
                hpheal = -caller.hp / 2;
                epheal = -BattleControl.Instance.GetEP(caller) / 2;
                hpheal = (int)(hpheal * boost);
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.CursedStew:
                hpheal = (int)(-caller.hp * boost);
                epheal = (int)(-BattleControl.Instance.GetEP(caller) * boost);
                break;
            case Item.ItemType.AetherSalad:
                epheal = (int)(-BattleControl.Instance.GetEP(caller) * boost);
                break;
            case Item.ItemType.StellarCandy:
                hpheal = (int)(-caller.hp * boost);
                break;
            case Item.ItemType.AetherPudding:
                hpheal = (int)(-21 * boost);
                epheal = (int)(-21 * boost);
                break;
        }

        bool damage = Item.GetProperty(ide, Item.ItemProperty.MinusHPIsDamage) != null;
        if (hpheal != 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                int targetHeal = (int)(hpheal * targets[i].GetItemReceiveBonus());
                if (targetHeal < 0 && damage)
                {
                    targets[i].TakeDamageStatus(-targetHeal);
                } else
                {
                    if (Item.GetProperty(ide, Item.ItemProperty.Revive) != null && targetHeal > 0 && targets[i].hp != 0)
                    {
                        BattleControl.Instance.CreateReviveParticles(targets[i], 1 + 3 * Mathf.Clamp(targetHeal / 60f, 0, 1));    //note: the heal health call produces revive particles if it revives
                    }

                    overheal += targets[i].HealHealthTrackOverhealPay(targetHeal);
                }
            }
            delay = true;
        }
        if (epheal != 0)
        {
            if (delay)
            {
                yield return new WaitForSeconds(0.5f);
            }

            int targetEnergy = (int)(epheal * targets[0].GetItemReceiveBonus());

            overheal += targets[0].HealEnergyTrackOverhealPay(targetEnergy);
            delay = true;
        }
        if (seheal != 0)
        {
            if (delay)
            {
                yield return new WaitForSeconds(0.5f);
            }

            int targetSoul = (int)(seheal * targets[0].GetItemReceiveBonus());

            overheal += targets[0].HealSoulEnergyTrackOverhealPay(targetSoul);
            delay = true;
        }
        if (staminaheal != 0)
        {
            if (delay)
            {
                yield return new WaitForSeconds(0.5f);
            }
            for (int i = 0; i < targets.Count; i++)
            {
                int targetStamina = (int)(staminaheal * targets[i].GetItemReceiveBonus());

                overheal += targets[i].HealStaminaTrackOverhealPay(targetStamina);
            }
            delay = true;
        }

        //cap overheal to avoid sussery
        if (overheal > 100)
        {
            overheal = 100;
        }

        sbyte trueOverheal = 0;
        if (ide.overhealDivisor > 0 && overheal > 0)
        {
            //Debug.Log("Ceil to int: " + overheal + " / " + ide.overhealDivisor + " = " + ((overheal + 0.0f) / ide.overhealDivisor));
            trueOverheal = (sbyte)(Mathf.CeilToInt((overheal + 0.0f) / ide.overhealDivisor));

            //cap overheal to avoid sussery
            //if (trueOverheal > 10)
            //{
            //    trueOverheal = 10;
            //}
        }
        //overheal divisor of 0 is no overheal


        //put before applying effects
        if (Item.GetProperty(ide, Item.ItemProperty.Miracle) != null)
        {
            if (delay)
            {
                yield return new WaitForSeconds(0.5f);
            }

            //apply miracle effect
            for (int j = 0; j < targets.Count; j++)
            {
                if (targets[j].hp == 0)
                {
                    targets[j].HealHealth(targets[j].maxHP);
                }
                else
                {
                    BattleControl.Instance.CreateReviveParticles(targets[j], 4);    //note: the heal health call produces revive particles if it revives
                    caller.InflictEffect(targets[j], new Effect(Effect.EffectType.Miracle, 1, Effect.INFINITE_DURATION));
                }
            }
            delay = true;
        }


        //Apply overheal effects
        if (ide.overhealDivisor > 0 && overheal > 0)
        {
            if (delay)
            {
                yield return new WaitForSeconds(0.5f);
            }
            for (int i = 0; i < targets.Count; i++)
            {
                switch (ide.overheal)
                {
                    case Item.OverhealPayEffect.Attack:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.AttackUp, trueOverheal, 3));
                        break;
                    case Item.OverhealPayEffect.Defense:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.DefenseUp, trueOverheal, 3));
                        break;
                    case Item.OverhealPayEffect.Endurance:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.EnduranceUp, trueOverheal, 3));
                        break;
                    case Item.OverhealPayEffect.Stamina:
                        targets[i].HealStamina(trueOverheal);
                        break;
                    case Item.OverhealPayEffect.SoulEnergy:
                        targets[i].HealSoulEnergy(trueOverheal);
                        break;
                    case Item.OverhealPayEffect.AttackShort:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.AttackUp, trueOverheal, 2));
                        break;
                    case Item.OverhealPayEffect.DefenseShort:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.DefenseUp, trueOverheal, 2));
                        break;
                    case Item.OverhealPayEffect.EnduranceShort:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.EnduranceUp, trueOverheal, 2));
                        break;
                    case Item.OverhealPayEffect.AttackDefense:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.AttackUp, trueOverheal, 3));
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.DefenseUp, trueOverheal, 3));
                        break;
                    case Item.OverhealPayEffect.FocusAttack:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.Focus, trueOverheal, Effect.INFINITE_DURATION));
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.AttackUp, trueOverheal, 3));
                        break;
                    case Item.OverhealPayEffect.AbsorbDefense:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.Absorb, trueOverheal, Effect.INFINITE_DURATION));
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.DefenseUp, trueOverheal, 3));
                        break;
                    case Item.OverhealPayEffect.Focus:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.Focus, trueOverheal, Effect.INFINITE_DURATION));
                        break;
                    case Item.OverhealPayEffect.Absorb:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.Absorb, trueOverheal, Effect.INFINITE_DURATION));
                        break;
                    case Item.OverhealPayEffect.Burst:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.Burst, trueOverheal, Effect.INFINITE_DURATION));
                        break;
                    case Item.OverhealPayEffect.HPRegen:
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.HealthRegen, trueOverheal, 3));
                        break;
                    case Item.OverhealPayEffect.EPRegen:
                        if (i != 0)
                        {
                            continue;
                        }
                        caller.InflictEffect(targets[i], new Effect(Effect.EffectType.EnergyRegen, trueOverheal, 3));
                        break;
                }
            }
        }

        //Apply weird effects
        //These are not affected by boosting for the most part since that doesn't necessarily make sense in most cases
        switch (GetItemType())
        {
            case Item.ItemType.WarpedLeaf:
                //Swap HP and EP

                //new ep = average of target hp
                //new hp = ep
                int tempep = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    tempep += targets[i].hp;
                    targets[i].hp = BattleControl.Instance.GetEP(caller);

                    if (targets[i].hp > targets[i].maxHP)
                    {
                        targets[i].hp = targets[i].maxHP;
                    }
                    if (targets[i].hp < 0)
                    {
                        targets[i].hp = 0;
                    }
                }
                tempep /= targets.Count;

                BattleControl.Instance.SetEP(caller, tempep);
                break;
            case Item.ItemType.WarpedTea:
                //halves missing hp, ep
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].hp = (targets[i].maxHP + targets[i].hp) / 2;
                }
                BattleControl.Instance.SetEP(caller, (BattleControl.Instance.GetMaxEP(caller) + BattleControl.Instance.GetEP(caller)) / 2);
                break;
            case Item.ItemType.StrangeBud:
                //inverts missing hp, ep
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].hp = targets[i].maxHP - targets[i].hp;
                }
                BattleControl.Instance.SetEP(caller, BattleControl.Instance.GetMaxEP(caller) - BattleControl.Instance.GetEP(caller));
                break;
            case Item.ItemType.StrangeSalad:
                //set to half hp, ep
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].hp = targets[i].maxHP / 2;
                }
                BattleControl.Instance.SetEP(caller, BattleControl.Instance.GetMaxEP(caller) / 2);
                break;
            case Item.ItemType.WeirdTea:
                //swaps ep and se
                int temp = BattleControl.Instance.GetEP(caller);
                BattleControl.Instance.SetEP(caller, BattleControl.Instance.GetSE(caller));
                BattleControl.Instance.SetSE(caller, temp);
                break;
            case Item.ItemType.ConversionShake:
                //swaps atk and def buffs
                for (int i = 0; i < targets.Count; i++)
                {
                    ConversionShake(targets[i]);
                }
                break;
            case Item.ItemType.FlashBud:
                yield return MainManager.Instance.FadeToWhite();
                //wonky
                BattleControl.Instance.SetEP(caller, MainManager.Instance.playerData.ep);
                BattleControl.Instance.SetSE(caller, MainManager.Instance.playerData.se);
                for (int i = 0; i < targets.Count; i++)
                {
                    PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(targets[i].entityID);
                    if (pde != null)
                    {
                        /*
                        if (targets[i].hp == 0 && pde.hp > 0)
                        {
                            BattleControl.Instance.CreateReviveParticles(targets[i], 1);
                        }
                        */
                        targets[i].hp = pde.hp;
                    }
                }
                yield return new WaitForSeconds(0.25f);
                yield return MainManager.Instance.UnfadeToWhite();
                break;
            case Item.ItemType.RubyFruit:
                for (int i = 0; i < targets.Count; i++)
                {
                    DiluteParticles(targets[i]);
                    if (boost < 1)
                    {
                        BoostEffectDurations(targets[i], 1);
                    }
                    else
                    {
                        BoostEffectDurations(targets[i], (int)(1 * boost));
                    }
                }
                break;
            case Item.ItemType.BoosterShake:
                //boost effect potency
                for (int i = 0; i < targets.Count; i++)
                {
                    BoostParticles(targets[i]);
                    for (int j = 0; j < targets[i].effects.Count; j++)
                    {
                        if (Effect.GetEffectClass(targets[i].effects[j].effect) != Effect.EffectClass.BuffDebuff)
                        {
                            continue;
                        }

                        if (targets[i].effects[j].duration == Effect.INFINITE_DURATION || targets[i].effects[j].potency >= Effect.INFINITE_DURATION - (int)(boost * 1))
                        {
                            continue;
                        }

                        //do the boost
                        if (boost < 1)
                        {
                            targets[i].effects[j].potency += 1;
                            targets[i].effects[j].duration -= 1;
                        }
                        else
                        {
                            targets[i].effects[j].potency += (sbyte)(boost * 1);
                            targets[i].effects[j].duration -= (sbyte)(boost * 1);
                        }
                    }

                    targets[i].ValidateEffects();
                }
                break;
            case Item.ItemType.DiluteShake:
                //boost effect duration
                for (int i = 0; i < targets.Count; i++)
                {
                    DiluteParticles(targets[i]);
                    for (int j = 0; j < targets[i].effects.Count; j++)
                    {
                        if (Effect.GetEffectClass(targets[i].effects[j].effect) != Effect.EffectClass.BuffDebuff)
                        {
                            continue;
                        }

                        if (targets[i].effects[j].duration >= Effect.INFINITE_DURATION - (int)(boost * 1) || targets[i].effects[j].potency == Effect.INFINITE_DURATION)
                        {
                            continue;
                        }

                        //do the boost
                        if (boost < 1)
                        {
                            targets[i].effects[j].duration += 1;
                            targets[i].effects[j].potency -= 1;
                        }
                        else
                        {
                            targets[i].effects[j].duration += (sbyte)(boost * 1);
                            targets[i].effects[j].potency -= (sbyte)(boost * 1);
                        }
                    }

                    targets[i].ValidateEffects();
                }
                break;
            //case Item.ItemType.ThickShake:
            //inflict special effect
            //break;
            case Item.ItemType.InversionStew:
                //invert effect potency
                //this is not very straightforward so split it off to another function
                for (int i = 0; i < targets.Count; i++)
                {
                    InvertParticles(targets[i]);
                    InvertEffects(targets[i]);
                }
                break;
            case Item.ItemType.ThickShake:
                //this one just gives you a bunch of effects
                for (int i = 0; i < targets.Count; i++)
                {
                    ThickParticles(targets[i]);
                }
                break;
            case Item.ItemType.RubyJuice:
                for (int i = 0; i < targets.Count; i++)
                {
                    DiluteParticles(targets[i]);
                    if (2 * boost < 1)
                    {
                        BoostEffectDurations(targets[i], 1);
                    } else
                    {
                        BoostEffectDurations(targets[i], (int)(2 * boost));
                    }
                }
                break;
        }


        if (delay)
        {
            yield return new WaitForSeconds(0.5f);
        }
        for (int i = 0; i < statusList.Length; i++)
        {
            if (statusList[i].potency == 0 || statusList[i].duration == 0)
            {
                continue;
            }

            //Debug.Log(statusList[i]);
            delay = true;
            for (int j = 0; j < targets.Count; j++)
            {
                if (j != 0)
                {
                    if (statusList[i].effect == Effect.EffectType.EnergyRegen)
                    {
                        continue;
                    }
                    if (statusList[i].effect == Effect.EffectType.EnergyLoss)
                    {
                        continue;
                    }
                    if (statusList[i].effect == Effect.EffectType.SoulRegen)
                    {
                        continue;
                    }
                    if (statusList[i].effect == Effect.EffectType.SoulLoss)
                    {
                        continue;
                    }
                }

                caller.InflictEffect(targets[j], statusList[i].BoostCopy(targets[j].GetItemReceiveBonus()));
            }

            if (i < statusList.Length - 1 && healOverTimeEffects.Length != 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        for (int i = 0; i < healOverTimeEffects.Length; i++)
        {
            if (healOverTimeEffects[i].potency == 0 || healOverTimeEffects[i].duration == 0)
            {
                continue;
            }

            delay = true;
            for (int j = 0; j < targets.Count; j++)
            {
                //Debug.Log(healOverTimeEffects[i]);
                if (j != 0)
                {
                    if (healOverTimeEffects[i].effect == Effect.EffectType.EnergyRegen)
                    {
                        continue;
                    }
                    if (healOverTimeEffects[i].effect == Effect.EffectType.EnergyLoss)
                    {
                        continue;
                    }
                    if (healOverTimeEffects[i].effect == Effect.EffectType.SoulRegen)
                    {
                        continue;
                    }
                    if (healOverTimeEffects[i].effect == Effect.EffectType.SoulLoss)
                    {
                        continue;
                    }
                }
                caller.InflictEffect(targets[j], healOverTimeEffects[i].BoostCopy(targets[j].GetItemReceiveBonus()));
            }

            if (i < healOverTimeEffects.Length - 1)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }        

        //target weak stomach
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] is PlayerEntity ptarget)
            {
                if (ptarget.BadgeEquipped(Badge.BadgeType.WeakStomach))
                {
                    //Poison
                    caller.InflictEffect(targets[i], new Effect(Effect.EffectType.Poison, 1, (sbyte)(2 * ptarget.BadgeEquippedCount(Badge.BadgeType.WeakStomach))));

                    if (healOverTimeEffects.Length == 0)
                    {
                        if (hpheal > 0)
                        {
                            sbyte heal = (sbyte)(hpheal / 10);
                            if (heal < 1)
                            {
                                heal = 1;
                            }
                            caller.InflictEffect(targets[i], new Effect(Effect.EffectType.HealthRegen, heal, (sbyte)(2 * ptarget.BadgeEquippedCount(Badge.BadgeType.WeakStomach))));
                        }
                        if (epheal > 0)
                        {
                            sbyte heal = (sbyte)(epheal / 10);
                            if (heal < 1)
                            {
                                heal = 1;
                            }
                            caller.InflictEffect(targets[i], new Effect(Effect.EffectType.EnergyRegen, heal, (sbyte)(2 * ptarget.BadgeEquippedCount(Badge.BadgeType.WeakStomach))));
                        }
                        if (seheal > 0)
                        {
                            sbyte heal = (sbyte)(seheal / 10);
                            if (heal < 1)
                            {
                                heal = 1;
                            }
                            caller.InflictEffect(targets[i], new Effect(Effect.EffectType.SoulRegen, heal, (sbyte)(2 * ptarget.BadgeEquippedCount(Badge.BadgeType.WeakStomach))));
                        }
                    }
                }
            }
        }

        if ((Item.GetProperty(ide, Item.ItemProperty.Quick) != null || GetModifier() == ItemModifier.Quick) && caller is PlayerEntity pcaller)
        {
            if (caller.CanMove() && pcaller.QuickSupply != 2)
            {
                caller.actionCounter--;
                caller.InflictEffectForce(caller, new Effect(Effect.EffectType.BonusTurns, 1, Effect.INFINITE_DURATION));
                pcaller.QuickSupply = 2;
            }
        }
    }

    public void BoostParticles(BattleEntity be)
    {
        int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position;

        GameObject eo = null;
        EffectScript_GenericColorRateOverTime es_b = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Booster"), gameObject.transform);
        eo.transform.position = position;
        es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(new Color(0.0f, 0.0f, 0.0f, 0.0f), newScale, power);
    }
    public void DiluteParticles(BattleEntity be)
    {
        int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position;

        GameObject eo = null;
        EffectScript_GenericColorRateOverTime es_b = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Dilute"), gameObject.transform);
        eo.transform.position = position;
        es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(new Color(0.0f, 0.0f, 0.0f, 0.0f), newScale, power);
    }

    public void BoostEffectDurations(BattleEntity target, int bonus)
    {
        for (int i = 0; i < target.effects.Count; i++)
        {
            //don't boost to infinity
            if (target.effects[i].duration < Effect.MAX_NORMAL_DURATION)
            {
                if (target.effects[i].duration > Effect.MAX_NORMAL_DURATION - bonus)
                {
                    target.effects[i].duration = Effect.MAX_NORMAL_DURATION;
                } else
                {
                    target.effects[i].duration = (sbyte)(target.effects[i].duration + bonus);
                }
            }
        }
    }
    public void ThickParticles(BattleEntity be)
    {
        int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position;

        GameObject eo = null;
        EffectScript_GenericColorRateOverTime es_b = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Thick"), gameObject.transform);
        eo.transform.position = position;
        es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(new Color(0.0f, 0.0f, 0.0f, 0.0f), newScale, power);
    }
    public void InvertParticles(BattleEntity be)
    {
        int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position;

        GameObject eo = null;
        EffectScript_GenericColorRateOverTime es_b = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Invert"), gameObject.transform);
        eo.transform.position = position;
        es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(new Color(0.0f, 0.0f, 0.0f, 0.0f), newScale, power);
    }
    public void InvertEffects(BattleEntity target)
    {
        //store them as pairs (a-b, c-d...)
        //some effects are off limits due to being problematic
        //(infinite attack reduction -> boost is a bit too good)
        //there are also a few "sproadic" effects
        Effect.EffectType[] conflictingStatuses =
        {
            //Effect.EffectType.AttackBoost,
            //Effect.EffectType.AttackReduction,

            //Effect.EffectType.DefenseBoost,
            //Effect.EffectType.DefenseReduction,

            //Effect.EffectType.EnduranceBoost,
            //Effect.EffectType.EnduranceReduction,

            //Effect.EffectType.AgilityBoost,
            //Effect.EffectType.AgilityReduction,

            //Effect.EffectType.MaxHPBoost,
            //Effect.EffectType.MaxHPReduction,

            //Effect.EffectType.MaxEPBoost,
            //Effect.EffectType.MaxEPReduction,

            //Effect.EffectType.MaxSEBoost,
            //Effect.EffectType.MaxSEReduction,

            //advanced ailments
            Effect.EffectType.Soulbleed,
            Effect.EffectType.Ethereal,

            Effect.EffectType.Sunflame,
            Effect.EffectType.Illuminate,

            Effect.EffectType.Brittle,
            Effect.EffectType.MistWall,

            Effect.EffectType.Inverted,
            Effect.EffectType.AstralWall,

            Effect.EffectType.Dread,
            Effect.EffectType.CounterFlare,

            Effect.EffectType.ArcDischarge,
            Effect.EffectType.Supercharge,

            Effect.EffectType.TimeStop,
            Effect.EffectType.QuantumShield,

            Effect.EffectType.Exhausted,
            Effect.EffectType.Soften,

            Effect.EffectType.AttackUp,
            Effect.EffectType.AttackDown,

            Effect.EffectType.DefenseUp,
            Effect.EffectType.DefenseDown,

            Effect.EffectType.EnduranceUp,
            Effect.EffectType.EnduranceDown,

            Effect.EffectType.AgilityUp,
            Effect.EffectType.AgilityDown,

            Effect.EffectType.FlowUp,
            Effect.EffectType.FlowDown,

            Effect.EffectType.HealthRegen,
            Effect.EffectType.HealthLoss,

            Effect.EffectType.EnergyRegen,
            Effect.EffectType.EnergyLoss,

            Effect.EffectType.SoulRegen,
            Effect.EffectType.SoulLoss,

            Effect.EffectType.Focus,
            Effect.EffectType.Defocus,

            Effect.EffectType.Absorb,
            Effect.EffectType.Sunder,

            Effect.EffectType.Burst,
            Effect.EffectType.Enervate,

            Effect.EffectType.Haste,
            Effect.EffectType.Hamper,

            Effect.EffectType.Awaken,
            Effect.EffectType.Disorient,

            //Effect.EffectType.BonusTurns,
            //Effect.EffectType.Cooldown,

            Effect.EffectType.Freeze,
            Effect.EffectType.Poison,

            Effect.EffectType.Dizzy,
            Effect.EffectType.Paralyze,

            Effect.EffectType.Sleep,
            Effect.EffectType.Berserk,

            Effect.EffectType.ParryAura,
            Effect.EffectType.DrainSprout,

            Effect.EffectType.BolsterAura,
            Effect.EffectType.BoltSprout,
        };

        //better way? make a list of indices of the statuses above for future reference to avoid loop retreading
        int[] conflictingStatusIndices = new int[conflictingStatuses.Length];
        for (int i = 0; i < conflictingStatusIndices.Length; i++)
        {
            conflictingStatusIndices[i] = -1;
        }
        for (int i = 0; i < target.effects.Count; i++)
        {
            for (int j = 0; j < conflictingStatuses.Length; j++)
            {
                if (target.effects[i].effect == conflictingStatuses[j])
                {
                    conflictingStatusIndices[j] = i;
                }
            }
        }

        //Now do stuff

        for (int i = 0; i < conflictingStatusIndices.Length; i++)
        {
            if (conflictingStatusIndices[i] != -1)
            {
                //this is funky looking but it works
                //mult of 2 + (i + 1) % 2
                int otherIndex = (i - (i % 2)) + ((i + 1) % 2);
                target.effects[conflictingStatusIndices[i]].effect = conflictingStatuses[otherIndex];

                //audit some specific cases
                if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.MistWall)
                {
                    target.effects[conflictingStatusIndices[i]].potency = 1;
                }
                if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.AstralWall)
                {
                    sbyte min = (sbyte)Mathf.CeilToInt(target.maxHP / 4f);
                    target.effects[conflictingStatusIndices[i]].potency = min;
                }
                if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.Inverted)
                {
                    target.effects[conflictingStatusIndices[i]].potency = 1;
                }
                if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.TimeStop)
                {
                    target.effects[conflictingStatusIndices[i]].potency = 1;
                }
            }
        }
    }

    public void ConversionShake(BattleEntity target)
    {
        //same as inverteffects but with a different list
        //store them as pairs (a-b, c-d...)
        //infinite effects seem fine to include here since you can only get the buffs if you already have the buffs
        //(though this does let you convert the perma defense items into perma attack items and vice versa)
        Effect.EffectType[] conflictingStatuses =
        {
            Effect.EffectType.AttackBoost,
            Effect.EffectType.DefenseBoost,

            Effect.EffectType.AttackReduction,
            Effect.EffectType.DefenseReduction,

            Effect.EffectType.AttackUp,
            Effect.EffectType.DefenseUp,

            Effect.EffectType.AttackDown,
            Effect.EffectType.DefenseDown,

            Effect.EffectType.Focus,
            Effect.EffectType.Absorb,

            Effect.EffectType.Defocus,
            Effect.EffectType.Sunder,

            //shh, don't tell anyone
            Effect.EffectType.Ethereal,
            Effect.EffectType.CounterFlare,

            Effect.EffectType.Illuminate,
            Effect.EffectType.Supercharge,

            Effect.EffectType.MistWall,
            Effect.EffectType.QuantumShield,

            Effect.EffectType.AstralWall,
            Effect.EffectType.Soften,   //note: don't make soften potency work or else this gets really broken
        };

        //4d loop seems bad for performance
        //(or at least it is potentially a ton of operations considering how long the list of possible conflicting statuses is)
        /*
        for (int i = 0; i < statuses.Count; i++)
        {
            for (int j = 0; j < conflictingStatuses.Length; j++)
            {
                //...
            }
        }
        */

        //better way? make a list of indices of the statuses above for future reference to avoid loop retreading
        int[] conflictingStatusIndices = new int[conflictingStatuses.Length];
        for (int i = 0; i < conflictingStatusIndices.Length; i++)
        {
            conflictingStatusIndices[i] = -1;
        }
        for (int i = 0; i < target.effects.Count; i++)
        {
            for (int j = 0; j < conflictingStatuses.Length; j++)
            {
                if (target.effects[i].effect == conflictingStatuses[j])
                {
                    conflictingStatusIndices[j] = i;
                }
            }
        }

        //Now do stuff

        for (int i = 0; i < conflictingStatusIndices.Length; i++)
        {
            if (conflictingStatusIndices[i] != -1)
            {
                //this is funky looking but it works
                //mult of 2 + (i + 1) % 2
                int otherIndex = (i - (i % 2)) + ((i + 1) % 2);
                target.effects[conflictingStatusIndices[i]].effect = conflictingStatuses[otherIndex];
            }

            //audit some specific cases
            if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.MistWall)
            {
                target.effects[conflictingStatusIndices[i]].potency = 1;
            }
            if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.Supercharge)
            {
                target.effects[conflictingStatusIndices[i]].potency = 1;
            }
            if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.AstralWall)
            {
                sbyte min = (sbyte)Mathf.CeilToInt(target.maxHP / 4f);
                target.effects[conflictingStatusIndices[i]].potency = min;
            }
            if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.TimeStop)
            {
                target.effects[conflictingStatusIndices[i]].potency = 1;
            }
            if (target.effects[conflictingStatusIndices[i]].effect == Effect.EffectType.Soften)
            {
                target.effects[conflictingStatusIndices[i]].potency = 1;
            }
        }
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.ItemSight))
            {
                return "";
            }
        }

        //Only certain items need this
        string outstring = "";
        ItemDataEntry ide = Item.GetItemDataEntry(GetItem());

        bool shouldHighlight = false;

        if (ide.weird)
        {
            shouldHighlight = true;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.Stack) != null)
        {
            shouldHighlight = true;
        }

        if (GetModifier() != ItemModifier.None)
        {
            shouldHighlight = true;
        }

        //item sight displays this always
        shouldHighlight = true;

        float boost = 1;
        if (caller.HasEffect(Effect.EffectType.ItemBoost))
        {
            boost = Item.GetItemBoost(caller.GetEffectEntry(Effect.EffectType.ItemBoost).potency);
        }
        if (GetModifier() == ItemModifier.Glistening)
        {
            boost *= 1.5f;
        }
        if (GetModifier() == ItemModifier.Focus)
        {
            boost *= 2f;
        }
        if (GetModifier() == ItemModifier.Echo)
        {
            boost *= 0.80001f;
        }
        if (GetModifier() == ItemModifier.Echoed)
        {
            boost *= 0.40001f;
        }
        int powerCount = 1;
        if (Item.GetProperty(ide, Item.ItemProperty.Stack) != null)
        {
            //powerCount = BattleControl.Instance.playerData.CountAllOfType(GetItemType());
            //Note: RemoveItemUsed is called immediately before executing this
            //  (which means there is no way to count stack items here ): )
            powerCount = itemCount;
            if (powerCount < 1)
            {
                powerCount = 1;
            }
        }
        float multiplier = Item.GetItemBoost(level - 1);
        if (Item.GetProperty(ide, Item.ItemProperty.Unity) != null)
        {
            powerCount *= itemCount;
            if (powerCount < 1)
            {
                powerCount = 1;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeBoostTen) != null)
        {
            if (BattleControl.Instance.turnCount > 10)
            {
                boost *= 10;
            }
            else
            {
                boost *= BattleControl.Instance.turnCount;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeBoostTwenty) != null)
        {
            if (BattleControl.Instance.turnCount > 20)
            {
                boost *= 20;
            }
            else
            {
                boost *= BattleControl.Instance.turnCount;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeWeaken) != null)
        {
            if (BattleControl.Instance.turnCount >= 10)
            {
                boost *= 0.100001f;
            }
            else
            {
                boost *= (11 - BattleControl.Instance.turnCount) * 0.100001f;
            }
        }
        boost *= multiplier * powerCount;
        if (Item.GetProperty(ide, Item.ItemProperty.Disunity) != null)
        {
            boost /= itemCount;
        }
        boost *= caller.GetItemUseBonus();    //(effect item boost is already calculated)

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleOnTurn1) != null && BattleControl.Instance.turnCount == 1)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtLowHP) != null && caller.hp <= PlayerData.PlayerDataEntry.GetDangerHP(caller.entityID))
        {
            boost *= 2;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtMaxHP) != null && caller.hp == caller.maxHP)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtLowItems) != null && BattleControl.Instance.GetItemInventory(caller).Count <= 5)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.BoostEnemies) != null)
        {
            int ecount = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveEnemy)).Count;
            if (ecount > 4)
            {
                ecount = 4;
            }
            if (ecount <= 1)
            {
                ecount = 1;
            }
            boost *= ecount;
        }



        if (boost != 1)
        {
            //In this case the description will not reflect the actual effect
            shouldHighlight = true;
        }

        if (!shouldHighlight)
        {
            return "";
        }

        TargetArea ta = GetBaseTarget();
        //Use this to build the list of targets
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, ta);
        if (!GetMultiTarget())
        {
            targets = new List<BattleEntity>
            {
                target
            };
        }
        else
        {
            //note: by convention, put the caller first in the target list
            if (targets.IndexOf(caller) > 0)    //(-1 and 0 cause this to be skipped)
            {
                targets.Remove(caller);
                targets.Insert(0, caller);
            }
        }


        int hpheal = (int)(ide.hp * boost);
        int epheal = (int)(ide.ep * boost);
        int seheal = (int)(ide.se * boost);
        int staminaheal = (int)(ide.stamina * boost);
        Effect[] statusList = (Effect[])(ide.effects.Clone());

        //Apply heal over time
        Effect[] healOverTimeEffects = new Effect[0];
        Effect[] tempHealOverTime = new Effect[3];

        //bool isHealOverTime = false;

        sbyte healOverTimeMult = 0;
        if (Item.GetProperty(ide, Item.ItemProperty.HealOverTime) != null)
        {
            healOverTimeMult = 3;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.SlowHealOverTime) != null)
        {
            healOverTimeMult = 5;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.SixHealOverTime) != null)
        {
            healOverTimeMult = 6;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.EightHealOverTime) != null)
        {
            healOverTimeMult = 8;
        }

        if (healOverTimeMult != 0)
        {
            //un-boost the heals (to not boost twice)
            hpheal = ide.hp;
            epheal = ide.ep;
            seheal = ide.se;

            //isHealOverTime = true;
            int count = 0;
            if (hpheal > 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.HealthRegen, (sbyte)(hpheal / healOverTimeMult), healOverTimeMult);
                count++;
            }
            if (hpheal < 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.HealthLoss, (sbyte)(-hpheal / healOverTimeMult), healOverTimeMult);
                count++;
            }

            if (epheal > 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.EnergyRegen, (sbyte)(epheal / healOverTimeMult), healOverTimeMult);
                count++;
            }
            if (epheal < 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.EnergyLoss, (sbyte)(-epheal / healOverTimeMult), healOverTimeMult);
                count++;
            }

            if (seheal > 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.SoulRegen, (sbyte)(seheal / healOverTimeMult), healOverTimeMult);
                count++;
            }
            if (seheal < 0)
            {
                tempHealOverTime[count] = new Effect(Effect.EffectType.SoulLoss, (sbyte)(-seheal / healOverTimeMult), healOverTimeMult);
                count++;
            }

            healOverTimeEffects = new Effect[count];

            for (int i = 0; i < count; i++)
            {
                healOverTimeEffects[i] = tempHealOverTime[i];
                //Debug.Log(healOverTimeEffects[i]);
            }

            //don't use the normal heal code later
            hpheal = 0;
            epheal = 0;
            seheal = 0;
        }

        //Boost the effects (boost of 1 is nothing)
        if (boost != 1)
        {
            for (int i = 0; i < statusList.Length; i++)
            {
                //boost by duration
                //if duration == Effect.INFINITE_DURATION, boost by potency
                //blacklist some effect types to prevent some overpowered things
                //(the permanent stat increases, item boost itself)
                /*
                if (Effect.GetEffectClass(statusList[i].effect) == Effect.EffectClass.Static)
                {
                    continue;
                }
                */

                //yeah no
                if (statusList[i].effect == Effect.EffectType.ItemBoost || statusList[i].effect == Effect.EffectType.Miracle)
                {
                    continue;
                }

                statusList[i] = statusList[i].BoostCopy(boost);
            }

            for (int i = 0; i < healOverTimeEffects.Length; i++)
            {
                healOverTimeEffects[i] = healOverTimeEffects[i].BoostCopy(boost);
            }
        }
        //the effects are not used in the highlight, so rebuild them
        int hphealOverTime = 0;
        int ephealOverTime = 0;
        int sehealOverTime = 0;
        int hpturns = 0;
        int epturns = 0;
        int seturns = 0;

        for (int i = 0; i < healOverTimeEffects.Length; i++)
        {
            if (healOverTimeEffects[i].effect == Effect.EffectType.HealthRegen)
            {
                hphealOverTime = healOverTimeEffects[i].potency;
                hpturns = healOverTimeEffects[i].duration;
            }
            if (healOverTimeEffects[i].effect == Effect.EffectType.HealthLoss)
            {
                hphealOverTime = -healOverTimeEffects[i].potency;
                hpturns = healOverTimeEffects[i].duration;
            }
            if (healOverTimeEffects[i].effect == Effect.EffectType.EnergyRegen)
            {
                ephealOverTime = healOverTimeEffects[i].potency;
                epturns = healOverTimeEffects[i].duration;
            }
            if (healOverTimeEffects[i].effect == Effect.EffectType.EnergyLoss)
            {
                ephealOverTime = -healOverTimeEffects[i].potency;
                epturns = healOverTimeEffects[i].duration;
            }
            if (healOverTimeEffects[i].effect == Effect.EffectType.SoulRegen)
            {
                sehealOverTime = healOverTimeEffects[i].potency;
                seturns = healOverTimeEffects[i].duration;
            }
            if (healOverTimeEffects[i].effect == Effect.EffectType.SoulLoss)
            {
                sehealOverTime = -healOverTimeEffects[i].potency;
                seturns = healOverTimeEffects[i].duration;
            }
        }

        //overheal isn't tracked (because annoying)

        //these calculate healing with variables or otherwise do weird things
        //note that this list should only include things that make sense to boost by multiplying
        //(so stat swap = not here)
        //(strange salad is not here)
        switch (GetItemType())
        {
            case Item.ItemType.WarpedCookie:
                //heal for other stat
                hpheal = (int)(BattleControl.Instance.GetEP(caller) * boost);
                for (int i = 0; i < targets.Count; i++)
                {
                    epheal += targets[0].hp;
                }
                epheal /= targets.Count;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.GoldenTea:
                hpheal = -caller.hp / 2;
                hpheal = (int)(hpheal * boost);
                break;
            case Item.ItemType.BrightSoup:
                epheal = -BattleControl.Instance.GetEP(caller) / 2;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.PineappleJuice:
                epheal = -BattleControl.Instance.GetEP(caller) / 2;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.MushroomSoup:
                hpheal = -caller.hp / 2;
                epheal = -BattleControl.Instance.GetEP(caller) / 2;
                hpheal = (int)(hpheal * boost);
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.CursedStew:
                hpheal = (int)(-caller.hp * boost);
                epheal = (int)(-BattleControl.Instance.GetEP(caller) * boost);
                break;
            case Item.ItemType.AetherSalad:
                epheal = (int)(-BattleControl.Instance.GetEP(caller) * boost);
                break;
            case Item.ItemType.StellarCandy:
                hpheal = (int)(-caller.hp * boost);
                break;
            case Item.ItemType.AetherPudding:
                hpheal = (int)(-21 * boost);
                epheal = (int)(-21 * boost);
                break;
        }

        hpheal = (int)(hpheal * target.GetItemReceiveBonus());
        epheal = (int)(epheal * target.GetItemReceiveBonus());
        seheal = (int)(seheal * target.GetItemReceiveBonus());
        staminaheal = (int)(staminaheal * target.GetItemReceiveBonus());


        //Apply weird effects
        switch (GetItemType())
        {
            case Item.ItemType.WarpedLeaf:
                //Swap HP and EP

                //new ep = average of target hp
                //new hp = ep
                int tempep = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    tempep += targets[i].hp;
                }
                tempep /= targets.Count;

                int wl_hp = BattleControl.Instance.GetEP(caller);
                if (wl_hp > target.maxHP)
                {
                    wl_hp = target.maxHP;
                }
                if (wl_hp < 0)
                {
                    wl_hp = 0;
                }

                hpheal = wl_hp - target.hp;

                if (tempep > BattleControl.Instance.GetMaxEP(caller))
                {
                    tempep = BattleControl.Instance.GetMaxEP(caller);
                }
                if (tempep < 0)
                {
                    tempep = 0;
                }

                epheal = tempep - BattleControl.Instance.GetEP(caller);
                break;
            case Item.ItemType.WarpedTea:
                //halves missing hp, ep
                int wa_hp = target.maxHP / 2 + target.hp / 2;
                int wa_ep = BattleControl.Instance.GetMaxEP(caller) / 2 + BattleControl.Instance.GetEP(caller) / 2;

                hpheal = wa_hp - target.hp;
                epheal = wa_ep - BattleControl.Instance.GetEP(caller);
                break;
            case Item.ItemType.StrangeBud:
                //inverts missing hp, ep
                int sb_hp = target.maxHP - target.hp;
                int sb_ep = BattleControl.Instance.GetMaxEP(caller) - BattleControl.Instance.GetEP(caller);

                hpheal = sb_hp - target.hp;
                epheal = sb_ep - BattleControl.Instance.GetEP(caller);
                break;
            case Item.ItemType.StrangeSalad:
                //inverts missing hp, ep
                int ss_hp = target.maxHP / 2;
                int ss_ep = BattleControl.Instance.GetMaxEP(caller) / 2;

                hpheal = ss_hp - target.hp;
                epheal = ss_ep - BattleControl.Instance.GetEP(caller);
                break;
            case Item.ItemType.WeirdTea:
                //swaps ep and se
                int wt_ep = BattleControl.Instance.GetEP(caller);
                int wt_se = BattleControl.Instance.GetSE(caller);
                int newep = wt_se;
                int newse = wt_ep;

                if (newep > BattleControl.Instance.GetMaxEP(caller))
                {
                    newep = BattleControl.Instance.GetMaxEP(caller);
                }
                if (newep < 0)
                {
                    newep = 0;
                }

                if (newse > BattleControl.Instance.GetMaxSE(caller))
                {
                    newse = BattleControl.Instance.GetMaxSE(caller);
                }
                if (newse < 0)
                {
                    newse = 0;
                }

                epheal = newep - wt_ep;
                seheal = newse - wt_se;
                break;
            case Item.ItemType.FlashBud:
                //wonky
                PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(target.entityID);
                if (pde != null)
                {
                    hpheal = pde.hp - target.hp;
                    epheal = MainManager.Instance.playerData.ep - BattleControl.Instance.GetEP(target);
                    seheal = MainManager.Instance.playerData.se - BattleControl.Instance.GetSE(target);
                }
                break;
        }


        if (hpheal != 0)
        {
            //overheal += target.HealHealthTrackOverhealPay(hpheal);
            outstring += "<highlighthpcolor>" + hpheal + "</highlighthpcolor>";
        }
        if (hphealOverTime != 0)
        {
            if (outstring.Length > 0)
            {
                outstring += ", ";
            }
            outstring += "<highlighthpcolor>" + hphealOverTime + "/" + hpturns + "t</highlighthpcolor>";
        }

        if (epheal != 0)
        {
            //overheal += target.HealEnergyTrackOverhealPay(epheal);
            if (outstring.Length > 0)
            {
                outstring += ", ";
            }
            outstring += "<highlightepcolor>" + epheal + "</highlightepcolor>";
        }
        if (ephealOverTime != 0)
        {
            if (outstring.Length > 0)
            {
                outstring += ", ";
            }
            outstring += "<highlightepcolor>" + ephealOverTime + "/" + epturns + "t</highlighteocolor>";
        }

        if (seheal != 0)
        {
            //overheal += target.HealSoulEnergyTrackOverhealPay(seheal);
            if (outstring.Length > 0)
            {
                outstring += ", ";
            }
            outstring += "<highlightsecolor>" + seheal + "</highlightsecolor>";
        }
        if (sehealOverTime != 0)
        {
            if (outstring.Length > 0)
            {
                outstring += ", ";
            }
            outstring += "<highlightsecolor>" + sehealOverTime + "/" + seturns + "t</highlightsecolor>";
        }

        if (staminaheal != 0)
        {
            //overheal += target.HealStaminaTrackOverhealPay(staminaheal);
            if (outstring.Length > 0)
            {
                outstring += ", ";
            }
            outstring += "<highlightstcolor>" + staminaheal + "</highlightstcolor>";
        }

        return outstring;
    }
}

public class Item_GenericThrowable : ItemMove
{
    //public override float GetBasePower() => 4.0f;
    //public override Item.ItemType GetItemType() => Item.ItemType.DebugBomb;
    public bool GetMultiTarget()
    {
        if (forceSingleTarget || GetModifier() == ItemModifier.Focus)
        {
            return false;
        }

        if (forceMultiTarget || GetModifier() == ItemModifier.Spread)
        {
            return true;
        }

        return Item.GetProperty(GetItem(), Item.ItemProperty.TargetAll) != null;
    }

    public Item_GenericThrowable()
    {
    }



    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        TargetArea ta = GetBaseTarget();
        //Use this to build the list of targets
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, ta);
        if (!GetMultiTarget())
        {
            targets = new List<BattleEntity>
            {
                caller.curTarget
            };
        }
        //note: throwables only target enemies so the special case of putting the caller first can never happen

        caller.SetAnimation("itemuse");
        yield return StartCoroutine(DefaultStartAnim(caller));
        //yield return StartCoroutine(caller.SmoothScale(0.5f, new Vector3(2, 2, 2)));

        caller.SetAnimation("itemthrow");
        yield return new WaitForSeconds(0.3f);
        caller.SetIdleAnimation();

        yield return StartCoroutine(ExecuteEffect(caller, targets, Item.GetItemBoost(level - 1)));

        //yield return StartCoroutine(caller.SmoothScale(0.5f, new Vector3(1, 1, 1)));
        yield return StartCoroutine(ProducerAnim(caller));

        if (caller is PlayerEntity pcaller)
        {
            yield return new WaitForSeconds(0.5f);
            ItemDataEntry ide = Item.GetItemDataEntry(GetItem());
            if (pcaller.BadgeEquipped(Badge.BadgeType.ItemRebate))
            {
                pcaller.HealCoins((int)(ide.sellPrice * 0.75f * pcaller.BadgeEquippedCount(Badge.BadgeType.ItemRebate)));
            }
        }
    }

    public IEnumerator ExecuteEffect(BattleEntity caller, List<BattleEntity> targets, float multiplier)
    {
        ItemDataEntry ide = Item.GetItemDataEntry(GetItem());

        float boost = 1;
        if (caller.HasEffect(Effect.EffectType.ItemBoost))
        {
            boost = Item.GetItemBoost(caller.GetEffectEntry(Effect.EffectType.ItemBoost).potency);
        }
        if (GetModifier() == ItemModifier.Glistening)
        {
            boost *= 1.5f;
        }
        if (GetModifier() == ItemModifier.Focus)
        {
            boost *= 2f;
        }
        if (GetModifier() == ItemModifier.Echo)
        {
            boost *= 0.80001f;
        }
        if (GetModifier() == ItemModifier.Echoed)
        {
            boost *= 0.40001f;
        }
        int powerCount = 1;
        if (Item.GetProperty(ide, Item.ItemProperty.Stack) != null)
        {
            powerCount = BattleControl.Instance.CountAllItemsOfType(caller, GetItemType());
            if (powerCount < 1)
            {
                powerCount = 1;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.Unity) != null)
        {
            powerCount *= itemCount;
            if (powerCount < 1)
            {
                powerCount = 1;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeBoostTen) != null)
        {
            if (BattleControl.Instance.turnCount > 10)
            {
                boost *= 10;
            }
            else
            {
                boost *= BattleControl.Instance.turnCount;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeBoostTwenty) != null)
        {
            if (BattleControl.Instance.turnCount > 20)
            {
                boost *= 20;
            }
            else
            {
                boost *= BattleControl.Instance.turnCount;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeWeaken) != null)
        {
            if (BattleControl.Instance.turnCount >= 10)
            {
                boost *= 0.100001f;
            }
            else
            {
                boost *= (11 - BattleControl.Instance.turnCount) * 0.100001f;
            }
        }
        boost *= multiplier * powerCount;
        if (Item.GetProperty(ide, Item.ItemProperty.Disunity) != null)
        {
            boost /= itemCount;
        }
        boost *= caller.GetItemUseBonus();    //(effect item boost is already calculated)

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleOnTurn1) != null && BattleControl.Instance.turnCount == 1)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtLowHP) != null && caller.hp <= PlayerData.PlayerDataEntry.GetDangerHP(caller.entityID))
        {
            boost *= 2;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtMaxHP) != null && caller.hp == caller.maxHP)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtLowItems) != null && BattleControl.Instance.GetItemInventory(caller).Count <= 5)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.BoostEnemies) != null)
        {
            int ecount = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveEnemy)).Count;
            if (ecount > 4)
            {
                ecount = 4;
            }
            if (ecount <= 1)
            {
                ecount = 1;
            }
            boost *= ecount;
        }



        Effect[] statusList = (Effect[])(ide.effects.Clone());

        //Boost the effects (boost of 1 is nothing)
        if (boost != 1)
        {
            for (int i = 0; i < statusList.Length; i++)
            {
                //boost by duration
                //if duration == Effect.INFINITE_DURATION, boost by potency
                //blacklist some effect types to prevent some overpowered things
                //(the permanent stat increases, item boost itself)
                /*
                if (Effect.GetEffectClass(statusList[i].effect) == Effect.EffectClass.Static)
                {
                    continue;
                }
                */

                //yeah no
                if (statusList[i].effect == Effect.EffectType.ItemBoost || statusList[i].effect == Effect.EffectType.Miracle)
                {
                    continue;
                }

                statusList[i] = statusList[i].BoostCopy(boost);
            }
        }

        //Void Icicle / Void Cone apply effects on caster instead of target
        bool applyOnUser = false;
        switch (GetItemType())
        {
            case Item.ItemType.VoidCone:
                applyOnUser = true;
                break;
            case Item.ItemType.VoidIcicle:
                applyOnUser = true;
                break;
        }

        if (applyOnUser)
        {
            for (int i = 0; i < statusList.Length; i++)
            {
                caller.InflictEffect(caller, statusList[i].BoostCopy(caller.GetItemReceiveBonus()));

                if (i < statusList.Length - 1)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }

            statusList = new Effect[0];
        }


        //step 1: throw anim
        //(currently nonexistent)

        //for multihits
        int hits = 1;
        switch (GetItemType())
        {
            case Item.ItemType.ThornBundle:
                hits = 3;
                break;
            case Item.ItemType.MintSpikes:
                hits = 3;
                break;
        }

        for (int k = 0; k < hits; k++)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                //step 2: bonk the enemy with the damage
                ulong properties = (ulong)ide.damageProperties;
                properties |= (ulong)BattleHelper.DamageProperties.Static;
                properties |= (ulong)BattleHelper.DamageProperties.HitsWhileDizzy;
                properties |= (ulong)BattleHelper.DamageProperties.Item;
                //

                if (k != hits - 1)
                {
                    properties |= (ulong)BattleHelper.DamageProperties.Combo;
                }

                if (caller.GetAttackHit(targets[i], ide.damageType, properties))
                {
                    caller.DealDamage(targets[i], (int)(ide.baseDamage * boost), ide.damageType, properties, BattleHelper.ContactLevel.Infinite);

                    //step 3: apply effects if needed
                    if (k == hits - 1)
                    {
                        for (int j = 0; j < statusList.Length; j++)
                        {
                            caller.InflictEffect(targets[i], statusList[j]);
                        }
                    }

                    //Plague Root is special
                    if (GetItemType() == Item.ItemType.PlagueRoot)
                    {
                        for (int j = 0; j < targets[i].effects.Count; j++)
                        {
                            if (!Effect.IsCurable(targets[i].effects[j].effect))
                            {
                                continue;
                            }

                            int bonus = (int)(1 * (boost));
                            if (bonus < 1)
                            {
                                bonus = 1;
                            }

                            //check for status problem
                            if (Effect.GetEffectClass(targets[i].effects[j].effect) == Effect.EffectClass.Ailment)
                            {
                                if (targets[i].statusMaxTurns < bonus)
                                {
                                    //Bad, need to lower the boost
                                    bonus = targets[i].statusMaxTurns;
                                }
                                if (bonus <= 0)
                                {
                                    continue;
                                }
                                //also lowers the limit too in either case (so you can't squeeze more turns than you should be able to)
                                targets[i].statusMaxTurns -= bonus;
                            }

                            //don't boost to infinity
                            if (targets[i].effects[j].duration < Effect.MAX_NORMAL_DURATION)
                            {
                                if (targets[i].effects[j].duration > Effect.MAX_NORMAL_DURATION - bonus)
                                {
                                    targets[i].effects[j].duration = Effect.MAX_NORMAL_DURATION;
                                }
                                else
                                {
                                    targets[i].effects[j].duration = (sbyte)(targets[i].effects[j].duration + bonus);
                                }
                            }
                        }
                    }
                }
            }

            if (k < hits - 1)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }

        if ((Item.GetProperty(ide, Item.ItemProperty.Quick) != null || GetModifier() == ItemModifier.Quick) && caller is PlayerEntity pcaller)
        {
            if (caller.CanMove() && pcaller.QuickSupply != 2)
            {
                caller.actionCounter--;
                caller.InflictEffectForce(caller, new Effect(Effect.EffectType.BonusTurns, 1, Effect.INFINITE_DURATION));
                pcaller.QuickSupply = 2;
            }
        }
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.ItemSight))
            {
                return "";
            }
        }

        ItemDataEntry ide = Item.GetItemDataEntry(GetItem());

        float multiplier = Item.GetItemBoost(level - 1);
        float boost = 1;
        if (caller.HasEffect(Effect.EffectType.ItemBoost))
        {
            boost = Item.GetItemBoost(caller.GetEffectEntry(Effect.EffectType.ItemBoost).potency);
        }
        if (GetModifier() == ItemModifier.Glistening)
        {
            boost *= 1.5f;
        }
        if (GetModifier() == ItemModifier.Focus)
        {
            boost *= 2f;
        }
        if (GetModifier() == ItemModifier.Echo)
        {
            boost *= 0.80001f;
        }
        if (GetModifier() == ItemModifier.Echoed)
        {
            boost *= 0.40001f;
        }
        int powerCount = 1;
        if (Item.GetProperty(ide, Item.ItemProperty.Stack) != null)
        {
            powerCount = BattleControl.Instance.CountAllItemsOfType(caller, GetItemType());
            if (powerCount < 1)
            {
                powerCount = 1;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.Unity) != null)
        {
            powerCount *= itemCount;
            if (powerCount < 1)
            {
                powerCount = 1;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeBoostTen) != null)
        {
            if (BattleControl.Instance.turnCount > 10)
            {
                boost *= 10;
            }
            else
            {
                boost *= BattleControl.Instance.turnCount;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeBoostTwenty) != null)
        {
            if (BattleControl.Instance.turnCount > 20)
            {
                boost *= 20;
            }
            else
            {
                boost *= BattleControl.Instance.turnCount;
            }
        }
        if (Item.GetProperty(ide, Item.ItemProperty.TimeWeaken) != null)
        {
            if (BattleControl.Instance.turnCount >= 10)
            {
                boost *= 0.100001f;
            }
            else
            {
                boost *= (11 - BattleControl.Instance.turnCount) * 0.100001f;
            }
        }
        boost *= multiplier * powerCount;
        if (Item.GetProperty(ide, Item.ItemProperty.Disunity) != null)
        {
            boost /= itemCount;
        }
        boost *= caller.GetItemUseBonus();    //(effect item boost is already calculated)

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleOnTurn1) != null && BattleControl.Instance.turnCount == 1)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtLowHP) != null && caller.hp <= PlayerData.PlayerDataEntry.GetDangerHP(caller.entityID))
        {
            boost *= 2;
        }
        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtMaxHP) != null && caller.hp == caller.maxHP)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.DoubleAtLowItems) != null && BattleControl.Instance.GetItemInventory(caller).Count <= 5)
        {
            boost *= 2;
        }

        if (Item.GetProperty(ide, Item.ItemProperty.BoostEnemies) != null)
        {
            int ecount = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveEnemy)).Count;
            if (ecount > 4)
            {
                ecount = 4;
            }
            if (ecount <= 1)
            {
                ecount = 1;
            }
            boost *= ecount;
        }



        Effect[] statusList = (Effect[])(ide.effects.Clone());

        //Boost the effects (boost of 1 is nothing)
        if (boost != 1)
        {
            for (int i = 0; i < statusList.Length; i++)
            {
                //boost by duration
                //if duration == Effect.INFINITE_DURATION, boost by potency
                //blacklist some effect types to prevent some overpowered things
                //(the permanent stat increases, item boost itself)
                /*
                if (Effect.GetEffectClass(statusList[i].effect) == Effect.EffectClass.Static)
                {
                    continue;
                }
                */

                //yeah no
                if (statusList[i].effect == Effect.EffectType.ItemBoost || statusList[i].effect == Effect.EffectType.Miracle)
                {
                    continue;
                }

                if (statusList[i].duration == Effect.INFINITE_DURATION)
                {
                    //Boost by potency
                    if (statusList[i].potency * boost > Effect.MAX_NORMAL_DURATION)
                    {
                        statusList[i] = new Effect(statusList[i].effect, (sbyte)(Effect.MAX_NORMAL_DURATION), statusList[i].duration);
                    }
                    else
                    {
                        statusList[i] = new Effect(statusList[i].effect, (sbyte)(statusList[i].potency * boost), statusList[i].duration);
                    }
                }
                else
                {
                    //Boost by duration
                    if (statusList[i].duration * boost > Effect.MAX_NORMAL_DURATION)
                    {
                        statusList[i] = new Effect(statusList[i].effect, statusList[i].potency, (sbyte)(Effect.MAX_NORMAL_DURATION));
                    }
                    else
                    {
                        statusList[i] = new Effect(statusList[i].effect, statusList[i].potency, (sbyte)(statusList[i].duration * boost));
                    }
                }
            }
        }

        //Void Icicle / Void Cone apply effects on caster and not target
        bool applyOnUser = false;
        switch (GetItemType())
        {
            case Item.ItemType.VoidCone:
                applyOnUser = true;
                break;
            case Item.ItemType.VoidIcicle:
                applyOnUser = true;
                break;
        }

        if (applyOnUser)
        {
            statusList = new Effect[0];
        }


        //step 1: throw anim

        //(Placeholder)


        //step 2: bonk the enemy with the damage
        ulong properties = (ulong)ide.damageProperties;
        properties |= (ulong)BattleHelper.DamageProperties.Static;
        properties |= (ulong)BattleHelper.DamageProperties.HitsWhileDizzy;
        properties |= (ulong)BattleHelper.DamageProperties.Item;
        //


        int val = caller.DealDamageCalculation(target, (int)(ide.baseDamage * boost), ide.damageType, properties);

        Effect.EffectType status = Effect.EffectType.Default;
        //step 3: apply effects if needed
        for (int j = 0; j < statusList.Length; j++)
        {
            //caller.InflictEffect(target, statusList[i]);
            if (Effect.GetEffectClass(statusList[j].effect) == Effect.EffectClass.Ailment)
            {
                status = statusList[j].effect;
            }
        }

        float statusBoost = 1;
        if (caller is PlayerEntity pcaller)
        {
            statusBoost = pcaller.CalculateStatusBoost(target);
        }


        int statusHP = -1;
        if (status != Effect.EffectType.Default)
        {
            statusHP = (int)(target.StatusWorkingHP(status) * statusBoost);
        }

        //note: factors in damage dealt
        bool doesWork = (target.hp > 0) && (target.hp - val <= statusHP);


        int hits = 1;
        switch (GetItemType())
        {
            case Item.ItemType.ThornBundle:
                hits = 3;
                break;
            case Item.ItemType.MintSpikes:
                hits = 3;
                break;
        }

        string damageString = "";

        for (int i = 0; i < hits; i++)
        {
            damageString += val;
            if (i > 0)
            {
                damageString += "?";
            }
            if (i < hits - 1)
            {
                damageString += ", ";
            }
        }

        if (statusHP == -1)
        {            
            return damageString + "";
        } else
        {
            if (doesWork)
            {
                return damageString + "<line><highlightyescolor>(" + statusHP + ")</highlightyescolor>";
            }
            else
            {
                return damageString + "<line><highlightnocolor>(" + statusHP + ")</highlightnocolor>";
            }
        }
    }
}

//theoretically I could just lump this stuff into the generic consumable script (since this is identical to the generic consumable just with a filled out EooT method)
//but ehh, it doesn't break anything
public class Item_AutoConsumable : Item_GenericConsumable
{

    public Item_AutoConsumable()
    {
    }

    /*
    */


    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        Debug.Log("Auto activate: " + item.type);
        itemCount = BattleControl.Instance.CountAllItemsOfType(caller, GetItemType());

        caller.curTarget = caller;

        //Basically reuse the consumable code

        //animations are basically the same though
        yield return StartCoroutine(DefaultStartAnim(caller));

        ItemDataEntry ide = Item.GetItemDataEntry(GetItem());

        //hacky fix
        bool removeQuick = false;
        if (GetModifier() == ItemModifier.Quick)
        {
            removeQuick = true;
            item.modifier = ItemModifier.None;
        }

        yield return StartCoroutine(ExecuteEffect(caller, ide, Item.GetItemBoost(level - 1)));

        if (removeQuick)
        {
            item.modifier = ItemModifier.Quick;
        }

        yield return StartCoroutine(ProducerAnim(caller));

        if (caller is PlayerEntity pcaller)
        {
            yield return new WaitForSeconds(0.5f);
            if (pcaller.BadgeEquipped(Badge.BadgeType.ItemRebate))
            {
                pcaller.HealCoins((int)(ide.sellPrice * 0.75f * pcaller.BadgeEquippedCount(Badge.BadgeType.ItemRebate)));
            }
        }

        BattleRemoveItemUsed(caller);
    }
}

public class Item_AutoThrowable : Item_GenericThrowable
{
    public Item_AutoThrowable()
    {
    }

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        Debug.Log("Auto activate: " + item.type);
        itemCount = BattleControl.Instance.CountAllItemsOfType(caller, GetItemType());

        //note: have to set up targetting
        switch (GetItemType())
        {
            //For consistency (and to make the Focus modifier work, these two need to correctly set their targets)
            case Item.ItemType.SlimeBomb:
                caller.curTarget = target;
                break;
            case Item.ItemType.GoldBomb:
                //backfire
                caller.curTarget = caller;
                break;

            case Item.ItemType.PepperNeedle:
                //attacks who hurt you
                caller.curTarget = caller.lastAttacker;
                break;
            case Item.ItemType.StickySpore:
                //attacks enemy damaged
                caller.curTarget = target;
                break;
        }

        TargetArea ta = GetBaseTarget();
        if (GetItemType() == Item.ItemType.GoldBomb)
        {
            ta = new TargetArea(TargetArea.TargetAreaType.LiveAlly, true);  //backfire (need to swap out the target range to do this)
        }
        
        //Use this to build the list of targets
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, ta);
        if (!GetMultiTarget())
        {
            targets = new List<BattleEntity>
            {
                caller.curTarget
            };
        }

        //gold bomb also targets caller first
        if (targets.Contains(caller))
        {
            targets.Remove(caller);
            targets.Insert(0, caller);
        }


        yield return StartCoroutine(DefaultStartAnim(caller));
        //yield return StartCoroutine(caller.SmoothScale(0.5f, new Vector3(2, 2, 2)));

        //hacky fix
        bool removeQuick = false;
        if (GetModifier() == ItemModifier.Quick)
        {
            removeQuick = true;
            item.modifier = ItemModifier.None;
        }

        if (targets.Count > 0 && targets[0] != null)
        {
            yield return StartCoroutine(ExecuteEffect(caller, targets, Item.GetItemBoost(level - 1)));
        }

        if (removeQuick)
        {
            item.modifier = ItemModifier.Quick;
        }

        yield return StartCoroutine(ProducerAnim(caller));

        //yield return StartCoroutine(caller.SmoothScale(0.5f, new Vector3(1, 1, 1)));

        if (caller is PlayerEntity pcaller)
        {
            yield return new WaitForSeconds(0.5f);
            ItemDataEntry ide = Item.GetItemDataEntry(GetItem());
            if (pcaller.BadgeEquipped(Badge.BadgeType.ItemRebate))
            {
                pcaller.HealCoins((int)(ide.sellPrice * 0.75f * pcaller.BadgeEquippedCount(Badge.BadgeType.ItemRebate)));
            }
        }

        BattleRemoveItemUsed(caller);
    }
}


public class MetaItem_Identity : MetaItemMove
{
    //ItemMove item;

    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.None);
    }

    public override string GetDescription()
    {
        return GetDescription(Move.Normal);
    }

    public override string GetName()
    {
        return GetName(Move.Normal);
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        itemMove.ChooseMove(caller, level);

        yield return StartCoroutine(itemMove.Execute(caller, level));
    }

    public override Move GetMove()
    {
        return Move.Normal;
    }
}

public class MetaItem_Quick : MetaItemMove
{
    //ItemMove item;

    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.None);
    }

    public override string GetDescription()
    {
        return GetDescription(Move.Normal);
    }

    public override string GetName()
    {
        return GetName(Move.Normal);
    }

    public override void ChooseMove(BattleEntity caller, int level = 1)
    {
        BattleControl.Instance.QuickSupplyUses += 1;
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        itemMove.ChooseMove(caller, level);

        yield return StartCoroutine(itemMove.Execute(caller, level));

        if (caller is PlayerEntity pcaller)
        {
            if (caller.CanMove() && pcaller.QuickSupply != 2)
            {
                caller.actionCounter--;
                caller.InflictEffectForce(caller, new Effect(Effect.EffectType.BonusTurns, 1, Effect.INFINITE_DURATION));
                pcaller.QuickSupply = 2;
            }
        }
    }

    public override Move GetMove()
    {
        return Move.Quick;
    }
}

public class MetaItem_Void : MetaItemMove
{
    //ItemMove item;

    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.None);
    }

    public override string GetDescription()
    {
        return GetDescription(Move.Void);
    }

    public override string GetName()
    {
        return GetName(Move.Void);
    }

    public override void ChooseMove(BattleEntity caller, int level = 1)
    {
        BattleControl.Instance.VoidSupplyUses += 1;
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //item.ChooseMove(caller, level);
        BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.UseItem);
        itemMove.itemCount = BattleControl.Instance.CountAllItemsOfType(caller, itemMove.GetItemType());

        //extremely hacky fix
        caller.ReceiveEffectForce(new Effect(Effect.EffectType.Freebie, 1, Effect.INFINITE_DURATION), caller.posId, Effect.EffectStackMode.AdditivePot, false);
        yield return StartCoroutine(itemMove.Execute(caller, level));
        caller.TokenRemoveOne(Effect.EffectType.Freebie);

        BattleControl.Instance.playerData.GetPlayerDataEntry(caller.entityID).itemsUsed++;
        BattleControl.Instance.playerData.itemsUsed++;
    }

    public override Move GetMove()
    {
        return Move.Void;
    }
}

public class MetaItem_Multi : MetaItemMove
{
    public List<ItemMove> itemMoves;
    public List<BattleEntity> targets;
    public List<int> indices;

    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.None);
    }

    public override string GetDescription()
    {
        return GetDescription(Move.Multi);
    }

    public override string GetName()
    {
        return GetName(Move.Multi);
    }

    public override void ChooseMove(BattleEntity caller, int level = 1)
    {
        //Debug.Log(itemMoves.Count);
        if (itemMoves.Count > 1)
        {
            BattleControl.Instance.MultiSupplyUses += 1;
        }
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        for (int i = 0; i < itemMoves.Count; i++)
        {
            caller.curTarget = targets[i];

            //itemMoves[i].ChooseMove(caller, level);
            //note: can't use the normal choosemove because the indices need to be kept correct
            BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.UseItem);
            itemMoves[i].itemCount = BattleControl.Instance.CountAllItemsOfType(caller, itemMoves[i].GetItemType());
            itemMoves[i].BattleRemoveItemUsed(caller, indices[i]);

            yield return StartCoroutine(itemMoves[i].Execute(caller, level));

            yield return new WaitForSeconds(0.5f);

            if (!caller.CanMove())
            {
                yield break;
            }
        }


    }

    public override Move GetMove()
    {
        return Move.Multi;
    }
}