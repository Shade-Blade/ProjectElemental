using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;
using static ShopItem;

public class WorldNPC_Spruce : WorldNPC_InventoryShopkeeper
{
    //Where any flag check should be (or any random inventory generation)
    public override void ShopInventoryInit()
    {
        inventory = new List<ShopItem>();

        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        }
        int floor = int.Parse(floorNo);

        int itemChapter = (floor / 10);

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
            if ((c == itemChapter || (RandomGenerator.Get() < 0.25f && c == itemChapter + 1) || (RandomGenerator.Get() < 0.25f && c == itemChapter - 1)) && c != 10)
            {
                itemPool.Add((Item.ItemType)i);
            }
        }
        List<Item.ItemType> itList = RandomTable<Item.ItemType>.ChooseRandom(itemPool, 5);

        for (int i = 0; i < itList.Count; i++)
        {
            Item.ItemModifier modifier = Item.ItemModifier.None;
            if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_RandomItemModifiers))
            {
                modifier = GlobalItemScript.GetRandomModifier(itList[i]);
            }

            PickupUnion pu = new PickupUnion(new Item(itList[i], modifier));
            inventory.Add(new ShopItem(pu, 2 * PickupUnion.GetBaseCost(pu), Currency.Coin));
        }


        //3 past items at a discount
        itemPool = new List<Item.ItemType>();
        for (int i = 1; i < (int)Item.ItemType.EndOfTable; i++)
        {
            int c = Item.GetItemDataEntry((Item.ItemType)i).chapter;
            if (c > itemChapter - 3 && c < itemChapter && c != 10)
            {
                itemPool.Add((Item.ItemType)i);
            }
        }

        itList = RandomTable<Item.ItemType>.ChooseRandom(itemPool, 3);
        for (int i = 0; i < itList.Count; i++)
        {
            Item.ItemModifier modifier = Item.ItemModifier.None;
            if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_RandomItemModifiers))
            {
                modifier = GlobalItemScript.GetRandomModifier(itList[i]);
            }

            PickupUnion pu = new PickupUnion(new Item(itList[i], modifier));
            inventory.Add(new ShopItem(pu, PickupUnion.GetBaseCost(pu), Currency.Coin));
        }


        //1 future item at a 2x upcharge
        itemPool = new List<Item.ItemType>();
        for (int i = 1; i < (int)Item.ItemType.EndOfTable; i++)
        {
            int c = Item.GetItemDataEntry((Item.ItemType)i).chapter;
            if (c > itemChapter && c < itemChapter + 3 && c != 10)
            {
                itemPool.Add((Item.ItemType)i);
            }
        }

        itList = RandomTable<Item.ItemType>.ChooseRandom(itemPool, 2);
        for (int i = 0; i < itList.Count; i++)
        {
            Item.ItemModifier modifier = Item.ItemModifier.None;
            if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_RandomItemModifiers))
            {
                modifier = GlobalItemScript.GetRandomModifier(itList[i]);
            }

            PickupUnion pu = new PickupUnion(new Item(itList[i], modifier));
            inventory.Add(new ShopItem(pu, 3 * PickupUnion.GetBaseCost(pu), Currency.Coin));
        }
    }
}
