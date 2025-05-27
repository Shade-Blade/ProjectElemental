using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Item;
using static KeyItem;
using static UnityEngine.UI.Image;

//contains GlobalItemScript, Item, ItemMove classes
//Note that ItemMove handles item removal from inventory in battle

public class GlobalItemScript : MonoBehaviour
{
    private static GlobalItemScript instance;
    public static GlobalItemScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GlobalItemScript>(); //this should work
                if (instance == null)
                {
                    GameObject b = new GameObject("GlobalItemScript");
                    GlobalItemScript c = b.AddComponent<GlobalItemScript>();
                    instance = c;
                    instance.transform.parent = MainManager.Instance.transform;
                }
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    public ItemDataEntry[] itemDataTable;
    public KeyItemDataEntry[] keyItemDataTable;
    string[][] itemText = null;
    string[][] keyItemText = null;

    //public KeyItemDataEntry[] keyItemDataTable;

    public RecipeDataEntry[] recipeDataTable;

    //X = first ingredient, Y = second
    //0 = the none row
    public RecipeDataEntry?[][] bigRecipeDataTable;

    public ItemType[] recipeOrder;

    public void Load()
    {
        
    }

    public void LoadItemDataTable()
    {
        string[][] itemDataRaw = MainManager.CSVParse(Resources.Load<TextAsset>("Data/ItemData").text);
        itemDataTable = new ItemDataEntry[itemDataRaw.Length - 1];
        for (int i = 1; i < itemDataTable.Length; i++)
        {
            ItemDataEntry? temp = ItemDataEntry.ParseItemDataEntry(itemDataRaw[i], (Item.ItemType)(i));
            itemDataTable[i - 1] = temp.GetValueOrDefault();
        }
        //How to use item table:
        //  Index it with (int)itemType - 1
        //(since 0 is not a real item)
    }
    public void UnloadItemDataTable()   //probably do this in overworld?
    {
        itemDataTable = null;
    }

    public void LoadItemText()
    {
        itemText = MainManager.GetAllTextFromFile("DialogueText/ItemText");    //MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/ItemText").text);
    }
    public void UnloadItemText()    //probably do this in overworld?
    {
        itemText = null;
    }

    public void LoadKeyItemText()
    {
        keyItemText = MainManager.GetAllTextFromFile("DialogueText/KeyItemText");
    }
    public void UnloadKeyItemText()    //probably do this in overworld?
    {
        itemText = null;
    }

    public void LoadKeyItemTable()
    {
        string[][] itemDataRaw = MainManager.CSVParse(Resources.Load<TextAsset>("Data/KeyItemData").text);
        keyItemDataTable = new KeyItemDataEntry[itemDataRaw.Length - 1];
        for (int i = 1; i < keyItemDataTable.Length; i++)
        {
            KeyItemDataEntry? temp = KeyItemDataEntry.ParseKeyItemDataEntry(itemDataRaw[i], (KeyItem.KeyItemType)(i));
            keyItemDataTable[i - 1] = temp.GetValueOrDefault();
        }
        //How to use item table:
        //  Index it with (int)itemType - 1
        //(since 0 is not a real item)
    }

    public void LoadRecipeDataTable()
    {
        string[][] recipeDataRaw = MainManager.CSVParse(Resources.Load<TextAsset>("Data/RecipeData").text);
        string[][] recipeOrderRaw = MainManager.CSVParse(Resources.Load<TextAsset>("Data/RecipeOrder").text);
        recipeDataTable = new RecipeDataEntry[recipeDataRaw.Length - 1];
        recipeOrder = new ItemType[recipeOrderRaw.Length - 2];
        for (int i = 1; i < recipeDataRaw.Length; i++)
        {
            RecipeDataEntry? temp = RecipeDataEntry.Parse(recipeDataRaw[i]);
            recipeDataTable[i - 1] = temp.GetValueOrDefault();
        }
        for (int i = 1; i < recipeOrder.Length + 1; i++)
        {
            ItemType temp;
            Enum.TryParse(recipeOrderRaw[i][0], out temp);
            if (temp == ItemType.None)
            {
                Debug.LogWarning("[Recipe Order] Can't parse " + recipeOrderRaw[i][0]);
            }
            recipeOrder[i - 1] = temp;
        }
        //How to use recipe table:
        //Iterate over everything while checking (entry).ingredientA and (entry).ingredientB    (if needed)
        //can probably be optimized by checking only the chapter of the later item (since that is where in the list the result has to be)
        //If you want to find which recipe makes a certain item then iterate while finding result
    }
    public void LoadBigRecipeTable()
    {
        if (recipeDataTable == null)
        {
            LoadRecipeDataTable();
        }

        //Better table (but it is pretty big)
        //Memory usage might get a bit bad
        //But you can't beat that O(1) check to see if a recipe is valid
        bigRecipeDataTable = new RecipeDataEntry?[(int)ItemType.EndOfTable][];
        for (int i = 0; i < bigRecipeDataTable.Length; i++)
        {
            bigRecipeDataTable[i] = new RecipeDataEntry?[(int)ItemType.EndOfTable];
        }
        //go back
        for (int i = 0; i < recipeDataTable.Length; i++)
        {
            ItemType a = ItemType.None;
            ItemType b = ItemType.None;

            a = recipeDataTable[i].ingredientA;
            b = recipeDataTable[i].ingredientB;

            bigRecipeDataTable[(int)a][(int)b] = recipeDataTable[i];
            bigRecipeDataTable[(int)b][(int)a] = recipeDataTable[i];
        }
    }
    public void UnloadRecipeDataTable()
    {
        recipeDataTable = null;
        recipeOrder = null;
    }
    public void UnloadBigRecipeDataTable()
    {
        bigRecipeDataTable = null;
    }

    public int ItemToRecipeIndex(ItemType it)
    {
        int output = -1;
        if (recipeOrder == null)
        {
            LoadRecipeDataTable();
        }
        for (int i = 0; i < recipeOrder.Length; i++)
        {
            if (recipeOrder[i] == it)
            {
                output = i;
                return output;
            }
        }
        return output;
    }
    public MainManager.GlobalFlag RecipeToFlag(ItemType it)
    {
        string newName = "GF_Recipe_" + it;

        MainManager.GlobalFlag output = (MainManager.GlobalFlag.GF_None);
        Enum.TryParse(newName, out output);

        return output;
    }
    public List<RecipeDataEntry> GetMissingRecipes()        //Can't use big table
    {
        if (recipeOrder == null)
        {
            LoadRecipeDataTable();
        }

        List<RecipeDataEntry> output = new List<RecipeDataEntry>();

        MainManager.GlobalFlag flag = MainManager.GlobalFlag.GF_Recipe_Mistake;

        for (int i = 0; i < recipeOrder.Length; i++)
        {
            flag = (MainManager.GlobalFlag)((int)MainManager.GlobalFlag.GF_Recipe_Mistake + i);
            bool check = MainManager.Instance.GetGlobalFlag(flag);

            if (!check)
            {
                output.Add(GetRecipeDataFromResult(recipeOrder[i]).Value);
            }
        }

        return output;
    }

    public string[][] GetTextFile()
    {
        if (itemText == null)
        {
            LoadItemText();
        }

        return itemText;
    }

    public string GetItemName(ItemType i)
    {
        if (itemText == null)
        {
            LoadItemText();
        }

        return itemText[(int)(i)][1];
    }

    public string GetItemDescription(ItemType i)
    {
        if (itemText == null)
        {
            LoadItemText();
        }

        string output = "";

        int length = itemText[(int)(i)].Length;
        output += "<descriptionfluffcolor>" + FormattedString.InsertEscapeSequences(FormattedString.ReplaceTextFileTags(itemText[(int)(i)][3])) + "</descriptionfluffcolor>";
        output += " <color,#000000>" + FormattedString.InsertEscapeSequences(FormattedString.ReplaceTextFileTags(itemText[(int)(i)][4])) + "</color><line>";
        if (length > 4 && itemText[(int)(i)][5].Length > 0)
        {
            output += "<descriptionnoticecolor>(" + FormattedString.InsertEscapeSequences(FormattedString.ReplaceTextFileTags(itemText[(int)(i)][5])) + ")</descriptionnoticecolor> ";
        }
        if (length > 5 && itemText[(int)(i)][6].Length > 0)
        {
            output += "<descriptionwarncolor>(" + FormattedString.InsertEscapeSequences(FormattedString.ReplaceTextFileTags(itemText[(int)(i)][6])) + ")</descriptionwarncolor>";
        }
        //Debug.Log(output);
        //Debug.Log((int)itemText[(int)(i)][5][0]);

        return output;
    }
    public string GetItemArticle(ItemType i)
    {
        if (itemText == null)
        {
            LoadItemText();
        }
        return itemText[(int)(i)][2];
    }

    public string GetItemText(ItemType i, int index)
    {
        if (itemText == null)
        {
            LoadItemText();
        }
        return itemText[(int)i][index];
    }

    public string GetRecipeText(ItemType it)
    {
        return GetRecipeText(GetRecipeDataFromResult(it).Value);
    }
    public string GetRecipeText(RecipeDataEntry rde)
    {
        //Special cases: mistake tier items need special text

        //To do later: Make this read from a text file

        if (rde.result == ItemType.Mistake)
        {
            return "<descriptionwarncolor>Created by an invalid combination of items.</descriptionwarncolor>";
        }
        if (rde.result == ItemType.BigMistake)
        {
            return "<descriptionwarncolor>Created by an invalid combination involving a Mistake.</descriptionwarncolor>";
        }
        if (rde.result == ItemType.CursedStew)
        {
            return "<descriptionwarncolor>Created by an invalid combination involving a rare item.</descriptionwarncolor>";
        }

        if (rde.quality == ItemQuality.Mistake)
        {
            throw new ArgumentException("Mistake tier item without special handling");
        }

        string output = "";
        if (rde.ingredientB == ItemType.None)
        {
            //1 item recipe
            output = "Created by cooking " + GetName(rde.ingredientA) + " alone.";
        } else
        {
            //2 item recipe
            output = "Created by cooking " + GetName(rde.ingredientA) + " and " + GetName(rde.ingredientB) + " together.";
        }

        if (rde.quality == ItemQuality.SpecialtyRecipe)
        {
            output += "\n<descriptionnoticecolor>(Specialty Recipe: Only a specific cook can make this.)</descriptionnoticecolor>";
        }
        if (rde.quality == ItemQuality.SupremeRecipe)
        {
            output += "\n<descriptionnoticecolor>(Supreme Recipe: Can only be created by one of the cooks that made the ingredients for this recipe.)</descriptionnoticecolor>";
        }

        return output;
    }
    public string GetItemModifierPrefix(ItemModifier im)
    {
        switch (im)
        {
            case ItemModifier.None:
                return "";
            case ItemModifier.Echo:
                return "<color,#00f000>(E)</color>";
            case ItemModifier.Echoed:
                return "<color,#90f090>(e)</color>";
            case ItemModifier.Glistening:
                return "<color,#f0c000>(G)</color>";
            case ItemModifier.Void:
                return "<color,#7000f0>(V)</color>";
            case ItemModifier.Spread:
                return "<color,#0090f0>(S)</color>";
            case ItemModifier.Focus:
                return "<color,#f00000>(F)</color>";
            case ItemModifier.Quick:
                return "<color,#909090>(Q)</color>";
        }

        return "";
    }
    public string GetItemModifierDescription(ItemModifier im)
    {
        switch (im)
        {
            case ItemModifier.None:
                return "";
            case ItemModifier.Echo:
                return "Echo: Item is 0.8x as powerful, but becomes Echoed after use.";
            case ItemModifier.Echoed:
                return "Echoed: Item is 0.4x as powerful. (Produced by an Echo item)";
            case ItemModifier.Glistening:
                return "Glistening: Item is 1.5x as powerful";
            case ItemModifier.Void:
                return "Void: Does not take up space in your inventory";
            case ItemModifier.Spread:   //note: don't apply to already multi target because nothing happens
                return "Spread: Item becomes multi target.";
            case ItemModifier.Focus:    //note: don't apply to single target because it is a benefit with no drawback
                return "Focus: Item becomes single target and 2x power.";
            case ItemModifier.Quick:
                return "Quick: Does not consume your action when used (Can't use Items next action)";
        }

        return "";
    }
    public static float GetItemModifierSellMultiplier(ItemModifier im)
    {
        switch (im)
        {
            case ItemModifier.None:
                return 1;
            case ItemModifier.Echo:
            case ItemModifier.Glistening:
            case ItemModifier.Spread:
            case ItemModifier.Focus:
            case ItemModifier.Quick:
                return 1.5f;
            case ItemModifier.Echoed:
                return 0.5f;
            case ItemModifier.Void:
                return 2f;
        }

        return 1;
    }
    public static ItemModifier GetModifierFromRecipe(ItemModifier a, ItemModifier b)
    {
        //no modifier and Echoed have lower priority
        if (a == ItemModifier.None)
        {
            return b;
        }
        if (b == ItemModifier.None)
        {
            return a;
        }
        if (a == ItemModifier.Echoed)
        {
            return b;
        }
        if (b == ItemModifier.Echoed)
        {
            return a;
        }

        return (a > b) ? a : b;
    }

