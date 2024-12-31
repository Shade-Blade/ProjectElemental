using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct StatusTableEntry //how much an entity resists certain statuses (positive statuses ignore this)
{                              //default controls default resistance if status is unspecified
    public Effect.EffectType status;

    public float susceptibility;
    public float turnMod;

    public StatusTableEntry(Effect.EffectType p_status = Effect.EffectType.Default, float p_suseptibility = 1, float p_turnMod = 1)
    {
        status = p_status;
        susceptibility = p_suseptibility;
        turnMod = p_turnMod;
    }

    public override string ToString()
    {
        string output = status.ToString() + ": " + susceptibility + " suseptibility, " + turnMod + " turn modifier";
        return output;
    }
}

[System.Serializable]
public class Effect
{
    public enum EffectStackMode
    {
        Default = 0,
        KeepPotAddDur,
        KeepDurAddPot,
        AdditiveDur,
        AdditivePot,
        OverwriteLow,
    }

    public enum EffectClass
    {
        Static,
        Status,
        BuffDebuff,
        Token
    }

    //potential future thing: add some "internal" statuses that show icons but are actually controlled by the system
    public enum EffectType
    {
        Default = -1, //only valid as a status table entry

        AttackBoost,
        DefenseBoost,
        EnduranceBoost,
        AgilityBoost,
        MaxHPBoost,
        MaxEPBoost,
        MaxSEBoost,
        Swift,

        AttackReduction,
        DefenseReduction,
        EnduranceReduction,
        AgilityReduction,
        MaxHPReduction,
        MaxEPReduction,
        MaxSEReduction,
        Slow,

        Berserk,
        Sleep,
        Poison,
        Freeze,
        Dizzy,
        Paralyze,
        Soulbleed,
        Sunflame,
        Brittle,
        Inverted,
        Dread,
        ArcDischarge,
        TimeStop,        
        Exhausted,
        Splotch,

        AttackUp,
        DefenseUp,
        EnduranceUp,
        AgilityUp,
        FlowUp,
        HealthRegen,
        EnergyRegen,
        SoulRegen,
        Hustle,
        EffectStasis,
        Immunity,
        Ethereal,
        Illuminate,
        MistWall,
        AstralWall,
        ParryAura,
        BolsterAura,
        AstralRecovery,
        Elusive,
        CounterFlare,
        Supercharge,
        QuantumShield,
        Soften,

        AttackDown,
        DefenseDown,
        EnduranceDown,
        AgilityDown,
        FlowDown,
        HealthLoss,
        EnergyLoss,
        SoulLoss,
        Sluggish,
        DrainSprout,
        BoltSprout,
        Seal,
        Sticky,
        DamageOverTime,

        Focus,
        Absorb,
        Burst,
        Haste,
        BonusTurns,
        Awaken,
        Miracle,
        Freebie,
        ItemBoost,  //1 = 1.33, 2 = 1.5, 3 = 2

        Defocus,
        Sunder,
        Enervate,
        Hamper,
        Cooldown,
        Disorient,

        /*
        Toughness,
        Limiter,
        Critical,
        Sturdy,
        Unpiercable,
        ExactDamageKill,
        NoMiracle,
        */
    }

    //public byte duration; //Effect.INFINITE_DURATION = infinite
    public sbyte duration;

    public sbyte potency;
    public EffectType effect;
    public int casterID;
    public const int NULL_CASTERID = int.MinValue;
    public const sbyte INFINITE_DURATION = 127;
    public const sbyte INFINITE_POTENCY = 127;
    public const sbyte MAX_NORMAL_DURATION = 126;

    //first static effect ID is 0, no need to make that a constant though
    public const int FIRST_STATUS_ID = (int)EffectType.Berserk;
    public const int FIRST_BUFF_ID = (int)EffectType.AttackUp;
    public const int FIRST_TOKEN_ID = (int)EffectType.Focus;

    //these are bidirectional, so you can't get more of the debuff than the cap
    //To do: way to increase this
    public const int FOCUS_CAP = 6;
    public const int ABSORB_CAP = 6;
    public const int BURST_CAP = 99;    //ehh there's no realistic way to make this too broken (?)

    public const int ILLUMINATE_CAP = 3;

    public Effect(EffectType effect, sbyte potency, sbyte duration, int casterID)
    {
        if ((potency < 0) ^ (duration < 0))
        {
            EffectType ne = InvertEffectType(effect);
            if (ne != EffectType.Default)
            {
                effect = ne;
                potency = (sbyte)(Mathf.Abs(potency));
                duration = (sbyte)(Mathf.Abs(duration));

                if (effect == EffectType.Sticky)
                {
                    duration = (sbyte)(potency + 1);
                    potency = 1;
                }
                if (effect == EffectType.ItemBoost)
                {
                    potency = (sbyte)(duration - 1);
                    duration = INFINITE_DURATION;
                }
                if (effect == Effect.EffectType.MistWall)
                {
                    potency = 1;
                }
                if (effect == Effect.EffectType.AstralWall)
                {
                    //make up some number I guess
                    potency = 15;
                }
                if (effect == Effect.EffectType.Inverted)
                {
                    potency = 1;
                }
                if (effect == Effect.EffectType.TimeStop)
                {
                    potency = 1;
                }
            }
        }
        else if (potency < 0 && duration < 0)
        {
            potency = (sbyte)(-potency);
            duration = (sbyte)(-duration);
        }

        this.effect = effect;
        this.potency = potency;
        this.duration = duration;
        this.casterID = casterID;
    }
    public Effect(EffectType effect, sbyte potency, sbyte duration)
    {
        casterID = NULL_CASTERID; //default

        if ((potency < 0) ^ (duration < 0))
        {
            EffectType ne = InvertEffectType(effect);
            if (ne != EffectType.Default)
            {
                effect = ne;
                potency = (sbyte)(Mathf.Abs(potency));
                duration = (sbyte)(Mathf.Abs(duration));

                if (effect == EffectType.Sticky)
                {
                    duration = (sbyte)(potency + 1);
                    potency = 1;
                }
                if (effect == EffectType.ItemBoost)
                {
                    potency = (sbyte)(duration - 1);
                    duration = INFINITE_DURATION;
                }
                if (effect == Effect.EffectType.MistWall)
                {
                    potency = 1;
                }
                if (effect == Effect.EffectType.AstralWall)
                {
                    //make up some number I guess
                    potency = 15;
                }
                if (effect == Effect.EffectType.Inverted)
                {
                    potency = 1;
                }
                if (effect == Effect.EffectType.TimeStop)
                {
                    potency = 1;
                }
            }
        }
        else if (potency < 0 && duration < 0)
        {
            potency = (sbyte)(-potency);
            duration = (sbyte)(-duration);
        }

        this.effect = effect;
        this.potency = potency;
        this.duration = duration;
    }
    public Effect(EffectType effect, sbyte duration)
    {
        potency = 1;
        casterID = NULL_CASTERID; //default

        if ((potency < 0) ^ (duration < 0))
        {
            EffectType ne = InvertEffectType(effect);
            if (ne != EffectType.Default)
            {
                effect = ne;
                potency = (sbyte)(Mathf.Abs(potency));
                duration = (sbyte)(Mathf.Abs(duration));

                if (effect == EffectType.Sticky)
                {
                    duration = (sbyte)(potency + 1);
                    potency = 1;
                }
                if (effect == EffectType.ItemBoost)
                {
                    potency = (sbyte)(duration - 1);
                    duration = INFINITE_DURATION;
                }
                if (effect == Effect.EffectType.MistWall)
                {
                    potency = 1;
                }
                if (effect == Effect.EffectType.AstralWall)
                {
                    //make up some number I guess
                    potency = 15;
                }
                if (effect == Effect.EffectType.Inverted)
                {
                    potency = 1;
                }
                if (effect == Effect.EffectType.TimeStop)
                {
                    potency = 1;
                }
            }
        }

        this.effect = effect;
        this.duration = duration;
    }
    public Effect(EffectType effect)
    {
        this.effect = effect;
        duration = 1;
        potency = 1;
        casterID = NULL_CASTERID; //default
    }

