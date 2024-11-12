using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OWItemBoxMenu : BoxMenu
{
    //int displayIndex;
    /*
    ItemMenuEntry.StatDisplay displayMode
    {
        get => displayModes[displayIndex];
    }
    List<ItemMenuEntry.StatDisplay> displayModes; //All possible display modes
    */

    //creation preset is stuff in textbox
    public enum CreationPreset
    {
        //note: items use Item.Parse so you can put in all the args
        Default = 0,    //MenuResult has items
        Pairs,          //MenuResult has item - righttext pairs
        PricePairs,     //MenuResult has item - price pairs
        Overworld,      //overworld
        OverworldHighlighted,   //overworld with color list
        OverworldHighlightedBlock,   //overworld with color list (color list = can't use)
        OverworldHighlightedBlockZ, //same as above + z target
        Battle,         //not really a good idea to make an OW item menu in battle but I don't think it will necessarily break things (but why would you want a special menu though?)
        Storage,        //Storage inventory
        StorageHighlighted,        //Storage with color list
        StorageHighlightedBlock,        //Storage with color list (color list = can't use)
        Selling,        //overworld + sell prices

        //TooManyItems,   //Too many items inventory
    }

    public bool requiresTarget = false;
    public bool zTarget = false;
    public bool zSelect = false;
    public List<Item> itemList;
    public List<string> rightTextList;
    public List<Color?> backgroundColors;
    public List<bool> canUseList;

    public bool menuDone = false;
    //public override event EventHandler<MenuExitEventArgs> menuExit;

    public bool noCancelGoBack;
    public bool checkCanUse;

    //To do: make this less spaghetti
    //Need to introduce a new class to glue all the parameters together
    //note: backgroundColors list can be incomplete (everything after is treated null)
    public static OWItemBoxMenu BuildMenu(List<Item> itemList = null, List<string> rightTextList = null, List<Color?> backgroundColors = null, List<bool> canUseList = null, string descriptorString = null, bool noCancelGoBack = false, bool checkCanUse = false, bool requiresTarget = false, bool zTarget = false, bool zSelect = false, bool canUseDisabled = true)
    {
        GameObject newObj = new GameObject("OW Item Menu");
        OWItemBoxMenu newMenu;

        newMenu = newObj.AddComponent<OWItemBoxMenu>();
        newMenu.requiresTarget = false;

        if (itemList == null)
        {
            newMenu.itemList = MainManager.Instance.playerData.itemInventory;
        } else
        {
            newMenu.itemList = itemList;
        }
        newMenu.rightTextList = rightTextList;
        newMenu.backgroundColors = backgroundColors;
        newMenu.canUseList = canUseList;
        newMenu.descriptorString = descriptorString;
        newMenu.noCancelGoBack = noCancelGoBack;
        newMenu.checkCanUse = checkCanUse;
        newMenu.requiresTarget = requiresTarget;
        newMenu.zTarget = zTarget;
        newMenu.zSelect = zSelect;
        newMenu.canUseDisabled = canUseDisabled;

        newMenu.Init();

        return newMenu;
    }

    public override void Init()
    {
        lifetime = 0;
        /*
        if (displayModes == null)
        {
            displayModes = new List<ItemMenuEntry.StatDisplay>
            {
                ItemMenuEntry.StatDisplay.Sprite
            };
            //displayModes.Add(ItemMenuEntry.StatDisplay.Time);
        }
        */
        //displayIndex = 0;
        active = true;
        menuIndex = 0;
        visualSelectIndex = menuIndex;

        menuBaseO = Instantiate(MainManager.Instance.menuBase, MainManager.Instance.Canvas.transform);
        descriptionBoxO = Instantiate(MainManager.Instance.descriptionBoxBase, MainManager.Instance.Canvas.transform);
        descriptionBoxScript = descriptionBoxO.GetComponent<DescriptionBoxScript>();
        //menuBaseO.transform.position = new Vector3(250, -70, 0);
        menuEntriesO = new List<GameObject>();

        bm = menuBaseO.GetComponent<BoxMenuScript>();

        //get items from item inventory
        List<Item> inv = itemList; //MainManager.Instance.playerData.itemInventory;
        menuEntries = new BoxMenuEntry[inv.Count];
        for (int i = 0; i < inv.Count; i++)
        {
            if (rightTextList != null)
            {
                menuEntries[i] = new ItemMenuEntry(inv[i], rightTextList[i]);
            }
            else
            {
                menuEntries[i] = new ItemMenuEntry(inv[i]);
            }
            if (backgroundColors != null)
            {
                if (i < backgroundColors.Count && backgroundColors[i] != null)
                {
                    menuEntries[i].backgroundColor = backgroundColors[i].Value;
                    menuEntries[i].hasBackground = true;
                }
            }

            if (canUseList != null)
            {
                if (i < canUseList.Count)
                {
                    menuEntries[i].canUse = canUseList[i];
                }
            }

            if (checkCanUse)
            {
                menuEntries[i].canUse = Item.CanUseOutOfBattle(itemList[i].type);
            }

            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        menuTopIndex = 0;
        visualTopIndex = menuTopIndex;
        bm.upArrow.enabled = false; //menuTopIndex > 0;
        bm.downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (descriptorString != null)
        {
            bm.descriptorTextBox.SetText(descriptorString, true);
        }
        else
        {
            bm.descriptorBox.enabled = false;
            bm.descriptorTextBox.gameObject.SetActive(false);
        }

        descriptionBoxScript.SetText(menuEntries[menuIndex].description);
    }
    public override void SelectOption()
    {
        if (requiresTarget && Item.CanUseOutOfBattle(itemList[menuIndex].type))
        {
            //Selecting a move takes you to a selection menu
            List<Item> inv = itemList; //MainManager.Instance.playerData.itemInventory;
            OWCharacterBoxMenu s2 = OWCharacterBoxMenu.BuildMenu();
            s2.transform.SetParent(transform);
            PushState(s2);
            s2.menuExit += InvokeExit;
        }
        else
        {
            InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
            //Clear();
        }
    }
    public override void ZOption()
    {
        if (zSelect && Item.CanUseOutOfBattle(itemList[menuIndex].type))
        {
            //Selecting a move takes you to a selection menu
            List<Item> inv = itemList; //MainManager.Instance.playerData.itemInventory;
            OWCharacterBoxMenu s2 = OWCharacterBoxMenu.BuildMenu();
            s2.transform.SetParent(transform);
            PushState(s2);
            s2.menuExit += InvokeExit;
        }
        if (zTarget && !zSelect)
        {
            menuIndex = -2;
            InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
        }
    }
    public override void Cancel()
    {
        if (!noCancelGoBack)
        {
            PopSelf();
            if (parent == null)
            {
                menuIndex = -1;
                SelectOption();
            }
        } else
        {
            menuIndex = -1;
            SelectOption();
        }
    }
    /*  
    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }
    */
    public override MenuResult GetResult()
    {
        //        
        if (menuIndex < 0)
        {
            return new MenuResult("cancel," + menuIndex);
        }

        string s = (((ItemMenuEntry)(menuEntries[menuIndex])).item.type.ToString()) + "," + menuIndex;
        return new MenuResult(s);
    }
}

//Just a copy paste of OWItemBoxMenu
public class OWKeyItemBoxMenu : BoxMenu
{
    public bool zTarget = false;
    public List<KeyItem> itemList;
    public List<string> rightTextList;
    public List<Color?> backgroundColors;
    public List<bool> canUseList;

    public bool menuDone = false;
    //public override event EventHandler<MenuExitEventArgs> menuExit;

    public bool noCancelGoBack;
    public bool checkCanUse;

    //To do: make this less spaghetti
    //Need to introduce a new class to glue all the parameters together
    //note: backgroundColors list can be incomplete (everything after is treated null)
    public static OWKeyItemBoxMenu BuildMenu(List<KeyItem> itemList = null, List<string> rightTextList = null, List<Color?> backgroundColors = null, List<bool> canUseList = null, string descriptorString = null, bool noCancelGoBack = false, bool checkCanUse = false, bool zTarget = false, bool canUseDisabled = true)
    {
        GameObject newObj = new GameObject("OW Key Item Menu");
        OWKeyItemBoxMenu newMenu;

        newMenu = newObj.AddComponent<OWKeyItemBoxMenu>();

        if (itemList == null)
        {
            newMenu.itemList = MainManager.Instance.playerData.keyInventory;
        }
        else
        {
            newMenu.itemList = itemList;
        }
        newMenu.rightTextList = rightTextList;
        newMenu.backgroundColors = backgroundColors;
        newMenu.canUseList = canUseList;
        newMenu.descriptorString = descriptorString;
        newMenu.noCancelGoBack = noCancelGoBack;
        newMenu.checkCanUse = checkCanUse;
        newMenu.zTarget = zTarget;
        newMenu.canUseDisabled = canUseDisabled;

        newMenu.Init();

        return newMenu;
    }

    public override void Init()
    {
        lifetime = 0;
        /*
        if (displayModes == null)
        {
            displayModes = new List<ItemMenuEntry.StatDisplay>
            {
                ItemMenuEntry.StatDisplay.Sprite
            };
            //displayModes.Add(ItemMenuEntry.StatDisplay.Time);
        }
        */
        //displayIndex = 0;
        active = true;
        menuIndex = 0;
        visualSelectIndex = menuIndex;

        menuBaseO = Instantiate(MainManager.Instance.menuBase, MainManager.Instance.Canvas.transform);
        descriptionBoxO = Instantiate(MainManager.Instance.descriptionBoxBase, MainManager.Instance.Canvas.transform);
        descriptionBoxScript = descriptionBoxO.GetComponent<DescriptionBoxScript>();
        //menuBaseO.transform.position = new Vector3(250, -70, 0);
        menuEntriesO = new List<GameObject>();

        bm = menuBaseO.GetComponent<BoxMenuScript>();

        //get items from item inventory
        List<KeyItem> inv = itemList; //MainManager.Instance.playerData.itemInventory;
        menuEntries = new BoxMenuEntry[inv.Count];
        for (int i = 0; i < inv.Count; i++)
        {
            if (rightTextList != null)
            {
                menuEntries[i] = new KeyItemMenuEntry(inv[i], rightTextList[i]);
            }
            else
            {
                menuEntries[i] = new KeyItemMenuEntry(inv[i]);
            }
            if (backgroundColors != null)
            {
                if (i < backgroundColors.Count && backgroundColors[i] != null)
                {
                    menuEntries[i].backgroundColor = backgroundColors[i].Value;
                    menuEntries[i].hasBackground = true;
                }
            }

            if (canUseList != null)
            {
                if (i < canUseList.Count)
                {
                    menuEntries[i].canUse = canUseList[i];
                }
            }

            if (checkCanUse)
            {
                menuEntries[i].canUse = true;
            }

            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        menuTopIndex = 0;
        visualTopIndex = menuTopIndex;
        bm.upArrow.enabled = false; //menuTopIndex > 0;
        bm.downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (descriptorString != null)
        {
            bm.descriptorTextBox.SetText(descriptorString, true);
        }
        else
        {
            bm.descriptorBox.enabled = false;
            bm.descriptorTextBox.gameObject.SetActive(false);
        }

        descriptionBoxScript.SetText(menuEntries[menuIndex].description);
    }
    public override void SelectOption()
    {
        InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
    }
    public override void ZOption()
    {
        if (zTarget)
        {
            menuIndex = -2;
            InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
        }
    }
    public override void Cancel()
    {
        if (!noCancelGoBack)
        {
            PopSelf();
            if (parent == null)
            {
                menuIndex = -1;
                SelectOption();
            }
        }
        else
        {
            menuIndex = -1;
            SelectOption();
        }
    }
    /*  
    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }
    */
    public override MenuResult GetResult()
    {
        //        
        if (menuIndex < 0)
        {
            return new MenuResult("cancel," + menuIndex);
        }

        string s = (((KeyItemMenuEntry)(menuEntries[menuIndex])).kitem.type.ToString()) + "," + menuIndex;
        return new MenuResult(s);
    }
}

//for displaying whatever you want (just provide the right strings and stuff)
public class GenericBoxMenu : BoxMenu
{
    public List<string> dataList;
    public List<string> rightTextList;
    public List<bool> usageList;
    public List<string> descriptionList;
    public string levelDescriptor;  //for recontextualizing the "level" for other things (maybe like buying X copies of an item, so this would say something like "qt: ")
    public List<int> maxLevelList;
    public List<Color?> colorList;  //Background colors

    public bool showAtOne = false;

    public bool menuDone = false;

    public bool zSelect = false;
    public bool canZSelect = false;
    //public override event EventHandler<MenuExitEventArgs> menuExit;

    public static GenericBoxMenu BuildMenu(List<string> dataList, List<string> rightTextList = null, bool canUseDisabled = false, bool canZSelect = false, List<bool> usageList = null, List<string> descriptionList = null, bool showAtOne = false, string levelDescriptor = null, string descriptor = null, List<int> maxLevelList = null, List<Color?> colorList = null)
    {
        GameObject newObj = new GameObject("Generic Menu");
        GenericBoxMenu newMenu;

        newMenu = newObj.AddComponent<GenericBoxMenu>();
        newMenu.dataList = dataList;
        newMenu.rightTextList = rightTextList;
        newMenu.usageList = usageList;
        newMenu.descriptionList = descriptionList;
        newMenu.showAtOne = showAtOne;
        newMenu.levelDescriptor = levelDescriptor;
        newMenu.descriptorString = descriptor;
        newMenu.maxLevelList = maxLevelList;
        newMenu.canUseDisabled = canUseDisabled;
        newMenu.canZSelect = canZSelect;
        newMenu.colorList = colorList;

        newMenu.Init();

        return newMenu;
    }

    //Helper methods for different applications

    //Pack up the menu data (note that this is not the full list of arguments to the above thing since the rest are part of the tag itself
    public static string PackMenuString(string levelDescriptor, string descriptor, List<string> entryList, List<string> rightTextList = null, List<bool> usageList = null, List<string> descriptionList = null, List<int> maxLevelList = null, List<Color?> colorList = null)
    {
        string output = "";

        for (int i = 0; i < entryList.Count; i++)
        {
            output += entryList[i];


            if (rightTextList == null && usageList == null && descriptionList == null && maxLevelList == null && colorList == null)
            {
                if (i < entryList.Count - 1)
                {
                    output += "|";
                }
                continue;
            }

            output += "|";

            if (rightTextList == null || i >= rightTextList.Count)
            {
                output += "";
            } else
            {
                output += rightTextList[i];
            }

            if (usageList == null && descriptionList == null && maxLevelList == null && colorList == null)
            {
                if (i < entryList.Count - 1)
                {
                    output += "|";
                }
                continue;
            }

            output += "|";

            if (usageList == null || i >= usageList.Count)
            {
                output += "true";
            }
            else
            {
                output += usageList[i];
            }

            if (descriptionList == null && maxLevelList == null && colorList == null)
            {
                if (i < entryList.Count - 1)
                {
                    output += "|";
                }
                continue;
            }

            output += "|";

            if (descriptionList == null || i >= descriptionList.Count)
            {
                output += "";
            }
            else
            {
                output += descriptionList[i];
            }


            if (maxLevelList == null && colorList == null)
            {
                if (i < entryList.Count - 1)
                {
                    output += "|";
                }
                continue;
            }

            output += "|";

            if (maxLevelList == null || i >= maxLevelList.Count)
            {
                output += "";
            }
            else
            {
                output += maxLevelList[i];
            }

            if (colorList == null)
            {
                if (i < entryList.Count - 1)
                {
                    output += "|";
                }
                continue;
            }

            output += "|";

            if (colorList == null || i >= colorList.Count)
            {
                output += "X";
            }
            else
            {
                if (colorList[i] == null)
                {
                    output += colorList[i];
                }
                else
                {
                    output += colorList[i];
                }
            }

            if (i < entryList.Count - 1)
            {
                output += "|";
            }
        }

        if (levelDescriptor != null)
        {
            output += "|";
            output += levelDescriptor;

            if (descriptor != null)
            {
                output += "|";
                output += descriptor;
            }
        }

        return output;
    }
    public static string PackMenuString(List<Item> list, string levelDescriptor = null, string descriptor = null, List<string> rightTextList = null, List<bool> usageList = null, List<int> maxLevelList = null, List<Color?> colorList = null)
    {
        List<string> names = new List<string>();
        List<string> descriptions = new List<string>();

        for (int i = 0; i < list.Count; i++) {
            names.Add(Item.GetSpriteString(list[i]) + " " + Item.GetName(list[i]));
            descriptions.Add(Item.GetDescription(list[i]));
        }

        return PackMenuString(levelDescriptor, descriptor, names, rightTextList, usageList, descriptions, maxLevelList, colorList);
    }
    public static string PackMenuString(List<KeyItem> list, string levelDescriptor = null, string descriptor = null, List<string> rightTextList = null, List<bool> usageList = null, List<int> maxLevelList = null, List<Color?> colorList = null)
    {
        List<string> names = new List<string>();
        List<string> descriptions = new List<string>();

        for (int i = 0; i < list.Count; i++)
        {
            names.Add(KeyItem.GetSpriteString(list[i]) + " " + KeyItem.GetName(list[i]));
            descriptions.Add(KeyItem.GetDescription(list[i]));
        }

        return PackMenuString(levelDescriptor, descriptor, names, rightTextList, usageList, descriptions, maxLevelList, colorList);
    }
    public static string PackMenuString(List<Badge> list, string levelDescriptor = null, string descriptor = null, List<string> rightTextList = null, List<bool> usageList = null, List<int> maxLevelList = null, List<Color?> colorList = null)
    {
        List<string> names = new List<string>();
        List<string> descriptions = new List<string>();

        for (int i = 0; i < list.Count; i++)
        {
            names.Add(Badge.GetSpriteString(list[i]) + " " + Badge.GetName(list[i]));
            descriptions.Add(Badge.GetDescription(list[i]));
        }

        return PackMenuString(levelDescriptor, descriptor, names, rightTextList, usageList, descriptions, maxLevelList, colorList);
    }
    public static string PackMenuString(List<Ribbon> list, string levelDescriptor = null, string descriptor = null, List<string> rightTextList = null, List<bool> usageList = null, List<int> maxLevelList = null, List<Color?> colorList = null)
    {
        List<string> names = new List<string>();
        List<string> descriptions = new List<string>();

        for (int i = 0; i < list.Count; i++)
        {
            names.Add(Ribbon.GetSpriteString(list[i]) + " " + Ribbon.GetName(list[i]));
            descriptions.Add(Ribbon.GetDescription(list[i]));
        }

        return PackMenuString(levelDescriptor, descriptor, names, rightTextList, usageList, descriptions, maxLevelList, colorList);
    }
    public static string PackMenuString(List<PickupUnion> list, string levelDescriptor = null, string descriptor = null, List<string> rightTextList = null, List<bool> usageList = null, List<int> maxLevelList = null, List<Color?> colorList = null)
    {
        List<string> names = new List<string>();
        List<string> descriptions = new List<string>();

        for (int i = 0; i < list.Count; i++)
        {
            names.Add(PickupUnion.GetSpriteString(list[i]) + " " + PickupUnion.GetName(list[i]));
            descriptions.Add(PickupUnion.GetDescription(list[i]));
        }

        return PackMenuString(levelDescriptor, descriptor, names, rightTextList, usageList, descriptions, maxLevelList, colorList);
    }


    public override void Init()
    {
        lifetime = 0;
        /*
        if (displayModes == null)
        {
            displayModes = new List<ItemMenuEntry.StatDisplay>
            {
                ItemMenuEntry.StatDisplay.Sprite
            };
            //displayModes.Add(ItemMenuEntry.StatDisplay.Time);
        }
        */
        //displayIndex = 0;
        active = true;
        menuIndex = 0;
        visualSelectIndex = menuIndex;

        menuBaseO = Instantiate(MainManager.Instance.menuBase, MainManager.Instance.Canvas.transform);

        if (descriptionList != null)
        {
            descriptionBoxO = Instantiate(MainManager.Instance.descriptionBoxBase, MainManager.Instance.Canvas.transform);
            descriptionBoxScript = descriptionBoxO.GetComponent<DescriptionBoxScript>();
        }
        //menuBaseO.transform.position = new Vector3(250, -70, 0);
        menuEntriesO = new List<GameObject>();

        bm = menuBaseO.GetComponent<BoxMenuScript>();

        //get items from item inventory
        List<string> list = dataList; //MainManager.Instance.playerData.itemInventory;
        menuEntries = new BoxMenuEntry[list.Count];

        //defaults
        string tempRT = null;
        bool tempUsage = true;
        string tempD = null;
        int tempLevel = 1;
        Color? tempColor = null;

        //Debug.Log(list.Count + " " + rightTextList.Count + " " + descriptionList.Count + " " + usageList.Count);
        MainManager.ListPrint(list);
        MainManager.ListPrint(rightTextList);
        MainManager.ListPrint(descriptionList);
        MainManager.ListPrint(usageList);

        for (int i = 0; i < list.Count; i++)
        {
            if (rightTextList != null && i < rightTextList.Count)
            {
                tempRT = FormattedString.ParseEscapeSequences(rightTextList[i]);
            }

            if (descriptionList != null && i < descriptionList.Count)
            {
                tempD = FormattedString.ParseEscapeSequences(descriptionList[i]);
            }

            if (usageList != null && i < usageList.Count)
            {
                tempUsage = usageList[i];
            }

            if (maxLevelList != null && i < maxLevelList.Count)
            {
                tempLevel = maxLevelList[i];
            }

            if (colorList != null && i < colorList.Count)
            {
                tempColor = colorList[i];
            }

            //Debug.Log(list[i] + " " + tempLevel);

            //could convert hasBackground and backgroundColor into a Color? variable but I'm lazy
            menuEntries[i] = new BoxMenuEntry(list[i], tempD, null, tempRT, tempUsage, tempLevel, tempColor.HasValue, tempColor.GetValueOrDefault());

            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i], showAtOne, levelDescriptor);
        }

        menuTopIndex = 0;
        visualTopIndex = menuTopIndex;
        bm.upArrow.enabled = false; //menuTopIndex > 0;
        bm.downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        //Debug.Log(descriptorString);
        if (descriptorString != null)
        {
            bm.descriptorBox.enabled = true;
            bm.descriptorTextBox.SetText(descriptorString, true);
        }
        else
        {
            bm.descriptorBox.enabled = false;
            bm.descriptorTextBox.gameObject.SetActive(false);
        }

        if (descriptionList != null)
        {
            descriptionBoxScript.SetText(menuEntries[menuIndex].description);
        }
    }
    public override void SelectOption()
    {
        InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
    }
    public override void ZOption()
    {
        if (canZSelect)
        {
            PopSelf();
            if (parent == null)
            {
                zSelect = true;
                //menuIndex = -2;
                SelectOption();
            }
        }
    }
    public override void Cancel()
    {
        PopSelf();
        if (parent == null)
        {
            menuIndex = -1;
            SelectOption();
        }
    }

    //To do later: retrieve data somehow from the speaker
    public override void IncrementLevel(int inc)
    {
        if (menuEntries[menuIndex].maxLevel <= 1)
        {
            return;
        }

        menuEntries[menuIndex].level += inc;
        if (menuEntries[menuIndex].level <= 0)
        {
            menuEntries[menuIndex].level += menuEntries[menuIndex].maxLevel;
        }
        menuEntries[menuIndex].level = ((menuEntries[menuIndex].level - 1) % menuEntries[menuIndex].maxLevel) + 1;

        //Somehow acquire more information
        //probably going to use a similar format to singular entries

        //sus
        string message = MainManager.Instance.lastSpeaker.RequestTextData(GetResult().output.ToString());

        if (message.Equals("nop"))
        {
            //do nothing
        } else
        {
            List<string> tempList = MainManager.ParsePipeStringList(message);
            //name, right text, canUse, desc, max level

            if (tempList.Count > 0)
            {
                menuEntries[menuIndex].name = tempList[0];
            }

            if (tempList.Count > 1)
            {
                menuEntries[menuIndex].rightText = tempList[1];
            }

            if (tempList.Count > 2)
            {
                bool.TryParse(tempList[2], out menuEntries[menuIndex].canUse);
                //menuEntries[menuIndex].canUse = tempList[2];
            }

            if (tempList.Count > 3)
            {
                menuEntries[menuIndex].description = tempList[3];
            }

            if (tempList.Count > 4)
            {
                int.TryParse(tempList[4], out menuEntries[menuIndex].maxLevel);
                //menuEntries[menuIndex].maxLevel = tempList[4];
            }
        }


        //rebuild the entry I guess
        BoxMenuEntryScript b = menuEntriesO[menuIndex].GetComponent<BoxMenuEntryScript>();
        b.Setup(menuEntries[menuIndex], showAtOne, levelDescriptor);

        if (descriptionBoxScript != null)
        {
            descriptionBoxScript.SetText(menuEntries[menuIndex].description);
        }
    }

    /*  
    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }
    */
    public override MenuResult GetResult()
    {
        //        
        if (menuIndex == -1)
        {
            return new MenuResult("cancel,-1");
        }

        //Format: thing,level,index
        //If no level list was provided: thing,index

        string s;

        //if Z select: z,(other stuff)
        
        if (zSelect)
        {
            if (maxLevelList != null)
            {
                s = "z," + FormattedString.InsertEscapeSequences(menuEntries[menuIndex].name) + "," + menuEntries[menuIndex].level + "," + menuIndex;
            }
            else
            {
                s = "z," + FormattedString.InsertEscapeSequences(menuEntries[menuIndex].name) + "," + menuIndex;
            }
        }
        else
        {
            if (maxLevelList != null)
            {
                s = FormattedString.InsertEscapeSequences(menuEntries[menuIndex].name) + "," + menuEntries[menuIndex].level + "," + menuIndex;
            }
            else
            {
                s = FormattedString.InsertEscapeSequences(menuEntries[menuIndex].name) + "," + menuIndex;
            }
        }
        return new MenuResult(s);
    }
}

