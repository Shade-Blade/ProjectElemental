using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static MainManager;

public class PitObstacleScript : WorldObject, IInteractable, ITextSpeaker
{
    public enum PitObstacleType
    {
        Random,
        Slash,  //cost = level
        Aetherize,
        Smash,  //cost = level
        Illuminate,
        DoubleJump,
        SuperJump,
        DashHop,
        Dig,
        CoinLock,
        HealthLock,
        EnergyLock,
        SoulLock,
        AstralLock,
        EnemyLock,
        CrystalLock
    }
    public PitObstacleType type;
    public int cost;

    //debug
    public int floor;

    public bool open;
    public bool setup;

    public GameObject subobject;
    public PitObstacleBoxScript pobs;
    public WorldCollectibleScript collectible;
    public SpriteRenderer sprite;
    public InteractTrigger it;
    public TextDisplayer text;
    public TMP_Text tmp_text;

    public WorldEnemyEntity wee;

    public WorldPlayer wp;

    public Sprite[] obstacleSprites;

    public PickupUnion pu;

    public MeshRenderer mr;
    public List<Material> materials;


    public void Start()
    {
        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        }
        floor = int.Parse(floorNo);

        switch (((floor - 1) / 10) % 10)
        {
            case 0:
                mr.material = materials[0];
                break;
            case 1:
                mr.material = materials[1];
                break;
            case 2:
                mr.material = materials[2];
                break;
            case 3:
                mr.material = materials[3];
                break;
            case 4:
                mr.material = materials[4];
                break;
            case 5:
                mr.material = materials[5];
                break;
            case 6:
                mr.material = materials[6];
                break;
            case 7:
                mr.material = materials[7];
                break;
            case 8:
                mr.material = materials[8];
                break;
            case 9:
                mr.material = materials[9];
                break;
        }

