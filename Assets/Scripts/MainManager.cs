using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Item;
using static MainManager;


//Thing that has a tattle
//glued to various objects
//Something you can use 
public interface ITattleable
{
    //To do later: convert this to my correct system
    string GetTattle();
}

//A class that holds the data for the player characters
[System.Serializable]
public class PlayerData
{
    [System.Serializable]
    public class PlayerDataEntry
    {
        public BattleHelper.EntityID entityID;
        public int hp;
        public int maxHP;
        //public float attackPower;
        //public List<Status> statuses; //cleared outside of battle
        public List<Badge> equippedBadges;
        public int jumpLevel;
        public int weaponLevel;
        public Ribbon ribbon;
        //you can derive the other stats with calculations and constants


        public int itemsUsed;
        public int cumulativeDamageDealt;
        public int cumulativeDamageTaken;
        public int maxDamagePerTurn;
        public int maxDamageSingleHit;

        public int turnsInFront;
        public float timeInFront;


        public SpriteID GetSpriteID()
        {
            switch (entityID)
            {
                case BattleHelper.EntityID.Wilex:
                    return SpriteID.Wilex;
                case BattleHelper.EntityID.Luna:
                    return SpriteID.Luna;
            }
            return (SpriteID)entityID;
        }

        public PlayerDataEntry(BattleHelper.EntityID eid)
        {           
            entityID = eid;
            maxHP = GetBaseHP(eid);
            hp = maxHP;
            //attackPower = GetBaseAttack(eid);
            //statuses = new List<Status>();
            equippedBadges = new List<Badge>();
        }
        public PlayerDataEntry(BattleHelper.EntityID eid, List<LevelUpgrade> upgrades)
        {
            int hpUpgrades = new List<LevelUpgrade>(upgrades).FindAll((e) => (e == LevelUpgrade.HP)).Count;
            int epUpgrades = new List<LevelUpgrade>(upgrades).FindAll((e) => (e == LevelUpgrade.EP)).Count;

            entityID = eid;
            maxHP = GetMaxHP(eid,hpUpgrades);
            hp = maxHP;
            //attackPower = GetBaseAttack(eid);
            //statuses = new List<Status>();
            equippedBadges = new List<Badge>();
        }
        public PlayerDataEntry(BattleHelper.EntityID eid, int p_maxHp)// int p_attackPower)
        {
            entityID = eid;
            maxHP = p_maxHp;
            hp = maxHP;
            //attackPower = p_attackPower;
            //statuses = new List<Status>();
            equippedBadges = new List<Badge>();
        }

        public string GetName()
        {
            return GetName(entityID);
        }

        public PlayerDataEntry Copy()
        {
            //Shallow ish clone
            //Badge list isn't
            PlayerDataEntry copied = (PlayerDataEntry)MemberwiseClone();
            
            //copied.hp = hp;
            //copied.maxHP = maxHP;
            copied.equippedBadges = new List<Badge>(equippedBadges);
            //copied.jumpLevel = jumpLevel;
            //copied.weaponLevel = weaponLevel;
            //copied.ribbon = ribbon;

            return copied;
        }
        public override string ToString()
        {
            string output = entityID.ToString();

            output += ",";
            output += hp;
            output += ",";
            output += maxHP;
            output += ",";
            output += jumpLevel;
            output += ",";
            output += weaponLevel;
            output += ",";
            output += ribbon.ToString();
            output += ",";
            output += itemsUsed;
            output += ",";
            output += cumulativeDamageDealt;
            output += ",";
            output += cumulativeDamageTaken;
            output += ",";
            output += maxDamagePerTurn;
            output += ",";
            output += maxDamageSingleHit;
            output += ",";
            output += turnsInFront;
            output += ",";
            output += timeInFront;
            output += "\n";
            output += Badge.ListToString(equippedBadges);       //equipped list appears at the bottom because it's a list

            return output;
        }
        public static PlayerDataEntry Parse(string[] input)    //Pass in 2 lines
        {
            if (input.Length != 2)
            {
                Debug.LogWarning("[Player Data Parsing] Wrong number of lines passed to Parse: " + input.Length);
            }

            string[] split = input[0].Split(",");

            //entity ID
            BattleHelper.EntityID entityID = BattleHelper.EntityID.DebugEntity;
            if (split.Length > 0)
            {
                Enum.TryParse(split[0], out entityID);
            }
            PlayerDataEntry output = new PlayerDataEntry(entityID);

            //hp
            int temp = 0;
            if (split.Length > 1)
            {
                int.TryParse(split[1], out temp);
                output.hp = temp;
            }

            temp = 0;
            if (split.Length > 2)
            {
                int.TryParse(split[2], out temp);
                output.maxHP = temp;
            }

            temp = 0;
            if (split.Length > 3)
            {
                int.TryParse(split[3], out temp);
                output.jumpLevel = temp;
            }

            temp = 0;
            if (split.Length > 4)
            {
                int.TryParse(split[4], out temp);
                output.weaponLevel = temp;
            }

            Ribbon r = new Ribbon(Ribbon.RibbonType.None);
            if (split.Length > 5)
            {
                r = Ribbon.Parse(split[5]);
                output.ribbon = r;
            }

            temp = 0;
            if (split.Length > 6)
            {
                int.TryParse(split[6], out temp);
                output.itemsUsed = temp;
            }

            temp = 0;
            if (split.Length > 7)
            {
                int.TryParse(split[7], out temp);
                output.cumulativeDamageDealt = temp;
            }

            temp = 0;
            if (split.Length > 8)
            {
                int.TryParse(split[8], out temp);
                output.cumulativeDamageTaken = temp;
            }

            temp = 0;
            if (split.Length > 9)
            {
                int.TryParse(split[9], out temp);
                output.maxDamagePerTurn = temp;
            }

            temp = 0;
            if (split.Length > 10)
            {
                int.TryParse(split[10], out temp);
                output.maxDamageSingleHit = temp;
            }

            temp = 0;
            if (split.Length > 11)
            {
                int.TryParse(split[11], out temp);
                output.turnsInFront = temp;
            }

            float tempF = 0f;
            if (split.Length > 12)
            {
                float.TryParse(split[12], out tempF);
                output.timeInFront = tempF;
            }

            List<Badge> badgeList = Badge.ParseList(input[1]);

            output.equippedBadges = badgeList;

            return output;
        }

        public static int GetBaseHP(BattleHelper.EntityID eid)
        {
            switch (eid)
            {
                case BattleHelper.EntityID.Wilex:
                    return 8;
                case BattleHelper.EntityID.Luna:
                    return 12;
            }
            return 10;
        }

        public static int GetDangerHP(BattleHelper.EntityID eid)
        {
            return GetBaseHP(eid) / 2;
        }
        public static int GetStompDamage(BattleHelper.EntityID eid, int jumpLevel)
        {
            switch (eid)
            {
                case BattleHelper.EntityID.Wilex:
                    switch (jumpLevel)
                    {
                        case 0:
                            return 2;
                        case 1:
                            return 3;
                        case 2:
                            return 5;
                    }
                    return 2;
                case BattleHelper.EntityID.Luna:
                    switch (jumpLevel)
                    {
                        case 0:
                            return 2;
                        case 1:
                            return 4;
                        case 2:
                            return 6;
                    }
                    return 2;
            }
            return 1;
        }
        public static int GetWeaponDamage(BattleHelper.EntityID eid, int weaponLevel)
        {
            switch (eid)
            {
                case BattleHelper.EntityID.Wilex:
                    switch (weaponLevel)
                    {
                        case 0:
                            return 3;
                        case 1:
                            return 5;
                        case 2:
                            return 7;
                    }
                    return 2;
                case BattleHelper.EntityID.Luna:
                    switch (weaponLevel)
                    {
                        case 0:
                            return 3;
                        case 1:
                            return 5;
                        case 2:
                            return 7;
                    }
                    return 2;
            }
            return 1;
        }
        public static float GetBaseAgilityDivisor(BattleHelper.EntityID eid)
        {
            //agility = max EP / (divisor)
            //note: ceiled

            //some considerations:
            //you should always be a bit hamstrung in terms of stamina
            //but the stamina max is pretty high so you can still get a bit too much sometimes
            //the offset should be pretty large so you can't just spend the whole battle with too much stamina
            //Note that the 50% bonus to the front character gives you a lot of stamina so low agility is fine

            //4/3 ratio I think

            //Numbers should be something that divides easily with 6 ish (the "period" of the agility increases should be something less than or equal to 4)
            //though larger numbers make that impossible ish

            //other values tried
            //7.5 and 10
            //  Too much agility in this case?
            //  (The "front gets 50% more" bonus is actually really big)
            //12 and 16 (with +0)
            //  1 agility is really low considering how expensive Multislash and Power Smash are
            //  Also makes the first EP upgrade too impactful (1 to 2 agility is a massive jump)
            //12 and 16 (with +6.001)
            //  0.001 changes the rounding for 30 ep so that it becomes 4 and 3 instead of 3 and 3
            //  Current

            switch (eid)
            {
                case BattleHelper.EntityID.Wilex:
                    return 12f;
                case BattleHelper.EntityID.Luna:
                    return 16f;
            }
            return 1;
        }
        public int GetAgility(int maxEP)
        {
            return Mathf.CeilToInt((maxEP + 7f) / GetBaseAgilityDivisor(entityID));
        }

        public static string GetName(BattleHelper.EntityID eid)
        {
            return eid.ToString();
        }

        public static int GetMaxHP(BattleHelper.EntityID eid, int upgradeCount)
        {
            //Upgrade amount = 1/2 base HP
            int hp = GetBaseHP(eid);
            hp = (int)(hp * (1f + upgradeCount / 2f));

            if (hp < 1)
            {
                return 1;
            }

            return hp;
        }

        public bool BadgeEquipped(Badge.BadgeType b)
        {
            for (int i = 0; i < equippedBadges.Count; i++)
            {
                if (equippedBadges[i].type == b)
                {
                    return true;
                }
            }
            return false;
        }


        public void UpdateMaxDamageDealt(int value)
        {
            if (value > maxDamagePerTurn)
            {
                maxDamagePerTurn = value;
            }
        }
        public void UpdateMaxDamageDealtSingle(int value)
        {
            if (value > maxDamageSingleHit)
            {
                maxDamageSingleHit = value;
            }
        }
    }

    public enum LevelUpgrade
    {
        HP,
        EP,
        SP
    }

    public const int MAX_LEVEL = 25;
    public const int MAX_LEVEL_EXTENDED = 31;
    public const int MAX_UPGRADES = 8;  //(upgrades per stat)
    public const int MAX_UPGRADES_EXTENDED = 10;

    //bases
    public const int BASE_MAX_ITEM_INVENTORY = 10;
    public const int BASE_MAX_STORAGE_SIZE = 32;

    public const int MAX_MONEY = 999;

    public int level = 1;
    public int astralTokens = 0;  //"unused" upgrades, can be converted back to level ups (Should equal number of downgrades)
    public List<LevelUpgrade> upgrades; //get 24 upgrades
    public List<LevelUpgrade> downgrades; //get up to 30 downgrades
    
    public int ep;
    public int maxEP;
    //public int internalMaxEP;     //maxEP is calculated by your upgrade count so I don't need this (likewise with HP)

    public int se;
    public int maxSE;   //May become different from maxSP with badges

    public int sp;  //used for badges
    public int usedSP;
    //public int maxSP; //used for badges

    public int exp;
    public int coins;

    public int shards;
    public int cumulativeShards;    //The thing that appears in the stats screen

    public int maxInventorySize = 10;
    public int maxStorageSize = 32;

    public int focusCap = 6;
    public int absorbCap = 6;
    public int burstCap = 6;


    //A few globally tracked stats (some are used for other things)
    public int itemCounter; //Each time you get a new item, increment by 1 (No need to do this for Badges or Ribbons because you can never get rid of them)
    //0 is treated as no value (overwrite with the right one later)

    public int itemsUsed = 0;

    public int cumulativeBattleTurns = 0;
    public int cumulativeDamageDealt;
    public int cumulativeDamageTaken;
    public int maxDamagePerTurn;
    public int maxDamageSingleHit;
    public int totalBattles;
    public int battlesWon;
    public int battlesFled;
    public int battlesLost;   //note: have to set this in a special way to save it to the save file (save it to the current file)


    public List<Item> itemInventory;
    public List<Item> storageInventory;
    public List<KeyItem> keyInventory;
    public List<Badge> badgeInventory;
    public List<Badge> equippedBadges;  //badges on individual characters will show up here as well as the individual badge equipped list
    public List<Badge> partyEquippedBadges; //badges equipped on party
    public List<Ribbon> ribbonInventory;    //each character only has 1 ribbon equipped

    public List<CharmEffect> charmEffects;  //only 1 of each type since it would get wacky otherwise (power charm = attack/defense)
    public List<InnEffect> innEffects;  //generally only 1 but why not (2 or more at the same time would be pretty OP)

    public List<SpriteID> bonusFollowers;

    public List<PlayerDataEntry> party;

    //(Remnants of time when I had more than 2 characters)
    //public List<PlayerDataEntry> hiddenParty;
    //public List<PlayerDataEntry> fullParty;

    //Bringing it back to handle the case where you control only one character
    public List<PlayerDataEntry> hiddenParty;


    public PlayerDataEntry GetPlayerDataEntry(BattleHelper.EntityID eid)
    {
        for (int i = 0; i < party.Count; i++)
        {
            if (eid == party[i].entityID)
            {
                return party[i];
            }
        }

        return null;
    }

    public PlayerData Copy()
    {
        //Copies everything but I need to split off all the pointers so things aren't referenced
        PlayerData copied = (PlayerData)MemberwiseClone();

        copied.party = new List<PlayerDataEntry>();
        for (int i = 0; i < party.Count; i++)
        {
            copied.party.Add(party[i].Copy());
        }
        copied.hiddenParty = new List<PlayerDataEntry>();
        for (int i = 0; i < hiddenParty.Count; i++)
        {
            copied.hiddenParty.Add(hiddenParty[i].Copy());
        }
        copied.upgrades = new List<LevelUpgrade>(upgrades);
        copied.downgrades = new List<LevelUpgrade>(downgrades);

        copied.itemInventory = new List<Item>(itemInventory);
        copied.storageInventory = new List<Item>(storageInventory);
        copied.keyInventory = new List<KeyItem>(keyInventory);
        copied.badgeInventory = new List<Badge>(badgeInventory);
        copied.equippedBadges = new List<Badge>(equippedBadges);
        copied.partyEquippedBadges = new List<Badge>(partyEquippedBadges);
        copied.ribbonInventory = new List<Ribbon>(ribbonInventory);

        copied.charmEffects = new List<CharmEffect>(charmEffects);
        for (int i = 0; i < copied.charmEffects.Count; i++)
        {
            copied.charmEffects[i] = new CharmEffect(copied.charmEffects[i].charmType, copied.charmEffects[i].charges, copied.charmEffects[i].duration, copied.charmEffects[i].resetDuration);
        }
        copied.innEffects = new List<InnEffect>(innEffects);
        for (int i = 0; i < copied.innEffects.Count; i++)
        {
            copied.innEffects[i] = new InnEffect(copied.innEffects[i].innType, copied.innEffects[i].charges);
        }

        if (bonusFollowers != null)
        {
            copied.bonusFollowers = new List<SpriteID>(bonusFollowers);
        }

        return copied;
    }

    public List<PlayerDataEntry> GetSortedParty()
    {
        List<PlayerDataEntry> output = new List<PlayerDataEntry>();
        foreach (PlayerDataEntry pde in party)
        {
            output.Add(pde);
        }
        output.Sort((a, b) => -(a.entityID - b.entityID));
        return output;
    }
    public void SwitchOrder()
    {
        PlayerDataEntry pde = party[0];
        party.Remove(pde);
        party.Add(pde);
    }

    public void UpdateMaxDamageDealt(int value)
    {
        if (value > maxDamagePerTurn)
        {
            maxDamagePerTurn = value;
        }
    }
    public void UpdateMaxDamageDealtSingle(int value)
    {
        if (value > maxDamageSingleHit)
        {
            maxDamageSingleHit = value;
        }
    }