public class OWCharacterBoxMenu : BoxMenu
{
    //CharacterMenuEntry.StatDisplay displayMode;

    public bool menuDone = false;

    public static OWCharacterBoxMenu BuildMenu()
    {
        GameObject newObj = new GameObject("OW Character Menu");
        OWCharacterBoxMenu newMenu;

        newMenu = newObj.AddComponent<OWCharacterBoxMenu>();
        
        newMenu.Init();

        return newMenu;
    }

    public override void Init()
    {
        lifetime = 0;
        //displayMode = CharacterMenuEntry.StatDisplay.Health;

        active = true;
        menuIndex = 0;
        visualSelectIndex = menuIndex;

        menuBaseO = Instantiate(MainManager.Instance.menuBase, MainManager.Instance.Canvas.transform);
        descriptionBoxO = Instantiate(MainManager.Instance.descriptionBoxBase, MainManager.Instance.Canvas.transform);
        descriptionBoxScript = descriptionBoxO.GetComponent<DescriptionBoxScript>();
        //menuBaseO.transform.position = new Vector3(250, -70, 0);
        menuEntriesO = new List<GameObject>();

        bm = menuBaseO.GetComponent<BoxMenuScript>();

        List<PlayerData.PlayerDataEntry> playerDataEntries = MainManager.Instance.playerData.party;
        descriptorString = MainManager.Instance.playerData.ep + "/" + MainManager.Instance.playerData.maxEP + "<ep>, " + MainManager.Instance.playerData.se + "/" + MainManager.Instance.playerData.maxSE + "<se>";

        menuEntries = new BoxMenuEntry[playerDataEntries.Count];
        for (int i = 0; i < playerDataEntries.Count; i++)
        {
            menuEntries[i] = new CharacterMenuEntry(playerDataEntries[i]);
            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        menuTopIndex = 0;
        visualTopIndex = menuTopIndex;
        bm.upArrow.enabled = false; //menuTopIndex > 0;
        bm.downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (descriptorString != null)
        {
            bm.descriptorTextBox.SetText(descriptorString, true);
        }
        else
        {
            bm.descriptorBox.enabled = false;
            bm.descriptorTextBox.gameObject.SetActive(false);
        }

        descriptionBoxScript.SetText(menuEntries[menuIndex].description);
    }
    public override void SelectOption()
    {
        //This is a bad idea
        //MainManager.Instance.lastTextboxMenuResultObject = GetFullResult();
        //Clear();
        InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
    }
    public override void ZOption()
    {
        //increment display mode
        //The getNames way of finding the last enum value doesn't work if enums don't start at 0 and go up by 1 each time
        /*
        if ((int)displayMode == Enum.GetNames(typeof(CharacterMenuEntry.StatDisplay)).Length - 1)
        {
            displayMode = 0;
        }
        else
        {
            displayMode = displayMode + 1;
        }


        List<PlayerData.PlayerDataEntry> playerDataEntries = BattleControl.Instance.playerData.party;

        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i] = new CharacterMenuEntry(playerDataEntries[i], displayMode);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }
        */
    }

    /*
    public override Move GetCurrent()
    {
        return null;
    }
    public override BattleAction GetAction()
    {
        return null;
    }
    public PlayerData.PlayerDataEntry GetPDataEntry()
    {
        List<PlayerData.PlayerDataEntry> playerDataEntries = BattleControl.Instance.playerData.fullParty;
        return playerDataEntries[menuIndex];
    }
    */

    public override MenuResult GetResult()
    {
        List<PlayerData.PlayerDataEntry> playerDataEntries = MainManager.Instance.playerData.party;
        return new MenuResult(playerDataEntries[menuIndex]);
    }
    public override BaseBattleMenu.BaseMenuName GetMenuName() => BaseBattleMenu.BaseMenuName.CharacterMenu;
}

public class PromptBoxMenu : BoxMenu
{
    public Image baseBox;

