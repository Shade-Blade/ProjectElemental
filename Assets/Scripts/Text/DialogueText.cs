using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

public class TMPString
{
    public enum ColorNames
    {
        Red,
        Orange,
        Yellow,
        Green,
        Cyan,
        Blue,
        Purple,
        Magenta,
        White,
        LightGray,
        DarkGray,
        Black
    }
    public static Color[] baseColors = new Color[]
    {
        new Color(1,0,0,1),
        new Color(1,0.5f,0,1),
        new Color(1,1,0,1),
        new Color(0,1,0,1),
        new Color(0,1,1,1),
        new Color(0,0,1,1),
        new Color(0.5f,0,1,1),
        new Color(1,0,1,1),
        new Color(0.66f,0.66f,0.66f,1),
        new Color(0.33f,0.33f,0.33f,1),
        new Color(0,0,0,1),
    };

    string cleanString;
    string formattedString;
    TagEntry[] tags;

    public static string ConvertSymbolTag(TagEntry tag)
    {

        string output = "";
        switch (tag.tag)
        {
            case TagEntry.TextTag.ZeroSpace:
                output = "\u200B";
                break;
            case TagEntry.TextTag.CArrow:
                output = "\u21BB";
                break;
            case TagEntry.TextTag.LArrow:
                output = "\u2190";
                break;
            case TagEntry.TextTag.RArrow:
                output = "\u2192";
                break;
            case TagEntry.TextTag.UArrow:
                output = "\u2191";
                break;
            case TagEntry.TextTag.DArrow:
                output = "\u2193";
                break;


            case TagEntry.TextTag.LRArrow:
                output = "\u2194";
                break;
            case TagEntry.TextTag.UDArrow:
                output = "\u2195";
                break;
            case TagEntry.TextTag.ULArrow:
                output = "\u2196";
                break;
            case TagEntry.TextTag.URArrow:
                output = "\u2197";
                break;
            case TagEntry.TextTag.DRArrow:
                output = "\u2198";
                break;
            case TagEntry.TextTag.DLArrow:
                output = "\u2199";
                break;

            case TagEntry.TextTag.Star:
                output = "\u2605";
                break;
            case TagEntry.TextTag.EmptyStar:
                output = "\u2604";
                break;
            case TagEntry.TextTag.Male:
                output = "\u2642";
                break;
            case TagEntry.TextTag.Female:
                output = "\u2640";
                break;
            case TagEntry.TextTag.Heart:
                output = "\u2665";
                break;
            case TagEntry.TextTag.EmptyHeart:
                output = "\u2661";
                break;
            case TagEntry.TextTag.QuarterNote:
                output = "\u2669";
                break;
            case TagEntry.TextTag.EighthNote:
                output = "\u266A";
                break;
            case TagEntry.TextTag.TwoEighthNotes:
                output = "\u266B";
                break;
            case TagEntry.TextTag.TwoSixteenthNotes:
                output = "\u266C";
                break;
            case TagEntry.TextTag.Flat:
                output = "\u266D";
                break;
            case TagEntry.TextTag.Natural:
                output = "\u266E";
                break;
            case TagEntry.TextTag.Sharp:
                output = "\u266F";
                break;
            case TagEntry.TextTag.Infinity:
                output = "\u221E";
                break;
        }
        return output;
    }