    public int GetMaxHP(BattleHelper.EntityID eid)
    {
        int hpUpgrades = 0;
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i] == LevelUpgrade.HP)
            {
                hpUpgrades++;
            }
        }

        for (int i = 0; i < downgrades.Count; i++)
        {
            if (downgrades[i] == LevelUpgrade.HP)
            {
                hpUpgrades--;
            }
        }

        int hpPlus = 0;
        /*
        if (BadgeEquipped(Badge.BadgeType.HPPlus, eid))
        {
            hpPlus++;
        }
        if (BadgeEquipped(Badge.BadgeType.HPPlusB, eid))
        {
            hpPlus++;
        }
        if (BadgeEquipped(Badge.BadgeType.HPPlusC, eid))
        {
            hpPlus++;
        }
        if (BadgeEquipped(Badge.BadgeType.HPPlusD, eid))
        {
            hpPlus++;
        }
        */

        hpPlus += BadgeEquippedCount(Badge.BadgeType.HPPlus, eid);


        if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Greed))
        {
            if ((hpUpgrades + hpPlus) > 0)
            {
                return PlayerDataEntry.GetMaxHP(eid, -1);
            }
            else
            {
                return PlayerDataEntry.GetMaxHP(eid, (hpUpgrades + hpPlus) - 1);
            }
        }

        return PlayerDataEntry.GetMaxHP(eid, hpUpgrades + hpPlus);
    }
    public int GetMaxEP()
    {
        int epUpgrades = 0;
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i] == LevelUpgrade.EP)
            {
                epUpgrades++;
            }
        }

        for (int i = 0; i < downgrades.Count; i++)
        {
            if (downgrades[i] == LevelUpgrade.EP)
            {
                epUpgrades--;
            }
        }

        int epPlus = 0;

        /*
        if (BadgeEquipped(Badge.BadgeType.EPPlus))
        {
            epPlus++;
        }
        if (BadgeEquipped(Badge.BadgeType.EPPlusB))
        {
            epPlus++;
        }
        if (BadgeEquipped(Badge.BadgeType.EPPlusC))
        {
            epPlus++;
        }
        if (BadgeEquipped(Badge.BadgeType.EPPlusD))
        {
            epPlus++;
        }
        */
        epPlus += BadgeEquippedCount(Badge.BadgeType.EPPlus);

        return GetMaxEP(epUpgrades + epPlus);
    }
    public int GetMaxSP()
    {
        int spUpgrades = 0;
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i] == LevelUpgrade.SP)
            {
                spUpgrades++;
            }
        }

        for (int i = 0; i < downgrades.Count; i++)
        {
            if (downgrades[i] == LevelUpgrade.SP)
            {
                spUpgrades--;
            }
        }

        if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Envy))
        {
            return 6 + (int)(spUpgrades * 0.5f);
        }

        return GetMaxSP(spUpgrades);
    }

    public void UpdateMaxStats()
    {
        for (int i = 0; i < party.Count; i++)
        {
            party[i].maxHP = GetMaxHP(party[i].entityID);

            if (party[i].hp > party[i].maxHP)
            {
                party[i].hp = party[i].maxHP;
            }
        }
        maxEP = GetMaxEP();
        sp = GetMaxSP();
        usedSP = CalculateUsedSP();

        if (usedSP > sp && !MainManager.Instance.Cheat_BadgeAnarchy)
        {
            equippedBadges = new List<Badge>();
            partyEquippedBadges = new List<Badge>();
            for (int i = 0; i < party.Count; i++)
            {
                party[i].equippedBadges = new List<Badge>();
            }
        }

        maxSE = GetMaxSE();

        if (ep > maxEP)
        {
            ep = maxEP;
        }

        if (se > maxSE)
        {
            se = maxSE;
        }
    }

    public void FullHeal()
    {
        UpdateMaxStats();
        for (int i = 0; i < party.Count; i++)
        {
            party[i].hp = party[i].maxHP;
        }

        ep = maxEP;
        //sp = maxSP;
        usedSP = CalculateUsedSP();
        se = maxSE;
    }

    public void HealHealth(int hp)
    {
        UpdateMaxStats();
        for (int i = 0; i < party.Count; i++)
        {
            party[i].hp += hp;
            if (party[i].hp < 1)
            {
                party[i].hp = 1;
            }
            if (party[i].hp > party[i].maxHP)
            {
                party[i].hp = party[i].maxHP;
            }
        }
    }
    public void HealEnergy(int ep)
    {
        UpdateMaxStats();
        this.ep += ep;
        if (this.ep > maxEP)
        {
            this.ep = maxEP;
        }
    }
    public void HealSoul(int se)
    {
        UpdateMaxStats();
        this.se += se;
        if (this.se > maxSE)
        {
            this.se = maxSE;
        }
    }
    public void AddXP(int xp)
    {
        if (level == GetMaxLevel())
        {
            return;
        }

        exp += xp;
        if (exp > 99)
        {
            exp = 99;
        }
    }

    public float GetHealthPercentage()
    {
        UpdateMaxStats();
        float h = 0;
        for (int i = 0; i < party.Count; i++)
        {
            h += (0.0f + party[i].hp) / party[i].maxHP;
        }
        return h / party.Count;
    }
    public float GetEnergyPercentage()
    {
        UpdateMaxStats();
        return (0.0f + ep) / maxEP;
    }
    public float GetSoulEnergyPercentage()
    {
        UpdateMaxStats();
        return (0.0f + se) / maxSE;
    }
    public float GetStatPercentage()
    {
        return (GetHealthPercentage() + GetEnergyPercentage() + GetSoulEnergyPercentage()) / 3;
    }

    public bool AtMaxStats()
    {
        for (int i = 0; i < party.Count; i++)
        {
            if (party[i].hp < party[i].maxHP)
            {
                return false;
            }
        }

        if (ep < maxEP)
        {
            return false;
        }

        if (se < maxSE)
        {
            return false;
        }

        return true;
    }

    public int GetMaxSE()
    {
        int sePlus = BadgeEquippedCount(Badge.BadgeType.SEPlus);

        return GetMaxSP() + 6 * sePlus;
    }
    public int GetMaxSE(int upgrades)
    {
        int sePlus = BadgeEquippedCount(Badge.BadgeType.SEPlus);

        return GetMaxSP(upgrades) + 6 * sePlus;
    }

    public static int GetMaxEP(int upgradeCount)
    {
        //Upgrade amount = 1/2 base EP
        int ep = 12;
        ep = (int)(ep * (1f + upgradeCount / 2f));

        return ep;
    }
    public static int GetMaxSP(int upgradeCount)
    {
        if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Envy))
        {
            return 6 + (int)(upgradeCount * 0.5f);
        }
        return 12 + upgradeCount * 6;
    }

    public static int GetMaxLevel()
    {
        return MAX_LEVEL;
    }
    public static int GetMaxUpgrades()
    {
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Greed))
        {
            return (int)(MAX_UPGRADES * 1.5f);  //note that health is not upgradeable
        }
        return MAX_UPGRADES;
    }

    public void Add(BattleHelper.EntityID eid)
    {
        party.Add(new PlayerDataEntry(eid));
    }
    public bool Remove(BattleHelper.EntityID eid)
    {
        if (party.Find((e) => (e.entityID == eid)) == null)
        {
            return false;
        }
        return party.Remove(party.Find((e) => (e.entityID == eid)));
    }
    public void TransferToHiddenParty(BattleHelper.EntityID eid)    //used for parts of the story where one character goes away (but I still need to preserve the data correctly)
    {
        PlayerData.PlayerDataEntry pde = GetPlayerDataEntry(eid);
        if (pde == null)
        {
            return;
        }

        //equipped badges need to make sense
        equippedBadges.RemoveAll((e) => pde.equippedBadges.Contains(e));

        hiddenParty.Add(pde);
        party.Remove(pde);
        UpdateMaxStats();
    }
    public void TransferFromHiddenParty(BattleHelper.EntityID eid)
    {
        PlayerData.PlayerDataEntry pde = hiddenParty.Find((e) => (e.entityID == eid));
        if (pde == null)
        {
            return;
        }
        //ensure that the equipped badges make sense

        //also make the badgeequipped thing work
        for (int i = 0; i < party.Count; i++)
        {
            for (int j = 0; j < pde.equippedBadges.Count; j++)
            {
                bool a = equippedBadges.Contains(pde.equippedBadges[j]);
                if (a)
                {
                    pde.equippedBadges.RemoveAt(j);
                    j--;
                    continue;
                } else
                {
                    equippedBadges.Add(pde.equippedBadges[j]);
                }
            }

            //same idea for ribbons
            if (pde.ribbon.Equals(party[i].ribbon))
            {
                //note: if party[i] has None equipped this code runs but doesn't change anything
                pde.ribbon = new Ribbon(Ribbon.RibbonType.None);
            }
        }

        hiddenParty.Remove(pde);
        party.Add(pde);
        UpdateMaxStats();
    }

    //how much SP used by badges?
    public int CalculateUsedSP()
    {
        int output = 0;
        for (int i = 0; i < equippedBadges.Count; i++)
        {
            output += Badge.GetSPCost(equippedBadges[i]);
        }

        return output;
    }

    public void AddBadge(Badge b, bool resort = true)
    {
        b.badgeCount = badgeInventory.Count + 1;
        badgeInventory.Add(b);
        if (resort)
        {
            badgeInventory.Sort((a, b) =>
            {
                int k = (int)a.type - (int)b.type;
                if (k != 0)
                {
                    return k;
                }

                return a.badgeCount - b.badgeCount;
            });
        }
    }

    public void AddRibbon(Ribbon b, bool resort = true)
    {
        b.ribbonCount = ribbonInventory.Count + 1;
        ribbonInventory.Add(b);
        if (resort)
        {
            ribbonInventory.Sort((a, b) =>
            {
                int k = (int)a.type - (int)b.type;
                if (k != 0)
                {
                    return k;
                }

                return a.ribbonCount - b.ribbonCount;
            });
        }
    }
    public void RemoveRibbon(Ribbon b)
    {
        ribbonInventory.Remove(b);
        for (int i = 0; i < party.Count; i++)
        {
            if (party[i].ribbon.Equals(b))
            {
                party[i].ribbon = new Ribbon(Ribbon.RibbonType.None);
            }
        }
    }

    public Ribbon GetVisualRibbon(BattleHelper.EntityID eid)
    {
        PlayerDataEntry pde = GetPlayerDataEntry(eid);

        if (pde == null)
        {
            return default;
        }

        if (pde.ribbon.type == Ribbon.RibbonType.MimicRibbon)
        {
            Ribbon mimicRibbon = pde.ribbon;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i].entityID != eid && (party[i].ribbon.type != Ribbon.RibbonType.None))
                {
                    mimicRibbon = party[i].ribbon;
                }
            }
            return mimicRibbon;
        }

        //Debug.Log(eid + " is wearing " + pde.ribbon);
        return pde.ribbon;
    }

    //rainbow wildcard is if the Rainbow ribbon acts as a wildcard in this case
    //note: also applies Mimic Ribbon (you can explicitly check for Mimic Ribbon but that only checks the case where it doesn't mimic anything)
    public bool GetRibbonEquipped(Ribbon.RibbonType r, BattleHelper.EntityID eid, bool rainbowWildcard = false)
    {
        PlayerDataEntry pde = null;
        for (int i = 0; i < party.Count; i++)
        {
            if (party[i].entityID == eid)
            {
                pde = party[i];
            }
        }

        if (pde == null)
        {
            return false;
        }

        Ribbon.RibbonType rt = GetVisualRibbon(eid).type;
        if (rainbowWildcard && rt == Ribbon.RibbonType.RainbowRibbon)
        {
            return true;
        }

        //Debug.Log("Check " + rt + " vs " + r);
        return rt == r;
    }

    public int GetMaxInventorySize()
    {
        if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Gluttony))
        {
            return maxInventorySize * 2 + itemInventory.FindAll((e) => (e.modifier == Item.ItemModifier.Void)).Count;
        }
        return maxInventorySize + itemInventory.FindAll((e) => (e.modifier == Item.ItemModifier.Void)).Count;
    }
    
    public int GetMaxStorageInventorySize()
    {
        if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Gluttony))
        {
            return maxStorageSize * 2 + storageInventory.FindAll((e) => (e.modifier == Item.ItemModifier.Void)).Count;
        }
        return maxStorageSize + storageInventory.FindAll((e) => (e.modifier == Item.ItemModifier.Void)).Count;
    }

    //try to add an item (return false if you have too many to add one)
    public bool AddItem(Item item)
    {
        if (itemInventory.Count >= GetMaxInventorySize() && item.modifier != ItemModifier.Void)
        {
            return false;
        }
        else
        {
            if (item.itemCount == 0)
            {
                itemCounter++;
                item.itemCount = itemCounter;
            }
            itemInventory.Insert(0, item);
            //itemInventory.Add(item);
            return true;
        }
    }
    public bool InsertItem(int index, Item item)
    {
        if (itemInventory.Count >= GetMaxInventorySize() && item.modifier != ItemModifier.Void)
        {
            return false;
        }
        else
        {
            if (item.itemCount == 0)
            {
                itemCounter++;
                item.itemCount = itemCounter;
            }
            itemInventory.Insert(index, item);
            return true;
        }
    }

    public void AddKeyItem(KeyItem keyItem)
    {
        keyInventory.Add(keyItem);
    }

    public void AddKeyItemStacking(KeyItem keyItem)
    {
        for (int i = 0; i < keyInventory.Count; i++)
        {
            if (keyInventory[i].type == keyItem.type)
            {
                keyInventory[i] = new KeyItem(keyItem.type, keyInventory[i].bonusData + keyItem.bonusData);
                return;
            }
        }

        AddKeyItem(keyItem);
    }
    public int CountKeyItemStacking(KeyItem.KeyItemType kit)
    {
        int count = 0;

        for (int i = 0; i < keyInventory.Count; i++)
        {
            if (keyInventory[i].type == kit)
            {
                count += keyInventory[i].bonusData;
            }
        }

        return count;
    }
    public bool RemoveKeyItemStacking(KeyItem.KeyItemType kit, int count = 1)
    {
        if (CountKeyItemStacking(kit) < count)
        {
            return false;
        }

        int c = count;

        for (int i = 0; i < keyInventory.Count; i++)
        {
            if (keyInventory[i].type == kit)
            {
                if (keyInventory[i].bonusData <= c)
                {
                    c -= keyInventory[i].bonusData;
                    keyInventory.RemoveAt(i);
                    i--;
                    continue;
                }
                else
                {
                    keyInventory[i] = new KeyItem(kit, keyInventory[i].bonusData - c);
                    c -= keyInventory[i].bonusData;
                }
            }
        }

        return true;
    }
    public int CountAllOfType(Item.ItemType i)
    {
        int output = itemInventory.FindAll((e) => (e.type == i)).Count;
        return output;
    }

    public bool HasItem(Item.ItemType t)
    {
        for (int i = 0; i < itemInventory.Count; i++)
        {
            if (itemInventory[i].type == t)
            {
                return true;
            }
        }
        return false;
    }
    public bool HasItem(Item t)
    {
        for (int i = 0; i < itemInventory.Count; i++)
        {
            if (itemInventory[i].Equals(t))
            {
                return true;
            }
        }
        return false;
    }
    public bool HasKeyItem(KeyItem.KeyItemType t)
    {
        for (int i = 0; i < keyInventory.Count; i++)
        {
            if (keyInventory[i].type == t)
            {
                return true;
            }
        }
        return false;
    }
    public bool RemoveKeyItem(KeyItem.KeyItemType t)
    {
        for (int i = 0; i < keyInventory.Count; i++)
        {
            if (keyInventory[i].type == t)
            {
                keyInventory.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    public bool HasBadge(Badge.BadgeType b)
    {
        for (int i = 0; i < badgeInventory.Count; i++)
        {
            if (badgeInventory[i].type == b)
            {
                return true;
            }
        }
        return false;
    }
    public bool RemoveBadge(Badge b)
    {
        bool exists = badgeInventory.Remove(b);
        equippedBadges.Remove(b);
        partyEquippedBadges.Remove(b);
        for (int i = 0; i < party.Count; i++)
        {
            party[i].equippedBadges.Remove(b);
        }

        usedSP = CalculateUsedSP();

        return exists;
    }

    public Badge.BadgeType[] Cheat_AlmostAllBadgesBlacklist()
    {
        return new Badge.BadgeType[] { Badge.BadgeType.SuperCurse, Badge.BadgeType.UltraCurse, Badge.BadgeType.MegaCurse, Badge.BadgeType.RagesPower, Badge.BadgeType.VoraciousEater, Badge.BadgeType.DarkEndurance, Badge.BadgeType.SoftPower, Badge.BadgeType.MetalPower, Badge.BadgeType.RiskyStart };
    }


    public bool BadgeEquippedFull(Badge.BadgeType b, BattleHelper.EntityID eid)
    {
        BadgeDataEntry bde = GlobalBadgeScript.Instance.badgeDataTable[(int)(b) - 1];

        if (bde.singleOrParty)
        {
            return BadgeEquipped(b);
        } else
        {
            return BadgeEquipped(b, eid);
        }
    }

    //Note that BadgeEquipped does not check the copy field
    //(Because if you are in need of the copy field then you would be using BadgeEquippedCount to check if you have multiple)
    public bool BadgeEquipped(Badge.BadgeType b)
    {
        if (MainManager.Instance.Cheat_AlmostAllBadgesActive)
        {
            Badge.BadgeType[] blacklist = Cheat_AlmostAllBadgesBlacklist();

            bool cont = true;

            for (int i = 0; i < blacklist.Length; i++)
            {
                if (blacklist[i] == b)
                {
                    cont = false;
                    break;
                }
            }

            if (cont)
            {
                //Not blacklisted, do stuff

                //do the same thing as the below code, but act as though you have every badge in the badge list
                for (int i = 1; i < (int)(Badge.BadgeType.EndOfTable); i++)
                {
                    if ((Badge.BadgeType)i == b)
                    {
                        return true;
                    }
                }

                //should not be possible to end up here
                //return false;
            }
        }


        for (int i = 0; i < partyEquippedBadges.Count; i++)
        {
            if (partyEquippedBadges[i].type == b)
            {
                return true;
            }
        }
        return false;
    }

    public bool BadgeEquipped(Badge.BadgeType b, BattleHelper.EntityID eid)
    {
        PlayerDataEntry pde = GetPlayerDataEntry(eid);
        if (pde == null)
        {
            return false;
        }

        if (MainManager.Instance.Cheat_AlmostAllBadgesActive)
        {
            Badge.BadgeType[] blacklist = Cheat_AlmostAllBadgesBlacklist();

            bool cont = true;

            for (int i = 0; i < blacklist.Length; i++)
            {
                if (blacklist[i] == b)
                {
                    cont = false;
                    break;
                }
            }

            if (cont)
            {
                //Not blacklisted, do stuff

                //do the same thing as the below code, but act as though you have every badge in the badge list
                for (int i = 1; i < (int)(Badge.BadgeType.EndOfTable); i++)
                {
                    if ((Badge.BadgeType)i == b)
                    {
                        return true;
                    }
                }

                //should not be possible to end up here
                //return false;
            }
        }


        for (int i = 0; i < pde.equippedBadges.Count; i++)
        {
            if (pde.equippedBadges[i].type == b)
            {
                return true;
            }
        }
        return false;
    }

    public int BadgeEquippedCountFull(Badge.BadgeType b, BattleHelper.EntityID eid)
    {
        BadgeDataEntry bde = GlobalBadgeScript.Instance.badgeDataTable[(int)(b) - 1];

        if (bde.singleOrParty)
        {
            return BadgeEquippedCount(b);
        }
        else
        {
            return BadgeEquippedCount(b, eid);
        }
    }
    public int BadgeEquippedCount(Badge.BadgeType b)
    {
        if (MainManager.Instance.Cheat_AlmostAllBadgesActive)
        {
            Badge.BadgeType[] blacklist = Cheat_AlmostAllBadgesBlacklist();

            bool cont = true;

            for (int i = 0; i < blacklist.Length; i++)
            {
                if (blacklist[i] == b)
                {
                    cont = false;
                    break;
                }
            }

            if (cont)
            {
                //Not blacklisted, do stuff

                //do the same thing as the below code, but act as though you have every badge in the badge list
                int icount = 0;
                for (int i = 1; i < (int)(Badge.BadgeType.EndOfTable); i++)
                {
                    if ((Badge.BadgeType)i == b)
                    {
                        icount++;
                    }
                    else
                    {
                        if (GlobalBadgeScript.Instance.badgeDataTable[i - 1].copy == b)
                        {
                            icount++;
                        }
                    }
                }

                if (MainManager.Instance.Cheat_BadgeDoubleStrength)
                {
                    icount *= 2;
                }

                if (MainManager.Instance.Cheat_BadgeNegativeStrength)
                {
                    icount *= -1;
                }

                return icount;
            }
        }

        int count = 0;
        for (int i = 0; i < partyEquippedBadges.Count; i++)
        {
            if (partyEquippedBadges[i].type == b)
            {
                count++;
            } else
            {
                if ((int)(partyEquippedBadges[i].type) - 1 < 0)
                {
                    Debug.LogError("Player has illegal badge");
                    continue;
                }

                if (GlobalBadgeScript.Instance.badgeDataTable[(int)(partyEquippedBadges[i].type) - 1].copy == b)
                {
                    count++;
                }
            }
        }

        if (MainManager.Instance.Cheat_BadgeDoubleStrength)
        {
            count *= 2;
        }

        if (MainManager.Instance.Cheat_BadgeNegativeStrength)
        {
            count *= -1;
        }

        return count;
    }
    public int BadgeEquippedCount(Badge.BadgeType b, BattleHelper.EntityID eid)
    {
        PlayerDataEntry pde = GetPlayerDataEntry(eid);
        if (pde == null)
        {
            return 0;
        }


        if (MainManager.Instance.Cheat_AlmostAllBadgesActive)
        {
            Badge.BadgeType[] blacklist = Cheat_AlmostAllBadgesBlacklist();

            bool cont = true;

            for (int i = 0; i < blacklist.Length; i++)
            {
                if (blacklist[i] == b)
                {
                    cont = false;
                    break;
                }
            }

            if (cont)
            {
                //Not blacklisted, do stuff

                //do the same thing as the below code, but act as though you have every badge in the badge list
                int icount = 0;
                for (int i = 1; i < (int)(Badge.BadgeType.EndOfTable); i++)
                {
                    if ((Badge.BadgeType)i == b)
                    {
                        icount++;
                    }
                    else
                    {
                        if (GlobalBadgeScript.Instance.badgeDataTable[i - 1].copy == b)
                        {
                            icount++;
                        }
                    }
                }

                if (MainManager.Instance.Cheat_BadgeDoubleStrength)
                {
                    icount *= 2;
                }

                if (MainManager.Instance.Cheat_BadgeNegativeStrength)
                {
                    icount *= -1;
                }

                return icount;
            }
        }


        int count = 0;
        for (int i = 0; i < pde.equippedBadges.Count; i++)
        {
            if (pde.equippedBadges[i].type == b)
            {
                count++;
            }
            else
            {
                if ((int)(pde.equippedBadges[i].type) - 1 < 0)
                {
                    Debug.LogError("Player has illegal badge");
                    continue;
                }

                if (GlobalBadgeScript.Instance.badgeDataTable[(int)(pde.equippedBadges[i].type) - 1].copy == b)
                {
                    count++;
                }
            }
        }

        if (MainManager.Instance.Cheat_BadgeDoubleStrength)
        {
            count *= 2;
        }

        if (MainManager.Instance.Cheat_BadgeNegativeStrength)
        {
            count *= -1;
        }

        return count;
    }


    public void ClearInnEffect()
    {
        innEffects = new List<InnEffect>();
    }
    public void AddInnEffect(InnEffect.InnType ie)  //note: replaces inn effect
    {
        if (ie == InnEffect.InnType.None)
        {
            innEffects = new List<InnEffect>();
            return;
        }
        innEffects = new List<InnEffect>
                {
                    new InnEffect(ie)
                };
    }
    public void AddCharmEffect(CharmEffect.CharmType ce, int level)
    {
        int charges = 0;
        int duration = 0;
        int resetDuration = 0;

        //level = 1, 2, 3 normally
        switch (ce)
        {
            case CharmEffect.CharmType.Attack:
            case CharmEffect.CharmType.Defense:
                //overcomplicating things :P
                if (level > 0)
                {
                    resetDuration = 2 * GetPrime(level);
                } else
                {
                    resetDuration = 3 + level;
                    if (resetDuration < 1)
                    {
                        resetDuration = 1;
                    }
                }
                duration = resetDuration;
                if (level > 0)
                {
                    charges = resetDuration * level;
                } else
                {
                    charges = resetDuration;
                }
                break;
            case CharmEffect.CharmType.Fortune:
                charges = 4 - level;
                if (level > 1)
                {
                    duration = level * 8;
                } else
                {
                    duration = Mathf.CeilToInt((8f / (2 - level)));
                }
                resetDuration = 0;
                break;
        }

        //change the current effect?
        bool newEffect = true;
        for (int i = 0; i < charmEffects.Count; i++)
        {
            if (charmEffects[i].charmType == ce || (ce == CharmEffect.CharmType.Defense && charmEffects[i].charmType == CharmEffect.CharmType.Attack) || (ce == CharmEffect.CharmType.Attack && charmEffects[i].charmType == CharmEffect.CharmType.Defense))
            {
                newEffect = false;
                charmEffects[i].charmType = ce;
                charmEffects[i].charges = charges;
                charmEffects[i].duration = duration;
                charmEffects[i].resetDuration = resetDuration;
            }
        }

        if (newEffect)
        {
            charmEffects.Add(new CharmEffect(ce, charges, duration, resetDuration));
        }

        charmEffects.Sort((a, b) => ((int)a.charmType - (int)b.charmType));
    }

    public bool ShowDangerAnim(MainManager.PlayerCharacter pc)
    {
        BattleHelper.EntityID eid = BattleHelper.EntityID.DebugEntity;
        Enum.TryParse(pc.ToString(), out eid);
        return ShowDangerAnim(eid);
    }
    public bool ShowDangerAnim(BattleHelper.EntityID eid)
    {
        PlayerDataEntry pde = GetPlayerDataEntry(eid);
        if (pde == null)
        {
            return false;
        }

        return pde.hp <= PlayerDataEntry.GetDangerHP(eid) || GetRibbonEquipped(Ribbon.RibbonType.ThornyRibbon, eid);
    }

    public static string LevelUpgradeListToString(List<LevelUpgrade> list)
    {
        string output = "";
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            output += list[i];
        }

        return output;
    }
    public static List<LevelUpgrade> ParseLevelUpgradeList(string list)
    {
        List<LevelUpgrade> output = new List<LevelUpgrade>();

        string[] split = list.Split(",");

        if ((split.Length == 0) || (split.Length == 1 && split[0].Length < 1))
        {
            return output;
        }

        LevelUpgrade lu;
        for (int i = 0; i < split.Length; i++)
        {
            Enum.TryParse(split[i], out lu);
            output.Add(lu);
        }

        return output;
    }

    public static PlayerData Parse(string[] list, int startIndex, out int newStartIndex)
    {
        PlayerData output = new PlayerData();

        string[] splitA = list[startIndex].Split(",");

        int temp = 0;
        if (splitA.Length > 0)
        {
            int.TryParse(splitA[0], out temp);
            output.level = temp;
        }

        temp = 0;
        if (splitA.Length > 1)
        {
            int.TryParse(splitA[1], out temp);
            output.astralTokens = temp;
        }

        temp = 0;
        if (splitA.Length > 2)
        {
            int.TryParse(splitA[2], out temp);
            output.ep = temp;
        }

        temp = 0;
        if (splitA.Length > 3)
        {
            int.TryParse(splitA[3], out temp);
            output.maxEP = temp;
        }

        temp = 0;
        if (splitA.Length > 4)
        {
            int.TryParse(splitA[4], out temp);
            output.se = temp;
        }

        temp = 0;
        if (splitA.Length > 5)
        {
            int.TryParse(splitA[5], out temp);
            output.maxSE = temp;
        }

        temp = 0;
        if (splitA.Length > 6)
        {
            int.TryParse(splitA[6], out temp);
            output.sp = temp;
        }

        temp = 0;
        if (splitA.Length > 7)
        {
            int.TryParse(splitA[7], out temp);
            output.usedSP = temp;
        }

        temp = 0;
        if (splitA.Length > 8)
        {
            int.TryParse(splitA[8], out temp);
            output.exp = temp;
        }

        temp = 0;
        if (splitA.Length > 9)
        {
            int.TryParse(splitA[9], out temp);
            output.coins = temp;
        }

        temp = 0;
        if (splitA.Length > 10)
        {
            int.TryParse(splitA[10], out temp);
            output.shards = temp;
        }

        temp = 0;
        if (splitA.Length > 11)
        {
            int.TryParse(splitA[11], out temp);
            output.cumulativeShards = temp;
        }

        temp = 0;
        if (splitA.Length > 12)
        {
            int.TryParse(splitA[12], out temp);
            output.maxInventorySize = temp;
        }

        temp = 0;
        if (splitA.Length > 13)
        {
            int.TryParse(splitA[13], out temp);
            output.maxStorageSize = temp;
        }

        temp = 0;
        if (splitA.Length > 14)
        {
            int.TryParse(splitA[14], out temp);
            output.focusCap = temp;
        }

        temp = 0;
        if (splitA.Length > 15)
        {
            int.TryParse(splitA[15], out temp);
            output.absorbCap = temp;
        }

        temp = 0;
        if (splitA.Length > 16)
        {
            int.TryParse(splitA[16], out temp);
            output.burstCap = temp;
        }

        temp = 0;
        if (splitA.Length > 17)
        {
            int.TryParse(splitA[17], out temp);
            output.itemCounter = temp;
        }

        temp = 0;
        if (splitA.Length > 18)
        {
            int.TryParse(splitA[18], out temp);
            output.itemsUsed = temp;
        }

        temp = 0;
        if (splitA.Length > 19)
        {
            int.TryParse(splitA[19], out temp);
            output.cumulativeBattleTurns = temp;
        }

        temp = 0;
        if (splitA.Length > 20)
        {
            int.TryParse(splitA[20], out temp);
            output.cumulativeDamageDealt = temp;
        }

        temp = 0;
        if (splitA.Length > 21)
        {
            int.TryParse(splitA[21], out temp);
            output.cumulativeDamageTaken = temp;
        }

        temp = 0;
        if (splitA.Length > 22)
        {
            int.TryParse(splitA[22], out temp);
            output.maxDamagePerTurn = temp;
        }

        temp = 0;
        if (splitA.Length > 23)
        {
            int.TryParse(splitA[23], out temp);
            output.maxDamageSingleHit = temp;
        }

        temp = 0;
        if (splitA.Length > 24)
        {
            int.TryParse(splitA[24], out temp);
            output.totalBattles = temp;
        }

        temp = 0;
        if (splitA.Length > 25)
        {
            int.TryParse(splitA[25], out temp);
            output.battlesWon = temp;
        }

        temp = 0;
        if (splitA.Length > 26)
        {
            int.TryParse(splitA[26], out temp);
            output.battlesFled = temp;
        }

        temp = 0;
        if (splitA.Length > 27)
        {
            int.TryParse(splitA[27], out temp);
            output.battlesLost = temp;
        }

        startIndex++;
        output.upgrades = ParseLevelUpgradeList(list[startIndex]);

        startIndex++;
        output.downgrades = ParseLevelUpgradeList(list[startIndex]);

        startIndex++;
        output.itemInventory = Item.ParseList(list[startIndex]);

        startIndex++;
        output.storageInventory = Item.ParseList(list[startIndex]);

        startIndex++;
        output.keyInventory = KeyItem.ParseList(list[startIndex]);

        startIndex++;
        output.badgeInventory = Badge.ParseList(list[startIndex]);

        startIndex++;
        output.equippedBadges = Badge.ParseList(list[startIndex]);

        startIndex++;
        output.partyEquippedBadges = Badge.ParseList(list[startIndex]);

        startIndex++;
        output.ribbonInventory = Ribbon.ParseList(list[startIndex]);

        startIndex++;
        output.charmEffects = CharmEffect.ParseList(list[startIndex]);

        startIndex++;
        output.innEffects = InnEffect.ParseList(list[startIndex]);

        startIndex++;
        string[] splitB = list[startIndex].Split(",");

        int partySize = 0;
        int hiddenSize = 0;
        int.TryParse(splitB[0], out partySize);
        int.TryParse(splitB[1], out hiddenSize);

        //Bonus followers
        output.bonusFollowers = new List<SpriteID>();
        for (int i = 2; i < splitB.Length; i++)
        {
            if (Enum.TryParse(splitB[i], out SpriteID sid))
            {
                output.bonusFollowers.Add(sid);
            }
            else
            {
                Debug.LogWarning("Can't parse sprite ID " + splitB[i]);
            }
        }

        output.party = new List<PlayerDataEntry>();
        output.hiddenParty = new List<PlayerDataEntry>();

        startIndex++;
        for (int i = 0; i < partySize; i++)
        {
            output.party.Add(PlayerDataEntry.Parse(new string[] { list[startIndex], list[startIndex + 1] }));
            startIndex += 2;
        }
        for (int i = 0; i < hiddenSize; i++)
        {
            output.hiddenParty.Add(PlayerDataEntry.Parse(new string[] { list[startIndex], list[startIndex + 1] }));
            startIndex += 2;
        }

        output.UpdateMaxStats();
        newStartIndex = startIndex;
        return output;
    }
    public override string ToString()
    {
        string output = "";

        output += level;
        output += ",";
        output += astralTokens;
        output += ",";
        output += ep;
        output += ",";
        output += maxEP;
        output += ",";
        output += se;
        output += ",";
        output += maxSE;
        output += ",";
        output += sp;
        output += ",";
        output += usedSP;
        output += ",";
        output += exp;
        output += ",";
        output += coins;
        output += ",";
        output += shards;
        output += ",";
        output += cumulativeShards;
        output += ",";
        output += maxInventorySize;
        output += ",";
        output += maxStorageSize;
        output += ",";
        output += focusCap;
        output += ",";
        output += absorbCap;
        output += ",";
        output += burstCap;
        output += ",";
        output += itemCounter;
        output += ",";
        output += itemsUsed;
        output += ",";
        output += cumulativeBattleTurns;
        output += ",";
        output += cumulativeDamageDealt;
        output += ",";
        output += cumulativeDamageTaken;
        output += ",";
        output += maxDamagePerTurn;
        output += ",";
        output += maxDamageSingleHit;
        output += ",";
        output += totalBattles;
        output += ",";
        output += battlesWon;
        output += ",";
        output += battlesFled;
        output += ",";
        output += battlesLost;

        //do all the lists
        output += "\n";
        output += LevelUpgradeListToString(upgrades);
        output += "\n";
        output += LevelUpgradeListToString(downgrades);
        output += "\n";
        output += Item.ListToString(itemInventory);
        output += "\n";
        output += Item.ListToString(storageInventory);
        output += "\n";
        output += KeyItem.ListToString(keyInventory);
        output += "\n";
        output += Badge.ListToString(badgeInventory);
        output += "\n";
        output += Badge.ListToString(equippedBadges);
        output += "\n";
        output += Badge.ListToString(partyEquippedBadges);
        output += "\n";
        output += Ribbon.ListToString(ribbonInventory);
        output += "\n";
        output += CharmEffect.ListToString(charmEffects);
        output += "\n";
        output += InnEffect.ListToString(innEffects);

        //party, hiddenparty
        output += "\n";
        output += party.Count;
        output += ",";
        output += hiddenParty.Count;
        
        if (bonusFollowers != null)
        {
            for (int i = 0; i < bonusFollowers.Count; i++)
            {
                output += ",";
                output += bonusFollowers[i];
            }
        }

        for (int i = 0; i < party.Count; i++)
        {
            output += "\n";
            output += party[i];
        }
        for (int i = 0; i < hiddenParty.Count; i++)
        {
            output += "\n";
            output += hiddenParty[i];
        }

        return output;
    }

    public PlayerData()
    {
        party = new List<PlayerDataEntry>();
        hiddenParty = new List<PlayerDataEntry>();
        itemInventory = new List<Item>();
        storageInventory = new List<Item>();
        keyInventory = new List<KeyItem>();
        level = 1;
        exp = 0;
        upgrades = new List<LevelUpgrade>();
        downgrades = new List<LevelUpgrade>();
        badgeInventory = new List<Badge>();
        equippedBadges = new List<Badge>();
        partyEquippedBadges = new List<Badge>();
        ribbonInventory = new List<Ribbon>();
        charmEffects = new List<CharmEffect>();
        innEffects = new List<InnEffect>();
    }
    public PlayerData(params BattleHelper.EntityID[] eids)
    {
        party = new List<PlayerDataEntry>();
        hiddenParty = new List<PlayerDataEntry>();
        itemInventory = new List<Item>();
        storageInventory = new List<Item>();
        keyInventory = new List<KeyItem>();
        level = 1;
        exp = 0;
        upgrades = new List<LevelUpgrade>();
        downgrades = new List<LevelUpgrade>();

        for (int i = 0; i < eids.Length; i++)
        {
            party.Add(new PlayerDataEntry(eids[i]));

            //party[i].maxHp = PlayerDataEntry.GetMaxHP(eids[i], -1);
            //party[i].hp = party[i].maxHp;
        }

        maxEP = GetMaxEP(0);
        ep = maxEP;

        //maxSP = GetMaxSP(0);
        sp = GetMaxSP(0);
        usedSP = 0;
        maxSE = sp;
        se = sp;

        badgeInventory = new List<Badge>();
        equippedBadges = new List<Badge>();
        partyEquippedBadges = new List<Badge>();
        ribbonInventory = new List<Ribbon>();
        charmEffects = new List<CharmEffect>();
        innEffects = new List<InnEffect>();
    }
}

public class MainManager : MonoBehaviour
{
    public Font font;
    public int fontSize = 20; //default size of canvas text

    //many methods are static (any methods that do not use specific data)
    public static MainManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MainManager>(); //this should work
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    public BattleControl BattleControl {
        get
        {
            if (battleControl == null)
            {
                battleControl = FindObjectOfType<BattleControl>();
            }
            return battleControl;
        }
    }
    public Canvas Canvas
    {
        get
        {
            if (intCanvas == null)
            {
                Canvas[] list = GetComponentsInChildren<Canvas>();
                intCanvas = list[1];
            }
            return intCanvas;
        }
    }
    public Canvas SuperCanvas
    {
        get
        {
            if (intCanvasS == null)
            {
                Canvas[] list = GetComponentsInChildren<Canvas>();
                intCanvasS = list[0];
            }
            return intCanvasS;
        }
    }
    public WorldCamera Camera
    {
        get
        {
            if (intCamera == null)
            {
                intCamera = FindObjectOfType<WorldCamera>();
            }
            return intCamera;
        }
    }
    private static MainManager instance;
    private BattleControl battleControl;
    private WorldCamera intCamera;
    public WorldCameraSnapshot cameraSnapshot;
    public int cameraAnimIndex;
    private Canvas intCanvas;
    private Canvas intCanvasS;
    [SerializeField]
    private FadeoutScript fadeOutScript;
    [SerializeField]
    private BattleFadeoutScript battleFadeOutScript;
    private GameObject textbox; //current textbox

    public MapScript mapScript;
    public SkyboxID curSkybox = (SkyboxID)(-1);

    public PlayerData playerData;
    public PlayerData gameOverPlayerData;   //Special thing for making the game over pause menu work properly (Need to be able to reset your state to before you changed stuff 
    public EncounterData nextBattle;
    public BattleStartArguments battleStartArguments;
    public int coinDrops;
    public int dropItemCount;
    public Item.ItemType dropItemType;

    //Used to pass menu results outside of textboxes in some cases (coroutines can't have out parameters)
    //Watch out for stale values! (set this to "" or null after use)
    //Note that the text creation method will set this to null to make stale value problems less common
    public string lastTextboxMenuResult;
    //public MenuResult lastTextboxMenuResultObject;
    public ITextSpeaker lastSpeaker;

    //public List<CutsceneScript> cutsceneQueue;
    //public List<InteractCutsceneTrigger> interactListeners; //need to use this to determine what happens when you press A (trigger cutscene?)
    public int lastStartedCutscene = -1;
    public int lastEndedCutscene = -1;

    public List<InteractTrigger> interactTriggers;

    public bool inCutscene
    {
        get => (lastStartedCutscene != lastEndedCutscene);
    }       //blocks movement (This is purely an overworld value, so battles don't touch this until the return from battle script runs

    public bool mapHalted;
    public bool isPaused;       //referenced to disable physics and overworld control (and of course all map update scripts)
    public float pauseTime;

    public PostProcessVolume concealedVolume;

    //moved to GlobalItemScript, GlobalBadgeScript
    //public ItemDataEntry[] itemDataTable;
    //public BadgeDataEntry[] badgeDataTable;

    //note: can't be moved into BattleControl because it is required to properly display the Bestiary

    //To do: Move this to GlobalInformationScript
    public string[][] enemyText;    //index with (int)entityID + 1
    public string[][] enemyMoveText;

    public BattleEntityData[] battleEntityTable;
    public BattleEntityMovesetData[] battleEntityMovesetTable;
    public BestiaryOrderEntry[] bestiaryOrder;


    public string[][] testTextFile;

    public WorldMode worldMode = WorldMode.Overworld;
    public enum WorldMode
    {
        Overworld,
        Battle,
        GameOver,
        Start
    }

    //not player, not follower (ignorePlayer), not camera zone
    public const int LayerMaskNonPlayer = 311;

    public Pause_SectionBase pauseMenuScript;
    public StartMenuManager startMenu;

    public WorldPlayer worldPlayer;

    public float worldspaceYaw;

    //assets and prefabs
    //public Sprite[] damageSprites;
    public GameObject overworldHUD;
    public NamePopupScript areaNamePopup;
    public AchievementPopupScript achievementPopup;

    public Sprite defaultSprite;
    public GameObject defaultTextbox;
    public GameObject defaultMinibubble;
    public GameObject getItemPopup;
    public GameObject tooManyItemsPopup;
    public GameObject worldspaceShopEntry;
    public GameObject defaultCollectible;
    public GameObject menuBase;
    public GameObject menuEntryBase;
    public GameObject descriptionBoxBase;
    public GameObject promptMenuBase;
    public GameObject promptMenuEntryBase;
    public GameObject numberMenu;
    public GameObject textEntryMenu;
    public GameObject text_ButtonSprite;
    public GameObject text_ItemSprite;
    public GameObject text_KeyItemSprite;
    public GameObject text_BadgeSprite;
    public GameObject text_RibbonSprite;
    public GameObject text_CommonSprite;
    public GameObject text_MiscSprite;
    public GameObject text_EffectSprite;
    public GameObject text_StateSprite;

    public GameObject gameOverObject;

    //general sprite material
    public Material defaultSpriteMaterial;
    public Material defaultSpriteSimpleMaterial;
    public Material defaultSpriteFlickerMaterial;

    public Material defaultGUISpriteMaterial;

    //Other sprites
    public Sprite damageEffectStar;
    public Sprite heartEffect;
    public Sprite energyEffect;
    public Sprite hexagonEffect;
    public Sprite soulEffect;
    public Sprite staminaEffect;
    public Sprite coinEffect;

    public Sprite[] itemSprites;
    public Sprite[] keyItemSprites;
    public Sprite[] badgeSprites;
    public Sprite[] ribbonSprites;
    public Sprite[] commonSprites;    
    public Sprite[] miscSprites;
    public Sprite[] effectSprites;
    public Sprite[] stateSprites;

    //pause
    public GameObject pauseMenu;
    //public GameObject pauseStatusMenu;    //going to make this use the resources load thing

    //physics materials I guess
    public PhysicMaterial noFrictionMaterial;
    public PhysicMaterial allFrictionMaterial;


    public int saveIndex;
    public string saveName;

    public float playTime;
    public float lastSaveTimestamp;

    public WorldLocation lastSaveLocation;
    public MapID lastSaveMap;

    //To do later: replace with a key value system
    //This will let me use enums for everything and avoid problems
    //(will have to store enums as strings when saving to avoid problems with changing enum values)

    //these numbers are pretty arbitrary
    //but I will probably need more flags than vars
    //since flags are less information
    //public const int GLOBAL_FLAG_COUNT = 512;
    //public const int GLOBAL_VAR_COUNT = 128;
    //public const int AREA_FLAG_COUNT = 128;
    //public const int AREA_VAR_COUNT = 32;
    //public const int MAP_FLAG_COUNT = 32;
    //public const int MAP_VAR_COUNT = 8;

    public OverworldHUDScript curOverworldHUD;
    public bool showCoins;
    public float hudShowTime;
    public int lastCoinCount;
    public const float HUD_MIN_SHOW_TIME = 1f;  //when you get coins and stuff it shows the hud for some time
    public const float HUD_BUTTON_SHOW_TIME = 3f;

    public Dictionary<GlobalFlag, bool> globalFlags;
    public Dictionary<GlobalVar, string> globalVars;
    //public bool[] globalFlags;
    //public string[] globalVars;

    public Dictionary<Badge.BadgeType, Badge.BadgeType> badgeMutation;
    public Dictionary<Badge.BadgeType, Badge.BadgeType> badgeMutationInverse;
    public Dictionary<Ribbon.RibbonType, Ribbon.RibbonType> ribbonMutation;
    public Dictionary<Ribbon.RibbonType, Ribbon.RibbonType> ribbonMutationInverse;

    //new setup
    //(enum value) and thing dictionaries
    //enums parse to numbers which should hash pretty easily (so no performance problems?)
    //Not sure what Enum.Parse does but it shouldn't be that bad?
    //When they get saved they get saved as text
    public enum GlobalVar //so the save file will have these as text (cross version compatibility)
    {
        GV_None = 0,
        GV_StoryProgress = 1,
        GV_BadgeRandomPermutation,
        GV_RibbonRandomPermutation,
        GV_PitFloor,
        GV_PitAltar,
    }
    public enum GlobalFlag //so the save file will have these as text (cross version compatibility)
    {
        GF_None = 0,
        GF_FileCode_Greed,
        GF_FileCode_Envy,
        GF_FileCode_Gluttony,
        GF_FileCode_Wrath,
        GF_FileCode_Sloth,
        GF_FileCode_Pride,
        GF_FileCode_Lust,

        GF_FileCode_Randomizer,

        GF_FileCodeExplain_Greed,
        GF_FileCodeExplain_Envy,
        GF_FileCodeExplain_Gluttony,
        GF_FileCodeExplain_Wrath,
        GF_FileCodeExplain_Sloth,
        GF_FileCodeExplain_Pride,
        GF_FileCodeExplain_Lust,

        GF_FileCodeExplain_Randomizer,



        GF_RandomItemModifiers,

        GF_QuestStart_Prologue_Test,
        GF_QuestComplete_Prologue_Test,

        //All of them are here
        //This will definitely not lower the performance of global flag operations :P
        GF_Recipe_Mistake,
        GF_Recipe_BigMistake,
        GF_Recipe_CarrotSalad,
        GF_Recipe_BerryCandy,
        GF_Recipe_BerryCarrot,
        GF_Recipe_RoastedNut,
        GF_Recipe_SourJam,
        GF_Recipe_SweetNut,
        GF_Recipe_PowerJam,
        GF_Recipe_BronzeSalad,
        GF_Recipe_FlowerCandy,
        GF_Recipe_BerryBronze,
        GF_Recipe_FlowerCarrot,
        GF_Recipe_FlowerBronze,
        GF_Recipe_BitterCoffee,
        GF_Recipe_AppleJuice,
        GF_Recipe_HealthySalad,
        GF_Recipe_CarrotCake,
        GF_Recipe_BronzeCake,
        GF_Recipe_DryBread,
        GF_Recipe_MiracleNeedle,
        GF_Recipe_RabbitBun,
        GF_Recipe_ApplePie,
        GF_Recipe_JuiceMix,
        GF_Recipe_LemonCandy,
        GF_Recipe_WarpedCookie,
        GF_Recipe_WarpedTea,
        GF_Recipe_ThornBundle,
        GF_Recipe_Lemonade,
        GF_Recipe_BerryCarrotBunch,
        GF_Recipe_CarrotPudding,
        GF_Recipe_BronzePudding,
        GF_Recipe_CoconutMilk,
        GF_Recipe_CoconutCream,
        GF_Recipe_MelonJug,
        GF_Recipe_MelonJuice,
        GF_Recipe_CocoPop,
        GF_Recipe_HustlePudding,
        GF_Recipe_MilkCoffee,
        GF_Recipe_TropicalParfait,
        GF_Recipe_SleepyTea,
        GF_Recipe_Meringue,
        GF_Recipe_SlimeBomb,
        GF_Recipe_SilverSalad,
        GF_Recipe_BerrySilver,
        GF_Recipe_FlowerSilver,
        GF_Recipe_SilverCake,
        GF_Recipe_SilverPudding,
        GF_Recipe_MetalSalad,
        GF_Recipe_SpicyNut,
        GF_Recipe_BombJuice,
        GF_Recipe_FriedEgg,
        GF_Recipe_PowerSteak,
        GF_Recipe_DeliciousMeal,
        GF_Recipe_ScaleBurger,
        GF_Recipe_PepperNeedle,
        GF_Recipe_DragonShake,
        GF_Recipe_FlowerBronzeBunch,
        GF_Recipe_HoneyCandy,
        GF_Recipe_HoneyCarrot,
        GF_Recipe_HoneyBronze,
        GF_Recipe_HoneySilver,
        GF_Recipe_HoneyCaramel,
        GF_Recipe_StrangeSalad,
        GF_Recipe_RoastedMushroom,
        GF_Recipe_StrangeShake,
        GF_Recipe_HoneyApple,
        GF_Recipe_DarkBreakfast,
        GF_Recipe_MiracleShroom,
        GF_Recipe_PoisonSkewer,
        GF_Recipe_WeirdTea,
        GF_Recipe_MoonCookie,
        GF_Recipe_SnowFluff,
        GF_Recipe_FluffCandy,
        GF_Recipe_TuftCandy,
        GF_Recipe_FluffCarrot,
        GF_Recipe_FluffBronze,
        GF_Recipe_FluffSilver,
        GF_Recipe_TuftCarrot,
        GF_Recipe_TuftBronze,
        GF_Recipe_TuftSilver,
        GF_Recipe_PotatoSalad,
        GF_Recipe_NectarCrystal,
        GF_Recipe_PumpkinPie,
        GF_Recipe_CandyPumpkin,
        GF_Recipe_SpicyFries,
        GF_Recipe_IceCream,
        GF_Recipe_MintSalad,
        GF_Recipe_EggplantTea,
        GF_Recipe_SweetSmoothie,
        GF_Recipe_SnowCone,
        GF_Recipe_LuxurySteak,
        GF_Recipe_DeluxeDinner,
        GF_Recipe_SnowyMeringue,
        GF_Recipe_PeachPie,
        GF_Recipe_MintSpikes,
        GF_Recipe_SwiftCake,
        GF_Recipe_TwinSalad,
        GF_Recipe_HalfSalad,
        GF_Recipe_BerryTwins,
        GF_Recipe_FlowerTwins,
        GF_Recipe_HoneyTwins,
        GF_Recipe_FluffTwins,
        GF_Recipe_TuftTwins,
        GF_Recipe_TwinCake,
        GF_Recipe_TwinPudding,
        GF_Recipe_BerryHalf,
        GF_Recipe_FlowerHalf,
        GF_Recipe_HoneyHalf,
        GF_Recipe_FluffHalf,
        GF_Recipe_TuftHalf,
        GF_Recipe_HalfCake,
        GF_Recipe_HalfPudding,
        GF_Recipe_GoldenTea,
        GF_Recipe_BrightSoup,
        GF_Recipe_PineappleJuice,
        GF_Recipe_MintSmoothie,
        GF_Recipe_MoonlightJelly,
        GF_Recipe_GoldRabbitBun,
        GF_Recipe_GoldOmelette,
        GF_Recipe_LivelySalad,
        GF_Recipe_MushroomSoup,
        GF_Recipe_ConversionShake,
        GF_Recipe_DragonTea,
        GF_Recipe_StrangeMushroom,
        GF_Recipe_GoldLemonade,
        GF_Recipe_BombShake,
        GF_Recipe_PineappleCone,
        GF_Recipe_DeluxeJuice,
        GF_Recipe_MelonBomb,
        GF_Recipe_GoldNut,
        GF_Recipe_RabbitCake,
        GF_Recipe_GoldenSalad,
        GF_Recipe_RoyalCandy,
        GF_Recipe_BerryGolden,
        GF_Recipe_FlowerGolden,
        GF_Recipe_HoneyGolden,
        GF_Recipe_FluffGolden,
        GF_Recipe_TuftGolden,
        GF_Recipe_RoyalGolden,
        GF_Recipe_RoyalCarrot,
        GF_Recipe_GoldenCake,
        GF_Recipe_GoldenPudding,
        GF_Recipe_RoyalBronze,
        GF_Recipe_RoyalSilver,
        GF_Recipe_RoyalTwins,
        GF_Recipe_RoyalHalf,
        GF_Recipe_SunMuffin,
        GF_Recipe_SolarSpaghetti,
        GF_Recipe_RoyalSalad,
        GF_Recipe_MoonPop,
        GF_Recipe_SunSauce,
        GF_Recipe_SourSmoothie,
        GF_Recipe_CelestialSoup,
        GF_Recipe_RoyalDinner,
        GF_Recipe_CelestialMuffin,
        GF_Recipe_PolarCaramel,
        GF_Recipe_CelestialParfait,
        GF_Recipe_SupremeDessert,
        GF_Recipe_SupremeDinner,
        GF_Recipe_CursedStew,
        GF_Recipe_AetherSalad,
        GF_Recipe_CrystalSalad,
        GF_Recipe_StellarCandy,
        GF_Recipe_MiracleCandy,
        GF_Recipe_StellarAether,
        GF_Recipe_MiracleCrystal,
        GF_Recipe_AetherCake,
        GF_Recipe_CrystalCake,
        GF_Recipe_AetherPudding,
        GF_Recipe_CrystalPudding,
        GF_Recipe_CrystalMeringue,
        GF_Recipe_MiracleCaramel,
        GF_Recipe_StellarCaramel,
        GF_Recipe_BoosterShake,
        GF_Recipe_DiluteShake,
        GF_Recipe_InversionStew,
        GF_Recipe_ThickShake,
        GF_Recipe_MiracleApple,
        GF_Recipe_Limeade,
        GF_Recipe_FlamingTea,
        GF_Recipe_RubyJuice,
        GF_Recipe_VoidCone,
        GF_Recipe_DiamondSoup,

        //bestiary
        //note: when setting: check for subindex stuff and set all the associated flags
        GF_Bestiary_Leafling,
        GF_Bestiary_Flowerling,
        GF_Bestiary_Shrublet,
        GF_Bestiary_Sunflower,
        GF_Bestiary_Sunnybud,
        GF_Bestiary_MiracleBloom,
        GF_Bestiary_Rockling,
        GF_Bestiary_Honeybud,
        GF_Bestiary_BurrowTrap,
        GF_Bestiary_Sundew,
        GF_Bestiary_Bandit,
        GF_Bestiary_Renegade,
        GF_Bestiary_Sentry,
        GF_Bestiary_Cactupole,
        GF_Bestiary_Sandswimmer,
        GF_Bestiary_Slime,
        GF_Bestiary_Slimewalker,
        GF_Bestiary_Slimeworm,
        GF_Bestiary_Slimebloom,
        GF_Bestiary_SirenFish,
        GF_Bestiary_Blazecrest,
        GF_Bestiary_Embercrest,
        GF_Bestiary_Ashcrest,
        GF_Bestiary_Flametongue,
        GF_Bestiary_Heatwing,
        GF_Bestiary_Lavaswimmer,
        GF_Bestiary_EyeSpore,
        GF_Bestiary_SpikeShroom,
        GF_Bestiary_Shrouder,
        GF_Bestiary_HoarderFly,
        GF_Bestiary_Mosquito,
        GF_Bestiary_Shieldwing,
        GF_Bestiary_Honeywing,
        GF_Bestiary_Shimmerwing,
        GF_Bestiary_LumistarVanguard,
        GF_Bestiary_LumistarSoldier,
        GF_Bestiary_LumistarStriker,
        GF_Bestiary_Plateshell,
        GF_Bestiary_Speartongue,
        GF_Bestiary_Chaintail,
        GF_Bestiary_Sawcrest,
        GF_Bestiary_Coiler,
        GF_Bestiary_Drillbeak,
        GF_Bestiary_PuffJelly,
        GF_Bestiary_Fluffling,
        GF_Bestiary_CloudJelly,
        GF_Bestiary_CrystalCrab,
        GF_Bestiary_CrystalSlug,
        GF_Bestiary_CrystalClam,
        GF_Bestiary_AuroraWing,
        GF_Bestiary_Plaguebud,
        GF_Bestiary_Starfish,
        GF_Bestiary_CursedEye,
        GF_Bestiary_StrangeTendril,
        GF_Bestiary_DrainBud,

        GF_ACH_Halfway,
        GF_ACH_Complete,
        GF_ACH_DiamondRibbon,
        GF_ACH_LevelCap,
        GF_ACH_Risky,
        GF_ACH_KeyHoarder,
        GF_ACH_CursedStew,
        GF_ACH_Ouch,
    }
    public enum StoryProgress
    {
        SP_None = 0,
        SP_Prologue_NewGame,  //This is what new files end up in
    }

    public enum Achievement
    {
        ACH_None = 0,

        ACH_Halfway,
        ACH_Complete,
        ACH_DiamondRibbon,
        ACH_LevelCap,
        ACH_Risky,
        ACH_KeyHoarder,
        ACH_CursedStew,
        ACH_Ouch,

        /*
        ACH_FullCompletion,
        ACH_AllBadges,
        ACH_AllRibbons,
        ACH_AllQuests,
        ACH_AllRecipes,
        ACH_AllEnemies,
        ACH_AllInformation,
        ACH_AllLore,
        ACH_AllShards,
        ACH_AllMusic,
        ACH_MaxLevel,
        ACH_RealMaxLevel,
        ACH_Champion,
        ACH_SecretBadge,
        ACH_ForsakenMountains,
        ACH_BeatKeruAster,
        ACH_SupremeRecipe,
        ACH_EdgeOfPossibility,
        ACH_SoulPillar,
        ACH_SoulShell,
        ACH_Prologue,
        ACH_Chapter1,
        ACH_Chapter2,
        ACH_Chapter3,
        ACH_Chapter4,
        ACH_Chapter5,
        ACH_Chapter6,
        ACH_Chapter7,
        ACH_Chapter8,
        ACH_EpilogueOrder,
        ACH_EpilogueChaos,
        ACH_HardPrologue,
        ACH_HardChapter1,
        ACH_HardChapter2,
        ACH_HardChapter3,
        ACH_HardChapter4,
        ACH_HardChapter5,
        ACH_HardChapter6,
        ACH_HardChapter7,
        ACH_HardChapter8,
        ACH_HardEpilogueOrder,
        ACH_HardEpilogueChaos,
        */

        EndOfTable
    }

    public enum MiscSprite
    {
        Default = 0,
        MysteryItem,
        MysteryRecipe,
        MysteryBadge,
        MysteryRibbon,
        NoItem,
        NoRecipe,
        NoBadge,
        NoRibbon,
        AbilitySlash,
        AbilitySlash2,
        AbilitySlash3,
        AbilityAetherize,
        AbilityDoubleJump,
        AbilitySuperJump,
        AbilitySmash,
        AbilitySmash2,
        AbilitySmash3,
        AbilityIlluminate,
        AbilityDashHop,
        AbilityDig,
        Health6,
        Health12,
        Health30,
        Health60,
        Energy6,
        Energy12,
        Energy30,
        Energy60,
        Soul6,
        Soul12,
        Soul30,
        Soul60,
        XP10,
        XP25,
        XP50,
        XP99
    }

    //area flags are stored per area, but are cleared when leaving area (used for things like enemy spawning)
    //public bool[] areaFlags;
    //public string[] areaVars;

    public Dictionary<string, bool> areaFlags;
    public Dictionary<string, string> areaVars;

    //map flags and vars are stored per map, are reset every time you change maps (probably won't need these very often, probably only for puzzle states)
    //
    //

    //Stored global text files (in CSV format)
    //Note: Map text is separate (stored with the map script)
    public string[][] menuText;
    public string[][] commonText;
    public string[][] systemText;

    public Color[][] weaponColors;

    //Cheat values
    //should show up at the bottom of inspector for easier access
    public bool Cheat_CheatMenuAvailable;   //note: cheat menu cannot unset this

    public bool Cheat_NoClip;               //noclip (you can go anywhere, A to go up, Z to go down)
    public bool Cheat_KeruAsterJump;
    public bool Cheat_FirstPersonCamera;
    public bool Cheat_FreeCam;              //freely moving cam
    public bool Cheat_RevolvingCam;         //camera that revolves around you
    public bool Cheat_SuperSlowTimeScale;
    public bool Cheat_SlowTimeScale;
    public bool Cheat_FastTimeScale;
    public bool Cheat_SuperFastTimeScale;
    public bool Cheat_Halt;                 //Timescale = 0
    public bool Cheat_InvisibleText;
    public bool Cheat_AlmostAllBadgesActive;    //does not include certain "worse than useless" badges
    public bool Cheat_BadgeDoubleStrength;  //badges act as if you have double of them equipped (for most stackable badges) (note that a lot of 1 off badges still have stacking logic)
    public bool Cheat_BadgeNegativeStrength; //badges have negative strength (Non stackable badges don't work with this) (Effect related badges are a bit broken with this)
    public bool Cheat_TooManyBadges;    //badge pickups are doubled
    public bool Cheat_NoEffectCaps;
    public bool Cheat_BattleWin;        //Battles are won instantly
    public bool Cheat_BattleRandomActions;  //wacky non-cheat ish actions in battle
    public bool Cheat_BattleCheatActions;   //Activates battle cheat actions upon entering battle
    public bool Cheat_BattleInfoActions;   //informational actions
    public bool Cheat_BadgeAnarchy;    //ignore badge thing
    public bool Cheat_StaminaAnarchy;    //ignore stamina thing
    public bool Cheat_EnergyAnarchy;    //ignore energy thing
    public bool Cheat_TargetAnarchy;    //Single target moves act as Anyone targetting moves
    public bool Cheat_LevelAnarchy; //makes level limits for moves really high
    public bool Cheat_SkillSwap;    //Skill swap is always active
    public bool Cheat_InfiniteBite; //Multi Bite is always active and gives infinite
    public bool Cheat_BadgeSwap;    //Badge swap is always active
    public bool Cheat_RibbonSwap;   //Ribbon swap is always active
    public bool Cheat_WilexMoveUnlock;  //unlocks max level of all wilex moves
    public bool Cheat_LunaMoveUnlock;  //unlocks max level of all luna moves
    public bool Cheat_PlayerCurseAttack;
    public bool Cheat_SeePickupCounts;  //reveals item count, badge count, etc in names of things
    public bool Cheat_DoubleStrengthEnviroEffects;
    public bool Cheat_QuadrupleStrengthEnviroEffects;
    public bool Cheat_UnrestrictedTextEntry;    //removes the escape sequences and lets you type infinite characters
    public bool Cheat_OverworldHazardImmunity;  //hazard states don't happen
    public bool Cheat_OverworldEncounterImmunity;  //Block mapscript from starting battles
    public bool Cheat_ControlNeverDisabled; //ignore the disable control thing
    public bool Cheat_SplitParty;   //split party mode (note: does not work correctly with map transitions, also need to change properties of the worldfollower to act similar to worldplayer) (note 2: if follower ends up on unstable ground, may cause problems)

    public int frameCounter;
    public float frameTime;
    public float lastCalculatedFPS;

    public float greedPartialCoins;

    public enum GameConst
    {
        PartyCount,     //# of characters in full party
        ItemProportion, //Item count / max item count
        ItemCount,      //Item count
        ItemMax,        //Max Item Count
        HPProportion,   //Average hp proportion for party (hp / maxhp)
        EPProportion,   //Average ep proportion for party (ep / maxep)
        Level,          //Current level
        CurrentXP,      //Not cumulative XP
        Coins,          //How much money you have
        Shard, //How many prismatic shards you have
        Chapter,
    }

    //where to put you on the world map and what to say on the pause screen
    public enum WorldLocation
    {
        None = 0,
        SolarGrove,
        CrystalHills,
        CrystalCity,
        CrystalCitadel,
        RealmOfPossibility,
        VerdantForest,
        SacredGrove,
        TrialOfSimplicity,
        TempestDesert,
        BanditCaverns,
        HiddenOasis,
        TrialOfHaste,
        GemstoneIslands,
        SapphireIsland,
        SapphireAtoll,
        TrialOfPatience,
        InfernalCaldera,
        MoltenPit,
        TrialOfZeal,
        ShroudedValley,
        SinisterCave,
        TrialOfAmbition,
        RadiantPlateau,
        Lumistar,
        RadiantPeak,
        TrialOfResolve,
        AetherTrench,
        MoltenTitan,
        GoldenPort,
        ForsakenMountains,
        ForsakenPass,
        DelugeTitan
    }
    public enum MapID
    {
        None = 0,   //error value
        Test_Main,
        Test_LandscapeC0,
        Test_LandscapeC1,
        Test_LandscapeC2,
        Test_LandscapeC3,
        Test_LandscapeC4,
        Test_LandscapeC5,
        Test_LandscapeC6,
        Test_LandscapeC7,
        Test_LandscapeC8,
        Test_LandscapeC9,
        Test_VertexWarp,
        RabbitHole_Lobby,
        RabbitHole_NormalFloor,
        RabbitHole_RestFloor,
        RabbitHole_FinalFloor
    }
    public enum BattleMapID
    {
        Test_BattleMap,
        TestBattle_SolarGrove,
        TestBattle_VerdantForest,
        TestBattle_TempestDesert,
        TestBattle_GemstoneIslands,
        TestBattle_InfernalCaldera,
        TestBattle_ShroudedValley,
        TestBattle_RadiantPlateau,
        TestBattle_AetherTrench,
        TestBattle_CrystalHills,
        TestBattle_ForsakenMountains,
    }
    public enum SkyboxID
    {
        Invalid = 0,
        Black,
        SolarGrove,
        CrystalHills,
        VerdantForest,
        TempestDesert,
        GemstoneIslands,
        GemstoneIslandSea,
        InfernalCaldera,
        ShroudedValley,
        RadiantPlateau,
        AetherTrench,
        ForsakenMountains
    }
    
    //characters you can control
    public enum PlayerCharacter
    {
        Wilex = 0,
        Luna = 1
    }
    public enum SpriteID
    {
        Default = -1,   //error condition

        Wilex = 0,
        Luna = 1,
        Keru,
        Aster,
        Leafling,
        Flowerling,
        Shrublet,

        P_Gryphon_Male,
        P_Gryphon_MaleOveralls,
        P_Gryphon_Female,
        P_Gryphon_Stella,

        P_Leafling,
        P_Flowerling,
        P_Shrublet,
        P_Sunnybud,
        P_Sunflower,
        P_MiracleBloom,

        C1_GrizzlyBear_ChildFemaleTown,
        C1_GrizzlyBear_ChildFemaleTemple,
        C1_GrizzlyBear_ChildMaleTown,
        C1_GrizzlyBear_ChildMaleTemple,
        C1_GrizzlyBear_MaleTown,
        C1_GrizzlyBear_MaleTemple,
        C1_GrizzlyBear_MaleTempleMasked,
        C1_GrizzlyBear_FemaleTown,
        C1_GrizzlyBear_FemaleTemple,
        C1_GrizzlyBear_Gourmand,
        C1_GrizzlyBear_Waxwell,

        C1_Rabbit_Aurelia,
        C1_Rabbit_FemaleFatTemple,
        C1_Rabbit_FemaleFatTown,
        C1_Rabbit_FemaleSkinnyTemple,
        C1_Rabbit_FemaleSkinnyTown,
        C1_Rabbit_Hazel,
        C1_Rabbit_MaleFatTemple,
        C1_Rabbit_MaleFatTempleMasked,
        C1_Rabbit_MaleFatTown,
        C1_Rabbit_MaleSkinnyTemple,
        C1_Rabbit_MaleSkinnyTown,
        C1_Rabbit_Pyri,
        C1_Rabbit_Sycamore,
        C1_Rabbit_Torstrum,

        C1_Squirrel_FemaleTemple,
        C1_Squirrel_FemaleTown,
        C1_Squirrel_MaleTemple,
        C1_Squirrel_MaleTempleMasked,
        C1_Squirrel_MaleTown,
        C1_Squirrel_Spruce,

        C1_BurrowTrap,
        C1_Sundew,
        C1_Rockling,
        C1_Honeybud,

        C2_FrogNormal_FemaleBandit,
        C2_FrogNormal_FemaleCitizen,
        C2_FrogNormal_FemaleFoxBandit,
        C2_FrogNormal_FemaleFoxCitizen,
        C2_FrogNormal_MaleBandit,
        C2_FrogNormal_MaleCitizen,
        C2_FrogNormal_MaleFoxBandit,
        C2_FrogNormal_MaleFoxCitizen,
        C2_FrogNormal_Rosette,
        C2_FrogNormal_Bead,

        C2_FrogCrested_Male,
        C2_FrogCrested_Female,

        C2_FrogPuffer_Amethyst,
        C2_FrogPuffer_Aqua,
        C2_FrogPuffer_Ruby,
        C2_FrogPuffer_FemaleBandit,
        C2_FrogPuffer_FemaleCitizen,
        C2_FrogPuffer_MaleBandit,
        C2_FrogPuffer_MaleCitizen,

        C2_FrogSpiky_Anchor,
        C2_FrogSpiky_FemaleBandit,
        C2_FrogSpiky_FemaleCitizen,
        C2_FrogSpiky_Glaive,
        C2_FrogSpiky_Halberd,
        C2_FrogSpiky_MaleBandit,
        C2_FrogSpiky_MaleCitizen,

        C2_Cactupole,
        C2_Sandswimmer,

        C3_Jellyfish_FemaleCommoner,
        C3_Jellyfish_FemaleNoble,
        C3_Jellyfish_MaleCommoner,
        C3_Jellyfish_MaleNoble,
        C3_Jellyfish_Muthi,

        C3_Slime,
        C3_RigidSlime,
        C3_SoftSlime,
        C3_ElementalSlime,
        C3_NormalSlime,
        C3_Slimeworm,
        C3_Slimewalker,
        C3_Slimebloom,
        C3_Sirenfish,

        C4_Flamecrest_AshcrestFemale,
        C4_Flamecrest_AshcrestMale,
        C4_Flamecrest_BlazecrestFemale,
        C4_Flamecrest_BlazecrestMale,
        C4_Flamecrest_EmbercrestFemale,
        C4_Flamecrest_EmbercrestMale,
        C4_Flamecrest_Rogen,
        C4_Flamecrest_Sizzle,

        C4_Flametongue_Ashcrest,
        C4_Flametongue_Embercrest,
        C4_Flametongue_Embra,
        C4_Flametongue_Ferra,
        C4_Flametongue_Islander,

        C4_Lavaswimmer,
        C4_Heatwing,

        C5_Mosquito_Cyano,
        C5_Mosquito_Male,
        C5_Mosquito_Female,

        C5_EyeSpore,
        C5_SpikeShroom,
        C5_Shrouder,
        C5_HoarderFly,

        C6_Crow_FemaleCommoner,
        C6_Crow_FemaleNoble,
        C6_Crow_Iris,
        C6_Crow_MaleCommoner,
        C6_Crow_MaleNoble,

        C6_Hawk_FemaleCommoner,
        C6_Hawk_FemaleNoble,
        C6_Hawk_MaleCommoner,
        C6_Hawk_MaleNoble,
        C6_Hawk_FemaleSoldier,
        C6_Hawk_MaleStriker,
        C6_Hawk_Polaris,
        C6_Hawk_Ilum,
        C6_Hawk_Hoarf,
        C6_Hawk_Sleet,

        C6_PolarBear_FemaleCommoner,
        C6_PolarBear_FemaleNoble,
        C6_PolarBear_MaleCommoner,
        C6_PolarBear_MaleNoble,
        C6_PolarBear_MaleSoldier,
        C6_PolarBear_Arctos,

        C6_Sparrow_FemaleCommoner,
        C6_Sparrow_FemaleNoble,
        C6_Sparrow_FemaleVanguard,
        C6_Sparrow_MaleCommoner,
        C6_Sparrow_MaleNoble,
        C6_Sparrow_MaleVanguard,
        C6_Sparrow_Squalle,
        C6_Sparrow_Lanche,

        C6_Shimmerwing,
        C6_Shieldwing,
        C6_Honeywing,

        C7_Chaintail_Alumi,
        C7_Chaintail_Female,
        C7_Chaintail_Male,
        C7_Chaintail_Palla,
        C7_Chaintail_Rhoda,
        C7_Chaintail_Thallia,

        C7_Plateshell_Cutle,
        C7_Plateshell_Female,
        C7_Plateshell_Male,
        C7_Plateshell_Osmi,

        C7_Speartongue_Female,
        C7_Speartongue_Male,
        C7_Speartongue_Lim,
        C7_Speartongue_Ridi,

        C7_Sawcrest,
        C7_Drillbeak,
        C7_Coiler,

        C8_Hydromander_Cloudmander,
        C8_Hydromander_Watermander,
        C8_Hydromander_Icemander,
        C8_Hydromander_Cyclus,
        C8_Hydromander_Sparchon,
        C8_Hydromander_Meryl,
        C8_Hydromander_Blanca,

        C8_PuffJelly,
        C8_Fluffling,

        C8_CloudJelly,
        C8_WaterJelly,
        C8_IceJelly,
        C8_CrystalCrab,
        C8_CrystalSlug,
        C8_CrystalClam,
        C8_AuroraWing,

        E_Plaguebud_Female,
        E_Plaguebud_Male,
        E_Plaguebud_Pestel,
        E_Plaguebud_Vali,

        E_CursedEye,
        E_StrangeTendril,
        E_Starfish,
        E_DrainBud,
    }

    //this is mostly just a reference
    //Standard is to use these but lowercase
    public enum StandardAnimName
    {
        Idle,
        WeakIdle,   //usually for poisoned entities
        FreezeIdle, //usually a still frame of hurt
        AngryIdle,  //berserk
        DizzyIdle,  //dizzy
        SleepIdle,  //sleep
        Walk,
        Jump,   //air moving upward
        Fall,
        Talk,
        AngryTalk,
        Hurt,   //paralyze will use this?
        Dead,   //not really used in most cases
    }

    public enum CommonTextLine
    {
        HP,
        EP,
        SP,
        Coin,
    }

    //Nonstatic methods
    //Mostly game logic and important coroutines

    //Worldspace Yaw is changed by the camera (so if it faces in different directions everything still makes sense)
    public float GetWorldspaceYaw()
    {
        return worldspaceYaw;
    }

    public void SetWorldspaceYaw(float yaw)
    {
        worldspaceYaw = yaw;
    }

    //Orient the XZ axes with respect to worldspace yaw
    //Y axis is unchanged
    public Vector2[] GetWorldspaceXZ()
    {
        float ws = worldspaceYaw * (Mathf.PI / 180);

        //x, z -> ???
        Vector2[] output = new Vector2[2];

        output[0] = Vector2.right * Mathf.Cos(ws) + Vector2.down * Mathf.Sin(ws);
        output[1] = Vector2.up * Mathf.Cos(ws) + Vector2.right * Mathf.Sin(ws);

        return output;
    }

    //transform with worldspace yaw
    public Vector2 WorldspaceXZTransform(Vector2 input)
    {
        Vector2[] matrix = GetWorldspaceXZ();

        Vector2 output = Vector2.zero;
        output += input.x * matrix[0];
        output += input.y * matrix[1];

        return output;
    }


    public static AnimationController CreateAnimationController(SpriteID spriteID, GameObject parent = null)
    {
        string path;
        if (!spriteID.ToString().Contains("_"))
        {
            path = "Sprites/Characters/Common/" + spriteID + "/AnimCont_" + spriteID;
        } else
        {
            path = "Sprites/Characters/" + spriteID.ToString().Replace("_", "/") + "/AnimCont_" + spriteID;
        }

        GameObject aco;
        if (parent == null)
        {
            aco = Instantiate(Resources.Load<GameObject>(path));
        } else
        {
            aco = Instantiate(Resources.Load<GameObject>(path), parent.transform);
        }

        AnimationController ac = aco.GetComponent<AnimationController>();

        return ac;
    }


    //This takes formatted strings
    public IEnumerator DisplayTextBox(string s, string[] vars = null)
    {
        GameObject textbox = Instantiate(defaultTextbox, Canvas.transform);
        TextboxScript script = textbox.GetComponent<TextboxScript>();
        yield return StartCoroutine(script.CreateText(s));
        Destroy(textbox);
    }

    //standard for battle? (but a few things can use the text file stuff)
    public IEnumerator DisplayTextBox(string s, Vector3 tailPos, string[] vars = null)
    {
        GameObject textbox = Instantiate(defaultTextbox, Canvas.transform);
        TextboxScript script = textbox.GetComponent<TextboxScript>();
        yield return StartCoroutine(script.CreateText(s, tailPos));
        Destroy(textbox);
    }

    public IEnumerator DisplayTextBox(string s, ITextSpeaker speaker, string[] vars = null)
    {
        GameObject textbox = Instantiate(defaultTextbox, Canvas.transform);
        TextboxScript script = textbox.GetComponent<TextboxScript>();
        yield return StartCoroutine(script.CreateText(s, speaker));
        Destroy(textbox);
    }

    //handles text file stuff
    //probably going to be standard almost everywhere
    public IEnumerator DisplayTextBox(string[][] file, int y, ITextSpeaker speaker = null, string[] vars = null)
    {
        yield return StartCoroutine(DisplayTextBox(file, y, 0, speaker, vars));
    }
    public IEnumerator DisplayTextBox(string[][] file, int y, int x, ITextSpeaker speaker = null, string[] vars = null)
    {
        GameObject textbox = Instantiate(defaultTextbox, Canvas.transform);
        TextManager script = textbox.GetComponent<TextManager>();
        //script.backObject.GetComponent<RectTransform>().anchoredPosition = Vector3.down * (75);
        //Debug.Log("B");
        yield return StartCoroutine(script.ShowText(file, y, x, speaker, vars));
        Destroy(textbox);
    }

    //uses the overworld cutscene thing (don't use this in battle, regular cutscenes can use the above one)
    //(so this may not be used very often)
    //(Probably good for simple NPCS that don't need special scripts for interactions)
    public IEnumerator DisplayTextBoxBlocking(string[][] file, int y, ITextSpeaker speaker = null, string[] vars = null)
    {
        yield return StartCoroutine(DisplayTextBoxBlocking(file, y, 0, speaker, vars));
    }
    public IEnumerator DisplayTextBoxBlocking(string[][] file, int y, int x, ITextSpeaker speaker = null, string[] vars = null)
    {
        /*
        inCutscene = true;
        yield return StartCoroutine(DisplayTextBox(file, y, x));
        inCutscene = false;
        */

        //special handling (but a bit hacky)
        //only acts like a cutscene if there are no cutscenes already
        //(otherwise cutscenes attempting to use text boxes won't work because of the queue system)
        if (LastStartedCutscene() != LastEndedCutscene())
        {
            yield return StartCoroutine(DisplayTextBox(file, y, x, speaker, vars));
        }
        else
        {
            yield return StartCoroutine(ExecuteCutscene(DisplayTextBox(file, y, x, speaker, vars)));
        }
    }
    public MinibubbleScript MakeMinibubble()
    {
        GameObject minibubble = Instantiate(MainManager.Instance.defaultMinibubble, MainManager.Instance.Canvas.transform);
        MinibubbleScript mbs = minibubble.GetComponent<MinibubbleScript>();
        return mbs;
    }

    public IEnumerator Pickup(PickupUnion pu)
    {
        if (pu.type == PickupUnion.PickupType.KeyItem && pu.keyItem.type == KeyItem.KeyItemType.CrystalKey)
        {
            if (playerData.CountKeyItemStacking(KeyItem.KeyItemType.CrystalKey) >= 4)
            {
                AwardAchievement(Achievement.ACH_KeyHoarder);
            }
        }

        if (pu.type == PickupUnion.PickupType.Ribbon && pu.ribbon.type == Ribbon.RibbonType.DiamondRibbon)
        {
            AwardAchievement(Achievement.ACH_DiamondRibbon);
        }

        switch (pu.type)
        {
            case PickupUnion.PickupType.None:
                yield break;
            case PickupUnion.PickupType.Coin:
                playerData.coins++;
                if (playerData.coins > PlayerData.MAX_MONEY)
                {
                    playerData.coins = PlayerData.MAX_MONEY;
                }
                yield break;
            case PickupUnion.PickupType.SilverCoin:
                playerData.coins += 5;
                if (playerData.coins > PlayerData.MAX_MONEY)
                {
                    playerData.coins = PlayerData.MAX_MONEY;
                }
                yield break;
            case PickupUnion.PickupType.GoldCoin:
                playerData.coins += 25;
                if (playerData.coins > PlayerData.MAX_MONEY)
                {
                    playerData.coins = PlayerData.MAX_MONEY;
                }
                yield break;
            case PickupUnion.PickupType.Shard:
                playerData.shards++;
                playerData.cumulativeShards++;
                yield return StartCoroutine(GetItemPopupBlocking(pu));
                yield break;
            case PickupUnion.PickupType.Item:
                //playerData.itemInventory.Add(pu.item);
                bool a = true;
                if (pu.item.type != Item.ItemType.None)
                {
                    a = playerData.AddItem(pu.item);
                }
                if (a)
                {
                    yield return StartCoroutine(GetItemPopupBlocking(pu));
                }
                else
                {
                    //Too many items!
                    yield return StartCoroutine(TooManyItemsPopupBlocking(pu));
                }
                yield break;
            case PickupUnion.PickupType.KeyItem:
                if (KeyItem.IsStackable(pu.keyItem) && pu.keyItem.bonusData == 0)
                {
                    Debug.Log("Stackable Key Item Count: 0 to 1");
                    pu.keyItem.bonusData = 1;
                }
                if (pu.keyItem.type != KeyItem.KeyItemType.None)
                {
                    if (KeyItem.IsStackable(pu.keyItem))
                    {
                        playerData.AddKeyItemStacking(pu.keyItem);
                    }
                    else
                    {
                        playerData.AddKeyItem(pu.keyItem);
                    }
                }
                yield return StartCoroutine(GetItemPopupBlocking(pu));
                yield break;
            case PickupUnion.PickupType.Badge:
                if (pu.badge.type != Badge.BadgeType.None)
                {
                    playerData.AddBadge(pu.badge);
                    if (Cheat_TooManyBadges)
                    {
                        playerData.AddBadge(pu.badge);
                    }
                }
                yield return StartCoroutine(GetItemPopupBlocking(pu));
                yield break;
            case PickupUnion.PickupType.Ribbon:
                //playerData.ribbonInventory.Add(pu.ribbon);
                if (pu.ribbon.type != Ribbon.RibbonType.None)
                {
                    playerData.AddRibbon(pu.ribbon);
                }
                yield return StartCoroutine(GetItemPopupBlocking(pu));
                yield break;
            case PickupUnion.PickupType.Misc:
                //Hardcoded what this does
                PlayerData.PlayerDataEntry pde;
                switch (pu.misc)
                {
                    case MiscSprite.MysteryItem:
                        //Transform into the item to give to you
                        Item.ItemType it;
                        while (true)
                        {
                            it = (Item.ItemType)RandomGenerator.GetIntRange(0, 256);
                            if (!Item.GetItemDataEntry(it).isRecipe)
                            {
                                break;
                            }
                        }
                        bool mi = true;
                        pu.type = PickupUnion.PickupType.Item;
                        pu.item = new Item(it, Item.ItemModifier.None);
                        if (pu.item.type != Item.ItemType.None)
                        {
                            mi = playerData.AddItem(pu.item);
                        }
                        Debug.Log(pu.item);
                        if (mi)
                        {
                            yield return StartCoroutine(GetItemPopupBlocking(pu));
                        }
                        else
                        {
                            //Too many items!
                            yield return StartCoroutine(TooManyItemsPopupBlocking(pu));
                        }
                        yield break;
                    case MiscSprite.MysteryRecipe:
                        Item.ItemType rt;
                        while (true)
                        {
                            rt = (Item.ItemType)RandomGenerator.GetIntRange(1, (int)Item.ItemType.EndOfTable);
                            if (Item.GetItemDataEntry(rt).isRecipe)
                            {
                                break;
                            }
                        }
                        bool mr = true;
                        pu.type = PickupUnion.PickupType.Item;
                        pu.item = new Item(rt, Item.ItemModifier.None);
                        if (pu.item.type != Item.ItemType.None)
                        {
                            mr = playerData.AddItem(pu.item);
                        }
                        if (mr)
                        {
                            yield return StartCoroutine(GetItemPopupBlocking(pu));
                        }
                        else
                        {
                            //Too many items!
                            yield return StartCoroutine(TooManyItemsPopupBlocking(pu));
                        }
                        yield break;
                    case MiscSprite.MysteryBadge:
                        pu.badge.type = (Badge.BadgeType)RandomGenerator.GetIntRange(1, (int)Badge.BadgeType.EndOfTable);
                        pu.type = PickupUnion.PickupType.Badge;
                        if (pu.badge.type != Badge.BadgeType.None)
                        {
                            playerData.AddBadge(pu.badge);
                            if (Cheat_TooManyBadges)
                            {
                                playerData.AddBadge(pu.badge);
                            }
                        }
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.MysteryRibbon:
                        pu.ribbon.type = (Ribbon.RibbonType)RandomGenerator.GetIntRange(1, (int)Ribbon.RibbonType.EndOfTable); ;
                        pu.type = PickupUnion.PickupType.Ribbon;
                        if (pu.ribbon.type != Ribbon.RibbonType.None)
                        {
                            playerData.AddRibbon(pu.ribbon);
                        }
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.NoItem:
                    case MiscSprite.NoRecipe:
                    case MiscSprite.NoBadge:
                    case MiscSprite.NoRibbon:
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.AbilitySlash:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
                        if (pde != null) {
                            if (pde.weaponLevel < 0)
                            {
                                pde.weaponLevel = 0;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilitySlash2:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
                        if (pde != null)
                        {
                            if (pde.weaponLevel < 1)
                            {
                                pde.weaponLevel = 1;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilitySlash3:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
                        if (pde != null)
                        {
                            if (pde.weaponLevel < 2)
                            {
                                pde.weaponLevel = 2;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilityAetherize:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
                        if (pde != null)
                        {
                            if (pde.weaponLevel < 2)
                            {
                                pde.weaponLevel = 2;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilityDoubleJump:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
                        if (pde != null)
                        {
                            if (pde.jumpLevel < 1)
                            {
                                pde.jumpLevel = 1;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilitySuperJump:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
                        if (pde != null)
                        {
                            if (pde.jumpLevel < 2)
                            {
                                pde.jumpLevel = 2;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilitySmash:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
                        if (pde != null)
                        {
                            if (pde.weaponLevel < 0)
                            {
                                pde.weaponLevel = 0;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilitySmash2:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
                        if (pde != null)
                        {
                            if (pde.weaponLevel < 1)
                            {
                                pde.weaponLevel = 1;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilitySmash3:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
                        if (pde != null)
                        {
                            if (pde.weaponLevel < 2)
                            {
                                pde.weaponLevel = 2;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilityIlluminate:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
                        if (pde != null)
                        {
                            if (pde.weaponLevel < 2)
                            {
                                pde.weaponLevel = 2;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilityDashHop:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
                        if (pde != null)
                        {
                            if (pde.jumpLevel < 1)
                            {
                                pde.jumpLevel = 1;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.AbilityDig:
                        pde = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
                        if (pde != null)
                        {
                            if (pde.jumpLevel < 2)
                            {
                                pde.jumpLevel = 2;
                                yield return StartCoroutine(GetItemPopupBlocking(pu));
                                yield break;
                            }
                        }
                        yield break;
                    case MiscSprite.Health6:
                        playerData.HealHealth(6);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Health12:
                        playerData.HealHealth(12);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Health30:
                        playerData.HealHealth(30);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Health60:
                        playerData.HealHealth(60);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Energy6:
                        playerData.HealEnergy(6);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Energy12:
                        playerData.HealEnergy(12);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Energy30:
                        playerData.HealEnergy(30);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Energy60:
                        playerData.HealEnergy(60);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Soul6:
                        playerData.HealSoul(6);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Soul12:
                        playerData.HealSoul(12);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Soul30:
                        playerData.HealSoul(30);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.Soul60:
                        playerData.HealSoul(60);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.XP10:
                        playerData.AddXP(10);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.XP25:
                        playerData.AddXP(25);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.XP50:
                        playerData.AddXP(50);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                    case MiscSprite.XP99:
                        playerData.AddXP(99);
                        yield return StartCoroutine(GetItemPopupBlocking(pu));
                        yield break;
                }

                yield return StartCoroutine(GetItemPopupBlocking(pu));
                yield break;
        }
    }
    public IEnumerator GetItemPopup(PickupUnion pu)
    {
        GetItemPopupMenuScript gipms = GetItemPopupMenuScript.BuildMenu(pu);

        bool inpopup = true;
        void MenuExit(object sender, MenuExitEventArgs meea)
        {
            inpopup = false;
        }
        gipms.menuExit += MenuExit;

        yield return new WaitUntil(() => !inpopup);
        gipms.menuExit -= MenuExit;

        gipms.ActiveClear();
        Destroy(gipms.gameObject);
    }
    public IEnumerator GetItemPopupBlocking(PickupUnion pu)
    {
        //special handling (but a bit hacky)
        //only acts like a cutscene if there are no cutscenes already
        //(otherwise cutscenes attempting to give you stuff won't work because of the queue system)
        if (LastStartedCutscene() != LastEndedCutscene())
        {
            yield return StartCoroutine(GetItemPopup(pu));
        }
        else
        {
            yield return StartCoroutine(ExecuteCutscene(GetItemPopup(pu)));
        }
    }
    public IEnumerator TooManyItemsPopup(PickupUnion pu)
    {
        TooManyItemsPopupMenuScript gipms = TooManyItemsPopupMenuScript.BuildMenu(pu);

        bool inpopup = true;
        void MenuExit(object sender, MenuExitEventArgs meea)
        {
            inpopup = false;
        }
        gipms.menuExit += MenuExit;
        yield return new WaitUntil(() => !inpopup);
        gipms.menuExit -= MenuExit;

        //using weird way of doing things

        //use result
        MenuResult result = gipms.GetFullResult(); //gipms.GetFullResult();
        //Debug.Log(result);
        string a = result.subresult.output.ToString();
        int index = -1; //(int)(result.output);
        int.TryParse((a.Split(",")[1]), out index);
        //Debug.Log((a.Split(",")[1]) + " " + index);

        if (index < 0)
        {
            index = 0;  //toss 0
        }

        if (result.subresult.subresult != null)
        {
            //Use item
            PlayerData.PlayerDataEntry pde = (PlayerData.PlayerDataEntry)(result.subresult.subresult.output);

            //putting the item in first
            playerData.itemInventory.Insert(0, pu.item);

            //and then I can do things normally
            Item.UseOutOfBattle(pde, playerData.itemInventory[index], index);
        }
        else
        {
            //Toss item
            if (index == 0)
            {
                //In this case, the new item is not in your inventory yet
                Vector3 position = WorldPlayer.Instance.transform.position + Vector3.up * 0.8f;
                Vector3 velocity = Vector3.up * 3 + WorldPlayer.Instance.FacingVector() * 3;
                WorldCollectibleScript.MakeCollectible(pu, position, velocity);
            }
            else
            {
                //take out a thing
                Item removedItem = playerData.itemInventory[index - 1];
                playerData.itemInventory.RemoveAt(index - 1);

                //put a thing in
                playerData.AddItem(pu.item);

                //toss
                Vector3 position = WorldPlayer.Instance.transform.position + Vector3.up * 0.8f;
                Vector3 velocity = Vector3.up * 3 + WorldPlayer.Instance.FacingVector() * 3;
                WorldCollectibleScript.MakeCollectible(new PickupUnion(removedItem), position, velocity);
            }
        }

        //gipms.Clear();
        gipms.ActiveClear();
        Destroy(gipms.gameObject);
    }
    public IEnumerator TooManyItemsPopupBlocking(PickupUnion pu)
    {
        //special handling (but a bit hacky)
        //only acts like a cutscene if there are no cutscenes already
        //(otherwise cutscenes attempting to give you stuff won't work because of the queue system)
        if (LastStartedCutscene() != LastEndedCutscene())
        {
            yield return StartCoroutine(TooManyItemsPopup(pu));
        }
        else
        {
            yield return StartCoroutine(ExecuteCutscene(TooManyItemsPopup(pu)));
        }
    }

    public void DropCoins(int count, Vector3 position, Vector3 baseVel, float spread)
    {
        //Debug.Log("Drop coins");
        //actual drop count should be kept somewhere between being too low and too high
        int dropCount = count;

        int singleCount = dropCount;
        int fiveCount = 0;
        int quarterCount = 0;

        //Exchange singles for fives
        while (dropCount > 10 && singleCount >= 5)
        {
            singleCount -= 5;
            fiveCount++;
            dropCount = singleCount + fiveCount + quarterCount;
        }

        //Exchange fives for quarters
        while (dropCount > 10 && fiveCount >= 5)
        {
            fiveCount -= 5;
            quarterCount++;
            dropCount = singleCount + fiveCount + quarterCount;
        }

        int dropIndex = 0;
        float dropAngle = 0;

        Vector3 newVel;

        for (int i = 0; i < singleCount; i++)
        {
            dropAngle = (dropIndex / (dropCount + 0f)) * 2 * Mathf.PI;

            newVel = baseVel + spread * Vector3.right * Mathf.Sin(dropAngle) + spread * Vector3.forward * Mathf.Cos(dropAngle);
            WorldCollectibleScript.MakeCollectible(new PickupUnion(PickupUnion.PickupType.Coin), position, newVel);

            dropIndex++;
        }
        for (int i = 0; i < fiveCount; i++)
        {
            dropAngle = (dropIndex / (dropCount + 0f)) * 2 * Mathf.PI;

            newVel = baseVel + spread * Vector3.right * Mathf.Sin(dropAngle) + spread * Vector3.forward * Mathf.Cos(dropAngle);
            WorldCollectibleScript.MakeCollectible(new PickupUnion(PickupUnion.PickupType.SilverCoin), position, newVel);

            dropIndex++;
        }
        for (int i = 0; i < quarterCount; i++)
        {
            dropAngle = (dropIndex / (dropCount + 0f)) * 2 * Mathf.PI;

            newVel = baseVel + spread * Vector3.right * Mathf.Sin(dropAngle) + spread * Vector3.forward * Mathf.Cos(dropAngle);
            WorldCollectibleScript.MakeCollectible(new PickupUnion(PickupUnion.PickupType.GoldCoin), position, newVel);

            dropIndex++;
        }
    }
    public void DropItem(Item.ItemType it, Vector3 position, Vector3 velocity = default)
    {
        if (it == Item.ItemType.None)
        {
            return;
        }
        WorldCollectibleScript.MakeCollectible(new PickupUnion(new Item(it, Item.ItemModifier.None, Item.ItemOrigin.EnemyDrop)), position, velocity);
    }
    public void ThrowExistingCollectible(WorldCollectibleScript wcs, Vector3 position, Vector3 velocity = default)
    {
        wcs.transform.position = position;
        wcs.rb.velocity = velocity;
        wcs.startPos = position;
        wcs.intangible = false;
        wcs.antigravity = false;

        //WorldCollectibleScript.MakeCollectible(new PickupUnion(new Item(it, Item.ItemModifier.None, Item.ItemOrigin.EnemyDrop)), position, velocity);
    }
    public void DropItems(Item.ItemType it, int count, Vector3 position, Vector3 firstDir, Vector3 baseVel, float spread)
    {
        //Debug.Log("Drop coins");
        //actual drop count should be kept somewhere between being too low and too high
        int dropCount = count;

        int dropIndex = 0;
        float dropAngle = 0;
        Vector3 newVel;

        float firstAngle = Mathf.Atan2(firstDir.x, firstDir.z);

        for (int i = 0; i < dropCount; i++)
        {
            dropAngle = (dropIndex / (dropCount + 0f)) * 2 * Mathf.PI + firstAngle;

            newVel = baseVel + spread * Vector3.right * Mathf.Sin(dropAngle) + spread * Vector3.forward * Mathf.Cos(dropAngle);

            if (GetGlobalFlag(GlobalFlag.GF_RandomItemModifiers))
            {
                WorldCollectibleScript.MakeCollectible(new PickupUnion(new Item(it, GlobalItemScript.GetRandomModifier(it), Item.ItemOrigin.EnemyDrop)), position, newVel);
            }
            else
            {
                WorldCollectibleScript.MakeCollectible(new PickupUnion(new Item(it, Item.ItemModifier.None, Item.ItemOrigin.EnemyDrop)), position, newVel);
            }

            dropIndex++;
        }
    }

    public float GetHyperScrollRate()
    {
        return 30;
    }

    /*
    public bool GetGlobalFlag(int index)
    {
        if (index > globalFlags.Length - 1)
        {
            Debug.LogWarning("Attempted to access invalid global flag " + index);
            return false;
        }
        else
        {
            return globalFlags[index];
        }
    }
    */
    public bool GetGlobalFlag(string gfs)
    {
        GlobalFlag gf = GlobalFlag.GF_None;
        Enum.TryParse(gfs, out gf);

        if (gf == GlobalFlag.GF_None)
        {
            Debug.LogWarning("GF_None is being accessed");
            return true;
        }
        if (!globalFlags.ContainsKey(gf))
        {
            return false;
        }
        return globalFlags[gf];
    }
    public bool GetGlobalFlag(GlobalFlag gf)
    {
        if (gf == GlobalFlag.GF_None)
        {
            Debug.LogWarning("GF_None is being accessed");
            return true;
        }
        if (!globalFlags.ContainsKey(gf))
        {
            return false;
        }
        return globalFlags[gf];
    }
    public bool GetAreaFlag(string index)
    {
        if (!areaFlags.ContainsKey(index))
        {
            //Debug.LogWarning("Attempted to access invalid area flag " + index);
            return false;
        } else
        {
            return areaFlags[index];
        }
    }
    public bool GetMapFlag(string index)
    {
        if (!mapScript.flags.ContainsKey(index))
        {
            //Debug.LogWarning("Attempted to access invalid map flag " + index);
            return false;
        }
        else
        {
            return mapScript.flags[index];
        }
    }
    public string GetGlobalVar(GlobalVar gv)
    {
        if (gv == GlobalVar.GV_None)
        {
            Debug.LogWarning("GV_None is being accessed");
            return null;
        }

        if (!globalVars.ContainsKey(gv))
        {
            return null;
        }
        return globalVars[gv];
    }
    public StoryProgress GetStoryProgress()
    {
        StoryProgress sp = StoryProgress.SP_None;
        Enum.TryParse(GetGlobalVar(GlobalVar.GV_StoryProgress), out sp);
        return sp;
    }
    public int GetCurrentChapter()  //note: should increment when the "chapter X" screen appears
    {
        StoryProgress sp = GetStoryProgress();
        return GetCurrentChapter(sp);
    }
    public int GetCurrentChapter(StoryProgress sp)  //note: should increment when the "chapter X" screen appears
    {
        //To do later: StoryProgress switch case
        return 9;
    }
    /*
    public string GetGlobalVar(int index)
    {
        if (index > globalVars.Length - 1)
        {
            Debug.LogWarning("Attempted to access invalid global var " + index);
            return "";
        } else
        {
            return globalVars[index];
        }
    }
    */
    public string GetAreaVar(string index)
    {
        if (!areaVars.ContainsKey(index))
        {
            //Debug.LogWarning("Attempted to access invalid area var " + index);
            return "";
        }
        else
        {
            return areaVars[index];
        }
    }
    public string GetMapVar(string index)
    {
        if (!mapScript.vars.ContainsKey(index))
        {
            //Debug.LogWarning("Attempted to access invalid map var " + index);
            return "";
        }
        else
        {
            return mapScript.vars[index];
        }
    }
    public void SetGlobalFlag(string gfs, bool set = true)
    {
        GlobalFlag gf = GlobalFlag.GF_None;
        Enum.TryParse(gfs, out gf);

        if (gf == GlobalFlag.GF_None)
        {
            Debug.LogWarning("Attempt to set GF_None");
            return;
        }
        globalFlags[gf] = set;
    }
    public void SetGlobalFlag(GlobalFlag gf, bool set = true)
    {
        if (gf == GlobalFlag.GF_None)
        {
            Debug.LogWarning("Attempt to set GF_None");
            return;
        }
        globalFlags[gf] = set;
    }
    /*
    public void SetGlobalFlag(int index, bool set)
    {
        if (index > globalFlags.Length - 1)
        {
            Debug.LogWarning("Attempted to set invalid global flag " + index);
        }
        else
        {
            globalFlags[index] = set;
        }
    }
    */
    public void AwardAchievement(Achievement ac)
    {
        GlobalFlag gf = Enum.Parse<GlobalFlag>("GF_" + ac.ToString());

        if (!GetGlobalFlag(gf))
        {
            //achievement popup
            DisplayAchievementPopup(ac);
        }

        SetGlobalFlag(gf, true);
    }
    public bool CheckAchivement(Achievement ac)
    {
        GlobalFlag gf = Enum.Parse<GlobalFlag>("GF_" + ac.ToString());
        return GetGlobalFlag(gf);
    }
    public void SetAreaFlag(string index, bool set)
    {
        areaFlags[index] = set;
        /*
        if (!areaFlags.ContainsKey(index))
        {
            Debug.LogWarning("Attempted to set invalid area flag " + index);
        }
        else
        {
            areaFlags[index] = set;
        }
        */
    }
    public void SetMapFlag(string index, bool set)
    {
        mapScript.flags[index] = set;
        /*
        if (index > mapScript.flags.Length - 1)
        {
            Debug.LogWarning("Attempted to set invalid map flag " + index);
        }
        else
        {
            mapScript.flags[index] = set;
        }
        */
    }
    public void SetGlobalVar(GlobalVar gv, string set)
    {
        if (gv == GlobalVar.GV_None)
        {
            Debug.LogWarning("Attempt to set GV_None");
            return;
        }
        globalVars[gv] = set;
    }
    /*
    public void SetGlobalVar(int index, string set)
    {
        if (index > globalVars.Length - 1)
        {
            Debug.LogWarning("Attempted to set invalid global var " + index);
        }
        else
        {
            globalVars[index] = set;
        }
    }
    */
    public void SetAreaVar(string index, string set)
    {
        areaVars[index] = set;
        /*
        if (index > areaVars.Length - 1)
        {
            Debug.LogWarning("Attempted to set invalid area var " + index);
        }
        else
        {
            areaVars[index] = set;
        }
        */
    }
    public void SetMapVar(string index, string set)
    {
        mapScript.vars[index] = set;
        /*
        if (index > mapScript.vars.Length - 1)
        {
            Debug.LogWarning("Attempted to set invalid map var " + index);
        }
        else
        {
            mapScript.vars[index] = set;
        }
        */
    }

    public void ResetGlobalFlags()
    {
        globalFlags = new Dictionary<GlobalFlag, bool>(); //new bool[GLOBAL_FLAG_COUNT];
    }
    public void ResetAreaFlags()
    {
        areaFlags = new Dictionary<string, bool>(); //new bool[AREA_FLAG_COUNT];
    }
    public void ResetGlobalVars()
    {
        globalVars = new Dictionary<GlobalVar, string>();
    }
    public void ResetAreaVars()
    {
        areaVars = new Dictionary<string, string>(); //new string[AREA_VAR_COUNT];
    }
    //Map flags and vars are handled per map so we don't really need reset methods

    public GlobalFlag ItemToRecipeFlag(Item.ItemType it)
    {
        GlobalFlag gf = GlobalFlag.GF_None;
        Enum.TryParse<MainManager.GlobalFlag>(("GF_Recipe_" + it.ToString()), true, out gf);
        return gf;
    }
    public bool GetRecipeFlag(Item.ItemType it)
    {
        GlobalFlag gf = ItemToRecipeFlag(it);

        return GetGlobalFlag(gf);
        //return true;
    }
    public void SetRecipeFlag(Item.ItemType it)
    {
        GlobalFlag gf = ItemToRecipeFlag(it);
        SetGlobalFlag(gf, true);
    }

    public GlobalFlag EntityIDToBestiaryFlag(BattleHelper.EntityID eid)
    {
        GlobalFlag gf = GlobalFlag.GF_None;
        Enum.TryParse<MainManager.GlobalFlag>(("GF_Bestiary_" + eid.ToString()), true, out gf);
        return gf;
    }
    //Todo: subindex checking (note: I will probably set this up to work in one direction, i.e. a checks b but b does not check a)
    public bool GetBestiaryFlag(BattleHelper.EntityID eid)
    {
        GlobalFlag gf = EntityIDToBestiaryFlag(eid);

        return GetGlobalFlag(gf);
        //return true;
    }
    //Todo: subindex checking (note: I will probably set this up to work in one direction, i.e. b sets a but a does not check b)
    public void SetBestiaryFlag(BattleHelper.EntityID eid)
    {
        GlobalFlag gf = EntityIDToBestiaryFlag(eid);
        SetGlobalFlag(gf, true);
    }



    //Bottom Left = (0,0)
    public Vector2 WorldPosToCanvasPos(Vector3 wpos)
    {
        Vector2 newPos;

        //l o n g line of code
        //Vector2 ScreenToCanvasScale = new Vector2(Canvas.GetComponent<RectTransform>().rect.width / Screen.width,
        //    Canvas.GetComponent<RectTransform>().rect.height / Screen.height);



        //Debug.Log(Canvas.GetComponent<RectTransform>().rect.width + " " + Canvas.GetComponent<RectTransform>().rect.height + " " + Screen.width + " " + Screen.height);

        //Vector2 ScreenToCanvasScale = new Vector2(800 / Screen.width,
        //    600 / Screen.height);

        newPos = WorldPosToCanvasPosProportion(wpos);
        //Debug.Log(newPos);
        newPos *= new Vector2(Canvas.GetComponent<RectTransform>().rect.width, Canvas.GetComponent<RectTransform>().rect.height);
        //Debug.Log(newPos);

        //newPos *= ScreenToCanvasScale;
        return newPos;
    }
    public bool IsPositionBehindCamera(Vector3 wpos)
    {
        Vector3 offset = wpos - Camera.transform.position;
        return Vector3.Dot(offset, Camera.transform.forward) < 0;
    }
    //Find position of top of screen relative to some starting point
    public Vector3 WorldPosCameraTopEdge(Vector3 wpos)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.camera, wpos);
        screenPos.y = Screen.height;
        Ray r = RectTransformUtility.ScreenPointToRay(Camera.camera, screenPos);

        //Plane containing target point perpendicular to camrea
        Plane p = new Plane(Camera.transform.forward, wpos);

        //ray intersection of ray and plane
        if (p.Raycast(r, out float enter))
        {
            return r.GetPoint(enter);
        }

        return Vector3.positiveInfinity;
    }
    public Vector2 WorldPosToCanvasPosB(Vector3 wpos)   //not sure how this is different from above. Possibly occurs due to anchor positioning acting weird for some reason
    {
        //actually: probably because it is just WorldToScreenPoint since the multiplication and division cancel out
        return WorldPosToCanvasPosProportion(wpos) * new Vector2(Screen.width, Screen.height);
    }
    public Vector2 WorldPosToCanvasPosC(Vector3 wpos)
    {
        return WorldPosToCanvasPosProportion(wpos) * new Vector2(CanvasWidth(), CanvasHeight());
    }
    public static float CanvasHeight()
    {
        return 600;
    }
    public static float CanvasWidth()
    {
        return (Screen.width / (0.0f + Screen.height)) * 600;
    }
    public Vector2 WorldPosToCanvasPosProportion(Vector3 wpos)
    {
        Vector2 newPos;
        newPos = RectTransformUtility.WorldToScreenPoint(Camera.camera, wpos);

        newPos /= new Vector2(Screen.width, Screen.height);
        return newPos;
    }

    public string GetConst(int index)
    {
        return GetConst((GameConst)index);
    }
    public string GetConst(GameConst con)
    {
        switch (con)
        {
            case GameConst.PartyCount:
                return playerData.party.Count + "";
            case GameConst.HPProportion:
                float a = 0;
                int count = playerData.party.Count;
                for (int i = 0; i < count; i++)
                {
                    a += (0.0f + playerData.party[i].hp) / playerData.party[i].maxHP;
                }
                return a / count + "";
            case GameConst.EPProportion:
                float b = (0.0f + playerData.ep) / playerData.maxEP;
                return b + "";
            case GameConst.ItemCount:
                return playerData.itemInventory.Count + "";
            case GameConst.ItemMax:
                return playerData.GetMaxInventorySize() + "";
            case GameConst.ItemProportion:
                int countA = playerData.itemInventory.Count;
                int max = playerData.GetMaxInventorySize();
                return (countA + 0.0f) / max + "";
            case GameConst.Level:
                return playerData.level + "";
            case GameConst.CurrentXP:
                return playerData.exp + "";
            case GameConst.Coins:
                return playerData.coins + "";
            case GameConst.Shard:
                return playerData.shards + "";
            case GameConst.Chapter:
                return GetCurrentChapter() + "";
        }

        return "";
    }

    public string GetTextFromFile(string filePath, int y, int x = 0)
    {
        string s = Resources.Load<TextAsset>(filePath).text;
        if (s == null)
        {
            Debug.LogError("[GetTextFromFile] Null file");
            return "<color,red>Invalid File</color>";
        }

        string[][] parse = CSVParse(s);
        if (parse == null)
        {
            Debug.LogError("[GetTextFromFile] CSV failure");
            return "<color,red>CSV Parsing Failure</color>";
        }
        if (y >= parse.Length)
        {
            Debug.LogError("[GetTextFromFile] Index " + y + " >= length " + parse.Length);
            return "<color,red>Invalid Line: Index " + y + " >= length " + parse.Length + "</color>";
        }
        if (x >= parse[y].Length)
        {
            Debug.LogError("[GetTextFromFile] subline: Index " + x + " >= length " + parse[y].Length);
            return "<color,red>Invalid Subline: Index " + x + " >= length " + parse[y].Length + "</color>";
        }

        return parse[y][x];
    }
    public string GetTextFromFile(string[][] file, int y, int x = 0)
    {
        if (file == null)
        {
            Debug.LogError("[GetTextFromFile] Null file");
            return "<color,red>File is null</color>";
        }
        if (y >= file.Length)
        {
            Debug.LogError("[GetTextFromFile] Index " + y + " >= length " + file.Length);
            return "<color,red>Invalid Line: Index " + y + " >= length " + file.Length + "</color>";
        }
        if (x >= file[y].Length)
        {
            Debug.LogError("[GetTextFromFile] subline: Index " + x + " >= length " + file[y].Length);
            return "<color,red>Invalid Subline: Index " + x + " >= length " + file[y].Length + "</color>";
        }

        return file[y][x];
    }
    public static string[][] GetAllTextFromFile(string filePath)
    {
        TextAsset ta = Resources.Load<TextAsset>(filePath);
        if (ta == null)
        {
            return null;
        }
        string s = ta.text;
        if (s == null)
        {
            string[][] output = new string[1][];
            output[0] = new string[1];
            output[0][0] = "<color,red>Invalid File</color>";
            Debug.LogError("[GetAllTextFromFile] File path " + filePath + " could not be read");
            return output;
        }

        string[][] parse = CSVParse(s);
        return parse;
    }
    public static string[][] GetMapText(MapID mapName)
    {
        return GetAllTextFromFile(GetMapTextPath(mapName));
    }
    public static string GetAreaName(string worldLocation)
    {
        Enum.TryParse(worldLocation, out WorldLocation wl);
        return GetAreaName(wl);
    }
    public static string GetAreaName(WorldLocation worldLocation)
    {
        return worldLocation.ToString();
    }
    public static string GetAreaDesc(string worldLocation)
    {
        Enum.TryParse(worldLocation, out WorldLocation wl);
        return GetAreaDesc(wl);
    }
    public static string GetAreaDesc(WorldLocation worldLocation)
    {
        return worldLocation.ToString() + " desc";
    }
    public static string GetMapName(MapID mapName)
    {
        return mapName.ToString();
    }

    public string GetButtonString(InputManager.Button b)
    {
        return InputManager.GetButtonString(b);
    }


    public void PitReset()
    {
        playerData = new PlayerData(BattleHelper.EntityID.Wilex, BattleHelper.EntityID.Luna);
        playerData.AddBadge(new Badge(Badge.BadgeType.SuperCurse));
        playerData.AddBadge(new Badge(Badge.BadgeType.UltraCurse));
        playerData.AddBadge(new Badge(Badge.BadgeType.MegaCurse));
        playerData.AddRibbon(new Ribbon(Ribbon.RibbonType.SharpRibbon));
        playerData.AddRibbon(new Ribbon(Ribbon.RibbonType.SafetyRibbon));

        playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).ribbon = playerData.ribbonInventory[0];
        playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna).ribbon = playerData.ribbonInventory[1];

        //Give you all the shields
        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Greed))
        {
            playerData.AddBadge(new Badge(Badge.BadgeType.RiskyShield));
            playerData.AddBadge(new Badge(Badge.BadgeType.ProtectiveShield));
            playerData.AddBadge(new Badge(Badge.BadgeType.PerfectShield));
            playerData.AddBadge(new Badge(Badge.BadgeType.FirstShield));
            playerData.AddBadge(new Badge(Badge.BadgeType.EnergyShield));
            playerData.AddBadge(new Badge(Badge.BadgeType.AgileShield));
            playerData.AddBadge(new Badge(Badge.BadgeType.SpiritShield));
            playerData.AddBadge(new Badge(Badge.BadgeType.DarkShield));
            playerData.AddBadge(new Badge(Badge.BadgeType.NullShield));
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Wrath))
        {
            playerData.AddBadge(new Badge(Badge.BadgeType.RagesPower));
        }

        playerData.UpdateMaxStats();
    }

    public void OnGUI()
    {
        if (InputManager.Instance.disableControl)
        {
            return;
        }

        if (!Cheat_CheatMenuAvailable)
        {
            return;
        }

        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        { 
            if (e.character == '~')
            {
                CheatMenu.BuildMenu();
            }
        }
    }


    //move to a start game script?
    //remove debug stuff later
    public void Awake()
    {
        //target frame rate is ignored if nonzero?
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;   //settings overwrites this?
        DontDestroyOnLoad(gameObject); //keep MainManager active at all times
        LoadBaseAssets();

        ResetGlobalFlags();
        ResetGlobalVars();
        ResetAreaFlags();
        ResetAreaVars();

        //nextBattle = new EncounterData(BattleHelper.EntityID.DebugEntity, BattleHelper.EntityID.DebugEntityA, BattleHelper.EntityID.DebugEntityB);
        nextBattle = null;
        mapScript = FindObjectOfType<MapScript>();

        curOverworldHUD = Instantiate(overworldHUD, Canvas.transform).GetComponent<OverworldHUDScript>();
        curOverworldHUD.Build();

        /*
        playerData.AddToFull(BattleHelper.EntityID.Knight);
        playerData.AddToFull(BattleHelper.EntityID.Mage);
        playerData.TryAddToCurrent(BattleHelper.EntityID.Knight);
        playerData.TryAddToCurrent(BattleHelper.EntityID.Mage);
        */

        //Debug item inventory

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot));
        playerData.itemInventory.Add(new Item(Item.ItemType.BerryCarrot));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleDrop));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleDrop));
        playerData.itemInventory.Add(new Item(Item.ItemType.CarrotSalad));
        playerData.itemInventory.Add(new Item(Item.ItemType.BigMistake));
        playerData.itemInventory.Add(new Item(Item.ItemType.PowerJam));
        playerData.itemInventory.Add(new Item(Item.ItemType.BerryCandy));
        playerData.itemInventory.Add(new Item(Item.ItemType.LightSeed));
        playerData.itemInventory.Add(new Item(Item.ItemType.LightSeed));
        playerData.itemInventory.Add(new Item(Item.ItemType.PepperNeedle));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.VoidCone));
        playerData.itemInventory.Add(new Item(Item.ItemType.FlamingTea));
        playerData.itemInventory.Add(new Item(Item.ItemType.RubyJuice));
        playerData.itemInventory.Add(new Item(Item.ItemType.VoidCone));
        playerData.itemInventory.Add(new Item(Item.ItemType.RubyJuice));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.ThornBundle));
        playerData.itemInventory.Add(new Item(Item.ItemType.Lemonade));
        playerData.itemInventory.Add(new Item(Item.ItemType.SlimeBomb));
        playerData.itemInventory.Add(new Item(Item.ItemType.PepperNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.DragonShake));
        playerData.itemInventory.Add(new Item(Item.ItemType.StickySpore));
        playerData.itemInventory.Add(new Item(Item.ItemType.SnowyMeringue));
        playerData.itemInventory.Add(new Item(Item.ItemType.PeachPie));
        playerData.itemInventory.Add(new Item(Item.ItemType.MintSpikes));
        playerData.itemInventory.Add(new Item(Item.ItemType.CrystalMeringue));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleCaramel));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarCaramel));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.SlimeBomb));
        playerData.itemInventory.Add(new Item(Item.ItemType.PepperNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.StickySpore));
        playerData.itemInventory.Add(new Item(Item.ItemType.SlimeBomb));
        playerData.itemInventory.Add(new Item(Item.ItemType.PepperNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.StickySpore));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.StrangeSalad));
        playerData.itemInventory.Add(new Item(Item.ItemType.MosquitoNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.DrainSeed));
        playerData.itemInventory.Add(new Item(Item.ItemType.BoltSeed));
        playerData.itemInventory.Add(new Item(Item.ItemType.MetalSalad));
        playerData.itemInventory.Add(new Item(Item.ItemType.HoneyCaramel));
        playerData.itemInventory.Add(new Item(Item.ItemType.GoldRabbitBun));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.GoldenSalad));
        playerData.itemInventory.Add(new Item(Item.ItemType.PolarCaramel));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleShroom));
        playerData.itemInventory.Add(new Item(Item.ItemType.StrangeMushroom));
        playerData.itemInventory.Add(new Item(Item.ItemType.GoldRabbitBun));
        playerData.itemInventory.Add(new Item(Item.ItemType.RabbitBun));
        playerData.itemInventory.Add(new Item(Item.ItemType.RoyalCandy));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleCrystal));
        playerData.itemInventory.Add(new Item(Item.ItemType.GoldOmelette));

        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleShroom));
        playerData.itemInventory.Add(new Item(Item.ItemType.StrangeMushroom));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleShroom));
        playerData.itemInventory.Add(new Item(Item.ItemType.StrangeMushroom));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.ConversionShake));
        playerData.itemInventory.Add(new Item(Item.ItemType.InversionStew));
        playerData.itemInventory.Add(new Item(Item.ItemType.BoosterShake));
        playerData.itemInventory.Add(new Item(Item.ItemType.DiluteShake));
        playerData.itemInventory.Add(new Item(Item.ItemType.ThickShake));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDessert));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDinner));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDessert));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDinner));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDessert));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDinner));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDessert));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDinner));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDessert));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDinner));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        playerData.itemInventory.Add(new Item(Item.ItemType.StellarAether));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.TwinCarrot));
        playerData.itemInventory.Add(new Item(Item.ItemType.TwinCake));
        playerData.itemInventory.Add(new Item(Item.ItemType.TwinCarrot));
        playerData.itemInventory.Add(new Item(Item.ItemType.TwinCarrot));
        playerData.itemInventory.Add(new Item(Item.ItemType.SlimeBomb));
        playerData.itemInventory.Add(new Item(Item.ItemType.SlimeBomb));
        playerData.itemInventory.Add(new Item(Item.ItemType.SlimeBomb));
        playerData.itemInventory.Add(new Item(Item.ItemType.SlimeBomb));
        playerData.itemInventory.Add(new Item(Item.ItemType.GoldNut));
        playerData.itemInventory.Add(new Item(Item.ItemType.GoldBomb));
        playerData.itemInventory.Add(new Item(Item.ItemType.PlagueRoot));
        playerData.itemInventory.Add(new Item(Item.ItemType.FlashBud));
        playerData.itemInventory.Add(new Item(Item.ItemType.SunSeed));
        playerData.itemInventory.Add(new Item(Item.ItemType.SunMuffin));
        */

        //playerData.itemInventory.Add(new Item(Item.ItemType.GoldNut));


        //playerData.keyInventory.Add(new KeyItem(KeyItem.KeyItemType.WoodKey, 2));
        /*
        playerData.keyInventory.Add(new KeyItem(KeyItem.KeyItemType.PlainCandle));
        playerData.keyInventory.Add(new KeyItem(KeyItem.KeyItemType.PlainCandle));
        playerData.keyInventory.Add(new KeyItem(KeyItem.KeyItemType.PlainCandle));
        */

        /*
        for (int i = 1; i < (int)(KeyItem.KeyItemType.EndOfTable); i++)
        {
            playerData.keyInventory.Add(new KeyItem((KeyItem.KeyItemType)i));
        }
        */

        //Sus
        /*
        for (int i = 1; i < (int)Item.ItemType.EndOfTable; i++)
        {
            playerData.itemInventory.Add(new Item((Item.ItemType)i, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, i));
        }
        */

        //playerData.itemInventory.Add(new Item(Item.ItemType.BerrySyrup));
        //playerData.itemInventory.Add(new Item(Item.ItemType.SweetBerry));

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.RoyalGolden));
        playerData.itemInventory.Add(new Item(Item.ItemType.RoyalGolden));
        playerData.itemInventory.Add(new Item(Item.ItemType.RoyalGolden));
        playerData.itemInventory.Add(new Item(Item.ItemType.RoyalGolden));
        playerData.itemInventory.Add(new Item(Item.ItemType.RoyalGolden));
        playerData.itemInventory.Add(new Item(Item.ItemType.SupremeDinner));
        playerData.itemInventory.Add(new Item(Item.ItemType.GoldenSalad));
        playerData.itemInventory.Add(new Item(Item.ItemType.GoldenSalad));

        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleShroom));
        */


        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 1));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 2));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 3));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 4));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 5));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 6));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 7));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 8));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 9));
        playerData.itemInventory.Add(new Item(Item.ItemType.Carrot, Item.ItemModifier.None, Item.ItemOrigin.Cheating, 0, 10));
        */

        /*
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleNeedle));
        playerData.itemInventory.Add(new Item(Item.ItemType.MiracleShroom));
        playerData.itemInventory.Add(new Item(Item.ItemType.TwinCarrot));
        playerData.itemInventory.Add(new Item(Item.ItemType.TwinCarrot));
        */



        /*
        for (int i = 1; i < (int)(Badge.BadgeType.EndOfTable); i++)
        {
            playerData.badgeInventory.Add(new Badge((Badge.BadgeType)i, i));
        }

        for (int i = 1; i < (int)(Ribbon.RibbonType.EndOfTable); i++)
        {
            playerData.ribbonInventory.Add(new Ribbon((Ribbon.RibbonType)i, i));
        }

        playerData.charmEffects.Add(new CharmEffect(CharmEffect.CharmType.Attack, 5, 2, 5));
        playerData.charmEffects.Add(new CharmEffect(CharmEffect.CharmType.Fortune, 3, 5, 0));
        playerData.innEffects.Add(new InnEffect(InnEffect.InnType.Health, 2));
        */

        //playerData.itemInventory.Add(new Item(Item.ItemType.DebugAutoRevive));
        //playerData.itemInventory.Add(new Item(Item.ItemType.DebugAutoRevive));

        //inputCircleBuffer = new CircleBuffer<InputSnapshot>(60);

        interactTriggers = new List<InteractTrigger>();

        //cutsceneQueue = new List<CutsceneScript>();
    }
    public void Start()
    {

        //PromptBoxMenu.BuildMenu(new string[] { "Press A", "Press B", "Don't press C" }, new string[] { "Press B", "2", "3" });
        //OWItemBoxMenu.BuildMenu();
        //StartCoroutine(DisplayTextBox("<next><next><scroll,1,true>Testing some special <next,true><scroll,1,false>Oops, got interrupted the<next,true>I did it again! Hopefully...<end>"));
        //StartCoroutine(DisplayTextBox("Do you <scroll,0.1><rainbow>like</rainbow><scroll,1> bananas?<prompt,Yes,0,No,1,Third Option,2,1><next>This text box doesn't actually support special dialogue for answering <wavy,0,0,0,0>that</wavy> <scramble>question</scramble> <scramble><wavy>yet</wavy></scramble><next>Noooooooo"));
        testTextFile = new string[10][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];
        testTextFile[7] = new string[1];
        testTextFile[8] = new string[1];
        testTextFile[0][0] = "Give me a healing item please!<itemMenu><next><branch,neqi,arg0,SupremeDinner,1><removeitem,arg1><goto,2>";
        testTextFile[1][0] = "Wrong item, try again!<itemMenu><next><branch,neqi,arg0,SupremeDinner,1><goto,2>";
        testTextFile[2][0] = "Correct! Now try some other stuff<prompt,Yes,4,No,5,Third Option,2,1><next><goto,arg><next>Amogus<next>Do do do do do do do <wait,1>do do do";
        testTextFile[3][0] = "<rainbow>Sussy tattle</rainbow> Wow look, <buttonsprite,A> an item: <itemsprite,GoldenCarrot> 1234567890<next><rainbow>Sussy tattle</rainbow> Wow look, <buttonsprite,A> an item: <itemsprite,AetherCarrot><itemsprite,GoldenCarrot> 1234567890";
        testTextFile[4][0] = "What does the end tag do?<next>Let's find out...<next>End tag incoming: <end>Amogus<next>Ok we are after the end tag";
        testTextFile[5][0] = "<wavy,1,1,1,0>Woah all this text is wavy at the same time. Pretty <rainbow>sus</rainbow> if you ask me</wavy><next>Can you <wavy><outline,red>outline <infinity><star> <rainbow> a <infinity><star> rainbow</rainbow> text?</outline></wavy>. How about <wavy><itemsprite,GoldenCarrot><underlay,red>underlays? <infinity><star> <rainbow><itemsprite,GoldenCarrot>a <infinity><star> Do they work?</rainbow></underlay></wavy> Now try <metallic,#ffc040>metal <infinity><star> <rainbow>a <infinity><star> stuff</rainbow></metallic> and iridescent <iridescent,#606060>text with <infinity><star> <rainbow>a <infinity><star> rainbows</rainbow></iridescent><zerospace><zerospace><next>Do all the fadeins work?<fadeinappear>A<itemsprite,GoldenCarrot></fadeinappear><fadeinwave>A<itemsprite,GoldenCarrot></fadeinwave><fadeinspin>A<itemsprite,GoldenCarrot></fadeinspin><fadeingrow>A<itemsprite,GoldenCarrot></fadeingrow><fadeinshrink>A<itemsprite,GoldenCarrot></fadeinshrink><next>Amogus";
        testTextFile[6][0] = "Menu result is <var>";
        testTextFile[7][0] = "Testing an item inventory<dataget,arg,0><itemMenu,arg,pairs><next>Menu result is <rainbow><arg></rainbow>Paddingpaddingpadding<datasend,sus>";
        //testTextFile[8][0] = "Buttons:  <button,A> <button,B> <button,Z> <button,Y> <button,R> <button,Start> <button,Down> <button,Up> <button,Left> <button,Right> stuff <next>Testing a menu of arbitrary stuff<dataget,arg,0><genericmenu,arg,6,true,true,true,true,true><next>Menu result is <rainbow><arg></rainbow>Paddingpaddingpadding<datasend,sus>";
        //testTextFile[8][0] = "<jump>Jumpy jump text</jump><jump,1,2>Jump longer</jump><jump,0.5,1>Jump smaller</jump> <next><fadeinwave>Fade in wave</fadeinwave><fadeinwave,5,10,12,0,0>Fade in wave bigger</fadeinwave><fadeinwave,5,10,0,0,0>Fall in</fadeinwave><next><fadeinshrink>Fade in Shrink, </fadeinshrink><fadeinshrink,10,5>Bigger fade in shrink</fadeinshrink><next><fadeingrow>Fade in Grow, </fadeingrow><fadeingrow,10>Fade in grow slower<next><fadeinspin>Fade in spin, </fadeinspin><fadeinspin,10,5>Fade in spin more</fadeinspin><next><fadeinappear>Fade in appear</fadeinappear> <fadeinappear,1,1,0,0,0,0,0>Fade in colored</fadeinappear>";
        //testTextFile[8][0] = "Main text with a minibubble <minibubble,false,Amogussy,1><next><minibubble,false,Amogussy,-1>Oh no this minibubble is gonna die<killminibubbles><next>Main text with a detached minibubble<minibubble,true,Sus sus sus sus sus sus sus,2><minibubble,true,Other text from another place,3><minibubble,true,Third mini bubble,25|0|0><waitminibubbles><next>Key item menu<keyitemmenu><next>Number menu<numbermenu,0,0,999><next>Enter some text<textentrymenu,16,sus>";

        //testTextFile[8][0] = "<boxstyle,outline><boxcolor,#ff0000,#00ff00>facing test<face,w,-100,0,0><face,w,l><face,l,w><emote,w,angryfizzle><minibubble,true,Keru says stuff,k,-1>Box style testing<next><boxstyle>Test 0<next><boxstyle,darkoutline><boxcolor,#ffff00>Test 2<next><boxstyle,fancyoutline>Test 3<next><boxstyle,shaded>Test 4<next><boxstyle,paper>Test 5<next><boxstyle,beads>Test 6<next><boxstyle,system>Test 7<next><sign><minibubble,false,l1,-1>Sign test<next><system>System test";

        testTextFile[8][0] = "<scramble>A lot of scrambled text for testing scramble tag</scramble> <condcut,hparty,wilex>Cut <condcut,nhkeru>true</condcut><boxstyle,outline></condcut><condcut,nhkeru>Cut false<boxstyle,outline></condcut>Condend false<condend,nhkeru>Condend true<condend,hkeru>After condend true";

        //testFile[0][0] = "<globalflag,0> <const,ItemProportion> item proportion!<itemMenu><next><removeitem,arg><branch,eqi,arg,cancel,2><goto,0>"; 
        //testFile[1][0] = "Flooop";
        //testFile[2][0] = "Yorp<globalvar,0>";
        //testFile[0][0] = "<globalflag,0> <removeitem,DebugHeal,false,true> <removeitem,0> <const,ItemProportion> item proportion!<itemMenu><next><goto,0>";
        //testFile[0][0] = "Let's check some stuff.<prompt,Yes,0,No,1,Third Option,2,1><next><set,arg,1><set,globalvar,0,0><setvar,globalvar,0,arg><setvar,arg,globalvar,0><setvar,globalvar,1,globalvar,0><branch,gtei,globalvar,1,0.5,1>Florp";
        //testFile[0][0] = "Do you <scroll,0.1><rainbow>like</rainbow><scroll,1> bananas?<prompt,Yes,0,No,1,Third Option,2,1><next><branch,gtei,const,HPProportion,0.9,2>Very unhealthy!";
        //testFile[1][0] = "Let's try something different!<next><scroll,1>Woooooooooooooooooooooooooooooooooooooooooooooooop<next><scroll,1>Bloooooooooooooooooooooooooop<next><goto,1>";
        //testFile[2][0] = "You're very wrong!!!";
        //StartCoroutine(DisplayTextBox(testFile, 0, 0));

        /*
        RepresentativePool poolTest = new RepresentativePool(new float[]{ 1/2f, 1/3f, 1/6f });
        for (int i = 0; i < 15; i++)
        {
            Debug.Log(poolTest.Get());
        }
        */
        //inputCircleBuffer = new CircleBuffer<InputSnapshot>(60);

        ReturnToStartMenu();
    }

    public void LoadWeaponColors()
    {
        //0,1,2,3 = sword colors
        //4,5,6,7 = hammer colors
        //note that 0,4 shouldn't be used ("error colors")
        weaponColors = new Color[8][];

        //4 entries per table
        //(outline), dark, medium, light
        //swords only have light being the highlight, while the hammer can have a big region of the light color

        weaponColors[0] = new Color[] { new Color(0f, 0f, 0f, 0), new Color(0f, 0f, 0f, 0), new Color(0f, 0f, 0f, 0), new Color(0f, 0f, 0f, 0) };
        weaponColors[1] = new Color[] { new Color(0.42f, 0.34f, 0.24f, 1), new Color(0.54f, 0.44f, 0.32f, 1), new Color(0.68f, 0.57f, 0.43f, 1), new Color(0.84f, 0.74f, 0.62f, 1) };
        weaponColors[2] = new Color[] { new Color(0.48f, 0.51f, 0.56f, 1), new Color(0.74f, 0.75f, 0.76f, 1), new Color(0.87f, 0.88f, 0.89f, 1), new Color(1f, 1f, 1f, 1) };
        weaponColors[3] = new Color[] { new Color(0.4f, -2f, 0f, 1), new Color(0.65f, 0f, 0.1f, 1), new Color(0.8f, 0f, 0.15f, 1), new Color(1f, 0.2f, 0.3f, 1) };
        weaponColors[4] = new Color[] { new Color(0f, 0f, 0f, 0), new Color(0f, 0f, 0f, 0), new Color(0f, 0f, 0f, 0), new Color(0f, 0f, 0f, 0) };
        weaponColors[5] = new Color[] { new Color(0.42f, 0.34f, 0.24f, 1), new Color(0.54f, 0.44f, 0.32f, 1), new Color(0.68f, 0.57f, 0.43f, 1), new Color(0.84f, 0.74f, 0.62f, 1) };
        weaponColors[6] = new Color[] { new Color(0.48f, 0.51f, 0.56f, 0), new Color(0.74f, 0.75f, 0.76f, 1), new Color(0.87f, 0.88f, 0.89f, 1), new Color(1f, 1f, 1f, 1) };
        weaponColors[7] = new Color[] { new Color(0.53f, 0.46f, 0.22f, 1), new Color(1f, 0.9f, 0.5f, 1), new Color(1.3f, 1.2f, 0.6f, 1), new Color(1.4f, 1.3f, 0.82f, 1) };
    }

    public void WeaponColorUpdate()
    {
        if (weaponColors == null || weaponColors.Length < 6)
        {
            LoadWeaponColors();
        }

        PlayerData.PlayerDataEntry wilex = null;
        PlayerData.PlayerDataEntry luna = null;
        PlayerData pd = null;

        if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
        {
            if (BattleControl.Instance == null)
            {
                return;
            }
            pd = BattleControl.Instance.playerData;
            if (pd == null)
            {
                return;
            }

            wilex = BattleControl.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
            luna = BattleControl.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
        }
        else
        {
            pd = MainManager.Instance.playerData;
            if (pd == null)
            {
                return;
            }

            wilex = MainManager.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
            luna = MainManager.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
        }

        if (wilex != null)
        {
            Color[] sc = weaponColors[wilex.weaponLevel + 1];

            Shader.SetGlobalVector("_WWeaponColorA", sc[0]);
            Shader.SetGlobalVector("_WWeaponColorB", sc[1]);
            Shader.SetGlobalVector("_WWeaponColorC", sc[2]);
            Shader.SetGlobalVector("_WWeaponColorD", sc[3]);
        }

        if (luna != null)
        {
            Color[] hc = weaponColors[luna.weaponLevel + 5];

            Shader.SetGlobalVector("_LWeaponColorA", hc[0]);
            Shader.SetGlobalVector("_LWeaponColorB", hc[1]);
            Shader.SetGlobalVector("_LWeaponColorC", hc[2]);
            Shader.SetGlobalVector("_LWeaponColorD", hc[3]);
        }
    }

    public void Update()
    {
        float targetTimeScale = 1;

        if (Cheat_FastTimeScale)
        {
            targetTimeScale *= 2;
        }
        if (Cheat_SlowTimeScale)
        {
            targetTimeScale *= 0.5f;
        }
        if (Cheat_SuperFastTimeScale)
        {
            targetTimeScale *= 8;
        }
        if (Cheat_SuperSlowTimeScale)
        {
            targetTimeScale *= 0.125f;
        }
        if (Cheat_Halt)
        {
            targetTimeScale *= 0;
        }

        if (Time.timeScale != targetTimeScale)
        {
            Time.timeScale = targetTimeScale;
        }

        frameCounter++;
        frameTime += Time.deltaTime;
        if (frameTime > 0.25f)
        {
            lastCalculatedFPS = (frameCounter / frameTime);
            frameCounter = 0;
            frameTime = 0;
        }

        if (gameOverPlayerData != null)
        {
            if (worldMode == WorldMode.Overworld)
            {
                gameOverPlayerData = null;
            }
        }

        if (worldMode == WorldMode.Overworld)
        {
            playerData.party[0].timeInFront += Time.deltaTime;
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Greed))
        {
            greedPartialCoins += 180 * Time.deltaTime;

            if (worldMode == WorldMode.Battle)
            {
                if (BattleControl.Instance != null)
                {
                    BattleControl.Instance.playerData.coins += (int)(greedPartialCoins);
                    greedPartialCoins -= (int)greedPartialCoins;
                    if (BattleControl.Instance.playerData.coins > PlayerData.MAX_MONEY)
                    {
                        BattleControl.Instance.playerData.coins = PlayerData.MAX_MONEY;
                    }
                }
            } else
            {
                playerData.coins += (int)(greedPartialCoins);
                greedPartialCoins -= (int)greedPartialCoins;
                if (playerData.coins > PlayerData.MAX_MONEY)
                {
                    playerData.coins = PlayerData.MAX_MONEY;
                }
            }
        }

        GlobalRibbonScript.Instance.RibbonColorUpdate();
        WeaponColorUpdate();

        if (curOverworldHUD != null)
        {
            if (hudShowTime > 0)
            {
                hudShowTime -= Time.deltaTime;
            } else
            {
                hudShowTime = 0;
            }

            bool coinChange = lastCoinCount != playerData.coins;
            lastCoinCount = playerData.coins;

            if (coinChange)
            {
                hudShowTime = HUD_MIN_SHOW_TIME;
            }

            if (InputManager.GetButtonDown(InputManager.Button.R))
            {
                if (hudShowTime > 0)
                {
                    hudShowTime = 0;
                } else
                {
                    hudShowTime = HUD_BUTTON_SHOW_TIME;
                }
            }


            float idleTime = 0;
            if (WorldPlayer.Instance != null)
            {
                idleTime = WorldPlayer.Instance.idleTime;
            }

            //Debug.Log(showCoins + " " + (hudShowTime) + " " + (idleTime));
            if (showCoins || hudShowTime > 0 || idleTime > 2)
            {
                curOverworldHUD.settableFadeIn = 1;
            }
            else
            {
                curOverworldHUD.settableFadeIn = 0;
            }
        }

        /*
        if (Time.time % 1 > 0.9f)
        {
            Debug.Log(1 / Time.deltaTime);
        }
        */
        /*
        if (Time.time > 1 && Time.time < 1.1f && !mapScript.halted)
        {
            StartCoroutine(EnterBattle());
        }
        */

        if (worldMode != WorldMode.Start)
        {
            if (playerData != null)
            {
                playTime += Time.deltaTime;
            }
        }

        if (worldPlayer == null && worldMode == WorldMode.Overworld)
        {
            worldPlayer = FindObjectOfType<WorldPlayer>();
        }

        if (concealedVolume != null)
        {
            //Hidden areas only exist in the overworld
            if (worldPlayer != null && worldMode == WorldMode.Overworld)
            {
                float concealedWeight = worldPlayer.concealedTime * 5;
                if (concealedWeight > 1)
                {
                    concealedWeight = 1;
                }
                concealedVolume.weight = concealedWeight;
            }
            else
            {
                concealedVolume.weight = 0;
            }
        }

        //note: grounded condition won't work while paused because the collision system is disabled
        //note: game over script runs the pause menu in that case
        if (isPaused && worldMode == WorldMode.Overworld)
        {
            if (InputManager.GetButtonDown(InputManager.Button.Start))
            {
                //isPaused = false;
                pauseMenuScript.Unpause();
            }
        }
        else
        {
            if (!inCutscene && !mapHalted && worldMode == WorldMode.Overworld && (worldPlayer == null || worldPlayer.IsGrounded() || worldPlayer.GetActionState() == WorldPlayer.ActionState.NoClip))
            {
                //global cutscenes can happen anywhere
                if (!StartGlobalCutscenes())
                {
                    if (InputManager.GetButtonDown(InputManager.Button.Start))
                    {
                        if (curOverworldHUD != null)
                        {
                            curOverworldHUD.SetFadeDirectly(0);
                        }
                        isPaused = true;
                        pauseMenuScript = Pause_SectionBase.buildMenu();
                    }
                }
            }
        }
    }

    public bool StartGlobalCutscenes()
    {
        //todo: make it a real file
        string[][] fileCodeText = new string[7][];
        fileCodeText[0] = new string[1];
        fileCodeText[0][0] = "<system>File Code Greed: Your Max HP is capped at a low value, but you have infinite money and start with many Shield badges. Many badges relating to health or coins will be replaced with other badges.";
        fileCodeText[1] = new string[1];
        fileCodeText[1][0] = "<system>File Code Envy: You start with 6 SP and level ups can only increase your SP by 0.5. However, all badges cost 1 SP at most and all Soul Moves cost half as much.";
        fileCodeText[2] = new string[1];
        fileCodeText[2][0] = "<system>File Code Gluttony: You can hold twice as many items, but items are only half as strong.";
        fileCodeText[3] = new string[1];
        fileCodeText[3][0] = "<system>File Code Wrath: The character in front is permanently Berserk. However, you can still switch places with them with <button,z> to change who is Berserk in front.";
        fileCodeText[4] = new string[1];
        fileCodeText[4][0] = "<system>File Code Sloth: You can only move once every 2 turns, but you regenerate SE at 6 times the normal rate and Resting will give you 3 times as much Soul Energy, as well as any resources from Rest Effects.";
        fileCodeText[5] = new string[1];
        fileCodeText[5][0] = "<system>File Code Pride: Failed action commands for attacking moves deal half damage, and failing to block causes you to take 50% more damage. Action commands and blocking are more difficult.";
        fileCodeText[6] = new string[1];
        fileCodeText[6][0] = "<system>File Code Randomizer: Randomizes the locations of badges and ribbons.";

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Greed) && !GetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Greed))
        {
            StartCoroutine(DisplayTextBoxBlocking(fileCodeText, 0));
            SetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Greed);
            return true;
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Envy) && !GetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Envy))
        {
            StartCoroutine(DisplayTextBoxBlocking(fileCodeText, 1));
            SetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Envy);
            return true;
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Gluttony) && !GetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Gluttony))
        {
            StartCoroutine(DisplayTextBoxBlocking(fileCodeText, 2));
            SetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Gluttony);
            return true;
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Wrath) && !GetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Wrath))
        {
            StartCoroutine(DisplayTextBoxBlocking(fileCodeText, 3));
            SetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Wrath);
            return true;
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Sloth) && !GetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Sloth))
        {
            StartCoroutine(DisplayTextBoxBlocking(fileCodeText, 4));
            SetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Sloth);
            return true;
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Pride) && !GetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Pride))
        {
            StartCoroutine(DisplayTextBoxBlocking(fileCodeText, 5));
            SetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Pride);
            return true;
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Randomizer) && !GetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Randomizer))
        {
            StartCoroutine(DisplayTextBoxBlocking(fileCodeText, 6));
            SetGlobalFlag(GlobalFlag.GF_FileCodeExplain_Randomizer);
            return true;
        }

        return false;
    }

    public void DisplayAreaPopup(string name)
    {
        //Debug.Log("Display " + name);
        DestroyAreaPopup();
        GameObject go = Instantiate((GameObject)Resources.Load("Menu/AreaNamePopup"), Canvas.transform);
        areaNamePopup = go.GetComponent<NamePopupScript>();

        areaNamePopup.SetText(name, true, true);
    }
    public void DestroyAreaPopup()
    {
        //Debug.Log("Destroy");
        if (areaNamePopup != null)
        {
            Destroy(areaNamePopup.gameObject);
        }
    }
    public void DisplayAchievementPopup(Achievement ac)
    {
        //Debug.Log("Display " + name);
        DestroyAchievementPopup();
        GameObject go = Instantiate((GameObject)Resources.Load("Menu/AchievementPopup"), Canvas.transform);
        achievementPopup = go.GetComponent<AchievementPopupScript>();

        achievementPopup.Setup(ac);
    }
    public void DestroyAchievementPopup()
    {
        //Debug.Log("Destroy");
        if (achievementPopup != null)
        {
            Destroy(achievementPopup.gameObject);
        }
    }


    public void SetHUDTime(float time = HUD_BUTTON_SHOW_TIME)
    {
        hudShowTime = time;
    }

    public ITextSpeaker GetSpeaker(int id)
    {
        if (id == int.MinValue)
        {
            return null;
        }
        if (BattleControl.Instance != null)
        {
            if (BattleControl.Instance.GetEntityByID(id) != null)
            {
                return BattleControl.Instance.GetEntityByID(id);
            }
        }
        else
        {
            if (mapScript != null)
            {
                return mapScript.GetEntityByID(id);
            }
        }
        Debug.LogWarning("Invalid speaker ID: " + id);
        return null;
    }
    public ITextSpeaker LocatePlayerSpeaker(PlayerCharacter pc)
    {
        if (BattleControl.Instance != null)
        {
            List < PlayerEntity > listA = BattleControl.Instance.GetPlayerEntities();
            BattleHelper.EntityID eid = (BattleHelper.EntityID)(int.MinValue);
            switch (pc)
            {
                case PlayerCharacter.Wilex:
                    eid = BattleHelper.EntityID.Wilex;
                    break;
                case PlayerCharacter.Luna:
                    eid = BattleHelper.EntityID.Luna;
                    break;
            }

            for (int i = 0; i < listA.Count; i++)
            {
                if (listA[i].entityID == eid)
                {
                    return listA[i];
                }
            }
        }
        else
        {
            if (mapScript != null)
            {
                WorldPlayer wp = WorldPlayer.Instance;
                if (wp.currentCharacter == pc)
                {
                    return wp;
                }

                List<WorldFollower> wfList = wp.followers;

                for (int i = 0; i < wfList.Count; i++)
                {
                    //Debug.Log(wfList[i].currentCharacter + " vs " + pc);
                    if (wfList[i].currentCharacter.ToString().Equals(pc.ToString()))
                    {
                        return wfList[i];
                    }
                }
            }
        }

        return null;
    }
    public ITextSpeaker LocateKeru()    //may give back null, but should also try to find her in the area
    {
        if (BattleControl.Instance != null)
        {
            //Very sus syntax because I am lazy
            BattleEntity k = BattleControl.Instance.GetEntities((e) => (e.entityID == BattleHelper.EntityID.Keru)).Find((e) => true);           

            if (k != null)
            {
                //Debug.Log("Keru is at " + k.transform.position);
                return k;
            }
        } else
        {
            //Harder to do this
            //WorldEntity k2 = new List<WorldEntity>(FindObjectsOfType<WorldEntity>()).Find((e) => (e.spriteID.Equals("Keru")));
            WorldFollower k2 = new List<WorldFollower>(FindObjectsOfType<WorldFollower>()).Find((e) => (e.spriteID.Equals("Keru")));
            if (k2 != null)
            {
                //Debug.Log("Keru is at " + k2.transform.position);
                return k2;
            }
        }

        //TODO: dummy way of doing this
        //Future shade has made the decision that this is the intended behavior
        //Because the logic would get a bit wacky here so it will probably be easier to use a null speaker
        //Debug.Log("Keru is not in this map");
        return null;
    }
    public bool KeruAvailable()
    {
        return true;
    }

    public Vector3 GetEntityPosition(int id)
    {
        if (BattleControl.Instance != null)
        {
            if (BattleControl.Instance.GetEntityByID(id) != null)
            {
                return BattleControl.Instance.GetEntityByID(id).transform.position;
            }
        } else
        {
            if (mapScript != null)
            {
                return mapScript.GetEntityByID(id).transform.position;
            }
        }
        Debug.LogWarning("Invalid ID: " + id);
        return Vector3.zero;
    }

    public void LoadBaseAssets()
    {
        font = Resources.Load<Font>("Fonts & Materials/Rubik-SemiBold");

        //there isn't a good reason why it's in the battle folder
        overworldHUD = Resources.Load<GameObject>("Battle/Overworld HUD");

        defaultTextbox = Resources.Load<GameObject>("Text/TextBox");
        defaultMinibubble = Resources.Load<GameObject>("Text/Minibubble");


        //Future thing to do: Try to push a lot of these references somewhere else because this is messy
        //  (May also be bad in terms of memory usage?)
        //(load them when needed? cache them elsewhere?)

        getItemPopup = Resources.Load<GameObject>("Menu/GetItemPopup");
        tooManyItemsPopup = Resources.Load<GameObject>("Menu/TooManyItemsPopup");
        worldspaceShopEntry = Resources.Load<GameObject>("Menu/WorldspaceShopEntry");
        defaultCollectible = Resources.Load<GameObject>("Overworld/StandardObjects/WorldCollectible");

        //nice C# syntax there
        //?? operator returns left side if it is non null otherwise it returns the right side
        //Unity is bad and ?? doesn't really work        
        menuBase = menuBase ? menuBase : (GameObject)Resources.Load("Menu/MoveMenuBase");
        menuEntryBase = menuEntryBase ? menuEntryBase : (GameObject)Resources.Load("Menu/MoveMenuEntry");
        descriptionBoxBase = descriptionBoxBase ? descriptionBoxBase : (GameObject)Resources.Load("Menu/DescriptionBox");
        promptMenuBase = promptMenuBase ? promptMenuBase : (GameObject)Resources.Load("Menu/PromptBoxMenu");
        promptMenuEntryBase = promptMenuEntryBase ? promptMenuEntryBase : (GameObject)Resources.Load("Menu/PromptMenuEntry");
        numberMenu = numberMenu ? numberMenu : (GameObject)Resources.Load("Menu/NumberMenu");
        textEntryMenu = textEntryMenu ? textEntryMenu : (GameObject)Resources.Load("Menu/TextEntryMenu");
        text_ButtonSprite = text_ButtonSprite ? text_ButtonSprite : (GameObject)Resources.Load("Menu/ButtonSprite");
        text_ItemSprite = text_ItemSprite ? text_ItemSprite : (GameObject)Resources.Load("Menu/ItemSprite");
        text_KeyItemSprite = text_KeyItemSprite ? text_KeyItemSprite : (GameObject)Resources.Load("Menu/KeyItemSprite");
        text_BadgeSprite = text_BadgeSprite ? text_BadgeSprite : (GameObject)Resources.Load("Menu/BadgeSprite");
        text_RibbonSprite = text_RibbonSprite ? text_RibbonSprite : (GameObject)Resources.Load("Menu/RibbonSprite");
        text_CommonSprite = text_CommonSprite ? text_CommonSprite : (GameObject)Resources.Load("Menu/CommonSprite");
        text_MiscSprite = text_MiscSprite ? text_MiscSprite : (GameObject)Resources.Load("Menu/MiscSprite");
        text_EffectSprite = text_EffectSprite ? text_EffectSprite : (GameObject)Resources.Load("Menu/EffectSprite");
        text_StateSprite = text_StateSprite ? text_StateSprite : (GameObject)Resources.Load("Menu/StateSprite");
        gameOverObject = gameOverObject ? gameOverObject : (GameObject)Resources.Load("GameOver/GameOverControl");

        defaultSpriteMaterial = Resources.Load<Material>("Sprites/Materials/ProperSpriteGeneral");
        defaultSpriteSimpleMaterial = Resources.Load<Material>("Sprites/Materials/ProperSpriteSimple");
        defaultSpriteFlickerMaterial = Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteFlicker");

        defaultGUISpriteMaterial = Resources.Load<Material>("Sprites/Materials/Canvas/Canvas_Sprite");

        damageEffectStar = Resources.Load<Sprite>("Sprites/Particle Effect/5Star");
        energyEffect = Resources.Load<Sprite>("Sprites/Particle Effect/4Star");
        heartEffect = Resources.Load<Sprite>("Sprites/Particle Effect/Heart");
        hexagonEffect = Resources.Load<Sprite>("Sprites/Particle Effect/Hexagon");
        soulEffect = Resources.Load<Sprite>("Sprites/Particle Effect/Soul");
        staminaEffect = Resources.Load<Sprite>("Sprites/Particle Effect/Stamina");
        coinEffect = Resources.Load<Sprite>("Sprites/Particle Effect/CoinRingFull");

        itemSprites = Resources.LoadAll<Sprite>("Sprites/Items/ItemSpritesV10");
        keyItemSprites = Resources.LoadAll<Sprite>("Sprites/Items/KeyItemSpritesV1");
        badgeSprites = Resources.LoadAll<Sprite>("Sprites/Badges/BadgeSpritesV6");
        ribbonSprites = Resources.LoadAll<Sprite>("Sprites/Ribbons/RibbonSpritesV2");
        commonSprites = Resources.LoadAll<Sprite>("Sprites/CommonSpritesV2");
        miscSprites = Resources.LoadAll<Sprite>("Sprites/Misc/MiscSpritesV1");

        effectSprites = Resources.LoadAll<Sprite>("Sprites/Battle/EffectIconsV11");
        stateSprites = Resources.LoadAll<Sprite>("Sprites/Battle/StateIconsV4");

        noFrictionMaterial = Resources.Load<PhysicMaterial>("Physics Materials/NoFriction");
        allFrictionMaterial = Resources.Load<PhysicMaterial>("Physics Materials/AllFriction");

        pauseMenu = pauseMenu ? pauseMenu : (GameObject)Resources.Load("Menu/Pause/PauseMenuHolder");
        //pauseStatusMenu = pauseStatusMenu ? pauseStatusMenu : (GameObject)Resources.Load("Menu/Pause/Pause_StatusMenu");

        
        GlobalItemScript.Instance.LoadItemDataTable();
        GlobalItemScript.Instance.LoadKeyItemTable();
        /*
        string[][] itemDataRaw = CSVParse(Resources.Load<TextAsset>("Data/ItemData").text);
        itemDataTable = new ItemDataEntry[itemDataRaw.Length - 1];
        for (int i = 1; i < itemDataTable.Length; i++)
        {
            ItemDataEntry? temp = ItemDataEntry.ParseItemDataEntry(itemDataRaw[i], (Item.ItemType)(i));
            itemDataTable[i - 1] = temp.GetValueOrDefault();
        }
        //How to use item table:
        //  Index it with (int)itemType - 1
        //(since 0 is not a real item)
        */

        GlobalBadgeScript.Instance.LoadBadgeDataTable();
        /*
        string[][] badgeDataRaw = CSVParse(Resources.Load<TextAsset>("Data/BadgeData").text);
        badgeDataTable = new BadgeDataEntry[badgeDataRaw.Length - 1];
        for (int i = 1; i < badgeDataTable.Length; i++)
        {
            BadgeDataEntry? temp = BadgeDataEntry.ParseBadgeDataEntry(badgeDataRaw[i], (Badge.BadgeType)(i));
            badgeDataTable[i - 1] = temp.GetValueOrDefault();
        }
        //badge table has to be indexed by 1 less
        */
        GlobalRibbonScript.Instance.LoadRibbonText();

        enemyText = MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/EnemyText").text);
        enemyMoveText = MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/EnemyMoveText").text);

        string[][] battleEntityDataRaw = CSVParse(Resources.Load<TextAsset>("Data/BattleEntityData").text);
        battleEntityTable = new BattleEntityData[battleEntityDataRaw.Length - 2];
        for (int i = 1; i < battleEntityTable.Length + 1; i++)
        //for (int i = 1; i < (int)(BattleHelper.EntityID.EndOfTable) + 1; i++)
        {
            BattleEntityData temp = BattleEntityData.ParseEntityData(battleEntityDataRaw[i], (BattleHelper.EntityID)(i - 1));
            battleEntityTable[i - 1] = temp;
        }

        string[][] battleEntityMovesetRaw = CSVParse(Resources.Load<TextAsset>("Data/BattleEntityMoveData").text);
        battleEntityMovesetTable = new BattleEntityMovesetData[battleEntityMovesetRaw.Length - 2];
        for (int i = 1; i < battleEntityMovesetTable.Length + 1; i++)
        {
            BattleEntityMovesetData temp = BattleEntityMovesetData.Parse(battleEntityMovesetRaw[i], (BattleHelper.EntityID)(i - 1));
            battleEntityMovesetTable[i - 1] = temp;
        }

        string[][] bestiaryOrderRaw = CSVParse(Resources.Load<TextAsset>("Data/BestiaryOrder").text);
        bestiaryOrder = new BestiaryOrderEntry[bestiaryOrderRaw.Length - 2];
        for (int i = 1; i < bestiaryOrder.Length + 1; i++)
        {
            BattleHelper.EntityID best_eid;

            Enum.TryParse(bestiaryOrderRaw[i][1], out best_eid);

            if (best_eid == BattleHelper.EntityID.DebugEntity)
            {
                Debug.LogWarning("[Bestiary Order] Can't parse entity ID " + bestiaryOrderRaw[i][1]);
            }

            string index = bestiaryOrderRaw[i][0];
            string subindex = "";

            if (!char.IsNumber(index[index.Length - 1]))
            {
                string oldIndex = bestiaryOrderRaw[i][0];
                index = oldIndex.Substring(0, oldIndex.Length - 1);
                subindex = oldIndex.Substring(oldIndex.Length - 1);
            }

            int intIndex = -1;
            int intSubIndex = 0;
            if (subindex.Length > 0)
            {
                //wtf is this math
                intSubIndex = -('a') + subindex[0];
            }

            int.TryParse(index, out intIndex);

            if (intIndex == -1)
            {
                Debug.LogWarning("[Bestiary Order] Can't parse index " + intIndex);
            }

            bestiaryOrder[i - 1] = new BestiaryOrderEntry(best_eid, intIndex, intSubIndex);
        }

        //Index with entityID
        //0 is a valid ID (but it is used for the debug entity)

        //Debug.Log(!menuBase);

        menuText = GetAllTextFromFile("DialogueText/MenuText");
        commonText = GetAllTextFromFile("DialogueText/CommonText");
        systemText = GetAllTextFromFile("DialogueText/SystemText");
    }

    public void ReturnToStartMenu()
    {
        worldMode = WorldMode.Start;

        //Cleanup
        if (mapScript != null)
        {
            Destroy(mapScript.gameObject);
        }
        if (BattleControl.Instance != null)
        {
            BattleControl.Instance.Destroy();
        }
        ClearInteractTriggers();
        if (curOverworldHUD != null)
        {
            Destroy(curOverworldHUD.gameObject);
        }

        playerData = null;
        globalFlags = new Dictionary<GlobalFlag, bool>();
        globalVars = new Dictionary<GlobalVar, string>();
        ResetAreaFlags();
        ResetAreaVars();


        startMenu = StartMenuManager.BuildMenu();
    }


    public bool Save()
    {
        return Save(saveIndex);
    }
    public bool Save(int saveIndex)
    {
        //Attempts to save with the current name and index
        string data = GetSaveFileString();

        //it's a txt file :P
        try
        {
            TextWriter tw = File.CreateText("savet" + saveIndex + ".txt");
            tw.Write(data);
            tw.Close();
            if (File.Exists("save" + saveIndex + ".txt"))
            {
                if (File.Exists("savebackup" + saveIndex + ".txt"))
                {
                    File.Delete("savebackup" + saveIndex + ".txt");
                }
                File.Move("save" + saveIndex + ".txt", "savebackup" + saveIndex + ".txt");
            }
            File.Move("savet" + saveIndex + ".txt", "save" + saveIndex + ".txt");
            File.Delete("savet" + saveIndex + ".txt");
            return true;
        } catch (Exception e)
        {
            Debug.LogError("[Saving] Exception: " + e);
            return false;
        }
    }
    public bool SaveBattleLossCount()   //Very complicated, it basically deconstructs the file and reconstructs it (Maybe I really should have made a data structure for save data...)
    {
        string data = File.ReadAllText("save" + saveIndex + ".txt");
        string[] split = data.Split("\n");

        string saveName = split[0];

        string versionString = split[1];

        Dictionary<GlobalFlag, bool> new_globalFlags = ParseGlobalFlagString(split[2]);
        Dictionary<GlobalVar, string> new_globalVars = ParseGlobalVarString(split[3]);

        string[] splitB = split[4].Split(",");

        WorldLocation wl = WorldLocation.None;
        MapID mid = MapID.None;
        Vector3 newPos = Vector3.zero;

        Enum.TryParse(splitB[0], out wl);
        Enum.TryParse(splitB[1], out mid);
        //Debug.Log(splitB[1] + " vs " + mid);
        newPos = ParseVector3(splitB[2]);

        float playTime = 0;
        float.TryParse(split[5], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out playTime);

        PlayerData newPlayerData = PlayerData.Parse(split, 6, out int _);


        //The only change :P
        Debug.Log(newPlayerData.battlesLost + " <- " + playerData.battlesLost);
        newPlayerData.battlesLost = playerData.battlesLost;


        string output = saveName;
        output += "\n";
        output += GetVersionString();
        output += "\n";
        output += UnparseGlobalFlagDictionary(new_globalFlags);
        output += "\n";
        output += UnparseGlobalVarDictionary(new_globalVars);
        output += "\n";
        output += wl;
        output += ",";
        output += mid;
        output += ",";
        output += Vector3ToString(newPos);
        output += "\n";
        output += playTime;
        output += "\n";
        output += newPlayerData.ToString();

        try
        {
            TextWriter tw = File.CreateText("savet" + saveIndex + ".txt");
            tw.Write(output);
            tw.Close();
            if (File.Exists("save" + saveIndex + ".txt"))
            {
                if (File.Exists("savebackup" + saveIndex + ".txt"))
                {
                    File.Delete("savebackup" + saveIndex + ".txt");
                }
                File.Move("save" + saveIndex + ".txt", "savebackup" + saveIndex + ".txt");
            }
            File.Move("savet" + saveIndex + ".txt", "save" + saveIndex + ".txt");
            File.Delete("savet" + saveIndex + ".txt");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Renaming Save] Exception: " + e);
            return false;
        }
    }
    public bool LoadSave(int index)
    {
        try
        {
            string data = File.ReadAllText("save" + index + ".txt");
            saveIndex = index;
            LoadSaveFileWithString(data);
            return true;
        } catch (Exception e)
        {
            Debug.LogError("[Loading Save] Exception: " + e);
            return false;
        }
    }
    public bool DeleteSave(int index)
    {
        try
        {
            File.Delete("save" + index + ".txt");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Delete Save] Exception: " + e);
            return false;
        }
    }
    public bool CopySave(int startIndex, int targetIndex)
    {
        try
        {
            if (File.Exists("save" + targetIndex + ".txt"))
            {
                File.Delete("save" + targetIndex + ".txt");
            }
            File.Copy("save" + startIndex + ".txt", "save" + targetIndex + ".txt");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Copy Save] Exception: " + e);
            return false;
        }
    }
    public bool RenameSave(int index, string newName, bool applyFileCodes = false)
    {
        string data = File.ReadAllText("save" + index + ".txt");
        string[] split = data.Split("\n");

        string output = "";
        for (int i = 0; i < split.Length; i++)
        {
            if (i > 0)
            {
                output += "\n";
            }
            if (i == 0)
            {
                output += newName;
            } else
            {
                //hacky setup
                if (i == 2)
                {
                    if (ConvertNameToFileCodeFlag(newName) != GlobalFlag.GF_None && applyFileCodes)
                    {
                        if (split[i].Length > 0)
                        {
                            output += ConvertNameToFileCodeFlag(newName) + ":true," + split[i];
                        }
                        else
                        {
                            output += ConvertNameToFileCodeFlag(newName) + ":true" + split[i];
                        }
                    } else
                    {
                        output += split[i];
                    }
                } else
                {
                    output += split[i];
                }
            }
        }

        try
        {
            TextWriter tw = File.CreateText("savet" + index + ".txt");
            tw.Write(output);
            tw.Close();
            if (File.Exists("save" + index + ".txt"))
            {
                if (File.Exists("savebackup" + index + ".txt"))
                {
                    File.Delete("savebackup" + index + ".txt");
                }
                File.Move("save" + index + ".txt", "savebackup" + index + ".txt");
            }
            File.Move("savet" + index + ".txt", "save" + index + ".txt");
            File.Delete("savet" + index + ".txt");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Renaming Save] Exception: " + e);
            return false;
        }
    }
    public bool CreateNewSave(int index, string newName)
    {
        //Attempts to save with the current name and index
        string data = GetBaseSaveFileString(newName);

        //it's a txt file :P
        try
        {
            TextWriter tw = File.CreateText("savet" + index + ".txt");
            tw.Write(data);
            tw.Close();
            if (File.Exists("save" + index + ".txt"))
            {
                if (File.Exists("savebackup" + index + ".txt"))
                {
                    File.Delete("savebackup" + index + ".txt");
                }
                File.Move("save" + index + ".txt", "savebackup" + index + ".txt");
            }
            File.Move("savet" + index + ".txt", "save" + index + ".txt");
            File.Delete("savet" + index + ".txt");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Saving] Exception: " + e);
            return false;
        }
    }

    public GlobalFlag ConvertNameToFileCodeFlag(string name)
    {
        if (name.Equals("Greed"))
        {
            return GlobalFlag.GF_FileCode_Greed;
        }

        if (name.Equals("Envy"))
        {
            return GlobalFlag.GF_FileCode_Envy;
        }

        if (name.Equals("Pride"))
        {
            return GlobalFlag.GF_FileCode_Pride;
        }

        if (name.Equals("Wrath"))
        {
            return GlobalFlag.GF_FileCode_Wrath;
        }

        if (name.Equals("Gluttony"))
        {
            return GlobalFlag.GF_FileCode_Gluttony;
        }

        if (name.Equals("Sloth"))
        {
            return GlobalFlag.GF_FileCode_Sloth;
        }

        if (name.Equals("Randomizer"))
        {
            return GlobalFlag.GF_FileCode_Randomizer;
        }

        return GlobalFlag.GF_None;
    }

    public string CreateBadgeRandomPermutation()
    {
        //Idea: first one is BadgeSwap(id 0)
        //In that slot is what BadgeSwap ends up being (as an int)

        List<int> badgeInt = new List<int>();

        for (int i = 0; i < (int)Badge.BadgeType.EndOfTable; i++)
        {
            if (i > (int)Badge.BadgeType.MegaCurse)
            {
                badgeInt.Add(i);
            }
        }

        List<int> shuffle = CreateShufflePermutation(badgeInt.Count);

        badgeInt = ApplyShufflePermutation(badgeInt, shuffle);

        //Add in the removed badges
        //(insert them in reverse order because it will reverse itself again
        badgeInt.Insert(0, 4);
        badgeInt.Insert(0, 3);
        badgeInt.Insert(0, 2);
        badgeInt.Insert(0, 1);

        List<Badge.BadgeType> output = new List<Badge.BadgeType>();

        for (int i = 0; i < badgeInt.Count; i++)
        {
            output.Add((Badge.BadgeType)badgeInt[i]);
        }

        string soutput = "";
        for (int i = 0; i < output.Count; i++)
        {
            if (i > 0)
            {
                soutput += "|";
            }
            soutput += (Badge.BadgeType)(i + 1) + "/" + output[i];
        }

        return soutput;
    }
    public string CreateRibbonRandomPermutation()
    {
        List<int> ribbonInt = new List<int>();

        for (int i = 1; i < (int)Ribbon.RibbonType.EndOfTable; i++)
        {
            ribbonInt.Add(i);
        }

        List<int> shuffle = CreateShufflePermutation(ribbonInt.Count);

        ribbonInt = ApplyShufflePermutation(ribbonInt, shuffle);

        List<Ribbon.RibbonType> output = new List<Ribbon.RibbonType>();

        for (int i = 0; i < ribbonInt.Count; i++)
        {
            output.Add((Ribbon.RibbonType)ribbonInt[i]);
        }

        string soutput = "";
        for (int i = 0; i < output.Count; i++)
        {
            if (i > 0)
            {
                soutput += "|";
            }
            soutput += (Ribbon.RibbonType)(i + 1) + "/" + output[i];
        }

        return soutput;
    }

    public string GetSaveFileString()
    {
        lastSaveTimestamp = playTime;
        Enum.TryParse(mapScript.mapName, out MapID mid);
        lastSaveMap = mid;
        Enum.TryParse(mapScript.worldLocation, out WorldLocation wl);
        lastSaveLocation = wl;

        //Things to save: global flags and vars, player data, current map + current position (so you respawn correctly), current area flags and vars?
        //The stuff above should comprise everything you want to save (persistent data should be in those)

        string output = saveName.Replace("\n", "");
        output += "\n";
        output += GetVersionString();
        output += "\n";
        output += UnparseGlobalFlagDictionary(globalFlags);
        output += "\n";
        output += UnparseGlobalVarDictionary(globalVars);
        output += "\n";
        output += mapScript.worldLocation;
        output += ",";
        output += mapScript.mapName;
        output += ",";
        output += Vector3ToString(WorldPlayer.Instance.transform.position);
        output += "\n";
        output += playTime;
        output += "\n";
        output += playerData.ToString();

        return output;
    }
    public static string GetVersionString()
    {
        return Application.version;
    }
    public string GetBaseSaveFileString(string name)
    {
        //TODO: Make this contain the correct data for the start of the game       

        lastSaveTimestamp = 0;
        lastSaveMap = MapID.None;
        lastSaveLocation = WorldLocation.None;

        Dictionary<GlobalFlag, bool> newFlags = new Dictionary<GlobalFlag, bool>();

        if (ConvertNameToFileCodeFlag(name) != GlobalFlag.GF_None)
        {
            newFlags.Add(ConvertNameToFileCodeFlag(name), true);
        }

        string output = name;
        output += "\n";
        output += GetVersionString();
        output += "\n";
        output += UnparseGlobalFlagDictionary(newFlags);
        output += "\n";
        output += UnparseGlobalVarDictionary(new Dictionary<GlobalVar, string>());
        output += "\n";
        output += WorldLocation.None;
        output += ",";
        output += MapID.RabbitHole_Lobby;
        output += ",";
        output += Vector3ToString(Vector3.up * 15 + Vector3.forward * 1.5f);
        output += "\n";
        output += 0;
        output += "\n";
        output += (new PlayerData(BattleHelper.EntityID.Wilex, BattleHelper.EntityID.Luna)).ToString();

        return output;
    }
    public void LoadSaveFileWithString(string data)
    {
        //To add: save functionality
        //Things to save: global flags and vars, player data, current map + current position (so you respawn correctly), current area flags and vars?
        //The stuff above should comprise everything you want to save (persistent data should be in those)

        string retrievedFile = data;        

        string[] split = retrievedFile.Split("\n");

        saveName = split[0];

        string versionName = split[1];

        //future TODO: version specific save translation (i.e. if I ever change how data is set up I have to handle that specially)
        //  But I am likely not going to change any names of flags, vars, etc that you can ever set normally to avoid save incompabitility


        Dictionary<GlobalFlag, bool> new_globalFlags = ParseGlobalFlagString(split[2]);
        Dictionary<GlobalVar, string> new_globalVars = ParseGlobalVarString(split[3]);

        string[] splitB = split[4].Split(",");

        WorldLocation wl = WorldLocation.None;
        MapID mid = MapID.None;
        Vector3 newPos = Vector3.zero;

        Enum.TryParse(splitB[0], out wl);
        lastSaveLocation = wl;
        Enum.TryParse(splitB[1], out mid);
        //Debug.Log(splitB[1] + " vs " + mid);
        newPos = ParseVector3(splitB[2]);

        float playTime = 0;
        float.TryParse(split[5], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out playTime);

        PlayerData newPlayerData = PlayerData.Parse(split, 6, out int _);

        //note that there are flags that influence the max stat calculation
        //so do this before checking max stats
        globalFlags = new_globalFlags;
        globalVars = new_globalVars;

        playerData = newPlayerData.Copy();

        ApplyFileCodeChanges();

        playerData.UpdateMaxStats();

        this.playTime = playTime;
        lastSaveTimestamp = playTime;

        //Warp
        //delete current map?
        if (mapScript == null)
        {
            //sus
        }
        else
        {
            Destroy(mapScript.gameObject);
        }

        ResetCutsceneSystem();

        worldMode = WorldMode.Overworld;
        mapHalted = false;
        //mapScript.Enable();
        Camera.SetSnapshot(cameraSnapshot); //may be improper

        ResetHUD();

        SnapFade(1);

        //load in new map
        LoadMap(mid, -1, newPos, 0);

        //in case I wasn't careful with save points and enemy locations, to make sure you can't softlock yourself with a bad save location (?)
        WorldPlayer wp = WorldPlayer.Instance;
        if (wp != null)
        {
            wp.SetEncounterCooldown();
        }
    }
    public void ApplyFileCodeChanges()
    {
        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Randomizer))
        {
            if (GetGlobalVar(GlobalVar.GV_BadgeRandomPermutation) == null)
            {
                SetGlobalVar(GlobalVar.GV_BadgeRandomPermutation, CreateBadgeRandomPermutation());
            }
            if (GetGlobalVar(GlobalVar.GV_RibbonRandomPermutation) == null)
            {
                SetGlobalVar(GlobalVar.GV_RibbonRandomPermutation, CreateRibbonRandomPermutation());
            }

            //parse random permutation
            string brand = GetGlobalVar(GlobalVar.GV_BadgeRandomPermutation);
            string[] splitB = brand.Split("|");

            badgeMutation = new Dictionary<Badge.BadgeType, Badge.BadgeType>();
            badgeMutationInverse = new Dictionary<Badge.BadgeType, Badge.BadgeType>();

            for (int i = 0; i < splitB.Length; i++)
            {
                Badge.BadgeType a = Badge.BadgeType.None;
                Badge.BadgeType b = Badge.BadgeType.None;

                string[] splitBi = splitB[i].Split("/");

                if (splitBi.Length < 2)
                {
                    continue;
                }

                Enum.TryParse(splitBi[0], true, out a);
                Enum.TryParse(splitBi[1], true, out b);

                if (a != Badge.BadgeType.None && b != Badge.BadgeType.None)
                {
                    badgeMutation.Add(a, b);
                    badgeMutationInverse.Add(b, a);
                }
            }

            string rrand = GetGlobalVar(GlobalVar.GV_RibbonRandomPermutation);
            string[] splitR = rrand.Split("|");

            ribbonMutation = new Dictionary<Ribbon.RibbonType, Ribbon.RibbonType>();
            ribbonMutationInverse = new Dictionary<Ribbon.RibbonType, Ribbon.RibbonType>();

            for (int i = 0; i < splitR.Length; i++)
            {
                Ribbon.RibbonType a = Ribbon.RibbonType.None;
                Ribbon.RibbonType b = Ribbon.RibbonType.None;

                string[] splitRi = splitR[i].Split("/");

                if (splitRi.Length < 2)
                {
                    continue;
                }

                Enum.TryParse(splitRi[0], true, out a);
                Enum.TryParse(splitRi[1], true, out b);

                if (a != Ribbon.RibbonType.None && b != Ribbon.RibbonType.None)
                {
                    ribbonMutation.Add(a, b);
                    ribbonMutationInverse.Add(b, a);
                }
            }
        }

        if (GetGlobalFlag(GlobalFlag.GF_FileCode_Greed))
        {
            //Greed badge changes
            List<Badge.BadgeType> greedBadgeInput = new List<Badge.BadgeType>
            {
                Badge.BadgeType.VitalEnergy,
                Badge.BadgeType.ItemRebate,
                Badge.BadgeType.GoldenPower,
                Badge.BadgeType.GoldenEnergy,
                Badge.BadgeType.GoldenShield,
                Badge.BadgeType.MoneyBoost,
                Badge.BadgeType.HPPlus,
                Badge.BadgeType.HPPlusB,
                Badge.BadgeType.HPPlusC,
                Badge.BadgeType.HPPlusD,
                Badge.BadgeType.HealthSteal
            };

            List<Badge.BadgeType> greedBadgeOutput = new List<Badge.BadgeType>
            {
                Badge.BadgeType.HealthyExercise,
                Badge.BadgeType.ItemBoost,
                Badge.BadgeType.SpiritPower,
                Badge.BadgeType.StaminaEnergy,
                Badge.BadgeType.SpiritShield,
                Badge.BadgeType.VictoryHeal,
                Badge.BadgeType.EPPlus,
                Badge.BadgeType.EPPlusB,
                Badge.BadgeType.EPPlusC,
                Badge.BadgeType.EPPlusD,
                Badge.BadgeType.EnergySteal
            };

            if (badgeMutation == null)
            {
                //Can just put everything in the dictionaries without problems
                badgeMutation = new Dictionary<Badge.BadgeType, Badge.BadgeType>();
                badgeMutationInverse = new Dictionary<Badge.BadgeType, Badge.BadgeType>();
                //Add all the Input -> Output pairs to the dictionary
                for (int i = 0; i < greedBadgeInput.Count; i++)
                {
                    badgeMutation.Add(greedBadgeInput[i], greedBadgeOutput[i]);
                    badgeMutationInverse.Add(greedBadgeOutput[i], greedBadgeInput[i]);
                }
            } else
            {
                //There is already some stuff in the dictionary already
                //Use the inverse dictionary to find what needs to change

                //If old dictionary has A -> B, and new dictionary has B -> C
                //Replace with A -> C
                //  (In the inverse table, make B -> B and C -> A)
                for (int i = 0; i < greedBadgeInput.Count; i++)
                {
                    if (badgeMutationInverse.ContainsKey(greedBadgeInput[i]))
                    {
                        //A -> B becomes A -> C
                        //A = (inverse(B))
                        Badge.BadgeType a = badgeMutationInverse[greedBadgeInput[i]];
                        Badge.BadgeType b = greedBadgeInput[i];
                        Badge.BadgeType c = greedBadgeOutput[i];

                        badgeMutation[a] = c;
                        badgeMutationInverse[c] = a;
                        badgeMutation[b] = b;
                        badgeMutationInverse[b] = b;
                    }
                }
            }
        }
    }
    public FileMenuEntry ConstructFileMenuEntry(int index)
    {
        Debug.Log("Parse file " + index);
        try
        {
            FileMenuEntry output = new FileMenuEntry();
            if (File.Exists("save" + index + ".txt"))
            {
                //Read stuff
                string retrievedFile = File.ReadAllText("save" + index + ".txt");

                string[] split = retrievedFile.Split("\n");

                string saveName = split[0].Replace("\r", "");

                string versionName = split[1];


                Dictionary<GlobalFlag, bool> new_globalFlags = ParseGlobalFlagString(split[2]);
                Dictionary<GlobalVar, string> new_globalVars = ParseGlobalVarString(split[3]);

                string[] splitB = split[4].Split(",");

                WorldLocation wl = WorldLocation.None;
                MapID mid = MapID.None;
                Vector3 newPos = Vector3.zero;

                Enum.TryParse(splitB[0], out wl);
                Enum.TryParse(splitB[1], out mid);
                //Debug.Log(splitB[1] + " vs " + mid);
                newPos = ParseVector3(splitB[2]);

                float playTime = 0;
                float.TryParse(split[5], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out playTime);

                PlayerData newPlayerData = PlayerData.Parse(split, 6, out int _);

                output.name = (index + 1) + ". " + saveName;
                output.worldLocation = GetAreaName(wl);
                output.worldMap = GetMapName(mid);
                output.playtime = ParseTime(playTime);
                output.specialSprites = GetSpecialString(new_globalFlags, new_globalVars);
                output.progressSprites = GetProgressString(new_globalFlags, new_globalVars);
                Color? col = GetSaveColor(new_globalFlags, new_globalVars);
                output.hasBackground = col.HasValue;
                output.backgroundColor = col.GetValueOrDefault();
                output.levelText = "Level " + newPlayerData.level;
            }

            return output;
        } catch (ArgumentException e)
        {
            Debug.LogError(e);
            return null;
        }
    }
    public string GetSpecialString(Dictionary<GlobalFlag, bool> globalFlags, Dictionary<GlobalVar, string> globalVars)
    {
        return "";
    }
    public string GetProgressString(Dictionary<GlobalFlag, bool> globalFlags, Dictionary<GlobalVar, string> globalVars)
    {
        if (globalVars.ContainsKey(GlobalVar.GV_PitFloor))
        {
            string pitfloor = globalVars[GlobalVar.GV_PitFloor];

            return "Floor " + pitfloor;
        } else
        {
            return "";
        }
    }
    public Color? GetSaveColor(Dictionary<GlobalFlag, bool> globalFlags, Dictionary<GlobalVar, string> globalVars)
    {
        return null;
    }


    /*
    public bool TryFadeOut()
    {
        if (fadeOutScript.FadeOutProgress <= 0)
        {
            StartCoroutine(fadeOutScript.FadeToBlack());
            return true;
        }
        return false;
    }
    public bool TryFadeIn()
    {
        if (fadeOutScript.FadeOutProgress > 0)
        {
            StartCoroutine(fadeOutScript.UnfadeToBlack());
            return true;
        }
        return false;
    }
    */
    public IEnumerator FadeToBlack()
    {
        yield return StartCoroutine(fadeOutScript.FadeToBlack());
    }
    public IEnumerator UnfadeToBlack()
    {
        yield return StartCoroutine(fadeOutScript.UnfadeToBlack());
    }
    public void SnapFade(float f)
    {
        fadeOutScript.SnapFade(f);
    }
    public IEnumerator FadeToWhite()
    {
        yield return StartCoroutine(fadeOutScript.FadeToWhite());
    }
    public IEnumerator UnfadeToWhite()
    {
        yield return StartCoroutine(fadeOutScript.UnfadeToWhite());
    }
    public IEnumerator BattleFadeToBlack(Color color, Vector2 position)
    {
        yield return StartCoroutine(battleFadeOutScript.FadeToBlack(color, position));
    }
    public IEnumerator BattleUnfadeToBlack(Color color, Vector2 position)
    {
        yield return StartCoroutine(battleFadeOutScript.UnfadeToBlack(color, position));
    }

    //note: this will work fine if called from game over
    public IEnumerator EnterBattle(BattleStartArguments bsa = null)
    {
        if (worldMode == WorldMode.Battle || mapHalted)
        {
            yield break;
        }
        //inCutscene = true;
        mapHalted = true;
        if (mapScript != null)
        {
            mapScript.Halt();
        }

        battleStartArguments = bsa;
        Vector3 position = bsa != null ? bsa.position : Vector3.zero;
        if (position == Vector3.zero)
        {
            position = worldPlayer.GetTextTailPosition();
        }

        int firstStriker = bsa != null ? bsa.firstStrikePosId : BattleStartArguments.FIRSTSTRIKE_NULL_ENTITY;

        Color firstStrikeColor = Color.yellow;

        bool autoStrike = !BattleControl.GetProperty(bsa.properties, BattleHelper.BattleProperties.IgnoreFirstStrikeBadges) && playerData.BadgeEquipped(Badge.BadgeType.AutoStrike);
        bool inviteDanger = !BattleControl.GetProperty(bsa.properties, BattleHelper.BattleProperties.IgnoreFirstStrikeBadges) && playerData.BadgeEquipped(Badge.BadgeType.InviteDanger);
        bool dodgeStep = !BattleControl.GetProperty(bsa.properties, BattleHelper.BattleProperties.IgnoreFirstStrikeBadges) && playerData.BadgeEquipped(Badge.BadgeType.DodgeStep);

        //Effects of all these:
        //Auto Strike converts non enemy first strikes to player first strikes
        //Invite Danger converts everything into enemy first strikes
        //Dodge step prevents enemy first strikes except for Invite Danger


        //bool playerFS = false;
        //bool enemyFS = false;

        if ((autoStrike && firstStriker < 0) || (firstStriker != BattleStartArguments.FIRSTSTRIKE_NULL_ENTITY && firstStriker < 0))
        {
            firstStrikeColor = Color.blue;
            //playerFS = true;
        }
        if ((inviteDanger) || (!dodgeStep && (firstStriker != BattleStartArguments.FIRSTSTRIKE_NULL_ENTITY && firstStriker >= 0)))
        {
            firstStrikeColor = Color.red;
            //enemyFS = true;
        }

        Vector2 screenPos = WorldPosToCanvasPosProportion(position);
        //screenPos.x /= 800;
        //screenPos.y /= 600;

        if (curOverworldHUD != null)
        {
            Destroy(curOverworldHUD.gameObject);
        }
        yield return StartCoroutine(BattleFadeToBlack(firstStrikeColor, screenPos));
        if (mapScript != null)
        {
            mapScript.Disable();
        }

        worldMode = WorldMode.Battle;

        yield return new WaitForSeconds(0.1f);  //to try to hide the lag spike better
        cameraSnapshot = Camera.MakeSnapshot();
        BattleControl.SetCameraDefault();
        //Camera.SetManual(new Vector3(0, 2, -6.5f), new Vector3(0, 0, 0));
        BattleControl.StartBattleStatic(bsa); //note that currently, BattleControl takes references from this script directly
        DestroyAreaPopup();
        yield return new WaitForSeconds(0.7f);
        yield return StartCoroutine(BattleUnfadeToBlack(firstStrikeColor, new Vector2(0.5f, 0.5f)));
    }
    public IEnumerator EnterBattle(EncounterData e, BattleStartArguments bsa = null)
    {
        nextBattle = e;
        yield return StartCoroutine(EnterBattle(bsa));
    }
    public IEnumerator ReturnFromBattle(BattleHelper.BattleOutcome outcome, bool canRetry)
    {
        coinDrops = BattleControl.coinDrops;
        dropItemType = BattleControl.dropItem;
        dropItemCount = BattleControl.dropItemCount;
        if (dropItemCount < 1)
        {
            dropItemType = Item.ItemType.None;
        }
        switch (outcome)
        {
            case BattleHelper.BattleOutcome.Win:
                yield return StartCoroutine(ReturnFromBattleNormal(outcome));
                break;
            case BattleHelper.BattleOutcome.Flee:
                yield return StartCoroutine(ReturnFromBattleNormal(outcome));
                break;
            case BattleHelper.BattleOutcome.Exit:
                yield return StartCoroutine(ReturnFromBattleNormal(outcome));
                break;
            case BattleHelper.BattleOutcome.Tie:
                if (mapScript.CanHandleBattleLoss())
                {
                    yield return StartCoroutine(ReturnFromBattleNormal(outcome));
                }
                else
                {
                    yield return StartCoroutine(ReturnFromBattleGameOver(outcome, canRetry));
                }
                break;
            case BattleHelper.BattleOutcome.Death:
                if (mapScript.CanHandleBattleLoss())
                {
                    yield return StartCoroutine(ReturnFromBattleNormal(outcome));
                }
                else
                {
                    yield return StartCoroutine(ReturnFromBattleGameOver(outcome, canRetry));
                }
                break;
        }
    }
    public IEnumerator ReturnFromBattleNormal(BattleHelper.BattleOutcome outcome)
    {
        yield return StartCoroutine(FadeToBlack());
        BattleControl.Destroy();
        Destroy(BattleControl.gameObject);
        yield return new WaitForSeconds(0.25f);

        //To do later: make this grab the last camera zone data
        //Camera.SetMode(WorldCamera.CameraMode.FollowPlayer);
        Camera.SetSnapshot(cameraSnapshot);

        //inCutscene = false;
        worldMode = WorldMode.Overworld;
        mapHalted = false;
        mapScript.Enable();
        mapScript.HandleBattleOutcome(outcome);
        SetSkybox(mapScript.GetSkyboxID());
        if (outcome == BattleHelper.BattleOutcome.Flee || outcome == BattleHelper.BattleOutcome.Exit)
        {
            WorldPlayer wp = WorldPlayer.Instance;
            if (wp != null)
            {
                wp.SetEncounterCooldown();
            }
        }
        ResetHUD();
        yield return StartCoroutine(UnfadeToBlack());
    }
    public IEnumerator ReturnFromBattleGameOver(BattleHelper.BattleOutcome outcome, bool canRetry)
    {
        yield return StartCoroutine(FadeToBlack());
        BattleControl.Destroy();
        Destroy(BattleControl.gameObject);
        yield return new WaitForSeconds(0.25f);
        GameOverControl.GameOverStatic(canRetry);
        worldMode = WorldMode.GameOver;
        yield return StartCoroutine(UnfadeToBlack());

    }

    /*
    public GameObject[] SpawnPlayerEntities(Vector3 position, float facingYaw)
    {
        GameObject[] output = new GameObject[2];

        output[0] = SpawnPlayer(position, facingYaw).gameObject;
        output[1] = SpawnFollower(position, facingYaw).gameObject;

        return output;
    }
    */
    public WorldPlayer SpawnPlayer(Vector3 position, float facingYaw)
    {
        string filePath = "Overworld/StandardEntities/WorldPlayer";
        //this is probably not efficient but since it only happens once its probably not that bad
        GameObject o = Instantiate(Resources.Load<GameObject>(filePath));
        WorldPlayer wp = o.GetComponent<WorldPlayer>();

        wp.SetTrueFacingRotation(facingYaw);
        wp.transform.position = position;

        wp.SetIdentity(playerData.party[0].entityID);

        return wp;
    }
    public WorldFollower SpawnFollower(Vector3 position, float facingYaw)
    {
        return SpawnFollower(position, facingYaw, playerData.party[1].GetSpriteID());
    }
    public WorldFollower SpawnFollower(Vector3 position, float facingYaw, SpriteID spriteID)
    {
        string filePath = "Overworld/StandardEntities/WorldFollower";
        //this is probably not efficient but since it only happens once its probably not that bad
        GameObject o = Instantiate(Resources.Load<GameObject>(filePath));
        WorldFollower wf = o.GetComponent<WorldFollower>();

        wf.SetTrueFacingRotation(facingYaw);
        wf.transform.position = position;

        wf.SetIdentity(spriteID);

        return wf;
    }

    public void ResetHUD()
    {
        if (curOverworldHUD != null)
        {
            Destroy(curOverworldHUD.gameObject);
        }
        curOverworldHUD = Instantiate(overworldHUD, Canvas.transform).GetComponent<OverworldHUDScript>();
        curOverworldHUD.Build();
    }

    public void MapChangeCleanup()
    {
        ResetCutsceneSystem();

        MinibubbleScript[] minibubbles = FindObjectsOfType<MinibubbleScript>();

        foreach (MinibubbleScript mbs in minibubbles)
        {
            Destroy(mbs.gameObject);
        }
    }

    public IEnumerator ChangeMap(MapID mapName, int exit, Vector3 offsetPos = default, float yawOffset = 0)
    {
        if (mapName == MapID.None)
        {
            Debug.LogError("Invalid map ID");
        }

        WorldLocation wl = WorldLocation.None;
        SkyboxID sid = curSkybox;
        //delete current map
        MapChangeCleanup();
        if (mapScript == null)
        {
            //sus
        } else
        {
            Enum.TryParse(mapScript.worldLocation, out wl);
            //wl = mapScript.worldLocation;
            Destroy(mapScript.gameObject);
        }

        //the old script is persistent for some reason?
        yield return new WaitForSeconds(0.15f);

        //load in new map
        LoadMap(mapName, exit, offsetPos, yawOffset);

        Enum.TryParse(mapScript.worldLocation, out WorldLocation nwl);
        //WorldLocation nwl = mapScript.worldLocation;

        //if worldlocation and skybox are different, reset vars
        //so ensure that areas are delineated by skybox changes
        //(The idea behind this is that cities/small subareas that have the same skybox aren't easy ways to reset area flags)
        //(Resetting area flags will respawn all the enemies, so it might be a bit jarring if you run into a town and immediately leave and everything came back)
        //(Note that area flags also reset on reload but that is "slower")
        if (wl != nwl && mapScript.GetSkyboxID() != sid)
        {
            ResetAreaFlags();
            ResetAreaVars();
        }

        DestroyAreaPopup();
        if (wl != nwl)
        {
            DisplayAreaPopup(GetAreaName(nwl));
        }
    }
    //note: stuff like loading in from a save file will call LoadMap instead of ChangeMap (because you are starting from a state of having no map active)
    public void LoadMap(MapID mapName, int exit, Vector3 offsetPos = default, float yawOffset = 0)
    {
        ResetHUD();
        showCoins = false;
        MapScript ms = CreateMap(mapName);
        mapScript = ms;
        ms.MapInit();
        SetSkybox(ms.GetSkyboxID());
        //spawn players
        WorldPlayer wp = null;
        WorldFollower wf = null;
        if (ms.playerHolder != null)
        {
            if (playerData.party.Count == 1)
            {
                wp = SpawnPlayer(offsetPos, yawOffset);
                wp.followers = new List<WorldFollower>();
                wp.transform.parent = ms.playerHolder.transform;
            } else
            {
                wp = SpawnPlayer(offsetPos, yawOffset);
                wf = SpawnFollower(offsetPos, yawOffset);

                wp.followers = new List<WorldFollower>
                {
                    wf
                };

                wf.followTarget = wp.transform;
                wf.followerIndex = 0;

                wp.transform.parent = ms.playerHolder.transform;
                wf.transform.parent = ms.playerHolder.transform;
            }

            //Bonus followers
            if (playerData.bonusFollowers != null)
            {
                for (int i = 0; i < playerData.bonusFollowers.Count; i++)
                {
                    WorldFollower wf2 = SpawnFollower(offsetPos, yawOffset, playerData.bonusFollowers[i]);
                    wp.followers.Add(wf2);

                    wf2.followTarget = wp.followers[wp.followers.Count - 2].transform;
                    wf2.followerIndex = i + 1;

                    wf2.transform.parent = ms.playerHolder.transform;
                }
            }
        }
        //camera needs to snap to the right position (but it needs to be done right as the player gets positioned in the entrance script)
        //Camera.SnapToTargets();
        
        //set up map entrance
        StartCoroutine(ExecuteCutscene(ms.DoEntrance(exit, offsetPos, yawOffset)));
    }
    public void SetSkybox(SkyboxID skyboxID)
    {
        if (curSkybox != skyboxID)
        {
            curSkybox = skyboxID;
            string filePath = GetSkyboxPath(skyboxID);
            RenderSettings.skybox = Resources.Load<Material>(filePath);
        }
    }

    public static string GetMapTextPath(MapID mapName)
    {
        return "DialogueText/Map/" + mapName.ToString();
    }
    public static string GetMapPrefabPath(MapID mapName)
    {
        return "Map/" + mapName.ToString();
    }
    public static string GetBattleMapPrefabPath(BattleMapID mapName)
    {
        return "BattleMap/" + mapName.ToString();
    }
    public static string GetSkyboxPath(SkyboxID skyboxID)
    {
        return "Skybox/" + skyboxID.ToString();
    }

    public MapScript CreateMap(MapID mapName)
    {
        string filePath = GetMapPrefabPath(mapName);
        //this is probably not efficient but since it only happens once its probably not that bad
        GameObject o = Instantiate(Resources.Load<GameObject>(filePath));
        return o.GetComponent<MapScript>();
    }
    public BattleMapScript CreateMap(BattleMapID bmapName)
    {
        string filePath = GetBattleMapPrefabPath(bmapName);
        //this is probably not efficient but since it only happens once its probably not that bad
        GameObject o = Instantiate(Resources.Load<GameObject>(filePath), BattleControl.gameObject.transform);
        return o.GetComponent<BattleMapScript>();
    }
    public MapScript CreateMap(MapID mapName, Transform parent)
    {
        MapScript ms = CreateMap(mapName);
        ms.transform.parent = parent;
        return ms;
    }
    public BattleMapScript CreateMap(BattleMapID bmapName, Transform parent)
    {
        BattleMapScript bms = CreateMap(bmapName);
        bms.transform.parent = parent;
        return bms;
    }

    public bool GetControlsEnabled()
    {
        return !inCutscene || Cheat_ControlNeverDisabled;
    }
    public bool GetControlsDisabled()
    {
        return inCutscene && !Cheat_ControlNeverDisabled;
    }

    public void AddInteractTrigger(InteractTrigger it)
    {
        if (!interactTriggers.Contains(it))
        {
            interactTriggers.Add(it);
        }
    }

    public void RemoveInteractTrigger(InteractTrigger it)
    {
        if (interactTriggers.Contains(it))
        {
            interactTriggers.Remove(it);
        }
    }

    public void ClearInteractTriggers()
    {
        while (interactTriggers.Count > 0)
        {
            interactTriggers.RemoveAt(0);
        }
    }

    public void InteractTriggerCleanup()
    {
        for (int i = 0; i < interactTriggers.Count; i++)
        {
            if (interactTriggers[i] == null || !interactTriggers[i].isActiveAndEnabled)
            {
                interactTriggers.RemoveAt(i);
                i--;
                continue;
            }
        }
    }

    //can you interact with stuff by pressing A (if so the method below executes the script)
    //To do later: make particle effects to apply to this (the thing that signals you can interact with something)
    public bool CanInteract()
    {
        InteractTriggerCleanup();
        return interactTriggers.Count > 0;
    }
    public bool TryInteract()
    {
        //Debug.Log(Time.time);
        if (CanInteract())
        {
            ExecuteInteractScript();
            return true;
        }
        return false;
    }
    public bool NonNPCInteractActive()  //special particle above your head appears
    {
        if (CanInteract())
        {
            for (int i = 0; i < interactTriggers.Count; i++)
            {
                if (!(interactTriggers[i] is NPCInteractTrigger))
                {
                    return true;
                }
            }

            return false;
        }
        return false;
    }
    public void ExecuteInteractScript()
    {
        InteractTriggerCleanup();

        //tiebreak somehow
        WorldPlayer wp = WorldPlayer.Instance;

        Vector3 start = wp.GetEyePoint();
        Vector3 facing = wp.FacingVector();

        InteractTrigger bestTrigger = null;
        float bestScore = 0;    //note: negative scoring triggers will not be activated (so you don't interact with stuff while facing away)

        float score = 0;
        Vector3 delta = Vector3.zero;
        float dist = 0;

        for (int i = 0; i < interactTriggers.Count; i++)
        {
            delta = interactTriggers[i].GetEvalPoint() - start;
            dist = delta.magnitude;
            score = Vector3.Dot(delta.normalized, facing) + interactTriggers[i].DotProductBonus();

            if (dist < 0.001f)
            {
                dist = 0.001f;
            }
            score /= dist;

            if (score > bestScore)
            {
                bestScore = score;
                bestTrigger = interactTriggers[i];
            }
        }

        //Debug.Log("Execute " + Time.time);
        if (bestTrigger != null)
        {
            bestTrigger.Interact();
        }
    }

    //To do later: find a different way to do this (more simple way)
    //Idea is to use cutscene queue?
    //Main manager stores a last started cutscene value and a last ended cutscene value
    //Cutscene script assigns itself an ID with last started cutscene + 1 (increments it also)
    //Cutscene script will hang until last cutscene value reaches its id - 1
    //When cutscene is done it updates last ended cutscene to its own id

    //Small extra thing is that if last started and last ended become the same, reset everything back to the initial state of -1 (?)
    //(prevents any dumb 2 billion cutscene overflow stuff)
    //(These cutscene queue positions should only be used in this system)

    //System will break if queue members are destroyed or interrupted mid cutscene

    //Force reset the system in certain situations

    public int LastStartedCutscene()
    {
        return lastStartedCutscene;
    }

    //note: includes pending cutscenes
    public void SetLastStartedCutscene(int lsc)
    {
        lastStartedCutscene = lsc;
    }

    public int LastEndedCutscene()
    {
        return lastEndedCutscene;
    }

    public void SetLastEndedCutscene(int lec)
    {
        lastEndedCutscene = lec;
    }

    public void ResetCutsceneSystem()
    {
        //Debug.Log("Reset cutscene system");
        lastStartedCutscene = -1;
        lastEndedCutscene = -1;
    }

    //??? may act weird with object destruction (So don't destroy objects that are actively executing a cutscene, unless you also are calling ResetCutsceneSystem right before)
    //Correct way to use this: 
    //  StartCoroutine(ExecuteCutscene( input() ))
    //      where input = an IEnumerator Method
    //Don't pass in StartCoroutine(input)   (because there is StartCoroutine(input) inside)
    public IEnumerator ExecuteCutscene(IEnumerator input) {
        int cutsceneID = LastStartedCutscene() + 1;
        SetLastStartedCutscene(cutsceneID);
        //Debug.Log("Cutsene start: " + cutsceneID + " last started = " + lastStartedCutscene);

        yield return new WaitUntil(() => (LastEndedCutscene() == cutsceneID - 1 || (LastEndedCutscene() == -1 && LastStartedCutscene() == -1)));

        if (LastEndedCutscene() == -1 && LastStartedCutscene() == -1)
        {
            //ResetCutsceneSystem was called before this cutscene could execute
            //(after the LSC was set but before the yield block could fulfil the first condition)
            //Debug.Log("Cutscene interrupted: " + cutsceneID);
            yield break;
        }


        yield return StartCoroutine(input);

        if (LastEndedCutscene() == -1 && LastStartedCutscene() == -1)
        {
            //ResetCutsceneSystem was called by the cutscene or something else while it was executing
            //Debug.Log("Cutscene interrupted late: " + cutsceneID);
            yield break;
        }

        SetLastEndedCutscene(cutsceneID);

        if (LastStartedCutscene() == LastEndedCutscene())
        {
            ResetCutsceneSystem();
        }

        //Debug.Log("Cutscene (normal) end: " + cutsceneID);

        //hacky fix for a problem I'm having
        //WorldPlayer.Instance.rb.velocity = Vector3.zero;
        //I forgot what this was fixing, so I hope only zeroing out the y velocity is not a problem
        WorldPlayer.Instance.rb.velocity -= XZProjectPreserve(WorldPlayer.Instance.rb.velocity);
    }


    public void ShowCoinCounter()
    {
        //Debug.Log("Show coins: " + playerData.coins);
        showCoins = true;
    }

    public void HideCoinCounter()
    {
        //Debug.Log("Hide coins");
        showCoins = false;
    }


    public IEnumerator StandardCameraShake(float time, float strength, float shakeFactor = 1, float shakeFactorB = 1)
    {
        cameraAnimIndex++;
        int currentIndex = cameraAnimIndex;
        float realStrength = 0;
        float realProgress = 0;
        float realTime = 0;

        float bend = 0.25f;

        while (realProgress < 1)
        {
            if (cameraAnimIndex != currentIndex)
            {
                yield break;
            }
            realTime += Time.deltaTime;
            realProgress += Time.deltaTime / time;

            if (realProgress < bend)
            {
                realStrength = realProgress * 4 * strength;
            } else
            {
                realStrength = (1 - realProgress) * (1 / (1 - bend)) * strength;
            }

            Vector3 trueDelta = realStrength * (Vector3.up * Mathf.Sin(realProgress * shakeFactor * 5) + Vector3.right * Mathf.Cos(realProgress * shakeFactor * 5)) * Mathf.Sin(realProgress * shakeFactorB * 15);

            Camera.SetOffset(trueDelta);

            yield return null;
        }

        Camera.SetOffset(Vector3.zero);
        cameraAnimIndex = 0;
    }
    public IEnumerator CameraShake1D(Vector3 direction, float time, float shakeFactor)
    {
        cameraAnimIndex++;
        int currentIndex = cameraAnimIndex;
        float realStrength = 0;
        float realProgress = 0;
        float realTime = 0;
        float strength = 1;

        float bend = 0.25f;

        while (realProgress < 1)
        {
            if (cameraAnimIndex != currentIndex)
            {
                yield break;
            }
            realTime += Time.deltaTime;
            realProgress += Time.deltaTime / time;

            if (realProgress < bend)
            {
                realStrength = realProgress * 4 * strength;
            }
            else
            {
                realStrength = (1 - realProgress) * (1 / (1 - bend)) * strength;
            }

            Vector3 trueDelta = realStrength * (direction) * Mathf.Sin(realProgress * shakeFactor * 15);

            Camera.SetOffset(trueDelta);

            yield return null;
        }

        Camera.SetOffset(Vector3.zero);
        cameraAnimIndex = 0;
    }
    public IEnumerator CameraJump(Vector3 direction, float time)    //Use direction vector's magnitude for strength
    {
        cameraAnimIndex++;
        int currentIndex = cameraAnimIndex;
        float realStrength = 0;
        float realProgress = 0;
        float realTime = 0;
        float strength = 1;

        float bend = 0.4f;

        while (realProgress < 1)
        {
            if (cameraAnimIndex != currentIndex)
            {
                yield break;
            }
            realTime += Time.deltaTime;
            realProgress += Time.deltaTime / time;

            if (realProgress < bend)
            {
                realStrength = realProgress * 3 * strength;
            }
            else
            {
                realStrength = (1 - realProgress) * (1 / (1 - bend)) * strength;
            }
            Vector3 trueDelta = direction * realStrength;
            Camera.SetOffset(trueDelta);
            yield return null;
        }

        Camera.SetOffset(Vector3.zero);
        cameraAnimIndex = 0;
    }



    //Randomizer stuff
    public Badge.BadgeType MutateBadgeType(Badge.BadgeType bt)
    {
        //Debug.Log("Mutate badge");
        if (badgeMutation != null && badgeMutation.ContainsKey(bt))
        {
            //Debug.Log(bt + " -> " + badgeMutation[bt]);
            return badgeMutation[bt];
        }
        return bt;
    }
    public Badge.BadgeType UnmutateBadgeType(Badge.BadgeType bt)
    {
        if (badgeMutationInverse != null && badgeMutationInverse.ContainsKey(bt))
        {
            return badgeMutationInverse[bt];
        }
        return bt;
    }

    public Ribbon.RibbonType MutateRibbonType(Ribbon.RibbonType rt)
    {
        if (ribbonMutation != null && ribbonMutation.ContainsKey(rt))
        {
            //Debug.Log(rt + " -> " + ribbonMutation[rt]);
            return ribbonMutation[rt];
        }
        return rt;
    }
    public Ribbon.RibbonType UnmutateRibbonType(Ribbon.RibbonType rt)
    {
        if (ribbonMutationInverse != null && ribbonMutationInverse.ContainsKey(rt))
        {
            return ribbonMutationInverse[rt];
        }
        return rt;
    }



    //old vestige of previous system idea
    /*
    //To do later: make this capable of interacting with the cutscene queue
    public IEnumerator ExecuteCutscene(CutsceneScript cutscene)
    {
        inCutscene = true;
        yield return StartCoroutine(cutscene.Execute());
        inCutscene = false;
    }
    */

    //Various static methods / calculations

    //given a float, return a percent representation (1 -> 100, 0.5 -> 50)
    public static float Percent(float input)
    {
        return Percent(input, 0);
    }
    public static float Percent(float input, int precision)
    {
        //0 precision => integer percentages (0%, 5%, 15%)

        int scale = (int)(Mathf.Pow(10, precision) * 100);

        int b = (int)(scale * input);

        float c = b * (100f / scale);

        return c;
    } //decimal places (with 0 being integer percentages) (rounds down)
    public static string PrecisionTruncate(float input)
    {
        int tryA = (int)(input * 10 + 0.5f);
        if (Mathf.Abs((tryA / 10f) - input) < 0.0099f)
        {
            return input.ToString("0.0");
        }

        return input.ToString("0.00");
    }

    //not really useful
    //1 = 2, 2 = 3
    public static int GetPrime(int index)
    {
        List<int> oldPrimes = new List<int> { 2, 3, 5, 7, 11, 13, 17, 19 };

        int candidate = oldPrimes[oldPrimes.Count - 1] + 1;
        while (index - 1 >= oldPrimes.Count)
        {
            //populate the list with more primes
            for (int i = 0; i < oldPrimes.Count; i++)
            {
                if (candidate / oldPrimes[i] == ((float)candidate) / oldPrimes[i])
                {
                    //divisible
                    break;
                }

                if (oldPrimes[i] * oldPrimes[i] > candidate)
                {
                    //after this point none of the other primes can be divisible
                    oldPrimes.Add(candidate);
                    break;
                }
            }

            candidate++;
        }

        return oldPrimes[index - 1];
    }

    public static Color ColorMap(float map)
    {
        map = (map - 6 * Mathf.Floor(map/6));

        float r = Mathf.Clamp01(Mathf.Abs(map - 3) - 1);
        float g = Mathf.Clamp01(-Mathf.Abs(map - 2) + 2);
        float b = Mathf.Clamp01(-Mathf.Abs(map - 4) + 2);
        Color output = new Color(r, g, b);

        return output;
    }

    public static string ParseTime(float time)
    {
        if (time < 0)
        {
            return "-" + ParseTime(-time);
        }
        string output = "";

        int hours = (int)(time / 3600);
        int minutes = (int)((time - (hours * 3600)) / 60);

        if (minutes >= 60)
        {
            minutes -= 60;
            hours += 1;
        }

        int seconds = (int)(time - hours * 3600 - minutes * 60);
        if (seconds >= 60)
        {
            seconds -= 60;
            minutes += 1;
        }
        if (minutes >= 60)
        {
            minutes -= 60;
            hours += 1;
        }

        output = hours + ":" + (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds);
        return output;
    }
    
    //Empty string is none
    public static List<Color?> ParseColorList(string colorList)
    {
        List<Color?> output = new List<Color?>();

        string[] split = colorList.Split(",");

        for (int m = 0; m < split.Length; m++)
        {
            if (split[m].Length == 0)
            {
                output.Add(null);
                continue;
            }

            output.Add(ParseColor(split[m]));
        }

        return output;
    }
    //Comma separated
    //Inverse of ParseColorList
    public static string PackColorList(List<Color?> input)
    {
        string output = "";

        for (int i = 0; i < input.Count; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            if (input[i] == null)
            {
                output += "X";
            }
            else
            {
                output += ColorToString(input[i].Value);
            }
        }

        return output;
    }

    //uses pipes as sepeaator
    public static (List<Item> items, List<string> strings) ParseItemStringList(string itemList)
    {
        List<Item> outputA = new List<Item>();
        List<string> outputB = new List<string>();

        string[] split = itemList.Split("|");

        Item d = new Item(); //was itemtype
        string s = "";
        for (int m = 0; m < split.Length; m++)
        {
            if (m % 2 == 1)
            {
                s = new string(split[m]);

                outputA.Add(d);
                outputB.Add(s);
            }
            else
            {
                /*
                if (Enum.TryParse(split[m], true, out d))
                {
                }
                else
                {
                    Debug.LogError("[Item String Pair Parsing] Can't parse item type \"" + split[m] + "\"");
                }
                */
                d = Item.Parse(split[m]);
            }
        }

        return (outputA, outputB);
    }

    public static (List<KeyItem> items, List<string> strings) ParseKeyItemStringList(string itemList)
    {
        List<KeyItem> outputA = new List<KeyItem>();
        List<string> outputB = new List<string>();

        string[] split = itemList.Split("|");

        KeyItem d = new KeyItem();
        string s = "";
        for (int m = 0; m < split.Length; m++)
        {
            if (m % 2 == 1)
            {
                s = new string(split[m]);

                outputA.Add(d);
                outputB.Add(s);
            }
            else
            {
                /*
                if (Enum.TryParse(split[m], true, out d))
                {
                }
                else
                {
                    Debug.LogError("[Item String Pair Parsing] Can't parse item type \"" + split[m] + "\"");
                }
                */
                d = KeyItem.Parse(split[m]);
            }
        }

        return (outputA, outputB);
    }

    public static List<string> ParsePipeStringList(string list)
    {
        List<string> output = new List<string>();

        string[] split = list.Split("|");

        for (int m = 0; m < split.Length; m++)
        {
            output.Add(split[m]);
        }

        return output;
    }


    public static List<int> ParseIntList(string list)
    {
        List<int> output = new List<int>();

        string[] split = list.Split("|");

        for (int m = 0; m < split.Length; m++)
        {
            int.TryParse(split[m], out int test);
            output.Add(test);
        }

        return output;
    }
    public static Dictionary<T,int> ParseEnumIntList<T>(string list) where T : struct {
        Dictionary<T, int> output = new Dictionary<T, int>();

        string[] split = list.Split("|");

        for (int m = 0; m < split.Length; m++)
        {
            string[] splitB = split[m].Split(':');

            if (splitB.Length != 2)
            {
                return null;
            }

            Enum.TryParse<T>(splitB[0], true, out T a);
            int.TryParse(splitB[1], out int b);

            //hacky way to block "invalid" enum values being parsed
            if (!int.TryParse(splitB[0].ToString(), out int _))
            {
                output.Add(a, b);
            }
        }

        return output;
    }
    public static string ListToString(List<int> list)
    {
        string output = "";

        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                output += "|";
            }
            output += list[i];
        }

        return output;
    }
    public static string ListToString(int[] list)
    {
        string output = "";

        for (int i = 0; i < list.Length; i++)
        {
            if (i > 0)
            {
                output += "|";
            }
            output += list[i];
        }

        return output;
    }
    public static string EnumIndexedListToString<T>(int[] list) where T : struct
    {
        string output = "";

        for (int i = 0; i < list.Length; i++)
        {
            //currently: ignore any "invalid" values
            if (int.TryParse((Enum.Parse<T>(i.ToString())).ToString(), out int _))
            {
                continue;
            }

            if (i > 0)
            {
                output += "|";
            }

            //This is stupid but oh well
            output += (Enum.Parse<T>(i.ToString())) + ":" + list[i];
        }

        return output;
    }

    //Useful for shuffling
    //Keep stored later for reasons (so you can shuffle things in specific ways)
    public static List<int> CreateShufflePermutation(int size, int hashOffset = 0)
    {
        UnityEngine.Random.State preState = UnityEngine.Random.state;

        int hash = 0;
        if (MainManager.Instance.saveName != null)
        {
            //salt the hash I guess?
            hash = (MainManager.Instance.saveName + " " + size + " " + hashOffset).GetHashCode();
        } else
        {
            hash = (size + " " + hashOffset).GetHashCode();
        }

        UnityEngine.Random.InitState(hash);

        List<int> output = new List<int>();
        for (int i = 0; i < size; i++)
        {
            output.Add(i);
        }

        for (int j = size - 1; j > 0; j--) {
            int temp = output[j];
            int targetIndex = UnityEngine.Random.Range(0, j - 1);
            output[j] = output[targetIndex];
            output[targetIndex] = temp;
        }

        UnityEngine.Random.state = preState;
        return output;
    }
    public static List<int> InvertShufflePermutation(List<int> perm)
    {
        List<int> output = new List<int>();
        for (int i = 0; i < perm.Count; i++)
        {
            output.Add(i);
        }

        for (int i = 0; i < perm.Count; i++)
        {
            //perm: converts i to perm[i]
            //output: converts perm[i] to i
            output[perm[i]] = i;
        }

        return output;
    }
    public static List<T> ApplyShufflePermutation<T>(List<T> target, List<int> perm)
    {
        if (target.Count != perm.Count)
        {
            throw new ArgumentException("Mismatched argument lengths: " + target.Count + " vs " + perm.Count);
        }

        List<T> output = new List<T>();
        for (int i = 0; i < target.Count; i++)
        {
            output.Add(default);
        }

        for (int i = 0; i < perm.Count; i++)
        {
            output[perm[i]] = target[i];
        }

        return output;
    }
    public static List<T> ShuffleList<T>(List<T> target)    //gives you a shuffled list (this is a bit inefficient but ehh)
    {
        List<int> shuffleList = CreateShufflePermutation(target.Count);
        return ApplyShufflePermutation<T>(target, shuffleList);
    }


    //this uses the excel formatting so I can use excel to edit files
    //Excel formatting:
    //\n is used to delineate line breaks
    //Entries may or may not have "" around them
    //Within quotes, commas are treated literally
    //"" is " when within quotes
    //  Note that """ is difficult to resolve (use quotes as context)
    public static string[][] CSVParse(string s)
    {
        //split by line breaks (no way to escape them)
        string[] r = s.Split('\n');
        string[][] output = new string[r.Length][];

        bool inQuote = false;
        string temp;
        List<string> tempList = new List<string>();

        for (int i = 0; i < r.Length; i++)
        {
            tempList = new List<string>();
            //Parse the line of data
            temp = "";
            for (int j = 0; j < r[i].Length; j++)
            {
                if (r[i][j] == '"')
                {
                    if (inQuote && j < r[i].Length - 1 && r[i][j + 1] == '"')
                    {
                        temp += '"';
                        j++; //skip both "s (with the j++ below)
                    } else
                    {
                        inQuote = !inQuote;
                    }
                    j++; //Skip adding "
                }

                if (j >= r[i].Length)
                {
                    break;
                }

                if (inQuote)
                {
                    temp += r[i][j];
                } else
                {
                    if (r[i][j] == ',')
                    {
                        tempList.Add(temp.Replace("\r",""));
                        temp = "";
                    } else
                    {
                        temp += r[i][j];
                    }
                }
            }

            //one more
            tempList.Add(temp.Replace("\r", ""));

            output[i] = (string[])tempList.ToArray().Clone();
        }

        return output;
    }

    //dictionaries
    //A single line in the save file contains all the bools
    public static Dictionary<GlobalFlag, bool> ParseGlobalFlagString(string input)
    {
        Dictionary<GlobalFlag, bool> output = new Dictionary<GlobalFlag, bool>();

        if (input.Replace("\r", "").Length == 0)
        {
            return output;
        }

        string[] split = input.Split(",");

        if ((split.Length == 0) || (split.Length == 1 && split[0].Length < 1))
        {
            return output;
        }

        for (int i = 0; i < split.Length; i++)
        {
            string[] tempSplit = split[i].Split(":");

            GlobalFlag temp = (GlobalFlag)(-1);
            bool tempBool = false;

            if (Enum.TryParse(tempSplit[0], true, out temp))
            {

            } else
            {
                Debug.LogWarning("[ParseGlobalFlagString] Could not parse GlobalFlag " + tempSplit[0]);
            }

            if (tempSplit.Length > 1 && bool.TryParse(tempSplit[1], out tempBool))
            {

            } else
            {
                Debug.LogWarning("[ParseGlobalFlagString] Could not parse bool " + (tempSplit.Length < 2 ? "??" : tempSplit[1]));
            }

            if ((int)temp != -1)
            {
                if (output.ContainsKey(temp))
                {
                    Debug.LogWarning("File has more than 1 game flag entry for " + temp + " in the save file: Old: " + output[temp] + " vs new " + tempBool);
                    output[temp] = tempBool;
                } else
                {
                    output.Add(temp, tempBool);
                }
            }
        }
        return output;
    }
    //should be the inverse of the above
    public static string UnparseGlobalFlagDictionary(Dictionary<GlobalFlag, bool> input)
    {
        string output = "";

        int i = 0;
        foreach (KeyValuePair<GlobalFlag, bool> kvp in input)
        {
            i = i + 1;
            if (i > 1)
            {
                output += ",";
            }
            output += kvp.Key.ToString() + ":" + kvp.Value.ToString();
        }

        return output;
    }

    //Global vars are the same way
    //Be careful to forbid \n, commas and :
    public static Dictionary<GlobalVar, string> ParseGlobalVarString(string input)
    {
        Dictionary<GlobalVar, string> output = new Dictionary<GlobalVar, string>();

        if (input.Replace("\r", "").Length == 0)
        {
            return output;
        }

        string[] split = input.Split(",");

        if ((split.Length == 0) || (split.Length == 1 && split[0].Length < 1))
        {
            return output;
        }

        for (int i = 0; i < split.Length; i++)
        {
            string[] tempSplit = split[i].Split(":");

            GlobalVar temp = (GlobalVar)(-1);
            string tempStr = tempSplit.Length > 1 ? tempSplit[1] : null;

            if (Enum.TryParse(tempSplit[0], true, out temp))
            {

            }
            else
            {
                Debug.LogWarning("[ParseGlobalVarString] Could not parse GlobalVar " + tempSplit[0]);
            }


            if ((int)temp != -1)
            {
                if (output.ContainsKey(temp))
                {
                    Debug.LogWarning("File has more than 1 game var entry for " + temp + " in the save file: Old: " + output[temp] + " vs new " + tempStr);
                    output[temp] = tempStr;
                }
                else
                {
                    output.Add(temp, tempStr);
                }
            }
        }
        return output;
    }
    //should be the inverse of the above
    public static string UnparseGlobalVarDictionary(Dictionary<GlobalVar, string> input)
    {
        string output = "";

        int i = 0;
        foreach (KeyValuePair<GlobalVar, string> kvp in input)
        {
            i = i + 1;
            if (i > 1)
            {
                output += ",";
            }
            output += kvp.Key.ToString() + ":" + kvp.Value.ToString();
        }

        return output;
    }

    public static int SubtractPreserveSign(int a, int b)
    {
        if (a - b < 0 && a >= 0)
        {
            return 0;
        }

        if (a - b > 0 && a < 0)
        {
            return 0;
        }

        return a - b;
    }

    //ties lead to the later one being chosen
    public static T GetHighest<T>(List<T> list, Comparer<T> comp)
    {
        if (list.Count == 0)
        {
            return default; //what
        }
        int highIndex = 0;

        for (int i = 0; i < list.Count; i++)
        {
            if (comp.Compare(list[i], list[highIndex]) >= 0)
            {
                highIndex = i;
            }
        }

        return list[highIndex];
    }
    public static T GetHighest<T>(List<T> list, Comparison<T> comp)
    {
        return GetHighest(list, Comparer<T>.Create(comp));
    }
    public static List<T> GetHighestNumber<T>(List<T> list, Comparer<T> comp, int count)
    {
        if (count <= 0)
        {
            return default;
        }

        if (count == 1)
        {
            List<T> o = new List<T>();
            o.Add(GetHighest(list, comp));
            return o;
        }

        List<T> cList = list.ConvertAll((e) => (e));
        cList.Sort(comp);

        List<T> oList = new List<T>();

        for (int i = 0; i < count; i++)
        {
            oList.Add(cList[cList.Count - 1]);
            cList.RemoveAt(cList.Count - 1);
            if (cList.Count == 0)
            {
                break;
            }
        }

        return oList;
    } //Note that having a count that is too high will return only what it can (so you end up with just a sorted list)
    public static List<T> GetHighestNumber<T>(List<T> list, Comparison<T> comp, int count)
    {
        return GetHighestNumber(list, Comparer<T>.Create(comp), count);
    }

    public static int FloatCompare(float a, float b)
    {
        if (a - b > 0)
        {
            return 1;
        } else if (a - b < 0)
        {
            return -1;
        } else
        {
            return 0;
        }
    }

    //formula A: atk - def
    //formula B (current): atk^2 / 4*def for (atk < 2def), atk - def for (atk >= 2def)
    //formula C: atk^2 / def + atk for (def > 0), atk - def for (def < 0)
    //  both formula B and C approach formula A, but B eventually reaches A at atk >= 2def
    //  formula C makes defense less effective

    //ehh, for now just use formula A because it is simpler
    public static int DamageFormula(int atk, int def)
    {
        return atk - def < 0 ? 0 : atk - def;

        /*
        if (atk - def <= 0)
        {
            return 0;
        }
        if (atk / 2 >= def)
        {
            return (int)Mathf.Clamp(atk - def, 0, float.MaxValue);
        }
        else
        {
            return (int)Mathf.Clamp((atk * atk) / (4 * def), 0, float.MaxValue);
        }
        */
    }
    public static int DamageReductionFormula(int atk, float red)
    {
        int value = (int)((2.0f * atk) / (2 + red));

        //Special formula for negative reduction
        if (red < 0)
        {
            value = (int)(((-red / 2.0f) + 1) * atk);
        }

        if (value < 1 && atk > 0)
        {
            value = 1;
        }

        return value;
    }

    public static Vector3 ParseVector3(string entry)
    {
        string[] entries = entry.Split("|");
        Vector3 output = Vector3.zero;

        float x = 0;
        float y = 0;
        float z = 0;

        if (entries.Length > 3)
        {
            Debug.LogWarning("[Vector3 Parsing] Vector3 is too long: (" + entry + "), superfluous numbers will be ignored");
        }

        //Note: zero length string will end up parsing to 0,0,0
        if (entries.Length > 0)
        {
            float.TryParse(entries[0], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x);
        }

        if (entries.Length > 1)
        {
            float.TryParse(entries[1], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y);
        }

        if (entries.Length > 2)
        {
            float.TryParse(entries[2], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out z);
        }

        output = Vector3.right * x + Vector3.up * y + Vector3.forward * z;
        return output;
    }
    public static string Vector3ToString(Vector3 v)
    {
        string output = v.x + "|" + v.y + "|" + v.z;
        return output;
    }

    public static int ParseHex2Byte(string parse)
    {
        //Debug.Log(parse + " " + parse.Length);
        if (parse.Length != 2)
        {
            return 0;
        }

        int CharToInt(char a)
        {
            //Debug.Log("ith " + a);
            if (int.TryParse(a + "", out int a2))
            {
                return a2;
            }
            else
            {
                return (a - 'a' + 10);
            }
        }

        char a = parse[0];
        char b = parse[1];

        int a2 = CharToInt(a);
        int b2 = CharToInt(b);

        return a2 * 16 + b2;
    }

    public static Color? ParseColor(string parse)
    {
        int CharToInt(char a)
        {
            //Debug.Log("ith " + a);
            if (int.TryParse(a + "", out int a2))
            {
                return a2;
            }
            else
            {
                return (a - 'a' + 10);
            }
        }

        //v2 version
        if (parse.Length > 1 && parse[0] == '#')
        {
            //Next up is a hex code thing
            string hexValue = parse.Substring(1);

            //something like FFFFFFFF might not work with ToInt32 since that returns an int not a uint
            //long colorVal = Convert.ToInt64("0x" + hexValue, 16);
            //uint realcolor = (uint)colorVal;

            //check correctness
            if (hexValue.Length != 4 && hexValue.Length != 6 && hexValue.Length != 8)
            {
                //Wrong length
                //Debug.Log("Wrong length " + hexValue + " has length " + hexValue.Length);
                return null;
            }

            float red = 0;
            float green = 0;
            float blue = 0;
            float alpha = 0;
            switch (hexValue.Length)
            {
                /*
                case 4: //weird 16 bit format (5 bits each for rgb, 1 bit for a) (
                    red = ((((realcolor) % (1 << 16)) >> 11) / 31f);
                    green = ((((realcolor) % (1 << 11)) >> 6) / 31f);
                    blue = ((((realcolor) % (1 << 6)) >> 1) / 31f);
                    alpha = ((((realcolor) % (1 << 1)) >> 0) / 1f);
                    break;
                */
                case 4: //rgba
                    red = CharToInt(hexValue[0]) / 15f;
                    green = CharToInt(hexValue[0]) / 15f;
                    blue = CharToInt(hexValue[0]) / 15f;
                    alpha = CharToInt(hexValue[0]) / 15f;
                    //red = ((((realcolor) % (1 << 16)) >> 12) / 15f);
                    //green = ((((realcolor) % (1 << 12)) >> 8) / 15f);
                    //blue = ((((realcolor) % (1 << 8)) >> 4) / 15f);
                    //alpha = ((((realcolor) % (1 << 4)) >> 0) / 15f);
                    break;
                case 6: //rrggbb
                    red = ParseHex2Byte(hexValue[0] + "" + hexValue[1]) / 255f;
                    green = ParseHex2Byte(hexValue[2] + "" + hexValue[3]) / 255f;
                    blue = ParseHex2Byte(hexValue[4] + "" + hexValue[5]) / 255f;
                    alpha = 1;
                    //red = ((((realcolor) % (1 << 24)) >> 16) / 255f);
                    //green = ((((realcolor) % (1 << 16)) >> 8) / 255f);
                    //blue = ((((realcolor) % (1 << 8)) >> 0) / 255f);
                    //alpha = 1;
                    break;
                case 8: //rrggbbaa
                    red = ParseHex2Byte(hexValue[0] + "" + hexValue[1]) / 255f;
                    green = ParseHex2Byte(hexValue[2] + "" + hexValue[3]) / 255f;
                    blue = ParseHex2Byte(hexValue[4] + "" + hexValue[5]) / 255f;
                    alpha = ParseHex2Byte(hexValue[6] + "" + hexValue[7]) / 255f;
                    //red = ((((realcolor) % (1L << 32)) >> 24) / 255f);
                    //green = ((((realcolor) % (1 << 24)) >> 16) / 255f);
                    //blue = ((((realcolor) % (1 << 16)) >> 8) / 255f);
                    //alpha = ((((realcolor) % (1 << 8)) >> 0) / 255f);
                    break;
            }

            //Debug.Log(parse + " Colors: " + red + " " + green + " " + blue + " " + alpha);
            return new Color(red, green, blue, alpha);
        }
        else
        {
            //Debug.Log("Invalid start " + parse);
            return null;
        }

        /*
        //fail parse: null
        try
        {
            if (parse.Length > 1 && parse[0] == '#')
            {
                //Next up is a hex code thing
                string hexValue = parse.Substring(1);

                //something like FFFFFFFF might not work with ToInt32 since that returns an int not a uint
                long colorVal = Convert.ToInt64("0x" + hexValue, 16);
                uint realcolor = (uint)colorVal;

                //check correctness
                if (hexValue.Length != 4 && hexValue.Length != 6 && hexValue.Length != 8)
                {
                    //Wrong length
                    //Debug.Log("Wrong length " + hexValue + " has length " + hexValue.Length);
                    return null;
                }

                float red = 0;
                float green = 0;
                float blue = 0;
                float alpha = 0;
                switch (hexValue.Length)
                {
                    
                    case 4: //weird 16 bit format (5 bits each for rgb, 1 bit for a) (
                        red = ((((realcolor) % (1 << 16)) >> 11) / 31f);
                        green = ((((realcolor) % (1 << 11)) >> 6) / 31f);
                        blue = ((((realcolor) % (1 << 6)) >> 1) / 31f);
                        alpha = ((((realcolor) % (1 << 1)) >> 0) / 1f);
                        break;
                    
                    case 4: //rgba
                        red = ((((realcolor) % (1 << 16)) >> 12) / 15f);
                        green = ((((realcolor) % (1 << 12)) >> 8) / 15f);
                        blue = ((((realcolor) % (1 << 8)) >> 4) / 15f);
                        alpha = ((((realcolor) % (1 << 4)) >> 0) / 15f);
                        break;
                    case 6: //rrggbb
                        red = ((((realcolor) % (1 << 24)) >> 16) / 255f);
                        green = ((((realcolor) % (1 << 16)) >> 8) / 255f);
                        blue = ((((realcolor) % (1 << 8)) >> 0) / 255f);
                        alpha = 1;
                        break;
                    case 8: //rrggbbaa
                        red = ((((realcolor) % (1L << 32)) >> 24) / 255f);
                        green = ((((realcolor) % (1 << 24)) >> 16) / 255f);
                        blue = ((((realcolor) % (1 << 16)) >> 8) / 255f);
                        alpha = ((((realcolor) % (1 << 8)) >> 0) / 255f);
                        break;
                }

                //Debug.Log("Colors: " + red + " " + green + " " + blue + " " + alpha);
                return new Color(red, green, blue, alpha);
            }
            else
            {
                //Debug.Log("Invalid start " + parse);
                return null;
            }

        } catch (FormatException)
        {
            Debug.Log("General parse failure: " + parse);
            return null;
        }
        */
    }
    public static string ColorToString(Color a)
    {
        //inverse of parse color
        string output = "#";

        int intVal = (int)(255 * a.r + 0.5f);

        string IntToHex(int a)
        {
            //Debug.Log("ith " + a);
            if (a < 10)
            {
                return a.ToString();
            } else
            {
                return ((char)('a' + (char)(a - 10))).ToString();
            }
        }

        output += IntToHex(intVal / 16);
        output += IntToHex(intVal % 16);
        intVal = (int)(255 * (a.g) + 0.5f);
        output += IntToHex(intVal / 16);
        output += IntToHex(intVal % 16);
        intVal = (int)(255 * (a.b) + 0.5f);
        output += IntToHex(intVal / 16);
        output += IntToHex(intVal % 16);

        if (a.a != 1)
        {
            intVal = (int)(255 * a.a + 0.5f);
            output += IntToHex(intVal / 16);
            output += IntToHex(intVal % 16);
        }

        //Debug.Log(a + " " + output);

        return output;
    }

    //a multiplier
    //enemy true level is ~5 + A above level used in formula (A is a constant dependent on enemy characteristics)
    //starts at level 1 (=100%)
    //level 19 = 400%
    //level 22 = 450%
    //level 25 = 500%
    public static float StatMultiplier(int oldLevel, int newLevel)
    {
        float oMult = 1.0f + (oldLevel - 1) * (1.0f / 6);
        float nMult = 1.0f + (newLevel - 1) * (1.0f / 6);
        return nMult / oMult;
    }
    //1 to 3
    public static float WeakStatMultiplier(int oldLevel, int newLevel)
    {
        float oMult = 1.0f + (oldLevel - 1) * (1.0f / 12);
        float nMult = 1.0f + (newLevel - 1) * (1.0f / 12);
        return nMult / oMult;
    }

    public static int XPCalculation(int startEnemyCount, int playerLevel, int enemyLevel, int enemyBonusXP)
    {
        float bonus = 0.75f;
        bonus = 0.775f + 0.075f * (startEnemyCount - 2) * (startEnemyCount - 3);

        if (bonus > 1.2f)
        {
            bonus = 1.2f;
        }

        int levelDiff = (enemyLevel - playerLevel);

        if (levelDiff <= 0)
        {
            return 0;
        }

        levelDiff += enemyBonusXP;
        float value = bonus * levelDiff;

        return Mathf.CeilToInt(value);
    }


    //Might use this, might not
    //Useful for real time based stuff
    //Time values are a bit sus due to time zones so this is the most "safe" way of doing things?
    //  Don't want to do time zone related stuff so things should run based on cycles or relative things
    //  (e.g. waiting 20 ish hours for a daily thing?)
    public static long GetUnixTime()
    {
        return (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }


    //Helpful math
    //Bezier curves
    //they use a lot of lerping
    public static Vector3 BezierCurve(float t, params Vector3[] points)
    {
        if (points.Length < 1)
        {
            throw new ArgumentException("Need more than 0 arguments.");
        }
        else if (points.Length == 1)
        {
            //degenerate case
            return points[0];
        }
        else if (points.Length == 2)
        {
            //one lerp
            return Vector3.LerpUnclamped(points[0], points[1], t);
        }
        else
        {
            //Pure math version
            //Doing everything in one step
            //2
            //(1-t)P1 + tP2
            //3
            //(1-t)^2 P1 + 2(1-t)t P2 + t^2 P3
            //4
            //(1-t^3 P1 + 3(1-t)^2 t P2 + 3(1-t) t^3 P3 + t^4 P4
            //The implementation below works fine so I won't use this one

            //Less memory intensive version
            //Uses a number of points equal to the curve order
            //Values are reused for later calculations
            Vector3[] tempArray = new Vector3[points.Length];
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = points[i];
            }
            int finalIndex = points.Length - 1;
            while (finalIndex > 0)
            {
                for (int j = 0; j < finalIndex; j++)
                {
                    tempArray[j] = Vector3.LerpUnclamped(tempArray[j], tempArray[j + 1], t);
                }
                finalIndex--;
            }
            return tempArray[0];

            //Recursive version
            /*
            //lerp between each pair of points to get another set of points (one less than you had before)
            //repeat until you have one point left
            Vector3[] pointset = new Vector3[points.Length - 1];
            for (int i = 0; i < pointset.Length; i++)
            {
                pointset[i] = Vector3.Lerp(points[i], points[i + 1], t);
            }
            return BezierCurve(t, pointset);
            */
        }
    }

    //quadratic easing (positive = fast at start, slow at end, negative = slow at start, fast at end. Heaviness values outside [-1, 1] will return values outside [0,1] near the slow end but will rebound)
    //(graph out the formula for more specifics)
    public static float EasingQuadratic(float input, float heaviness)
    {
        return input * (1 + heaviness) - input * input * heaviness;
    }

    //Watch out for impossible values
    //(Also watch out for ambiguous values with high heaviness)
    public static float InverseEasingQuadratic(float input, float heaviness)
    {
        float h = heaviness;
        float x = input;

        float termA = (h + 1) / (2 * h);
        float termB = Mathf.Sqrt(h * h - 4 * h * x + 2 * h + 1) / (2 * h);

        return termA - termB;
    }


    //exponential easing (positive = fast at start, negative = fast at end. Heaviness can be anything
    //(graph out the formula for more specifics)
    public static float EasingExponential(float input, float heaviness)
    {
        return (1 / (1 - Mathf.Exp(-heaviness)) * (1 - Mathf.Exp(-heaviness * input)));
    }
    //springy easing (heaviness needs to be positive, each +1 to rotation value adds a half oscillation
    //More heaviness = less springiness
    //The formula can go from 2 to 0 (but may not depending on exact values)
    //(graph out the formula for more specifics)
    public static float EasingSpringy(float input, float heaviness, float omega)
    {
        return (1 - (Mathf.Exp(-heaviness * input) * Mathf.Cos((omega + 0.5f) * Mathf.PI * input)));
    }

    //Unlike the exponential easings, this one has a set time it will reach the end
    //Time to target = sqrt(abs(input - target) / force)
    //Inverse (force) = (abs(input - target) / time^2)
    //Note: calculating a force from a time value would need to use the starting point
    //(and my easing functions are specifically designed so that the starting point is unnecessary)
    public static float EasingQuadraticTime(float input, float target, float force)
    {
        force = Mathf.Abs(force);

        //x - offset
        //the x value is the next value the formula should take (the next input to feed back into this)
        if (input - target < 0)
        {
            force = -force;
        }
        float formulaInput = Time.smoothDeltaTime - Mathf.Sqrt(Mathf.Abs((input - target) / force));

        //point where you reach the target is at x = 0
        if (formulaInput > 0)
        {
            return target;
        }

        float output = force * formulaInput * formulaInput + target;

        return output;
    }
    public static Vector3 EasingQuadraticTime(Vector3 input, Vector3 target, float force)
    {
        //sus code
        return new Vector3(EasingQuadraticTime(input.x, target.x, force), EasingQuadraticTime(input.y, target.y, force), EasingQuadraticTime(input.z, target.z, force));
    }
    public static Quaternion EasingQuadraticTime(Quaternion input, Quaternion target, float force)
    {
        //why is the description for the w part different from the x,y,z parts? sus
        //well interpolating like this works fine anyway
        return new Quaternion(EasingQuadraticTime(input.x, target.x, force), EasingQuadraticTime(input.y, target.y, force), EasingQuadraticTime(input.z, target.z, force), EasingQuadraticTime(input.w, target.w, force));
    }
    public static float EasingExponentialTime(float input, float target, float halflife = 0.1f)
    {
        // special formula for use in cases where you only have current position and target position
        //usage: (pos) = eet(pos, target, strength)

        // multiply remaining distance by (strength ^ deltatime)
        //Remaining distance = input - target

        //input -> input + (target - input) * (1 - prop)

        //Calculating half life
        //h = log(0.5) / log(strength)

        //calculating strength from half life
        //strength = 0.5 ^ (1/h)

        if (halflife == 0)
        {
            return target;
        }

        float strength = Mathf.Pow(0.5f, 1 / halflife);

        return (target - input) * (1 - Mathf.Pow(strength, Time.smoothDeltaTime)) + input;
    }

    public static Vector3 EasingExponentialTime(Vector3 input, Vector3 target, float halflife = 0.1f)
    {
        // special formula for use in cases where you only have current position and target position
        //usage: (pos) = eet(pos, target, strength)

        if (halflife == 0) 
        {
            return target;
        }

        float strength = Mathf.Pow(0.5f, 1 / halflife);

        return (target - input) * (1 - Mathf.Pow(strength, Time.smoothDeltaTime)) + input;
    }

    public static Quaternion EasingExponentialTime(Quaternion input, Quaternion target, float halflife = 0.1f)
    {
        // special formula for use in cases where you only have current position and target position
        //usage: (pos) = eet(pos, target, strength)

        if (halflife == 0)
        {
            return target;
        }

        float strength = Mathf.Pow(0.5f, 1 / halflife);

        return Quaternion.Lerp(input, target, 1 - Mathf.Pow(strength, Time.smoothDeltaTime));//(target - input) * (1 - Mathf.Pow(strength, Time.smoothDeltaTime)) + input;
    }

    public static float EasingExponentialFixedTime(float input, float target, float halflife = 0.1f)
    {
        // special formula for use in cases where you only have current position and target position
        //usage: (pos) = eet(pos, target, strength)

        // multiply remaining distance by (strength ^ deltatime)
        //Remaining distance = input - target

        //input -> input + (target - input) * (1 - prop)

        //Calculating half life
        //h = log(0.5) / log(strength)

        //calculating strength from half life
        //strength = 0.5 ^ (1/h)

        if (halflife == 0)
        {
            return target;
        }

        float strength = Mathf.Pow(0.5f, 1 / halflife);

        return (target - input) * (1 - Mathf.Pow(strength, Time.fixedDeltaTime)) + input;
    }

    public static Vector3 EasingExponentialFixedTime(Vector3 input, Vector3 target, float halflife = 0.1f)
    {
        // special formula for use in cases where you only have current position and target position
        //usage: (pos) = eet(pos, target, strength)

        // multiply remaining distance by (strength ^ deltatime)
        //Remaining distance = input - target

        //input -> input + (target - input) * (1 - prop)

        //Calculating half life
        //h = log(0.5) / log(strength)

        //calculating strength from half life
        //strength = 0.5 ^ (1/h)

        if (halflife == 0)
        {
            return target;
        }

        float strength = Mathf.Pow(0.5f, 1 / halflife);

        return (target - input) * (1 - Mathf.Pow(strength, Time.fixedDeltaTime)) + input;
    }

    //how to make another easing function
    //for some function f(x, C)
    //  calculate f(f^-1 (x, C) + dt , C)
    //  x is input position
    //  C is arbitrary constants


    public static Vector2 XZProject(Vector3 v)
    {
        return v.x * Vector2.right + v.z * Vector2.up;
    }

    public static Vector3 XZProjectPreserve(Vector3 v)
    {
        return v.x * Vector3.right + v.z * Vector3.forward;
    }

    public static Vector3 XZProjectReverse(Vector2 v)
    {
        return v.x * Vector3.right + v.y * Vector3.forward;
    }

    public static void ListPrint<T>(IList<T> list)
    {
        if (list == null)
        {
            return;
        }
        string output = "";
        for (int i = 0; i < list.Count; i++)
        {
            output += "(";
            output += list[i].ToString();
            output += ")";

            if (i < list.Count - 1)
            {
                output += ", ";
            }
        }

        if (output.Length == 0)
        {
            Debug.Log("[empty list]");
        } else
        {
            Debug.Log(output);
        }
    }

    public static void ListPrint<T>(List<T> list, int index, int max = 5)
    {
        if (list == null)
        {
            return;
        }
        bool leftOmit = false;
        bool rightOmit = false;

        string output = "";
        for (int i = 0; i < list.Count; i++)
        {
            if (i < index - max)
            {
                if (!leftOmit)
                {
                    output += "...";
                }
                leftOmit = true;
                continue;
            }
            if (i > index + max)
            {
                rightOmit = true;
                continue;
            }

            if (i == index)
            {
                output += "[";
            }
            output += "(";
            output += list[i].ToString();
            output += ")";
            if (i == index)
            {
                output += "]";
            }

            if (i < list.Count - 1)
            {
                output += ", ";
            }
        }

        if (rightOmit)
        {
            output = output + "...";
        }

        Debug.Log(output);
    }



    //auditor stuff
    public static void MapAudit()
    {
        if (FindObjectOfType<MapScript>() == null)
        {
            Debug.LogWarning("No map to audit");
            return;
        }

        Debug.Log("-- Auditing Entities --");
        List<WorldEntity> entityList = new List<WorldEntity>(FindObjectsOfType<WorldEntity>());

        List<WorldEntity> idList = new List<WorldEntity>();

        for (int i = 0; i < entityList.Count; i++)
        {
            if (entityList[i].meid < 0)
            {
                if (entityList[i].name.Contains("WorldPlayer") && entityList[i].meid == -1)
                {
                    continue;
                }
                if (entityList[i].name.Contains("WorldFollower") && entityList[i].meid == -2)
                {
                    continue;
                }
                Debug.LogWarning(entityList[i].name + " has a meid below 0 and isn't a special entity: " + entityList[i].meid);
                continue;
            }

            while (idList.Count < entityList[i].meid + 1)
            {
                idList.Add(null);
            }

            if (idList[entityList[i].meid] != null)
            {
                Debug.LogWarning("Duplicate meid: " + idList[entityList[i].meid].name + " and " + entityList[i].name + " share an ID " + entityList[i].meid);
            }
            idList[entityList[i].meid] = entityList[i];
        }

        for (int i = 0; i < idList.Count; i++)
        {
            if (idList[i] == null)
            {
                Debug.LogWarning("Skipped meid: " + i);
                continue;
            }
        }

        Debug.Log("-- Auditing NPCs --");
        List<WorldNPCEntity> npcList = new List<WorldNPCEntity>(FindObjectsOfType<WorldNPCEntity>());
        foreach (WorldNPCEntity npc in npcList)
        {
            foreach (EncounterData.EncounterDataEntry ede in npc.encounter.encounterList)
            {
                Enum.TryParse(ede.entid, out BattleHelper.EntityID eid);
                if (eid == BattleHelper.EntityID.DebugEntity && !(ede.entid.Equals("DebugEntity")))
                {
                    Debug.LogError("NPC Entity has an invalid entity id in the encounter");
                }

                //Note: enums can parse numbers as enum values?
                if (int.TryParse(ede.entid, out int c))
                {
                    Debug.LogWarning("Parsing a number as an entity ID: " + c);
                }
            }
        }

        Debug.Log("-- Auditing Enemies --");
        List<WorldEnemyEntity> enemyList = new List<WorldEnemyEntity>(FindObjectsOfType<WorldEnemyEntity>());

        //foreach in a foreach is a bit sussy
        foreach (WorldEnemyEntity enemy in enemyList)
        {
            if (enemy.encounter.encounterList.Count == 0)
            {
                Debug.LogWarning("Enemy Entity has a 0 enemy encounter");
            }

            //encounter list check
            foreach (EncounterData.EncounterDataEntry ede in enemy.encounter.encounterList)
            {
                Enum.TryParse(ede.entid, out BattleHelper.EntityID eid);
                if (eid == BattleHelper.EntityID.DebugEntity && !(ede.entid.Equals("DebugEntity")))
                {
                    Debug.LogError("Enemy Entity has an invalid entity id in the encounter");
                }

                if (int.TryParse(ede.entid, out int c))
                {
                    Debug.LogWarning("Parsing a number as an entity ID: " + c);
                }
            }
        }


        Debug.Log("-- Auditing Collectibles and Shop Items --");
        List<WorldCollectibleScript> collectibleList = new List<WorldCollectibleScript>(FindObjectsOfType<WorldCollectibleScript>());
        for (int i = 0; i < collectibleList.Count; i++)
        {
            if (collectibleList[i].maxLifetime != 0)
            {
                Debug.LogWarning("Collectible " + collectibleList[i] + " has a max lifetime other than 0");
            }

            if (collectibleList[i].pickupUnion.type == PickupUnion.PickupType.Item && collectibleList[i].pickupUnion.item.origin != Item.ItemOrigin.Overworld)
            {
                Debug.LogWarning("Collectible " + collectibleList[i] + " is an overworld item that does not have ItemOrigin.Overworld. (Not always a mistake)");
            }
        }

        List<ShopItemScript> shopList = new List<ShopItemScript>(FindObjectsOfType<ShopItemScript>());
        for (int i = 0; i < shopList.Count; i++)
        {
            if (shopList[i].shopItem.pickupUnion.type == PickupUnion.PickupType.Item && shopList[i].shopItem.pickupUnion.item.origin != Item.ItemOrigin.Shop)
            {
                Debug.LogWarning("Shop item " + shopList[i] + " is a shop item that does not have ItemOrigin.Shop. (Not always a mistake)");
            }

            if (shopList[i].shopkeeperEntity == null)
            {
                Debug.LogError("Shop item " + shopList[i] + " is not associated with a shopkeeper");
            }

            if (shopList[i].sz == null)
            {
                Debug.LogError("Shop item " + shopList[i] + " is not associated with a shop zone");
            }

            if (shopList[i].sprite == null)
            {
                Debug.LogError("Shop item " + shopList[i] + " doesn't have a sprite");
            }
        }

        Debug.Log("-- Auditing Map Script and Map Exits --");
        string name = FindObjectOfType<MapScript>().mapName;
        if (Enum.TryParse(name, out MainManager.MapID mapID))
        {
            if (int.TryParse(name, out int mapIDInt))
            {
                Debug.LogWarning("Map Name is an int instead of an enum value: " + mapIDInt + " when it should be " + (MainManager.MapID)mapIDInt);
            }
        }
        else
        {
            Debug.LogError("Map Name does not parse to a valid enum ID: " + name);
        }
        Debug.Log("MapName: " + name);
        if (FindObjectOfType<MapScript>().playerHolder == null)
        {
            Debug.LogError("Map " + name + " does not have a playerholder");
        }

        List<MapExit> exitList = new List<MapExit>(FindObjectsOfType<MapExit>());
        List<MapExit> seenExitIDs = new List<MapExit>();
        for (int i = 0; i < exitList.Count; i++)
        {
            if (seenExitIDs.Count < exitList[i].exitID - 1 || seenExitIDs[exitList[i].exitID] == null)
            {
                while (seenExitIDs.Count < exitList[i].exitID + 1)
                {
                    seenExitIDs.Add(null);
                }
                seenExitIDs[exitList[i].exitID] = exitList[i];
            }
            else
            {
                Debug.LogWarning("Duplicate Exit ID detected: " + exitList[i].name + " with id " + exitList[i].exitID + " shared with " + seenExitIDs[exitList[i].exitID].name);
            }

            if (Enum.TryParse(exitList[i].nextMap, out MainManager.MapID exitID))
            {
                if (int.TryParse(exitList[i].nextMap, out int exitIDInt))
                {
                    if (exitIDInt < 1)
                    {
                        Debug.LogError("Exit target of " + exitList[i].name + "  is an invalid int instead of an enum value: " + exitIDInt);
                    }
                    else
                    {
                        Debug.LogWarning("Exit target of " + exitList[i].name + "  is an int instead of an enum value: " + exitIDInt + " when it should be " + (MainManager.MapID)exitIDInt);
                    }
                }
                else
                {
                    if (exitID == MainManager.MapID.None)
                    {
                        Debug.LogError("Exit target of " + exitList[i].name + "  is invalid (None)");
                    }
                }
            }
            else
            {
                Debug.LogError("Exit target of " + exitList[i].name + " does not parse to a valid enum ID: " + exitList[i].nextMap);
            }

        }

        for (int i = 0; i < seenExitIDs.Count; i++)
        {
            if (seenExitIDs[i] == null)
            {
                Debug.LogWarning("Skipped exit id: " + i);
                continue;
            }
        }
    }
}
