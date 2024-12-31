using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//determines who can be targetted by a move
public class TargetArea
{
    public enum TargetAreaType
    {
        None, //more often used than self target
        Special, //use special function
        Self,
        PlayerCanSwitch,
        PlayerCanSwitchNoHP,
        Enemy,
        LiveEnemy,
        Ally,
        AllyNotSelf,
        LiveAlly,
        LiveAllyNotSelf,
        LiveAllyNotSelfMovable,
        DeadAlly,
        Anyone,
        LiveAnyone,
        AnyoneNotSelf,
        LiveAnyoneNotSelf,
        LiveEnemyLow,
        LiveEnemyLowBottommost,
        LiveEnemyGrounded,
        LiveEnemyTopmost,
        LiveEnemyLowStompable,
        LiveEnemyLowFrontmost,
        Scan,
    }

    public class TargetAreaFunc
    {
        private string internalName;
        public string name
        {
            get
            {
                if (range != TargetAreaType.Special)
                {
                    internalName = GetRangeName(range);
                    return internalName;
                }
                else
                {
                    return internalName;
                }
            }
        }

        private Func<BattleEntity, BattleEntity, bool> checkerInternal;
        public Func<BattleEntity, BattleEntity, bool> checkerFunction
        {
            get
            {
                if (range != TargetAreaType.Special)
                {
                    checkerInternal = GetChecker(range);
                    return checkerInternal;
                }
                else
                {
                    return checkerInternal;
                }
            }
            set
            {
                checkerInternal = value;
                range = TargetAreaType.Special;
            }
        }
        public TargetAreaType range;

        public TargetAreaFunc(TargetAreaType range)
        {
            this.range = range;
            checkerInternal = GetChecker(range);
            internalName = GetRangeName(range);
        }
        public TargetAreaFunc(Func<BattleEntity, BattleEntity, bool> checker, string name = null)
        {
            checkerInternal = checker;
            range = TargetAreaType.Special;
            internalName = name;
        }

        public static string GetRangeName(TargetAreaType range)
        {
            return range.ToString();
        }
        public static Func<BattleEntity, BattleEntity, bool> GetChecker(TargetAreaType range)
        {
            //caller, entity
            switch (range)
            {
                case TargetAreaType.Self:
                    return ((c, e) => (c == e));
                case TargetAreaType.Enemy:
                    return ((c, e) => (e.posId >= 0 ^ c.posId >= 0));
                case TargetAreaType.LiveEnemy:
                    return ((c, e) => (e.hp > 0 && (e.posId >= 0 ^ c.posId >= 0)));
                case TargetAreaType.Ally:
                    return ((c, e) => !(e.posId >= 0 ^ c.posId >= 0));
                case TargetAreaType.AllyNotSelf:
                    return ((c, e) => e.posId != c.posId && !(e.posId >= 0 ^ c.posId >= 0));
                case TargetAreaType.LiveAlly:
                    return ((c, e) => (e.hp > 0 && !(e.posId >= 0 ^ c.posId >= 0)));
                case TargetAreaType.LiveAllyNotSelf:
                    return ((c, e) => e.posId != c.posId && (e.hp > 0 && !(e.posId >= 0 ^ c.posId >= 0)));
                case TargetAreaType.LiveAllyNotSelfMovable:
                    return ((c, e) => e.posId != c.posId && (e.CanMove() || e.AutoMove()) && (e.hp > 0 && !(e.posId >= 0 ^ c.posId >= 0)));
                case TargetAreaType.DeadAlly:
                    return ((c, e) => (e.hp <= 0 && !(e.posId >= 0 ^ c.posId >= 0)));
                case TargetAreaType.Anyone:
                    return ((c, e) => true);
                case TargetAreaType.LiveAnyone:
                    return ((c, e) => (e.hp > 0));
                case TargetAreaType.AnyoneNotSelf:
                    return ((c, e) => (e.posId != c.posId));
                case TargetAreaType.LiveAnyoneNotSelf:
                    return ((c, e) => (e.posId != c.posId && e.hp > 0));
                case TargetAreaType.LiveEnemyLow:
                    return ((c, e) => (e.GetEntityProperty(BattleHelper.EntityProperties.Airborne, false) && (e.hp > 0 && (e.posId >= 0 ^ c.posId >= 0))));
                case TargetAreaType.LiveEnemyLowBottommost:
                    return ((c, e) => (BattleControl.Instance.IsBottommost(e) && e.GetEntityProperty(BattleHelper.EntityProperties.Airborne, false) && (e.hp > 0 && (e.posId >= 0 ^ c.posId >= 0))));
                case TargetAreaType.LiveEnemyGrounded:
                    return ((c, e) => (e.GetEntityProperty(BattleHelper.EntityProperties.Grounded) && (e.hp > 0 && (e.posId >= 0 ^ c.posId >= 0))));
                case TargetAreaType.LiveEnemyTopmost:
                    return ((c, e) => (BattleControl.Instance.IsTopmost(e) && (e.hp > 0 && (e.posId >= 0 ^ c.posId >= 0))));
                case TargetAreaType.LiveEnemyLowStompable:
                    return ((c, e) => (BattleControl.Instance.IsTopmost(e) && (e.hp > 0 && (e.posId >= 0 ^ c.posId >= 0)) && e.GetEntityProperty(BattleHelper.EntityProperties.LowStompable, true)));
                case TargetAreaType.LiveEnemyLowFrontmost:
                    return ((c, e) => (BattleControl.Instance.IsFrontmostLow(c, e)));
                case TargetAreaType.Scan:
                    return ((c, e) => ((e.posId >= 0 ^ c.posId >= 0) && !e.GetEntityProperty(BattleHelper.EntityProperties.NoTattle)));
            }
            return (c, e) => false;
        }
    }

