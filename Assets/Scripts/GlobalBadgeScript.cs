using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Badge;
using static Item;

public class GlobalBadgeScript : MonoBehaviour
{
    private static GlobalBadgeScript instance;
    public static GlobalBadgeScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GlobalBadgeScript>(); //this should work
                if (instance == null)
                {
                    GameObject b = new GameObject("GlobalBadgeScript");
                    GlobalBadgeScript c = b.AddComponent<GlobalBadgeScript>();
                    instance = c;
                    instance.transform.parent = MainManager.Instance.transform;
                }
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    public BadgeDataEntry[] badgeDataTable;
    string[][] badgeText;

    public void LoadBadgeDataTable()
    {
        string[][] badgeDataRaw = MainManager.CSVParse(Resources.Load<TextAsset>("Data/BadgeData").text);
        badgeDataTable = new BadgeDataEntry[badgeDataRaw.Length - 1];
        for (int i = 1; i < badgeDataTable.Length; i++)
        {
            BadgeDataEntry? temp = BadgeDataEntry.ParseBadgeDataEntry(badgeDataRaw[i], (Badge.BadgeType)(i));
            badgeDataTable[i - 1] = temp.GetValueOrDefault();
        }
        //badge table has to be indexed by 1 less
    }
    public void UnloadBadgeDataTable()
    {
        badgeDataTable = null;
    }
    public void LoadBadgeText()
    {
        badgeText = MainManager.GetAllTextFromFile("DialogueText/BadgeText");    //MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/ItemText").text);
    }
    public void UnloadBadgeText()
    {
        badgeText = null;
    }

    public string[][] GetTextFile()
    {
        return badgeText;
    }

    public string GetBadgeName(BadgeType i)
    {
        if (badgeText == null)
        {
            LoadBadgeText();
        }

        return badgeText[(int)(i)][1];
    }

    public string GetBadgeDescription(BadgeType i)
    {
        if (badgeText == null)
        {
            LoadBadgeText();
        }

        string output = "";

        int length = badgeText[(int)(i)].Length;
        output += badgeText[(int)(i)][2]; //FormattedString.InsertEscapeSequences(badgeText[(int)(i)][2]);
        if (length > 3 && badgeText[(int)(i)][3].Length > 0)
        {
            output += " <descriptionnoticecolor>(" + badgeText[(int)(i)][3] + ")</descriptionnoticecolor>";
        }
        //Debug.Log(output);
        //Debug.Log((int)itemText[(int)(i)][5][0]);

        return output;
    }

    public string GetBadgeText(BadgeType i, int index)
    {
        return badgeText[(int)i][index];
    }

    public static Sprite GetBadgeSpriteFromText(string badge)
    {
        BadgeType badgeType;

        Enum.TryParse(badge, out badgeType);

        return GetBadgeSprite(badgeType);
    }

    public static Sprite GetBadgeSprite(BadgeType badgeType)
    {
        if ((int)badgeType >= (int)BadgeType.EndOfTable || (int)badgeType <= 0)
        {
            return MainManager.Instance.badgeSprites[MainManager.Instance.badgeSprites.Length - 1];
        }
        return MainManager.Instance.badgeSprites[(int)(badgeType) - 1];
    }
}

public struct BadgeDataEntry
{
    //one line of badge data
    public int cost;    //Cost (theoretically can use a smaller data type but eh)
    public bool singleOrParty;  //false = single, true = party
    public Badge.BadgeType copy;    //none = does not copy (Whenever you check GetBadgeCount, you will get all the badges with copy value set to the target type, and also the type itself)
    public bool badgeSwap;

    public static BadgeDataEntry? ParseBadgeDataEntry(string[] entry, Badge.BadgeType i = (Badge.BadgeType)(-1))
    {
        BadgeDataEntry bde = new BadgeDataEntry();

        //validation
        string check = entry[0];
        if (!check.Equals(i.ToString()))
        {
            Debug.LogWarning("[Badge Parsing] Data table has a mismatch: " + i + " is reading from " + entry[0]);
        }

        //cost
        int temp = 0;
        if (entry.Length > 1)
        {
            int.TryParse(entry[1], out temp);
            bde.cost = temp;
        }

        //singleorparty
        bool tempBool = false;
        if (entry.Length > 2)
        {
            bool.TryParse(entry[2], out tempBool);
            bde.singleOrParty = tempBool;
        }

        //copy field
        //use anim
        Badge.BadgeType tempBadge = (Badge.BadgeType)(-1);
        if (entry.Length > 3)
        {
            if (entry[3].Length == 0)
            {
                bde.copy = Badge.BadgeType.None;
            }
            else
            {
                if (Enum.TryParse(entry[3], true, out tempBadge))
                {
                }
                else
                {
                    Debug.LogError("[Badge Parsing] " + i + " Can't parse copy badge \"" + tempBadge + "\"");
                }
                bde.copy = tempBadge;
            }
        }

        //badge swap
        tempBool = false;
        if (entry.Length > 4)
        {
            bool.TryParse(entry[4], out tempBool);
            bde.badgeSwap = tempBool;
        }

        return bde;
    }
}

