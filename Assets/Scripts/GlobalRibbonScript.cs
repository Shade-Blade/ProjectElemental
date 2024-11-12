using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Ribbon;

public class GlobalRibbonScript : MonoBehaviour
{
    private static GlobalRibbonScript instance;
    public static GlobalRibbonScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GlobalRibbonScript>(); //this should work
                if (instance == null)
                {
                    GameObject b = new GameObject("GlobalRibbonScript");
                    GlobalRibbonScript c = b.AddComponent<GlobalRibbonScript>();
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


    string[][] ribbonText;
    RibbonType pastWRibbonType;
    RibbonType pastLRibbonType;

    //make this a data file or something?
    //This is a (?) x 3 table (first entry is reserved for a null value)
    [SerializeField]
    public Color[][] ribbonColors;

    public Color lTestRibbonColorA;
    public Color lTestRibbonColorB;
    public Color lTestRibbonColorC;

    public void LoadRibbonText()
    {
        ribbonText = MainManager.GetAllTextFromFile("DialogueText/RibbonText");    //MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/ItemText").text);
    }
    public void UnloadRibbonText()
    {
        ribbonText = null;
    }

    public string[][] GetTextFile()
    {
        return ribbonText;
    }

    public void LoadRibbonColorTable()
    {
        ribbonColors = new Color[14][];

        //dark (outline), medium, light (most of the ribbon is light colored usually, but some of Luna's sprites have a lot of medium color)
        //Note that wilex doesn't have any medium color in his ribbon sprites

        //Note: some of these use "illegal" colors

        ribbonColors[0] = new Color[] { new Color(0f, 0f, 0f, 0), new Color(0f, 0f, 0f, 0) , new Color(0f, 0f, 0f, 0) };    //note: displays weirdly (silhouette shows up?)
        ribbonColors[(int)RibbonType.SafetyRibbon] = new Color[] { new Color(0f, 0.4f, 0f, 1), new Color(0f, 0.75f, 0f, 1), new Color(0f, 1f, 0f, 1) };
        ribbonColors[(int)RibbonType.SharpRibbon] = new Color[] { new Color(0.4f, 0f, 0f, 1), new Color(0.75f, 0f, 0f, 1), new Color(1f, 0f, 0f, 1) };
        ribbonColors[(int)RibbonType.BeginnerRibbon] = new Color[] { new Color(0.4f, 0.25f, 0.1f, 1), new Color(0.6f, 0.36f, 0.2f, 1), new Color(0.776f, 0.533f, 0.349f, 1) };
        ribbonColors[(int)RibbonType.ExpertRibbon] = new Color[] { new Color(0.3f, 0.3f, 0.5f, 1), new Color(0.85f, 0.87f, 0.89f, 1), new Color(0.95f, 1.05f, 1.1f, 1) };
        ribbonColors[(int)RibbonType.ChampionRibbon] = new Color[] { new Color(0.4f, 0.2f, 0f, 1), new Color(0.9f, 0.8f, 0.2f, 1), new Color(1.4f, 0.95f, 0.5f, 1) };
        ribbonColors[(int)RibbonType.StaticRibbon] = new Color[] { new Color(0.4f, 0.4f, 0.2f, 1), new Color(0.75f, 0.75f, 0.325f, 1), new Color(1, 1f, 0.5f, 1) };
        ribbonColors[(int)RibbonType.SlimyRibbon] = new Color[] { new Color(0.1f, -0.1f, 0.2f, 1), new Color(0.2f, 0, 0.3f, 1), new Color(0.4f, 0, 0.6f, 1) };
        ribbonColors[(int)RibbonType.FlashyRibbon] = new Color[] { new Color(0.4f, 0.5f, 0.4f, 1), new Color(0.7f, 1, 0.7f, 1), new Color(0.85f, 2f, 0.85f, 1) };
        ribbonColors[(int)RibbonType.SoftRibbon] = new Color[] { new Color(0.2f, 0.2f, 0.4f, 1), new Color(0.3f, 0.3f, 0.75f, 1), new Color(0.4f, 0.4f, 1f, 1) };
        ribbonColors[(int)RibbonType.MimicRibbon] = new Color[] { new Color(0.3f, 0.2f, 0.4f, 1), new Color(0.5f, 0.3f, 0.75f, 1), new Color(0.7f, 0.4f, 1f, 1) };
        ribbonColors[(int)RibbonType.ThornyRibbon] = new Color[] { new Color(0.4f, -0.5f, 0.2f, 1), new Color(0.75f, 0.3f, 0.6f, 1), new Color(1f, 0.4f, 0.8f, 1) };
        ribbonColors[(int)RibbonType.DiamondRibbon] = new Color[] { new Color(0f, 0.4f, 0.4f, 1), new Color(0.6f, 0.9f, 0.9f, 1), new Color(0.8f, 1.5f, 1.5f, 1) };
        ribbonColors[(int)RibbonType.RainbowRibbon] = new Color[] { new Color(0.5f, 0f, 1, 1), new Color(0.75f, 0f, 1, 1), new Color(1f, 0.2f, 1, 1) };
    }

    public Color[] GetRibbonColors(RibbonType r)
    {
        if (ribbonColors == null || ribbonColors.Length < (int)RibbonType.EndOfTable)
        {
            LoadRibbonColorTable();
        }

        return ribbonColors[(int)r];
    }

    public string GetRibbonName(RibbonType i)
    {
        if (ribbonText == null)
        {
            LoadRibbonText();
        }

        return ribbonText[(int)(i)][1];
    }

    public string GetRibbonDescription(RibbonType i, bool ribbonPower)
    {
        if (ribbonText == null)
        {
            LoadRibbonText();
        }

        string output = "";

        int length = ribbonText[(int)(i)].Length;
        if (ribbonText[(int)(i)][2].Length > 0)
        {
            output += "Block: " + ribbonText[(int)(i)][2] + "<line>";
        }
        if (ribbonText[(int)(i)][3].Length > 0)
        {
            output += "Rest: " + ribbonText[(int)(i)][3] + "<line>";
        }
        if (ribbonText[(int)(i)][4].Length > 0)
        {
            output += "<descriptionnoticecolor>" + ribbonText[(int)(i)][4] + "</descriptionnoticecolor><line>";
        }
        if (ribbonPower && length > 5 && ribbonText[(int)(i)][5].Length > 0)
        {
            output += "<descriptionwarncolor>(Ribbon Power: " + ribbonText[(int)(i)][5] + ")</descriptionwarncolor>";
        }
        //Debug.Log(output);
        //Debug.Log((int)itemText[(int)(i)][5][0]);

        return output;
    }

    public string GetRibbonText(RibbonType i, int index)
    {
        if (ribbonText == null)
        {
            LoadRibbonText();
        }

        return ribbonText[(int)i][index];
    }

    public static Sprite GetRibbonSpriteFromText(string ribbon)
    {
        RibbonType ribbonType;

        Enum.TryParse(ribbon, out ribbonType);

        return GetRibbonSprite(ribbonType);
    }

    public static Sprite GetRibbonSprite(RibbonType ribbonType)
    {
        return MainManager.Instance.ribbonSprites[(int)(ribbonType) - 1];
    }

    public void RibbonColorUpdate()
    {
        PlayerData.PlayerDataEntry wilex = null;
        PlayerData.PlayerDataEntry luna = null;
        bool wmimic = false;
        bool lmimic = false;
        bool wrainbow = false;
        bool lrainbow = false;
        PlayerData pd = null;

        if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
        {
            if (BattleControl.Instance == null)
            {
                return;
            }
            pd = BattleControl.Instance.playerData;
            if (pd == null)
            {
                return;
            }

            wilex = BattleControl.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
            luna = BattleControl.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
        }
        else
        {
            pd = MainManager.Instance.playerData;
            if (pd == null)
            {
                return;
            }

            wilex = MainManager.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
            luna = MainManager.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
        }

        RibbonType currentW = pastWRibbonType;
        RibbonType currentL = pastLRibbonType;

        if (wilex != null)
        {
            currentW = pd.GetVisualRibbon(BattleHelper.EntityID.Wilex).type;
            wmimic = wilex.ribbon.type == RibbonType.MimicRibbon;
            wrainbow = wilex.ribbon.type == RibbonType.RainbowRibbon;
        }
        if (luna != null)
        {
            currentL = pd.GetVisualRibbon(BattleHelper.EntityID.Luna).type;
            lmimic = luna.ribbon.type == RibbonType.MimicRibbon;
            lrainbow = luna.ribbon.type == RibbonType.RainbowRibbon;
        }

        //if (currentW != pastWRibbonType)
        //{
        pastWRibbonType = currentW;
        Color[] wc = GetRibbonColors(currentW);
        Shader.SetGlobalVector("_WRibbonColorA", wc[0]);
        Shader.SetGlobalVector("_WRibbonColorB", wc[1]);
        Shader.SetGlobalVector("_WRibbonColorC", wc[2]);
        Shader.SetGlobalFloat("_WRibbonMimic", wmimic ? 1 : 0);
        Shader.SetGlobalFloat("_WRibbonRainbow", (wrainbow || (lrainbow && wmimic)) ? 1 : 0);
        //}
        //if (currentL != pastLRibbonType)
        //{
        pastLRibbonType = currentL;
        Color[] lc = GetRibbonColors(currentL);
        //Shader.SetGlobalVector("_LRibbonColorA", GetRibbonColor(currentL));
        Shader.SetGlobalVector("_LRibbonColorA", lc[0]);
        Shader.SetGlobalVector("_LRibbonColorB", lc[1]);
        Shader.SetGlobalVector("_LRibbonColorC", lc[2]);
        Shader.SetGlobalFloat("_LRibbonMimic", lmimic ? 1 : 0);
        Shader.SetGlobalFloat("_LRibbonRainbow", (lrainbow || (wrainbow && lmimic)) ? 1 : 0);
        //}
    }
}

[System.Serializable]
public struct Ribbon
{
    public enum RibbonType
    {
        //Ribbon power triples the power but also gives you 3t of the specified effect (*though tokens are infinite duration)
        None = 0,
        SafetyRibbon,       //[Lime green] Hold A -> Safety Block: Block but lose 2 energy, Rest: No effect
        SharpRibbon,        //[Red] Hold B -> Sharp Block: Take 1 more damage instead of -1 damage, but enemy takes 1 damage from contact hazard
        BeginnerRibbon,     //[Bronze] Increased guard window, Rest: (1/2 agility) stamina (Defocus 1, sunder 1, enervate 1)
        ExpertRibbon,       //[Silver] Decreased guard window, but block is -3 instead of -1, Rest: Clear negative effects (Seal, Immunity)
        ChampionRibbon,     //[Gold] Double A -> Perfect Block: Block with -4, Rest: 5 hp, 5 ep if alone (last alive), nothing otherwise (Attack down 1, defense down 1, endurance down 1)
        StaticRibbon,       //[Yellow] Left + A -> No damage reduction, but get 1/2 energy recovery, Rest: 2 ep (Paralyze)
        SlimyRibbon,        //[Black] Up + A -> 2 dark damage contact hazard, Rest: 1/3t atk (Poison)
        FlashyRibbon,       //[Mint Green, sparkly] Down + A -> Blockable effects are fully negated, Rest: 1/3t def (Dizzy)
        SoftRibbon,         //[Blue] Right + A -> -2 instead of -1, Rest: 2 hp (Sleep)
        MimicRibbon,        //[Purple] N/A (copies effect of other char's ribbon)
        ThornyRibbon,       //[Magenta] Can't block, rest gives 1 damage (Focus 2, Berserk), but unconditional +1 attack and take 1 damage per turn
        DiamondRibbon,      //[Cyan] B + A -> No damage reduction, but get 100% money gain, Rest: 15 coins (Freeze)
        RainbowRibbon,      //[Rainbow] Combines all other guards, but no rest effect
        EndOfTable
    }