    public TargetAreaFunc checker;
    public Func<BattleEntity, BattleEntity, bool> checkerFunction
    {
        get
        {
            return checker.checkerFunction;
        }
    }
    public TargetAreaType range
    {
        get
        {
            return checker.range;
        }
        set
        {
            if (value == TargetAreaType.Special)
            {
                throw new NotImplementedException();
            }
            checker = new TargetAreaFunc(range);
        }
    }
    public bool allPossible; //false = single target, true = all targets in range

    public TargetArea() : this(TargetAreaType.None)
    {

    }
    public TargetArea(TargetAreaType p_range)
    {
        allPossible = false;
        checker = new TargetAreaFunc(p_range);
    }
    public TargetArea(TargetAreaType p_range, bool p_allPossible)
    {
        allPossible = p_allPossible;
        checker = new TargetAreaFunc(p_range);
    }
    public TargetArea(Func<BattleEntity, BattleEntity, bool> p_checker, bool p_allPossible)
    {
        checker = new TargetAreaFunc(p_checker);
        allPossible = p_allPossible;
    }
    public TargetArea(Func<BattleEntity, BattleEntity, bool> p_checker, string name, bool p_allPossible)
    {
        checker = new TargetAreaFunc(p_checker, name);
        allPossible = p_allPossible;
    }

    //only checks the target range, you have to manually set allPossible yourself
    public static bool TryParseRange(string s, out TargetArea t)
    {
        t = new TargetArea();
        //now try to parse it really
        TargetAreaType range;
        if (Enum.TryParse(s,out range))
        {
            t = new TargetArea(range);
            return true;
        } else
        {
            return false;
        }
    }

    public bool GetCheckerResult(BattleEntity c, BattleEntity e)
    {
        return checker.checkerFunction(c, e);
    }
    public List<BattleEntity> SelectFrom(BattleEntity caller, List<BattleEntity> list)
    {
        return list.FindAll((e) => checker.checkerFunction(caller, e));
    }
}

public interface IEntityHighlighter
{
    public string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1);
}



//determines how targets are chosen from a list of targets (how enemies are prioritized)
//This is specifically used by enemies and not players (you pick the targets, and even if you don't I'll manually force targetting without this wacky system)

public class TargetStrategy
{
    public enum TargetStrategyType
    {
        None,   //returns null always!
        Special, //anything using special methods
        Random, //bad
        HighHP,
        LowHP,
        FrontMost,
        BackMost,
        XCenterMost, //closer to center of fight
        XLateralMost, //furthest from center of fight
    }

    public class TargetStrategyFunc
    {
        private string internalName;
        public string name
        {
            get
            {
                if (priority != TargetStrategyType.Special)
                {
                    internalName = GetRangeName(priority);
                    return internalName;
                }
                else
                {
                    return internalName;
                }
            }
        }

        private Func<BattleEntity, BattleEntity, int> selectorInternal;
        public Func<BattleEntity, BattleEntity, int> selectorFunction
        {
            get
            {
                if (priority != TargetStrategyType.Special)
                {
                    selectorInternal = GetSelector(priority);
                    return selectorInternal;
                }
                else
                {
                    return selectorInternal;
                }
            }
            set
            {
                selectorInternal = value;
                priority = TargetStrategyType.Special;
            }
        }
        public TargetStrategyType priority;

        public TargetStrategyFunc(TargetStrategyType priority)
        {
            this.priority = priority;
            selectorInternal = GetSelector(priority);
            internalName = GetRangeName(priority);
        }
        public TargetStrategyFunc(Func<BattleEntity, BattleEntity, int> selector, string name = null)
        {
            selectorInternal = selector;
            priority = TargetStrategyType.Special;
            internalName = name;
        }

        public static string GetRangeName(TargetStrategyType priority)
        {
            return priority.ToString();
        }

        //these seem "reversed"
        //(most of the time the bias is towards [0] in a sorted list)        
        public static Func<BattleEntity, BattleEntity, int> GetSelector(TargetStrategyType priority)
        {
            //compare 2 entities to see which one is better (basically a comparison function)
            switch (priority)
            {
                case TargetStrategyType.Random: //doing it this way guarantees a fixed result
                    return (a, b) =>
                    {
                        uint thash = RandomGenerator.Hash((uint)BattleControl.Instance.turnCount);
                        int ahash = ((int)RandomGenerator.Hash((uint)a.posId + thash) + (int)RandomGenerator.Hash((uint)a.hp + thash));
                        int bhash = ((int)RandomGenerator.Hash((uint)b.posId + thash) + (int)RandomGenerator.Hash((uint)b.hp + thash));

                        return ahash - bhash;
                    };
                case TargetStrategyType.HighHP:
                    return (a, b) => (b.hp - a.hp);
                case TargetStrategyType.LowHP:
                    return (a, b) => (a.hp - b.hp);
                case TargetStrategyType.FrontMost:
                    return (a, b) => (MainManager.FloatCompare(b.transform.position[0], a.transform.position[0]));
                case TargetStrategyType.BackMost:
                    return (a, b) => (MainManager.FloatCompare(a.transform.position[0], b.transform.position[0]));
                case TargetStrategyType.XCenterMost:
                    return (a, b) => (MainManager.FloatCompare(Mathf.Abs(b.transform.position[0]), Mathf.Abs(a.transform.position[0])));
                case TargetStrategyType.XLateralMost:
                    return (a, b) => (MainManager.FloatCompare(Mathf.Abs(a.transform.position[0]), Mathf.Abs(b.transform.position[0])));
            }
            return (c, i) => 0;
        }
    }

