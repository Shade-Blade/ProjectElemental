using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//C# doesn't have union types
//But I need some way to hold together all the pickupable things
[System.Serializable]
public class PickupUnion
{
    public enum PickupType
    {
        None,
        Coin,       //1 coin
        SilverCoin, //5 coins
        GoldCoin,   //25 coins
        Shard,
        Item,
        KeyItem,
        Badge,
        Ribbon
    }

    public PickupType type;

    public Item item;
    public KeyItem keyItem;
    public Badge badge;
    public Ribbon ribbon;

    public PickupUnion()
    {
        type = PickupType.None;
        item = default;
        keyItem = default;
        badge = default;
        ribbon = default;
    }
    public PickupUnion(PickupType p_type)
    {
        type = p_type;

        item = default;
        keyItem = default;
        badge = default;
        ribbon = default;
    }
    public PickupUnion(Item p_item)
    {
        type = PickupType.Item;

        item = p_item;
        keyItem = default;
        badge = default;
        ribbon = default;
    }
    public PickupUnion(KeyItem p_keyItem)
    {
        type = PickupType.KeyItem;

        item = default;
        keyItem = p_keyItem;
        badge = default;
        ribbon = default;
    }
    public PickupUnion(Badge p_badge)
    {
        type = PickupType.Badge;

        item = default;
        keyItem = default;
        badge = p_badge;
        ribbon = default;
    }
    public PickupUnion(Ribbon p_ribbon)
    {
        type = PickupType.Ribbon;

        item = default;
        keyItem = default;
        badge = default;
        ribbon = p_ribbon;
    }

    public PickupUnion Copy()
    {
        PickupUnion output = new PickupUnion();

        output.item = default;
        output.keyItem = default;
        output.badge = default;
        output.ribbon = default;

        output.type = type;
        switch (type)
        {
            case PickupType.Item:
                output.item = item;
                break;
            case PickupType.KeyItem:
                output.keyItem = keyItem;
                break;
            case PickupType.Badge:
                output.badge = badge;
                break;
            case PickupType.Ribbon:
                output.ribbon = ribbon;
                break;
        }

        return output;
    }

    public void Mutate()
    {
        if (type == PickupType.Badge)
        {
            if (badge.badgeCount != 0)
            {
                return;
            }
            badge.type = MainManager.Instance.MutateBadgeType(badge.type);
        }
        if (type == PickupType.Ribbon)
        {
            if (ribbon.ribbonCount != 0)
            {
                return;
            }
            ribbon.type = MainManager.Instance.MutateRibbonType(ribbon.type);
        }
    }
    public void Unmutate()
    {
        if (type == PickupType.Badge)
        {
            if (badge.badgeCount != 0)
            {
                return;
            }
            badge.type = MainManager.Instance.UnmutateBadgeType(badge.type);
        }
        if (type == PickupType.Ribbon)
        {
            if (ribbon.ribbonCount != 0)
            {
                return;
            }
            ribbon.type = MainManager.Instance.UnmutateRibbonType(ribbon.type);
        }
    }


    public static string GetName(PickupUnion pu)
    {
        switch (pu.type)
        {
            case PickupType.None:
                return "Nothing";
            case PickupType.Coin:
                return "Coin";
            case PickupType.SilverCoin:
                return "Silver Coin";
            case PickupType.GoldCoin:
                return "Gold Coin";
            case PickupType.Shard:
                return "Prisma Shard";
            case PickupType.Item:
                return Item.GetName(pu.item);
            case PickupType.KeyItem:
                return KeyItem.GetName(pu.keyItem);
            case PickupType.Badge:
                return Badge.GetName(pu.badge);
            case PickupType.Ribbon:
                return Ribbon.GetName(pu.ribbon);
        }

        return "Invalid name";
    }

    public static string GetDescription(PickupUnion pu)
    {
        switch (pu.type)
        {
            case PickupType.None:
                return "Wow, look! Nothing!";
            case PickupType.Coin:
                return "A bronze coin that is valuable.";
            case PickupType.SilverCoin:
                return "A silver coin that is somewhat valuable.";
            case PickupType.GoldCoin:
                return "A valuable, gold coin. You feel a faint energy coming from it.";
            case PickupType.Shard:
                return "A strange shard that originated from the Realm of Possibility.";
            case PickupType.Item:
                return Item.GetDescription(pu.item);
            case PickupType.KeyItem:
                return KeyItem.GetDescription(pu.keyItem);
            case PickupType.Badge:
                return Badge.GetDescription(pu.badge);
            case PickupType.Ribbon:
                return Ribbon.GetDescription(pu.ribbon);
        }

        return "No name found for this pickup";
    }