[System.Serializable]
public struct Badge
{
    public enum BadgeType
    {
        None = 0,
        BadgeSwap,
        SuperCurse,
        UltraCurse,
        MegaCurse,
        Focus,
        FocusB,
        MultiStomp,
        ElectroStomp,
        Taunt,
        ParalyzeStomp,
        FlameStomp,
        DoubleStomp,
        Overstomp,
        SmartStomp,
        TeamQuake,
        MultiSlash,
        MultiSlashB,
        SlipSlash,
        PoisonSlash,
        PreciseSlash,
        SwordDischarge,
        SwordDance,
        BoomerangSlash,
        Aetherize,
        DarkSlash,
        FlameBat,
        AstralWall,
        Brace,
        BraceB,
        DashThrough,
        FlipKick,
        FluffHeal,
        SleepStomp,
        MeteorStomp,
        UnderStrike,
        IronStomp,
        ElementalStomp,
        TeamThrow,
        PowerSmash,
        PowerSmashB,
        DazzleSmash,
        HammerThrow,
        BreakerSmash,
        FlameSmash,
        MomentumSmash,
        QuakeSmash,
        Illuminate,
        LightSmash,
        HammerBeat,
        MistWall,
        StrangeEgg,
        StatusRelay,
        VoraciousEater,
        MultiBite,
        QuickBite,
        VoidBite,
        ItemBoost,
        ItemSaver,
        HPPlus,
        HPPlusB,
        HPPlusC,
        HPPlusD,
        EPPlus,
        EPPlusB,
        EPPlusC,
        EPPlusD,
        SEPlus,
        SEPlusB,
        LastCounter,
        LastChance,
        LastBurst,
        RiskyPower,
        RiskyShield,
        RiskyEndurance,
        ProtectivePower,
        ProtectiveShield,
        ProtectiveEndurance,
        PerfectPower,
        PerfectShield,
        PerfectEndurance,
        FirstPower,
        FirstShield,
        FirstEndurance,
        EnergeticPower,
        EnergyShield,
        EnergyBurst,
        AgilePower,
        AgileShield,
        AgileEndurance,
        SpiritPower,
        SpiritShield,
        SpiritFlow,
        GoldenPower,
        GoldenShield,
        Refund,
        DarkPower,
        DarkShield,
        DarkEndurance,
        NullPower,
        NullShield,
        NullEndurance,
        AttackBoost,
        AttackBoostB,
        DefenseBoost,
        DefenseBoostB,
        EnduranceBoost,
        EnduranceBoostB,
        AgilityBoost,
        AgilityBoostB,
        VitalEnergy,
        SoulfulEnergy,
        StaminaEnergy,
        GoldenEnergy,
        EnergeticSoul,
        HealthyExercise,
        SoftPower,
        MetalPower,
        AttackFormation,
        DefenseFormation,
        MagicClock,
        PowerGear,
        ShieldGear,
        EnergyGear,
        PowerMomentum,
        TenaciousStrikes,
        HealthGrowth,
        EnergyGrowth,
        HealthRegen,
        HealthRegenB,
        EnergyRegen,
        EnergyRegenB,
        SoulRegen,
        HealthBalance,
        EffectBoost,
        StatusBoost,
        StatusConversion,
        StatusCatalyst,
        StatusExploit,
        Instinct,
        DizzyMomentum,
        Trance,
        DizzyAgility,
        HypnoStrike,
        UnsteadyStance,
        Capacitor,
        Resistor,
        Generator,
        Inductor,
        Conductor,
        Overexert,
        Sleepwalk,
        DeepSleep,
        SweetDreams,
        LightSleep,
        NightmareStrike,
        QuickNap,
        RagesPower,
        RageShield,
        RageRally,
        UndyingRage,
        Aggravate,
        VengefulRage,
        ToxicStrength,
        ToxicShield,
        ToxicEnergy,
        ToxicResistance,
        NerveStrike,
        WeakStomach,
        FrostEdge,
        IceShell,
        Glacier,
        Preservative,
        Icebreaker,
        Supercooled,
        HealthSight,
        DefenseSight,
        StatusSight,
        CharmSight,
        PowerSight,
        ItemSight,
        BattleSight,
        SoftTouch,
        PerfectVictory,
        DecisiveVictory,
        StatusResist,
        StatusResistB,
        CharmBoost,
        RibbonPower,
        RibbonSwap,
        LongRest,
        HealthSteal,
        EnergySteal,
        RiskyRush,
        ProtectiveRush,
        PerfectFocus,
        HeadStart,
        RiskyStart,
        VictoryHeal,
        VictorySurge,
        AgilityRush,
        StaminaRebound,
        MoneyBoost,
        DarkConcentration,
        DepletionBurst,
        RevivalFlame,
        DeathSwap,
        SkillSwap,
        FocusRecycling,
        AbsorbRecycling,
        BurstRecycling,
        StaminaRecycling,
        WeatherShield,
        Overkill,
        SmartAmbush,
        AutoStrike,
        InviteDanger,
        ItemFinder,
        DodgeStep,
        StealthStep,
        HasteStep,

