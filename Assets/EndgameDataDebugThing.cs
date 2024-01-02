using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameDataDebugThing : InteractTrigger
{
    public override void Interact()
    {
        PlayerData playerData = new PlayerData(BattleHelper.EntityID.Wilex, BattleHelper.EntityID.Luna);
        //playerData.TryAddToCurrent(BattleHelper.EntityID.Turtle);

        //Level numbers
        //Start = 1
        //C1 ~= 4
        //C2 ~= 7
        //C3 ~= 10
        //C4 ~= 13
        //C5 ~= 16
        //C6 ~= 19
        //C7 ~= 22
        //C8 ~= 25 (cap)
        //endgame cap = 31
        playerData.level = 31;
        playerData.upgrades = new List<PlayerData.LevelUpgrade>();
        //playerData.upgrades = new List<PlayerData.LevelUpgrade> { PlayerData.LevelUpgrade.HP, PlayerData.LevelUpgrade.EP, PlayerData.LevelUpgrade.SP };

        playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).jumpLevel = 2;
        playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).weaponLevel = 2;
        playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna).jumpLevel = 2;
        playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna).weaponLevel = 2;

        for (int i = 0; i < playerData.level - 1; i++)
        {
            switch (i % 3)
            {
                case 0:
                    playerData.upgrades.Add(PlayerData.LevelUpgrade.HP);
                    break;
                case 1:
                    playerData.upgrades.Add(PlayerData.LevelUpgrade.EP);
                    break;
                case 2:
                    playerData.upgrades.Add(PlayerData.LevelUpgrade.SP);
                    break;
            }
        }

        playerData.UpdateMaxStats();
        playerData.FullHeal();


        for (int i = 1; i < (int)(KeyItem.KeyItemType.EndOfTable); i++)
        {
            playerData.keyInventory.Add(new KeyItem((KeyItem.KeyItemType)i));
        }


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

        MainManager.Instance.playerData = playerData;
    }
}