    //note: "" needs to be handled specially so you don't end up with a double space
    public static string GetArticle(PickupUnion pu)
    {
        switch (pu.type)
        {
            case PickupType.None:
                return "";
            case PickupType.Coin:
                return "a";
            case PickupType.SilverCoin:
                return "a";
            case PickupType.GoldCoin:
                return "a";
            case PickupType.Shard:
                return "a";
            case PickupType.Item:
                return Item.GetArticle(pu.item);
            case PickupType.KeyItem:
                return KeyItem.GetArticle(pu.keyItem);
            case PickupType.Badge:
                return "";
            case PickupType.Ribbon:
                return "the";   //ribbons are unique
        }

        return "";
    }

    public static float GetScale(PickupUnion pu)
    {
        switch (pu.type)
        {
            case PickupType.None:
                return 1;
            case PickupType.Coin:
                return 0.4f;
            case PickupType.SilverCoin:
                return 0.4f;
            case PickupType.GoldCoin:
                return 0.4f;
            case PickupType.Shard:
                return 0.75f;
            case PickupType.Item:
                return 1;
            case PickupType.KeyItem:
                return 1;
            case PickupType.Badge:
                return 1;
            case PickupType.Ribbon:
                return 1;
        }

        return 1;
    }
    public static string GetSpriteString(PickupUnion pu)
    {
        switch (pu.type)
        {
            case PickupType.None:
                return "";
            case PickupType.Coin:
                return "<commonsprite,coin>";
            case PickupType.SilverCoin:
                return "<commonsprite,silvercoin>";
            case PickupType.GoldCoin:
                return "<commonsprite,goldcoin>";
            case PickupType.Shard:
                return "<commonsprite,shard>";
            case PickupType.Item:
                return Item.GetSpriteString(pu.item);
                //return "<itemsprite," + item.type + ">";
            case PickupType.KeyItem:
                return KeyItem.GetSpriteString(pu.keyItem);
            case PickupType.Badge:
                return Badge.GetSpriteString(pu.badge);
            case PickupType.Ribbon:
                return Ribbon.GetSpriteString(pu.ribbon);
        }

        return "N/A";
    }

    public static Sprite GetSprite(PickupUnion pu)
    {
        switch (pu.type)
        {
            case PickupType.None:
                return null;
            case PickupType.Coin:
                return MainManager.Instance.commonSprites[(int)(Text_CommonSprite.SpriteType.Coin)];
            case PickupType.SilverCoin:
                return MainManager.Instance.commonSprites[(int)(Text_CommonSprite.SpriteType.SilverCoin)];
            case PickupType.GoldCoin:
                return MainManager.Instance.commonSprites[(int)(Text_CommonSprite.SpriteType.GoldCoin)];
            case PickupType.Shard:
                return MainManager.Instance.commonSprites[(int)(Text_CommonSprite.SpriteType.Shard)];
            case PickupType.Item:
                return GlobalItemScript.GetItemSprite(pu.item.type);
            case PickupType.KeyItem:
                return GlobalItemScript.GetKeyItemSprite(pu.keyItem.type);
            case PickupType.Badge:
                return GlobalBadgeScript.GetBadgeSprite(pu.badge.type);
            case PickupType.Ribbon:
                return GlobalRibbonScript.GetRibbonSprite(pu.ribbon.type);
        }

        return null;
    }
    public static Material GetSpriteMaterial(PickupUnion pu)
    {
        return MainManager.Instance.defaultSpriteMaterial;
    }

    public static PickupUnion Parse(string input)
    {
        var output = new PickupUnion();

        if (input == null || input.Length == 0)
        {
            output.type = PickupType.None;
            return output;
        }

        //check for the ones without data
        PickupType pt;
        if (Enum.TryParse(input, out pt))
        {
            output.type = pt;
            return output;
        }

        //all the others are more complex
        string[] inputSplit = input.Split("|");

        Item.ItemType it;
        KeyItem.KeyItemType kit;
        Badge.BadgeType bt;
        Ribbon.RibbonType rt;

        if (Enum.TryParse(inputSplit[0], out it))
        {
            output.type = PickupType.Item;
            output.item = Item.Parse(input);
            return output;
        }

        if (Enum.TryParse(inputSplit[0], out kit))
        {
            output.type = PickupType.KeyItem;
            output.keyItem = KeyItem.Parse(input);
            return output;
        }

        if (Enum.TryParse(inputSplit[0], out bt))
        {
            output.type = PickupType.Badge;
            output.badge = Badge.Parse(input);
            return output;
        }

        if (Enum.TryParse(inputSplit[0], out rt))
        {
            output.type = PickupType.Ribbon;
            output.ribbon = Ribbon.Parse(input);
            return output;
        }

        return output;
    }
    public override string ToString()
    {
        switch (type)
        {
            case PickupType.None:
                return "";
            case PickupType.Coin:
            case PickupType.SilverCoin:
            case PickupType.GoldCoin:
            case PickupType.Shard:
                return type.ToString();
            case PickupType.Item:
                return item.ToString();
            case PickupType.KeyItem:
                return keyItem.ToString();
            case PickupType.Badge:
                return badge.ToString();
            case PickupType.Ribbon:
                return ribbon.ToString();
        }

        return null;
    }
}