    //convert to the formattedString format
    public static string ConvertTagEntry(TagEntry tag)
    {
        //Note: higher layers may already work with these tags, but this layer should get rid of any tags that don't mean anything at this stage

        string output = "";
        switch (tag.tag)
        {
            case TagEntry.TextTag.Arg:
            case TagEntry.TextTag.GlobalVar:
            case TagEntry.TextTag.GlobalFlag:
            case TagEntry.TextTag.AreaVar:
            case TagEntry.TextTag.AreaFlag:
            case TagEntry.TextTag.MapVar:
            case TagEntry.TextTag.MapFlag:
            case TagEntry.TextTag.Const:
                output = FormattedString.ParseNonlocalVars(tag.ToString());
                break;
            case TagEntry.TextTag.System:
                output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold ColorMatchDark Outline\"><color=#ffffff>";
                break;
            case TagEntry.TextTag.Outline:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold Outline\"><color=#ffffff>";
                        break;
                    }
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }

                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold Outline\"><color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.GrayOutline:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold Gray Outline\"><color=#ffffff>";
                        break;
                    }
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }

                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold Gray Outline\"><color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.ColorDarkOutline:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold ColorMatchDark Outline\"><color=#ffffff>";
                        break;
                    }
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }

                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold ColorMatchDark Outline\"><color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.ColorOutline:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold ColorMatch Outline\"><color=#ffffff>";
                        break;
                    }
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }

                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold ColorMatch Outline\"><color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.Underlay:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        break;
                    }
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }

                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold Underlay\"><color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.ChaoticUnderlay:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        break;
                    }
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }

                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold Chaotic\"><color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.Metallic:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        break;
                    }
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }

                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold Metallic\"><color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.Iridescent:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        break;
                    }
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }

                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold Iridescent\"><color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.Effect:
            case TagEntry.TextTag.EffectSprite:
                /*
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<color=#ffffff><sprite=\"EffectIconsV7\" index=" + tag.args[0] + " color=#ffffff></color>";
                break;
                */
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_EffectSprite.GetEffectWidth(tag.args[0]) + "><size=1%>I</size>";
                break;
            case TagEntry.TextTag.State:
            case TagEntry.TextTag.StateSprite:
                /*
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<color=#ffffff><sprite=\"StateIconsV2\" index=" + tag.args[0] + " color=#ffffff></color>";
                break;
                */
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_StateSprite.GetStateWidth(tag.args[0]) + "><size=1%>I</size>";
                break;
            case TagEntry.TextTag.Button: //spawns 2 zero width spaces to left and right due to how I set them up
            case TagEntry.TextTag.ButtonSprite: //spawns 2 zero width spaces to left and right due to how I set them up
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_ButtonSprite.GetButtonWidth(tag.args[0]) + "><size=1%>I</size>";
                //output = "z";
                break;
            case TagEntry.TextTag.Item:
            case TagEntry.TextTag.ItemSprite:
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_ItemSprite.GetItemWidth(tag.args[0]) + "><size=1%>I</size>";
                //output = "z";
                break;
            case TagEntry.TextTag.KeyItem:
            case TagEntry.TextTag.KeyItemSprite:
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_KeyItemSprite.GetKeyItemWidth(tag.args[0]) + "><size=1%>I</size>";
                //output = "z";
                break;
            case TagEntry.TextTag.Badge:
            case TagEntry.TextTag.BadgeSprite:
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_BadgeSprite.GetBadgeWidth(tag.args[0]) + "><size=1%>I</size>";
                //output = "z";
                break;
            case TagEntry.TextTag.Ribbon:
            case TagEntry.TextTag.RibbonSprite:
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_RibbonSprite.GetRibbonWidth(tag.args[0]) + "><size=1%>I</size>";
                //output = "z";
                break;
            case TagEntry.TextTag.HP:
            case TagEntry.TextTag.EP:
            case TagEntry.TextTag.SE:
            case TagEntry.TextTag.SP:
            case TagEntry.TextTag.Stamina:
            case TagEntry.TextTag.Carrot:
            case TagEntry.TextTag.Clock:
            case TagEntry.TextTag.Coin:
            case TagEntry.TextTag.SilverCoin:
            case TagEntry.TextTag.GoldCoin:
            case TagEntry.TextTag.Shard:
            case TagEntry.TextTag.XP:
            case TagEntry.TextTag.AstralToken:
            case TagEntry.TextTag.Common:
            case TagEntry.TextTag.CommonSprite:
                if (tag.tag != TagEntry.TextTag.Common && tag.tag != TagEntry.TextTag.CommonSprite)
                {
                    output = "<size=1%>I</size><space=" + Text_CommonSprite.GetWidth(tag.tag.ToString()) + "><size=1%>I</size>";
                    break;
                }
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_CommonSprite.GetWidth(tag.args[0]) + "><size=1%>I</size>";
                //output = "zz";
                break;
            case TagEntry.TextTag.Misc:
            case TagEntry.TextTag.MiscSprite:
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<size=1%>I</size><space=" + Text_MiscSprite.GetMiscWidth(tag.args[0]) + "><size=1%>I</size>";
                //output = "z";
                break;
            case TagEntry.TextTag.ZeroSpace:
            case TagEntry.TextTag.CArrow:
            case TagEntry.TextTag.LArrow:
            case TagEntry.TextTag.RArrow:
            case TagEntry.TextTag.UArrow:
            case TagEntry.TextTag.DArrow:
            case TagEntry.TextTag.LRArrow:
            case TagEntry.TextTag.UDArrow:
            case TagEntry.TextTag.ULArrow:
            case TagEntry.TextTag.URArrow:
            case TagEntry.TextTag.DRArrow:
            case TagEntry.TextTag.DLArrow:
            case TagEntry.TextTag.Star:
            case TagEntry.TextTag.EmptyStar:
            case TagEntry.TextTag.Male:
            case TagEntry.TextTag.Female:
            case TagEntry.TextTag.Heart:
            case TagEntry.TextTag.EmptyHeart:
            case TagEntry.TextTag.QuarterNote:
            case TagEntry.TextTag.EighthNote:
            case TagEntry.TextTag.TwoEighthNotes:
            case TagEntry.TextTag.TwoSixteenthNotes:
            case TagEntry.TextTag.Flat:
            case TagEntry.TextTag.Natural:
            case TagEntry.TextTag.Sharp:
            case TagEntry.TextTag.Infinity:
                output = ConvertSymbolTag(tag);
                break;
            case TagEntry.TextTag.HighlightYesColor:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkGreen Outline + Overlay\"><color=#00ff00>";
                }
                break;
            case TagEntry.TextTag.HighlightNoColor:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkRed Outline + Overlay\"><color=#ff0000>";
                }
                break;
            case TagEntry.TextTag.HighlightDangerColor:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkOrange Outline + Overlay\"><color=#ff8000>";
                }
                break;
            case TagEntry.TextTag.DescriptionWarnColor:
                if (!tag.open)
                {
                    output = "</color>";
                }
                else
                {
                    output = "<color=#e01000>";
                }
                break;
            case TagEntry.TextTag.DescriptionCautionColor:
                if (!tag.open)
                {
                    output = "</color>";
                }
                else
                {
                    output = "<color=#e0e000>";
                }
                break;
            case TagEntry.TextTag.DescriptionNoticeColor:
                if (!tag.open)
                {
                    output = "</color>";
                }
                else
                {
                    output = "<color=#2D48CF>";
                }
                break;
            case TagEntry.TextTag.DescriptionFluffColor:
                if (!tag.open)
                {
                    output = "</color>";
                }
                else
                {
                    output = "<color=#808080>";
                }
                break;
            case TagEntry.TextTag.ExhaustColor:
                if (!tag.open)
                {
                    output = "</color>";
                }
                else
                {
                    output = "<color=" + MoveBoxMenu.ExhaustColor() + ">";
                }
                break;
            case TagEntry.TextTag.DebtColor:
                if (!tag.open)
                {
                    output = "</color>";
                }
                else
                {
                    output = "<color=" + MoveBoxMenu.DebtColor() + ">";
                }
                break;
            case TagEntry.TextTag.HighlightHPColor:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkGreen Outline + Overlay\"><color=#00ff00>";
                }
                break;
            case TagEntry.TextTag.HighlightEPColor:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkYellow Outline + Overlay\"><color=#ffff00>";
                }
                break;
            case TagEntry.TextTag.HighlightSEColor:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkMagenta Outline + Overlay\"><color=#ff00ff>";
                }
                break;
            case TagEntry.TextTag.HighlightSTColor:
                if (!tag.open)
                {
                    output = "</color></font>";
                }
                else
                {
                    output = "<font=\"ShantellSans-Bold SDF\" material=\"ShantellSans-Bold DarkMint Outline + Overlay\"><color=#a0ffa0>";
                }
                break;
            case TagEntry.TextTag.Sprite:
                if (tag.args == null || tag.args.Length == 0)
                {
                    break;
                }
                output = "<sprite";

                //hacky
                /*
                if (tag.args.Length > 2)
                {
                    if (!tag.args[2].Equals(""))
                    {
                        output = "<color=" + tag.args[2] + "><sprite";
                    }
                }
                */

                if (tag.args.Length > 0)
                {
                    if (!tag.args[0].Equals(""))
                    {
                        output += "=" + tag.args[0];
                    }
                }
                if (tag.args.Length > 1)
                {
                    if (!tag.args[1].Equals(""))
                    {
                        output += " name=" + tag.args[1];
                    }
                }
                if (tag.args.Length > 2)
                {
                    if (!tag.args[2].Equals(""))
                    {
                        output += " color=" + tag.args[2];
                    }
                }
                if (tag.args.Length > 3)
                {
                    if (!tag.args[3].Equals(""))
                    {
                        output += " index=" + tag.args[3];
                    }
                }
                output += ">";

                //hacky
                /*
                if (tag.args.Length > 2)
                {
                    if (!tag.args[2].Equals(""))
                    {
                        output += "</color>";
                    }
                }
                */

                break;
            case TagEntry.TextTag.Line:
                output = "\n";
                break;
            case TagEntry.TextTag.Color:
                if (!tag.open)
                {
                    output = "</color>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        break;
                    }
                    //unnecessary?
                    /*
                    ColorNames color;
                    if (Enum.TryParse(tag.args[0], out color))
                    {
                        tag.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }
                    */
                    output = "<color=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.Size:
                if (!tag.open)
                {
                    output = "</size>";
                }
                else
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        break;
                    }
                    output = "<size=" + tag.args[0] + ">";
                }
                break;
            case TagEntry.TextTag.NoBreak:
                if (tag.open)
                {
                    output = "<nobr>";
                }
                else
                {
                    output = "</nobr>";
                }
                break;
            case TagEntry.TextTag.Bold:
                if (tag.open)
                {
                    output = "<b>";
                }
                else
                {
                    output = "</b>";
                }
                break;
            case TagEntry.TextTag.Italics:
                if (tag.open)
                {
                    output = "<i>";
                }
                else
                {
                    output = "</i>";
                }
                break;
            case TagEntry.TextTag.Strikethrough:
                if (tag.open)
                {
                    output = "<s>";
                }
                else
                {
                    output = "</s>";
                }
                break;
            case TagEntry.TextTag.Underline:
                if (tag.open)
                {
                    output = "<u>";
                }
                else
                {
                    output = "</u>";
                }
                break;
            case TagEntry.TextTag.VOffset:
                if (tag.open)
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        output = "<voffset>";
                        break;
                    }
                    output = "<voffset=" + tag.args[0] + ">";
                }
                else
                {
                    output = "</voffset>";
                }
                break;
            case TagEntry.TextTag.Space:
                if (tag.open)
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        output = "<space>";
                        break;
                    }
                    output = "<space=" + tag.args[0] + ">";
                }
                else
                {
                    output = "</space>";
                }
                break;
            case TagEntry.TextTag.Align:
                if (tag.open)
                {
                    if (tag.args == null || tag.args.Length == 0)
                    {
                        output = "<align>";
                        break;
                    }
                    output = "<align=" + tag.args[0] + ">";
                }
                else
                {
                    output = "</align>";
                }
                break;
        }
        return output;
    }
    public override string ToString()
    {
        return formattedString;
    }

    public TMPString(string s, List<TagEntry> tags) : this(s, tags.ToArray())
    {
        
    }
    public TMPString(string s, TagEntry[] tags)
    {
        cleanString = (string)s.Clone(); //FormattedString.StripTags(s);
        this.tags = tags;
        formattedString = (string)cleanString.Clone();

        List<TagEntry> tagsList = new List<TagEntry>(tags);
        for (int i = 0; i < tagsList.Count; i++)
        {
            tagsList[i].trueStartIndex = i;
        }
        tagsList.Sort((a, b) => (a.startIndex == b.startIndex ? a.trueStartIndex - b.trueStartIndex : a.startIndex - b.startIndex));

        int splitIndex = -1;
        for (int i = tagsList.Count-1; i >= 0; i--)
        {
            splitIndex = tagsList[i].startIndex;
            if (splitIndex < 0 || splitIndex > formattedString.Length)
            {
                throw new ArgumentException("Invalid startindex in tag of "+splitIndex + " vs "+ formattedString.Length);
            }
            if (splitIndex == formattedString.Length)
            {
                formattedString = formattedString + ConvertTagEntry(tagsList[i]);
            }
            else
            {
                formattedString = formattedString.Substring(0, splitIndex) + ConvertTagEntry(tagsList[i]) + formattedString.Substring(splitIndex);
            }
        }        
    }
    public TMPString(FormattedString s) : this(s.GetCleanString(), s.tags)
    {

    }
    public TMPString(string s) : this(new FormattedString(s))
    {

    }
}

public class TagEntry
{
    //Important todo: actually document these
    //Possibly refactor stuff (but refactoring this is hard, and the lower levels seem fine)
    public enum TextTag
    {
        Null,      //do nothing (also used for invalid tags)

        //Text File replacement tags
        MenuText,
        CommonText,
        SystemText,
        ItemText,   //reads from the item text file (item names, descriptions...) (usage: itemtext,[item enum],[index to read or name, desc])
        KeyItemText,    //same as ItemText (same usage too)
        LocalText,  //Attempts to read from the mapscript's text file (note: no recursion because this is only parsed once)

        //Variable Tags (Note: variables values are set as soon as it's put into the text displayer, so having a set variable tag right before a variable might not work)
        Var,       //String var (This is used with specific strings where the var is known and is set right before string displayed)
        Arg,        //Slightly more useful than Var since menu result is easier to change and stuff (though this uses the global menu result thing, note that set and setvar change the local one but that also sets the global one)
        GlobalFlag,//Global flags
        GlobalVar, //Global vars
        AreaFlag,   //Map flags
        AreaVar,    //Map variable
        MapFlag,   //Map flags
        MapVar,    //Map variable
        Const,     //Named constants that aren't considered vars of some kind (these are more like internal variables)

        Set,   //Set variables and flags (first arg is var type, next is index, next is new value) (also works with "arg")
        SetVar,         //Set var to another var (allows "arg" to access textbox menu result) (cannot set "arg" to itself)

        DataGet,    //Get data from the speaker by sending it a string (Args: [argX], [request] OR [MEID], [argX], [request])
        DataSend,   //Send data to the speaker in a string (Args: [Data] OR [MEID], [DATA]) (note that [MEID] can be o, l, w as special shorthand)

        //Text Control Tags
        Next,      //Used for mult-text boxes
        End,        //Force closes the text (really should be using this at the end)
        Goto,       //Only works when text files are available, goes to a certain line (goto,arg is also used for prompt and item menu stuff)
        Branch,     //Conditional goto (if var = ???, go somewhere) Note that "arg" is set to be equal to the menu result in textbox scripts
                    //= Branch, [branch instruction], [variable, "arg" or var type + index], [other var or immediate value], [branch destination line number and x]
        CondEnd,    //Conditional end tag (Uses same structure as branch)
                        //Currently acts as normal next tag if condition is false
        CondCut,        //Future replacement for Branch and Goto since there are situations where my current setup make them not work


