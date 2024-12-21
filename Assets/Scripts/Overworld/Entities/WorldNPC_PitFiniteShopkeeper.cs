using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;

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
        }
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
