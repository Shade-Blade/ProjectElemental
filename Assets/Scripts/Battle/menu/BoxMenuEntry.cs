using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class BoxMenuEntry //base class for battle menu stuff, this contains the bare minimum to display things
{
    //these can be null
    public string name;
    public string description;
    public string spriteString; //image string (use a <sprite> tag)
    public string rightText; //right justified text left of image (aka cost)

    public int maxLevel;    //1 is default (hides the level display, but 0, -1... should work similarly)
    public int level;       //use l/r to switch between different versions of the same thing

    public bool canUse;

    public bool hasBackground;
    public Color backgroundColor;


    public BoxMenuEntry()
    {
        //basically nothing
        name = null;
        description = null;
        spriteString = null;
        rightText = null;
        canUse = true;
        maxLevel = 1;
        level = 1;

        hasBackground = false;
        backgroundColor = Color.white;
    }

    public BoxMenuEntry(string p_name = null, string p_description = null, string p_spriteString = null, string p_rightText = null, bool p_canUse = true, int p_maxlevel = 1, bool p_hasBackground = false, Color p_backgroundColor = default)
    {
        name = p_name;
        description = p_description;
        spriteString = p_spriteString;
        rightText = p_rightText;
        canUse = p_canUse;
        maxLevel = p_maxlevel;
        level = 1;

        if (level > maxLevel)
        {
            level = maxLevel;
        }

        hasBackground = p_hasBackground;
        backgroundColor = p_backgroundColor;
    }

    /*
    public Sprite GetSprite()
    {
        return sprite;
    }

    public void SetSprite(Sprite s)
    {
        sprite = s;
    }
    */
}

//thing that journal and quest menus use
public class InformationMenuEntry : BoxMenuEntry
{
    //One of these should be non null
    //(if both are, use default sprite)
    public Sprite sprite;
    public string spritePath;

    //null -> empty string
    public string sideText;

    //if it is null or empty string, no info
    public string[] infoText;

    public InformationMenuEntry(Sprite sprite, string spritePath, string mainText, string sideText = null, string infoText = null, string description = null)
    {
        this.sprite = sprite;
        this.spritePath = spritePath;
        if (sideText == null)
        {
            sideText = "";
        }
        this.sideText = sideText;
        this.infoText = FormattedString.SplitByTag(infoText, false, TagEntry.TextTag.Next);
        this.name = mainText;

        //note: description is less necessary, might decide to hide the box later
        this.description = description;
    }
}

public class SettingsMenuEntry : BoxMenuEntry
{
    public string[] rightTextSet;
    public SettingsMenuEntry(string mainText, string[] rightTextSet, int startLevel, string description = null)
    {
        this.name = mainText;
        this.description = description;
        this.rightTextSet = rightTextSet;
        this.level = startLevel;
        this.maxLevel = rightTextSet.Length - 1;
        if (level > maxLevel)
        {
            level = maxLevel;
        }
        //Debug.Log("Last = " + rightTextSet[rightTextSet.Length - 1]);
        this.rightText = (rightTextSet == null || rightTextSet.Length == 0) ? "" : rightTextSet[level];
    }
}

public class FileMenuEntry
{
    //If they are all null, this is an empty file    
    public string name;             //name
    public string specialSprites;   //right side of name
    public string progressSprites;  //below name, right of playtime
    public string levelText;
    public string playtime;         //left of progress
    public string worldLocation;    //below playtime
    public string worldMap;         //same textbox as worldLocation

    //Special background colors
    public bool hasBackground;
    public Color backgroundColor;

    public FileMenuEntry()
    {
        //Nothing
    }

    public static FileMenuEntry ConstructFileMenuEntry(int fileIndex)
    {
        //Hand it off to MainManager I guess
        return MainManager.Instance.ConstructFileMenuEntry(fileIndex);
    }
}

public class MetaItemMenuEntry : BoxMenuEntry
{
    public MetaItemMove.Move move;

    public MetaItemMenuEntry(MetaItemMove.Move move, bool p_canUse = true)
    {
        this.move = move;
        name = MetaItemMove.GetName(move);
        description = MetaItemMove.GetDescription(move);
        spriteString = MetaItemMove.GetSpriteString(move);
        canUse = p_canUse;
        maxLevel = 1;
        level = 1;
    }
}

public class ItemMenuEntry : BoxMenuEntry
{
    public Item item;
    //public float power;

    public TargetArea target = new TargetArea(TargetArea.TargetAreaType.None);

    /*
    public enum StatDisplay
    {
        Sprite,
        Time,
        Money,
        Shards
    }
    */