        ShowCoins,  //show coin display
        HideCoins,  //hide coin display

        //Minibubbles
        Minibubble, //Makes a minibubble args: [mode bool, whether it is detached or not (Detached means it acts like a small independent text box, Non-detached means it gets closed when going to the next text box)], [text, use shorthand but can handle literals], [float in attached mode (range -1,1) OR int (meid) in detached]), [float only read if attached mode is forced on due to nonexistent meid]
            //note that currently the minibubble has a restricted number of textbox tags it supports natively
            //All TextDisplayer tags work since it uses a TextDisplayer
        KillMinibubbles,    //Destroy all minibubbles associated with this textbox
        WaitMinibubbles,    //Scrolling is forbidden until the minibubbles are all done (avoid overusing this because this prevents you from fast scrolling)

        //Menu Tags
        //Note: You should put these right before the end or right before a <next> tag (i.e. a scenario where they are at the end of the scrolling)
        //(don't know what happens if you don't, might still work fine)

        Prompt,     //Spawn a prompt menu (each argument pair is text and an argument, optional odd argument to determine where to go when B is pressed (0 indexed value into the list of options) (Special value -1: makes cancel give a value of -1) (If not present: can't cancel)
        ItemMenu,   //Spawn an item menu (1st arg = arg (set this to have a comma separated list of items to show, or a item number pair?)). 2nd arg = mode. Remove items using the passed result (which is the selected item)
        //To do later: KeyItemMenu, BadgeMenu, etc
        KeyItemMenu,    //similar logic to itemMenu (though I didn't put in as many options)
        NumberMenu,     //select a number (args = min, start num, max)
        TextEntryMenu,  //text entry
        GenericMenu,    //generic menu (1st arg = arg (some number of lists of menu arguments), 2nd arg = number of arguments per menu entry, 3rd arg = show at one (bool), 4th arg = use disabled (bool), 5th arg = has level descriptor at end of arg (bool))
        //  GenericMenu arg format: (entries) ... [level descriptor], [menu descriptor]
        //  Each entry = entry text, right text, canuse, desc, max level, background color
        //      Arg is pipe separated

        //Item Tags
        RemoveItem, //Remove item of given string type (1st arg = item string (or inventory index, or arg to get inventory index from item menu), 2nd arg (t/f) => normal items / key items, 3rd arg (t/f) => remove all of that type))
        //to do: removeKeyItem, removeBadge...

        //Tail Tags
        Tail,      //Move tail around (Changes the speaker object) (Can either use IDs or Vector3 positions)
        TailRealTimeUpdate,     //Does tail constantly move to speaker.GetTextTailPosition()? (use for if the speaker is moving around while talking, or if the camera is moving while the text is open)
        //Note: tail,keru changes box style

        //other speaker related things
        Anim,
        AnimData,

        Face,       //target, target to face towards (1 entityid or 3 args for a vector3)
        Emote,      //target, emote

        //Box and Tail style tags
        BoxStyle,   //(changes box border)
        BoxColor,   //(inner color, resets to default if 0 args are given), (border color, uses inner color if only 1 arg is given) also changes tail color for consistency

        Sign,   //special preset boxstyle and boxcolor (also applies NoScroll)
        System, //likewise
        KeruDistant,    //note that tail,keru should be used in most cases instead (Use this if you manually want the text to have keru's style of text)   
        BoxReset,  //reset style and color

        //Scroll tags
        NoScroll,  //Reveal all text instantly (same as infinite scroll speed)
        Scroll,    //Modify default scroll speed (and possibly prevent you from fast scrolling)
        Wait,      //Scroll wait (same as a weird combination of scroll speed changes)

        //New effect tags (Handled by TextDisplayer) (Has arguments for controlling how strong effects are)
        Rainbow,   //Makes text rainbow colored
        Wavy,      //Makes text wavy
        Shaky,     //Makes text shaky
        Scramble,    //scramble text (randomly mirrors it around)
        Jump,   //Jumping text (args: [jump vel], [jump time])

        //fadeins (args: [timescale]...)
        FadeInShrink,   //...[startsize mult]
        FadeInGrow,     //...no args (because startsize is 0)
        FadeInSpin,     //...[start spin mult]
        FadeInAppear,       //... [colorA r, g, b], [colorB r, g, b] (Get both colors)
        FadeInWave,     //...[start distance], [start omega], [omega offset], [omega offset per char]       (for no rotation, set [start omega] = 0)


        //things that use materials
        Outline,    //outline text
        GrayOutline,    //outline text
        ColorDarkOutline,    //outline text
        ColorOutline,    //outline text
        Underlay,   //underlay text
        ChaoticUnderlay,    //chaotic underlay
        Metallic,   //metallic
        Iridescent, //Normal color should be about = 0.4, 0.4, 0.4 (Too high = becomes pure white)

        //special sprites (either they use the sprite tag or spawn separate things that have to attach to the text displayer)
        //new thing is that they all use the SpecialSprite system for consistency (Also using the textmeshpro sprites is hard to change them)
        EffectSprite,   //icons of effects in battle
        StateSprite,
        ButtonSprite,   //Button sprite (this adds a lot more complexity as button sprites are separate objects!)
        ItemSprite,     //Item sprite (similar to buttonsprites) (currently will only handle ItemTypes and not any special item data)
        KeyItemSprite,  //(basically just copy pastes of ItemSprite stuff)
        BadgeSprite,    //(...)
        RibbonSprite,
        CommonSprite,
        MiscSprite,

        //Equivalents to above
        Effect,
        State,
        Button,
        Item,
        KeyItem,
        Badge,
        Ribbon,
        Common,
        Misc,

        //hacky fix for certain things
        ZeroSpace,  //zero width space

        //arrow characters
        CArrow, //circle arrow
        LArrow,
        RArrow,
        UArrow,
        DArrow,

        LRArrow,
        UDArrow,
        ULArrow,
        URArrow,
        DRArrow,
        DLArrow,

        //various other characters (can be inserted literally also but that can be a bit annoying)
        Star,
        EmptyStar,
        Male,
        Female,
        Heart,          //Note: Use <HP> if you mean hp, but you might want a normal heart instead
        EmptyHeart,
        QuarterNote,
        EighthNote,
        TwoEighthNotes,
        TwoSixteenthNotes,
        Flat,
        Natural,
        Sharp,
        Infinity,

        //Shorthand for <Common, [x]>
        HP,
        EP,
        SE,
        SP,
        Stamina,
        Carrot,
        Clock,
        Coin,
        SilverCoin,
        GoldCoin,
        Shard,
        XP,
        AstralToken,

        //Standardized colors for certain situations
        HighlightYesColor,      //green
        HighlightNoColor,       //red
        HighlightDangerColor,   //orange
        DescriptionWarnColor,   //red
        DescriptionCautionColor,   //yellow
        DescriptionNoticeColor, //blue
        DescriptionFluffColor,  //gray
        ExhaustColor,   //dark orange
        DebtColor,   //bright orange
        HighlightHPColor,
        HighlightEPColor,
        HighlightSEColor,
        HighlightSTColor,

        //Old effect tags (things that are handled by TMPro mostly) (these get converted to proper tags)
        Sprite,    //Display a sprite (that is inside the sprites folder)
        Line,      //Alternate form of \n (use instead of \n)
        Color,      //set text color (arg = color to set to)
        Size,      //Relative size
        NoBreak,   //prevent lines from breaking up text
        Bold,      //bold
        Italics,   //italics
        Strikethrough, //strikethrough
        Underline,  //underline
        VOffset,    //vertical offset
        Space,      //creates horizontal space
        Align,      //Aligns text (Only one per line will work) (Unknown if this works with text scrolling. Likely yes?)
    }

    public enum Branch
    {
        EQI, //equal to immediate value
        NEQI, //not equal to immediate value
        LTI, //less than immediate value
        LTEI, //less than or equal to immediate value
        GTI,
        GTEI,
        EQ,
        NEQ,
        LT,
        LTE,
        GT,
        GTE,

        //1 arg branch statements (these are shorthands though)
        HITEM,  //has item
        NHITEM, //not has item
        HKITEM, //has key item
        NHKITEM, //not has key item

        HPARTY, //has party member
        NHPARTY,    //not has party member

        //0 args
        HKERU,
        NHKERU,
    }

    public enum BoxStyle
    {
        Default,
        Outline,
        DarkOutline,
        LightOutline,
        FancyOutline,
        Shaded,
        Paper,
        Beads,
        System
    }

    public enum Emote
    {
        Alert,
        Question,
        AngryFizzle
    }

    public TextTag tag;
    public int trueStartIndex = -1; //index in original string
    public int trueEndIndex = -1; //where closing > is
    public int startIndex = -1; //indices are calculated based on the tagless string (Start index is immediately before the character ("e<" would have an index of 0))
    public bool open = true; //opening tag or closing tag? (default is true)
    public string[] args;

    public TagEntry() : this(TextTag.Null, 0, 0, 0)
    {

    }
    public TagEntry(TextTag p_tag, int p_startIndex, int p_trueStartIndex = -1, int p_trueEndIndex = -1)
    {
        tag = p_tag;
        trueStartIndex = p_trueStartIndex;
        trueEndIndex = p_trueEndIndex;
        startIndex = p_startIndex;
        open = true;
        args = new string[0];
    }
    public TagEntry(TextTag p_tag, int p_startIndex, bool p_open, int p_trueStartIndex = -1, int p_trueEndIndex = -1)
    {
        tag = p_tag;
        trueStartIndex = p_trueStartIndex;
        trueEndIndex = p_trueEndIndex;
        startIndex = p_startIndex;
        open = p_open;
        args = new string[0];
    }
    public TagEntry(TextTag p_tag, int p_startIndex, bool p_open, string[] p_args, int p_trueStartIndex = -1, int p_trueEndIndex = -1)
    {
        tag = p_tag;
        trueStartIndex = p_trueStartIndex;
        trueEndIndex = p_trueEndIndex;
        startIndex = p_startIndex;
        open = p_open;
        args = p_args;
    }

