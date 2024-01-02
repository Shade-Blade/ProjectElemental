using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PlayerData;

public class LevelUpMenuScript : MonoBehaviour
{
    public GameObject[] options;
    public GameObject pointer;

    //a lot of hardcoded references :P
    public Image hpImage;
    public Image epImage;
    public Image spImage;

    public string[] descriptions;
    public bool[] usages;

    protected float inputDir;

    public GameObject wilexHPIndicator;
    public GameObject wilexHPIndicatorArrow;
    public TextDisplayer wilexHPIndicatorLeft;
    public TextDisplayer wilexHPIndicatorRight;

    public GameObject lunaHPIndicator;
    public GameObject lunaHPIndicatorArrow;
    public TextDisplayer lunaHPIndicatorLeft;
    public TextDisplayer lunaHPIndicatorRight;

    public GameObject EPIndicator;
    public GameObject EPIndicatorArrow;
    public TextDisplayer EPIndicatorLeft;
    public TextDisplayer EPIndicatorRight;

    public GameObject wilexAgilityIndicator;
    public TextDisplayer wilexAgilityIndicatorLeft;
    public TextDisplayer wilexAgilityIndicatorRight;

    public GameObject lunaAgilityIndicator;
    public TextDisplayer lunaAgilityIndicatorLeft;
    public TextDisplayer lunaAgilityIndicatorRight;

    public GameObject SPIndicator;
    public GameObject SPIndicatorArrow;
    public TextDisplayer SPIndicatorLeft;
    public TextDisplayer SPIndicatorRight;

    public GameObject SEIndicator;
    public TextDisplayer SEIndicatorLeft;
    public TextDisplayer SEIndicatorRight;

    public DescriptionBoxScript descBox;

    public bool done = false;
    public bool didUpgrade = false;
    public int state = 0;
    public float stateWait = 0;

    public int selectIndex = 0;

    PlayerData pd;