    public string[][] table; //Create a table of entry-line pairs (choose entry to output a value)
    public int cancelIndex;

    public bool menuDone = false;

    public string topText;

    public float width;
    

    public static PromptBoxMenu BuildMenu(string[] text, string[] args, int p_cancelIndex = -1, string topText = null)
    {
        GameObject newObj = new GameObject("Prompt Box Menu");
        PromptBoxMenu newMenu;
        newMenu = newObj.AddComponent<PromptBoxMenu>();
        newMenu.table = new string[text.Length][];
        newMenu.cancelIndex = p_cancelIndex;
        newMenu.topText = topText;

        for (int i = 0; i < text.Length; i++)
        {
            newMenu.table[i] = new string[2];
            newMenu.table[i][0] = text[i];

            if (i >= args.Length)
            {
                newMenu.table[i][1] = i.ToString();
            }
            else
            {
                newMenu.table[i][1] = args[i];
            }
        }

        newMenu.Init();
        return newMenu;
    }

    
    public override void Init()
    {
        lifetime = 0;
        active = true;
        menuIndex = 0;
        visualSelectIndex = menuIndex;

        menuBaseO = Instantiate(MainManager.Instance.promptMenuBase, MainManager.Instance.Canvas.transform);
        menuEntriesO = new List<GameObject>();

        BoxMenuScript bm = menuBaseO.GetComponent<BoxMenuScript>();
        //Total height = 50 * number of entries


        menuEntries = new BoxMenuEntry[table.Length];
        for (int i = 0; i < table.Length; i++)
        {
            menuEntries[i] = new PromptMenuEntry(table[i][0], table[i][1]);
            GameObject g = Instantiate(MainManager.Instance.promptMenuEntryBase, bm.mask.transform);
            g.transform.localPosition = GetRelativePosition(i);// + Vector3.up * 5;
            menuEntriesO.Add(g);
            PromptMenuEntryScript b = menuEntriesO[i].GetComponent<PromptMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        menuEntriesO[menuIndex].GetComponent<PromptMenuEntryScript>().SetTextWavy();

        float maxWidth = 0;
        for (int i = 0; i < menuEntriesO.Count; i++)
        {
            PromptMenuEntryScript b = menuEntriesO[i].GetComponent<PromptMenuEntryScript>();
            if (maxWidth < b.textMesh.GetRenderedValues()[0] + 35)
            {
                maxWidth = b.textMesh.GetRenderedValues()[0] + 35;
            }
        }

        menuTopIndex = 0;
        visualTopIndex = menuTopIndex;
        Vector3 dimensions = Vector3.up * 40 * table.Length + (maxWidth + 35) * Vector3.right;
        bm.GetComponent<RectTransform>().sizeDelta = dimensions;
        width = dimensions.x;
        bm.menuBox.rectTransform.sizeDelta = dimensions;
        bm.mask.rectTransform.sizeDelta = dimensions - Vector3.right * 10 - Vector3.up * 10;
        if (topText == null || topText.Length == 0)
        {
            bm.descriptorBox.gameObject.SetActive(false);
        } else
        {
            bm.descriptorTextBox.SetText(topText, true, true);
            bm.descriptorBox.rectTransform.anchoredPosition = Vector2.up * (dimensions.y * 0.5f + 35);
        }
        baseBox = bm.menuBox;

        bm.selectorArrow.transform.localPosition = Vector3.left * (25f + width/2) + menuEntriesO[menuIndex].transform.localPosition + Vector3.up * 2.5f;
    }
    public override void Clear()
    {
        active = false;
        Destroy(menuBaseO);
        for (int i = 0; i < menuEntriesO.Count; i++)
        {
            Destroy(menuEntriesO[i]);
        }
    }

    protected override void MenuUpdate()
    {
        lifetime += Time.deltaTime;
        if (bm == null)
        {
            bm = menuBaseO.GetComponent<BoxMenuScript>();
            //Debug.Log("set");
        }
        if (Mathf.Sign(InputManager.GetAxisVertical()) != inputDir || InputManager.GetAxisVertical() == 0)
        {
            holdDur = 0;
            holdValue = 0;
            inputDir = Mathf.Sign(InputManager.GetAxisVertical());
            if (InputManager.GetAxisVertical() == 0)
            {
                inputDir = 0;
            }
            //Debug.Log(InputManager.GetAxisHorizontal());
            //now go
            if (inputDir != 0)
            {
                //Reset
                if (table.Length > 1)
                {
                    menuEntriesO[menuIndex].GetComponent<PromptMenuEntryScript>().ResetText();
                }

                //inputDir positive = up and - index, negative = down and + index
                if (inputDir > 0)
                {
                    menuIndex--;
                }
                else
                {
                    menuIndex++;
                }
            }

            if (menuIndex > menuEntries.Length - 1)
            {
                menuIndex = 0;
            }
            if (menuIndex < 0)
            {
                menuIndex = menuEntries.Length - 1;
            }


            if (menuTopIndex > menuIndex)
            {
                menuTopIndex = menuIndex;
            }
            if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
            {
                menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
            }

            PromptMenuEntryScript p = menuEntriesO[menuIndex].GetComponent<PromptMenuEntryScript>();
            if (!p.wavy)
            {
                p.SetTextWavy();
            }

            //BoxMenuScript bm = menuBaseO.GetComponent<BoxMenuScript>();
            //bm.upArrow.enabled = menuTopIndex > 0;
            //bm.downArrow.enabled = menuTopIndex < menuEntries.Length - 6 && menuEntries.Length > 6;
        }
        if (Mathf.Sign(InputManager.GetAxisVertical()) == inputDir && InputManager.GetAxisVertical() != 0)
        {
            holdDur += Time.deltaTime;

            if (holdDur >= HYPER_SCROLL_TIME)
            {
                int pastHoldValue = holdValue;

                //note: hardcoded 30 so you scroll 30 per second
                //should be fast enough to be useful in big menus but also slow enough to stop on a specific number relatively easily (or get close easily)
                if (MainManager.Instance.GetHyperScrollRate() * (holdDur - HYPER_SCROLL_TIME) > holdValue)
                {
                    holdValue = (int)(MainManager.Instance.GetHyperScrollRate() * (holdDur - HYPER_SCROLL_TIME));
                }

                //Reset
                if (table.Length > 1)
                {
                    menuEntriesO[menuIndex].GetComponent<PromptMenuEntryScript>().ResetText();
                }

                if (inputDir > 0)
                {
                    menuIndex -= (holdValue - pastHoldValue);
                }
                else
                {
                    menuIndex += (holdValue - pastHoldValue);
                }

                //No loop around
                if (menuIndex > menuEntries.Length - 1)
                {
                    menuIndex = menuEntries.Length - 1;
                }
                if (menuIndex < 0)
                {
                    menuIndex = 0;
                }

                if (menuTopIndex > menuIndex)
                {
                    menuTopIndex = menuIndex;
                }
                if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
                {
                    menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
                }

                PromptMenuEntryScript p = menuEntriesO[menuIndex].GetComponent<PromptMenuEntryScript>();
                if (!p.wavy)
                {
                    p.SetTextWavy();
                }
            }
        }

        if (visualTopIndex != menuTopIndex)
        {
            visualTopIndex = MainManager.EasingQuadraticTime(visualTopIndex, menuTopIndex, 100);
            for (int i = 0; i < menuEntriesO.Count; i++)
            {
                //menuEntriesO[i].transform.localPosition = GetRelativePosition(i - menuTopIndex) + Vector3.up * 5;
                menuEntriesO[i].transform.localPosition = GetRelativePosition(i - visualTopIndex);// + Vector3.up * 5;
            }
        }

        if (menuEntries.Length > 0)
        {
            visualSelectIndex = MainManager.EasingQuadraticTime(visualSelectIndex, menuIndex, 400);

            if (visualSelectIndex < visualTopIndex)
            {
                visualSelectIndex = visualTopIndex;
            }
            if (visualSelectIndex > visualTopIndex + MENU_SIZE_PER_PAGE - 1)
            {
                visualSelectIndex = visualTopIndex + MENU_SIZE_PER_PAGE - 1;
            }
            Vector3 next = Vector3.left * (25f + width / 2) + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * 2.5f;
            bm.selectorArrow.transform.localPosition = next;

            //bm.selectorArrow.transform.localPosition = targetLocal;
            //bm.selectorArrow.transform.localPosition = Vector3.left * 150f + menuEntriesO[menuIndex].transform.localPosition + Vector3.up * 2.5f;
        }

        if (InputManager.GetButtonDown(InputManager.Button.A) && menuEntries[menuIndex].canUse && menuEntries.Length > 0 && lifetime > MIN_SELECT_TIME) //Press A to select stuff
        {
            SelectOption();
        }
        if (InputManager.GetButtonDown(InputManager.Button.B) && cancelIndex != -1 && lifetime > MIN_SELECT_TIME)
        {
            menuIndex = cancelIndex;
            SelectOption();
        }
        //B press = use cancel option if possible
    }

    public override void SelectOption()
    {
        //Debug.Log(table[menuIndex][0]);
        InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
        menuDone = true;
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(menuEntries[menuIndex].rightText);
    }

    Vector3 GetRelativePosition(float i)
    {
        int max = table.Length;
        return ((max - 1) * Vector3.up * 18) + Vector3.down * i * 36;
    }
}