    public static Sprite GetItemSpriteFromText(string item)
    {
        ItemType itemType;

        Enum.TryParse(item, true, out itemType);

        return GetItemSprite(itemType);
    }

    public static Sprite GetItemSprite(ItemType itemType)
    {
        if ((int)itemType <= 0)
        {
            Debug.LogWarning("Accessing item sprite too low " + (int)itemType);
            return MainManager.Instance.itemSprites[MainManager.Instance.itemSprites.Length - 1];
        }
        if ((int)itemType > (int)Item.ItemType.EndOfTable)
        {
            Debug.LogWarning("Accessing item sprite too high " + (int)itemType);
            return MainManager.Instance.itemSprites[MainManager.Instance.itemSprites.Length - 1];
        }
        return MainManager.Instance.itemSprites[(int)(itemType) - 1];
    }

    //These two are separate as the materials are incompatible
    public static Material GetItemModifierMaterial(ItemModifier im)
    {
        switch (im)
        {
            case ItemModifier.Focus:
                return Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteItemModifier_Focus");
            case ItemModifier.Spread:
                return Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteItemModifier_Spread");
            case ItemModifier.Echo:
                return Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteItemModifier_Echo");
            case ItemModifier.Echoed:
                return Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteItemModifier_Echoed");
            case ItemModifier.Glistening:
                return Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteItemModifier_Glistening");
            case ItemModifier.Void:
                return Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteItemModifier_Void");
            case ItemModifier.Quick:
                return Resources.Load<Material>("Sprites/Materials/Special/ProperSpriteItemModifier_Quick");
        }

        return MainManager.Instance.defaultSpriteMaterial;
    }
    public static Material GetItemModifierGUIMaterial(ItemModifier im)
    {
        switch (im)
        {
            case ItemModifier.Focus:
                return Resources.Load<Material>("Sprites/Materials/Canvas/Canvas_ItemModifier_Focus");
            case ItemModifier.Spread:
                return Resources.Load<Material>("Sprites/Materials/Canvas/Canvas_ItemModifier_Spread");
            case ItemModifier.Echo:
                return Resources.Load<Material>("Sprites/Materials/Canvas/Canvas_ItemModifier_Echo");
            case ItemModifier.Echoed:
                return Resources.Load<Material>("Sprites/Materials/Canvas/Canvas_ItemModifier_Echoed");
            case ItemModifier.Glistening:
                return Resources.Load<Material>("Sprites/Materials/Canvas/Canvas_ItemModifier_Glistening");
            case ItemModifier.Void:
                return Resources.Load<Material>("Sprites/Materials/Canvas/Canvas_ItemModifier_Void");
            case ItemModifier.Quick:
                return Resources.Load<Material>("Sprites/Materials/Canvas/Canvas_ItemModifier_Quick");
        }

        return MainManager.Instance.defaultGUISpriteMaterial;
    }
    public static Material GetItemModifierMaterial(string im)
    {
        ItemModifier itemModifier;

        Enum.TryParse(im, true, out itemModifier);
        Debug.Log(im + " " + itemModifier);

        return GetItemModifierMaterial(itemModifier);
    }
    public static Material GetItemModifierGUIMaterial(string im)
    {
        ItemModifier itemModifier;

        Enum.TryParse(im, true, out itemModifier);

        return GetItemModifierGUIMaterial(itemModifier);
    }

    public static ItemModifier GetRandomModifier(Item.ItemType i)
    {
        ItemDataEntry ide = GetItemDataEntry(i);

        bool dual = false;
        if (Item.GetProperty(ide, Item.ItemProperty.TargetAll) != null)
        {
            dual = true;
        }

        List<Item.ItemModifier> modifierPool = new List<Item.ItemModifier>
        {
            ItemModifier.Echo,
            ItemModifier.Glistening,
            ItemModifier.Quick,
            ItemModifier.Void
        };

        if (dual)
        {
            modifierPool.Add(ItemModifier.Focus);
        } else
        {
            modifierPool.Add(ItemModifier.Spread);
        }

        return RandomTable<Item.ItemModifier>.ChooseRandom(modifierPool);
    }
    public static bool ItemMultiTarget(Item.ItemType i)
    {
        ItemDataEntry ide = GetItemDataEntry(i);

        bool dual = false;
        if (Item.GetProperty(ide, Item.ItemProperty.TargetAll) != null)
        {
            dual = true;
        }

        return dual;
    }

    public string GetKeyItemName(KeyItemType i)
    {
        if (keyItemText == null)
        {
            LoadKeyItemText();
        }

        return keyItemText[(int)(i)][1];
    }

    public string GetKeyItemDescription(KeyItemType i)
    {
        if (keyItemText == null)
        {
            LoadKeyItemText();
        }

        string output = "";

        int length = keyItemText[(int)(i)].Length;
        output += "<descriptionfluffcolor>" + FormattedString.InsertEscapeSequences(keyItemText[(int)(i)][3]) + "</descriptionfluffcolor>";
        output += " <color,#000000>" + FormattedString.InsertEscapeSequences(keyItemText[(int)(i)][4]) + "</color><line>";
        if (length > 4 && keyItemText[(int)(i)][5].Length > 0)
        {
            output += "<descriptionnoticecolor>(" + FormattedString.InsertEscapeSequences(keyItemText[(int)(i)][5]) + ")</descriptionnoticecolor> ";
        }
        if (length > 5 && keyItemText[(int)(i)][6].Length > 0)
        {
            output += "<descriptionwarncolor>(" + FormattedString.InsertEscapeSequences(keyItemText[(int)(i)][6]) + ")</descriptionwarncolor>";
        }
        //Debug.Log(output);
        //Debug.Log((int)itemText[(int)(i)][5][0]);

        return output;
    }
    public string GetKeyItemArticle(KeyItemType i)
    {
        if (keyItemText == null)
        {
            LoadKeyItemText();
        }
        return keyItemText[(int)(i)][2];
    }
    public string GetKeyItemText(KeyItemType i, int index)
    {
        if (keyItemText == null)
        {
            LoadKeyItemText();
        }
        return keyItemText[(int)i][index];
    }
    public string[][] GetKeyItemText()
    {
        if (keyItemText == null)
        {
            LoadKeyItemText();
        }
        return keyItemText;
    }

    public static Sprite GetKeyItemSpriteFromText(string keyItem)
    {
        KeyItemType keyItemType;

        Enum.TryParse(keyItem, out keyItemType);

        return GetKeyItemSprite(keyItemType);
    }

    public static Sprite GetKeyItemSprite(KeyItemType itemType)
    {
        return MainManager.Instance.keyItemSprites[(int)(itemType) - 1];
    }

    //returns Mistake, BigMistake, CursedStew for invalid recipes
    public RecipeDataEntry GetRecipeDataFromIngredients(ItemType ingredientA, ItemType ingredientB = ItemType.None)
    {
        if (recipeDataTable == null)
        {
            LoadRecipeDataTable();
        }
        RecipeDataEntry output = GetMistakeType(ingredientA, ingredientB);
        foreach (RecipeDataEntry r in recipeDataTable)
        {
            bool oneIngredient = r.ingredientB == ItemType.None;

            if (oneIngredient)
            {
                if (ingredientB != ItemType.None)
                {
                    continue;
                }

                if (ingredientA == r.ingredientA)
                {
                    output = r;
                    break;
                }
            } else
            {
                if (ingredientB == ItemType.None)
                {
                    continue;
                }

                if (ingredientA == r.ingredientA && ingredientB == r.ingredientB)
                {
                    output = r;
                    break;
                }
                if (ingredientB == r.ingredientA && ingredientA == r.ingredientB)
                {
                    output = r;
                    break;
                }
            }
        }

        return output;  
    }
    public RecipeDataEntry GetMistakeType(ItemType ingredientA, ItemType ingredientB = ItemType.None)
    {
        //Big mistake condition: recipe contains Mistake or BigMistake
        bool bigMistake = (ingredientA == ItemType.Mistake) || (ingredientA == ItemType.BigMistake) || (ingredientB == ItemType.Mistake) || (ingredientB == ItemType.BigMistake);

        //Chapter 9 items (Note that Cursed Stew is a chapter 9 item so cursed stew + any = cursed stew)
        bool cursedStew = ((int)(ingredientA) >= (int)ItemType.AetherCarrot) || ((int)(ingredientB) >= (int)ItemType.AetherCarrot);

        if (cursedStew)
        {
            return GetRecipeDataFromResult(ItemType.CursedStew).GetValueOrDefault();
        }

        if (bigMistake)
        {
            return GetRecipeDataFromResult(ItemType.BigMistake).GetValueOrDefault();
        }

        return GetRecipeDataFromResult(ItemType.Mistake).GetValueOrDefault();
    }

    public RecipeDataEntry? GetRecipeDataFromResult(ItemType result)
    {
        if (recipeDataTable == null)
        {
            LoadRecipeDataTable();
        }
        RecipeDataEntry? output = null;
        foreach (RecipeDataEntry r in recipeDataTable)
        {
            if (r.result == result)
            {
                return r;
            }
        }

        //even the mistakes have entries (even if they are nonfunctional)
        Debug.LogWarning(result + " has no recipe entry");

        return output;
    }
    //unfortunately big table doesn't help me find the recipe that makes result

    //use this if you really need the O(1) recipe check complexity
    public RecipeDataEntry GetRecipeDataFromIngredientsBigTable(ItemType ingredientA, ItemType ingredientB = ItemType.None)
    {
        if (bigRecipeDataTable == null)
        {
            LoadBigRecipeTable();
        }
        RecipeDataEntry output = GetMistakeType(ingredientA, ingredientB);

        RecipeDataEntry? test = bigRecipeDataTable[(int)ingredientA][(int)ingredientB];

        if (test == null)
        {
            return output;
        }

        return test.Value;
    }

    //makes the get item move method more compact
    public T GetOrAddComponent<T>() where T : Component
    {
        if (gameObject.GetComponent<T>())
            return gameObject.GetComponent<T>();
        else
            return gameObject.AddComponent<T>() as T;
    }

    //problem: need to use my thing that reduces how many scripts I need
    public T GetOrAddItemMove<T>(Item type) where T : ItemMove
    {
        T[] list = gameObject.GetComponents<T>();
        foreach (T t in list)
        {
            if (t.Equals(type))
            {
                return t;
            }
        }

        T output = gameObject.AddComponent<T>();
        output.SetItem(type);
        return output;
    }

    //need to clean things up some times
    //note that item moves are not needed that often so deleting them at start of your turn is fine
    //Note that end of turn is unsafe due to the existence of auto activate items
    //(and if you need them again you can just remake them)
    public void ClearItemMoves()
    {
        ItemMove[] i = gameObject.GetComponents<ItemMove>();

        foreach (ItemMove j in i)
        {
            Destroy(j);
        }

        MetaItemMove[] k = gameObject.GetComponents<MetaItemMove>();
        foreach (MetaItemMove l in k)
        {
            Destroy(l);
        }
    }
}