        if (!setup)
        {
            Setup();
        }
    }
    public void Setup()
    {
        //Set random
        if (type == PitObstacleType.Random)
        {
            ChooseRandomReward(floor);
            ChooseRandomType();
        }


        setup = true;
        it.interactable = this;
        pobs.pos = this;
        wp = WorldPlayer.Instance;
        text.SetText(cost + "", true);

        switch (type)
        {
            case PitObstacleType.EnemyLock:
                Destroy(text.gameObject);
                break;
            case PitObstacleType.Slash:
            case PitObstacleType.Aetherize:
            case PitObstacleType.Smash:
            case PitObstacleType.Illuminate:
            case PitObstacleType.DoubleJump:
            case PitObstacleType.SuperJump:
            case PitObstacleType.DashHop:
            case PitObstacleType.Dig:
                Destroy(text.gameObject);
                Destroy(it.gameObject);
                break;
        }

        if (type != PitObstacleType.EnemyLock)
        {
            Destroy(wee.gameObject);
        } else
        {
            //need to not have wee.Setup() activate before this point so I have disabled the object
            int effectiveFloor = Mathf.Max(cost / 2, (int)(floor * 0.75f));
            if (effectiveFloor > floor + 30)    //really high level floor enemies are hard
            {
                effectiveFloor = (effectiveFloor - floor - 30) / 2 + floor + 30;
            }
            wee.encounter = EncounterData.GeneratePitEncounter(effectiveFloor, 5);
            wee.spriteID = EnemyBuilder.EntityIDToSpriteID(Enum.Parse<BattleHelper.EntityID>(wee.encounter.encounterList[0].entid, true)).ToString();

            //this calls awake
            wee.gameObject.SetActive(true);
            wee.transform.localPosition = Vector3.up * 0.5f * (1 + wee.GetHeight());
            //wee.Resetup();
        }

        switch (type)
        {
            case PitObstacleType.Random:
                Open();
                break;
            case PitObstacleType.Slash:
                sprite.sprite = obstacleSprites[0 + cost];
                break;
            case PitObstacleType.Aetherize:
                sprite.sprite = obstacleSprites[3];
                break;
            case PitObstacleType.Smash:
                sprite.sprite = obstacleSprites[6 + cost];
                break;
            case PitObstacleType.Illuminate:
                sprite.sprite = obstacleSprites[9];
                break;
            case PitObstacleType.DoubleJump:
                sprite.sprite = obstacleSprites[4];
                break;
            case PitObstacleType.SuperJump:
                sprite.sprite = obstacleSprites[5];
                break;
            case PitObstacleType.DashHop:
                sprite.sprite = obstacleSprites[10];
                break;
            case PitObstacleType.Dig:
                sprite.sprite = obstacleSprites[11];
                break;
            case PitObstacleType.CoinLock:
            case PitObstacleType.HealthLock:
            case PitObstacleType.EnergyLock:
            case PitObstacleType.SoulLock:
            case PitObstacleType.AstralLock:
            case PitObstacleType.EnemyLock:
            case PitObstacleType.CrystalLock:
                sprite.sprite = obstacleSprites[12];
                sprite.transform.localScale = Vector3.one * 0.5f;
                break;
        }
        Color c = GetColor(type);
        sprite.color = c;
        tmp_text.color = c;

        if (type == PitObstacleType.DoubleJump)
        {
            subobject.transform.localPosition = Vector3.up * 1.5f;
        }

        if (type == PitObstacleType.SuperJump)
        {
            subobject.transform.localPosition = Vector3.up * 2.25f;
        }

        if (type == PitObstacleType.Dig)
        {
            subobject.transform.localPosition = Vector3.up * 0.375f;
        }

        if (collectible != null)
        {
            collectible.Setup(pu);
        }
    }

    //Called after determining the reward (and the reward's cost)
    public void ChooseRandomType()
    {
        PlayerData pd = MainManager.Instance.playerData;
        List<IRandomTableEntry<PitObstacleType>> randomtableEntries = new List<IRandomTableEntry<PitObstacleType>>
        {
            new RandomTableEntry<PitObstacleType>(PitObstacleType.Slash, 0.25f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.DoubleJump, 0.25f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.Smash, 0.25f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.DashHop, 0.25f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.CoinLock, pd.coins > 250 ? pd.coins / 250 : 1),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.HealthLock, pd.GetHealthPercentage() > 0.5f ? pd.GetHealthPercentage() : 0.5f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.EnergyLock, pd.GetEnergyPercentage() > 0.5f ? pd.GetEnergyPercentage() : 0.5f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.SoulLock, pd.GetSoulEnergyPercentage() > 0.5f ? pd.GetSoulEnergyPercentage() : 0.5f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.AstralLock, pd.astralTokens > 0 ? 0.5f : 0.1f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.EnemyLock, 0.5f),
            new RandomTableEntry<PitObstacleType>(PitObstacleType.CrystalLock, 0.25f)
        };

        RandomTable<PitObstacleType> obstacleTable = new RandomTable<PitObstacleType>(randomtableEntries);

        PitObstacleType result = PitObstacleType.Random;

        if (cost > 999)
        {
            cost = 999;
        }

        bool resultLegal = false;
        while (!resultLegal)
        {
            result = obstacleTable.Output();
            resultLegal = true;
            type = result;

            //Audit results

            //these costs would not be worth it for low cost items
            if (type == PitObstacleType.CrystalLock && cost <= 200)
            {
                resultLegal = false;
            }
            if (type == PitObstacleType.AstralLock && cost <= 200)
            {
                resultLegal = false;
            }

            //Don't lock the crystal key behind the crystal key only lock
            if (pu.type == PickupUnion.PickupType.KeyItem && pu.keyItem.type == KeyItem.KeyItemType.CrystalKey && type == PitObstacleType.CrystalLock)
            {
                resultLegal = false;
            } 

            if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) == null && (type == PitObstacleType.DoubleJump || type == PitObstacleType.Slash))
            {
                resultLegal = false;
            }
            if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna) == null && (type == PitObstacleType.DashHop || type == PitObstacleType.Smash))
            {
                resultLegal = false;
            }

            //my system breaks down
            /*
            if (type == PitObstacleType.EnemyLock && cost > 220)
            {
                resultLegal = false;
            }
            */

            //Impossible to pay
            if (type == PitObstacleType.HealthLock)
            {
                //Debug.Log("Converted HP: " + ShopItem.ConvertCost(ShopItem.Currency.HP, cost) + " vs " + pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).maxHP + " and " + pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).maxHP);
                resultLegal = false;
                if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna) != null)
                {
                    if (ShopItem.ConvertCost(ShopItem.Currency.HP, cost) < pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).maxHP)
                    {
                        resultLegal = true;
                    }
                }
                if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) != null)
                {
                    if (ShopItem.ConvertCost(ShopItem.Currency.HP, cost) < pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).maxHP)
                    {
                        resultLegal = true;
                    }
                }
            }
            if (type == PitObstacleType.EnergyLock)
            {
                //Debug.Log("Converted EP: " + ShopItem.ConvertCost(ShopItem.Currency.EP, cost) + " vs " + pd.maxEP);
                if (ShopItem.ConvertCost(ShopItem.Currency.EP, cost) >= pd.maxEP)
                {
                    resultLegal = false;
                }
            }
            if (type == PitObstacleType.SoulLock)
            {
                //Debug.Log("Converted SE: " + ShopItem.ConvertCost(ShopItem.Currency.SE, cost) + " vs " + pd.maxSE);
                if (ShopItem.ConvertCost(ShopItem.Currency.SE, cost) >= pd.maxSE)
                {
                    resultLegal = false;
                }
            }

            //Cost ceilings for abilities (Don't let you get strong stuff for free)
            if (type == PitObstacleType.Slash && cost >= 225)
            {
                resultLegal = false;
            }
            if (type == PitObstacleType.Smash && cost >= 225)
            {
                resultLegal = false;
            }
            if (type == PitObstacleType.DashHop && cost >= 225)
            {
                resultLegal = false;
            }
            if (type == PitObstacleType.DoubleJump && cost >= 225)
            {
                resultLegal = false;
            }

            int effectiveFloor = Mathf.Max(cost / 2, (int)(floor * 0.75f));
            if (effectiveFloor > floor + 30)    //really high level floor enemies are hard
            {
                effectiveFloor = (effectiveFloor - floor - 30) / 2 + floor + 30;
            }
            if (type == PitObstacleType.EnemyLock && effectiveFloor > floor * 2 && floor < 10)
            {
                resultLegal = false;
            }

            //note: Coin locks are always legal so this will always terminate eventually (however, this system becomes biased towards coin costs at high values)
        }
        type = result;

        //Convert costs
        if (type == PitObstacleType.HealthLock)
        {
            cost = ShopItem.ConvertCost(ShopItem.Currency.HP, cost);
        }
        if (type == PitObstacleType.EnergyLock)
        {
            cost = ShopItem.ConvertCost(ShopItem.Currency.EP, cost);
        }
        if (type == PitObstacleType.SoulLock)
        {
            cost = ShopItem.ConvertCost(ShopItem.Currency.SE, cost);
        }
        if (type == PitObstacleType.AstralLock)
        {
            cost = ShopItem.ConvertCost(ShopItem.Currency.AstralToken, cost);
        }
        if (type == PitObstacleType.Slash)
        {
            //set level
            if (cost < RandomGenerator.GetIntRange(PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySlash), PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySlash2)))
            {
                cost = 0;
            } else if (cost < RandomGenerator.GetIntRange(PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySlash2), PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySlash3)))
            {
                cost = 1;
            } else
            {
                cost = 2;
                if (RandomGenerator.Get() < 0.5f)
                {
                    type = PitObstacleType.Aetherize;
                    cost = 0;
                }
            }
        }
        if (type == PitObstacleType.Smash)
        {
            //set level
            if (cost < RandomGenerator.GetIntRange(PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySmash), PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySmash2)))
            {
                cost = 0;
            }
            else if (cost < RandomGenerator.GetIntRange(PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySmash2), PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySmash3)))
            {
                cost = 1;
            }
            else
            {
                cost = 2;
                if (RandomGenerator.Get() < 0.5f)
                {
                    type = PitObstacleType.Illuminate;
                    cost = 0;
                }
            }
        }
        if (type == PitObstacleType.DoubleJump)
        {
            //set level
            if (cost < RandomGenerator.GetIntRange(PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilityDoubleJump), PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilitySuperJump)))
            {
                cost = 0;
            }
            else
            {
                type = PitObstacleType.SuperJump;
                cost = 0;
            }
        }
        if (type == PitObstacleType.DashHop)
        {
            //set level
            if (cost < RandomGenerator.GetIntRange(PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilityDashHop), PickupUnion.GetMiscCost(MainManager.MiscSprite.AbilityDig)))
            {
                cost = 0;
            }
            else
            {
                type = PitObstacleType.Dig;
                cost = 0;
            }
        }
        if (type == PitObstacleType.CrystalLock)
        {
            cost = 1; //hardcode
        }

        //Special cases (cap costs to some amount, also fix ability requirements)
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Health6 && type == PitObstacleType.HealthLock)
        {
            if (cost > 5)
            {
                cost = 5;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Health12 && type == PitObstacleType.HealthLock)
        {
            if (cost > 10)
            {
                cost = 10;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Health30 && type == PitObstacleType.HealthLock)
        {
            if (cost > 25)
            {
                cost = 25;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Health60 && type == PitObstacleType.HealthLock)
        {
            if (cost > 50)
            {
                cost = 50;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Energy6 && type == PitObstacleType.EnergyLock)
        {
            if (cost > 5)
            {
                cost = 5;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Energy12 && type == PitObstacleType.EnergyLock)
        {
            if (cost > 10)
            {
                cost = 10;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Energy30 && type == PitObstacleType.EnergyLock)
        {
            if (cost > 25)
            {
                cost = 25;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Energy60 && type == PitObstacleType.EnergyLock)
        {
            if (cost > 50)
            {
                cost = 50;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Soul6 && type == PitObstacleType.SoulLock)
        {
            if (cost > 5)
            {
                cost = 5;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Soul12 && type == PitObstacleType.SoulLock)
        {
            if (cost > 10)
            {
                cost = 10;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Soul30 && type == PitObstacleType.SoulLock)
        {
            if (cost > 25)
            {
                cost = 25;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && pu.misc == MainManager.MiscSprite.Soul60 && type == PitObstacleType.SoulLock)
        {
            if (cost > 50)
            {
                cost = 50;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && type == PitObstacleType.Slash)
        {
            if (pu.misc == MainManager.MiscSprite.AbilitySlash)
            {
                //make it not dumb (locking a thing behind itself)
                type = PitObstacleType.Smash;
                cost = 0;
            }
            if (pu.misc == MainManager.MiscSprite.AbilitySlash2 && cost >= 1)
            {
                cost = 0;
            }
            if (pu.misc == MainManager.MiscSprite.AbilitySlash3 && cost >= 2)
            {
                cost = 1;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && type == PitObstacleType.Smash)
        {
            if (pu.misc == MainManager.MiscSprite.AbilitySmash)
            {
                //make it not dumb (locking a thing behind itself)
                type = PitObstacleType.Slash;
                cost = 0;
            }
            if (pu.misc == MainManager.MiscSprite.AbilitySmash2 && cost >= 1)
            {
                cost = 0;
            }
            if (pu.misc == MainManager.MiscSprite.AbilitySmash3 && cost >= 2)
            {
                cost = 1;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && (type == PitObstacleType.DoubleJump || type == PitObstacleType.SuperJump))
        {
            if (pu.misc == MainManager.MiscSprite.AbilityDoubleJump)
            {
                //make it not dumb (locking a thing behind itself)
                type = PitObstacleType.Slash;
                cost = 1;
            }
            if (pu.misc == MainManager.MiscSprite.AbilitySuperJump)
            {
                type = PitObstacleType.DoubleJump;
                cost = 0;
            }
        }
        if (pu.type == PickupUnion.PickupType.Misc && (type == PitObstacleType.DashHop || type == PitObstacleType.Dig))
        {
            if (pu.misc == MainManager.MiscSprite.AbilityDashHop)
            {
                //make it not dumb (locking a thing behind itself)
                type = PitObstacleType.Smash;
                cost = 0;
            }
            if (pu.misc == MainManager.MiscSprite.AbilityDig)
            {
                type = PitObstacleType.DashHop;
                cost = 0;
            }
        }
    }

    public static (PickupUnion, int) ChooseRandomRewardStatic(int floor)
    {
        PickupUnion pu = new PickupUnion();

        PlayerData pd = MainManager.Instance.playerData;
        List<IRandomTableEntry<PickupUnion.PickupType>> randomtableEntries = new List<IRandomTableEntry<PickupUnion.PickupType>>
        {
            new RandomTableEntry<PickupUnion.PickupType>(PickupUnion.PickupType.Item, pd.GetMaxInventorySize() - pd.itemInventory.Count > 5 ? 30 : 15),
            new RandomTableEntry<PickupUnion.PickupType>(PickupUnion.PickupType.Badge, 60), //badges are more useful so spawn them more
            new RandomTableEntry<PickupUnion.PickupType>(PickupUnion.PickupType.Ribbon, 10),    //less helpful
            new RandomTableEntry<PickupUnion.PickupType>(PickupUnion.PickupType.Misc, 10),  //usually less helpful (but there are also hardcoded ability spawns)
        };

        float keyWeight = 10;

        bool hasTotemPower = false;
        bool hasTotemFortune = false;
        bool hasCandle = false;
        for (int i = 0; i < pd.keyInventory.Count; i++)
        {
            if (pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemA || pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemB || pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemC)
            {
                hasTotemFortune = true;
                continue;
            }
            if (pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemA || pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemB || pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemC)
            {
                hasTotemPower = true;
                continue;
            }
            if (pd.keyInventory[i].type >= KeyItem.KeyItemType.PlainCandle && pd.keyInventory[i].type <= KeyItem.KeyItemType.RainbowCandle)
            {
                hasCandle = true;
                continue;
            }
        }
        if (hasTotemPower)
        {
            keyWeight -= 2.5f;
        }
        if (hasTotemFortune)
        {
            keyWeight -= 2.5f;
        }
        if (hasCandle)
        {
            keyWeight -= 2.5f;
        }

        randomtableEntries.Add(new RandomTableEntry<PickupUnion.PickupType>(PickupUnion.PickupType.KeyItem, keyWeight));

        RandomTable<PickupUnion.PickupType> pickupTypeTable = new RandomTable<PickupUnion.PickupType>(randomtableEntries);
        PickupUnion.PickupType resultType = pickupTypeTable.Output();

        //Too many switch statements
        pu.type = resultType;
        switch (resultType)
        {
            case PickupUnion.PickupType.Item:
                int itemChapter = (floor / 10) + RandomGenerator.GetIntRange(-2, 3);
                if (itemChapter < 0)
                {
                    itemChapter = 0;
                }
                if (itemChapter > 9)
                {
                    itemChapter = 9;
                }

                if (RandomGenerator.Get() * itemChapter > 7)
                {
                    itemChapter = RandomGenerator.GetIntRange(0, 10);
                }

                //Debug.Log(itemChapter);
                List<Item.ItemType> itemPool = new List<Item.ItemType>();
                for (int i = 1; i < (int)Item.ItemType.EndOfTable; i++)
                {
                    if (Item.GetItemDataEntry((Item.ItemType)i).chapter == itemChapter)
                    {
                        itemPool.Add((Item.ItemType)i);
                    }
                }
                pu.item.type = RandomTable<Item.ItemType>.ChooseRandom(itemPool);
                if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_RandomItemModifiers))
                {
                    pu.item.modifier = GlobalItemScript.GetRandomModifier(pu.item.type);
                }
                break;
            case PickupUnion.PickupType.KeyItem:
                List<IRandomTableEntry<KeyItem.KeyItemType>> kitentries = new List<IRandomTableEntry<KeyItem.KeyItemType>>
                {
                    new RandomTableEntry<KeyItem.KeyItemType>(KeyItem.KeyItemType.CrystalKey, 1),
                    new RandomTableEntry<KeyItem.KeyItemType>(KeyItem.KeyItemType.PowerTotemA, hasTotemPower ? 0 : 1),
                    new RandomTableEntry<KeyItem.KeyItemType>(KeyItem.KeyItemType.FortuneTotemA, hasTotemFortune ? 0 : 1),
                    new RandomTableEntry<KeyItem.KeyItemType>(KeyItem.KeyItemType.PlainCandle, hasCandle ? 0 : 1),
                };
                RandomTable<KeyItem.KeyItemType> kittable = new RandomTable<KeyItem.KeyItemType>(kitentries);
                KeyItem.KeyItemType kitResult = kittable.Output();
                switch (kitResult)
                {
                    case KeyItem.KeyItemType.PlainCandle:
                        int choice = (floor / 10) + RandomGenerator.GetIntRange(0, 4);
                        //if (choice == 14)
                        //{
                        //    choice = 13;
                        //}
                        kitResult = KeyItem.KeyItemType.PlainCandle + choice;
                        break;
                    case KeyItem.KeyItemType.PowerTotemA:
                        switch (RandomGenerator.GetIntRange(0, 3))
                        {
                            case 1:
                                kitResult = KeyItem.KeyItemType.PowerTotemB;
                                break;
                            case 2:
                                kitResult = KeyItem.KeyItemType.PowerTotemC;
                                break;
                        }
                        break;
                    case KeyItem.KeyItemType.FortuneTotemA:
                        switch (RandomGenerator.GetIntRange(0, 3))
                        {
                            case 1:
                                kitResult = KeyItem.KeyItemType.FortuneTotemB;
                                break;
                            case 2:
                                kitResult = KeyItem.KeyItemType.FortuneTotemC;
                                break;
                        }
                        break;
                }
                pu.keyItem.type = kitResult;
                pu.keyItem.bonusData = 0;
                break;
            case PickupUnion.PickupType.Badge:
                int badgeChapter = (floor / 10) + RandomGenerator.GetIntRange(-2, 3);
                if (badgeChapter < 0)
                {
                    badgeChapter = 0;
                }
                if (badgeChapter > 9)
                {
                    badgeChapter = 9;
                }
                List<Badge.BadgeType> badgePool = new List<Badge.BadgeType>();
                for (int i = 1; i < (int)Badge.BadgeType.EndOfTable; i++)
                {
                    //note: the find thing looks weird but that is because the null value for badges doesn't exist because badges are structs
                    if (Badge.GetBadgeDataEntry((Badge.BadgeType)i).chapter == badgeChapter && pd.badgeInventory.Find((e) => (e.type == (Badge.BadgeType)i)).type != (Badge.BadgeType)i)
                    {
                        badgePool.Add((Badge.BadgeType)i);
                    }
                }
                if (badgePool.Count == 0)
                {
                    //failsafe 1: broaden the range
                    for (int i = 1; i < (int)Badge.BadgeType.EndOfTable; i++)
                    {
                        //note: the find thing looks weird but that is because the null value for badges doesn't exist because badges are structs
                        if ((Badge.GetBadgeDataEntry((Badge.BadgeType)i).chapter >= badgeChapter - 1 && Badge.GetBadgeDataEntry((Badge.BadgeType)i).chapter <= badgeChapter + 1) && pd.badgeInventory.Find((e) => (e.type == (Badge.BadgeType)i)).type != (Badge.BadgeType)i)
                        {
                            badgePool.Add((Badge.BadgeType)i);
                        }
                    }


                    if (badgePool.Count == 0)
                    {
                        //failsafe 2: 50% chance of a random, 50% chance of mystery
                        if (RandomGenerator.Get() < 0.5f)
                        {
                            for (int i = 1; i < (int)Badge.BadgeType.EndOfTable; i++)
                            {
                                badgePool.Add((Badge.BadgeType)i);
                            }
                            pu.badge.type = RandomTable<Badge.BadgeType>.ChooseRandom(badgePool);
                        } else
                        {
                            pu.type = PickupUnion.PickupType.Misc;
                            pu.misc = MainManager.MiscSprite.MysteryBadge;
                        }
                    }
                    else
                    {
                        pu.badge.type = RandomTable<Badge.BadgeType>.ChooseRandom(badgePool);
                    }
                }
                else
                {
                    pu.badge.type = RandomTable<Badge.BadgeType>.ChooseRandom(badgePool);
                }
                break;
            case PickupUnion.PickupType.Ribbon:
                //Each ribbon is equally likely, but ribbons you already have are half chance
                List<Ribbon.RibbonType> ribbonPool = new List<Ribbon.RibbonType>();
                int[] ribbonCountList = new int[(int)Ribbon.RibbonType.EndOfTable];
                for (int i = 0; i < pd.ribbonInventory.Count; i++)
                {
                    ribbonCountList[(int)(pd.ribbonInventory[i].type)]++;
                }
                for (int i = 1; i < (int)Ribbon.RibbonType.EndOfTable; i++)
                {
                    for (int j = 0; j < (2 - ribbonCountList[j]); j++)
                    {
                        ribbonPool.Add((Ribbon.RibbonType)i);
                    }
                }
                if (ribbonPool.Count == 0)
                {
                    //failsafe 2: 50% chance of a random, 50% chance of mystery
                    if (RandomGenerator.Get() < 0.5f)
                    {
                        for (int i = 1; i < (int)Ribbon.RibbonType.EndOfTable; i++)
                        {
                            ribbonPool.Add((Ribbon.RibbonType)i);
                        }
                        pu.ribbon.type = RandomTable<Ribbon.RibbonType>.ChooseRandom(ribbonPool);
                    }
                    else
                    {
                        pu.type = PickupUnion.PickupType.Misc;
                        pu.misc = MainManager.MiscSprite.MysteryRibbon;
                    }
                }
                else
                {
                    pu.ribbon.type = RandomTable<Ribbon.RibbonType>.ChooseRandom(ribbonPool);
                }
                break;
            case PickupUnion.PickupType.Misc:
                List<IRandomTableEntry<MainManager.MiscSprite>> miscTableEntries = new List<IRandomTableEntry<MainManager.MiscSprite>>
                {
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.Health6, pd.GetHealthPercentage() > 0.66f ? 0.3f : 1),
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.Energy6, pd.GetEnergyPercentage() > 0.66f ? 0.3f : 1),
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.Soul6, pd.GetSoulEnergyPercentage() > 0.66f ? 0.3f : 1),
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.XP10, pd.level >= PlayerData.GetMaxLevel() ? 0 : 1),
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.ItemBag2, pd.GetMaxInventorySize() - pd.itemInventory.Count > 2 ? 1 : 0.3f),
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.RecipeBag2, pd.GetMaxInventorySize() - pd.itemInventory.Count > 2 ? 0.5f : 0.15f),
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.ItemBag4, pd.GetMaxInventorySize() - pd.itemInventory.Count > 4 ? 0.5f : 0.1f),
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.RecipeBag4, pd.GetMaxInventorySize() - pd.itemInventory.Count > 4 ? 0.25f : 0.05f),
                    new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySlash, 0.25f)    //stand in for all the ability rewards   (Making this very rare because I have hardcoded spawns as well)
                };
                RandomTable<MainManager.MiscSprite> mstable = new RandomTable<MainManager.MiscSprite>(miscTableEntries);
                MainManager.MiscSprite msresult = mstable.Output();

                /*
                bool resultLegal = false;

                if (msresult == MainManager.MiscSprite.AbilitySlash)
                {
                    foreach (PlayerData.PlayerDataEntry pde in pd.party)
                    {
                        if (pde.weaponLevel < 2)
                        {
                            resultLegal = true;
                        }
                        if (pde.jumpLevel < 2)
                        {
                            resultLegal = true;
                        }
                    }
                }

                while (!resultLegal)
                {
                    msresult = mstable.Output();
                    foreach (PlayerData.PlayerDataEntry pde in pd.party)
                    {
                        if (pde.weaponLevel < 2)
                        {
                            resultLegal = true;
                        }
                        if (pde.jumpLevel < 2)
                        {
                            resultLegal = true;
                        }
                    }
                }
                */

                float power = floor * (0.6f);
                switch (msresult)
                {
                    case MainManager.MiscSprite.Health6:
                        if (power > 60)
                        {
                            msresult = MainManager.MiscSprite.Health60;
                        }
                        else if (power > 30)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(30, 60, power) == 60)
                            {
                                msresult = MainManager.MiscSprite.Health60;
                            }
                            else
                            {
                                msresult = MainManager.MiscSprite.Health30;
                            }

                        }
                        else if (power > 12)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(12, 30, power) == 30)
                            {
                                msresult = MainManager.MiscSprite.Health30;
                            }
                            else
                            {
                                msresult = MainManager.MiscSprite.Health12;
                            }
                        }
                        else if (power > 6)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(6, 12, power) == 12)
                            {
                                msresult = MainManager.MiscSprite.Health12;
                            }
                        }
                        break;
                    case MainManager.MiscSprite.Energy6:
                        if (power > 60)
                        {
                            msresult = MainManager.MiscSprite.Energy60;
                        }
                        else if (power > 30)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(30, 60, power) == 60)
                            {
                                msresult = MainManager.MiscSprite.Energy60;
                            }
                            else
                            {
                                msresult = MainManager.MiscSprite.Energy30;
                            }

                        }
                        else if (power > 12)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(12, 30, power) == 30)
                            {
                                msresult = MainManager.MiscSprite.Energy30;
                            }
                            else
                            {
                                msresult = MainManager.MiscSprite.Energy12;
                            }
                        }
                        else if (power > 6)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(6, 12, power) == 12)
                            {
                                msresult = MainManager.MiscSprite.Energy12;
                            }
                        }
                        break;
                    case MainManager.MiscSprite.Soul6:
                        if (power > 60)
                        {
                            msresult = MainManager.MiscSprite.Soul60;
                        }
                        else if (power > 30)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(30, 60, power) == 60)
                            {
                                msresult = MainManager.MiscSprite.Soul60;
                            }
                            else
                            {
                                msresult = MainManager.MiscSprite.Soul30;
                            }

                        }
                        else if (power > 12)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(12, 30, power) == 30)
                            {
                                msresult = MainManager.MiscSprite.Soul30;
                            }
                            else
                            {
                                msresult = MainManager.MiscSprite.Soul12;
                            }
                        }
                        else if (power > 6)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(6, 12, power) == 12)
                            {
                                msresult = MainManager.MiscSprite.Soul12;
                            }
                        }
                        break;
                    case MainManager.MiscSprite.XP10:
                        if (power > 99)
                        {
                            msresult = MainManager.MiscSprite.XP99;
                        }
                        else if (power > 50)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(50, 99, power) == 99)
                            {
                                msresult = MainManager.MiscSprite.XP99;
                            }
                            else
                            {
                                msresult = MainManager.MiscSprite.XP50;
                            }
                        }
                        else if (power > 25)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(25, 50, power) == 50)
                            {
                                msresult = MainManager.MiscSprite.XP50;
                            }
                            else
                            {
                                msresult = MainManager.MiscSprite.XP25;
                            }
                        }
                        else if (power > 10)
                        {
                            if (RandomGenerator.PsuedoIntGenerate(10, 25, power) == 25)
                            {
                                msresult = MainManager.MiscSprite.XP25;
                            }
                        }
                        //Debug.Log("XP branch");
                        break;
                    case MainManager.MiscSprite.AbilitySlash:
                        List<IRandomTableEntry<MainManager.MiscSprite>> abilityEntries = new List<IRandomTableEntry<MainManager.MiscSprite>>();
                        if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) != null)
                        {
                            int wl = pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).weaponLevel;
                            if (wl < 2)
                            {
                                switch (wl)
                                {
                                    default:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySlash, 1));
                                        break;
                                    case 0:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySlash2, 1));
                                        break;
                                    case 1:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySlash3, 1));
                                        break;
                                    case 2:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.MysteryItem, 1));
                                        break;
                                }
                            }
                            int jl = pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).jumpLevel;
                            if (jl < 2)
                            {
                                switch (jl)
                                {
                                    default:
                                        //Error condition (shouldn't be possible)
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.MysteryBadge, 1));
                                        break;
                                    case 0:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilityDoubleJump, 1));
                                        break;
                                    case 1:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySuperJump, 1));
                                        break;
                                    case 2:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.MysteryItem, 1));
                                        break;
                                }
                            }
                        }
                        if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna) != null)
                        {
                            int wl = pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).weaponLevel;
                            if (wl < 2)
                            {
                                switch (wl)
                                {
                                    default:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySmash, 1));
                                        break;
                                    case 0:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySmash2, 1));
                                        break;
                                    case 1:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySmash3, 1));
                                        break;
                                    case 2:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.MysteryItem, 1));
                                        break;
                                }
                            }
                            int jl = pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).jumpLevel;
                            if (jl < 2)
                            {
                                switch (jl)
                                {
                                    default:
                                        //Error condition (shouldn't be possible)
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.MysteryRibbon, 1));
                                        break;
                                    case 0:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilityDashHop, 1));
                                        break;
                                    case 1:
                                        abilityEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilityDig, 1));
                                        break;
                                }
                            }
                        }
                        //Debug.Log("Ability table branch: " + abilityEntries.Count);
                        RandomTable<MainManager.MiscSprite> abilityTable = new RandomTable<MainManager.MiscSprite>(abilityEntries);
                        msresult = abilityTable.Output();
                        if (abilityEntries.Count == 0)
                        {
                            //failsafe
                            msresult = MainManager.MiscSprite.MysteryRecipe;
                        }
                        break;
                }
                pu.misc = msresult;
                //Debug.Log(msresult + " as misc result");
                break;
        }

        pu.Mutate();

        int cost = Mathf.CeilToInt((RandomGenerator.Get() * 0.66f + 0.66f) * PickupUnion.GetBaseCost(pu));
        //Debug.Log(pu + " " + cost);

        return (pu, cost);
    }
    public void ChooseRandomReward(int floor)
    {
        (pu, cost) = ChooseRandomRewardStatic(floor);
    }

    public Color GetColor(PitObstacleType pot)
    {
        switch (pot)
        {
            case PitObstacleType.Random:
                return new Color(1, 1, 1, 1);
            case PitObstacleType.Slash:
                return new Color(1, 0, 0, 1);
            case PitObstacleType.Aetherize:
                return new Color(1, 0, 0, 1);
            case PitObstacleType.Smash:
                return new Color(0, 1, 0, 1);
            case PitObstacleType.Illuminate:
                return new Color(1, 0.7f, 0, 1);
            case PitObstacleType.DoubleJump:
                return new Color(1, 1, 0, 1);
            case PitObstacleType.SuperJump:
                return new Color(1, 0.5f, 0, 1);
            case PitObstacleType.DashHop:
                return new Color(0, 1, 0, 1);
            case PitObstacleType.Dig:
                return new Color(0.25f, 0.25f, 0.25f, 1);
            case PitObstacleType.CoinLock:
                return new Color(1, 0.7f, 0, 1);
            case PitObstacleType.HealthLock:
                return new Color(1, 0, 0, 1);
            case PitObstacleType.EnergyLock:
                return new Color(1, 1, 0, 1);
            case PitObstacleType.SoulLock:
                return new Color(1, 0, 1, 1);
            case PitObstacleType.AstralLock:
                return new Color(0.5f, 0, 1, 1);
            case PitObstacleType.EnemyLock:
                return new Color(0.5f, 0.5f, 0.5f, 1);
            case PitObstacleType.CrystalLock:
                return new Color(0, 1, 1, 1);
        }
        return new Color(1, 1, 1, 1);
    }

    public string GetTypeCostString()
    {
        return GetTypeCostStringStatic(type, cost != 1);
    }
    public static string GetTypeCostStringStatic(PitObstacleType pot, bool plural)
    {
        switch (pot)
        {
            case PitObstacleType.Random:
            case PitObstacleType.Slash:
            case PitObstacleType.Aetherize:
            case PitObstacleType.Smash:
            case PitObstacleType.Illuminate:
            case PitObstacleType.DoubleJump:
            case PitObstacleType.SuperJump:
            case PitObstacleType.DashHop:
            case PitObstacleType.Dig:
                return "?";
            case PitObstacleType.CoinLock:
                if (plural)
                {
                    return "coins";
                } else
                {
                    return "coin";
                }
            case PitObstacleType.HealthLock:
                return "HP";
            case PitObstacleType.EnergyLock:
                return "EP";
            case PitObstacleType.SoulLock:
                return "SE";
            case PitObstacleType.AstralLock:
                if (plural)
                {
                    return "Astral Tokens";
                }
                else
                {
                    return "Astral Token";
                }
            case PitObstacleType.EnemyLock:
                return "?";
            case PitObstacleType.CrystalLock:
                return "Crystal Key";
        }
        return "?";
    }

    public override void WorldUpdate()
    {
        if (open || !setup)
        {
            return;
        }        

        if (type == PitObstacleType.EnemyLock && wee == null)
        {
            Open();
        }

        //illuminate / aetherize are opened by proximity
        if (type == PitObstacleType.Illuminate && wp.GetActionState() == WorldPlayer.ActionState.Illuminate && Vector3.Distance(transform.position, wp.transform.position) < 1.5f)
        {
            Open();
        }
        if (type == PitObstacleType.Aetherize && wp.GetActionState() == WorldPlayer.ActionState.Aetherize && Vector3.Distance(transform.position, wp.transform.position) < 1.5f)
        {
            Open();
        }

        //dig can be opened by undig or by going below it
        if (type == PitObstacleType.Dig && wp.GetActionState() == WorldPlayer.ActionState.Dig)
        {
            if (Mathf.Abs(wp.transform.position.x - transform.position.x) < 0.5f && Mathf.Abs(wp.transform.position.z - transform.position.z) < 0.5f && (wp.transform.position.y - transform.position.y) < 0)
            {
                Open();
            }
        }
    }


    public void Bonk(Vector3 kickpos, Vector3 kicknormal)
    {
        if (type == PitObstacleType.DashHop)
        {
            Open();
        }
    }

    public void HeadHit(WorldPlayer.StompType stompType)
    {
        if (type == PitObstacleType.DashHop && stompType == WorldPlayer.StompType.DashHop)
        {
            Open();
        }
        if (type == PitObstacleType.DoubleJump && stompType == WorldPlayer.StompType.DoubleJump)
        {
            Open();
        }
        if (type == PitObstacleType.SuperJump && stompType == WorldPlayer.StompType.SuperJump)
        {
            Open();
        }
    }

    public bool Slash(Vector3 slashvector, Vector3 playerpos)
    {
        PlayerData pd = MainManager.Instance.playerData;
        if (type == PitObstacleType.Slash && pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).weaponLevel >= cost)
        {
            Open();
        }
        return true;
    }

    public bool Smash(Vector3 smashvector, Vector3 playerpos)
    {
        PlayerData pd = MainManager.Instance.playerData;
        if (type == PitObstacleType.Smash && pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).weaponLevel >= cost)
        {
            Open();
        }
        return true;
    }

    public void Stomp(WorldPlayer.StompType stompType)
    {
        if (type == PitObstacleType.DashHop && stompType == WorldPlayer.StompType.DashHop)
        {
            Open();
        }
        if (type == PitObstacleType.DoubleJump && stompType == WorldPlayer.StompType.DoubleJump)
        {
            Open();
        }
        if (type == PitObstacleType.SuperJump && stompType == WorldPlayer.StompType.SuperJump)
        {
            Open();
        }
    }

    public void Undig()
    {
        if (type == PitObstacleType.Dig)
        {
            Open();
        }
    }

    public void Open()
    {
        open = true;
        Destroy(pobs.gameObject);
        Destroy(sprite.gameObject);
        if (text != null)
        {
            Destroy(text.gameObject);
        }
        if (it != null)
        {
            Destroy(it.gameObject);
        }
        if (wee != null)
        {
            Destroy(wee.gameObject);
        }
        if (collectible != null)
        {
            collectible.intangible = false;
            collectible.antigravity = false;
        }
    }

    public void Interact()
    {
        StartCoroutine(InteractCutscene());
    }

    public IEnumerator InteractCutscene()
    {
        int crystalKeyCount = 0;
        PlayerData pd = MainManager.Instance.playerData;

        crystalKeyCount = pd.CountKeyItemStacking(KeyItem.KeyItemType.CrystalKey);

        string[][] testTextFile = new string[18][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];
        testTextFile[7] = new string[1];
        testTextFile[8] = new string[1];
        testTextFile[9] = new string[1];
        testTextFile[10] = new string[1];
        testTextFile[11] = new string[1];
        testTextFile[12] = new string[1];
        testTextFile[13] = new string[1];
        testTextFile[14] = new string[1];
        testTextFile[15] = new string[1];
        testTextFile[16] = new string[1];
        testTextFile[17] = new string[1];

        int locktext = 0;
        int locktextcrystal = 1;
        int locktextenemy = 2;
        int locktextenemycrystal = 3;
        int cantpay = 4;

        testTextFile[0][0] = "<system>This lock costs <var,1> <var,2> to unlock. Do you want to unlock it?<prompt,Unlock,1,Cancel,-1,1>";
        testTextFile[1][0] = "<system>This lock costs <var,1> <var,2> to unlock. Do you want to unlock it? (Crystal Keys can unlock any lock. You have <var,3>.)<prompt,Unlock,1,Use Crystal Key,2,Cancel,-1,2>";
        testTextFile[2][0] = "<system>This lock requires fighting an enemy to unlock. Do you want to unlock it? (Floor <color,blue><var,0></color> encounter)<prompt,Unlock,1,Cancel,-1,1>";
        testTextFile[3][0] = "<system>This lock requires fighting an enemy to unlock. Do you want to unlock it? (Floor <color,blue><var,0></color> encounter) (Crystal Keys can unlock any lock. You have <var,3>.)<prompt,Unlock,1,Use Crystal Key,2,Cancel,-1,2>";
        testTextFile[4][0] = "<system>You don't have enough <var,1> to open this lock.";


        int enemyFloor = Mathf.Max(cost / 2, floor - 10);
        if (enemyFloor > floor + 30)    //really high level floor enemies are hard
        {
            enemyFloor = (enemyFloor - floor - 30) / 2 + floor + 15;
        }

        string[] vars = new string[4] { enemyFloor + "", cost + "", GetTypeCostString(), crystalKeyCount + ""};

        string[] varsB = new string[2] { enemyFloor + "", GetTypeCostStringStatic(type, true) };

        if (type == PitObstacleType.EnemyLock)
        {
            if (crystalKeyCount > 0)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, locktextenemycrystal, this, vars));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, locktextenemy, this, vars));
            }
        }
        else
        {
            if (crystalKeyCount > 0)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, locktextcrystal, this, vars));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, locktext, this, vars));
            }
        }

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out int state);

        if (type == PitObstacleType.EnemyLock && state == 1)
        {
            MainManager.Instance.mapScript.StartBattle(wee);
            yield break;
        } else
        {
            switch (state)
            {
                case -1:
                default:
                    //cancel
                    yield break;
                case 1:
                    //Pay?
                    bool canPay = false;
                    switch (type)
                    {
                        case PitObstacleType.CoinLock:
                            if (pd.coins >= cost)
                            {
                                pd.coins -= cost;
                                canPay = true;
                            }
                            break;
                        case PitObstacleType.HealthLock:
                            foreach (PlayerData.PlayerDataEntry pde in pd.party)
                            {
                                if (pde.hp > cost)
                                {
                                    canPay = true;
                                }
                            }
                            if (canPay)
                            {
                                foreach (PlayerData.PlayerDataEntry pde in pd.party)
                                {
                                    pde.hp -= cost;
                                    if (pde.hp < 1)
                                    {
                                        pde.hp = 1;
                                    }
                                }
                            }
                            break;
                        case PitObstacleType.EnergyLock:
                            if (pd.ep >= cost)
                            {
                                pd.ep -= cost;
                                canPay = true;
                            }
                            break;
                        case PitObstacleType.SoulLock:
                            if (pd.se >= cost)
                            {
                                pd.se -= cost;
                                canPay = true;
                            }
                            break;
                        case PitObstacleType.AstralLock:
                            if (pd.astralTokens >= cost)
                            {
                                pd.astralTokens -= cost;
                                canPay = true;
                            }
                            break;
                        case PitObstacleType.CrystalLock:
                            canPay = pd.RemoveKeyItem(KeyItem.KeyItemType.CrystalKey);
                            break;
                    }
                    if (!canPay)
                    {
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, cantpay, this, varsB));
                        yield break;
                    }
                    else
                    {
                        Open();
                        yield break;
                    }
                case 2:
                    //Pay with crystal keys
                    pd.RemoveKeyItemStacking(KeyItem.KeyItemType.CrystalKey, 1);
                    Open();
                    yield break;
            }
        }
    }



    public string RequestTextData(string request)
    {
        return "";
    }

    public void SendTextData(string data)
    {

    }

    public void EnableSpeakingAnim()
    {
    }

    public bool SpeakingAnimActive()
    {
        return false;
    }

    public void DisableSpeakingAnim()
    {
    }

    public void SetAnimation(string animationID, bool force = false, float time = -1)
    {
    }

    public void SendAnimationData(string data)
    {
    }

    public Vector3 GetTextTailPosition()
    {
        return transform.position;
    }

    public void TextBleep()
    {
    }

    public void SetFacing(Vector3 facingTarget)
    {
    }

    public void EmoteEffect(TagEntry.Emote emote)
    {
    }
}
