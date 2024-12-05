using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalQuestScript : MonoBehaviour
{
    private static GlobalQuestScript instance;
    public static GlobalQuestScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GlobalQuestScript>(); //this should work
                if (instance == null)
                {
                    GameObject b = new GameObject("GlobalQuestScript");
                    GlobalQuestScript c = b.AddComponent<GlobalQuestScript>();
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

    public List<QuestFlags> questFlags;
    public List<QuestText> questText;   //unload when not needed
    public string[][] achievementText;

    //Calculated things
    public List<Quest.QuestType> startedQuests;
    public List<Quest.QuestType> availableQuests;
    public List<Quest.QuestType> completeQuests;

    public bool[] achievementStates;
    public bool[] globalAchievementStates;

    public void LoadQuestFiles()
    {
        if (questFlags == null)
        {
            LoadQuestFlags();
        }
        if (questText == null)
        {
            LoadQuestText();
        }
    }
    public void LoadQuestText()
    {
        string[][] file = MainManager.GetAllTextFromFile("DialogueText/QuestText");

        questText = new List<QuestText>();

        //??? the -1 comes from a weird extra line
        for (int i = 1; i < file.Length - 1; i++)
        {
            questText.Add(QuestText.Parse(file[i], (Quest.QuestType)(i)));
        }
    }
    public void LoadQuestFlags()
    {
        string[][] file = MainManager.GetAllTextFromFile("Data/QuestData");

        questFlags = new List<QuestFlags>();

        //??? the -1 comes from a weird extra line
        for (int i = 1; i < file.Length - 1; i++)
        {
            questFlags.Add(QuestFlags.Parse(file[i], (Quest.QuestType)(i)));
        }
    }
    public void LoadAchievementText()
    {
        string[][] file = MainManager.GetAllTextFromFile("DialogueText/AchievementText");
        achievementText = new string[(int)(MainManager.Achievement.EndOfTable) - 1][];

        for (int i = 1; i < file.Length - 1; i++)
        {
            achievementText[i - 1] = file[i];
        }
    }

    public QuestFlags GetQuestFlags(Quest.QuestType qt)
    {
        if (questFlags == null)
        {
            LoadQuestFlags();
        }

        return questFlags[(int)qt - 1];
    }
    public QuestText GetQuestText(Quest.QuestType qt)
    {
        if (questText == null)
        {
            LoadQuestText();
        }

        return questText[(int)qt - 1];
    }
    public string GetAchievementText(MainManager.Achievement ach, int index)
    {
        if (achievementText == null)
        {
            LoadAchievementText();
        }

        return achievementText[(int)ach - 1][index];
    }

    public void TakeAllAvailableQuests()
    {
        RebuildQuestStates();
        for (int i = 0; i < availableQuests.Count; i++)
        {
            //Use quest data to set all the taken flags
            QuestFlags qf = GetQuestFlags(availableQuests[i]);

            MainManager.Instance.SetGlobalFlag(qf.startedFlag, true);
        }
        RebuildQuestStates();
    }

    public void RebuildQuestStates()
    {
        if (questFlags == null)
        {
            LoadQuestFlags();
        }

        availableQuests = new List<Quest.QuestType>();
        startedQuests = new List<Quest.QuestType>();
        completeQuests = new List<Quest.QuestType>();

        for (int i = 1; i < (int)(Quest.QuestType.EndOfTable); i++)
        {
            Quest.QuestType current = (Quest.QuestType)i;
            Quest.QuestState qs = Quest.DetermineQuestState(current);           

            if (qs == Quest.QuestState.Available)
            {
                availableQuests.Add(current);
            }
            if (qs == Quest.QuestState.Taken)
            {
                startedQuests.Add(current);
            }
            if (qs == Quest.QuestState.Complete)
            {
                completeQuests.Add(current);
            }
        }
    }
    public void UnloadQuestStates()
    {
        availableQuests = null;
        startedQuests = null;
        completeQuests = null;
    }

    public void RebuildAchievementStates()
    {
        achievementStates = new bool[(int)(MainManager.Achievement.EndOfTable) - 1];
        globalAchievementStates = new bool[(int)(MainManager.Achievement.EndOfTable) - 1];

        for (int i = 0; i < achievementStates.Length; i++)
        {
            achievementStates[i] = MainManager.Instance.CheckAchivement((MainManager.Achievement)(i + 1));
        }

        //debug / unused
        //need to figure out how to acquire the global achievement state if that's ever a thing
        //(not sure what I'll use this for)
        for (int i = 0; i < globalAchievementStates.Length; i++)
        {
            globalAchievementStates[i] = true;
        }
        globalAchievementStates[0] = false;
    }
}

public class Quest
{
    public enum QuestType
    {
        None = 0,           //Not a quest
        Prologue_Test,
        EndOfTable          //Not a quest
    }
    public QuestType type;

    public enum QuestState
    {
        Unavailable,    //You don't get to see the unavailable quests (since these are all the future quests)
        Available,
        Taken,
        Complete
    }

    //these can be referenced from the global script
    /*
    public QuestFlags flags;
    public QuestText text;

    public Quest(QuestType p_type, QuestFlags p_flags = null, QuestText p_text = null)
    {
        type = p_type;
        flags = p_flags;
        text = p_text;
    }
    */

    public static MainManager.GlobalFlag TryStartFlagTranslation(QuestType type)
    {
        string newName = "GF_QuestStart_" + type.ToString();

        MainManager.GlobalFlag output = (MainManager.GlobalFlag.GF_None);
        Enum.TryParse(newName, out output);

        if (output == MainManager.GlobalFlag.GF_None)
        {
            Debug.LogWarning("[Start Flag Translation] Can't parse " + newName);
        }

        return output;
    }
    public static MainManager.GlobalFlag TryCompleteFlagTranslation(QuestType type)
    {
        string newName = "GF_QuestComplete_" + type.ToString();

        MainManager.GlobalFlag output = (MainManager.GlobalFlag.GF_None);
        Enum.TryParse(newName, out output);

        if (output == MainManager.GlobalFlag.GF_None)
        {
            Debug.LogWarning("[Complete Flag Translation] Can't parse " + newName);
        }

        return output;
    }

    //Note: "Sequence breaking" the quest flags will result in strange behavior
    //Story quests will force activate the started flag alongside changing the story progress so that the story quest never appears in the Available menu
    //(I will probably also make the availability condition contain the starting flag also)
    //(That may be a way to make "secret" quests or just quests that don't appear in the interface)
    public static QuestState DetermineQuestState(QuestType type)
    {
        QuestFlags qf = GlobalQuestScript.Instance.GetQuestFlags(type);

        if (MainManager.Instance.GetGlobalFlag(qf.completedFlag))
        {
            return QuestState.Complete;
        }

        if (MainManager.Instance.GetGlobalFlag(qf.startedFlag))
        {
            return QuestState.Taken;
        }

        if (qf.CheckAvailability())
        {
            return QuestState.Available;
        }

        return QuestState.Unavailable;
    }
    public static List<bool> CheckProgressFlags(QuestType type)
    {
        QuestFlags qf = GlobalQuestScript.Instance.GetQuestFlags(type);
        return qf.CheckProgressFlags();
    }
    public static QuestText GetText(QuestType type)
    {
        return GlobalQuestScript.Instance.GetQuestText(type);
    }
}

public class QuestText
{
    public string name;
    public string issuer;
    public int difficulty;      //1 = fetch quest / find a thing (Not a battle thing), 2 = probably requires battles, 3 = boss related, 4 = many bosses (story stuff is here, but also major long sidequests)
    public string description;
    public List<string> text;   //each segment of text (so unflagged text, then text for after the first progress marker...)    (you can incorporate the started flag and completed flag for extra text in those cases)
    //Note that started and complete flags have no effect on the text by default

    public static QuestText Parse(string[] entry, Quest.QuestType type)
    {
        //More comma separated values
        //0 = type
        //1 = name
        //2 = issuer
        //3 = difficulty
        //4 = description
        //5+ text

        QuestText output = new QuestText();

        Quest.QuestType check = (Quest.QuestType)(-1);

        if (Enum.TryParse(entry[0], out check))
        {
            if (check != type)
            {
                Debug.LogWarning("[Quest Text Parsing] Mismatch between quest types, method was given " + type + " vs " + check);
            }
        }
        else
        {
            Debug.LogError("[Quest Text Parsing] Could not parse quest type " + entry[0]);
        }

        string name = entry[1];
        string issuer = entry[2];
        int difficulty = 1;
        int.TryParse(entry[3], out difficulty);
        string description = entry[4];
        List<string> text = new List<string>();

        for (int i = 5; i < entry.Length; i++)
        {
            if (entry[i].Length > 0)
            {
                text.Add(entry[i]);
            }
        }

        output.name = name;
        output.issuer = issuer;
        output.difficulty = difficulty;
        output.description = description;
        output.text = text;

        return output;
    }

    public string GetSideText()
    {
        string output = "Issuer: " + issuer;
        output += "\n";
        output += "Difficulty:";

        if (difficulty > 4)
        {
            output += "<rainbow>";
        } else
        {
            output += " ";
        }

        for (int i = 0; i < difficulty; i++)
        {
            output += "\u2605";
        }

        if (difficulty > 4)
        {
            output += "</rainbow>";
        }

        return output;
    }

    public List<string> FilterTextList(List<bool> boolList)
    {
        List<string> output = new List<string>();

        for (int i = 0; i < text.Count; i++)
        {
            if (i > boolList.Count - 1 || boolList[i])
            {
                output.Add(text[i]);
            }
        }

        return output;
    }
}

public class QuestFlags
{
    public QuestAvailabilityCondition availabilityCondition;
    public MainManager.GlobalFlag startedFlag;
    public MainManager.GlobalFlag completedFlag;
    public List<MainManager.GlobalFlag> progressFlags;  //First segment of text is the flagless one, then each subsequent segment corresponds to a flag here. Note that flags can be activated "out of order" (so nonlinear quests can work)

    public class QuestAvailabilityCondition
    {
        public List<MainManager.GlobalFlag> flagList;
        public MainManager.StoryProgress storyProgress; //You have to be at this progress or later

        //Uses a pipe separated list of flags. First argument can be Story Progress (but it can also be a flag)
        public static QuestAvailabilityCondition Parse(string text)
        {
            string[] stringList = text.Split('|');

            QuestAvailabilityCondition output = new QuestAvailabilityCondition();

            MainManager.StoryProgress sp = (MainManager.StoryProgress)(-1);
            MainManager.GlobalFlag gf = (MainManager.GlobalFlag)(-1);

            Enum.TryParse(stringList[0], out sp);

            List<MainManager.GlobalFlag> outputFlags = new List<MainManager.GlobalFlag>();

            int startIndex = 0;
            if ((int)sp != -1)
            {
                output.storyProgress = sp;
                startIndex = 1;
            }

            if (stringList.Length > startIndex)
            {
                for (int i = startIndex; i < stringList.Length; i++)
                {
                    gf = (MainManager.GlobalFlag)(-1);
                    if (Enum.TryParse(stringList[i], out gf))
                    {

                    }
                    else
                    {
                        Debug.LogError("[Quest Requirement Parsing] Could not parse requirement " + stringList[i]);
                    }

                    outputFlags.Add(gf);
                }
            }

            output.flagList = outputFlags;
            return output;
        }
    }

    public bool CheckStarted()
    {
        return MainManager.Instance.GetGlobalFlag(startedFlag);
    }
    public bool CheckCompleted()
    {
        return MainManager.Instance.GetGlobalFlag(completedFlag);
    }
    public List<bool> CheckProgressFlags()  //note: used to check the text list
    {
        List<bool> output = new List<bool>
        {
            true    //first segment of text is conditionless (i.e. this is the extended text of the description)
        };

        for (int i = 0; i < progressFlags.Count; i++)
        {
            output.Add(MainManager.Instance.GetGlobalFlag(progressFlags[i]));
        }

        return output;
    }
    public bool CheckAvailability()
    {
        MainManager.StoryProgress sp = availabilityCondition.storyProgress;
        if ((int)sp != -1)
        {
            string storyProgress = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_StoryProgress);
            if (storyProgress == null)
            {
                storyProgress = MainManager.StoryProgress.SP_None.ToString();
            }
            MainManager.StoryProgress parsedProgress = Enum.Parse<MainManager.StoryProgress>(storyProgress);
            if ((int)parsedProgress < (int)sp)
            {
                return false;
            }
        }

        if (availabilityCondition.flagList == null)
        {
            return true;
        }

        for (int i = 0; i < availabilityCondition.flagList.Count; i++)
        {
            if (!MainManager.Instance.GetGlobalFlag(availabilityCondition.flagList[i]))
            {
                return false;
            }
        }

        return true;
    }

    //format
    //0 = [type]
    //1 = [quest availability condition list]
    //2 = [progress flags]
    //Was   [type],[quest availability condition list],[started flag],[completion flag],[progress flags]
    //[type],[quest availability condition list],[progress flags]
    public static QuestFlags Parse(string[] entry, Quest.QuestType type)
    {
        QuestFlags output = new QuestFlags();

        QuestAvailabilityCondition qac = null;
        MainManager.GlobalFlag sf = (MainManager.GlobalFlag)(-1);
        MainManager.GlobalFlag cf = (MainManager.GlobalFlag)(-1);
        MainManager.GlobalFlag pfI = (MainManager.GlobalFlag)(-1);
        List<MainManager.GlobalFlag> pf = new List<MainManager.GlobalFlag>();

        Quest.QuestType check = (Quest.QuestType)(-1);

        if (Enum.TryParse(entry[0], out check))
        {
            if (check != type)
            {
                Debug.LogWarning("[Quest Flag Parsing] Mismatch between quest types, method was given " + type + " vs " + check);
            }
        } else
        {
            Debug.LogError("[Quest Flag Parsing] Could not parse quest type " + entry[0] + " (when given reference " + type + ")");
        }

        qac = QuestAvailabilityCondition.Parse(entry[1]);

        /*
        if (Enum.TryParse(entry[2], out sf))
        {

        } else
        {
            Debug.LogError("[Quest Flag Parsing] Could not parse global flag (startedFlag) " + entry[2]);
        }
        if (Enum.TryParse(entry[3], out cf))
        {

        } else
        {
            Debug.LogError("[Quest Flag Parsing] Could not parse global flag (completionFlag) " + entry[3]);
        }
        */
        sf = Quest.TryStartFlagTranslation(type);
        cf = Quest.TryCompleteFlagTranslation(type);
        

        string[] pfText = entry[2].Split('|');
        //now can support an empty string in progress flags
        if (pfText.Length > 1 || (pfText.Length > 0 && pfText[0].Length > 0))
        {
            for (int i = 0; i < pfText.Length; i++)
            {
                pfI = (MainManager.GlobalFlag)(-1);
                if (Enum.TryParse(pfText[i], out pfI))
                {

                }
                else
                {
                    Debug.LogError("[Quest Flag Parsing] Could not parse global flag (progressFlag) " + i + " " + pfText[i]);
                }
                if ((int)pfI != -1)
                {
                    pf.Add(pfI);
                }
            }
        }

        output.availabilityCondition = qac;
        output.startedFlag = sf;
        output.completedFlag = cf;
        output.progressFlags = pf;

        return output;
    }
}