    public static EffectClass GetEffectClass(EffectType se)
    {
        if ((int)se < FIRST_STATUS_ID)
        {
            return EffectClass.Static;
        }
        else if ((int)se < FIRST_BUFF_ID)
        {
            return EffectClass.Status;
        }
        else if ((int)se < FIRST_TOKEN_ID)
        {
            return EffectClass.BuffDebuff;
        }
        else
        {
            return EffectClass.Token;
        }
    }

    public static bool IsBlockableDebuff(EffectType se)
    {
        //balance decisions: should I make every debuff blockable?
        //I upgraded a lot of the worse debuffs to become ailments, but you can block those still
        return ((int)se <= (int)EffectType.Sticky && (int)se >= (int)EffectType.AttackDown) || ((int)se <= (int)EffectType.Disorient && (int)se >= (int)EffectType.Defocus);
    }


    public static bool IsCleanseable(EffectType se, bool curePermanents = true)
    {
        if ((int)se < FIRST_STATUS_ID)
        {
            //nope
            if (!curePermanents)
            {
                return false;
            }

            //ehh, getting rid of escalating buffs will be a thing?
            return ((int)se <= (int)EffectType.MaxSEBoost && (int)se >= (int)EffectType.AttackBoost);
        }
        else if ((int)se < FIRST_BUFF_ID)
        {
            //statuses
            return false;
        }
        else if ((int)se < FIRST_TOKEN_ID)
        {
            return ((int)se <= (int)EffectType.AstralWall && (int)se >= (int)EffectType.AttackUp);
        }
        else
        {
            //make ItemBoost, Miracle, Freebie not cleanseable (would be really punishing)
            //also make BonusTurns not either
            if (se == EffectType.BonusTurns)
            {
                return false;
            }
            return ((int)se < (int)EffectType.Miracle && (int)se >= (int)EffectType.Focus);
        }
    }
    public static bool IsCurable(EffectType se, bool curePermanents = true)
    {
        if ((int)se < FIRST_STATUS_ID)
        {
            //nope
            if (!curePermanents)
            {
                return false;
            }

            return (int)se <= (int)EffectType.MaxSEReduction && (int)se >= (int)EffectType.AttackReduction;
        }
        else if ((int)se < FIRST_BUFF_ID)
        {
            return true;
        }
        else if ((int)se < FIRST_TOKEN_ID)
        {
            return (int)se <= (int)EffectType.BoltSprout && (int)se >= (int)EffectType.AttackDown;
        }
        else
        {
            //also make BonusTurns not either
            if (se == EffectType.Cooldown)
            {
                return false;
            }
            return ((int)se <= (int)EffectType.Disorient && (int)se >= (int)EffectType.Defocus);
        }
    }
    public static bool IsDeathCurable(EffectType se)
    {
        if ((int)se < FIRST_STATUS_ID)
        {
            return false;   //Permanents are permanent (I am going to be extremely stringent with anti-permanent things, even on enemies)
            //return (int)se <= (int)EffectType.MaxSEReduction && (int)se >= (int)EffectType.AttackReduction;
        }
        else if ((int)se < FIRST_BUFF_ID)
        {
            return true;
        }
        else if ((int)se < FIRST_TOKEN_ID)
        {
            return (int)se <= (int)EffectType.BoltSprout || (int)se >= (int)EffectType.DamageOverTime;
        }
        else
        {
            return true;    //note: includes cooldown (But it probably isn't very exploitable)
        }
    }
    public static EffectType InvertEffectType(EffectType se)
    {
        //Returns Default if the effect doesn't have a pair
        //note: uses the ailment -> other ailment thing
        //note 2: includes the permanent effects, exclude them specifically if you don't want them inverted
        EffectType[] conflictingStatuses =
        {
            EffectType.AttackBoost,
            EffectType.AttackReduction,

            EffectType.DefenseBoost,
            EffectType.DefenseReduction,

            EffectType.EnduranceBoost,
            EffectType.EnduranceReduction,

            EffectType.AgilityBoost,
            EffectType.AgilityReduction,

            EffectType.MaxHPBoost,
            EffectType.MaxHPReduction,

            EffectType.MaxEPBoost,
            EffectType.MaxEPReduction,

            EffectType.MaxSEBoost,
            EffectType.MaxSEReduction,

            //advanced ailments
            EffectType.Soulbleed,
            EffectType.Ethereal,

            EffectType.Sunflame,
            EffectType.Illuminate,

            EffectType.Brittle,
            EffectType.MistWall,

            EffectType.Inverted,
            EffectType.AstralWall,

            EffectType.Dread,
            EffectType.CounterFlare,

            EffectType.ArcDischarge,
            EffectType.Supercharge,

            EffectType.TimeStop,
            EffectType.QuantumShield,

            EffectType.Exhausted,
            EffectType.Soften,

            EffectType.AttackUp,
            EffectType.AttackDown,

            EffectType.DefenseUp,
            EffectType.DefenseDown,

            EffectType.EnduranceUp,
            EffectType.EnduranceDown,

            EffectType.AgilityUp,
            EffectType.AgilityDown,

            EffectType.FlowUp,
            EffectType.FlowDown,

            EffectType.HealthRegen,
            EffectType.HealthLoss,

            EffectType.EnergyRegen,
            EffectType.EnergyLoss,

            EffectType.SoulRegen,
            EffectType.SoulLoss,

            EffectType.Focus,
            EffectType.Defocus,

            EffectType.Absorb,
            EffectType.Sunder,

            EffectType.Burst,
            EffectType.Enervate,

            EffectType.Haste,
            EffectType.Hamper,

            EffectType.Awaken,
            EffectType.Disorient,

            EffectType.BonusTurns,
            EffectType.Cooldown,

            EffectType.Freeze,
            EffectType.Poison,

            EffectType.Dizzy,
            EffectType.Paralyze,

            EffectType.Sleep,
            EffectType.Berserk,

            EffectType.ParryAura,
            EffectType.DrainSprout,

            EffectType.BolsterAura,
            EffectType.BoltSprout,

            EffectType.Sticky,
            EffectType.ItemBoost,   //Requires special handling
        };

        for (int i = 0; i < conflictingStatuses.Length; i++)
        {
            if (se == conflictingStatuses[i])
            {
                return conflictingStatuses[((i / 2) * 2) + ((i + 1) % 2)];
            }
        }

        return EffectType.Default;
    }