//note: outside of battle, this should give you exactly enough information to make the item work without supplemental code
//Although, a few items have special one off properties that don't make sense to put into the data (so they are hardcoded)
//Weird flag designates these items
public struct ItemDataEntry
{
    //one line of the item data

    public int hp;
    public int ep; //(if dual then it only applies to user)
    public int se; //(if dual then it only applies to user)
    public int stamina;
    public int sellPrice;
    public  Effect[] effects;   //effects to apply to target(s)
    //bool cure;          //status cure?
    //bool dual;          //more like an "all possible targets" thing (in the case of attack items)
    //bool healovertime;  //(by convention, heals for 3 turns, out of battle it heals immediately for all 3 turns worth)
    public OverhealPayEffect overheal;    //how is overheal handled? (Reused for the "pay for effect items")
    public int overhealDivisor;        //the divisor (overheal is usually something like X where X = overheal / some number rounded up)
    public ItemPropertyBlock[] properties;
    public bool weird;
    //special weird effects will be hardcoded (*no real reason not to)
    //damage items use the damage stuff (but reuses a bit of the above stuff for special stuff)
    public ItemUseAnim useAnim;
    public ItemQuality itemQuality;
    public bool isAttackItem;
    public BattleHelper.DamageType damageType; //note: the table can have | entries (they get ORed)
    public BattleHelper.DamageProperties damageProperties;   //note: the table can have | entries (they get ORed)
    public int baseDamage;
    public int chapter;
    public bool isRecipe;

    public static ItemDataEntry? ParseItemDataEntry(string[] entry, ItemType i = (ItemType)(-1))
    {
        /*
        string debug = "";
        for (int h = 0; h < entry.Length; h++)
        {
            debug += entry[h] + "\n";
        }
        Debug.Log(debug);
        */

        //note: itemtype is used for validation (you get the string out of the big csv file and use it to double check)
        ItemDataEntry ide = new ItemDataEntry();

        //validation
        string check = entry[0];
        if (!check.Equals(i.ToString()))
        {
            Debug.LogWarning("[Item Parsing] Data table has a mismatch: " + i + " is reading from " + entry[0]);
        }

        //hp
        int temp = 0;
        if (entry.Length > 1)
        {
            int.TryParse(entry[1], out temp);
            ide.hp = temp;
        }

        //ep
        temp = 0;
        if (entry.Length > 2)
        {
            int.TryParse(entry[2], out temp);
            ide.ep = temp;
        }

        //se
        temp = 0;
        if (entry.Length > 3)
        {
            int.TryParse(entry[3], out temp);
            ide.se = temp;
        }

        //stamina
        temp = 0;
        if (entry.Length > 4)
        {
            int.TryParse(entry[4], out temp);
            ide.stamina = temp;
        }

        //sellprice
        temp = 0;
        if (entry.Length > 5)
        {
            int.TryParse(entry[5], out temp);
            ide.sellPrice = temp;
        }

        //effects
        Effect[] tempEffects = null;
        if (entry.Length > 6)
        {
            //Format:
            //effect:pot/dur|effect2:pot/dur

            //try to parse effects
            string[] tempEffectStrings = entry[6].Split("|");
            tempEffects = new Effect[tempEffectStrings.Length];

            if (entry[6].Length == 0)
            {
                tempEffects = new Effect[0];
            }

            string tempE_T;
            string tempE_PD;
            string tempE_P;
            string tempE_D;
            Effect.EffectType temp_E2_T = (Effect.EffectType)(-1);
            sbyte temp_E2_P = 0;
            sbyte temp_E2_D = 0;

            if (entry[6].Length > 0)
            {
                for (int j = 0; j < tempEffectStrings.Length; j++)
                {
                    tempE_T = tempEffectStrings[j].Split(":")[0];

                    if (tempEffectStrings[j].Split(":").Length < 2)
                    {
                        Debug.LogError("Malformed effect in " + i + " (line in question: " + entry[6] + ")");
                        continue;
                    }
                    if (tempE_T.Length == 0)
                    {
                        Debug.LogError("Malformed effect in " + i + " (line in question: " + entry[6] + ")");
                        continue;
                    }

                    tempE_PD = tempEffectStrings[j].Split(":")[1];
                    tempE_P = tempE_PD.Split("/")[0];
                    tempE_D = tempE_PD.Split("/")[1];

                    if (Enum.TryParse(tempE_T, true, out temp_E2_T))
                    {
                    }
                    else
                    {
                        Debug.LogError("[Item Parsing] " + i + " Can't parse effect type \"" + tempE_T + "\"");
                    }

                    if (tempE_P.Equals("X"))
                    {
                        temp_E2_P = Effect.INFINITE_POTENCY;
                    }
                    else
                    {
                        sbyte.TryParse(tempE_P, out temp_E2_P);
                    }
                    if (tempE_D.Equals("X"))
                    {
                        temp_E2_D = Effect.INFINITE_DURATION;
                    }
                    else
                    {
                        sbyte.TryParse(tempE_D, out temp_E2_D);
                    }

                    tempEffects[j] = new Effect(temp_E2_T, temp_E2_P, temp_E2_D);
                }
            }           

            ide.effects = (Effect[])tempEffects.Clone();
        }

        //overheal
        OverhealPayEffect tempOverheal = (OverhealPayEffect)(-1);
        if (entry.Length > 7)
        {
            if (entry[7].Length == 0)
            {
                ide.overheal = OverhealPayEffect.None;
            } else
            {
                if (Enum.TryParse(entry[7], true, out tempOverheal))
                {
                }
                else
                {
                    Debug.LogError("[Item Parsing] " + i + " Can't parse overheal effect \"" + entry[7] + "\"");
                }
                ide.overheal = tempOverheal;
            }
        }

        //overheal divisor
        temp = 0;
        if (entry.Length > 8)
        {
            int.TryParse(entry[8], out temp);
            ide.overhealDivisor = temp;
        }

        //item property blocks
        //format
        //property1:item1/item2/item3... | property2:item1/item2...
        //though no spaces

        ItemPropertyBlock[] tempProps = null;
        if (entry.Length > 9)
        {
            string[] tempPropStrings = entry[9].Split("|");
            tempProps = new ItemPropertyBlock[tempPropStrings.Length];

            if (entry[9].Length == 0)
            {
                tempProps = new ItemPropertyBlock[0];
            }

            string tempProp;
            string tempPropB;
            string[] tempPropC;

            ItemProperty tempItemProperty = (ItemProperty)(-1);
            ItemType[] tempItemTypes = null;  

            if (entry[9].Length > 0)
            {
                for (int k = 0; k < tempPropStrings.Length; k++)
                {
                    tempProp = tempPropStrings[k].Split(":")[0];

                    if (tempPropStrings[k].Split(":").Length < 2)
                    {
                        //itemless
                        if ((Enum.TryParse(tempProp, true, out tempItemProperty)))
                        {
                        }
                        else
                        {
                            Debug.LogError("[Item Parsing] " + i + " Can't parse property \"" + tempProp + "\"");
                        }
                        tempProps[k] = new ItemPropertyBlock(tempItemProperty, null);
                        continue;
                    }
                    if (tempProp.Length == 0)
                    {
                        Debug.LogError("Malformed property block in " + i + " (line in question: " + entry[9] + ")");
                        continue;
                    }


                    tempPropB = tempPropStrings[k].Split(":")[1];
                    tempPropC = tempPropB.Split("/");

                    tempItemTypes = new ItemType[tempPropC.Length];

                    if ((Enum.TryParse(tempProp, true, out tempItemProperty)))
                    {
                    }
                    else
                    {
                        Debug.LogError("[Item Parsing] " + i + " Can't parse property \"" + tempProp + "\"");
                    }

                    for (int l = 0; l < tempPropC.Length; l++)
                    {
                        if ((Enum.TryParse(tempPropC[l], true, out tempItemTypes[l])))
                        {
                        }
                        else
                        {
                            Debug.LogError("[Item Parsing] " + i + " Can't parse item type in property \"" + tempPropC[l] + "\"");
                        }
                    }

                    tempProps[k] = new ItemPropertyBlock(tempItemProperty, tempItemTypes);
                }
            }            

            ide.properties = (ItemPropertyBlock[])tempProps.Clone();
        }

        //weird item
        bool tempBool = false;
        if (entry.Length > 10)
        {
            bool.TryParse(entry[10], out tempBool);
            ide.weird = tempBool;
        }

        //use anim
        ItemUseAnim tempUseAnim = (ItemUseAnim)(-1);
        if (entry.Length > 11)
        {
            if (entry[11].Length == 0)
            {
                ide.useAnim = ItemUseAnim.None;
            } else
            {
                if (Enum.TryParse(entry[11], true, out tempUseAnim))
                {
                }
                else
                {
                    Debug.LogError("[Item Parsing] " + i + " Can't parse item use anim \"" + tempUseAnim + "\"");
                }
                ide.useAnim = tempUseAnim;
            }
        }

        //item quality
        ItemQuality tempQuality = (ItemQuality)(-1);
        if (entry.Length > 12)
        {
            if (entry[12].Length == 0)
            {
                ide.itemQuality = ItemQuality.Mistake;
            }
            else
            {
                if (Enum.TryParse(entry[12], true, out tempQuality))
                {
                }
                else
                {
                    Debug.LogError("[Item Parsing] " + i + " Can't parse item quality \"" + tempQuality + "\"");
                }
                ide.itemQuality = tempQuality;
            }
        }

        //is attack item (note: some items double as healing / attack items though the healing part is only out of battle)
        tempBool = false;
        if (entry.Length > 13)
        {
            bool.TryParse(entry[13], out tempBool);
            ide.isAttackItem = tempBool;
        }

        //damageTypes
        //use | as separator
        //note that this is a flag thing so not an array
        BattleHelper.DamageType damageType = 0;
        if (entry.Length > 14)
        {
            string[] tempDamageTypes = entry[14].Split("|");

            BattleHelper.DamageType d;
            for (int m = 0; m < tempDamageTypes.Length; m++)
            {
                if (tempDamageTypes[m].Length == 0)
                {
                    continue;
                }
                if (Enum.TryParse(tempDamageTypes[m], true, out d))
                {
                    damageType |= d;
                }
                else
                {
                    Debug.LogError("[Item Parsing] " + i + " Can't parse damage type \"" + tempDamageTypes[m] + "\"");
                }
            }

            ide.damageType = damageType;
        }

        //damageProperties
        //use | as separator
        //note that this is a flag thing so not an array
        BattleHelper.DamageProperties damageProperties = 0;
        if (entry.Length > 15)
        {
            string[] tempDamageProperties = entry[15].Split("|");

            BattleHelper.DamageProperties d;
            for (int m = 0; m < tempDamageProperties.Length; m++)
            {
                if (tempDamageProperties[m].Length == 0)
                {
                    continue;
                }
                if (Enum.TryParse(tempDamageProperties[m], true, out d))
                {
                    damageProperties |= d;
                }
                else
                {
                    Debug.LogError("[Item Parsing] " + i + " Can't parse damage property \"" + tempDamageProperties[m] + "\"");
                }
            }

            ide.damageProperties = damageProperties;
        }

        //baseDamage
        temp = 0;
        if (entry.Length > 16)
        {
            int.TryParse(entry[16], out temp);
            ide.baseDamage = temp;
        }


        //chapter
        temp = 0;
        if (entry.Length > 16)
        {
            int.TryParse(entry[17], out temp);
            ide.chapter = temp;
        }

        //recipe
        tempBool = false;
        if (entry.Length > 10)
        {
            bool.TryParse(entry[18], out tempBool);
            ide.isRecipe = tempBool;
        }

        return ide;
    }
}

[System.Serializable]
public struct RecipeDataEntry
{
    //Some of this data isn't strictly necessary (chapter and quality can be found in the data table)
    public ItemType result;
    public int chapter;
    public ItemQuality quality;
    public ItemType ingredientA;
    public ItemType ingredientB;