    public TargetStrategyFunc selector;
    public Func<BattleEntity, BattleEntity, int> selectorFunction
    {
        get => selector.selectorFunction;
    }
    public TargetStrategyType priority
    {
        get
        {
            return selector.priority;
        }
        set
        {
            if (value == TargetStrategyType.Special)
            {
                throw new NotImplementedException();
            }
            selector = new TargetStrategyFunc(priority);
        }
    }

    public TargetStrategy() : this(TargetStrategyType.None)
    {

    }
    public TargetStrategy(TargetStrategyType tp)
    {
        selector = new TargetStrategyFunc(tp);

    }
    public TargetStrategy(Func<BattleEntity, BattleEntity, int> p_selector, string p_name = null)
    {
        selector = new TargetStrategyFunc(p_selector, p_name);
    }

    public int GetSelectorResult(BattleEntity a, BattleEntity b)
    {
        return selector.selectorFunction(a, b);
    }
    public BattleEntity SelectFrom(BattleEntity caller, List<BattleEntity> list)
    {
        return MainManager.GetHighest(list, (a, b) => (selector.selectorFunction(a, b)));
    }
    public List<BattleEntity> SelectFrom(BattleEntity caller, List<BattleEntity> list, int count)
    {
        return MainManager.GetHighestNumber(list, (a, b) => (selector.selectorFunction(a, b)), count);
    }
}

public abstract class PlayerMove : Move, IEntityHighlighter
{
    public abstract int GetTextIndex();
    //public abstract string GetName();
    public abstract string GetDescription(int level = 1);
    public override string GetDescription() //overshadowed by default parameter method?
    {
        return GetDescription(1);
    }
    //public abstract float GetBasePower();

    public abstract int GetBaseCost();

    public abstract BaseBattleMenu.BaseMenuName GetMoveType();

    public enum CantMoveReason
    {
        Unknown = 0,    //this is the "you just can't use it" text
        NoTargets,
        NotEnoughEnergy,
        NotEnoughSoul,
        NotEnoughHealth,
        NotEnoughCoins,
        NotEnoughStamina,
        BlockFlee,
        BlockSkills,
        BlockJump,
        BlockWeapon,
        BlockSoul,
        BlockItems,
        BlockSkillEnvironment,
        NoItems,
        FullItems,
        ItemOverworldOnly,
        ItemExpended,
        ItemMultiSupplyBlock,
        MoveExpended,
        TeamMoveNoTeammate,
        TeamMoveUnavailableTeammate,
        RibbonUnavailable,
        BadgeSwapDisabled,
        BadgeTooExpensive,
        BadgeUnavailable,
        BadgeSwapExpended,
    }

    public virtual CantMoveReason GetCantMoveReason(BattleEntity caller, int level = 1)
    {
        if (BattleControl.IsPlayerControlled(caller, true) && !PlayerTurnController.Instance.CanChoose(this, caller))
        {
            return CantMoveReason.Unknown;
        }

        if (GetTargetArea(caller, level).range != TargetArea.TargetAreaType.None && BattleControl.Instance.GetEntities(caller, GetBaseTarget()).Count == 0)
        {
            return CantMoveReason.NoTargets;
        }

        int cost = GetCost(caller, level);

        if (UseStamina() && cost > 0 && caller.HasEffect(Effect.EffectType.Exhausted))
        {
            return CantMoveReason.BlockSkills;
        }

        if (UseFlow() && caller.HasEffect(Effect.EffectType.Dread))
        {
            return CantMoveReason.BlockSoul;
        }

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

        if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.SacredGrove)
        {
            if (UseStamina() && (staminaCost - caller.GetEffectHasteBonus() >= 8))
            {
                return CantMoveReason.BlockSkillEnvironment;
            }
        }
        if (BattleControl.Instance.enviroEffect == BattleHelper.EnvironmentalEffect.TrialOfSimplicity)
        {
            if (UseStamina() && (staminaCost - caller.GetEffectHasteBonus() >= 5))
            {
                return CantMoveReason.BlockSkillEnvironment;
            }
        }

        if (UseStamina() && (caller.stamina < staminaCost - caller.GetEffectHasteBonus()))
        {
            return CantMoveReason.NotEnoughStamina;
        }

