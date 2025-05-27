using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleSightScript : MonoBehaviour
{
    PlayerData pd;

    PlayerEntity wilex;
    PlayerEntity luna;
    PlayerData.PlayerDataEntry wilexPDE;
    PlayerData.PlayerDataEntry lunaPDE;

    //will be set in update step so avoiding TextDisplayer overhead is a good idea
    public TMP_Text turnCount;
    public TMP_Text damageDealt;
    public TMP_Text damageTaken;
    public TMP_Text maxDamagePerTurn;
    public TMP_Text maxDamageSingleHit;

    public void Setup(PlayerData p_pd, PlayerEntity p_wilex, PlayerEntity p_luna)
    {
        pd = p_pd;
        wilex = p_wilex;
        luna = p_luna;

        wilexPDE = pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
        lunaPDE = pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
        TextUpdate();
    }

    public void Update()
    {
        TextUpdate();
    }

    public void TextUpdate()
    {
        turnCount.text = "Turn " + BattleControl.Instance.turnCount + " - Battle " + pd.totalBattles;

        int currentDamageDealt = 0;
        if (wilex != null)
        {
            currentDamageDealt += wilex.damageDealt;
        }
        if (luna != null)
        {
            currentDamageDealt += luna.damageDealt;
        }


        int currentDamageTaken = 0;
        if (wilex != null)
        {
            currentDamageTaken += wilex.damageTaken;
        }
        if (luna != null)
        {
            currentDamageTaken += luna.damageTaken;
        }

        string damageDealtText = "Damage Dealt: ";
        damageDealtText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkRed Outline + Overlay\"><color=#ff5050>";
        damageDealtText += wilex == null ? "?" : wilex.damageDealt;
        damageDealtText += " (";
        damageDealtText += wilexPDE == null ? "?" : wilexPDE.cumulativeDamageDealt;
        damageDealtText += ")</font></color> | ";
        damageDealtText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkGreen Outline + Overlay\"><color=#00ff00>";
        damageDealtText += luna == null ? "?" : luna.damageDealt;
        damageDealtText += " (";
        damageDealtText += lunaPDE == null ? "?" : lunaPDE.cumulativeDamageDealt;
        damageDealtText += ")</font></color> | ";
        damageDealtText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkYellow Outline + Overlay\"><color=#ffff00>";
        damageDealtText += currentDamageDealt;
        damageDealtText += " (";
        damageDealtText += pd.cumulativeDamageDealt;
        damageDealtText += ")</font></color>";

        damageDealt.text = damageDealtText;


        string damageTakenText = "Damage Taken: ";
        damageTakenText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkRed Outline + Overlay\"><color=#ff5050>";
        damageTakenText += wilex == null ? "?" : wilex.damageTaken;
        damageTakenText += " (";
        damageTakenText += wilexPDE == null ? "?" : wilexPDE.cumulativeDamageTaken;
        damageTakenText += ")</font></color> | ";
        damageTakenText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkGreen Outline + Overlay\"><color=#00ff00>";
        damageTakenText += luna == null ? "?" : luna.damageTaken;
        damageTakenText += " (";
        damageTakenText += lunaPDE == null ? "?" : lunaPDE.cumulativeDamageTaken;
        damageTakenText += ")</font></color> | ";
        damageTakenText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkYellow Outline + Overlay\"><color=#ffff00>";
        damageTakenText += currentDamageTaken;
        damageTakenText += " (";
        damageTakenText += pd.cumulativeDamageTaken;
        damageTakenText += ")";

        damageTaken.text = damageTakenText;


        string maxDamagePerTurnText = "Max Damage Per Turn: ";
        maxDamagePerTurnText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkRed Outline + Overlay\"><color=#ff5050>";
        maxDamagePerTurnText += wilexPDE == null ? "?" : wilexPDE.maxDamagePerTurn;
        maxDamagePerTurnText += "</font></color> | ";
        maxDamagePerTurnText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkGreen Outline + Overlay\"><color=#00ff00>";
        maxDamagePerTurnText += lunaPDE == null ? "?" : lunaPDE.maxDamagePerTurn;
        maxDamagePerTurnText += "</font></color> | ";
        maxDamagePerTurnText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkYellow Outline + Overlay\"><color=#ffff00>";
        maxDamagePerTurnText += pd.maxDamagePerTurn;

        maxDamagePerTurn.text = maxDamagePerTurnText;


        string maxDamageSingleHitText = "Max Damage Single Hit: ";
        maxDamageSingleHitText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkRed Outline + Overlay\"><color=#ff5050>";
        maxDamageSingleHitText += wilexPDE == null ? "?" : wilexPDE.maxDamageSingleHit;
        maxDamageSingleHitText += "</font></color> | ";
        maxDamageSingleHitText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkGreen Outline + Overlay\"><color=#00ff00>";
        maxDamageSingleHitText += lunaPDE == null ? "?" : lunaPDE.maxDamageSingleHit;
        maxDamageSingleHitText += "</font></color> | ";
        maxDamageSingleHitText += "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkYellow Outline + Overlay\"><color=#ffff00>";
        maxDamageSingleHitText += pd.maxDamageSingleHit;

        maxDamageSingleHit.text = maxDamageSingleHitText;
    }
}