    public static RecipeDataEntry? Parse(string[] entry)
    {
        RecipeDataEntry result = new RecipeDataEntry();

        ItemType tempItem;
        int tempInt;
        ItemQuality tempQuality;

        tempItem = ItemType.None;
        if (entry.Length > 0)
        {
            if (entry[0].Length == 0)
            {
                result.result = ItemType.None;
            }
            else
            {
                if (Enum.TryParse(entry[0], true, out tempItem))
                {
                }
                else
                {
                    Debug.LogError("[Recipe Parsing] Can't parse item type \"" + entry[0] + "\"");
                }
                result.result = tempItem;
            }
        }

        if (entry.Length > 1)
        {
            int.TryParse(entry[1], out tempInt);
            result.chapter = tempInt;
        }

        tempQuality = ItemQuality.Mistake;
        if (entry.Length > 2)
        {
            if (entry[2].Length == 0)
            {
                result.quality = ItemQuality.Mistake;
            }
            else
            {
                if (Enum.TryParse(entry[2], true, out tempQuality))
                {
                }
                else
                {
                    Debug.LogError("[Recipe Parsing] Can't parse item quality \"" + entry[2] + "\"");
                }
                result.quality = tempQuality;
            }
        }

        tempItem = ItemType.None;
        if (entry.Length > 3)
        {
            if (entry[3].Length == 0)
            {
                result.ingredientA = ItemType.None;
            }
            else
            {
                if (Enum.TryParse(entry[3], true, out tempItem))
                {
                }
                else
                {
                    Debug.LogError("[Recipe Parsing] Can't parse item type \"" + entry[3] + "\"");
                }
                result.ingredientA = tempItem;
            }
        }

        tempItem = ItemType.None;
        if (entry.Length > 4)
        {
            if (entry[4].Length == 0)
            {
                result.ingredientB = ItemType.None;
            }
            else
            {
                if (Enum.TryParse(entry[4], true, out tempItem))
                {
                }
                else
                {
                    Debug.LogError("[Recipe Parsing] Can't parse item type \"" + entry[4] + "\"");
                }
                result.ingredientB = tempItem;
            }
        }

        return result;
    }

    //Cook npcs generally fail to cook the specialty / supreme recipes (except there will be a hardcoded exception for that specific cook's special recipes)
}


public struct KeyItemDataEntry
{
    //Key item data
    public int buyPrice;
    public bool usable;
    public bool consumable; //note: code is hardcoded for each key item as there isn't much overlap in key item stuff*
    public bool stackable;


    public static KeyItemDataEntry? ParseKeyItemDataEntry(string[] entry, KeyItemType i = (KeyItemType)(-1))
    {
        /*
               string debug = "";
               for (int h = 0; h < entry.Length; h++)
               {
                   debug += entry[h] + "\n";
               }
               Debug.Log(debug);
               */

        //note: itemtype is used for validation (you get the string out of the big csv file and use it to double check)
        KeyItemDataEntry ide = new KeyItemDataEntry();

        //validation
        string check = entry[0];
        if (!check.Equals(i.ToString()))
        {
            Debug.LogWarning("[Key Item Parsing] Data table has a mismatch: " + i + " is reading from " + entry[0]);
        }

        //buy price
        int temp = 0;
        if (entry.Length > 1)
        {
            int.TryParse(entry[1], out temp);
            ide.buyPrice = temp;
        }

        //usable
        bool tempBool = false;
        if (entry.Length > 2)
        {
            bool.TryParse(entry[2], out tempBool);
            ide.usable = tempBool;
        }

        //consumable
        tempBool = false;
        if (entry.Length > 3)
        {
            bool.TryParse(entry[3], out tempBool);
            ide.consumable = tempBool;
        }

        tempBool = false;
        if (entry.Length > 4)
        {
            bool.TryParse(entry[4], out tempBool);
            ide.stackable = tempBool;
        }


        return ide;
    }
}

[System.Serializable]
public struct KeyItem
{
    public enum KeyItemType
    {
        None = 0,
        WoodKey,        //C0 key
        StoneKey,       //C1 key
        SandKey,        //C2 key
        SlimeKey,       //C3 key
        BasaltKey,      //C4 key
        DarkKey,        //C5 key
        IceKey,         //C6 key
        ShinyKey,       //C7 key
        CrystalKey,     //C8 key
        StellarKey,     //C9 key
        PlainCandle,    //no effect
        PeachCandle,        //health
        StrawberryCandle,   //energy
        AppleCandle,    //absorb
        LemonCandle,    //stamina
        WatermelonCandle,   //burst
        CherryCandle,   //focus
        EggplantCandle, //ethereal
        PumpkinCandle,  //immunity
        PineappleCandle,    //bonus turns
        HoneyCandle,    //item boost
        FlowerCandle,   //illuminate
        StellarCandle,  //soul
        RainbowCandle,  //freebie
        PowerTotemA,
        PowerTotemB,
        PowerTotemC,
        FortuneTotemA,
        FortuneTotemB,
        FortuneTotemC,

        EndOfTable,
    }

    public KeyItem(KeyItemType p_type, int p_bonusData = 0)
    {
        type = p_type;
        bonusData = p_bonusData;
    }

    public KeyItemType type;
    public int bonusData;

    public static bool CanUse(KeyItem ki)
    {
        return GlobalItemScript.Instance.keyItemDataTable[(int)(ki.type) - 1].usable;
    }

    public static bool IsConsumable(KeyItem ki)
    {
        return GlobalItemScript.Instance.keyItemDataTable[(int)(ki.type) - 1].consumable;
    }

    public static bool IsStackable(KeyItem ki)
    {
        return GlobalItemScript.Instance.keyItemDataTable[(int)(ki.type) - 1].stackable;
    }

    public static int GetCost(KeyItem ki)
    {
        return GlobalItemScript.Instance.keyItemDataTable[(int)(ki.type) - 1].buyPrice;
    }


    //Probably a good idea to make these methods the real source for this information
    //...and make the move scripts get their information from here
    //I will have to set up global data and text tables though
    public static string GetName(KeyItemType ki)
    {
        return GlobalItemScript.Instance.GetKeyItemName(ki);
        //return ki.ToString();
    }
    public static string GetName(KeyItem ki)
    {
        return GetName(ki.type);
    }
    public static string GetDescription(KeyItemType ki)
    {
        return GlobalItemScript.Instance.GetKeyItemDescription(ki);
        //return ki.ToString() + " description";
    }
    public static string GetDescription(KeyItem ki)
    {
        return GetDescription(ki.type);
    }
    public static string GetArticle(KeyItemType ki)
    {
        return GlobalItemScript.Instance.GetKeyItemArticle(ki);
    }
    public static string GetArticle(KeyItem ki)
    {
        return GetArticle(ki.type);
    }
    public static string GetKeyItemText(KeyItemType i, int value)
    {
        return GlobalItemScript.Instance.GetKeyItemText(i, value);
    }
    public static string GetKeyItemText(KeyItem i, int value)
    {
        return GetKeyItemText(i.type, value);
    }
    public static string GetSpriteString(KeyItemType ki)
    {
        return "<keyitemsprite," + ki.ToString() + ">";
    }
    public static string GetSpriteString(KeyItem ki)
    {
        return GetSpriteString(ki.type);
    }

    public static KeyItem Parse(string input)
    {
        KeyItem output = new KeyItem();

        string[] split = input.Split("|");

        if (split.Length > 0)
        {
            //this looks a bit sus
            Enum.TryParse(split[0], true, out output.type);
        }

        if (split.Length > 1)
        {
            int.TryParse(split[1], out output.bonusData);
        }

        return output;
    }
    public override string ToString()
    {
        //return type.ToString();

        //Inverse of Parse
        string output = "";

        output += type.ToString();

        if (bonusData == 0)
        {
            return output;
        }
        output += "|";
        output += bonusData.ToString();

        return output;
    }

    public static List<KeyItem> ParseList(string input)
    {
        string[] split = input.Split(",");
        List<KeyItem> output = new List<KeyItem>();

        //Empty
        if ((split.Length < 1) || (split.Length == 1 && split[0].Length < 1))
        {
            return output;
        }

        for (int i = 0; i < split.Count(); i++)
        {
            output.Add(Parse(split[i]));
        }

        return output;
    }
    public static string ListToString(List<KeyItem> list)
    {
        string output = "";
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            output += list[i].ToString();
        }
        return output;
    }

}

[System.Serializable]
public struct Item
{
    public enum ItemType
    {
        None = 0,
        Carrot,
        BerrySyrup,
        SourBerry,
        SweetBerry,
        LightSeed,
        MiracleDrop,
        Mistake,
        BigMistake,
        CarrotSalad,
        BerryCandy,
        BerryCarrot,
        RoastedNut,
        SourJam,
        SweetNut,
        PowerJam,
        BronzeCarrot,
        FlowerSyrup,
        Apple,
        BitterBean,
        RockNut,
        LivelyLeaf,
        ClearStrawberry,
        BronzeSalad,
        FlowerCandy,
        BerryBronze,
        FlowerCarrot,
        FlowerBronze,
        BitterCoffee,
        AppleJuice,
        HealthySalad,
        Lemon,
        Flour,
        Dragonfruit,
        MistWater,
        StaticThorn,
        WarpedLeaf,
        CarrotCake,
        BronzeCake,
        DryBread,
        MiracleNeedle,
        RabbitBun,
        ApplePie,
        JuiceMix,
        LemonCandy,
        WarpedCookie,
        WarpedTea,
        ThornBundle,
        Lemonade,
        CarrotBunch,
        Watermelon,
        MelonSlice,
        Coconut,
        SleepDart,
        BerryCarrotBunch,
        CarrotPudding,
        BronzePudding,
        CoconutMilk,
        CoconutCream,
        MelonJug,
        MelonJuice,
        CocoPop,
        HustlePudding,
        MilkCoffee,
        TropicalParfait,
        SleepyTea,
        Meringue,
        SlimeBomb,
        SilverCarrot,
        ScaleSteak,
        RockEgg,
        BasaltNut,
        MoltenNut,
        BombFruit,
        LavaPepper,
        SpicyCherry,
        SilverSalad,
        BerrySilver,
        FlowerSilver,
        SilverCake,
        SilverPudding,
        MetalSalad,
        SpicyNut,
        BombJuice,
        FriedEgg,
        PowerSteak,
        DeliciousMeal,
        ScaleBurger,
        PepperNeedle,
        DragonShake,
        HoneySyrup,
        BronzeBunch,
        DarkNeedle,
        Eggplant,
        DarkMushroom,
        MoonlightBerry,
        MosquitoNeedle,
        StrangeBud,
        StickySpore,
        FlowerBronzeBunch,
        HoneyCandy,
        HoneyCarrot,
        HoneyBronze,
        HoneySilver,
        HoneyCaramel,
        StrangeSalad,
        RoastedMushroom,
        StrangeShake,
        HoneyApple,
        DarkBreakfast,
        MiracleShroom,
        PoisonSkewer,
        WeirdTea,
        MoonCookie,
        MintIcicle,
        IcePotato,
        IcePumpkin,
        CrystalLeaf,
        IceNectar,
        LivelyPeach,
        SnowFluff,
        SnowTuft,
        FluffCandy,
        TuftCandy,
        FluffCarrot,
        FluffBronze,
        FluffSilver,
        TuftCarrot,
        TuftBronze,
        TuftSilver,
        PotatoSalad,
        NectarCrystal,
        PumpkinPie,
        CandyPumpkin,
        SpicyFries,
        IceCream,
        MintSalad,
        EggplantTea,
        SweetSmoothie,
        SnowCone,
        LuxurySteak,
        DeluxeDinner,
        SnowyMeringue,
        PeachPie,
        MintSpikes,
        SwiftCake,
        TwinCarrot,
        HalfCarrot,
        GoldenLeaf,
        BrightMushroom,
        GoldenPineapple,
        DrainSeed,
        BoltSeed,
        GoldBomb,
        TwinSalad,
        HalfSalad,
        BerryTwins,
        FlowerTwins,
        HoneyTwins,
        FluffTwins,
        TuftTwins,
        TwinCake,
        TwinPudding,
        BerryHalf,
        FlowerHalf,
        HoneyHalf,
        FluffHalf,
        TuftHalf,
        HalfCake,
        HalfPudding,
        GoldenTea,
        BrightSoup,
        PineappleJuice,
        MintSmoothie,
        MoonlightJelly,
        GoldRabbitBun,
        GoldOmelette,
        LivelySalad,
        MushroomSoup,
        ConversionShake,
        DragonTea,
        StrangeMushroom,
        GoldLemonade,
        BombShake,
        PineappleCone,
        DeluxeJuice,
        MelonBomb,
        GoldNut,
        RabbitCake,
        GoldenCarrot,
        RoyalSyrup,
        SunTomato,
        SunSeed,
        FlashBud,
        GoldenSalad,
        RoyalCandy,
        BerryGolden,
        FlowerGolden,
        HoneyGolden,
        FluffGolden,
        TuftGolden,
        RoyalGolden,
        RoyalCarrot,
        GoldenCake,
        GoldenPudding,
        RoyalBronze,
        RoyalSilver,
        RoyalTwins,
        RoyalHalf,
        SunMuffin,
        SolarSpaghetti,
        RoyalSalad,
        MoonPop,
        SunSauce,
        SourSmoothie,
        CelestialSoup,
        RoyalDinner,
        CelestialMuffin,
        PolarCaramel,
        CelestialParfait,
        SupremeDessert,
        SupremeDinner,
        AetherCarrot,
        CrystalCarrot,
        MiracleSyrup,
        StellarSyrup,
        PlagueRoot,
        GoldenApple,
        EmeraldLime,
        AzureRose,
        RubyFruit,
        VoidIcicle,
        DiamondMushroom,
        CursedStew,
        AetherSalad,
        CrystalSalad,
        StellarCandy,
        MiracleCandy,
        StellarAether,
        MiracleCrystal,
        AetherCake,
        CrystalCake,
        AetherPudding,
        CrystalPudding,
        CrystalMeringue,
        MiracleCaramel,
        StellarCaramel,
        BoosterShake,
        DiluteShake,
        InversionStew,
        ThickShake,
        MiracleApple,
        Limeade,
        FlamingTea,
        RubyJuice,
        VoidCone,
        DiamondSoup,
        SpicyEgg,
        LavaEgg,
        ShockEgg,
        SourEgg,
        LeafyEgg,
        SweetEgg,
        WaveEgg,
        BitterEgg,
        LightEgg,
        DarkEgg,
        SilverEgg,
        GoldenEgg,

