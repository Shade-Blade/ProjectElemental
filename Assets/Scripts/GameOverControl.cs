using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using static MainManager;

public class GameOverControl : MonoBehaviour
{
    public TMPro.TMP_Text text;

    public const float FADEIN_TIME = 1f;
    public float fadeIn;

    public bool menuCreated = false;
    private MenuHandler menuHandler;
    private bool inMenu;
    public MenuResult menuResult;

    public bool allowBattleRetry = false;

    public bool chosePause;

    public enum GameOverOptions
    {
        Retry,  //Restart Battle
        PauseMenu,  //Change Loadout and Restart
        SaveFile,   //Reload Last Save (?:??:?? ago)
        MainMenu    //Go back to the Main Menu
    }

    //make the game over screen appear (but the caller has to clean everything else up on its own)
    public static GameOverControl GameOverStatic(bool p_allowBattleRetry = false)
    {
        GameObject o = Instantiate(MainManager.Instance.gameOverObject);
        o.transform.SetParent(MainManager.Instance.Canvas.transform);
        o.transform.localPosition = Vector3.zero;

        GameOverControl gvc = o.GetComponent<GameOverControl>();
        gvc.fadeIn = 0;
        gvc.menuCreated = false;
        gvc.allowBattleRetry = p_allowBattleRetry;

        return gvc;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (allowBattleRetry)
        {
            MainManager.Instance.gameOverPlayerData = MainManager.Instance.playerData.Copy();
        }
        chosePause = false;

        MainManager.Instance.SaveBattleLossCount();
    }

    // Update is called once per frame
    void Update()
    {
        float fade = Mathf.Clamp01(fadeIn / FADEIN_TIME);
        text.color = new Color(1, 1, 1, fade);
        fadeIn += Time.deltaTime;

        if (fadeIn > FADEIN_TIME && !menuCreated)
        {
            menuCreated = true;
            CreateMenu();
        }

        if (!inMenu)
        {
            if (menuHandler != null)
            {
                menuResult = menuHandler.GetFullResult();
                if (menuResult != null)
                {
                    MainManager.Instance.lastTextboxMenuResult = menuResult.output.ToString();
                }
                else
                {
                    MainManager.Instance.lastTextboxMenuResult = null;
                }
                menuHandler.Clear();
                Destroy(menuHandler.gameObject);
                menuHandler = null;

                //You selected something, what now?
                int result = int.Parse(menuResult.ToString());
                switch (result)
                {
                    case 0:
                        BattleControl.StartBattleStatic(MainManager.Instance.battleStartArguments);
                        Destroy(gameObject);
                        return;
                    case 1:
                        MainManager.Instance.isPaused = true;
                        MainManager.Instance.pauseMenuScript = Pause_SectionBase.buildMenu();
                        chosePause = true;
                        break;
                    case 2:
                        MainManager.Instance.LoadSave(MainManager.Instance.saveIndex);
                        Destroy(gameObject);
                        return;
                    case 3:
                        MainManager.Instance.ReturnToStartMenu();
                        Destroy(gameObject);
                        break;
                }
            }
        }

        if (MainManager.Instance.isPaused)
        {
            if (InputManager.GetButtonDown(InputManager.Button.Start))
            {
                //isPaused = false;
                MainManager.Instance.pauseMenuScript.Unpause();
                BattleControl.StartBattleStatic(MainManager.Instance.battleStartArguments);
                Destroy(gameObject);
                return;
            }
        }

        if (chosePause && !MainManager.Instance.isPaused)
        {
            //isPaused = false;
            //MainManager.Instance.pauseMenuScript.Unpause();
            BattleControl.StartBattleStatic(MainManager.Instance.battleStartArguments);
            Destroy(gameObject);
            return;
        }
    }

    //then we build the retry battle menu
    //In some cases this will only let you return to last save (normal battles, overworld game overs)
    public void CreateMenu()
    {
        //int argCount = 1;
        string[] tempText = null;
        string[] tempArgs = null;

        int tempCancel = -1;    //B = nothing happens

        string time = MainManager.ParseTime(MainManager.Instance.playTime - MainManager.Instance.lastSaveTimestamp);

        if (MainManager.Instance.lastSaveTimestamp == 0)
        {
            time = "N/A";
        }

        if (allowBattleRetry)
        {
            //argCount = 9;
            tempText = new string[4];
            tempArgs = new string[4];
            tempText[0] = "Retry Battle";
            tempArgs[0] = "0";
            tempText[1] = "Change Loadout";
            tempArgs[1] = "1";
            if (time.Equals("N/A"))
            {
                tempText[2] = "Reload Save (New Game)";
            }
            else
            {
                tempText[2] = "Reload Save (" + time + " ago)";
            }
            tempArgs[2] = "2";
            tempText[3] = "Return to the Main Menu";
            tempArgs[3] = "3";
        }
        else
        {
            //argCount = 5;
            tempText = new string[2];
            tempArgs = new string[2];
            if (time.Equals("N/A"))
            {
                tempText[0] = "Reload Save (New Game)";
            }
            else
            {
                tempText[0] = "Reload Save (" + time + " ago)";
            }
            tempArgs[0] = "2";
            tempText[1] = "Return to the Main Menu";
            tempArgs[1] = "3";
        }



        menuHandler = PromptBoxMenu.BuildMenu(tempText, tempArgs, tempCancel);
        inMenu = true;
        menuHandler.menuExit += MenuExit;
    }

    public void MenuExit(object sender, MenuExitEventArgs meea)
    {
        inMenu = false;
    }
}