    public override string ToString()
    {
        return effect.ToString() + (potency != 1 ? (" " + potency) : "") + " for " + duration + " turns." + ((casterID > int.MinValue) ? (" Caused by id " + casterID) : "");
    }

    public Effect Copy()
    {
        return new Effect(effect, potency, duration, casterID);
    }

    //Multiplicative boosting
    //Boosts by duration (or by potency if effect is already permanent)
    //Sub 1 boosts still force potency >= 1, duration >= 1
    public Effect BoostCopy(float boost)
    {
        Effect output = Copy();

        if (output.duration == INFINITE_DURATION)
        {
            //Boost by potency
            if (output.potency * boost > MAX_NORMAL_DURATION)
            {
                output = new Effect(output.effect, MAX_NORMAL_DURATION, output.duration);
            }
            else
            {
                if (output.potency * boost < 1)
                {
                    output = new Effect(output.effect, 1, output.duration);
                } else
                {
                    output = new Effect(output.effect, (sbyte)(output.potency * boost), output.duration);
                }                

                if (boost == 0)
                {
                    output = new Effect(output.effect, 0, output.duration);
                }
            }
        }
        else
        {
            //Boost by duration
            if (output.duration * boost > MAX_NORMAL_DURATION)
            {
                output = new Effect(output.effect, output.potency, MAX_NORMAL_DURATION);
            }
            else
            {
                if (output.duration * boost < 1)
                {
                    output = new Effect(output.effect, output.potency, 1);
                }
                else
                {
                    output = new Effect(output.effect, output.potency, (sbyte)(output.duration * boost));
                }                

                if (boost == 0)
                {
                    output = new Effect(output.effect, output.potency, 0);
                }
            }
        }

        return output;
    }
}

[System.Serializable]
public class DefenseTableEntry
{
    public BattleHelper.DamageType type;
    public int amount;
    public const int IMMUNITY_CONSTANT = 10000000; //equal = immune, greater than = damage actually heals target
    //We can safely assume that nothing will do 10 million damage

    public DefenseTableEntry(BattleHelper.DamageType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}

[System.Serializable]
public class CharmEffect
{
    public enum CharmType
    {
        None = 0,
        Attack,
        Defense,
        Fortune,
    }

    public CharmType charmType;
    public int charges;         //for fortune this is a potency tracker
    public int duration;        //decremented each turn, activates at 0 (or for fortune, disappears at 0)
    public int resetDuration;

    public CharmEffect(CharmType p_charmType, int p_charges, int p_duration, int p_resetDuration = 0)
    {
        charmType = p_charmType;
        charges = p_charges;
        duration = p_duration;
        resetDuration = p_resetDuration;
    }

    public float GetFortunePower()
    {
        return GetFortunePower(charges);
    }
    public static float GetFortunePower(int power)
    {
        switch (power)
        {
            case 1:
                return 1.6666f; //note: calculation uses ceil so ...67 would turn 3 into 6 instead of 5
            case 2:
                return 2;
            case 3:
                return 3;
            default:
                if (power > 0)
                {
                    return 3 * GetFortunePower(power - 3);
                } else
                {
                    return 1 + (2f / (4 - power)) - 0.0001f;
                }
        }
    }
    public static string GetName(CharmType c)
    {
        return c.ToString();
    }
    public string GetMenuString()
    {
        if (charmType == CharmType.Fortune)
        {
            float fPower = GetFortunePower(charges);
            int intPower = (int)fPower;
            string powerText = "x" + (intPower == fPower ? intPower : fPower);
            return "(" + duration + ") " + charmType.ToString() + " " + powerText;
        }
        else
        {
            return "(" + charges + ") " + charmType.ToString() + " in " + duration + " turn" + (duration != 1 ? "s" : "") + " (1/" + resetDuration + ")";

        }
    }
    public override string ToString()
    {
        /*
        if (charmType == CharmType.Fortune)
        {
            float fPower = GetFortunePower(charges);
            int intPower = (int)fPower;
            string powerText = "x" + (intPower == fPower ? intPower : fPower);
            return "(" + duration + ") " + charmType.ToString() + " " + powerText;
        } else
        {
            return "(" + charges + ") " + charmType.ToString() + " in " + duration + " turn" + (duration != 1 ? "s" : "") + " (1/" + resetDuration + ")";

        }
        */
        return charmType + "|" + charges + "|" + duration + "|" + resetDuration;
    }
    public static CharmEffect Parse(string input)
    {
        string[] split = input.Split('|');

        CharmType charmType = CharmType.None;

        Enum.TryParse(split[0], out charmType);

        int charges = 0;
        if (split.Length > 1)
        {
            int.TryParse(split[1], out charges);
        }

        int duration = 0;
        if (split.Length > 2)
        {
            int.TryParse(split[2], out duration);
        }

        int resetDuration = 0;
        if (split.Length > 3)
        {
            int.TryParse(split[3], out resetDuration);
        }

        CharmEffect output = new CharmEffect(charmType, charges, duration, resetDuration);

        return output;
    }
    public static List<CharmEffect> ParseList(string input)
    {
        string[] split = input.Split(",");

        List<CharmEffect> output = new List<CharmEffect>();

        //Empty
        if ((split.Length < 1) || (split.Length == 1 && split[0].Length < 1))
        {
            return output;
        }

        for (int i = 0; i < split.Count(); i++)
        {
            CharmEffect newEffect = Parse(split[i]);

            bool doadd = true;
            if (newEffect.charmType == CharmType.Fortune && (newEffect.charges <= 0 || newEffect.duration <= 0))
            {
                doadd = false;
            }
            if ((newEffect.charmType == CharmType.Attack || newEffect.charmType == CharmType.Defense) && newEffect.charges <= 0)
            {
                doadd = false;
            }

            if (doadd)
            {
                output.Add(newEffect);
            }
        }

        return output;
    }
    public static string ListToString(List<CharmEffect> input)
    {
        string output = "";
        for (int i = 0; i < input.Count; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            output += input[i];
        }
        return output;
    }
}

[System.Serializable]
public class InnEffect
{
    public enum InnType
    {
        None = 0,
        Health, //healing
        Energy, //energizing
        Absorb, //rocky
        Stamina,    //windy
        Burst,  //seaside
        Focus,  //steamy
        Ethereal,   //hidden
        Immunity,   //clear
        BonusTurn,  //swift
        ItemBoost,  //luxury
        Soul,       //calming
        Freebie,    //supreme
    }