        EndOfTable,

        //breaks (Item.ItemType)int things (it uses the wrong one for some reason)
        //FirstEgg = SpicyEgg,
        //LastEgg = GoldenEgg

        //DebugAutoRevive
    }

    /*
    public enum ItemClass
    {
        NormalItem,
        KeyItem
    }
    */

    public enum ItemModifier
    {
        None,
        Focus,      //Force single target   (red) (F)
        Spread,     //Force multi target    (blue)  (S)
        Echo,       //0.8x power, but becomes Echoed    (green) (E)
        Echoed,     //0.4x power                        (green) (e)
        Glistening,  //1.5x power                        (yellow) (G)
        Void,       //Increases item consumable capacity, ignores item consumable limit (black) (V)
        Quick,      //Quick item!   (white) (Q)
    }

    public enum ItemOrigin
    {
        Overworld = 0,  //default, most collectibles will have this and the others are hardcoded
        EnemyDrop,
        NPCGift,    //Quest reward items
        Shop,       //note: will also encompass items gotten from inns and the special one off shops
        CookStella,
        CookTorstrum,
        CookSizzle,
        CookGourmand,
        Producer,   //produced by a different item
        Egg,        //produced by an egg move
        Cheating,   //cheat will default to this, doesn't really do anything special
    }


    public ItemType type;
    public ItemModifier modifier;   //sus technology means I can add more dimensions to items, and my game is already using item.itemtype for stuff anyway
    public ItemOrigin origin;
    public int bonusData;
    public int itemCount;

    public Item(ItemType p_itemType, ItemModifier p_itemModifier = ItemModifier.None, ItemOrigin p_itemOrigin = ItemOrigin.Overworld, int p_bonusData = 0, int p_itemCount = 0)
    {
        type = p_itemType;
        modifier = p_itemModifier;
        origin = p_itemOrigin;
        bonusData = p_bonusData;
        itemCount = p_itemCount;
    }


    /*
    public ItemClass GetItemClass()
    {
        return ItemClass.NormalItem;
    }
    */

    //Items can have multiple
    public enum ItemProperty
    {
        NoBattle,       //Not usable in battle
        NoOverworld,    //Not usable in overworld
        Producer,       //Produces another item on use (*can accept a list)
        Limited,        //Sets a flag that prevents further in battle use (No effect in overworld)
        Stack,          //All get consumed at once, stacking effect
        Unity,          //Power = count of item
        Disunity,       //Power = (1 / count of item)
        Spectral,       //Disappears as battle ends
        Volatile,       //Converts to another item as battle ends (*can accept a list)
        Phantom,        //Destroy when you use a different item
        Ephemeral,      //Become a different item when you use a different item
        TimeBoostTen,       //Boost by turn count (capped at 10)
        TimeBoostTwenty,    //Boost by turn count (capped at 20)
        TimeWeaken,         //Boost by 100% (lose 10% per turn down to 10% at turn 10+)
        DoubleOnTurn1,  //Double strength on first turn
        DoubleAtLowHP,  //Danger hp
        DoubleAtMaxHP,  //max hp
        DoubleAtLowItems,   //5 items or less
        TargetAll,           //Applies to all possible targets (*though ep and se heals only target user)
        TargetAnyone,   //Enables anyone targetting (note: dual + anyone -> targets everyone!)
        TargetNotSelf,        //Cannot target self (only makes sense in the context of ally or anyone targetting)
        HealOverTime,   //Converts healing to heal over time
        SlowHealOverTime, //Heal over time for 5 turns
        SixHealOverTime,
        EightHealOverTime,
        Revive,         //Can be used to revive dead characters
        Miracle,        //also sets revive, applies Miracle if character is alive before
        Cure,           //Cures negative effects
        CureBonus,      //Cure and get bonus for cured effects
        MinusHPIsDamage,    //minus hp is treated as status damage
        Passive_HPRegen,    //note: applies to all
        Passive_HPLoss,    //note: applies to all
        Passive_EPRegen,
        Passive_EPLoss,
        Passive_SERegen,
        Passive_SELoss,
        Passive_AttackUp,
        Passive_AttackDown,
        Passive_DefenseUp,
        Passive_DefenseDown,
        Passive_EnduranceUp,
        Passive_EnduranceDown,
        Passive_AgilityUp,
        Passive_AgilityDown,
        Quick,  //acts like QuickItem when used (+1 action)
        BoostEnemies,   //boost by enemy count (cap at 4x)
    }
    public enum ItemQuality //mostly related to what message the cook npcs play when you make a certain item
    {
        Mistake = -1,        //mistake-like items
        Base,           //base items
        BaseRare,       //rare base items (*only a few endgame items are like this)
        Recipe,         //most recipes end up here
        WeirdRecipe,    //less edible recipes
        GridFillRecipe, //recipes that fill in the grid of carrot + other thing recipes
        GoodRecipe,     //more complex recipes
        SpecialtyRecipe,    //Recipes specific to specific cook npcs
        SupremeRecipe,  //combinations of specialty recipes
        MagicEgg        //special eggs
    }
    public enum ItemUseAnim
    {
        //Note: attack items use the same animations
        None = -1,
        Eat,
        Drink,
        EatBad,
        DrinkBad,
        EatGood,
        DrinkGood,
    }
    public enum OverhealPayEffect
    {
        None = -1,
        Attack,
        Defense,
        Endurance,
        Stamina,
        SoulEnergy,
        AttackShort,
        DefenseShort,
        EnduranceShort,
        AttackDefense,
        FocusAttack,
        AbsorbDefense,
        Focus,
        Absorb,
        Burst,
        HPRegen,
        EPRegen

    }

    public class ItemPropertyBlock
    {
        public ItemProperty property;
        public ItemType[] items;        //can be null or 0 length array

        public ItemPropertyBlock(ItemProperty p_property, ItemType[] p_items)
        {
            property = p_property;
            if (p_items != null)
            {
                items = (ItemType[])p_items.Clone();
            } else
            {
                items = new ItemType[0];
            }
        }

        public override string ToString()
        {
            string output = property + "(";
            string suboutput = "";

            for (int i = 0; i < items.Length; i++)
            {
                suboutput += items[i];
                if (i < items.Length - 1)
                {
                    suboutput += " ";
                }
            }

            output = property + "(" + suboutput + ")";

            return output;
        }
    }