    //Overworld only version (*note: not the overworld item usage menu, that is part of pause menu code)
    public ItemMenuEntry(Item p_item)//(Item p_item, StatDisplay display = StatDisplay.Sprite)
    {
        item = p_item;
        rightText = null;
        target = Item.GetTarget(item);
        //power = Item.GetPower(item.type);

        name = Item.GetSpriteString(item) + " " + Item.GetName(item);
        description = Item.GetDescription(item);

        //canUse = p_move.CanChoose(caller);
        //canUse = Item.GetProperty(item.type, Item.ItemProperty.NoOverworld) == null;

        maxLevel = 0;   //items don't really have levels(?)

        rightText = "";
    }

    //special tech version
    public ItemMenuEntry(Item p_item, string p_rightText)//(Item p_item, StatDisplay display = StatDisplay.Sprite)
    {
        item = p_item;
        rightText = p_rightText;
        target = Item.GetTarget(item);
        //power = Item.GetPower(item.type);

        name = Item.GetSpriteString(item) + " " + Item.GetName(item);
        description = Item.GetDescription(item);

        //canUse = p_move.CanChoose(caller);
        //canUse = Item.GetProperty(item.type, Item.ItemProperty.NoOverworld) == null;

        maxLevel = 0;   //items don't really have levels(?)
    }


    public ItemMenuEntry(Item p_item, ItemMove p_move)//(Item p_item, StatDisplay display = StatDisplay.Sprite)
    {
        item = p_item;
        rightText = null;
        target = Item.GetTarget(item);
        //power = Item.GetPower(item.type);

        name = Item.GetSpriteString(item) + " " + Item.GetName(item);
        description = Item.GetDescription(item);

        //canUse = p_move.CanChoose(caller);

        maxLevel = 0;   //items don't really have levels(?)

        rightText = "";
    }

    public ItemMenuEntry(PlayerEntity caller, Item p_item, ItemMove p_move)//(Item p_item, StatDisplay display = StatDisplay.Sprite)
    {
        item = p_item;
        rightText = null;
        target = Item.GetTarget(item);
        //power = Item.GetPower(item.type);

        name = Item.GetSpriteString(item) + " " + Item.GetName(item);
        description = Item.GetDescription(item);

        canUse = p_move.CanChoose(caller);

        maxLevel = 0;   //items don't really have levels(?)

        rightText = "";
    }

    public ItemMenuEntry(PlayerEntity caller, Item p_item, ItemMove p_move, Color color)//(Item p_item, StatDisplay display = StatDisplay.Sprite)
    {
        item = p_item;
        rightText = null;
        target = Item.GetTarget(item);
        //power = Item.GetPower(item.type);

        name = Item.GetSpriteString(item) + " " + Item.GetName(item);
        description = Item.GetDescription(item);

        canUse = p_move.CanChoose(caller);

        maxLevel = 0;   //items don't really have levels(?)

        rightText = "";

        hasBackground = true;
        backgroundColor = color;
    }
}

public class KeyItemMenuEntry : BoxMenuEntry
{
    public KeyItem kitem;

    //pause menu version
    public KeyItemMenuEntry(KeyItem p_keyitem)//(Item p_item, StatDisplay display = StatDisplay.Sprite)
    {
        kitem = p_keyitem;

        name = KeyItem.GetSpriteString(kitem) + " " + KeyItem.GetName(kitem);
        if (KeyItem.IsStackable(kitem) && kitem.bonusData > 0)
        {
            name += " x " + kitem.bonusData;
        }

        description = KeyItem.GetDescription(kitem);

        maxLevel = 0;   //items don't really have levels(?)

        rightText = "";

    }

    public KeyItemMenuEntry(KeyItem p_keyitem, string p_rightText)//(Item p_item, StatDisplay display = StatDisplay.Sprite)
    {
        kitem = p_keyitem;
        rightText = p_rightText;

        name = KeyItem.GetSpriteString(kitem) + " " + KeyItem.GetName(kitem);
        if (KeyItem.IsStackable(kitem) && kitem.bonusData > 0)
        {
            name += " x " + kitem.bonusData;
        }

        description = KeyItem.GetDescription(kitem);

        maxLevel = 0;   //items don't really have levels(?)

    }
}

//note: used in the pause menu and also Badge Swap's menu
public class BadgeMenuEntry : BoxMenuEntry
{
    public Badge b;
    public EquipType et;

    public enum EquipType
    {
        None,
        Wilex,
        Luna,
        Party
    }


    public BadgeMenuEntry(Badge p_b, EquipType p_et = EquipType.None, bool p_canUse = true)
    {
        b = p_b;
        et = p_et;
        rightText = "" + Badge.GetSPCost(b);
        name = Badge.GetSpriteString(b) + " " + Badge.GetName(b);
        description = Badge.GetDescription(b);
        canUse = p_canUse;
    }
}