    public InnType innType;
    public int charges;

    //better for making the starting charge counts consistent
    public InnEffect(InnType p_innType)
    {
        innType = p_innType;
        switch (p_innType)
        {
            case InnType.Health:
            case InnType.Energy:
                charges = 4;        //+1 because you effectively can't use the first one (since you start that battle at full stats probably)
                break;
            case InnType.Absorb:
            case InnType.Stamina:
            case InnType.Burst:
            case InnType.Focus:
                charges = 3;
                break;
            case InnType.Ethereal:
                charges = 2;
                break;
            case InnType.Immunity:
                charges = 3;
                break;
            case InnType.BonusTurn:
            case InnType.ItemBoost:
                charges = 2;
                break;
            case InnType.Soul:
                charges = 4;        //+1 because you effectively can't use the first one (since you start that battle at full stats probably)
                break;
            case InnType.Freebie:
                charges = 2;
                break;
        }
    }
    public InnEffect(InnType p_innType, int p_charges)
    {
        innType = p_innType;
        charges = p_charges;
    }

    public static string GetName(InnType i)
    {
        return i.ToString() + " Rest";
    }

    public override string ToString()
    {
        return innType + "|" + charges;
    }
    public static InnEffect Parse(string input)
    {
        string[] split = input.Split('|');

        InnType innType = InnType.None;

        Enum.TryParse(split[0], out innType);

        int charges = 0;
        if (split.Length > 1)
        {
            int.TryParse(split[1], out charges);
        }

        InnEffect output = new InnEffect(innType, charges);

        return output;
    }
    public static List<InnEffect> ParseList(string input)
    {
        string[] split = input.Split(",");

        List<InnEffect> output = new List<InnEffect>();

        //Empty
        if ((split.Length < 1) || (split.Length == 1 && split[0].Length < 1))
        {
            return output;
        }

        for (int i = 0; i < split.Count(); i++)
        {
            InnEffect newEffect = Parse(split[i]);

            if (newEffect.innType != InnType.None && newEffect.charges > 0)
            {
                output.Add(newEffect);
            }
        }

        return output;
    }
    public static string ListToString(List<InnEffect> input)
    {
        string output = "";
        for (int i = 0; i < input.Count; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            output += input[i];
        }
        return output;
    }
}

//static helper class for battle methods
public static class BattleHelper
{
    [Flags]
    public enum DamageType      //note: applies effects on your damage (these can be combined, except normal is simply "devoid" of the other types) (Note: there won't be 33 or more types so no need for ulong)
    {
        Default =   1 << 31, //Not a real damage type, don't use

        //Do not reorder (there might be something that relies on this specific order)
        Normal =    0,       //basic attacks (most real world melee weapons are here)
        Light =     1,       //Applies a bonus to damage at high hp (Applies before effect damage boosts)
        Water =     1 << 1, //Applies bonus on enemies that were not hit last turn
        Air =       1 << 2, //Piercing damage
        Dark =      1 << 3,       //Applies a bonus to damage at low hp (applies the same as Light)
        Fire =      1 << 4, //Applies a bonus based on damage taken this turn and last turn (target damage taken)
        Earth =     1 << 5, //Applies bonus on enemies that were hit this turn

        Prismatic = 1 << 6, //takes lowest defense of all of them (Air pierces so this inherits that property)
        Void =      1 << 7, //Same as prismatic but different effect

        //don't use these, use the advanced formulas instead (*or just use these for the outcome check)
        Spectral =  Light | Water | Air,    
        Soul =      Light | Earth | Fire,
        Astral =    Water | Earth | Dark,
        Aether =    Dark | Fire | Air,

        Vitrum = Light | Water | Earth,
        Quantum = Water | Air | Dark,
        Phlox = Dark | Fire | Earth,
        Plasma = Light | Fire | Air,

        PierceEthereal = Light | Prismatic | Void,

        Everything = Light | Dark | Fire | Water | Earth | Air  //Don't
    }
    [Flags]
    public enum DamageProperties : ulong //properties of your attacks (separate from type) (Future note: I can't make more than 64 flags in one enum because ulong is the largest type (next step after this is to add a second properties enum, or some other supplemental data))
    {
        None =                  0,
        PierceDef =             1L,
        PiercesEthereal =       1L << 1,  //can hit ethereal (but light damage is hardcoded to do this too)
        HitsWhileDizzy =        1L << 2,  //can hit while you are dizzy
        HitsWhileDizzyWeak =    1L << 3,  //can hit while you are dizzy (at 50% power)
        Combo =                 1L << 4,  //not present on final hit
        //Counter =               1L << 5,  //counters cannot be countered themselves (causes the correct events to happen)
        ContactHazard =         1L << 6,  //Does not apply any buffs, can't be countered, invokes hiddenHurt event
        NoCharge =              1L << 7,  //does not use charge (sets the buffered charge remover thing though)
        Static =                1L << 8,  //Ignores all attack modification, does not trigger buffered charge remover
        LightDarkAbsolute =     1L << 9, //uses 60 instead of maxhp in the damage formula
        IgnoreElementCalculation =      1L << 10, //ignores the special damage calculation from fire, earth, water, light, dark
        Unblockable =           1L << 11, //Can't be blocked
        NonLethal =             1L << 12, //can't reduce you to 0 hp
        RemoveMaxHP =           1L << 13, //reduces your max hp
        MinOne =                1L << 14, //minimum damage is 1
        PierceOne =             1L << 15,    
        SAC_BlockFail =         1L << 16, //special action command (fail) (aka: force fail block) (Note that some stuff uses a separate method that ignores this so use special flags to avoid problems)
        SAC_BlockSuccess =      1L << 17, //special action command (success) (aka: force succeed in blocking)
        PlusOneOnBuff =         1L << 18, //plus one if you have a damage buff active
        AdvancedElementCalc =   1L << 19, //use advanced element formulas (special)
        HPDrainOneToOne =       1L << 20, //1 to 1 hp drain
        HPDrainTwoToOne =       1L << 21, //2 to 1 hp drain
        HPDrainOneMax =         1L << 22, //1 hp drain
        EPDrainOneToOne =       1L << 23, //1 to 1 ep drain
        EPDrainTwoToOne =       1L << 24, //2 to 1 ep drain
        EPDrainOneMax =         1L << 25, //1 ep drain
        EPLossOneToOne =        1L << 26, //1 to 1 ep lost
        EPLossTwoToOne =        1L << 27, //2 to 1 ep lost
        EPLossOneMax =          1L << 28, //1 ep lost
        Hardcode =              1L << 29, //Number is static (passes through the calculation without change)
        CountBuffs =            1L << 30, //bonus damage based on target buffs
        CountDebuffs =          1L << 31, //bonus damage based on target debuffs (status counts as 2)
        CountDebuffs2 =         1L << 32, //bonus damage based on target debuffs * 2 (status counts as 4)
        PreserveFocus =         1L << 33, //focus not lost after attacking
        Knockback =             1L << 34, //a knockback attack (doesn't knock back if no enemies behind or too heavy) (hits you into behind, damage to behind is 1/2 of last damage taken)
        MetaKnockback =         1L << 35, //Same as knockback but the knockback hits are themselves more knockback hits    
        AC_Premature =          1L << 36, //something to encapsulate the special case of slip slash (since it does some damage before the action command actually ends)
        AC_Success =            1L << 37, //show action command thing
        AC_SuccessStall =       1L << 38, //show action command thing but don't increment counter (used for spread damage things)
        //note: AC_Failure does not exist since you can probably infer it from missing the last 3 and also not having the static / hardcode flags
        Block_Negate =          1L << 39, //0 attack power if blocked
        Block_Resist =          1L << 40, //1/2 attack power if blocked