    //Probably a good idea to make these methods the real source for this information
    //...and make the move scripts get their information from here
    //I will have to set up global data and text tables though
    public static string GetName(ItemType i)
    {
        /*
        return i.ToString();
        */
        return GlobalItemScript.Instance.GetItemName(i);
    }
    public static string GetName(Item i)
    {
        string modifierPrefix = GlobalItemScript.Instance.GetItemModifierPrefix(i.modifier);

        if (modifierPrefix.Length > 0)
        {
            modifierPrefix = modifierPrefix + " ";
        }

        if (MainManager.Instance.Cheat_SeePickupCounts)
        {
            return modifierPrefix + GetName(i.type) + " " + i.itemCount;
        }
        else
        {
            return modifierPrefix + GetName(i.type);
        }
    }
    public static string GetDescription(ItemType i)
    {
        return GlobalItemScript.Instance.GetItemDescription(i);

        /*
        switch (i)
        {
            case ItemType.Carrot:
                return "Restores 6 hp";
            case ItemType.BerrySyrup:
                return "Restores 6 ep";
            case ItemType.LightSeed:
                return "3 light damage.";
        }

        return "Description not implemented";
        */
    }
    public static string GetDescription(Item i)
    {
        if (i.modifier != ItemModifier.None)
        {
            return GetDescription(i.type) + "<line>(" + GlobalItemScript.Instance.GetItemModifierPrefix(i.modifier) + " " + GlobalItemScript.Instance.GetItemModifierDescription(i.modifier) + ")";
        }

        return GetDescription(i.type);
    }
    public static string GetArticle(ItemType i)
    {
        return GlobalItemScript.Instance.GetItemArticle(i);
    }
    public static string GetArticle(Item i)
    {
        return GetArticle(i.type);
    }
    public static string GetItemText(ItemType i, int value)
    {
        return GlobalItemScript.Instance.GetItemText(i, value);
    }
    public static string GetItemText(Item i, int value)
    {
        return GetItemText(i.type, value);
    }
    public static TargetArea GetTarget(ItemType i, bool dual)
    {
        //ItemDataEntry ide = Item.GetItemDataEntry(item);

        ItemDataEntry ide = GetItemDataEntry(i);
        bool isAttackItem = ide.isAttackItem;
        bool revive = GetProperty(ide, ItemProperty.Revive) != null;
        bool miracle = GetProperty(ide, ItemProperty.Miracle) != null;
        bool anyone = GetProperty(ide, ItemProperty.TargetAnyone) != null;
        bool notself = GetProperty(ide, ItemProperty.TargetNotSelf) != null;


        if (anyone)
        {
            if (revive || miracle)
            {
                if (notself)
                {
                    return new TargetArea(TargetArea.TargetAreaType.AnyoneNotSelf, dual);
                }
                else
                {
                    return new TargetArea(TargetArea.TargetAreaType.Anyone, dual);
                }
            }
            else
            {
                if (notself)
                {
                    return new TargetArea(TargetArea.TargetAreaType.LiveAnyoneNotSelf, dual);
                }
                else
                {
                    return new TargetArea(TargetArea.TargetAreaType.LiveAnyone, dual);
                }
            }
        }
        else
        {
            if (isAttackItem)
            {
                if (revive || miracle)
                {
                    return new TargetArea(TargetArea.TargetAreaType.Enemy, dual);
                }
                else
                {
                    return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, dual);
                }
            }
            else
            {
                if (revive || miracle)
                {
                    if (notself)
                    {
                        return new TargetArea(TargetArea.TargetAreaType.AllyNotSelf, dual);
                    }
                    else
                    {
                        return new TargetArea(TargetArea.TargetAreaType.Ally, dual);
                    }
                }
                else
                {
                    if (notself)
                    {
                        return new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSelf, dual);
                    }
                    else
                    {
                        return new TargetArea(TargetArea.TargetAreaType.LiveAlly, dual);
                    }
                }
            }
        }

        /*
        if (item != Item.ItemType.MiracleDrop)
        {
            return new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
        } else
        {
            return new TargetArea(TargetArea.TargetAreaType.Ally, false);
        }
        */


        //return GetItemMoveScript(i).GetBaseTarget();
        //return new TargetArea(TargetArea.TargetRange.None);
    }
    public static TargetArea GetTarget(ItemType i)
    {
        ItemDataEntry ide = GetItemDataEntry(i);
        bool dual = GetProperty(ide, ItemProperty.TargetAll) != null;
        return GetTarget(i, dual);
    }
    public static TargetArea GetTarget(Item i)
    {
        if (i.modifier == ItemModifier.Spread)
        {
            return GetTarget(i.type, true);
        }

        if (i.modifier == ItemModifier.Focus)
        {
            return GetTarget(i.type, false);
        }

        return GetTarget(i.type);
    }
    public static float GetItemBoost(int level)
    {
        float boost;
        switch (level)
        {
            case 1:
                boost = (4.00001f / 3);
                break;
            case 2:
                boost = 1.5f;
                break;
            case 3:
                boost = 2f;
                break;
            default:
                if (level <= 0)
                {
                    boost = (2f) / (2f - level);
                }
                else
                {
                    boost = level - 1;
                }
                break;
        }
        return boost;
    }
    /*
    public static float GetPower(ItemType i)
    {
        return GetItemMoveScript(i).GetBasePower();
        //return 0.0f;
    }
    */
    public static string GetActionCommandDesc(ItemType itemType)
    {
        //ehh
        return GetDescription(itemType);
        //return null;
        //return GetItemMoveScript(itemType).GetActionCommandDesc();
    }
    public static string GetActionCommandDesc(Item item)
    {
        return GetActionCommandDesc(item.type);
    }
    public static string GetSpriteString(ItemType i)
    {
        return "<itemsprite," + i.ToString() + ">";
    }
    public static string GetSpriteString(Item i)
    {
        return "<itemsprite," + i.type.ToString() + "," + i.modifier + ">";
    }

    //This will eventually read from an item data table
    public static ItemPropertyBlock[] GetProperties(ItemType i)
    {
        return GetItemDataEntry(i).properties;
    }
    public static ItemPropertyBlock[] GetProperties(Item i) {
        return GetProperties(i.type);
    }
    public static ItemPropertyBlock GetProperty(ItemType item, ItemProperty p) //null if not there
    {
        ItemPropertyBlock[] test = GetProperties(item);
        if (test == null)
        {
            return null;
        }

        for (int i = 0; i < test.Length; i++)
        {
            //Debug.Log(item + " " + test[i]);
            if (test[i].property == p)
            {
                return test[i];
            }
        }
        return null;
    }
    public static ItemPropertyBlock GetProperty(Item item, ItemProperty p)
    {
        return GetProperty(item.type, p);
    }
    public static ItemPropertyBlock GetProperty(ItemDataEntry ide, ItemProperty p)
    {
        ItemPropertyBlock[] test = ide.properties;
        if (test == null)
        {
            return null;
        }

        for (int i = 0; i < test.Length; i++)
        {
            if (test[i].property == p)
            {
                return test[i];
            }
        }
        return null;
    }
    public static int GetSellPrice(ItemType item)
    {
        return GetItemDataEntry(item).sellPrice;
    }
    public static int GetSellPrice(Item item)
    {
        return (int)(GetSellPrice(item.type) * GlobalItemScript.GetItemModifierSellMultiplier(item.modifier));
    }

    public static ItemDataEntry GetItemDataEntry(ItemType item)
    {
        if ((int)item <= 0 || item >= ItemType.EndOfTable)
        {
            Debug.LogError("Attempting to get data entry of invalid item type: " + item);
            return default;
        }
        return GlobalItemScript.Instance.itemDataTable[(int)item - 1];
    }
    public static ItemDataEntry GetItemDataEntry(Item item)
    {
        return GetItemDataEntry(item.type);
    }

    public static bool CanUseInsideBattle(ItemType i)
    {
        return (GetProperty(i, ItemProperty.NoBattle) == null);
    }
    public static bool CanUseOutOfBattle(ItemType i)
    {
        return (GetProperty(i, ItemProperty.NoOverworld) == null);
    }

    public static bool CanUseInsideBattle(Item i)
    {
        return CanUseInsideBattle(i.type);
    }
    public static bool CanUseOutOfBattle(Item i)
    {
        return CanUseOutOfBattle(i.type);
    }

    //this is pure item effect stuff
    public static void UseOutOfBattle(PlayerData.PlayerDataEntry player, Item i, int index)
    {
        if (i.type == ItemType.CursedStew)
        {
            MainManager.Instance.AwardAchievement(MainManager.Achievement.ACH_CursedStew);
        }

        PlayerData players = MainManager.Instance.playerData;

        List<Item> inv = MainManager.Instance.playerData.itemInventory;

        ItemDataEntry ide = GetItemDataEntry(i);

        bool dual = GetProperty(ide, ItemProperty.TargetAll) != null;

        if (i.modifier == ItemModifier.Spread)
        {
            dual = true;
        }
        if (i.modifier == ItemModifier.Focus)
        {
            dual = false;
        }

        bool stack = GetProperty(ide, ItemProperty.Stack) != null;
        float boost = 1;

        int count = inv.FindAll((e) => (e.type == i.type)).Count;

        if (i.modifier == ItemModifier.Glistening)
        {
            boost *= 1.5f;
        }
        if (i.modifier == ItemModifier.Focus)
        {
            boost *= 2f;
        }
        if (i.modifier == ItemModifier.Echo)
        {
            boost *= 0.80001f;
        }
        if (i.modifier == ItemModifier.Echoed)
        {
            boost *= 0.40001f;
        }

        if (GetProperty(ide, ItemProperty.Unity) != null)
        {
            boost *= count;
        }
        if (GetProperty(ide, ItemProperty.Disunity) != null)
        {
            boost /= count;
        }


        if (stack)
        {
            boost *= count;
            inv.RemoveAll((e) => (e.type == i.type && e.modifier != ItemModifier.Echo));

            for (int j = 0; j < inv.Count; j++)
            {
                if (inv[j].type == i.type && inv[j].modifier == ItemModifier.Echo)
                {
                    inv[j] = new Item(inv[j].type, ItemModifier.Echoed, ItemOrigin.Producer, inv[j].bonusData, inv[j].itemCount);
                }
            }

            player.itemsUsed += count;
            MainManager.Instance.playerData.itemsUsed += count;

        } else
        {
            //Debug.Log(inv[index] + " vs " + i);

            //use index to remove
            if (inv[index].Equals(i))    //skip this condition if there is no menu
            {
                inv.RemoveAt(index);
                player.itemsUsed += 1;
                MainManager.Instance.playerData.itemsUsed += 1;
            }
            else if (inv.Remove(inv.Find((e) => (e.Equals(i)))))
            {
                //all good, but the index might be wrong
                Debug.LogWarning("Used an item and removed a different index");
                player.itemsUsed += 1;
                MainManager.Instance.playerData.itemsUsed += 1;
            }
            else
            {
                Debug.LogWarning("Used an item that could not be removed from inventory (item duplication?)");
            }
        }

        ItemPropertyBlock block;

        if (i.modifier == ItemModifier.Echo)
        {
            if (inv.Count < MainManager.Instance.playerData.GetMaxInventorySize())
            {
                if (index >= inv.Count - 1 || index < 0)
                {
                    //inv.Add(new Item(block.items[j]));
                    MainManager.Instance.playerData.AddItem(new Item(i.type, ItemModifier.Echoed, ItemOrigin.Producer, 0, 0));
                }
                else
                {
                    MainManager.Instance.playerData.InsertItem(index, new Item(i.type, ItemModifier.Echoed, ItemOrigin.Producer, 0, 0));
                }
            }
        } else
        {
            block = GetProperty(ide, ItemProperty.Producer);
            if (block != null)
            {
                for (int j = 0; j < block.items.Length; j++)
                {
                    //BattleControl.Instance.playerData.AddItem(new Item(block.items[j]));
                    if (i.modifier == ItemModifier.Void || inv.Count < MainManager.Instance.playerData.GetMaxInventorySize())
                    {
                        if (index >= inv.Count - 1 || index < 0)
                        {
                            //inv.Add(new Item(block.items[j]));
                            MainManager.Instance.playerData.AddItem(new Item(block.items[j], i.modifier, ItemOrigin.Producer, 0, 0));
                        }
                        else
                        {
                            //inv.Insert(index + 1, new Item(block.items[j]));
                            MainManager.Instance.playerData.InsertItem(index, new Item(block.items[j], i.modifier, ItemOrigin.Producer, 0, 0));
                        }
                    }
                }
            }
        }

        ApplyPhantom(inv);

        bool doubler = GetProperty(ide, ItemProperty.DoubleOnTurn1) != null;
        bool doubleB = GetProperty(ide, ItemProperty.DoubleAtLowHP) != null;
        bool doubleC = GetProperty(ide, ItemProperty.DoubleAtMaxHP) != null;
        bool doubleD = GetProperty(ide, ItemProperty.DoubleAtLowItems) != null;

        if (doubler)
        {
            boost *= 2;
        }

        if (doubleB && player.hp <= PlayerData.PlayerDataEntry.GetDangerHP(player.entityID))
        {
            boost *= 2;
        }
        if (doubleC && player.hp >= player.maxHP)
        {
            boost *= 2;
        }

        //no boost from enemies since there are no enemies out of battle


        //mainmanager's playerdata is the one to use here
        if (doubleD && MainManager.Instance.playerData.itemInventory.Count <= 5)
        {
            boost *= 2;
        }

        int hpheal = (int)(ide.hp * boost);
        int epheal = (int)(ide.ep * boost);
        int seheal = (int)(ide.se * boost);

        //Weird calculation
        switch (i.type)
        {
            case Item.ItemType.WarpedCookie:
                //heal for other stat
                hpheal = (int)(players.ep * boost);
                for (int j = 0; j < players.party.Count; j++)
                {
                    epheal += players.party[j].hp;
                }
                epheal /= players.party.Count;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.GoldenTea:
                hpheal = -player.hp / 2;
                hpheal = (int)(hpheal * boost);
                break;
            case Item.ItemType.BrightSoup:
                epheal = -players.ep / 2;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.PineappleJuice:
                epheal = -players.ep / 2;
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.MushroomSoup:
                hpheal = -player.hp / 2;
                epheal = -players.ep / 2;
                hpheal = (int)(hpheal * boost);
                epheal = (int)(epheal * boost);
                break;
            case Item.ItemType.CursedStew:
                hpheal = (int)(-player.hp * boost);
                epheal = (int)(-players.ep * boost);
                break;
            case Item.ItemType.AetherSalad:
                epheal = (int)(-players.ep * boost);
                break;
            case Item.ItemType.StellarCandy:
                hpheal = (int)(-player.hp * boost);
                break;
            case Item.ItemType.AetherPudding:
                hpheal = (int)(-21 * boost);
                epheal = (int)(-21 * boost);
                break;
        }

        if (hpheal != 0)
        {
            if (dual)
            {
                foreach (PlayerData.PlayerDataEntry pde in players.party)
                {
                    pde.hp += hpheal;
                    if (pde.hp < 1) //Can't go below 1 hp in the overworld (doesn't really make sense)
                    {
                        pde.hp = 1;
                    }

                    if (pde.hp > pde.maxHP)
                    {
                        pde.hp = pde.maxHP;
                    }
                }
            } else
            {
                player.hp += hpheal;
                if (player.hp < 1)
                {
                    player.hp = 1;
                }

                if (player.hp > player.maxHP)
                {
                    player.hp = player.maxHP;
                }
            }
        }
        if (epheal != 0)
        {
            players.ep += epheal;
            if (players.ep < 0)
            {
                players.ep = 0;
            }

            if (players.ep > players.maxEP)
            {
                players.ep = players.maxEP;
            }
        }
        if (seheal != 0)
        {
            players.se += hpheal;
            if (players.se < 0)
            {
                players.se = 0;
            }

            if (players.se > players.maxSE)
            {
                players.se = players.maxSE;
            }
        }

        //Apply weird effects
        switch (i.type)
        {
            case Item.ItemType.WarpedLeaf:
                //Swap HP and EP

                //new ep = average of target hp
                //new hp = ep
                int tempep = 0;
                for (int j = 0; j < players.party.Count; j++)
                {
                    tempep += players.party[j].hp;
                    players.party[j].hp = players.ep;

                    if (players.party[j].hp > players.party[j].maxHP)
                    {
                        players.party[j].hp = players.party[j].maxHP;
                    }
                    if (players.party[j].hp < 0)
                    {
                        players.party[j].hp = 0;
                    }
                }
                tempep /= players.party.Count;

                players.ep = tempep;

                if (players.ep > players.maxEP)
                {
                    players.ep = players.maxEP;
                }
                if (players.ep < 0)
                {
                    players.ep = 0;
                }

                break;
            case Item.ItemType.WarpedTea:
                //halves missing hp, ep
                for (int j = 0; j < players.party.Count; j++)
                {
                    players.party[j].hp = players.party[j].maxHP / 2 + players.party[j].hp / 2;
                }
                players.ep = players.maxEP / 2 + players.ep / 2;
                break;
            case Item.ItemType.StrangeBud:
                //inverts missing hp, ep
                for (int j = 0; j < players.party.Count; j++)
                {
                    players.party[j].hp = players.party[j].maxHP - players.party[j].hp;
                }
                players.ep = players.maxEP - players.ep;
                break;
            case Item.ItemType.StrangeSalad:
                //set to half hp, ep
                for (int j = 0; j < players.party.Count; j++)
                {
                    players.party[j].hp = players.party[j].maxHP / 2;
                }
                players.ep = players.maxEP / 2;
                break;
            case Item.ItemType.WeirdTea:
                //swaps ep and se
                int temp = players.ep;
                players.ep = players.se;
                players.se = temp;

                if (players.ep > players.maxEP)
                {
                    players.ep = players.maxEP;
                }
                if (players.ep < 0)
                {
                    players.ep = 0;
                }

                if (players.se > players.maxSE)
                {
                    players.se = players.maxSE;
                }
                if (players.se < 0)
                {
                    players.se = 0;
                }
                break;
        }

        return;
    } 
    public static void ApplyVolatile()
    {
        ApplyVolatile(MainManager.Instance.playerData.itemInventory);
    }
    public static void ApplyVolatile(List<Item> inv)
    {
        //List<Item> inv = MainManager.Instance.playerData.itemInventory;

        ItemPropertyBlock blockA;
        ItemPropertyBlock blockB;

        for (int i = 0; i < inv.Count; i++)
        {
            blockA = GetProperty(inv[i], ItemProperty.Spectral);

            if (blockA != null)
            {
                inv.RemoveAt(i);
                i--;
                continue;
            }

            blockB = GetProperty(inv[i], ItemProperty.Volatile);

            if (blockB != null)
            {
                Item newItem = inv[i];
                newItem.type = blockB.items[0];
                inv[i] = newItem;
                for (int j = 1; j < blockB.items.Length; j++)
                {
                    Item newItem2 = new Item(blockB.items[j], inv[i].modifier, ItemOrigin.Producer, 0, 0);
                    if (inv[i].modifier != ItemModifier.Void && inv.Count < MainManager.Instance.playerData.GetMaxInventorySize())
                    {
                        if (i >= inv.Count - 1 || i < 0)
                        {
                            MainManager.Instance.playerData.AddItem(newItem2);
                            //inv.Add(newItem2);
                        }
                        else
                        {
                            MainManager.Instance.playerData.InsertItem(i + 1, newItem2);
                            //inv.Insert(i + 1, newItem2);
                        }
                    }
                }
                continue;
            }
        }
    }
    public static void ApplyPhantom()
    {
        ApplyPhantom(MainManager.Instance.playerData.itemInventory);
    }
    public static void ApplyPhantom(List<Item> inv)
    {
        //List<Item> inv = MainManager.Instance.playerData.itemInventory;

        ItemPropertyBlock blockA;
        ItemPropertyBlock blockB;

        for (int i = 0; i < inv.Count; i++)
        {
            blockA = GetProperty(inv[i], ItemProperty.Phantom);

            if (blockA != null)
            {
                inv.RemoveAt(i);
                i--;
                continue;
            }

            blockB = GetProperty(inv[i], ItemProperty.Ephemeral);

            if (blockB != null)
            {
                Item newItem = inv[i];
                newItem.type = blockB.items[0];
                inv[i] = newItem;
                for (int j = 1; j < blockB.items.Length; j++)
                {
                    Item newItem2 = new Item(blockB.items[j], inv[i].modifier, ItemOrigin.Producer, 0, 0);
                    if (inv[i].modifier != ItemModifier.Void && inv.Count < MainManager.Instance.playerData.GetMaxInventorySize())
                    {
                        if (i >= inv.Count - 1 || i < 0)
                        {
                            MainManager.Instance.playerData.AddItem(newItem2);
                            //inv.Add(newItem2);
                        }
                        else
                        {
                            MainManager.Instance.playerData.InsertItem(i + 1, newItem2);
                            //inv.Insert(i + 1, newItem2);
                        }
                    }
                }
                continue;
            }
        }
    }
    public static int CountItemsWithProperty(ItemProperty prop, List<Item> items)
    {
        int count = 0;
        for (int i = 0; i < items.Count; i++)
        {
            ItemPropertyBlock b = GetProperty(items[i], prop);

            if (b != null)
            {
                count++;
            }
        }
        return count;
    }

    /*
    public static ItemMove GetItemMoveScript(Item i)
    {
        return GetItemMoveScript(i.type);
    }
    */

    //it works
    public static ItemMove GetItemMoveScript(Item i)
    {
        ItemDataEntry ide = GetItemDataEntry(i);

        ItemMove m = null;

        
        switch (i.type)
        {
            case ItemType.MiracleNeedle:
            case ItemType.MiracleShroom:
            case ItemType.StrangeMushroom:
                m = GlobalItemScript.Instance.GetOrAddItemMove<Item_AutoConsumable>(i);
                break;
            case ItemType.SlimeBomb:
            case ItemType.PepperNeedle:
            case ItemType.StickySpore:
            case ItemType.GoldBomb:
                m = GlobalItemScript.Instance.GetOrAddItemMove<Item_AutoThrowable>(i);
                break;
        }


        if (m != null)
        {
            m.SetItem(i);
            return m;
        }

        

        bool isAttackItem = ide.isAttackItem;

        if (isAttackItem)
        {
            m = GlobalItemScript.Instance.GetOrAddItemMove<Item_GenericThrowable>(i);
            m.SetItem(i);
            return m;
        } else
        {
            m = GlobalItemScript.Instance.GetOrAddItemMove<Item_GenericConsumable>(i);
            m.SetItem(i);
            return m;
        }
    }

    public static MetaItemMove GetMetaItemMoveScript(MetaItemMove.Move move)
    {
        MetaItemMove m = null;
        switch (move)
        {
            case MetaItemMove.Move.Normal:
                m = GlobalItemScript.Instance.GetOrAddComponent<MetaItem_Identity>();
                break;
            case MetaItemMove.Move.Multi:
                m = GlobalItemScript.Instance.GetOrAddComponent<MetaItem_Multi>();
                break;
            case MetaItemMove.Move.Quick:
                m = GlobalItemScript.Instance.GetOrAddComponent<MetaItem_Quick>();
                break;
            case MetaItemMove.Move.Void:
                m = GlobalItemScript.Instance.GetOrAddComponent<MetaItem_Void>();
                break;
        }

        return m;
    }

    public override bool Equals(object obj)
    {
        Item b = (Item)obj;

        if (type != b.type)
        {
            return false;
        }

        if (modifier != b.modifier)
        {
            return false;
        }

        if (origin != b.origin)
        {
            return false;
        }

        if (bonusData != b.bonusData)
        {
            return false;
        }

        if (itemCount != b.itemCount)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(type, modifier, origin, bonusData, itemCount);
    }

    /*
public static ItemMove GetItemMoveScript(ItemType i)
{
   return null;
}
*/
    public static Item Parse(string input)
    {
        Item output = new Item();

        string[] split = input.Split("|");

        if (split.Length > 0)
        {
            //this looks a bit sus
            Enum.TryParse(split[0], true, out output.type);
        }

        if (split.Length > 1)
        {
            Enum.TryParse(split[1], true, out output.modifier);
        }

        if (split.Length > 2)
        {
            Enum.TryParse(split[2], true, out output.origin);
        }

        if (split.Length > 3)
        {
            int.TryParse(split[3], out output.bonusData);
        }

        if (split.Length > 4)
        {
            int.TryParse(split[4], out output.itemCount);
        }

        return output;
    }
    public override string ToString()
    {
        //return type.ToString();

        //Inverse of Parse
        string output = "";

        output += type.ToString();

        output += "|";
        output += modifier.ToString();

        output += "|";
        output += origin.ToString();


        if (bonusData == 0 && itemCount == 0)
        {
            return output;
        }
        output += "|";
        output += bonusData.ToString();

        if (itemCount == 0)
        {
            return output;
        }
        output += "|";
        output += itemCount.ToString();

        return output;
    }

    public static List<Item> ParseList(string input)
    {
        string[] split = input.Split(",");
        List<Item> output = new List<Item>();

        //Empty
        if ((split.Length < 1) || (split.Length == 1 && split[0].Length < 1))
        {
            return output;
        }

        for (int i = 0; i < split.Count(); i++)
        {
            output.Add(Parse(split[i]));
        }

        return output;
    }
    public static string ListToString(List<Item> list)
    {
        string output = "";
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            output += list[i].ToString();
        }
        return output;
    }

}