        return CantMoveReason.Unknown;
    }

    //How do you pay for this move?
    public virtual BattleHelper.MoveCurrency GetCurrency(BattleEntity caller = null)
    {
        if (caller is PlayerEntity pcaller)
        {
            if (pcaller.BadgeEquipped(Badge.BadgeType.VitalEnergy))
            {
                return BattleHelper.MoveCurrency.Health;
            }
            if (pcaller.BadgeEquipped(Badge.BadgeType.SoulfulEnergy))
            {
                return BattleHelper.MoveCurrency.Soul;
            }
            if (pcaller.BadgeEquipped(Badge.BadgeType.GoldenEnergy))
            {
                return BattleHelper.MoveCurrency.Coins;
            }
            if (pcaller.BadgeEquipped(Badge.BadgeType.StaminaEnergy))
            {
                return BattleHelper.MoveCurrency.Stamina;
            }
        }

        return BattleHelper.MoveCurrency.Energy;
    }



    //Future thing to code: add some way to change which currency this uses (hp, coins)
    public virtual int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level);
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
        return false;
    }

    //use this as a template
    public virtual int StandardCostCalculation(BattleEntity caller, int level = 1, int scale = 2)
    {
        if (GetBaseCost() == 0)
        {
            return 0;
        }

        return StandardCostModification(caller, level, GetBaseCost() + ((level - 1) * scale));
    }

    public virtual int StandardCostModification(BattleEntity caller, int level = 1, int cost = 1)
    {
        if (cost <= 0)
        {
            return 0;
        }

        int modifiedCost = cost;

        if (UseBurst())
        {
            modifiedCost -= caller.GetEffectEnduranceBonus() + caller.GetBadgeEnduranceBonus();

            int momentum = 0;
            int dizzyMomentum = 0;
            if (caller is PlayerEntity pcallerB)
            {
                momentum = pcallerB.BadgeEquippedCount(Badge.BadgeType.PowerMomentum);
                dizzyMomentum = pcallerB.BadgeEquippedCount(Badge.BadgeType.DizzyMomentum);
            }

            if (momentum > 0)
            {
                modifiedCost = (int)(modifiedCost * (1 + (0.25f * (momentum + 1)) * caller.actionCounter));
            }
            else
            {
                modifiedCost = (int)(modifiedCost * (1 + 0.25f * caller.actionCounter));
            }

            if (dizzyMomentum > 0)
            {
                modifiedCost = (int)(modifiedCost / (dizzyMomentum + 1));
            }
        }

        if (UseFlow())
        {
            modifiedCost -= caller.GetEffectFlowBonus() + caller.GetBadgeFlowBonus();
        }

        if (modifiedCost < 1)
        {
            modifiedCost = 1;
        }

        //apply the currency changers
        if (caller is PlayerEntity pcaller)
        {
            if (UseBurst())
            {
                if (pcaller.BadgeEquipped(Badge.BadgeType.VitalEnergy))
                {
                    return (int)Mathf.Clamp(modifiedCost - 1, 1, float.PositiveInfinity);
                }
                if (pcaller.BadgeEquipped(Badge.BadgeType.SoulfulEnergy))
                {
                    return modifiedCost;
                }
                if (pcaller.BadgeEquipped(Badge.BadgeType.GoldenEnergy))
                {
                    return modifiedCost * 5;
                }
                if (pcaller.BadgeEquipped(Badge.BadgeType.StaminaEnergy))
                {
                    return modifiedCost * 2;
                }

                if (BattleControl.Instance.GetEP(caller) <= 6 && pcaller.BadgeEquipped(Badge.BadgeType.NullEndurance))
                {
                    modifiedCost = (int)((1 / (1.00001f + pcaller.BadgeEquippedCount(Badge.BadgeType.NullEndurance)) * modifiedCost));
                    if (modifiedCost < 1)
                    {
                        modifiedCost = 1;
                    }
                }
            }

            if (UseFlow())
            {
                if (pcaller.BadgeEquipped(Badge.BadgeType.DarkEndurance))
                {
                    modifiedCost *= (1 + pcaller.BadgeEquippedCount(Badge.BadgeType.DarkEndurance));
                }

                if (pcaller.BadgeEquipped(Badge.BadgeType.EnergeticSoul))
                {
                    return (int)Mathf.Clamp(modifiedCost / 2, 1, float.PositiveInfinity);
                }
            }
        }

        return modifiedCost;
    }

    public PlayerMove()
    {
    }

    //Future thing to code: a "reason why you can't use this" function
    public override bool CanChoose(BattleEntity caller, int level = 1)
    {
        if (GetBaseCost() == 0)
        {
            return base.CanChoose(caller, level);    //Note: need to capture the "no valid target" check //true;
        }

        int cost = GetCost(caller, level);

        if (UseStamina() && cost > 0 && caller.HasEffect(Effect.EffectType.Exhausted))
        {
            return false;
        }

        if (UseFlow() && caller.HasEffect(Effect.EffectType.Dread))
        {
            return false;
        }

        if (!MainManager.Instance.Cheat_EnergyAnarchy)
        {
            switch (GetCurrency(caller))
            {
                case BattleHelper.MoveCurrency.Energy:
                    if (BattleControl.Instance.GetEP(caller) < cost)
                    {
                        return false;
                    }
                    break;
                case BattleHelper.MoveCurrency.Health:  //Note: you are not allowed to pay with all your hp (leaving you with 0)
                    if (caller.hp - 1 < cost)
                    {
                        return false;
                    }
                    break;
                case BattleHelper.MoveCurrency.Stamina:
                    if (caller.stamina < cost - caller.GetEffectHasteBonus())
                    {
                        return false;
                    }
                    break;
                case BattleHelper.MoveCurrency.Soul:
                    if (BattleControl.Instance.GetSE(caller) < cost)
                    {
                        return false;
                    }
                    break;
                case BattleHelper.MoveCurrency.Coins:
                    if (BattleControl.Instance.playerData.coins < cost)
                    {
                        return false;
                    }
                    break;
            }
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

        return base.CanChoose(caller, level);
    }    

    //pay the costs of the move (note that handling execution of the move is elsewhere)
    public override void ChooseMove(BattleEntity caller, int level = 1)
    {
        this.level = level;
        base.ChooseMove(caller, level);

        int cost = GetCost(caller, level);

        int prevStamina = caller.stamina;
        int prevEnergy = BattleControl.Instance.GetEP(caller);

        bool burstRecycling = false;
        int burstRecyclingCount = 0;

        if (caller is PlayerEntity p)
        {
            burstRecycling = p.BadgeEquipped(Badge.BadgeType.BurstRecycling);
            burstRecyclingCount = p.BadgeEquippedCount(Badge.BadgeType.BurstRecycling);
            if (UseBurst())
            {
                p.energyUsed += cost;
            }
            p.movesUsed++;
            
        }

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

        int healEffects = 0;

        int staminaCost = cost;
        if (caller is PlayerEntity pcaller)
        {
            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Coins && pcaller.BadgeEquipped(Badge.BadgeType.GoldenEnergy))
            {
                staminaCost = cost / 5;
            }
            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Stamina && pcaller.BadgeEquipped(Badge.BadgeType.StaminaEnergy))
            {
                //stamina cost is ignored in this case so this entire block is pretty unnecesary
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

            //Healthy Exercise
            if (UseStamina() && GetCurrency(caller) != BattleHelper.MoveCurrency.Stamina)
            {
                if (pcaller.BadgeEquipped(Badge.BadgeType.HealthyExercise))
                {
                    int hpheal = Mathf.CeilToInt((staminaCost * pcaller.BadgeEquippedCount(Badge.BadgeType.HealthyExercise)) / 6.0f);
                    pcaller.HealHealth(hpheal);
                    healEffects++;
                }
            }
            //Alt
            if (GetCurrency(caller) == BattleHelper.MoveCurrency.Stamina)
            {
                if (pcaller.BadgeEquipped(Badge.BadgeType.HealthyExercise))
                {
                    int hpheal = Mathf.CeilToInt((cost * pcaller.BadgeEquippedCount(Badge.BadgeType.HealthyExercise)) / 6.0f);
                    pcaller.HealHealth(hpheal);
                    healEffects++;
                }
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
                if (burstRecycling)
                {
                    int count = burstRecyclingCount;
                    Effect e = caller.GetEffectEntry(Effect.EffectType.Burst);
                    if (e != null)
                    {
                        e.potency = (sbyte)(e.potency * ((count + 0.0001f) / (count + 1f)));
                        if (e.potency == 0)
                        {
                            caller.RemoveEffect(Effect.EffectType.Burst);
                        }
                    }
                }
                else
                {
                    caller.RemoveEffect(Effect.EffectType.Burst);
                }
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

        if (UseBurst())
        {
            BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Skill);
        }

        if (GetMoveType() == BaseBattleMenu.BaseMenuName.Jump)
        {
            BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Jump);
        }

        if (GetMoveType() == BaseBattleMenu.BaseMenuName.Weapon)
        {
            BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.Weapon);
        }

        if (UseFlow())
        {
            BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.SoulMove);
        }

        int postStamina = caller.stamina;
        int postEnergy = BattleControl.Instance.GetEP(caller);
        if (caller is PlayerEntity pB)
        {
            if (UseBurst())
            {
                if (prevStamina > 0 && postStamina == 0 && pB.BadgeEquipped(Badge.BadgeType.StaminaRebound))
                {
                    Vector3 offsetA = Vector3.zero;
                    if (healEffects > 0)
                    {
                        offsetA = Vector3.left * 0.5f;
                    }

                    int rebound = pB.BadgeEquippedCount(Badge.BadgeType.StaminaRebound);
                    int healAmount = (int)(0.001f + (pB.agility * rebound) / (1f));
                    pB.HealStamina(healAmount);
                    healEffects++;
                }

                //stamina recycling
                if (pB.BadgeEquipped(Badge.BadgeType.StaminaRecycling))
                {
                    Vector3 offsetA = Vector3.zero;
                    if (healEffects > 0)
                    {
                        offsetA = Vector3.up * 0.5f;
                    }

                    int recyclingCount = pB.BadgeEquippedCount(Badge.BadgeType.StaminaRecycling);
                    int healAmount = (int)(0.001f + (((prevStamina - postStamina) * recyclingCount) / (1f + recyclingCount)));
                    if ((healAmount > 0)) {
                        pB.HealStamina(healAmount);
                        healEffects++;
                    }
                }

                //depletion burst
                if (prevEnergy > 0 && postEnergy == 0 && pB.BadgeEquipped(Badge.BadgeType.DepletionBurst))
                {
                    pB.HealEnergy(5 * pB.BadgeEquippedCount(Badge.BadgeType.DepletionBurst));
                    healEffects++;
                }
            }
        }
    }

    //What text can display if you hover over an enemy?
    //Usually this displays damage, status susceptibility, etc
    //(this should support TextDisplayer stuff)
    public virtual string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        return "";
    }
}