    public static bool TryParse(string str, out TagEntry t)
    {
        t = new TagEntry();
        string tag = (string)str.Clone();
        if (tag.Length < 2)
        {
            return false;
        }
        if (tag[0] != '<' || tag[tag.Length - 1] != '>')
        {
            return false;
        }
        tag = tag.Substring(1, tag.Length - 2); //get rid of < and >

        if (tag[0] == '/')
        {
            t.open = false;
            tag = tag.Substring(1);
        }

        //(?<!\\), does not seem to work
        string[] split = System.Text.RegularExpressions.Regex.Split(tag, ","); //,

        //new escape sequences (so you can use , in tags)
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = FormattedString.ParseEscapeSequences(split[i]);
        }


        t.tag = TextTag.Null;
        TextTag test = TextTag.Null;
        if (Enum.TryParse(split[0], true, out test))
        {
            t.tag = test;
        } else
        {
            return false;
        }

        string[] newArgs = new string[split.Length - 1];
        for (int i = 1; i < split.Length; i++)
        {
            newArgs[i - 1] = split[i];
        }
        t.args = (string[])newArgs.Clone();

        return true;
    }

    public static TagEntry Parse(string str)
    {
        TagEntry t = new TagEntry();
        string tag = (string)str.Clone();
        if (tag.Length < 2 || tag[0] != '<' || tag[tag.Length - 1] != '>')
        {
            return null;
            //throw new ArgumentException("Invalid tag (no <> surrounding it)");
        }
        tag = tag.Substring(1, tag.Length - 2); //get rid of < and >

        if (tag[0] == '/')
        {
            t.open = false;
            tag = tag.Substring(1);
        }

        //(?<!\\), does not seem to work
        string[] split = System.Text.RegularExpressions.Regex.Split(tag, ","); //,

        //new escape sequences (so you can use , in tags)
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = FormattedString.ParseEscapeSequences(split[i]);
        }


        t.tag = TextTag.Null;
        TextTag test = TextTag.Null;
        if (Enum.TryParse(split[0], true, out test))
        {
            t.tag = test;
        }

        string[] newArgs = new string[split.Length - 1];
        for (int i = 1; i < split.Length; i++)
        {
            newArgs[i - 1] = split[i];
        }
        t.args = (string[])newArgs.Clone();
        return t;
    }
    //Convert a string to tag
    public TagEntry(string str)
    {
        string tag = (string)str.Clone();
        if (tag[0] != '<' || tag[tag.Length - 1] != '>')
        {
            throw new ArgumentException("Invalid tag (no <> surrounding it)");
        }
        tag = tag.Substring(1, tag.Length - 2); //get rid of < and >

        if (tag[0] == '/')
        {
            open = false;
            tag = tag.Substring(1);
        }

        //(?<!\\), does not seem to work
        string[] split = System.Text.RegularExpressions.Regex.Split(tag, ","); //,

        //new escape sequences (so you can use , in tags)
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = FormattedString.ParseEscapeSequences(split[i]);
        }


        this.tag = TextTag.Null;
        TextTag test = TextTag.Null;
        if (Enum.TryParse(split[0], true, out test))
        {
            this.tag = test;
        }

        string[] newArgs = new string[split.Length - 1];
        for (int i = 1; i < split.Length; i++)
        {
            newArgs[i - 1] = split[i];
        }
        args = (string[])newArgs.Clone();
    }
    public TagEntry(string str, int p_trueStartIndex, int p_trueEndIndex, int p_startIndex)
    {
        string tag = (string)str.Clone();
        if (tag[0] != '<' || tag[tag.Length - 1] == '>')
        {
            throw new ArgumentException("Invalid tag (no <> surrounding it)");
        }
        tag = tag.Substring(1, tag.Length - 2); //get rid of < and >

        if (tag[0] == '/')
        {
            open = false;
            tag = tag.Substring(1);
        }

        //(?<!\\), does not seem to work
        string[] split = System.Text.RegularExpressions.Regex.Split(tag, ","); //,

        //new escape sequences (so you can use , in tags)
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = FormattedString.ParseEscapeSequences(split[i]);
        }

        this.tag = TextTag.Null;
        TextTag test = TextTag.Null;
        if (Enum.TryParse(split[0], true, out test))
        {
            this.tag = test;
        }

        string[] newArgs = new string[split.Length - 1];
        for (int i = 1; i < split.Length; i++)
        {
            newArgs[i - 1] = split[i];

        }

        trueStartIndex = p_trueStartIndex;
        trueEndIndex = p_trueEndIndex;
        startIndex = p_startIndex;
    }

    public static string TagToString(TagEntry t)
    {
        string output = "<";
        if (!t.open)
        {
            output += "/";
        }
        output += t.tag.ToString();

        for (int i = 0; i < t.args.Length; i++)
        {
            output += ",";
            output += t.args[i];
        }

        output += ">";

        return output;
    }
    public override string ToString()
    {
        return TagToString(this);
    }

    public string GetString()
    {
        return TagToString(this);
    }
}


public class FormattedString
{
    public string internalString;
    public TagEntry[] tags;

    public FormattedString(string s)
    {
        internalString = s;
        if (internalString == null)
        {
            internalString = "";
        }
        Update();
    }
    private void Update()
    {
        if (internalString == null)
        {
            internalString = "";
        }

        //<var> = <var,1>
        //vars are 1 indexed
        bool intag = false;

        List<char> newlist = new List<char>();
        List<char> currTag = new List<char>();
        List<TagEntry> tagEntries = new List<TagEntry>();

        int start = -1;
        int tstart = -1;

        for (int i = 0; i < internalString.Length; i++)
        {
            if (internalString[i] == '<') //tag left
            {
                if (intag)
                {
                    Debug.LogError("Problem string: " + internalString.Substring(0, i));
                    throw new ArgumentException("Nested < is not supported");
                }
                intag = true;
                tstart = i;
                start = newlist.Count;
                while (currTag.Count > 0) //clear out the current tag
                {
                    currTag.RemoveAt(0);
                }
            }
            if (internalString[i] == '&')
            {
                continue;
            }
            if (!intag)
            {
                newlist.Add(internalString[i]);
            }
            else
            {
                currTag.Add(internalString[i]);
            }
            if (internalString[i] == '>' && intag) //parse tag
            {
                string tag = new string(currTag.ToArray());

                //Debug.Log(start + " start with " + newlist.Count + " " + tstart);
                //Debug.Log(new string(newlist.ToArray()));
                //Debug.Log(tag);

                TagEntry t = new TagEntry(tag)
                {
                    startIndex = start,
                    trueStartIndex = tstart,
                    trueEndIndex = i
                };
                tagEntries.Add(t);

                intag = false;
            }
        }

        string temp = new string(newlist.ToArray());
        temp = ParseEscapeSequences(temp);
        temp = temp.Replace(",", "c");
        string temp2 = GetCleanString(true);
        temp2 = temp2.Replace("&", "a");
        temp2 = temp2.Replace(",", "c");
        temp2 = temp2.Replace("<", "l");
        temp2 = temp2.Replace(">", "r");
        if (!temp.Equals(temp2))
        {
            Debug.LogWarning("Problem with constructing tagless string (may result in invalid indices somewhere)");
            Debug.LogWarning(temp);
            Debug.LogWarning(temp2);
        }

        tags = tagEntries.ToArray();
    }

    //Ignores true indices for the most part
    public static string BuildString(string s, TagEntry[] tags)
    {
        string output = s;
        List<TagEntry> tagsList = new List<TagEntry>(tags);
        for (int i = 0; i < tagsList.Count; i++)
        {
            tagsList[i].trueStartIndex = i;
        }
        tagsList.Sort((a, b) => (a.startIndex == b.startIndex ? a.trueStartIndex - b.trueStartIndex : a.startIndex - b.startIndex));

        int splitIndex = -1;
        for (int i = tagsList.Count-1; i >= 0; i--)
        {
            splitIndex = tagsList[i].startIndex;
            if (splitIndex < 0 || splitIndex >= output.Length)
            {
                throw new ArgumentException("Invalid startindex in tag.");
            }
            output = output.Substring(0, splitIndex + 1) + tagsList[i].ToString() + output.Substring(splitIndex + 1);
        }

        return output;
    }

    public static string StripTags(string s)
    {
        bool intag = false;
        char[] c = s.ToCharArray();
        List<char> newlist = new List<char>();

        for (int i = 0; i < s.Length; i++)
        {
            if (c[i] == '<') //tag left
            {
                intag = true;
            }
            if (!intag)
            {
                newlist.Add(c[i]);
            }
            if (c[i] == '>' && intag)
            {
                intag = false;
            }
        }
        return new string(newlist.ToArray());
    }
    public static string InsertEscapeSequences(string s)
    {
        string output = (string)s.Clone();
        output = output.Replace("&", "&a");
        output = output.Replace(",", "&c");
        output = output.Replace("<", "&l");
        output = output.Replace(">", "&r");
        return output;
    }
    public static string ParseEscapeSequences(string s)
    {
        string output = (string)s.Clone();
        //new escape sequences (so you can use , in tags)
        output = output.Replace("&c", ",");
        output = output.Replace("&a", "&");
        output = output.Replace("&l", "<");
        output = output.Replace("&r", ">");
        return output;

    } //parse my special escape sequences
    //public static string[] ParseNextTags(string s, bool leaveTags = false) //may put next tags in between actual text (so you can still parse them)
    //{
    //    return ParseByTag(s, TagEntry.TextTag.Next, leaveTags);
    //}
    public static string[] SplitByTag(string s, bool leaveTags, TagEntry.TextTag t) //may put next tags in between actual text (so you can still parse them)    (e.g. 1<next>2 => 3 things with leaveTags, second one is <next>)
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string[] output;
        List<string> outputList = new List<string>();
        TagEntry[] tags = fs.tags;
        List<TagEntry> sorted = new List<TagEntry>(tags);
        sorted.Sort((e, f) => (-e.trueStartIndex + f.trueStartIndex));
        tags = sorted.ToArray();