//most of the move methods could apply to items
public abstract class ItemMove : Move, IEntityHighlighter
{
    //public abstract string GetName();
    //public abstract string GetDescription();

    //public abstract Item.ItemType GetItemType();
    public ItemType GetItemType()
    {
        return item.type;
    }

    public Item GetItem()
    {
        return item;
    }

    public ItemModifier GetModifier()
    {
        return item.modifier;
    }

    public Item item;

    public int itemCount;

    //special unused values
    public bool forceSingleTarget = false;
    public bool forceMultiTarget = false;

    //used to allow items to share scripts but act differently
    public void SetItem(Item item)
    {
        this.item = item;
        itemCount = 1;
    }

    public virtual IEnumerator DefaultStartAnim(BattleEntity caller)
    {
        ItemType it = GetItemType();
        ItemDataEntry ide = Item.GetItemDataEntry(item.type);
        Debug.Log("Item anim: "+it);

        MainManager.Instance.PlaySound(caller.gameObject, MainManager.Sound.SFX_Item_Use);

        //spawn a sprite
        Sprite isp = GlobalItemScript.GetItemSprite(it);

        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;

        GameObject so = new GameObject("Item Use Sprite");
        so.transform.parent = BattleControl.Instance.transform;
        SpriteRenderer s = so.AddComponent<SpriteRenderer>();
        s.sprite = isp;
        s.material = GlobalItemScript.GetItemModifierMaterial(GetModifier());
        so.transform.position = position;

        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_Item_UseFireworks"), gameObject.transform);
        eo.transform.position = position + Vector3.forward * 0.01f;
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);

        yield return new WaitForSeconds(1f);

        Destroy(so);