public abstract class SoulMove : PlayerMove
{
    public enum MoveType
    {
        Revitalize,
        Hasten,
        LeafStorm,
        ElectroDischarge,
        MistWave,
        Overheat,
        VoidCrush,
        FlashFreeze,
        Cleanse,
        Blight,
        ChromaBlast,
        AbyssalDawn
    }

    //public abstract int GetTextIndex();

    //How do you pay for this move?
    public override BattleHelper.MoveCurrency GetCurrency(BattleEntity caller = null)
    {
        if (caller is PlayerEntity pcaller)
        {
            if (pcaller.BadgeEquipped(Badge.BadgeType.EnergeticSoul))
            {
                return BattleHelper.MoveCurrency.Energy;
            }
        }

        return BattleHelper.MoveCurrency.Soul;
    }

    //does this cost stamina?
    public override bool UseStamina()
    {
        return false;
    }

    //does this use burst, endurance, etc?
    public override bool UseBurst()
    {
        return false;
    }

    //does this use awaken, flow, etc?
    public override bool UseFlow()
    {
        return true;
    }

    public override int StandardCostCalculation(BattleEntity caller, int level = 1, int scale = 2)
    {
        if (GetBaseCost() == 0)
        {
            return 0;
        }

        return StandardCostModification(caller, level, GetBaseCost() + ((level - 1) * scale));
    }