        SuppressHitParticles =   1L << 41,  //damage type hit effects don't appear

        Item =                  1L << 42,   //Item damage
        TrueMinOne =            1L << 43,   //No matter what, deals minimum of 1 damage
        StatusExploit =         1L << 44,   //status exploit (certain damage types are stronger on certain statuses)
        SoftTouch =             1L << 45,   //does not wake up sleep or break ice
        NightmareStrike =       1L << 46,   //+2 damage to sleeping targets that will wake up
        Aggravate =             1L << 47,   //+2 damage to berserk
        Icebreaker =            1L << 48,   //+2 to ice

        StandardContactHazard = ContactHazard | Static | Unblockable,

        //checking stuff
        NoChargeProperties = Hardcode | Static | NoCharge,
        NoCounterProperties = ContactHazard | Static,
    }

    public enum ActionCommandText
    {
        Nice,
        Good,
        Great,
        Perfect,
        Miss,
        Immune,
        Absorb
    }

    //Note: mostly just used to make the state sprites
    //Tied to the state sprite sheet
    public enum EntityState
    {
        /* Entity properties (things that end up in entityProperties, usually set immediately and never changed) */
        Toughness = 0,
        Limiter,
        Critical,
        Sturdy,
        Unpiercable,
        ExactDamageKill,

        /* Dynamic entity properties (things that end up in entityProperties, more likely to change) */
        NoMiracle,
        NoStatus,
        NoDebuff,
        StateStunned,
        StateCharge,
        StateDefensive,
        StateRage,
        StateCounter,
        StateCounterHeavy,
        StateContactHazard,
        StateContactHazardHeavy,
        CharacterMark,
        PositionMark,

        Charge,
        Countdown,

        /* Charms (handled globally) */
        CharmAttack,
        CharmDefense,
        CharmFortune,

        /* Rest effects (handled globally) */
        HealthRest,
        EnergyRest,
        AbsorbRest,
        StaminaRest,
        BurstRest,
        FocusRest,
        EtherealRest,
        ImmunityRest,
        BonusTurnRest,
        ItemBoostRest,
        SoulRest,
        FreebieRest,

        /* Enviro effects (handled globally) */
        ElectricWind,
        SeasideAir,
        ScaldingHeat,
        DarkFog,
        FrigidBreeze,
        AetherHunger,
        DigestiveAcid,
        AcidFlow,
        SacredGrove,
        IonizedSand,
        WhiteoutBlizzard,
        VoidShadow,
        CounterWave,
        ScaldingMagma,
        TrialOfSimplicity,
        TrialOfHaste,
        TrialOfResolve,
        TrialOfCunning,
        TrialOfPatience,
        TrialOfStrength,
    }

    //order unimportant, just that there is a one to one correspondence between the names in the state enum
    public enum EnvironmentalEffect
    {
        None,
        ElectricWind,
        SeasideAir,
        ScaldingHeat,
        DarkFog,
        FrigidBreeze,
        AetherHunger,
        DigestiveAcid,
        AcidFlow,
        SacredGrove,
        IonizedSand,
        WhiteoutBlizzard,
        VoidShadow,
        CounterWave,
        ScaldingMagma,
        TrialOfSimplicity,
        TrialOfHaste,
        TrialOfResolve,
        TrialOfAmbition,
        TrialOfPatience,
        TrialOfZeal,
    }

    public enum ContactLevel    //contact hazard range (to check if contact hazards should activate)
    {
        Contact = 0,        //touch
        Weapon,         //use weapon (you are still close enough that there may be come hazards that hit you)
        Infinite,       //usually long ranged attacks, contact hazards that still activate on these kinds of attacks are illogical (So this is the "immune to contact hazards" range)
    }
    public enum MultihitReductionFormula    //note that there is no "none" formula since you can just not use the functions that accept these formulas
    {
        ReduceByOne,        //x2 = x - 1
        ReduceByTwo,        //x2 = x - 2
        ReduceThreeFourths, //x2 = x * 0.75
        ReduceTwoThirds,    //x2 = x * 0.66
        ReduceHalf          //x2 = x * 0.5
    }

    //yeah a 2 element enum is really not very good
    //and all my code only really checks for Miss so this could really be a bool
    /*
    public enum AttackOutcome
    {
        Normal,
        Miss,
    }
    */
    [Flags]
    public enum EntityProperties : ulong //(Future note: if I make more than 32 flags, I need to make this a ulong, which means I have to modify all my uint methods)
    {
        None = 0,
        NoTarget =      1uL, //can't be targetted through normal means
        NoCount =       1uL << 1, //not counted when determining live players and enemies
        CountAtZero =   1uL << 2, //Counted for determining battle end if hp = 0 (i.e. battle won't end even if they are at 0. Note that they still have to be in the battle to count)
        Invulnerable =  1uL << 3, //can't be damaged through normal means
        Airborne =      1uL << 4, //"high up (so low moves like melee attacks don't work)"    (*Hardcoded to occur with homepos.y > 1, but can be set manually)
        LowStompable =  1uL << 5, //Can be low stomped
        Grounded =      1uL << 6, //"touching a surface (so earthquake moves will work)"
        Illusory =      1uL << 7, //"all attacks miss" (this is independent from ethereal)
        Unpiercable =   1uL << 8, //defense cannot be pierced
        DebuffImmune =  1uL << 9, //can't be debuffed (may be independent from status immunity)
        Ceiling =       1uL << 10, //fails the topmost check (so you can't target them with jumps)
        NoVoidCrush =   1uL << 11, //Not void crushable, even if the formula says so
        Toughness =     1uL << 12, //3 damage or less is negated
        Resistant =       1uL << 13, //1 damage max
        Hardened =      1uL << 14, //can only take lethal damage (damage below max hp is negated)
        Sturdy =        1uL << 15, //at max hp, damage capped at max hp - 1
        ExactDamageKill =   1uL << 16, //will heal if not exact damage killed
        NoMiracle =     1uL << 17, //can't get miracle effect (flag set when Miracle activates)
        GetEffectsAtNoHP = 1uL << 18, //can you receive effects at 0 hp? (only set for special enemies that don't die at 0 hp)
        KeepEffectsAtNoHP = 1uL << 19, //Death will cure curable stuff normally
        NoTattle =      1uL << 20,   //Can't tattle
        ScanHideMoves = 1uL << 21,   //Scan does not reveal moveset
        ScanMovesetMismatch = 1uL << 22, //For enemies whose movesets dont match the data table (The three of these quiet the warning message for the data table not having the right moveset)