    public RibbonType type;
    public int ribbonCount;
    public int bonusData;

    public Ribbon(RibbonType p_type, int p_ribbonCount = 0, int p_bonusData = 0)
    {
        type = p_type;
        ribbonCount = p_ribbonCount;
        bonusData = p_bonusData;
    }

    public static string GetName(RibbonType r)
    {
        return GlobalRibbonScript.Instance.GetRibbonName(r);
        //return b.ToString();
    }
    public static string GetName(Ribbon r)
    {
        if (MainManager.Instance.Cheat_SeePickupCounts)
        {
            return GetName(r.type) + " " + r.ribbonCount;
        }
        else
        {
            return GetName(r.type);
        }
    }
    public static string GetDescription(RibbonType r, bool ribbonPower = false)
    {
        return GlobalRibbonScript.Instance.GetRibbonDescription(r, ribbonPower);
        //return b.ToString() + " description";
    }
    public static string GetDescription(Ribbon r, bool ribbonPower = false)
    {
        return GetDescription(r.type, ribbonPower);
    }
    public static string GetSpriteString(RibbonType r)
    {
        return "<ribbonsprite," + r.ToString() + ">";
    }
    public static string GetSpriteString(Ribbon r)
    {
        return GetSpriteString(r.type);
    }

    //Inverse of toString
    public static Ribbon Parse(string input)
    {
        Ribbon output = new Ribbon();

        string[] split = input.Split("|");

        if (split.Length > 0)
        {
            //this looks a bit sus
            Enum.TryParse(split[0], true, out output.type);
        }

        if (split.Length > 1)
        {
            int.TryParse(split[1], out output.ribbonCount);
        }

        if (split.Length > 2)
        {
            int.TryParse(split[2], out output.bonusData);
        }

        return output;
    }
    public override string ToString()
    {
        if (ribbonCount == 0 && bonusData == 0)
        {
            return type.ToString();
        }
        if (bonusData == 0)
        {
            return type.ToString() + "|" + ribbonCount;
        }
        return type.ToString() + "|" + ribbonCount + "|" + bonusData;
    }
    public static List<Ribbon> ParseList(string input)
    {
        string[] split = input.Split(",");
        List<Ribbon> output = new List<Ribbon>();

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
    public static string ListToString(List<Ribbon> list)
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

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Ribbon))
        {
            return false;
        }
        Ribbon r = ((Ribbon)(obj));
        if (type != r.type)
        {
            return false;
        }
        if (ribbonCount != r.ribbonCount)
        {
            return false;
        }
        if (bonusData != r.bonusData)
        {
            return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(type);
    }
}
