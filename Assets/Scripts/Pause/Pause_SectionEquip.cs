using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Pause_HandlerEquip;

public class Pause_SectionEquip : Pause_SectionShared_SideTabs
{
    //public GameObject subobject;

    public TextDisplayer sortText;
    public TextDisplayer sortTip;

    public Pause_SectionEquip_Inventory inventory;
    public Pause_SectionShared_CharacterStats characterStats;

    public TextDisplayer badgeText;
    public Image badgePieChart;

    public Image selectorArrow;

    public override void ApplyUpdate(object state)
    {
        selectorArrow.color = new Color(0.5f, 0.5f, 0.5f, 1);
        sortTip.gameObject.SetActive(true);
        sortTip.SetText("(<button,z> to change)", true, true);
        textbox.transform.parent.transform.parent.gameObject.SetActive(false);
        if (state == null)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                //I could make this better but I am lazy, and it doesn't really matter anyway
                //Less room for error if I runtime init this instead of manually setting it?
                if (tabImages == null || tabImages.Length != tabs.Length)
                {
                    tabImages = new Image[tabs.Length];
                }
                if (tabImages[i] == null)
                {
                    tabImages[i] = tabs[i].GetComponent<Image>();
                }
                tabImages[i].color = new Color(0.75f, 0.75f, 0.75f, 1);
            }

            return;
        }
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabImages == null || tabImages.Length != tabs.Length)
            {
                tabImages = new Image[tabs.Length];
            }
            if (tabImages[i] == null)
            {
                tabImages[i] = tabs[i].GetComponent<Image>();
            }
            tabImages[i].color = new Color(0.9f, 0.9f, 0.9f, 1);
        }

        Pause_HandlerEquip.UpdateObject uo = (Pause_HandlerEquip.UpdateObject)state;

        int index = uo.tabindex;
        tabIndex = index;
        int badgeSelected = uo.badgeIndex;

        Pause_HandlerEquip.SortMode sortMode = uo.sortMode;

        sortText.SetText(sortMode.ToString(), true, true);

        UpdateSPUsage();

        Pause_HandlerEquip.BadgeSubpage bs = (Pause_HandlerEquip.BadgeSubpage)tabIndex;

        characterStats.ApplyUpdate(uo.player);

        inventory.Clear();
        inventory.bs = bs;
        inventory.selectedPlayer = uo.player;
        PlayerData pd = MainManager.Instance.playerData;
        if (inventory.selectedPlayer == 0)
        {
            inventory.selectedPlayer = pd.GetSortedParty()[0].entityID;
        }
        inventory.menuIndex = badgeSelected;
        inventory.sortMode = sortMode;
        //Debug.Log(badgeSelected);
        inventory.Init();

        characterStats.ApplyUpdate(inventory.selectedPlayer);

        //Debug.Log("New state: " + index + ", badge selected " + badgeSelected);
        return;
    }
    public void UpdateSPUsage()
    {
        PlayerData pd = MainManager.Instance.playerData;
        badgeText.SetText((pd.sp - pd.usedSP) + "/" + pd.sp + " SP", true, true);
        badgePieChart.fillAmount = 1 - (pd.usedSP / (pd.sp + 0.0f));
    }
    public void DisableSortTip()
    {
        sortTip.gameObject.SetActive(false);
    }
    public override object GetState()
    {
        //polls stuff from SectionEquip_Inventory
        Pause_HandlerEquip.UpdateObject uo = new UpdateObject(tabIndex, inventory.selectedPlayer, inventory.menuIndex, inventory.sortMode);
        return uo;
    }

    public void UpdateCharacterSection(BattleHelper.EntityID eid)
    {
        //inventory.selectedPlayer = eid;
        characterStats.ApplyUpdate(eid);
    }

    public override Pause_SectionShared GetSubsection(object state)
    {
        ApplyUpdate(state);
        return subsections[0];
    }

    public override void Init()
    {
        if (tabIndex < 0)
        {
            tabIndex = 0;
        }

        if (!RibbonsAvailable())
        {
            tabs[3].SetActive(false);
        } else
        {
            tabs[3].SetActive(true);
        }

        selectorArrow.gameObject.SetActive(false);

        sortTip.gameObject.SetActive(true);
        sortTip.enabled = true;
        sortTip.SetText("(<button,z> to change)", true, true);

        PlayerData pd = MainManager.Instance.playerData;
        subobject.SetActive(true);
        badgeText.SetText((pd.sp - pd.usedSP) + "/" + pd.sp + " SP", true, true);
        badgePieChart.fillAmount = 1 - (pd.usedSP / (pd.sp + 0.0f));

        Pause_HandlerEquip.BadgeSubpage bs = (Pause_HandlerEquip.BadgeSubpage)tabIndex;
        inventory.bs = bs;
        inventory.selectedPlayer = (BattleHelper.EntityID)(characterStats.GetState());
        if (inventory.selectedPlayer == 0)
        {
            inventory.selectedPlayer = pd.GetSortedParty()[0].entityID;
            characterStats.ApplyUpdate(pd.GetSortedParty()[0].entityID);
        }

        //Hacky
        textbox.transform.parent.transform.parent.gameObject.SetActive(false);

        inventory.Init();
        base.Init();
    }

    public override void Clear()
    {
        inventory.Clear();
        subobject.SetActive(false);
        base.Clear();
    }
}