        SoftTouch =     1uL << 23,
        DeepSleep =     1uL << 24,
        Glacier =       1uL << 25,

        StateStunned =  1uL << 26,
        StateCharge =   1uL << 27,
        StateDefensive = 1uL << 28,
        StateRage =     1uL << 29,
        StateCounter = 1uL << 30,
        StateCounterHeavy = 1uL << 31,
        StateContactHazard = 1uL << 32,
        StateContactHazardHeavy = 1uL << 33,
        CharacterMark = 1uL << 34,
        PositionMark =  1uL << 35,

        HideHP = 1uL << 36,  //Replaces the hp number with a ?, also replaces the hp bar with an ambiguous thing

        NoShadow = 1uL << 37,

        SuppressMovesetWarning = NoTattle | ScanHideMoves | ScanMovesetMismatch
    }

    public const float AIRBORNE_CUTOFFHEIGHT = 0.8f;
    public const float LOWSTOMPABLE_CUTOFFHEIGHT = 1.35f;   //was 1.6 before, but now is 1.35 so that Cactupoles are not low stompable, (But you can still low stomp Burrow Traps and Sundews as they are 1.25 height)

    [Flags]
    public enum BattleProperties : ulong
    {
        CanRetry = 1L,                          //Retrying is possible
        NoFlee = 1L << 1,                       //Can't flee
        FreeFlee = 1L << 2,                     //Flee for 0 stamina
        FleeResetStats = 1L << 3,               //Fleeing does not update playerdata, so your stats are reset to before the battle
        IgnoreFirstStrikeBadges = 1L << 3,      //First strike badges don't do anything

        ScriptedEncounter = CanRetry | NoFlee | IgnoreFirstStrikeBadges     //Bosses, minibosses and other stuff has this
    }

    public enum Event
    {
        Hurt,
        StatusHurt,
        ComboHurt,   //used instead of hurt (part of combo moves) (Avoid making these long animations since they can happen a short time after each other)
        HiddenHurt,  //hurt without animation (similar to combo hurt)
        KnockbackHurt,  //hurt by knockback move
        MetaKnockbackHurt,  //hurt by meta knockback move
        Death,       //a lethal hit triggers both the corresponding hurt and death events (hurt then death)
        StatusDeath, //similar to death
        CureStatus, //when you stop having an effect
        StatusInflicted, //when a status is inflicted (not triggered by other effects, usually accompanied by a Hurt effect beforehand)
        Heal,       //HealHealth triggers this (note: can be in the PostMove phase)
        Revive,     //when HealHealth heals an enemy from 0 to non-zero
        Miss,       //When an attack misses (Note: by convention, this should be called when the attack should have hit, usually only invoked once but ocassionally multiple times)

        //"false" events (used for signalling things, generally only exist as broadcasted events)
        Skill,
        Jump,
        Weapon,
        SoulMove,
        UseItem,
        Tactic,

        PayEnergy,
        Rest,
        Check,
    }
    public enum MoveCurrency
    {
        Energy,
        Soul,
        Stamina,
        Health,
        Coins
    }
    public enum BattleOutcome
    {
        Win,        //you win
        Exit,       //special conditions
        Flee,       //you ran away
        Tie,        //usually considered the same as death, rarely used
        Death       //rip
    }
    public enum EntityID //contains all battle entity IDs, by convention player IDs are negative (but there is no direct effect tied to that?)
    {
        Keru = -3,
        Luna = -2,
        Wilex = -1,
        DebugEntity = 0,
        Leafling,
        Flowerling,
        Shrublet,
        Rootling,
        Sunflower,
        Sunnybud,
        MiracleBloom,
        SunSapling,
        Rockling,
        Honeybud,
        BurrowTrap,
        Sundew,
        VinePlatform,
        Sycamore,
        GiantVine,
        VineThrone,
        MasterOfAutumn,
        Bandit,
        Renegade,
        Sentry,
        Cactupole,
        Sandswimmer,
        DesertMinibossA,
        DesertMinibossB,
        DesertMinibossC,
        DesertBossGuy,
        StormCannon,
        Stormtamer,
        Stormkiller,
        TrainingDummy,
        TrainingDummyBoss,
        TournamentPawn,
        TournamentKnight,
        TournamentBishopA,
        TournamentBishopB,
        TournamentRook,
        TournamentQueenA,
        TournamentQueenB,
        TournamentKing,
        TournamentChampion,
        Slime,
        Slimewalker,
        Slimeworm,
        Slimebloom,
        SirenFish,
        FalseDragonArm,
        FalseDragon,
        AmalgamLeftArm,
        AmalgamRightArm,
        DiscordantAmalgam,
        Blazecrest,
        Embercrest,
        Ashcrest,
        Flametongue,
        Heatwing,
        Lavaswimmer,
        LavaWyvern,
        MetalWyvern,
        EyeSpore,
        SpikeShroom,
        Shrouder,
        HoarderFly,
        Mosquito,
        SporeSpider,
        MoonSpider,
        Shieldwing,
        Honeywing,
        Shimmerwing,
        LumistarVanguard,
        LumistarSoldier,
        LumistarStriker,
        KingIlum,
        TyrantBlade,
        Plateshell,
        Speartongue,
        Chaintail,
        Sawcrest,
        Coiler,
        Drillbeak,
        AetherBoss,
        AetherSuperboss,
        PuffJelly,
        Fluffling,
        CloudJelly,
        CrystalCrab,
        CrystalSlug,
        CrystalClam,
        AuroraWing,
        FinalBoss,
        FinalSuperboss,
        Plaguebud,
        Starfish,
        CursedEye,
        StrangeTendril,
        DrainBud,
        EndOfTable
    }
    public enum DamageEffect
    {
        Damage,
        Heal,
        NegativeHeal,
        Energize,
        DrainEnergy,
        BlockedDamage,
        SuperBlockedDamage,
        UnblockableDamage,
        MaxHPDamage,
        SoftDamage,
        SoulEnergize,
        DrainSoulEnergy,
        Stamina,
        DrainStamina,
        Coins,
        NegativeCoins
    }