    public override int StandardCostModification(BattleEntity caller, int level = 1, int cost = 1)
    {
        if (cost <= 0)
        {
            return 0;
        }

        int modifiedCost = cost;

        if (UseBurst())
        {
            modifiedCost -= caller.GetEffectEnduranceBonus() + caller.GetBadgeEnduranceBonus();

            modifiedCost = (int)(modifiedCost * (1 + 0.25f * caller.actionCounter));
        }

        if (UseFlow())
        {
            modifiedCost -= caller.GetEffectFlowBonus() + caller.GetBadgeFlowBonus();
        }

        if (modifiedCost < 1)
        {
            modifiedCost = 1;
        }

        //apply the currency changers
        if (caller is PlayerEntity pcaller)
        {
            if (pcaller.BadgeEquipped(Badge.BadgeType.DarkEndurance))
            {
                modifiedCost *= 2;
            }

            if (pcaller.BadgeEquipped(Badge.BadgeType.EnergeticSoul))
            {
                if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Envy))
                {
                    return (int)Mathf.Clamp(modifiedCost / 4, 1, float.PositiveInfinity);                
                }
                else
                {
                    return (int)Mathf.Clamp(modifiedCost / 2, 1, float.PositiveInfinity);
                }
            }
        }

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Envy))
        {
            modifiedCost /= 2;
        }

        return modifiedCost;
    }


    public override bool ShowNamePopup()
    {
        return false;
    }

    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);

    public static string GetNameWithIndex(int index)
    {
        string output = BattleControl.Instance.soulText[index + 1][1];
        return output;
    }

    public static string GetDescriptionWithIndex(int index, int level = 1)
    {
        string output = BattleControl.Instance.soulText[index + 1][2];

        if (level != 1)
        {
            output += " <color,#0000ff>(Lv. " + level + ": " + BattleControl.Instance.soulText[index + 1][1 + level] + ")</color>";
        }

        return output;
    }

    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Soul;

    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
        return 1;
    }

    //currently they use the same action command
    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_PressATimed.GetACDesc();
    }
}

//just for better organization, and for possible future feature stuff
public abstract class EnemyMove : Move
{
    //is this usable when berserk (not used)
    //public abstract bool BerserkerMove();

