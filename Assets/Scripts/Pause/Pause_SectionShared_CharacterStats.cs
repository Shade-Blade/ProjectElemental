using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause_SectionShared_CharacterStats : Pause_SectionShared
{
    public Image characterIcon;
    public Sprite wilexIcon;
    public Sprite lunaIcon;

    public Image leftArrow;
    public Image rightArrow;

    public BattleHelper.EntityID entityID;

    public TextDisplayer specificStats;
    public TextDisplayer partyStats;

    public Image backImage;

    public override void ApplyUpdate(object state)
    {
        //Wacky hacky signalling thing
        if (state == null)
        {
            if (backImage != null)
            {
                backImage.color = new Color(1, 1, 1, 1);
            }
            return;
        }
        if (backImage != null)
        {
            backImage.color = new Color(1, 0.9f, 0.8f, 1);
        }

        PlayerData pd = MainManager.Instance.playerData;

        entityID = (BattleHelper.EntityID)state;
        //Debug.Log("Received update " + entityID);
        if (entityID != BattleHelper.EntityID.Wilex && entityID != BattleHelper.EntityID.Luna)
        {
            return;
        }

        if (entityID == BattleHelper.EntityID.Wilex)
        {
            characterIcon.sprite = wilexIcon;
        }
        if (entityID == BattleHelper.EntityID.Luna)
        {
            characterIcon.sprite = lunaIcon;
        }

        if (pd.party.Count < 2)
        {
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
        } else
        {
            leftArrow.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(true);
        }

        PlayerData.PlayerDataEntry pde = pd.GetPlayerDataEntry(entityID);

        //to do: make this use menu text (but I also need to make everything else use menu text too)
        specificStats.SetText(pde.hp + "/" + pde.maxHP + " <hp><line>(Agility <stamina>" + (pde.GetAgility(pd.maxEP) + 1 * pd.BadgeEquippedCountFull(Badge.BadgeType.AgilityBoost, entityID)) + ")", true, true);
        partyStats.SetText(pd.ep + "/" + pd.maxEP + " <ep><line>" + pd.se + "/" + pd.maxSE + " <se>", true, true);
    }

    public override object GetState()
    {
        //Debug.Log("Asked for state " + entityID);
        return entityID;
    }
}