        EndOfTable
    }

    public BadgeType type;
    public int badgeCount;  //number that counts up based on how many badges you take
    public int bonusData;

    public Badge(BadgeType b, int p_badgeCount = 0, int p_bonusData = 0)
    {
        type = b;
        badgeCount = p_badgeCount;
        bonusData = p_bonusData;
    }

    public static BadgeDataEntry GetBadgeDataEntry(BadgeType b)
    {
        if ((int)b >= (int)BadgeType.EndOfTable || (int)b <= 0)
        {
            return new BadgeDataEntry();
        }
        return GlobalBadgeScript.Instance.badgeDataTable[(int)b - 1];
    }
    public static BadgeDataEntry GetBadgeDataEntry(Badge b)
    {
        return GetBadgeDataEntry(b.type);
    }

    public static string GetName(BadgeType b)
    {
        return GlobalBadgeScript.Instance.GetBadgeName(b); // b.ToString();
    }
    public static string GetName(Badge b)
    {
        if (MainManager.Instance.Cheat_SeePickupCounts)
        {
            return GetName(b.type) + " " + b.badgeCount;
        }
        else
        {
            return GetName(b.type);
        }
    }
    public static string GetDescription(BadgeType b)
    {
        return GlobalBadgeScript.Instance.GetBadgeDescription(b); //b.ToString() + " description";
    }
    public static string GetDescription(Badge b)
    {
        return GetDescription(b.type);
    }
    public static string GetSpriteString(BadgeType i)
    {
        return "<badgesprite," + i.ToString() + ">";
    }
    public static string GetSpriteString(Badge i)
    {
        return GetSpriteString(i.type);
    }
    public static int GetSPCost(BadgeType b)
    {
        BadgeDataEntry bde = GetBadgeDataEntry(b);

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Envy))
        {
            if (bde.cost > 0)
            {
                return 1;
            }
        }

        return bde.cost;
    }
    public static int GetSPCost(Badge b)
    {
        return GetSPCost(b.type);
    }

    //Inverse of toString
    public static Badge Parse(string input)
    {
        Badge output = new Badge();

        string[] split = input.Split("|");

        if (split.Length > 0)
        {
            //this looks a bit sus
            Enum.TryParse(split[0], true, out output.type);
        }

        if (output.type == BadgeType.None)
        {
            Debug.LogWarning("[Badge Parsing] Failed to parse a badge type: " + split[0]);
        }

        if (split.Length > 1)
        {
            int.TryParse(split[1], out output.badgeCount);
        }

        if (split.Length > 2)
        {
            int.TryParse(split[2], out output.bonusData);
        }

        return output;
    }
    public override string ToString()
    {
        if (badgeCount == 0 && bonusData == 0)
        {
            return type.ToString();
        }

        if (bonusData == 0)
        {
            return type.ToString() + "|" + badgeCount;
        }

        return type.ToString() + "|" + badgeCount + "|" + bonusData;
    }

    public static List<Badge> ParseList(string input)
    {
        //Debug.Log(input);
        string[] split = input.Replace("\r","").Split(",");
        List<Badge> output = new List<Badge>();

        //Empty
        if ((split.Length < 1) || (split.Length == 1 && split[0].Length < 1))
        {
            return output;
        }

        for (int i = 0; i < split.Count(); i++)
        {
            output.Add(Parse(split[i]));
        }

        return output;
    }
    public static string ListToString(List<Badge> list)
    {
        string output = "";
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            output += list[i].ToString();
        }
        return output;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Badge)) return false;

        Badge b = (Badge)obj;

        if (b.type != type)
        {
            return false;
        }

        if (b.badgeCount != badgeCount)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(type, badgeCount);
    }
}