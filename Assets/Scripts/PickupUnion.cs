using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
        Ribbon,
        Misc,
    }

    public PickupType type;

    public Item item;
    public KeyItem keyItem;
    public Badge badge;
    public Ribbon ribbon;
    public MainManager.MiscSprite misc;

    public PickupUnion()
    {
        type = PickupType.None;
        item = default;
        keyItem = default;
        badge = default;
        ribbon = default;
        misc = MainManager.MiscSprite.Default;
    }
    public PickupUnion(PickupType p_type)
    {
        type = p_type;

        item = default;
        keyItem = default;
        badge = default;
        ribbon = default;
        misc = MainManager.MiscSprite.Default;
    }
    public PickupUnion(Item p_item)
    {
        type = PickupType.Item;

        item = p_item;
        keyItem = default;
        badge = default;
        ribbon = default;
        misc = MainManager.MiscSprite.Default;
    }
    public PickupUnion(KeyItem p_keyItem)
    {
        type = PickupType.KeyItem;

        item = default;
        keyItem = p_keyItem;
        badge = default;
        ribbon = default;
        misc = MainManager.MiscSprite.Default;
    }
    public PickupUnion(Badge p_badge)
    {
        type = PickupType.Badge;

        item = default;
        keyItem = default;
        badge = p_badge;
        ribbon = default;
        misc = MainManager.MiscSprite.Default;
    }
    public PickupUnion(Ribbon p_ribbon)
    {
        type = PickupType.Ribbon;

        item = default;
        keyItem = default;
        badge = default;
        ribbon = p_ribbon;
        misc = MainManager.MiscSprite.Default;
    }
    public PickupUnion(MainManager.MiscSprite p_misc)
    {
        type = PickupType.Misc;

        item = default;
        keyItem = default;
        badge = default;
        ribbon = default;
        misc = p_misc;
    }

    public PickupUnion Copy()
    {
        PickupUnion output = new PickupUnion();

        output.item = default;
        output.keyItem = default;
        output.badge = default;
        output.ribbon = default;
        output.misc = MainManager.MiscSprite.Default;

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
            case PickupType.Misc:
                output.misc = misc;
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


    //Names of the misc items are hardcoded
    public static string GetName(MainManager.MiscSprite ms)
    {
        switch (ms)
        {
            case MainManager.MiscSprite.MysteryItem:
                return "Mystery Item";
            case MainManager.MiscSprite.MysteryRecipe:
                return "Mystery Recipe";
            case MainManager.MiscSprite.MysteryBadge:
                return "Mystery Badge";
            case MainManager.MiscSprite.MysteryRibbon:
                return "Mystery Ribbon";
            case MainManager.MiscSprite.NoItem:
            case MainManager.MiscSprite.NoRecipe:
            case MainManager.MiscSprite.NoBadge:
            case MainManager.MiscSprite.NoRibbon:
                return "Nothing";
            case MainManager.MiscSprite.AbilitySlash:
                return "Wilex Ability: Slash";
            case MainManager.MiscSprite.AbilitySlash2:
                return "Wilex Ability: Slash II";
            case MainManager.MiscSprite.AbilitySlash3:
                return "Wilex Ability: Slash III";
            case MainManager.MiscSprite.AbilityAetherize:
                return "Wilex Ability: Aetherize";
            case MainManager.MiscSprite.AbilityDoubleJump:
                return "Wilex Ability: Double Jump";
            case MainManager.MiscSprite.AbilitySuperJump:
                return "Wilex Ability: Super Jump";
            case MainManager.MiscSprite.AbilitySmash:
                return "Luna Ability: Smash";
            case MainManager.MiscSprite.AbilitySmash2:
                return "Luna Ability: Smash II";
            case MainManager.MiscSprite.AbilitySmash3:
                return "Luna Ability: Smash III";
            case MainManager.MiscSprite.AbilityIlluminate:
                return "Luna Ability: Illuminate";
            case MainManager.MiscSprite.AbilityDashHop:
                return "Luna Ability: Dash Hop";
            case MainManager.MiscSprite.AbilityDig:
                return "Luna Ability: Dig";
            case MainManager.MiscSprite.Health6:
                return "6 Health";
            case MainManager.MiscSprite.Health12:
                return "12 Health";
            case MainManager.MiscSprite.Health30:
                return "30 Health";
            case MainManager.MiscSprite.Health60:
                return "60 Health";
            case MainManager.MiscSprite.Energy6:
                return "6 Energy";
            case MainManager.MiscSprite.Energy12:
                return "12 Energy";
            case MainManager.MiscSprite.Energy30:
                return "30 Energy";
            case MainManager.MiscSprite.Energy60:
                return "60 Energy";
            case MainManager.MiscSprite.Soul6:
                return "6 Soul Energy";
            case MainManager.MiscSprite.Soul12:
                return "12 Soul Energy";
            case MainManager.MiscSprite.Soul30:
                return "30 Soul Energy";
            case MainManager.MiscSprite.Soul60:
                return "60 Soul Energy";
            case MainManager.MiscSprite.XP10:
                return "10 XP";
            case MainManager.MiscSprite.XP25:
                return "25 XP";
            case MainManager.MiscSprite.XP50:
                return "50 XP";
            case MainManager.MiscSprite.XP99:
                return "99 XP";
        }
        return "?";
    }
    public static string GetDescription(MainManager.MiscSprite ms)
    {
        switch (ms)
        {
            case MainManager.MiscSprite.MysteryItem:
                return "A random item. Check your inventory to see what this is.";
            case MainManager.MiscSprite.MysteryRecipe:
                return "A random recipe item. Check your inventory to see what this is.";
            case MainManager.MiscSprite.MysteryBadge:
                return "A random Badge. Check your inventory to see what this is.";
            case MainManager.MiscSprite.MysteryRibbon:
                return "A random Ribbon. Check your inventory to see what this is.";
            case MainManager.MiscSprite.NoItem:
                return "You can't get any Items right now.";
            case MainManager.MiscSprite.NoRecipe:
                return "You can't get any recipe items right now.";
            case MainManager.MiscSprite.NoBadge:
                return "You can't get any Badges right now.";
            case MainManager.MiscSprite.NoRibbon:
                return "You can't get any Ribbons right now.";
            case MainManager.MiscSprite.AbilitySlash:
                return "Slash with <buttonsprite,b> in the overworld. Base attack power: 3";
            case MainManager.MiscSprite.AbilitySlash2:
                return "Slash with <buttonsprite,b> in the overworld. Base attack power: 5";
            case MainManager.MiscSprite.AbilitySlash3:
                return "Slash with <buttonsprite,b> in the overworld. Base attack power: 7. This also gives you access to <misc,AbilityAetherize> Aetherize by holding <buttonsprite,b>.";
            case MainManager.MiscSprite.AbilityAetherize:
                return "Hold <buttonsprite,b> in the overworld.";
            case MainManager.MiscSprite.AbilityDoubleJump:
                return "Press <buttonsprite,a> in midair after jumping to jump again.";
            case MainManager.MiscSprite.AbilitySuperJump:
                return "Spin in place and press <buttonsprite,a> to do a Super Jump.";
            case MainManager.MiscSprite.AbilitySmash:
                return "Smash with <buttonsprite,b> in the overworld. Base attack power: 3";
            case MainManager.MiscSprite.AbilitySmash2:
                return "Smash with <buttonsprite,b> in the overworld. Base attack power: 5";
            case MainManager.MiscSprite.AbilitySmash3:
                return "Smash with <buttonsprite,b> in the overworld. Base attack power: 7. This also gives you access to <misc,AbilityIlluminate> Illuminate by holding <buttonsprite,b>.";
            case MainManager.MiscSprite.AbilityIlluminate:
                return "Hold <buttonsprite,b> in the overworld.";
            case MainManager.MiscSprite.AbilityDashHop:
                return "Tap a direction and press <buttonsprite,a> to do a Dash Hop. You can chain Dash Hops by holding a direction and pressing <buttonsprite,a> as you land from a previous hop.";
            case MainManager.MiscSprite.AbilityDig:
                return "Spin in place and hold <buttonsprite,a> to dig under the ground.";
            case MainManager.MiscSprite.Health6:
                return "The whole party gains 6 HP.";
            case MainManager.MiscSprite.Health12:
                return "The whole party gains 12 HP.";
            case MainManager.MiscSprite.Health30:
                return "The whole party gains 30 HP.";
            case MainManager.MiscSprite.Health60:
                return "The whole party gains 60 HP.";
            case MainManager.MiscSprite.Energy6:
                return "The whole party gains 6 EP.";
            case MainManager.MiscSprite.Energy12:
                return "The whole party gains 12 EP.";
            case MainManager.MiscSprite.Energy30:
                return "The whole party gains 30 EP.";
            case MainManager.MiscSprite.Energy60:
                return "The whole party gains 60 EP.";
            case MainManager.MiscSprite.Soul6:
                return "The whole party gains 6 SE.";
            case MainManager.MiscSprite.Soul12:
                return "The whole party gains 12 SE.";
            case MainManager.MiscSprite.Soul30:
                return "The whole party gains 30 SE.";
            case MainManager.MiscSprite.Soul60:
                return "The whole party gains 60 SE.";
            case MainManager.MiscSprite.XP10:
                return "The whole party gains 10 XP. This will not put you over 99 XP.";
            case MainManager.MiscSprite.XP25:
                return "The whole party gains 25 XP. This will not put you over 99 XP.";
            case MainManager.MiscSprite.XP50:
                return "The whole party gains 50 XP. This will not put you over 99 XP.";
            case MainManager.MiscSprite.XP99:
                return "The whole party gains 99 XP. This will not put you over 99 XP.";
        }
        return "?";
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
            case PickupType.Misc:
                return GetName(pu.misc);
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
            case PickupType.Misc:
                return GetDescription(pu.misc);
        }

        return "No description found for this pickup";
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
            case PickupType.Misc:
                return "<miscsprite," + pu.misc.ToString() + ">";
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
            case PickupType.Misc:
                if (pu.misc < 0)
                {
                    return null;
                }
                return MainManager.Instance.miscSprites[(int)(pu.misc - 1)];
        }

        return null;
    }
    public static Material GetSpriteMaterial(PickupUnion pu)
    {
        return MainManager.Instance.defaultSpriteMaterial;
    }

    //Costs in coins here
    public static int GetBaseCost(PickupUnion pu)
    {
        switch (pu.type)
        {
            case PickupType.None:
                return 0;
            case PickupType.Coin:
                return 1;
            case PickupType.SilverCoin:
                return 5;
            case PickupType.GoldCoin:
                return 25;
            case PickupType.Shard:
                return 75;
            case PickupType.Item:
                return Item.GetItemDataEntry(pu.item).sellPrice;
            case PickupType.KeyItem:
                return KeyItem.GetCost(pu.keyItem);
            case PickupType.Badge:
                if (pu.badge.type == Badge.BadgeType.SuperCurse)
                {
                    return 0;
                }
                if (pu.badge.type == Badge.BadgeType.UltraCurse)
                {
                    return 0;
                }
                if (pu.badge.type == Badge.BadgeType.MegaCurse)
                {
                    return 0;
                }

                //Made up formula
                return 20 * (1 + Badge.GetBadgeDataEntry(pu.badge).chapter);
            case PickupType.Ribbon:
                switch (pu.ribbon.type)
                {
                    case Ribbon.RibbonType.RainbowRibbon:
                        return 250;
                    case Ribbon.RibbonType.DiamondRibbon:   //probably not worth 999 in reality but big number is funny
                        return 999;
                }
                //most ribbons are pretty balanced
                return 60;
            case PickupType.Misc:
                //arbitrary numbers
                return GetMiscCost(pu.misc);
        }

        return 0;
    }

    public static int GetMiscCost(MainManager.MiscSprite ms)
    {
        switch (ms)
        {
            case MainManager.MiscSprite.MysteryItem:
                return 30;
            case MainManager.MiscSprite.MysteryRecipe:
                return 50;
            case MainManager.MiscSprite.MysteryBadge:
                return 100;
            case MainManager.MiscSprite.MysteryRibbon:
                return 75;
            case MainManager.MiscSprite.NoItem:
            case MainManager.MiscSprite.NoRecipe:
            case MainManager.MiscSprite.NoBadge:
            case MainManager.MiscSprite.NoRibbon:
                return 0;
            case MainManager.MiscSprite.AbilitySlash:
                return 25;
            case MainManager.MiscSprite.AbilitySlash2:
                return 75;
            case MainManager.MiscSprite.AbilitySlash3:
                return 150;
            case MainManager.MiscSprite.AbilityAetherize:
                return 150;
            case MainManager.MiscSprite.AbilityDoubleJump:
                return 50;
            case MainManager.MiscSprite.AbilitySuperJump:
                return 100;
            case MainManager.MiscSprite.AbilitySmash:
                return 25;
            case MainManager.MiscSprite.AbilitySmash2:
                return 75;
            case MainManager.MiscSprite.AbilitySmash3:
                return 150;
            case MainManager.MiscSprite.AbilityIlluminate:
                return 150;
            case MainManager.MiscSprite.AbilityDashHop:
                return 25;
            case MainManager.MiscSprite.AbilityDig:
                return 125;
            case MainManager.MiscSprite.Health6:
                return 6;
            case MainManager.MiscSprite.Health12:
                return 12;
            case MainManager.MiscSprite.Health30:
                return 30;
            case MainManager.MiscSprite.Health60:
                return 60;
            case MainManager.MiscSprite.Energy6:
                return 6;
            case MainManager.MiscSprite.Energy12:
                return 12;
            case MainManager.MiscSprite.Energy30:
                return 30;
            case MainManager.MiscSprite.Energy60:
                return 60;
            case MainManager.MiscSprite.Soul6:
                return 6;
            case MainManager.MiscSprite.Soul12:
                return 12;
            case MainManager.MiscSprite.Soul30:
                return 30;
            case MainManager.MiscSprite.Soul60:
                return 60;
            case MainManager.MiscSprite.XP10:
                return 30;
            case MainManager.MiscSprite.XP25:
                return 75;
            case MainManager.MiscSprite.XP50:
                return 150;
            case MainManager.MiscSprite.XP99:
                return 299;
        }

        return 0;
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
        MainManager.MiscSprite ms;

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

        if (Enum.TryParse(inputSplit[0], out ms))
        {
            output.type = PickupType.Misc;
            output.misc = ms;
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
            case PickupType.Misc:
                return misc.ToString();
        }

        return null;
    }
}
