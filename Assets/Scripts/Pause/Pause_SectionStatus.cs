using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using static BattleHelper;

//This menu is full of elements
//Some get turned off when they are not applicable
public class Pause_SectionStatus : Pause_SectionShared
{
    public GameObject subobject;

    public PlayerData playerData;

    public TextDisplayer currentLocationText;
    public TextDisplayer lastSaveText;

    public TextDisplayer levelText;
    public TextDisplayer xpText;
    public GameObject astralTokenObject;
    public TextDisplayer astralTokenText;

    public TextDisplayer coinDescriptor;
    public TextDisplayer coinText;
    public TextDisplayer shardDescriptor;
    public TextDisplayer shardText;
    public TextDisplayer itemDescriptor;
    public TextDisplayer itemText;
    public TextDisplayer playtimeDescriptor;
    public TextDisplayer playtimeText;

    public GameObject charmEffectObject;
    public TextDisplayer charmEffectText;
    public GameObject restEffectObject;
    public TextDisplayer restEffectText;

    public Sprite[] weaponMedalSprites;

    public GameObject characterObjectA;
    public GameObject abilityMedalA1;
    public GameObject abilityMedalA2;
    public GameObject abilityMedalA3;
    public GameObject abilityMedalA4;

    public GameObject characterObjectB;
    public GameObject abilityMedalB1;
    public GameObject abilityMedalB2;
    public GameObject abilityMedalB3;
    public GameObject abilityMedalB4;

    public GameObject healthObjectA;
    public TextDisplayer healthTextA;
    public GameObject healthObjectB;
    public TextDisplayer healthTextB;
    public TextDisplayer energyText;
    public TextDisplayer soulText;

    public List<Pause_SectionStatus_MenuNode> menuNodes;
    public List<Pause_SectionStatus_MenuNode> realMenuNodes;    //filtered by being active

    public GameObject selector;
    public Pause_SectionStatus_MenuNode selectedNode;

    public enum MenuNodeType
    {
        None,
        Level,
        AstralTokens,
        XP,
        HPA,
        AgilityA,
        Slash,
        Aetherize,
        DoubleJump,
        SuperJump,
        Smash,
        Illuminate,
        DashHop,
        Dig,
        HPB,
        AgilityB,
        EP,
        SE,
        SP,
        CurrentLocation,
        LastSave,
        Coins,
        PrismaShards,
        Items,
        Playtime,
        CharmEffect,
        RestEffect,
        LightCore,
        WaterCore,
        AirCore,
        DarkCore,
        FireCore,
        EarthCore,
        Chapter7Thing,
        Chapter8Thing,
        MotherCore
    }

    public string GetNodeDescription(MenuNodeType mnt)
    {
        return mnt.ToString() + " desc";
    }

    public override void ApplyUpdate(object state)
    {
        if (state == null)
        {
            //Debug.Log("null");
            selector.SetActive(false);
            //selectedNode = null;
            if (textbox.isActiveAndEnabled)
            {
                textbox.SetText("", true, true);
            }
            textbox.transform.parent.transform.parent.gameObject.SetActive(false);
            return;
        }

        selector.SetActive(true);
        selectedNode = (Pause_SectionStatus_MenuNode)state;
        //Debug.Log(selectedNode.menuNodeType + " go to");

        textbox.transform.parent.transform.parent.gameObject.SetActive(true);
        textbox.SetText(GetNodeDescription(selectedNode.menuNodeType), true, true);
    }
    public override object GetState()
    {
        return selectedNode;
    }