    //Convention: Shared is at the start in rough order of chapters they first appear
    //Then the chapter specific enemies (preserve a similar order to which enemy in the list first uses the move)
    //  Enemy specific moves are prefixed with the first enemy that uses the move (Note that similar enemies may share scripts. They may not be "Shared" moves if those moves are too specific)
    //Hard mode exclusive moves are prefixed with Hard (Most of them are shared moves also)
    //  Should appear after the normal move moves
    //  (Hard prefix should be after the enemy name)
    public enum MoveIndex
    {
        Unknown = 0,
        Bite,
        Swoop,      
        FrontBite,
        BiteThenFly,
        Slash,
        DualSlash,
        DoubleSwoop,
        TripleSwoop,
        Hard_CounterEnrage,
        Hard_CounterRush,
        Hard_CounterHarden,
        Hard_CounterRecover,
        Hard_CounterReinforce,
        Hard_CounterRally,
        Hard_CounterRoar,
        Hard_CounterHide,
        Hard_CounterProtect,
        Hard_CounterShield,
        Hard_CounterMarshall,
        Leafling_Hard_TailWhip,
        Flowerling_Hard_SwoopBloom,
        Sunflower_Hard_SolarBite,
        Sunnybud_BiteFlyHeal,
        Sunnybud_BiteFlyHealIlluminate,
        Sunnybud_BiteFlyHealMiracle,
        Rootling_FrontSlam,
        Rootling_DoubleSlam,
        Rootling_Dig,
        Rootling_Uproot,
        SunSapling_FrontSlam,
        SunSapling_BigSlam,
        SunSapling_GrowCharge,
        SunSapling_PowerRoar,
        Honeybud_SwoopHeal,
        Honeybud_SwoopHealIlluminate,
        Honeybud_SwoopHealMiracle,
        BurrowTrap_PollenBite,
        BurrowTrap_CounterPollenBite,
        BurrowTrap_Hard_SunBloom,
        Sundew_PoisonToss,
        Sundew_CounterPoisonToss,
        Sundew_Hard_ExhaustBall,
        VinePlatform_Hard_CounterSoften,
        Sycamore_ThornToss,
        Sycamore_Pollenate,
        Sycamore_FlowerShuriken,
        Sycamore_Overgrowth,
        Sycamore_VineStab,
        Sycamore_FullBloom,
        Sycamore_VineField,
        Sycamore_Hard_RootShake,
        Sycamore_Hard_RootGrasp,
        Sycamore_Fall,
        GiantVine_Slam,
        GiantVine_BigSlam,
        GiantVine_Telegraph,
        GiantVine_Grab,
        GiantVine_Constrict,
        GiantVine_Throw,
        GiantVine_Hard_LashOut,
        MasterOfAutumn_ThornToss,
        MasterOfAutumn_PollenStorm,
        MasterOfAutumn_FlowerShuriken,
        MasterOfAutumn_Overgrowth,
        MasterOfAutumn_VineStab,
        MasterOfAutumn_FullBloom,
        MasterOfAutumn_VineField,
        MasterOfAutumn_Resummon,
        MasterOfAutumn_Fall,
        MasterOfAutumn_Hard_RootShake,
        MasterOfAutumn_Hard_RootDrain,
        Bandit_Slash,
        Bandit_Hard_TeamCounter,
        Renegade_Hard_HeatWave,
        Sentry_Fling,
        Sentry_CounterFling,
        Cactupole_ThornShock,
        Cactupole_Hard_StormFortify,
        Sandswimmer_Bite,
        Sandswimmer_Hard_FlashDischarge,
        Slime_Stomp,
        Slime_SplashStomp,
        Slime_Hard_CounterMistWall,
        Slimewalker_WaterCannon,
        Slimewalker_Hard_SoftSplash,
        Slimeworm_Charge,
        Slimeworm_Mortar,
        Slimeworm_Hard_DeepMortar,
        Slimebloom_Zap,
        Slimebloom_Lob,
        Slimebloom_Hard_Flash,
        Sirenfish_BubbleSong,
        Sirenfish_PowerSong,
        Sirenfish_Hard_NightmareBite,
        Blazecrest_FlameBreath,
        Blazecrest_Roar,
        Blazecrest_Hard_CounterRageFlare,
        Embercrest_Fireball,
        Ashcrest_ThornBomb,
        Ashcrest_SplashBomb,
        Flametongue_FlameLick,
        Flametongue_Hard_HustleLick,
        Heatwing_FlameSpread,
        Heatwing_Hard_DreadScreech,
        EyeSpore_SporeBeam,
        EyeSpore_Hard_CounterSpiteBeam,
        SpikeSpore_PoisonSpikes,
        SpikeSpore_Hard_SpikeBomb,
        Shrouder_SporeCloud,
        Shrouder_SporeCloak,
        HoarderFly_PoisonHeal,
        HoarderFly_Hard_DustWind,
        HoarderFly_Hard_FinalHeal,
        Mosquito_ShockNeedle,
        Mosquito_DrainBite,
        Mosquito_Hard_Shroud,
        Shieldwing_DualPeck,
        Shieldwing_ChillingScreech,
        Shieldwing_FeatherWall,
        Honeywing_SpitHeal,
        Honeywing_SwoopHealSoften,
        Shimmerwing_DazzlingScreech,
        Shimmerwing_RallySong,
        Shimmerwing_Hard_StaticFlurry,
        LumistarVanguard_HornBlast,
        LumistarVanguard_LanternRally,
        LumistarVanguard_Hard_SoftGlow,
        LumistarSoldier_Charge,
        LumistarSoldier_ChargeSlash,
        LumistarStriker_DualSlash,
        LumistarStriker_Charge,
        LumistarStriker_QuadSlash,
        Plateshell_Slam,
        Plateshell_RageFireball,
        Speartongue_TongueStab,
        Speartongue_Shroud,
        Speartongue_Stare,
        Chaintail_ShockClaw,
        Chaintail_TailWhip,
        Chaintail_Hard_PowerFlash,
        Sawcrest_SawRush,
        Sawcrest_CounterSawToggle,
        Sawcrest_Hard_DeepCut,
        Sawcrest_Hard_CounterRevUp,
        Coiler_Slap,
        Coiler_Charge,
        Coiler_ElectroStorm,
        Coiler_Hard_CounterRollerShell,
        Drillbeak_Drill,
        Drillbeak_Hard_DreadStab,
        PuffJelly_Slam,
        PuffJelly_Hard_BlinkSlam,
        Fluffling_Bash,
        Fluffling_Hard_WaterTorpedo,
        CloudJelly_IceSwing,
        CloudJelly_FrostFortify,
        CloudJelly_BubbleToss,
        CloudJelly_BubbleBlast,
        CloudJelly_PowerBolt,
        CloudJelly_PowerCharge,
        CloudJelly_CounterFormChange,
        CrystalCrab_TripleClaw,
        CrystalCrab_DarkClaw,
        CrystalCrab_Hard_CounterClearClaw,
        CrystalSlug_Slap,
        CrystalSlug_ChillingStare,
        CrystalClam_HealingBreath,
        CrystalClam_CleansingBreath,
        CrystalClam_Explode,
        AuroraWing_RubyDust,
        AuroraWing_SapphireDust,
        AuroraWing_EmeraldDust,
        Plaguebud_HeadSprout,
        Plaguebud_TailSprout,
        Starfish_FeebleWave,
        Starfish_FatigueFog,
        Starfish_LeafStorm,
        CursedEye_UnnervingStare,
        CursedEye_MaliciousStare,
        CursedEye_CounterSpitefulStare,
        CursedEye_Hard_InvertedStare,
        StrangeTendril_StrangeCoil,
        StrangeTendril_Slam,
        DrainBud_PowerDrain,
        DrainBud_Hard_DrainBloom,
    }