    /*
    //ehh this is probably not very useful
    //I can probably just check by effect class if I even keep this
    public enum EffectPopupPriority //when do the popups from getting effects appear?
    {
        Never,          //this is a numerical thing (so like it will display everything above a certain point)
        BuffInflict,    //so these should be roughly in order of rarity (with common stuff near the top since those are more likely to be annoying)
        EffectInflict,  
        EnviroEffects,
        SpecialProperties,
    }
    */

    public const int LOW_HEALTH_THRESHOLD = 5;
    public const int LOW_ENERGY_THRESHOLD = 5;
    //public const int BASE_GUARD_AMOUNT = 1;
    public const int BASE_SUPERGUARD_WINDOW = 6;   //This is a double input (so the input should be a little lenient) (but this still requires you to fulfill the guard window
    public const int BASE_GUARD_WINDOW = 12; //60 fps
    public const int BASE_GUARD_COOLDOWN = 16; //Prevent you from spamming the block button to get it every time

    public static bool GetDamageProperty(ulong i, DamageProperties property)
    {
        //if you check multiple, returns 1 if any are present
        return (i & (ulong)property) != 0;
    }
    public static Vector3 GetDefaultPosition(int id)         //where entities appear by default
    {
        //negative ids are players

        //positive ids
        //To make things more clear on the Y-levels I have it as 10 per line even though I will never use that

        //0-9 are in a line
        //10-19 are above (off ground)
        //20-29 are further above
        //30-39 are even further above
        /*
        if (id < 0)
        {
            //remnant of further out camera
            //return Vector3.left * 2.0f + Vector3.right * 1.5f * id + Vector3.forward * (id + 1) * 0.15f;
            return Vector3.left * 1.25f + Vector3.right * 1.2f * id + Vector3.forward * (id + 1) * 0.15f;
        }
        else
        {
            //was 5 per line but 10 is easier to tell at a glance
            return new Vector3(0.4f, 0.0f, 0.15f) +
                1.2f * Vector3.right * (id % 10) + 1.2f * Vector3.up * (id / 10) - 0.15f * Vector3.forward * (id % 10);
        }
        */

        //newer: make things 80% x ish

        //Special allies: spawn behind you and closer together
        if (id <= -10)
        {
            return Vector3.left * 1.05f + Vector3.right * (0.6f * (id + 10) - 2.5f) + Vector3.forward * (((id + 10) + 1) * 0.15f + 0.6f);
        }


        if (id < 0)
        {
            //remnant of further out camera
            //return Vector3.left * 2.0f + Vector3.right * 1.5f * id + Vector3.forward * (id + 1) * 0.15f;
            return Vector3.left * 1.05f + Vector3.right * 1f * id + Vector3.forward * (id + 1) * 0.15f;
        }
        else
        {
            //was 5 per line but 10 is easier to tell at a glance
            return new Vector3(-0.05f, 0.0f, 0.12f) +
                1.1f * Vector3.right * (id % 10) + 1.1f * Vector3.up * (id / 10) - 0.15f * Vector3.forward * (id % 10);
        }
    }

    public static Func<int, int, int> GetMultihitReductionFormula(MultihitReductionFormula formula)
    {
        switch (formula)
        {
            case MultihitReductionFormula.ReduceByOne:
                return (a, i) => {
                    int k = a;
                    for (int j = 0; j < i; j++)
                    {
                        k = k - 1;
                    }
                    if (k < 1 && a > 0)
                    {
                        k = 1;
                    }
                    return k;
                };
            case MultihitReductionFormula.ReduceByTwo:
                return (a, i) => {
                    int k = a;
                    for (int j = 0; j < i; j++)
                    {
                        k = k - 2;
                    }
                    if (k < 1 && a > 0)
                    {
                        k = 1;
                    }
                    return k;
                };
            case MultihitReductionFormula.ReduceThreeFourths:
                return (a, i) => {
                    int k = a;
                    for (int j = 0; j < i; j++)
                    {
                        k = (k * 3) / 4;
                    }
                    if (k < 1 && a > 0)
                    {
                        k = 1;
                    }
                    return k;
                };
            case MultihitReductionFormula.ReduceTwoThirds:
                return (a, i) => {
                    int k = a;
                    for (int j = 0; j < i; j++)
                    {
                        k = (k * 2) / 3;
                    }
                    if (k < 1 && a > 0)
                    {
                        k = 1;
                    }
                    return k;
                };
            case MultihitReductionFormula.ReduceHalf:
                return (a, i) => {
                    int k = a;
                    for (int j = 0; j < i; j++)
                    {
                        k = k / 2;
                    }
                    if (k < 1 && a > 0)
                    {
                        k = 1;
                    }
                    return k;
                };
            default:
                return null;
        }
    }

    public static string GetCurrencyIcon(MoveCurrency cost)
    {
        switch (cost)
        {
            case MoveCurrency.Energy:
                return "<ep>";
            case MoveCurrency.Soul:
                return "<se>";
            case MoveCurrency.Health:
                return "<hp>";
            case MoveCurrency.Stamina:
                return "<stamina>";
            case MoveCurrency.Coins:
                return "<coin>";
        }

        return "";
    }
}

[System.Serializable]
public class EncounterData
{
    [System.Serializable]
    public class EncounterDataEntry
    {
        public string entid;
        //public BattleHelper.EntityID entid;
        public int posid;
        public bool usePos;
        public Vector3 pos;
        public string bonusdata;

        public EncounterDataEntry(BattleHelper.EntityID entid, int posid)
        {
            this.entid = entid.ToString();
            this.posid = posid;
            usePos = false;
            this.pos = Vector3.zero;
        }

        public EncounterDataEntry(BattleHelper.EntityID entid, int posid, Vector3 pos)
        {
            this.entid = entid.ToString();
            this.posid = posid;
            usePos = true;
            this.pos = pos;
        }

        public BattleHelper.EntityID GetEntityID()
        {
            Enum.TryParse(entid, true, out BattleHelper.EntityID eid);
            if (eid == BattleHelper.EntityID.DebugEntity && !entid.Equals("DebugEntity"))
            {
                Debug.LogError("Could not parse entity id: " + entid);
            }
            return eid;
        }

        public override string ToString()
        {
            return "(" + entid + " " + posid + " " + bonusdata + ")";
        }
    }

    public List<EncounterDataEntry> encounterList;
    //To add later: encounter music and encounter maps
    public MainManager.BattleMapID battleMapName = MainManager.BattleMapID.Test_BattleMap;

    public EncounterData(List<EncounterDataEntry> data)
    {
        encounterList = data.ConvertAll((e) => (e));
    }

