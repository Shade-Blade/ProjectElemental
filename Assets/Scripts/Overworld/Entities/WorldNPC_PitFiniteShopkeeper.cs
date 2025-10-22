using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;
using static ShopItem;

public class WorldNPC_PitFiniteShopkeeper : WorldNPC_FiniteShopkeeper
{
    public PitNPCHolderScript.PitNPC npc;

    public override void ShopInventoryInit()
    {
        items = new List<ShopItem>();
        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        }
        int floor = int.Parse(floorNo);

        switch (npc)
        {
            case PitNPCHolderScript.PitNPC.Roielle:
                MakeShopItemList(floor + 30, 3, ShopItem.Currency.Coin, 2);
                break;
            case PitNPCHolderScript.PitNPC.ShopkeeperMosquito:
                MakeShopItemList(floor + 20, 3, ShopItem.Currency.HP, 1f);
                break;
            case PitNPCHolderScript.PitNPC.ShopkeeperSpeartongue:
                MakeShopItemList(floor + 10, 3, ShopItem.Currency.EP, 1f);
                break;
            case PitNPCHolderScript.PitNPC.Embra:
                MakeShopRandomBags(floor);
                break;
            case PitNPCHolderScript.PitNPC.Osmi:
                MakeShopRandomEquipment(floor);
                break;
        }
    }

    public void MakeShopRandomBags(int baseFloor)
    {
        List<ShopItem> extList = new List<ShopItem>();
        PickupUnion pu = new PickupUnion(MiscSprite.MysteryItem);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.ItemBag2);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.ItemBag4);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.RecipeBag2);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.RecipeBag4);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.ItemBagX);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));

        items = RandomTable<ShopItem>.ChooseRandom(extList, 3);
    }

    public void MakeShopRandomEquipment(int baseFloor)
    {
        List<ShopItem> extList = new List<ShopItem>();
        PickupUnion pu = new PickupUnion(MiscSprite.MysteryBadge);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.ItemBag2);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.MysteryRibbon);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.MysteryBadge);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));
        pu = new PickupUnion(MiscSprite.RecipeBagX);
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, PickupUnion.GetBaseCost(pu)), ShopItem.Currency.Coin));

        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        }
        int floor = int.Parse(floorNo);

        int badgeChapter = (floor / 10);

        PlayerData pd = MainManager.Instance.playerData;

        if (badgeChapter < 0)
        {
            badgeChapter = 0;
        }
        if (badgeChapter > 9)
        {
            badgeChapter = 9;
        }
        //Debug.Log(itemChapter);
        List<Badge.BadgeType> badgePool = new List<Badge.BadgeType>();
        for (int i = 1; i < (int)Badge.BadgeType.EndOfTable; i++)
        {
            if (floor > 100)
            {
                badgePool.Add((Badge.BadgeType)i);
            }
            else
            {
                int c = Badge.GetBadgeDataEntry((Badge.BadgeType)i).chapter;
                if (c == badgeChapter + 2 || (RandomGenerator.Get() < 0.5f && c == badgeChapter + 3) || (RandomGenerator.Get() < 0.5f && c == badgeChapter + 1))
                {
                    //???
                    if (pd.badgeInventory.Find((e) => (e.type == (Badge.BadgeType)i)).type != (Badge.BadgeType)i)
                    {
                        badgePool.Add((Badge.BadgeType)i);
                    }
                }
            }
        }
        List<Badge.BadgeType> btList = RandomTable<Badge.BadgeType>.ChooseRandom(badgePool, 4);
        pu = new PickupUnion(new Badge(btList[0]));
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, (int)(0.6f * PickupUnion.GetBaseCost(pu))), ShopItem.Currency.Coin));
        pu = new PickupUnion(new Badge(btList[1]));
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, (int)(1.3f * PickupUnion.GetBaseCost(pu))), ShopItem.Currency.Coin));
        pu = new PickupUnion(new Badge(btList[2]));
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, (int)(0.4f * PickupUnion.GetBaseCost(pu))), ShopItem.Currency.Coin));
        pu = new PickupUnion(new Badge(btList[3]));
        extList.Add(new ShopItem(pu, ShopItem.ConvertCost(ShopItem.Currency.Coin, (int)(1.6f * PickupUnion.GetBaseCost(pu))), ShopItem.Currency.Coin));

        items = RandomTable<ShopItem>.ChooseRandom(extList, 3);
    }

    public void MakeShopItemList(int baseFloor, int count, ShopItem.Currency currency, float costMult)
    {
        int itemChapter = (baseFloor / 10);

        if (itemChapter < 0)
        {
            itemChapter = 0;
        }
        if (itemChapter > 9)
        {
            itemChapter = 9;
        }
        //Debug.Log(itemChapter);
        List<Item.ItemType> itemPool = new List<Item.ItemType>();
        for (int i = 1; i < (int)Item.ItemType.EndOfTable; i++)
        {
            int c = Item.GetItemDataEntry((Item.ItemType)i).chapter;
            if (c == itemChapter || (RandomGenerator.Get() < 0.25f && c == itemChapter + 1) || (RandomGenerator.Get() < 0.25f && c == itemChapter - 1))
            {
                if (c != 10)    //don't include the eggs because they are weak items on purpose
                {
                    itemPool.Add((Item.ItemType)i);
                }
            }
        }
        List<Item.ItemType> itList = RandomTable<Item.ItemType>.ChooseRandom(itemPool, 3);

        for (int i = 0; i < itList.Count; i++)
        {
            Item.ItemModifier modifier = Item.ItemModifier.None;
            if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_RandomItemModifiers))
            {
                modifier = GlobalItemScript.GetRandomModifier(itList[i]);
            }

            PickupUnion pu = new PickupUnion(new Item(itList[i], modifier));
            items.Add(new ShopItem(pu, ShopItem.ConvertCost(currency, PickupUnion.GetBaseCost(pu)), currency));
        }
    }
}