//used in pause menu and Ribbon Swap's menu
public class RibbonMenuEntry : BoxMenuEntry
{
    public Ribbon r;
    public BadgeMenuEntry.EquipType et;

    /*
    public enum EquipType
    {
        None,
        Wilex,
        Luna,
        Party
    }
    */

    public RibbonMenuEntry(Ribbon p_r, BadgeMenuEntry.EquipType p_et = BadgeMenuEntry.EquipType.None, bool p_canUse = true, bool p_ribbonPower = false)
    {
        r = p_r;
        et = p_et;
        rightText = null;
        name = Ribbon.GetSpriteString(r) + " " + Ribbon.GetName(r); //r.type.ToString();
        description = Ribbon.GetDescription(r, p_ribbonPower); // r.type.ToString() + " description";
        canUse = p_canUse;
    }
}

public class MoveMenuEntry : BoxMenuEntry
{
    public int cost;
    //public bool isHP = false;         (MoveCurrency.HP)
    //public bool hasPrice = true;      rightText != null
    public TargetArea target = new TargetArea(TargetArea.TargetAreaType.None);
    //public PlayerEntity.PlayerMove move = PlayerEntity.PlayerMove.Null;

    public PlayerMove move;

    public BattleHelper.MoveCurrency currency;

    /*
    public enum StatDisplay
    {
        Cost,
        //Time
    }
    */

    //This will become the standard one
    //It will retrieve the correct data from other files to set everything up
    public MoveMenuEntry(PlayerMove move)
    {
        name = move.GetName();
        description = move.GetDescription();
        cost = move.cost;
        target = move.GetBaseTarget();
        currency = move.GetCurrency();

        rightText = cost > 0 ? cost.ToString() : null;
        spriteString = cost > 0 ? " " + BattleHelper.GetCurrencyIcon(currency) : null;
    }

    //calculates costs too
    public MoveMenuEntry(BattleEntity caller, PlayerMove move)
    {
        level = move.level;
        name = move.GetName();
        description = move.GetDescription(move.level);
        cost = move.GetCost(caller, move.level);
        target = move.GetTargetArea(caller, move.level);
        canUse = move.CanChoose(caller, move.level);
        maxLevel = move.GetMaxLevel(caller);
        currency = move.GetCurrency(caller);

        //redundant?
        this.move = move;

        rightText = cost > 0 ? cost.ToString() : null;
        spriteString = GetSpriteString(currency);
    }

    public string GetSpriteString(BattleHelper.MoveCurrency currency)
    {
        if (cost <= 0)
        {
            return null;
        }
        return BattleHelper.GetCurrencyIcon(currency);
    }

    public void RecalculateMove(BattleEntity caller)
    {
        description = move.GetDescription(level);
        canUse = move.CanChoose(caller, level);
        cost = move.GetCost(caller, level);
        target = move.GetTargetArea(caller, level);
        rightText = cost > 0 ? cost.ToString() : null;
        currency = move.GetCurrency(caller);
        spriteString = GetSpriteString(currency);
    }
}

public class TacticsMenuEntry : BoxMenuEntry
{
    public TargetArea target = new TargetArea(TargetArea.TargetAreaType.None);

    public BattleHelper.MoveCurrency currency;

    public int cost;

    public TacticsMenuEntry(BattleEntity caller, BattleAction b)
    {
        name = b.GetName();
        description = b.GetDescription();
        cost = b.GetCost(caller);
        target = b.GetBaseTarget();
        canUse = b.CanChoose(caller);
        currency = b.GetCurrency(caller);

        rightText = cost > 0 ? cost.ToString() : null;
        spriteString = GetSpriteString(currency);
    }

    public string GetSpriteString(BattleHelper.MoveCurrency currency)
    {
        if (cost <= 0)
        {
            return null;
        }
        return BattleHelper.GetCurrencyIcon(currency);
    }
}

public class CharacterMenuEntry : BoxMenuEntry
{
    public PlayerData.PlayerDataEntry entity;


    public CharacterMenuEntry(PlayerData.PlayerDataEntry p_entity)
    {
        entity = p_entity;
        name = entity.GetName();
        canUse = true;
        //Debug.Log(BattleControl.Instance.playerData);

        //add character descriptions later

        rightText = entity.hp + "/" + entity.maxHP;
        spriteString = " <hp>";
    }
}

//Mostly overworld stuff
//(but the code here isn't much different here)
public class PromptMenuEntry : BoxMenuEntry
{
    public string text;
    public string arg;
    
    public PromptMenuEntry(string p_text, string p_arg)
    {
        text = p_text;
        arg = p_arg;

        name = text;
        rightText = arg;
    }
}