        int lastEnd = 0;



        for (int i = tags.Length - 1; i >= 0; i--)
        {
            if (tags[i].tag == t)
            {
                //Debug.Log(fs.internalString.Length + " " + lastEnd + " " + tags[i].trueStartIndex);
                outputList.Add(fs.internalString.Substring(lastEnd, tags[i].trueStartIndex - lastEnd));
                if (leaveTags)
                {
                    outputList.Add(fs.internalString.Substring(tags[i].trueStartIndex, tags[i].trueEndIndex - tags[i].trueStartIndex + 1));
                }
                lastEnd = tags[i].trueEndIndex + 1;
            }
        }
        if (lastEnd < fs.internalString.Length)
        {
            outputList.Add(fs.internalString.Substring(lastEnd));
        }

        output = outputList.ToArray();
        return output;
    }
    public static string ReplaceTagWith(string s, TagEntry.TextTag t, string replacement)
    {
        string[] splitString = SplitByTag(s, false, t);
        string output = "";
        for (int i = 0; i < splitString.Length; i++)
        {
            if (i > 0)
            {
                output += replacement;
            }

            output += splitString[i];
        }

        return output;
    }

    public static string[] SplitByTags(string s, bool leaveTags, params TagEntry.TextTag[] t) //may put next tags in between actual text (so you can still parse them)
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string[] output;
        List<string> outputList = new List<string>();
        TagEntry[] tags = fs.tags;
        List<TagEntry> sorted = new List<TagEntry>(tags);
        sorted.Sort((e, f) => (-e.trueStartIndex + f.trueStartIndex));
        tags = sorted.ToArray();

        int lastEnd = 0;



        for (int i = tags.Length - 1; i >= 0; i--)
        {
            for (int j = 0; j < t.Length; j++)
            {
                if (tags[i].tag == t[j])
                {
                    //Debug.Log(fs.internalString.Length + " " + lastEnd + " " + tags[i].trueStartIndex);
                    outputList.Add(fs.internalString.Substring(lastEnd, tags[i].trueStartIndex - lastEnd));
                    if (leaveTags)
                    {
                        outputList.Add(fs.internalString.Substring(tags[i].trueStartIndex, tags[i].trueEndIndex - tags[i].trueStartIndex + 1));
                    }
                    lastEnd = tags[i].trueEndIndex + 1;
                }
            }
        }
        if (lastEnd < fs.internalString.Length)
        {
            outputList.Add(fs.internalString.Substring(lastEnd));
        }

        output = outputList.ToArray();
        return output;
    }
    public static string[] SplitByTags(string s, bool leaveTags = true)
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string[] output;
        List<string> outputList = new List<string>();
        TagEntry[] tags = fs.tags;
        List<TagEntry> sorted = new List<TagEntry>(tags);
        sorted.Sort((e, f) => (-e.trueStartIndex + f.trueStartIndex));
        tags = sorted.ToArray();

        int lastEnd = 0;



        for (int i = tags.Length - 1; i >= 0; i--)
        {
            //Debug.Log(fs.internalString.Length + " " + lastEnd + " " + tags[i].trueStartIndex);
            outputList.Add(fs.internalString.Substring(lastEnd, tags[i].trueStartIndex - lastEnd));
            if (leaveTags)
            {
                outputList.Add(fs.internalString.Substring(tags[i].trueStartIndex, tags[i].trueEndIndex - tags[i].trueStartIndex + 1));
            }
            lastEnd = tags[i].trueEndIndex + 1;
        }
        if (lastEnd < fs.internalString.Length)
        {
            outputList.Add(fs.internalString.Substring(lastEnd));
        }

        output = outputList.ToArray();
        return output;
    }
    public static string ParseCutTags(string s)
    {
        string[] cutString = SplitByTag(s, true, TagEntry.TextTag.CondCut);

        string output = "";

        int cutDepth = 0;
        int cutInDepth = 0;
        for (int i = 0; i < cutString.Length; i++)
        {
            TagEntry t = TagEntry.Parse(cutString[i]);
            if (t != null && t.tag == TagEntry.TextTag.CondCut)
            {
                //If cut is true: skip until you find the correct depth closing tag
                if (!t.open)
                {
                    cutDepth--;
                    if (cutInDepth == 0 || cutInDepth > cutDepth)
                    {
                        if (cutInDepth > cutDepth)
                        {
                            cutInDepth = 0;
                        }
                    }
                }
                else
                {
                    cutDepth += 1;
                    if (cutInDepth == 0 || cutInDepth > cutDepth)
                    {
                        if (ParseBranchCondition(t))
                        {
                            cutInDepth = cutDepth;
                        }
                    }
                }
            } else
            {
                if (cutInDepth == 0 || cutInDepth > cutDepth)
                {
                    output += cutString[i];
                }
            }
        }

        return output;
    }
    public static string ParseLines(string s) 
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string output = s;
        TagEntry[] tags = fs.tags;

        for (int i = tags.Length - 1; i >= 0; i--)
        {
            if (tags[i].tag == TagEntry.TextTag.Line)
            {
                //get rid of tag
                output = output.Substring(0, tags[i].trueStartIndex) + output.Substring(tags[i].trueEndIndex + 1, output.Length - 1 - tags[i].trueEndIndex);
                output = output.Insert(tags[i].trueStartIndex, "\n");
            }
        }

        return output;
    } //parses line tags
    public static string ParseSymbols(string s)
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string output = s;
        TagEntry[] tags = fs.tags;

        for (int i = tags.Length - 1; i >= 0; i--)
        {
            string temp = TMPString.ConvertSymbolTag(tags[i]);
            if (temp.Length > 0)
            {
                //get rid of tag
                output = output.Substring(0, tags[i].trueStartIndex) + output.Substring(tags[i].trueEndIndex + 1, output.Length - 1 - tags[i].trueEndIndex);
                output = output.Insert(tags[i].trueStartIndex, temp);
            }
        }

        return output;
    }
    public static string ParseVars(string s, string[] vars)
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string output = s;
        TagEntry[] tags = fs.tags;

        for (int i = tags.Length - 1; i >= 0; i--)
        {
            if (tags[i].tag == TagEntry.TextTag.Var)
            {
                int arg = 0;
                if (tags[i].args.Length > 0)
                {
                    if (int.TryParse(tags[i].args[0], out int result))
                    {
                        arg = result;
                    }
                }
                //get rid of tag
                output = output.Substring(0, tags[i].trueStartIndex) + output.Substring(tags[i].trueEndIndex + 1, output.Length - 1 - tags[i].trueEndIndex);

                if (vars == null || vars.Length <= arg)
                {
                    output = output.Insert(tags[i].trueStartIndex, "Invalid Var " + arg + " &r= " + (vars == null ? "X" : vars.Length));
                }
                else
                {
                    bool special = false;
                    //new special thing: offsets
                    if (tags[i].args.Length > 1 && vars.Length > arg)
                    {
                        if (int.TryParse(vars[arg], out int number) && int.TryParse(tags[i].args[1], out int offset))
                        {
                            special = true;
                            output = output.Insert(tags[i].trueStartIndex, (number + offset).ToString());
                        }
                    }

                    if (!special)
                    {
                        output = output.Insert(tags[i].trueStartIndex, vars[arg]);
                    }
                }
            }
        }

        return output;
    } //has a failsafe for nonexistent vars
    //note that textboxes get passed through here with vars = null but that will not mess up unless there are vars in the text

    public static string ParseVar(string s, string var, int value)  //selective version (e.g. if value = 5 then all the <var,5> tags get parsed)
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string output = s;
        TagEntry[] tags = fs.tags;

        for (int i = tags.Length - 1; i >= 0; i--)
        {
            if (tags[i].tag == TagEntry.TextTag.Var)
            {
                int arg = 0;
                if (tags[i].args.Length > 0)
                {
                    if (int.TryParse(tags[i].args[0], out int result))
                    {
                        arg = result;
                    }
                }

                if (arg == value)
                {
                    //get rid of tag
                    output = output.Substring(0, tags[i].trueStartIndex) + output.Substring(tags[i].trueEndIndex + 1, output.Length - 1 - tags[i].trueEndIndex);
                    output = output.Insert(tags[i].trueStartIndex, var);
                }
            }
        }

        return output;
    }
    public static string ParseNonlocalVars(string s) //parses nonlocal vars and flags
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string output = s;
        TagEntry[] tags = fs.tags;

        for (int i = tags.Length - 1; i >= 0; i--)
        {
            TagEntry.TextTag[] tagsToCheck = new TagEntry.TextTag[] { TagEntry.TextTag.Arg, TagEntry.TextTag.GlobalVar, TagEntry.TextTag.GlobalFlag, TagEntry.TextTag.AreaVar, TagEntry.TextTag.AreaFlag, TagEntry.TextTag.MapVar, TagEntry.TextTag.MapFlag, TagEntry.TextTag.Const };
            bool success = false;
            for (int j = 0; j < tagsToCheck.Length; j++)
            {
                if (tags[i].tag == tagsToCheck[j])
                {
                    success = true;
                    break;
                }
            }
            if (!success)
            {
                continue;
            }

            int arg = -1;
            if (tags[i].args.Length > 0)
            {
                if (int.TryParse(tags[i].args[0], out int result))
                {
                    arg = result;
                }
            }
            //get rid of tag
            output = output.Substring(0, tags[i].trueStartIndex) + output.Substring(tags[i].trueEndIndex + 1, output.Length - 1 - tags[i].trueEndIndex);
            string insert = "";
            switch (tags[i].tag)
            {
                case TagEntry.TextTag.Arg:
                    insert = ParseArg(MainManager.Instance.lastTextboxMenuResult, "arg" + (arg == -1 ? "" : arg));
                    break;
                case TagEntry.TextTag.GlobalVar:
                    MainManager.GlobalVar gv = (MainManager.GlobalVar)(-1);
                    if (tags[i].args.Length > 0) {
                        Enum.TryParse(tags[i].args[0], out gv);
                    }
                    MainManager.Instance.GetGlobalVar(gv);
                    break;
                case TagEntry.TextTag.GlobalFlag:
                    MainManager.GlobalFlag gf = (MainManager.GlobalFlag)(-1);
                    if (tags[i].args.Length > 0)
                    {
                        Enum.TryParse(tags[i].args[0], out gf);
                    }
                    insert = MainManager.Instance.GetGlobalFlag(gf).ToString();
                    break;
                case TagEntry.TextTag.AreaVar:
                    insert = MainManager.Instance.GetAreaVar(tags[i].args[0]);
                    break;
                case TagEntry.TextTag.AreaFlag:
                    insert = MainManager.Instance.GetAreaFlag(tags[i].args[0]).ToString();
                    break;
                case TagEntry.TextTag.MapVar:
                    insert = MainManager.Instance.GetMapVar(tags[i].args[0]);
                    break;
                case TagEntry.TextTag.MapFlag:
                    insert = MainManager.Instance.GetMapFlag(tags[i].args[0]).ToString();
                    break;
                case TagEntry.TextTag.Const:
                    MainManager.GameConst g;
                    if (arg == -1 && Enum.TryParse(tags[i].args[0], true, out g))
                    {
                        insert = MainManager.Instance.GetConst(g);
                    } else
                    {
                        insert = MainManager.Instance.GetConst(arg);
                    }
                    break;
            }
            if (insert == null)
            {
                insert = "";
            }
            output = output.Insert(tags[i].trueStartIndex, insert);
        }

        return output;
    }
    public static string ParseNonlocalVar(string tagname, string index) //Tagname = name of tag (mapflag, mapvar, etc), Index = index of var (flag 0 vs flag 1)
    {
        TagEntry.TextTag t;
        t = (TagEntry.TextTag)Enum.Parse(typeof(TagEntry.TextTag), tagname, true);

        int arg = -1;
        if (t == TagEntry.TextTag.Const)
        {
            MainManager.GameConst g;
            if (Enum.TryParse(index, true, out g))
            {
                arg = (int)g;
            } else
            {
                if (int.TryParse(index, out int result))
                {
                    arg = result;
                }
            }
        } else
        {
            if (int.TryParse(index, out int result))
            {
                arg = result;
            }
        }

        string insert = "";
        switch (t)
        {
            case TagEntry.TextTag.Arg:
                insert = ParseArg(MainManager.Instance.lastTextboxMenuResult, "arg" + (arg == -1 ? "" : arg));
                break;
            case TagEntry.TextTag.GlobalVar:
                MainManager.GlobalVar gv = (MainManager.GlobalVar)(-1);
                Enum.TryParse(index, out gv);
                MainManager.Instance.GetGlobalVar(gv);
                break;
            case TagEntry.TextTag.GlobalFlag:
                MainManager.GlobalFlag gf = (MainManager.GlobalFlag)(-1);
                Enum.TryParse(index, out gf);
                insert = MainManager.Instance.GetGlobalFlag(gf).ToString();
                break;
            case TagEntry.TextTag.AreaVar:
                insert = MainManager.Instance.GetAreaVar(index);
                break;
            case TagEntry.TextTag.AreaFlag:
                insert = MainManager.Instance.GetAreaFlag(index).ToString();
                break;
            case TagEntry.TextTag.MapVar:
                insert = MainManager.Instance.GetMapVar(index);
                break;
            case TagEntry.TextTag.MapFlag:
                insert = MainManager.Instance.GetMapFlag(index).ToString();
                break;
            case TagEntry.TextTag.Const:
                Debug.Log(arg);
                insert = MainManager.Instance.GetConst(arg);
                break;
        }

        return insert;
    }
    public static void SetNonlocalVar(string tagname, string index, string set)
    {
        TagEntry.TextTag t;
        t = (TagEntry.TextTag)Enum.Parse(typeof(TagEntry.TextTag), tagname, true);

        switch (t)
        {
            //case TagEntry.TextTag.Arg:
                //can't support arg here
            case TagEntry.TextTag.GlobalVar:
                MainManager.GlobalVar gv = (MainManager.GlobalVar)(-1);
                Enum.TryParse(index, out gv);
                MainManager.Instance.SetGlobalVar(gv, set);
                break;
            case TagEntry.TextTag.GlobalFlag:
                MainManager.GlobalFlag gf = (MainManager.GlobalFlag)(-1);
                Enum.TryParse(index, out gf);
                MainManager.Instance.SetGlobalFlag(gf, bool.Parse(set));
                break;
            case TagEntry.TextTag.AreaVar:
                MainManager.Instance.SetAreaVar(index, set);
                break;
            case TagEntry.TextTag.AreaFlag:
                MainManager.Instance.SetAreaFlag(index, bool.Parse(set));
                break;
            case TagEntry.TextTag.MapVar:
                MainManager.Instance.SetMapVar(index, set);
                break;
            case TagEntry.TextTag.MapFlag:
                MainManager.Instance.SetMapFlag(index, bool.Parse(set));
                break;
            case TagEntry.TextTag.Const:
                throw new ArgumentException("Cannot set game constants. (value = " + index + ", "+set+")");
        }
    }

    public static string ReplaceTextFileTags(string s)
    {
        FormattedString fs = new FormattedString(s);

        //<var> = <var,0>
        string output = s;
        TagEntry[] tags = fs.tags;

        for (int i = tags.Length - 1; i >= 0; i--)
        {
            TagEntry.TextTag[] tagsToCheck = new TagEntry.TextTag[] { TagEntry.TextTag.MenuText, TagEntry.TextTag.CommonText, TagEntry.TextTag.SystemText, TagEntry.TextTag.ItemText, TagEntry.TextTag.KeyItemText, TagEntry.TextTag.LocalText };
            bool success = false;
            for (int j = 0; j < tagsToCheck.Length; j++)
            {
                if (tags[i].tag == tagsToCheck[j])
                {
                    success = true;
                    break;
                }
            }
            if (!success)
            {
                continue;
            }

            int arg = -1;
            if (tags[i].args.Length > 0)
            {
                if (int.TryParse(tags[i].args[0], out int result))
                {
                    arg = result;
                }
            }
            //get rid of tag
            output = output.Substring(0, tags[i].trueStartIndex) + output.Substring(tags[i].trueEndIndex + 1, output.Length - 1 - tags[i].trueEndIndex);
            string insert = "";
            int n1 = 0;
            int n2 = 0;
            switch (tags[i].tag)
            {
                case TagEntry.TextTag.MenuText:
                    if (tags[i].args.Length > 0 && int.TryParse(tags[i].args[0], out n1))
                    {

                    }
                    if (tags[i].args.Length > 1 && int.TryParse(tags[i].args[1], out n2))
                    {

                    }
                    insert = MainManager.Instance.GetTextFromFile(MainManager.Instance.menuText, n1, n2);
                    break;
                case TagEntry.TextTag.CommonText:
                    if (tags[i].args.Length > 0 && int.TryParse(tags[i].args[0], out n1))
                    {

                    } else
                    {
                        if (tags[i].args.Length > 0 && Enum.TryParse(tags[i].args[0], true, out MainManager.CommonTextLine ctl))
                        {
                            n1 = (int)ctl;
                        }
                    }
                    if (tags[i].args.Length > 1 && int.TryParse(tags[i].args[1], out n2))
                    {

                    } else
                    {
                        n2 = 1;
                    }
                    insert = MainManager.Instance.GetTextFromFile(MainManager.Instance.commonText, n1, n2);
                    break;
                case TagEntry.TextTag.SystemText:
                    if (tags[i].args.Length > 0 && int.TryParse(tags[i].args[0], out n1))
                    {

                    }
                    if (tags[i].args.Length > 1 && int.TryParse(tags[i].args[1], out n2))
                    {

                    }
                    insert = MainManager.Instance.GetTextFromFile(MainManager.Instance.systemText, n1, n2);
                    break;
                case TagEntry.TextTag.ItemText:
                    //parse the item
                    Item.ItemType it = (Item.ItemType) (- 1);
                    if (tags[i].args.Length > 0 && Enum.TryParse(tags[i].args[0], true, out it))
                    {

                    } else
                    {
                        Debug.LogError("[Item Text Parsing] Text " + tags[i].args[0] + " can't be parsed as an item");
                    }

                    int value = -1;
                    if (tags[i].args.Length > 1 && int.TryParse(tags[i].args[1], out value))
                    {

                    }

                    if (tags[i].args.Length > 1 && tags[i].args[1].Equals("menuname"))
                    {
                        insert = Item.GetSpriteString(it) + " " + Item.GetName(it);
                    } else if (tags[i].args.Length > 1 && tags[i].args[1].Equals("name"))
                    {
                        insert = Item.GetName(it);
                    }
                    else if (tags[i].args.Length > 1 && tags[i].args[1].Equals("desc"))
                    {
                        insert = Item.GetDescription(it);
                    }
                    else
                    {
                        if ((int)it != -1 && value != -1)
                        {
                            insert = Item.GetItemText(it, value);
                        }
                    }

                    break;
                case TagEntry.TextTag.KeyItemText:
                    //parse the item
                    KeyItem.KeyItemType kit = (KeyItem.KeyItemType)(-1);
                    if (tags[i].args.Length > 0 && Enum.TryParse(tags[i].args[0], true, out kit))
                    {

                    }
                    else
                    {
                        Debug.LogError("[Key Item Text Parsing] Text " + tags[i].args[0] + " can't be parsed as a key item");
                    }

                    int valueB = -1;
                    if (tags[i].args.Length > 1 && int.TryParse(tags[i].args[1], out valueB))
                    {

                    }

                    if (tags[i].args.Length > 1 && tags[i].args[1].Equals("menuname"))
                    {
                        insert = KeyItem.GetSpriteString(kit) + " " + KeyItem.GetName(kit);
                    }
                    else if (tags[i].args.Length > 1 && tags[i].args[1].Equals("name"))
                    {
                        insert = KeyItem.GetName(kit);
                    }
                    else if (tags[i].args.Length > 1 && tags[i].args[1].Equals("desc"))
                    {
                        insert = KeyItem.GetDescription(kit);
                    }
                    else
                    {
                        if ((int)kit != -1 && valueB != -1)
                        {
                            insert = KeyItem.GetKeyItemText(kit, valueB);
                        }
                    }

                    break;
                case TagEntry.TextTag.LocalText:
                    if (tags[i].args.Length > 0 && int.TryParse(tags[i].args[0], out n1))
                    {

                    }
                    if (tags[i].args.Length > 1 && int.TryParse(tags[i].args[1], out n2))
                    {

                    }
                    insert = MainManager.Instance.GetTextFromFile(MainManager.Instance.mapScript.textFile, n1, n2);
                    break;
            }
            if (insert == null)
            {
                insert = "";
            }
            output = output.Insert(tags[i].trueStartIndex, insert);
        }

        return output;
    }
    public static string ReplaceTextFileShorthand(string s)
    {
        string[][] file = null;
        int index = -1;
        
        (file, index) = GetTextFileFromShorthandFull(s);

        if (file == null)
        {
            return s;
        }

        return MainManager.Instance.GetTextFromFile(file, index);
    }
    public static string[][] GetTextFileFromShorthand(string s) //null if no file matches
    {
        if (s.Length < 2)
        {
            return null;
        }

        TagEntry.TextTag[] tagsToCheck = new TagEntry.TextTag[] { TagEntry.TextTag.MenuText, TagEntry.TextTag.CommonText, TagEntry.TextTag.SystemText, TagEntry.TextTag.ItemText, TagEntry.TextTag.KeyItemText, TagEntry.TextTag.LocalText };
        string firstLetter = "mcsikl";   //char array

        string numberGuess = s.Substring(1);

        int numberVal = 0;

        if (int.TryParse(numberGuess, out numberVal))
        {

        }
        else
        {
            return null;
        }

        for (int i = 0; i < tagsToCheck.Length; i++)
        {
            if (s[0] == firstLetter[i])
            {
                Debug.Log(tagsToCheck[i]);
                switch (tagsToCheck[i])
                {
                    case TagEntry.TextTag.MenuText:
                        return MainManager.Instance.menuText;
                    case TagEntry.TextTag.CommonText:
                        return MainManager.Instance.commonText;
                    case TagEntry.TextTag.SystemText:
                        return MainManager.Instance.systemText;
                    case TagEntry.TextTag.ItemText:
                        return GlobalItemScript.Instance.GetTextFile();// Item.GetName((Item.ItemType)numberVal);
                    case TagEntry.TextTag.KeyItemText:
                        return GlobalItemScript.Instance.GetKeyItemText();
                    case TagEntry.TextTag.LocalText:
                        return MainManager.Instance.mapScript.textFile;
                }
            }
        }

        return null;
    }
    public static (string[][], int) GetTextFileFromShorthandFull(string s) //null, -1 if no file matches
    {
        if (s.Length < 2)
        {
            return (null, -1);
        }

        TagEntry.TextTag[] tagsToCheck = new TagEntry.TextTag[] { TagEntry.TextTag.MenuText, TagEntry.TextTag.CommonText, TagEntry.TextTag.SystemText, TagEntry.TextTag.ItemText, TagEntry.TextTag.KeyItemText, TagEntry.TextTag.LocalText };
        string firstLetter = "mcsikl";   //char array

        string numberGuess = s.Substring(1);

        int numberVal = 0;

        if (int.TryParse(numberGuess, out numberVal))
        {

        }
        else
        {
            return (null, -1);
        }

        for (int i = 0; i < tagsToCheck.Length; i++)
        {
            if (s[0] == firstLetter[i])
            {
                switch (tagsToCheck[i])
                {
                    case TagEntry.TextTag.MenuText:
                        return (MainManager.Instance.menuText, numberVal);
                    case TagEntry.TextTag.CommonText:
                        return (MainManager.Instance.commonText, numberVal);
                    case TagEntry.TextTag.SystemText:
                        return (MainManager.Instance.systemText, numberVal);
                    case TagEntry.TextTag.ItemText:
                        return (GlobalItemScript.Instance.GetTextFile(), numberVal);// Item.GetName((Item.ItemType)numberVal);
                    case TagEntry.TextTag.KeyItemText:
                        return (GlobalItemScript.Instance.GetKeyItemText(), numberVal);
                    case TagEntry.TextTag.LocalText:
                        return (MainManager.Instance.mapScript.textFile, numberVal);
                }
            }
        }

        return (null, -1);
    }
    public static bool IsTextFileShorthand(string s)
    {
        if (s.Length < 2)
        {
            return false;
        }

        TagEntry.TextTag[] tagsToCheck = new TagEntry.TextTag[] { TagEntry.TextTag.MenuText, TagEntry.TextTag.CommonText, TagEntry.TextTag.SystemText, TagEntry.TextTag.ItemText, TagEntry.TextTag.KeyItemText, TagEntry.TextTag.LocalText };
        string firstLetter = "mcsikl";   //char array

        string numberGuess = s.Substring(1);

        int numberVal = 0;

        if (int.TryParse(numberGuess, out numberVal))
        {

        }
        else
        {
            return false;
        }

        for (int i = 0; i < tagsToCheck.Length; i++)
        {
            if (s[0] == firstLetter[i])
            {
                return true;
            }
        }

        return false;
    }
    public static bool IsStringCSV(string s)
    {
        //this is an extremely bad idea but it should be fine
        //you should use <line> anyway
        return s.Contains('\n');
    }

    public static bool IsZeroArgBranch(TagEntry.Branch b)
    {
        if (b == TagEntry.Branch.HKERU || b == TagEntry.Branch.NHKERU)
        {
            return true;
        }
        return false;
    }
    public static bool IsOneArgBranch(TagEntry.Branch b)
    {
        if (b == TagEntry.Branch.HITEM || b == TagEntry.Branch.NHITEM || b == TagEntry.Branch.HKITEM || b == TagEntry.Branch.NHKITEM || b == TagEntry.Branch.HPARTY || b == TagEntry.Branch.NHPARTY)
        {
            return true;
        }
        return false;
    }
    public static bool IsImmediateBranch(TagEntry.Branch b)
    {
        if (b == TagEntry.Branch.EQI || b == TagEntry.Branch.NEQI || b == TagEntry.Branch.GTEI || b == TagEntry.Branch.LTEI || b == TagEntry.Branch.LTI || b == TagEntry.Branch.GTI)
        {
            return true;
        }
        return false;
    }

    public static bool ParseBranchCondition(TagEntry tag, string arg = "", int commaSplit = -1)
    {
        if ((tag.tag != TagEntry.TextTag.Branch) && (tag.tag != TagEntry.TextTag.CondEnd) && (tag.tag != TagEntry.TextTag.CondCut))
        {
            throw new ArgumentException("Parsed tag must be a branch-like tag (branch, condend, condcut): " + tag.tag + " is not one");
        }

        if (commaSplit != -1)
        {
            arg = arg.Split(',')[commaSplit];
        }

        TagEntry.Branch b = (TagEntry.Branch)Enum.Parse(typeof(TagEntry.Branch), tag.args[0], true);

        int rightStartIndex = 1;

        string sleftSide = null;
        string srightSide = null;

        if (IsZeroArgBranch(b))
        {

        } else
        {
            if (b == TagEntry.Branch.HPARTY || b == TagEntry.Branch.NHPARTY)
            {
                //to do: make another category for these?
                sleftSide = tag.args[1];
            } else
            {
                if (tag.args[1].Contains("arg"))
                {
                    sleftSide = ParseArg(arg, tag.args[1]);
                    rightStartIndex = 2;
                }
                else
                {
                    sleftSide = ParseNonlocalVar(tag.args[1], tag.args[2]);
                    rightStartIndex = 3;
                }
            }
        }

        if (IsZeroArgBranch(b) || IsOneArgBranch(b))
        {
            //Do nothing because there is no right side
        } else
        {
            if (IsImmediateBranch(b))
            {
                //Parse as an immediate value
                srightSide = tag.args[rightStartIndex];
            }
            else
            {

                //Parse as a variable
                if (tag.args[rightStartIndex].Contains("arg"))
                {                    
                    srightSide = ParseArg(arg, tag.args[rightStartIndex]);
                }
                else
                {
                    srightSide = ParseNonlocalVar(tag.args[rightStartIndex], tag.args[rightStartIndex + 1]);
                }
            }
        }

        bool output = false;
        float leftSide;
        float rightSide;

        if (float.TryParse(sleftSide, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out leftSide))
        {

        } else
        {
            leftSide = float.NaN;
        }
        if (float.TryParse(srightSide, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out rightSide))
        {

        }
        else
        {
            rightSide = float.NaN;
        }

        Debug.Log(b + " " + sleftSide + " " + srightSide);

        //note that equality/inequality use string checking instead of numerical checking
        //can lead to problems with float format inconsistencies (4 vs 4.0 vs 4.00 etc)
        //However, this does make them more versatile in some situations (can check if you give them the right item)
        switch (b)
        {
            case TagEntry.Branch.EQ:
            case TagEntry.Branch.EQI:
                output = sleftSide.Equals(srightSide);
                break;
            case TagEntry.Branch.NEQ:
            case TagEntry.Branch.NEQI:
                output = !sleftSide.Equals(srightSide);
                break;
            case TagEntry.Branch.LT:
            case TagEntry.Branch.LTI:
                output = leftSide < rightSide;
                break;
            case TagEntry.Branch.LTE:
            case TagEntry.Branch.LTEI:
                output = leftSide <= rightSide;
                break;
            case TagEntry.Branch.GT:
            case TagEntry.Branch.GTI:
                output = leftSide > rightSide;
                break;
            case TagEntry.Branch.GTE:
            case TagEntry.Branch.GTEI:
                output = leftSide >= rightSide;
                break;
            case TagEntry.Branch.HITEM:
                Item.ItemType it;
                if (Enum.TryParse(sleftSide, true, out it))
                {
                    output = MainManager.Instance.playerData.HasItem(it);
                } else
                {
                    output = false; //failsafe
                }
                break;
            case TagEntry.Branch.NHITEM:
                Item.ItemType it2;
                if (Enum.TryParse(sleftSide, true, out it2))
                {
                    output = !MainManager.Instance.playerData.HasItem(it2);
                }
                else
                {
                    output = false; //failsafe
                }
                break;
            case TagEntry.Branch.HKITEM:
                KeyItem.KeyItemType it3;
                if (Enum.TryParse(sleftSide, true, out it3))
                {
                    output = MainManager.Instance.playerData.HasKeyItem(it3);
                }
                else
                {
                    output = false; //failsafe
                }
                break;
            case TagEntry.Branch.NHKITEM:
                KeyItem.KeyItemType it4;
                if (Enum.TryParse(sleftSide, true, out it4))
                {
                    output = !MainManager.Instance.playerData.HasKeyItem(it4);
                }
                else
                {
                    output = false; //failsafe
                }
                break;
            case TagEntry.Branch.HPARTY:
                BattleHelper.EntityID pc;
                if (Enum.TryParse(sleftSide, true, out pc))
                {
                    output = MainManager.Instance.playerData.GetPlayerDataEntry(pc) != null;
                } else
                {
                    output = false;
                }
                break;
            case TagEntry.Branch.NHPARTY:
                BattleHelper.EntityID pc2;
                if (Enum.TryParse(sleftSide, true, out pc2))
                {
                    output = !(MainManager.Instance.playerData.GetPlayerDataEntry(pc2) != null);
                }
                else
                {
                    output = false;
                }
                break;
            case TagEntry.Branch.HKERU:
                output = MainManager.Instance.KeruAvailable();
                break;
            case TagEntry.Branch.NHKERU:
                output = !MainManager.Instance.KeruAvailable();
                break;
        }

        return output;
    }
    public static int[] GetBranchDestination(TagEntry tag)
    {
        if (tag.tag != TagEntry.TextTag.Branch)
        {
            throw new ArgumentException("Parsed tag must be a branch.");
        }
        TagEntry.Branch b = (TagEntry.Branch)Enum.Parse(typeof(TagEntry.Branch), tag.args[0], true);

        int rightStartIndex;
        int destStartIndex;

        if (tag.args[1].Contains("arg"))
        {
            rightStartIndex = 2;
        }
        else
        {
            rightStartIndex = 3;
        }
        if (IsImmediateBranch(b))
        {
            //Parse as an immediate value
            destStartIndex = rightStartIndex + 1;
        }
        else
        {
            if (IsZeroArgBranch(b))
            {
                destStartIndex = 1;
            } else
            {
                if (IsOneArgBranch(b))
                {
                    destStartIndex = rightStartIndex;
                }
                else
                {
                    //Parse as a variable
                    if (tag.args[rightStartIndex].Contains("arg"))
                    {
                        destStartIndex = rightStartIndex + 1;
                    }
                    else
                    {
                        destStartIndex = rightStartIndex + 2;
                    }
                }
            }
        }

        int[] output = new int[tag.args.Length - destStartIndex];
        output[0] = int.Parse(tag.args[destStartIndex]);
        if (output.Length > 1)
        {
            output[1] = int.Parse(tag.args[destStartIndex + 1]);
        }
        return output;
    }
    public static bool HasTag(string s, TagEntry.TextTag tag)
    {
        FormattedString fs = new FormattedString(s);
        return fs.HasTag(tag);
    }

    //"arg" => return entire menuresult as string
    //"arg#" => split menuresult and return the # index thing (note that arg0 gives the 0 index thing)
    public static string ParseArg(MenuResult menuResult, string arg)
    {
        return ParseArg(menuResult.output.ToString(), arg);
    }
    public static string ParseArg(string menuResultString, string arg)
    {
        //split by commas

        //Invalid format
        if (!arg.Contains("arg"))
        {
            return null;
        }

        //what is the index?

        int index = -1;

        if (arg.Equals("arg"))
        {
            index = 0;
            return menuResultString;
        }
        else
        {
            bool check = int.TryParse(arg.Split("arg")[1], out index);
            if (!check)
            {
                return null;
            }
        }

        //Now time to parse our result

        string[] output = menuResultString.Split(",");

        if (index == -1)
        {
            return null;
        }

        if (index >= output.Length)
        {
            return null;
        }
        return output[index];
    }
    //makes a new menu result that has the thing set
    public static MenuResult SetMenuResult(MenuResult result, string set, string arg)
    {
        MenuResult output = result;

        if (!arg.Contains("arg"))
        {
            MainManager.Instance.lastTextboxMenuResult = output.output.ToString();
            return output;
        }

        //what is the index?

        int index = -1;

        if (arg.Equals("arg"))
        {
            index = 0;
            MainManager.Instance.lastTextboxMenuResult = set;
            return new MenuResult(set);
        }
        else
        {
            bool check = int.TryParse(arg.Split("arg")[1], out index);
            if (!check)
            {
                MainManager.Instance.lastTextboxMenuResult = output.output.ToString();
                return output;
            }
        }

        string[] split = result.ToString().Split(",");

        string[] outsplit = new string[Math.Max(split.Length, index + 1)];

        for (int i = 0; i < outsplit.Length; i++)
        {
            outsplit[i] = "";
        }
        for (int i = 0; i < split.Length; i++)
        {
            outsplit[i] = split[i];
        }

        outsplit[index] = set;

        string outstring = "";

        for (int i = 0; i < outsplit.Length; i++)
        {
            outstring += outsplit[i];
            outstring += ",";
        }

        MainManager.Instance.lastTextboxMenuResult = output.output.ToString();
        return output;
    }

    public string ParseVars(string[] vars)
    {
        //<var> = <var,0>
        string output = internalString;
        TagEntry[] tags = this.tags;

        for (int i = tags.Length - 1; i >= 0; i--)
        {
            if (tags[i].tag == TagEntry.TextTag.Var)
            {
                int arg = 0;
                if (tags[i].args.Length > 0)
                {
                    if (int.TryParse(tags[i].args[0], out int result))
                    {
                        arg = result;
                    }
                }
                //get rid of tag
                output = output.Substring(0, tags[i].trueStartIndex) + output.Substring(tags[i].trueEndIndex + 1, output.Length - 1 - tags[i].trueEndIndex);
                output = output.Insert(tags[i].trueStartIndex, vars[arg]);
            }
        }

        return output;
    } //Will throw exceptions if there is a var tag and there aren't enough vars in the array
    public string ParseVar(string var, int value)
    {
        //<var> = <var,0>
        //vars are 0 indexed
        string output = internalString;
        TagEntry[] tags = this.tags;

        for (int i = tags.Length - 1; i >= 0; i--)
        {
            if (tags[i].tag == TagEntry.TextTag.Var)
            {
                int arg = 0;
                if (tags[i].args.Length > 0)
                {
                    if (int.TryParse(tags[i].args[0], out int result))
                    {
                        arg = result;
                    }
                }

                if (arg == value)
                {
                    //get rid of tag
                    output = output.Substring(0, tags[i].trueStartIndex) + output.Substring(tags[i].trueEndIndex + 1, output.Length - 1 - tags[i].trueEndIndex);
                    output = output.Insert(tags[i].trueStartIndex, var);
                }
            }
        }

        return output;
    }
    public bool HasTag(TagEntry.TextTag tag)
    {
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].tag == tag)
            {
                return true;
            }
        }
        return false;
    }

    public string GetCleanString(bool parseEscapeSequences = false)//bool useLines = false)
    {
        if (parseEscapeSequences)
        {
            //strip tags before parsing
            return ParseEscapeSequences(StripTags(internalString));
        }
        else
        {
            return StripTags(internalString);
        }
    }
}

public class DialogueText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