    public abstract MoveIndex GetMoveIndex();
    public override string GetName()
    {
        return GetNameWithIndex(GetMoveIndex());
    }

    public override bool ShowNamePopup()
    {
        return true;
    }

    public override string GetDescription()
    {
        //note: may change in harder difficulties
        if (BattleControl.Instance.curseLevel > 0)
        {
            return GetHardDescriptionWithIndex(GetMoveIndex());
        }
        else
        {
            return GetDescriptionWithIndex(GetMoveIndex());
        }
    }

    public static string GetNameWithIndex(MoveIndex mi)
    {
        //Debug.Log(mi);
        string output = MainManager.Instance.enemyMoveText[(int)mi + 1][1];
        return output;
    }

    public static string GetDescriptionWithIndex(MoveIndex mi)
    {
        string output = MainManager.Instance.enemyMoveText[(int)mi + 1][2];
        return output;
    }
    public static string GetHardDescriptionWithIndex(MoveIndex mi)
    {
        string output = MainManager.Instance.enemyMoveText[(int)mi + 1][2] + "<line><color,red>" + MainManager.Instance.enemyMoveText[(int)mi + 1][3] + "</color>";
        return output;
    }
}

public abstract class Move : MonoBehaviour
{
    //static variables are incompatible with how I want to code moves

    //public static float base_time;
    //public static TargetArea base_target;
    //public static string move_name;
    //public static float base_power; //multiply caller.attackPower by value

    //low time values execute before high time values
    //public float time;      //reciprocal of move speed
    //public float power; //individual enemies may change power mult (and individual move parameters)
                        //for example, an enemy may have a bite attack stronger than normal and a different attack weaker than normal
                        //no separate target because that doesn't change

                        //future shade: this can be done separately, power has become redundant

    //kicking this from playermove back to here
    //it makes code easier but makes some stuff weirder
    public int level;

    //max level of this move
    public virtual int GetMaxLevel(BattleEntity caller)
    {
        return 1;
    }

    //Note: Avoid mixing moves with callers and sourceless moves
    //(may cause weird problems?)
    public virtual bool GetSourceless()
    {
        return false;
    }

    public Move()
    {
        //power = GetBasePower();
        //time = GetBaseTime();
        level = 1;
    }
    
    public abstract TargetArea GetBaseTarget();

    
    public abstract string GetName();
    public abstract bool ShowNamePopup();
    public abstract string GetDescription();
    
    //public abstract float GetBasePower();
    public virtual string GetActionCommandDesc(int level = 1)
    {
        return "";
    }

    //does this move have a counter reaction? (Note that moves can do stuff when executed while also doing something else as a counter)
    //public bool isCounterReact;

    /*
    public virtual BoxMenuEntry ConstructBattleMenuEntry()
    {
        BoxMenuEntry b = new BoxMenuEntry(GetName());
        return b;
    }
    */

    public virtual TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        return GetBaseTarget();
    }

    //coroutine
    public virtual IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return null;
    }

    //interrupt other stuff
    public virtual IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        yield return null;
    }

    /*
    //first strike
    public virtual IEnumerator ExecuteFirstStrike(BattleEntity caller)
    {
        yield return StartCoroutine(Execute(caller));
    }
    */

    //this is executed in the PreMove phase, so don't put anything too important in here
    //Note that since moves are chosen after the PreMove phase, this will happen the turn AFTER doing this move!
    public virtual void PreMove(BattleEntity caller, int level = 1)
    {
    }

    //executed in choose move phase (initializing stuff?) (deplete cost)
    public virtual void ChooseMove(BattleEntity caller, int level = 1)
    {
        this.level = level;
    }

    //Note that this base form calls the GetEntity function
    public virtual bool CanChoose(BattleEntity caller, int level = 1)
    {
        if (BattleControl.IsPlayerControlled(caller, true) && !PlayerTurnController.Instance.CanChoose(this, caller))
        {
            return false;
        }

        //make sure at least one target is available
        if (GetTargetArea(caller, level).range == TargetArea.TargetAreaType.None || BattleControl.Instance.GetEntities(caller, GetBaseTarget()).Count > 0)
        {
            return true;
        }

        return false;
    }

    //executed in postmove phase
    public virtual void PostMove(BattleEntity caller, int level = 1)
    {

    }
}