    public void Setup(PlayerData pd)
    {
        this.pd = pd;
        done = false;
        didUpgrade = false;
        state = 0;

        //Build everything
        descriptions = new string[3];
        descriptions[0] = "Increase your Max HP if you want to survive longer in battles.";
        descriptions[1] = "Increase your Max EP if you want to use more moves, or use moves more often.";
        descriptions[2] = "Increase your Max SP if you want to use more Soul Moves or equip more Badges.";

        int hpUpgrades = new List<LevelUpgrade>(pd.upgrades).FindAll((e) => (e == LevelUpgrade.HP)).Count;
        int epUpgrades = new List<LevelUpgrade>(pd.upgrades).FindAll((e) => (e == LevelUpgrade.EP)).Count;
        int spUpgrades = new List<LevelUpgrade>(pd.upgrades).FindAll((e) => (e == LevelUpgrade.SP)).Count;
        int hpDowngrades = new List<LevelUpgrade>(pd.downgrades).FindAll((e) => (e == LevelUpgrade.HP)).Count;
        int epDowngrades = new List<LevelUpgrade>(pd.downgrades).FindAll((e) => (e == LevelUpgrade.EP)).Count;
        int spDowngrades = new List<LevelUpgrade>(pd.downgrades).FindAll((e) => (e == LevelUpgrade.SP)).Count;

        hpUpgrades -= hpDowngrades;
        epUpgrades -= epDowngrades;
        spUpgrades -= spDowngrades;

        usages = new bool[3];
        usages[0] = false;
        if (hpUpgrades < PlayerData.GetMaxUpgrades())
        {
            usages[0] = true;
        }
        int hpOffset = 0;
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Greed))
        {
            hpOffset = 1;
            usages[0] = hpUpgrades < 0;    //max is considered 0 upgrades
        }
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Envy))
        {
            usages[0] = false;
        }

        if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) == null)
        {
            wilexHPIndicator.SetActive(false);
            wilexAgilityIndicator.SetActive(false);
        }
        if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna) == null)
        {
            lunaHPIndicator.SetActive(false);
            lunaAgilityIndicator.SetActive(false);
        }

        PlayerData.PlayerDataEntry wilex = pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
        PlayerData.PlayerDataEntry luna = pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna);

        if (!usages[0])
        {
            descriptions[0] = "You can't upgrade that!";
            hpImage.color = new Color(0.5f, 0.5f, 0.5f);
            //Only one number appears
            if (wilexHPIndicator.activeSelf)
            {
                wilexHPIndicatorArrow.SetActive(false);
                wilexHPIndicatorRight.gameObject.SetActive(false);
                wilexHPIndicatorLeft.gameObject.transform.localPosition = Vector3.zero;
                wilexHPIndicatorLeft.SetText(PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Wilex, hpUpgrades - hpOffset + pd.BadgeEquippedCountFull(Badge.BadgeType.HPPlus, BattleHelper.EntityID.Wilex)) + "", true, true);
                if (!lunaHPIndicator.activeSelf)
                {
                    wilexHPIndicator.gameObject.transform.localPosition = Vector3.down * 75;
                }
            }
            if (lunaHPIndicator.activeSelf)
            {
                lunaHPIndicatorArrow.SetActive(false);
                lunaHPIndicatorRight.gameObject.SetActive(false);
                lunaHPIndicatorLeft.gameObject.transform.localPosition = Vector3.zero;
                lunaHPIndicatorLeft.SetText(PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Luna, hpUpgrades - hpOffset + pd.BadgeEquippedCountFull(Badge.BadgeType.HPPlus, BattleHelper.EntityID.Luna)) + "", true, true);
            }
        }
        else
        {
            //Both numbers appear?
            if (wilexHPIndicator.activeSelf)
            {
                wilexHPIndicatorLeft.SetText(PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Wilex, hpUpgrades - hpOffset + pd.BadgeEquippedCountFull(Badge.BadgeType.HPPlus, BattleHelper.EntityID.Wilex)) + "", true, true);
                wilexHPIndicatorRight.SetText(PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Wilex, hpUpgrades + 1 - hpOffset + pd.BadgeEquippedCountFull(Badge.BadgeType.HPPlus, BattleHelper.EntityID.Wilex)) + "", true, true);
                if (!lunaHPIndicator.activeSelf)
                {
                    wilexHPIndicator.gameObject.transform.localPosition = Vector3.down * 75;
                }
            }
            if (lunaHPIndicator.activeSelf)
            {
                lunaHPIndicatorLeft.SetText(PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Luna, hpUpgrades - hpOffset + pd.BadgeEquippedCountFull(Badge.BadgeType.HPPlus, BattleHelper.EntityID.Luna)) + "", true, true);
                lunaHPIndicatorRight.SetText(PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Luna, hpUpgrades + 1 - hpOffset + pd.BadgeEquippedCountFull(Badge.BadgeType.HPPlus, BattleHelper.EntityID.Luna)) + "", true, true);
            }
        }

        usages[1] = false;
        if (epUpgrades < PlayerData.GetMaxUpgrades())
        {
            usages[1] = true;
        }
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Envy))
        {
            usages[1] = false;
        }
        if (!usages[1])
        {
            descriptions[1] = "You can't upgrade that!";
            epImage.color = new Color(0.5f, 0.5f, 0.5f);
            //only 1 number appears
            wilexAgilityIndicator.gameObject.SetActive(false);
            lunaAgilityIndicator.gameObject.SetActive(false);
            EPIndicatorArrow.SetActive(false);
            EPIndicatorLeft.gameObject.transform.localPosition = Vector3.zero;
            EPIndicatorLeft.SetText(PlayerData.GetMaxEP(epUpgrades + pd.BadgeEquippedCount(Badge.BadgeType.EPPlus)) + "", true, true);
            EPIndicatorRight.gameObject.SetActive(false);
        } else
        {
            //both numbers appear
            if (wilexAgilityIndicator.activeSelf)
            {
                wilexAgilityIndicatorLeft.SetText(wilex.GetAgility(PlayerData.GetMaxEP(epUpgrades + pd.BadgeEquippedCount(Badge.BadgeType.EPPlus))) + 2 * pd.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, wilex.entityID) + "", true, true);
                wilexAgilityIndicatorRight.SetText(wilex.GetAgility(PlayerData.GetMaxEP(epUpgrades + 1 + pd.BadgeEquippedCount(Badge.BadgeType.EPPlus))) + 2 * pd.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, wilex.entityID) + "", true, true);
                if (!lunaAgilityIndicator.activeSelf)
                {
                    wilexAgilityIndicator.gameObject.transform.localPosition = Vector3.down * 45;
                }
            }
            if (lunaAgilityIndicator.activeSelf)
            {
                lunaAgilityIndicatorLeft.SetText(luna.GetAgility(PlayerData.GetMaxEP(epUpgrades + pd.BadgeEquippedCount(Badge.BadgeType.EPPlus))) + 2 * pd.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, luna.entityID) + "", true, true);
                lunaAgilityIndicatorRight.SetText(luna.GetAgility(PlayerData.GetMaxEP(epUpgrades + 1 + pd.BadgeEquippedCount(Badge.BadgeType.EPPlus))) + 2 * pd.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, luna.entityID) + "", true, true);
            }
            EPIndicatorLeft.SetText(PlayerData.GetMaxEP(epUpgrades + pd.BadgeEquippedCount(Badge.BadgeType.EPPlus)) + "", true, true);
            EPIndicatorRight.SetText(PlayerData.GetMaxEP(epUpgrades + 1 + pd.BadgeEquippedCount(Badge.BadgeType.EPPlus)) + "", true, true);
        }

        usages[2] = false;
        if (spUpgrades < PlayerData.GetMaxUpgrades())
        {
            usages[2] = true;
        }
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Envy))
        {
            usages[2] = true;   //forced upgrade
        }
        if (!usages[2])
        {
            descriptions[2] = "You can't upgrade that!";
            spImage.color = new Color(0.5f, 0.5f, 0.5f);
            //only 1 number appears
            SEIndicator.SetActive(false);

            SPIndicatorArrow.SetActive(false);
            SPIndicatorLeft.SetText(PlayerData.GetMaxSP(spUpgrades) + "", true, true); ;
            SPIndicatorLeft.gameObject.transform.localPosition = Vector3.zero;
            SPIndicatorRight.gameObject.SetActive(false);
        } else
        {
            SPIndicatorLeft.SetText(PlayerData.GetMaxSP(spUpgrades) + "", true, true);;
            SPIndicatorRight.SetText(PlayerData.GetMaxSP(spUpgrades + 1) + "", true, true);
            if (pd.GetMaxSP() != pd.maxSE)
            {
                SEIndicatorLeft.SetText(pd.GetMaxSE(spUpgrades) + "", true, true);
                SEIndicatorRight.SetText((pd.GetMaxSE(spUpgrades + 1)) + "", true, true);
            }
            else
            {
                SEIndicator.SetActive(false);
            }
        }

        descBox.gameObject.SetActive(false);
        pointer.SetActive(false);
        gameObject.transform.localPosition = Vector3.up * 2000;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case 0: //Fly down
                stateWait += Time.deltaTime;
                gameObject.transform.localPosition = MainManager.EasingQuadraticTime(gameObject.transform.localPosition, Vector3.zero, 200);
                if (stateWait > 0.5f)
                {
                    gameObject.transform.localPosition = Vector3.zero;
                    state = 1;
                    stateWait = 0;

                    descBox.gameObject.SetActive(true);
                    descBox.SetText(descriptions[selectIndex], true, true);
                    pointer.SetActive(true);
                    pointer.transform.localPosition = options[selectIndex].transform.localPosition + Vector3.up * 350;  //decided to leave this in (so it flies in instead of popping in)
                }
                break;
            case 1: //control
                if (Mathf.Sign(InputManager.GetAxisHorizontal()) != inputDir || InputManager.GetAxisHorizontal() == 0)
                {
                    inputDir = Mathf.Sign(InputManager.GetAxisHorizontal());
                    if (InputManager.GetAxisHorizontal() == 0)
                    {
                        inputDir = 0;
                    }

                    if (inputDir != 0)
                    {
                        //inputDir positive = up and - index, negative = down and + index
                        if (inputDir > 0)
                        {
                            selectIndex++;
                            //Debug.Log(selectIndex + "+");
                        }
                        else
                        {
                            selectIndex--;
                            //Debug.Log(selectIndex + "-");
                        }
                    }
                    //Debug.Log(selectIndex);

                    if (selectIndex < 0)
                    {
                        selectIndex = 0;
                    }
                    if (selectIndex > 2)
                    {
                        selectIndex = 2;
                    }
                }
                descBox.SetText(descriptions[selectIndex], true, true);
                pointer.transform.localPosition = MainManager.EasingQuadraticTime(pointer.transform.localPosition, options[selectIndex].transform.localPosition + Vector3.up * 120, 5000);
                if (usages[selectIndex] && InputManager.GetButtonDown(InputManager.Button.A))
                {
                    //Select
                    state = 2;
                    stateWait = 0;
                }
                //failsafe
                if (!usages[0] && !usages[1] && !usages[2])
                {
                    state = 3;
                    stateWait = 0;
                }
                break;
            case 2: //picked a thing
                if (!didUpgrade)
                {
                    switch (selectIndex)
                    {
                        case 0:
                            pd.upgrades.Add(LevelUpgrade.HP);
                            List<PlayerEntity> peList = BattleControl.Instance.GetPlayerEntities();
                            for (int i = 0; i < peList.Count; i++)
                            {
                                PlayerData.PlayerDataEntry pde = pd.GetPlayerDataEntry(peList[i].entityID);
                                if (pde != null)
                                {
                                    peList[i].maxHP = pd.GetMaxHP(peList[i].entityID);
                                    peList[i].hp = peList[i].maxHP;
                                }
                            }
                            break;
                        case 1:
                            pd.upgrades.Add(LevelUpgrade.EP);
                            BattleControl.Instance.maxEP = pd.GetMaxEP();
                            BattleControl.Instance.ep = BattleControl.Instance.maxEP;
                            break;
                        case 2:
                            pd.upgrades.Add(LevelUpgrade.SP);
                            BattleControl.Instance.maxSE = pd.GetMaxSE();
                            BattleControl.Instance.se = BattleControl.Instance.maxSE;
                            break;
                    }
                    pd.FullHeal();
                    //Debug.Log(pd);
                    didUpgrade = true;
                }
                pointer.SetActive(false);
                descBox.gameObject.SetActive(false);
                stateWait += Time.deltaTime;
                //movement
                for (int i = 0; i < options.Length; i++)
                {
                    if (i != selectIndex)
                    {
                        if (options[i] != null)
                        {
                            Destroy(options[i]);
                        }
                    }
                    else
                    {                        
                        options[i].transform.localPosition = MainManager.EasingQuadraticTime(options[i].transform.localPosition, Vector3.zero, 3200);
                        options[i].transform.localScale = MainManager.EasingQuadraticTime(options[i].transform.localScale, Vector3.one * 1.5f, 16);
                    }
                }
                if (stateWait > 0.25f)
                {
                    state = 3;
                    stateWait = 0;
                }
                break;
            case 3:
                stateWait += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.up * stateWait * 5000;

                if (stateWait > 0.25f)
                {
                    Destroy(gameObject);
                }

                break;
        }
    }
}