        yield return null;
    }
    /*
    public virtual IEnumerator UseAnim(BattleEntity caller)
    {
        yield return null;
    }
    */
    public virtual IEnumerator ProducerAnim(BattleEntity caller)
    {
        ItemType it = GetItemType();
        Debug.Log("Item producer anim: " + it);

        //No producer anim because the item is not being used
        if (caller.HasEffect(Effect.EffectType.Freebie))
        {
            yield break;
        }

        ItemPropertyBlock block;
        List<Item> itemList = new List<Item>();

        if (GetModifier() == ItemModifier.Echo)
        {
            if (BattleControl.Instance.playerData.itemInventory.Count <= BattleControl.Instance.playerData.GetMaxInventorySize())
            {
                itemList.Add(new Item(GetItemType(), ItemModifier.Echoed, ItemOrigin.Producer, 0, 0));
            }
        } else
        {
            block = GetProperty(GetItemDataEntry(it), ItemProperty.Producer);
            if (block != null)
            {
                for (int j = 0; j < block.items.Length; j++)
                {
                    if (GetModifier() == ItemModifier.Void || BattleControl.Instance.playerData.itemInventory.Count + j <= BattleControl.Instance.playerData.GetMaxInventorySize())
                    {
                        itemList.Add(new Item(block.items[j], item.modifier, ItemOrigin.Producer, 0, 0));
                    }
                }
            }
        }

        if (itemList.Count == 0)
        {
            yield break;
        }

        for (int i = 0; i < itemList.Count; i++)
        {
            Debug.Log("Item produced anim: " + itemList[i].type);
            //spawn a sprite
            Sprite isp = GlobalItemScript.GetItemSprite(itemList[i].type);


            Vector2 startPosition = caller.transform.position + caller.height * Vector3.up;
            Vector3 endPosition = caller.transform.position + caller.height * Vector3.up + Vector3.up * 0.75f;

            GameObject so = new GameObject("Item Produced Sprite");
            so.transform.parent = BattleControl.Instance.transform;
            SpriteRenderer s = so.AddComponent<SpriteRenderer>();
            s.sprite = isp;
            if (GetModifier() == ItemModifier.Echo)
            {
                s.material = GlobalItemScript.GetItemModifierMaterial(ItemModifier.Echoed);
            }
            else
            {
                s.material = GlobalItemScript.GetItemModifierMaterial(GetModifier());
            }
            so.transform.position = startPosition;

            IEnumerator  PosLerp(GameObject o, float duration, Vector3 posA, Vector3 posB)
            {
                float time = 0;

                while (time < 1)
                {
                    time += Time.deltaTime / duration;
                    o.transform.position = posA + (posB - posA) * (1 - (1 - time) * (1 - time));
                    yield return null;
                }

                o.transform.position = posB;
            }

            yield return StartCoroutine(PosLerp(so, 0.4f, startPosition, endPosition));
            //yield return new WaitForSeconds(1f);

            Destroy(so);
        }

        yield return null;
    }

    public override bool ShowNamePopup()
    {
        return false;
    }

    public override string GetName() => Item.GetName(GetItem());
    public override string GetDescription() => Item.GetDescription(GetItem());
    public override TargetArea GetBaseTarget() => Item.GetTarget(GetItem());

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return DefaultStartAnim(caller);
    }

    public override bool CanChoose(BattleEntity caller, int level = 1)
    {
        return true;
    }

    public override void ChooseMove(BattleEntity caller, int level = 1)
    {
        BattleControl.Instance.BroadcastEvent(caller, BattleHelper.Event.UseItem);
        itemCount = BattleControl.Instance.CountAllItemsOfType(caller, GetItemType());
        BattleRemoveItemUsed(caller);
    }

    //Battle item use
    //this is a little hacky the way I did this
    public void BattleRemoveItemUsed(BattleEntity caller, int index = -1)
    {
        List<Item> inv = BattleControl.Instance.GetItemInventory(caller); //BattleControl.Instance.playerData.itemInventory; //MainManager.Instance.playerData.itemInventory;

        if (caller.HasEffect(Effect.EffectType.Freebie))
        {
            //need to keep track of this still so that you can't spam the 1/battle items
            //(Because many of them are extremely overpowered if you can use them too many times)
            BattleControl.Instance.GetUsedItemInventory(caller).Add(GetItem());

            caller.TokenRemoveOne(Effect.EffectType.Freebie);
            return;
        }

        //Item saver incrementing
        if (caller is PlayerEntity pcaller)
        {
            if (pcaller.BadgeEquipped(Badge.BadgeType.ItemSaver))
            {
                //No
                if (item.type >= ItemType.SpicyEgg && item.type <= ItemType.GoldenEgg)
                {

                } else
                {
                    pcaller.itemSaver += 1;
                }
                if (pcaller.itemSaver >= 2)
                {
                    pcaller.itemSaver = 0;
                    pcaller.InflictEffect(caller, new Effect(Effect.EffectType.Freebie, (sbyte)pcaller.BadgeEquippedCount(Badge.BadgeType.ItemSaver), Effect.INFINITE_DURATION));
                }
            }
        }

        //try to use the box menu to get the right item
        BoxMenu b = FindObjectOfType<ItemBoxMenu>();

        if (index != -1 && inv[index].Equals(item))
        {
            inv.RemoveAt(index);
            BattleControl.Instance.playerData.GetPlayerDataEntry(caller.entityID).itemsUsed += 1;
            BattleControl.Instance.playerData.itemsUsed += 1;
        }
        else if (index != -1)
        {
            bool remove = false;
            for (int i = index; i >= 0; i--)
            {
                if (inv[i].Equals(item))
                {
                    inv.RemoveAt(i);
                    remove = true;
                    BattleControl.Instance.playerData.GetPlayerDataEntry(caller.entityID).itemsUsed += 1;
                    BattleControl.Instance.playerData.itemsUsed += 1;
                    break;
                }
            }
            if (!remove)
            {
                if (inv.Remove(inv.Find((e) => (e.Equals(item)))))
                {
                    //all good, but the index might be wrong
                    Debug.LogWarning(item + ": Used an item and removed a different index (Normal for auto-activate items)");
                    BattleControl.Instance.playerData.GetPlayerDataEntry(caller.entityID).itemsUsed += 1;
                    BattleControl.Instance.playerData.itemsUsed += 1;
                }
                else
                {
                    Debug.LogError(item + ": Used an item that could not be removed from inventory (item duplication?)");
                }
            } else
            {
                Debug.LogWarning(item + ": Used an item and possibly removed a different index (May appear with Double Bite)");
                BattleControl.Instance.playerData.GetPlayerDataEntry(caller.entityID).itemsUsed += 1;
                BattleControl.Instance.playerData.itemsUsed += 1;
            }
        }
        else if (b != null && inv[b.menuIndex].Equals(item))    //skip this condition if there is no menu
        {
            inv.RemoveAt(b.menuIndex);
            BattleControl.Instance.playerData.GetPlayerDataEntry(caller.entityID).itemsUsed += 1;
            BattleControl.Instance.playerData.itemsUsed += 1;
        }
        else if (inv.Remove(inv.Find((e) => (e.Equals(item)))))
        {
            //all good, but the index might be wrong
            Debug.LogWarning(item + ": Used an item and removed a different index (Normal for auto-activate items)");
            BattleControl.Instance.playerData.GetPlayerDataEntry(caller.entityID).itemsUsed += 1;
            BattleControl.Instance.playerData.itemsUsed += 1;
        }
        else
        {
            Debug.LogError(item + ": Used an item that could not be removed from inventory (item duplication?)");
        }

        //Apply properties
        Item.ItemPropertyBlock block;

        /*
        block = Item.GetProperty(GetItemType(), Item.ItemProperty.Limited);
        if (block != null)
        {
            inv.RemoveAll((e) => (e.type == GetItemType()));
        }
        */

        block = GetProperty(GetItem(), ItemProperty.Stack);
        if (block != null)
        {
            inv.RemoveAll((e) => (e.type == GetItemType() && e.modifier != ItemModifier.Echo));

            for (int i = 0; i < inv.Count; i++)
            {
                if (inv[i].type == GetItemType() && inv[i].modifier == ItemModifier.Echo)
                {
                    inv[i] = new Item(inv[i].type, ItemModifier.Echoed, ItemOrigin.Producer, inv[i].bonusData, inv[i].itemCount);
                }
            }
        }

        if (GetModifier() == ItemModifier.Echo)
        {
            if (inv.Count < BattleControl.Instance.playerData.GetMaxInventorySize())
            {
                if (b == null || b.menuIndex >= inv.Count - 1 || b.menuIndex < 0)
                {
                    //inv.Add(new Item(block.items[j]));
                    BattleControl.Instance.AddItemInventory(caller, new Item(GetItemType(), ItemModifier.Echoed, ItemOrigin.Producer, 0, 0));
                }
                else
                {
                    BattleControl.Instance.InsertItemInventory(caller, b.menuIndex, new Item(GetItemType(), ItemModifier.Echoed, ItemOrigin.Producer, 0, 0));
                }
            }
        }
        else
        {
            block = Item.GetProperty(GetItem(), Item.ItemProperty.Producer);
            if (block != null)
            {
                for (int i = 0; i < block.items.Length; i++)
                {
                    //BattleControl.Instance.playerData.AddItem(new Item(block.items[i]));
                    if (inv.Count < BattleControl.Instance.GetMaxItemInventory(caller) || GetModifier() == ItemModifier.Void)
                    {
                        if (b == null || b.menuIndex >= inv.Count - 1 || b.menuIndex < 0)
                        {
                            BattleControl.Instance.AddItemInventory(caller, new Item(block.items[i], item.modifier, ItemOrigin.Producer, 0, 0));
                            //inv.Add(new Item(block.items[i]));
                        }
                        else
                        {
                            BattleControl.Instance.InsertItemInventory(caller, b.menuIndex, new Item(block.items[i], item.modifier, ItemOrigin.Producer, 0, 0));
                            //inv.Insert(b.menuIndex + 1, new Item(block.items[i]));
                        }
                    }
                }
            }
        }

        //note: this means stacking items only get added once
        //(which means in most cases the used inventory can only have 1 of each stacking item)
        //(exception is if a stacking item gets added to your inventory in some way after using one earlier, probably with a producer item (gold bomb can do this))
        BattleControl.Instance.GetUsedItemInventory(caller).Add(GetItem());

        Item.ApplyPhantom(BattleControl.Instance.GetItemInventory(caller));

        /*
        string debug = "";
        foreach (ItemType it in BattleControl.Instance.usedItems)
        {
            debug += it + " ";
        }
        Debug.Log(debug);
        */        
    }

    public void RemoveAllOfType(BattleEntity caller)
    {
        List<Item> inv = BattleControl.Instance.GetItemInventory(caller); //BattleControl.Instance.playerData.itemInventory; //MainManager.Instance.playerData.itemInventory;
        inv.RemoveAll((e) => e.type == GetItemType());
    }

    public virtual string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        return "";
    }
}

public abstract class MetaItemMove : Move
{
    public ItemMove itemMove;

    public enum Move
    {
        Normal,
        Multi,
        Quick,
        Void
    }

    public override bool ShowNamePopup()
    {
        return false;
    }

    public static string GetName(Move move)
    {
        switch (move)
        {
            case Move.Normal:
                return "Item";
            case Move.Multi:
                return "Multi Supply";
            case Move.Quick:
                return "Quick Supply";
            case Move.Void:
                return "Void Supply";
        }
        return "";
    }

    public static string GetDescription(Move move)
    {
        switch (move)
        {
            case Move.Normal:
                return "Use an item normally.";
            case Move.Multi:
                return "Use up to 3 items with one action. (In the menu, use <button,B> to cancel, or <button,Z> to end your selection early.)";
            case Move.Quick:
                return "Use an item and get another action. (Can't do this more than once per turn.)";
            case Move.Void:
                return "Use an item that was used previously in battle.";
        }
        return "";
    }

    public static string GetSpriteString(Move move)
    {
        switch (move)
        {
            case Move.Normal:
                return "<itemsprite,Carrot>";
            case Move.Multi:
                return "<badgesprite,MultiSupply>";
            case Move.Quick:
                return "<badgesprite,QuickSupply>";
            case Move.Void:
                return "<badgesprite,VoidSupply>";
        }
        return "";
    }

    public virtual Move GetMove()
    {
        return Move.Normal;
    }


    public override bool CanChoose(BattleEntity caller, int level = 1)
    {
        return BattleControl.Instance.CanUseMetaItemMove(GetMove());
    }
}