    public EncounterData()
    {
        encounterList = new List<EncounterDataEntry>();
    }
    public EncounterData(params EncounterDataEntry[] data)
    {
        encounterList = new List<EncounterDataEntry>(data);
    }
    public EncounterData(params BattleHelper.EntityID[] eids)
    {
        encounterList = new List<EncounterDataEntry>();
        for (int i = 0; i < eids.Length; i++)
        {
            encounterList.Add(new EncounterDataEntry(eids[i],i));
        }
    }

    public static EncounterData GeneratePitEncounter(int floor, float weirdnessFactor = 1)
    {
        string[][] pitEnemyData = MainManager.CSVParse(Resources.Load<TextAsset>("Data/PitEnemyPool").text);


        BattleHelper.EntityID[] idArray = new BattleHelper.EntityID[pitEnemyData.Length];
        int[] levelArray = new int[pitEnemyData.Length - 1];
        for (int i = 1; i < pitEnemyData.Length - 1; i++)
        {
            idArray[i - 1] = Enum.Parse<BattleHelper.EntityID>(pitEnemyData[i][0], true);
            levelArray[i - 1] = int.Parse(pitEnemyData[i][1]);
        }

        //formula breaks since 36 is the max level of the enemies I have?
        if (floor > 124)
        {
            floor = 124;
        }

        float levelNormal = 3 + (30f * floor / 100f); //5 + (25f * floor / 100f);

        //range = +-2

        //Debug.Log(levelNormal);

        List<BattleHelper.EntityID> startEnemies = new List<BattleHelper.EntityID>();
        List<int> startIndices = new List<int>();
        for (int i = 0; i < levelArray.Length; i++)
        {
            if (levelArray[i] < levelNormal + 2 && levelArray[i] > levelNormal - 2)
            {
                startIndices.Add(i);
            }
        }

        //sus static syntax
        int randomIndex = RandomTable<int>.ChooseRandom(startIndices);

        //Now add enemies to a new list
        List<BattleHelper.EntityID> newEnemyList = new List<BattleHelper.EntityID>();
        List<int> newLevelList = new List<int>();

        int levelTotal = (int)(levelNormal * (2.7f + floor / 50f));        

        while (levelTotal > 0)
        {
            int newRandomIndex = 0;
            if (RandomGenerator.Get() < 0.05f * weirdnessFactor)
            {
                //Choose a later enemy
                //Range = +-4 from randomIndex + 16
                newRandomIndex = RandomGenerator.GetIntRange(randomIndex - 12, randomIndex + 20);
            } else if (RandomGenerator.Get() < 0.1f * weirdnessFactor)
            {
                //Choose from a bigger range
                //Range = +-8 from randomIndex
                newRandomIndex = RandomGenerator.GetIntRange(randomIndex - 8, randomIndex + 8);
            }
            else
            {
                //Choose normally
                //Range = +-2 from randomIndex
                newRandomIndex = RandomGenerator.GetIntRange(randomIndex - 2, randomIndex + 3);
            }

            //if out of bounds, choose again
            //(Choose from the 5 enemies at the start of the table)
            if (newRandomIndex < 0)
            {
                newRandomIndex = RandomGenerator.GetIntRange(0, 6);
            }

            //(Choose from the 5 enemies at the end of the table)
            if (newRandomIndex >= levelArray.Length)
            {
                newRandomIndex = RandomGenerator.GetIntRange(levelArray.Length - 5, levelArray.Length);
            }

            //no
            if (idArray[newRandomIndex] == BattleHelper.EntityID.DebugEntity)
            {
                continue;
            }

            //Now add
            newEnemyList.Add(idArray[newRandomIndex]);
            newLevelList.Add(levelArray[newRandomIndex]);
            levelTotal -= levelArray[newRandomIndex];

            if (newEnemyList.Count > 3)
            {
                break;
            }
        }

        if (levelTotal < 0)
        {
            for (int i = 0; i < newEnemyList.Count; i++)
            {
                if (newLevelList[i] < -2 * levelTotal)
                {
                    newLevelList.RemoveAt(i);
                    newEnemyList.RemoveAt(i);
                }
            }
        }

        //swap indices such that the front enemy is the highest level one (with some small margin of error)
        //(Stops that situation where a low level enemy is on the field but a really high level enemy is in the battle behind it)
        for (int i = 1; i < newEnemyList.Count; i++)
        {
            if (newLevelList[i] > newLevelList[0] + 4)
            {
                int swaplevel = newLevelList[0];
                BattleHelper.EntityID swapEID = newEnemyList[0];

                newLevelList[0] = newLevelList[i];
                newEnemyList[0] = newEnemyList[i];

                newLevelList[i] = swaplevel;
                newEnemyList[i] = swapEID;
            }
        }

        //build an encounter with this information
        EncounterData ed = new EncounterData();
        ed.encounterList = new List<EncounterDataEntry>();

        int offset = 0;
        if (newEnemyList.Count < 3)
        {
            offset = 1;
        }
        if (newEnemyList.Count < 2)
        {
            offset = 2;
        }
        for (int i = 0; i < newEnemyList.Count; i++)
        {
            bool airborne = false;

            airborne = (BattleEntityData.GetBattleEntityData(newEnemyList[i]).entityProperties & (ulong)BattleHelper.EntityProperties.Airborne) != 0;

            EncounterDataEntry ede = new EncounterDataEntry(newEnemyList[i], airborne ? 10 + i + offset : i + offset);

            if (newEnemyList[i] == BattleHelper.EntityID.CloudJelly)
            {
                switch (RandomGenerator.GetIntRange(0, 3))
                {
                    case 0:
                        ede.bonusdata = "cloud";
                        break;
                    case 1:
                        ede.bonusdata = "water";
                        break;
                    case 2:
                        ede.bonusdata = "ice";
                        break;
                }
            }

            if (newEnemyList[i] == BattleHelper.EntityID.Sawcrest)
            {
                if (RandomGenerator.Get() < 0.5f)
                {
                    ede.bonusdata = "active";
                }
            }

            ed.encounterList.Add(ede);
        }

        return ed;
    }

    public int GetOverkillLevel()
    {
        int level = 0;

        foreach (var e in encounterList)
        {
            BattleEntityData bed = BattleEntityData.GetBattleEntityData(Enum.Parse<BattleHelper.EntityID>(encounterList[0].entid, true));
            if (bed.level > level)
            {
                level = bed.level;
            }
        }

        return level;
    }

    public float GetEncounterDifficultyLevel()
    {
        float sumlevel = 0;
        foreach (var e in encounterList)
        {
            BattleEntityData bed = BattleEntityData.GetBattleEntityData(Enum.Parse<BattleHelper.EntityID>(encounterList[0].entid, true));
            sumlevel += bed.level;
        }

        return (sumlevel - 12) / 4f;
    }

    public override string ToString()
    {
        string output = "";

        for (int i = 0; i < encounterList.Count; i++)
        {
            output += encounterList[i].ToString();
            if (i < encounterList.Count - 1)
            {
                output += " ";
            }
        }

        return output;
    }
}