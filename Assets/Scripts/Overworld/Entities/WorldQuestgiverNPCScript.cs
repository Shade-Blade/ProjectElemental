using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldQuestgiverNPCScript : WorldNPCEntity
{
    public Quest.QuestType selectedQuestType;

    public GameObject questMenu;

    public override IEnumerator InteractCutscene()
    {
        questMenu = null;
        selectedQuestType = Quest.QuestType.None;
        GlobalQuestScript.Instance.RebuildQuestStates();

        //This is complex
        string[][] testTextFile = new string[6][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];

        testTextFile[0][0] = "Questgiver start text<prompt,Look at available quests,1,Take all available quests,2,Cancel,3,2>";
        testTextFile[1][0] = "Anything else?<prompt,Look at available quests,1,Take all available quests,2,Cancel,3,2>";
        testTextFile[2][0] = "No available quests";
        testTextFile[3][0] = "Take the \"<var,0>\" quest?<prompt,Yes,1,No,2,1>";
        testTextFile[4][0] = "Bye (no more available quests)";
        testTextFile[5][0] = "Bye";

        int state = 0;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        while (true)
        {
            if (state == 3)
            {
                questMenu = null;
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this));
                yield break;
            }

            if (GlobalQuestScript.Instance.availableQuests.Count == 0)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
                yield break;
            }

            if (state == 2)
            {
                //Easy to handle
                questMenu = null;
                GlobalQuestScript.Instance.TakeAllAvailableQuests();
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
                yield break;
            }

            if (state == 1)
            {
                //Very complex code
                yield return StartCoroutine(SelectQuestInterface());

                if (selectedQuestType == Quest.QuestType.None)
                {
                    questMenu = null;
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this));
                    yield break;
                }

                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this, new string[] {GlobalQuestScript.Instance.GetQuestText(selectedQuestType).name}));

                //Select quest dialogue
                int ynstate = 0;
                menuResult = MainManager.Instance.lastTextboxMenuResult;
                int.TryParse(menuResult, out ynstate);

                if (ynstate == 1)
                {
                    //Take the quest
                    QuestFlags qf = GlobalQuestScript.Instance.GetQuestFlags(selectedQuestType);

                    MainManager.Instance.SetGlobalFlag(qf.startedFlag, true);
                    GlobalQuestScript.Instance.RebuildQuestStates();

                    //yes
                    questMenu = null;
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
                    menuResult = MainManager.Instance.lastTextboxMenuResult;
                    int.TryParse(menuResult, out state);
                }
                else
                {
                    //no
                    questMenu = null;
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this));
                    yield break;
                }
            }
        }
    }

    public IEnumerator SelectQuestInterface()
    {
        //Build a new menu
        Pause_SectionQuest qs = Instantiate((GameObject)Resources.Load("Menu/Overworld_AvailableQuestMenu"), MainManager.Instance.Canvas.transform).GetComponent<Pause_SectionQuest>();
        questMenu = qs.gameObject;
        qs.Init();
        Pause_HandlerQuest qe = Pause_HandlerQuest.BuildMenu(qs, Pause_HandlerQuest.QuestSubpage.Available);
        MenuHandler b = qe;
        //this doesn't really matter, this object is invisible
        //(Only results in problems if the npc is destroyed while the menu is active, which is something that is really bad in general)
        b.transform.parent = transform;

        yield return new WaitUntil(() => qe.AvailableDone());

        int index = qe.AvailableIndex();

        if (index < 0)
        {
            selectedQuestType = Quest.QuestType.None;
        } else
        {
            selectedQuestType = GlobalQuestScript.Instance.availableQuests[index];
        }

        //Cleanup
        qe.Clear();
        Destroy(qe.gameObject);
        qs.Clear();
        Destroy(qs.gameObject);
    }
}