    public void Setup()
    {
        if (currentLocationText == null)
        {
            return;
        }

        Enum.TryParse(MainManager.Instance.mapScript.mapName, out MainManager.MapID mid);
        currentLocationText.SetText(MainManager.GetAreaName(MainManager.Instance.mapScript.worldLocation) + "<line>" + MainManager.GetMapName(mid), true, true);

        if (MainManager.Instance.lastSaveTimestamp == 0)
        {
            lastSaveText.SetText("New Game", true, true);
        }
        else
        {
            lastSaveText.SetText(MainManager.GetAreaName(MainManager.Instance.lastSaveLocation) + "<line>(" + MainManager.ParseTime(MainManager.Instance.playTime - MainManager.Instance.lastSaveTimestamp) + ") ago", true, true);
        }

        levelText.SetText("Level " + playerData.level, true, true);
        xpText.SetText(" <xp> XP: " + playerData.exp + "/100", true, true);
        if (playerData.astralTokens == 0)
        {
            astralTokenObject.SetActive(false);
        } else
        {
            astralTokenObject.SetActive(true);
            astralTokenText.SetText("(" + playerData.astralTokens + ")", true, true);
        }

        coinDescriptor.SetText("<coin> Coins", true, true);
        shardDescriptor.SetText("<shard> Shards", true, true);
        itemDescriptor.SetText("<carrot> Items", true, true);
        playtimeDescriptor.SetText("<clock> Playtime", true, true);

        coinText.SetText(playerData.coins + "", true, true);
        shardText.SetText(playerData.shards + " (" + playerData.cumulativeShards + " total found)", true, true);
        itemText.SetText(playerData.itemInventory.Count + "", true, true);
        playtimeText.SetText(MainManager.ParseTime(MainManager.Instance.playTime), true, true);

        if (playerData.charmEffects == null || playerData.charmEffects.Count == 0)
        {
            charmEffectObject.SetActive(false);
        } else
        {
            charmEffectObject.SetActive(true);
            string charmText = "";
            for (int i = 0; i < playerData.charmEffects.Count; i++)
            {
                charmText += playerData.charmEffects[i].GetMenuString();
                if (i < playerData.charmEffects.Count - 1)
                {
                    charmText += "<line>";
                }
            }
            charmEffectText.SetText(charmText, true, true);
        }
        if (playerData.innEffects == null || playerData.innEffects.Count == 0)
        {
            restEffectObject.SetActive(false);
        } else
        {
            restEffectObject.SetActive(true);
            InnEffect ie = playerData.innEffects[0];
            restEffectText.SetText("(" + ie.charges + ") " + InnEffect.GetName(ie.innType), true, true);
        }

        PlayerData.PlayerDataEntry wilex = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
        PlayerData.PlayerDataEntry luna = playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
        if (playerData.party.Count == 1)
        {
            PlayerData.PlayerDataEntry lone = (wilex != null ? wilex : luna);

            if (wilex != null)
            {
                characterObjectB.SetActive(false);
                healthObjectB.SetActive(false);

                healthTextA.SetText(lone.hp + "/" + lone.maxHP + " <hp> HP<line>(Agility <stamina> " + (lone.GetAgility(playerData.maxEP) + 1 * playerData.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, lone.entityID)) + ")", true, true);

                characterObjectA.transform.localPosition = new Vector3(0, 0, 0);
                healthObjectA.transform.localPosition = new Vector3(0, 5, 0);

                abilityMedalA1.SetActive(lone.weaponLevel >= 0);

                Image sr = abilityMedalA1.GetComponentInChildren<Image>();
                if (lone.weaponLevel > 0)
                {
                    sr.sprite = weaponMedalSprites[lone.weaponLevel];
                }

                abilityMedalA2.SetActive(lone.weaponLevel >= 2);
                abilityMedalA3.SetActive(lone.jumpLevel >= 2);
                abilityMedalA4.SetActive(lone.jumpLevel >= 1);
            }
            else
            {
                characterObjectA.SetActive(false);
                healthObjectA.SetActive(false);

                healthTextB.SetText(lone.hp + "/" + lone.maxHP + " <hp> HP<line>(Agility <stamina> " + (lone.GetAgility(playerData.maxEP) + 1 * playerData.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, lone.entityID)) + ")", true, true);

                characterObjectB.transform.localPosition = new Vector3(0, 0, 0);
                healthObjectB.transform.localPosition = new Vector3(0, 5, 0);

                Image sr = abilityMedalB1.GetComponentInChildren<Image>();
                if (lone.weaponLevel > 0)
                {
                    sr.sprite = weaponMedalSprites[lone.weaponLevel + 3];
                }

                abilityMedalB1.SetActive(lone.weaponLevel >= 0);
                abilityMedalB2.SetActive(lone.weaponLevel >= 2);
                abilityMedalB3.SetActive(lone.jumpLevel >= 2);
                abilityMedalB4.SetActive(lone.jumpLevel >= 1);
            }
        }
        else
        {
            characterObjectB.SetActive(true);
            healthObjectB.SetActive(true);
            healthTextA.SetText(wilex.hp + "/" + wilex.maxHP + " <hp> HP<line>(Agility <stamina> " + (wilex.GetAgility(playerData.maxEP) + 1 * playerData.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, wilex.entityID)) + ")", true, true);
            healthTextB.SetText(luna.hp + "/" + luna.maxHP + " <hp> HP<line>(Agility <stamina> " + (luna.GetAgility(playerData.maxEP) + 1 * playerData.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, luna.entityID)) + ")", true, true);
            characterObjectA.transform.localPosition = new Vector3(-80, 0, 0);
            characterObjectB.transform.localPosition = new Vector3(80, 0, 0);
            healthObjectA.transform.localPosition = new Vector3(-80, 5, 0);
            healthObjectB.transform.localPosition = new Vector3(80, 5, 0);

            Image sr = abilityMedalA1.GetComponentInChildren<Image>();
            if (wilex.weaponLevel > 0)
            {
                sr.sprite = weaponMedalSprites[wilex.weaponLevel];
            }
            sr = abilityMedalB1.GetComponentInChildren<Image>();
            if (luna.weaponLevel > 0)
            {
                sr.sprite = weaponMedalSprites[luna.weaponLevel + 3];
            }


            abilityMedalA1.SetActive(wilex.weaponLevel >= 0);
            abilityMedalA2.SetActive(wilex.weaponLevel >= 2);
            abilityMedalA3.SetActive(wilex.jumpLevel >= 2);
            abilityMedalA4.SetActive(wilex.jumpLevel >= 1);
            abilityMedalB1.SetActive(luna.weaponLevel >= 0);
            abilityMedalB2.SetActive(luna.weaponLevel >= 2);
            abilityMedalB3.SetActive(luna.jumpLevel >= 2);
            abilityMedalB4.SetActive(luna.jumpLevel >= 1);
        }
        energyText.SetText(playerData.ep + "/" + playerData.maxEP + " <ep> EP", true, true);
        soulText.SetText(playerData.se + "/" + playerData.maxSE + " <se> SE<line>(" + playerData.sp + " SP)", true, true);

        realMenuNodes = menuNodes.FindAll((e) => (e.gameObject.activeInHierarchy));

        foreach (Pause_SectionStatus_MenuNode mn in realMenuNodes)
        {
            mn.RecalculateDirections(realMenuNodes);
        }

        selector.SetActive(false);
        if (selectedNode == null)
        {
            selectedNode = realMenuNodes[0];
            selector.transform.position = selectedNode.transform.position;
        }
    }

    public override void Init()
    {
        playerData = MainManager.Instance.playerData;
        subobject.SetActive(true);
        Setup();

        //Hacky
        textbox.transform.parent.transform.parent.gameObject.SetActive(false);

        base.Init();
    }

    public override void Clear()
    {
        selector.SetActive(false);
        subobject.SetActive(false);
        base.Clear();
    }

    public virtual void Update()
    {
        //Bad idea
        Vector3 desiredPosition = selectedNode.transform.position;

        selector.transform.position = MainManager.EasingQuadraticTime(selector.transform.position, desiredPosition, 16000);
    }
}
