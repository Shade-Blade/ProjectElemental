using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpLauncherScript : WorldObject, ITextSpeaker, ISignalReceiver
{
    public TextDisplayer textbox;
    public LauncherScript launcher;

    public int exitID;

    public string signstring;

    public bool active = false;

    public void Start()
    {
        launcher.target = this;
    }

    public void ReceiveSignal(int signal)
    {
        if (active)
        {
            return;
        }
        active = true;
        StartCoroutine(MainManager.Instance.ExecuteCutscene(WarpCutscene()));
    }

    public virtual IEnumerator WarpCutscene()
    {
        string[][] testTextFile = new string[2][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[0][0] = FormattedString.ReplaceTextFileShorthand(signstring);
        testTextFile[1][0] = "<system>Warp to Floor <var,0>?<prompt,Yes,1,No,2,1>";

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out int state);

        if (state == NumberMenu.CANCEL_VALUE)
        {
            //cancel
            foreach (WorldFollower wf in WorldPlayer.Instance.followers)
            {
                WorldPlayer.Instance.FollowerWarp(WorldPlayer.Instance.FacingVector() * 0.075f);
                wf.ScriptedLaunch(3 * (Vector3.back + Vector3.up), 0f);
            }
            WorldPlayer.Instance.ScriptedLaunch(3 * (Vector3.back + Vector3.up), 0f);
            Debug.Log("launch " + WorldPlayer.Instance.rb.velocity);
            yield return new WaitForSeconds(0.5f);  //block the next instance of this cutscene from starting?
            active = false;
            yield break;
        }

        int floortowarp = state;

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this, new string[] { floortowarp.ToString()}));
        menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        if (state == 2)
        {
            //cancel
            foreach (WorldFollower wf in WorldPlayer.Instance.followers)
            {
                WorldPlayer.Instance.FollowerWarp(WorldPlayer.Instance.FacingVector() * 0.075f);
                wf.ScriptedLaunch(3 * (Vector3.back + Vector3.up), 0f);
            }
            WorldPlayer.Instance.ScriptedLaunch(3 * (Vector3.back + Vector3.up), 0f);
            yield return new WaitForSeconds(0.5f);  //block the next instance of this cutscene from starting?
            active = false;
            yield break;
        }

        StartCoroutine(MainManager.Instance.ExecuteCutscene(FlyCutscene(floortowarp)));
    }

    public IEnumerator FlyCutscene(int floor) {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerShockwave2"), MainManager.Instance.mapScript.transform);
        effect.transform.position = transform.position;

        PlayerData pd = MainManager.Instance.playerData;

        //go to floor
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, (floor).ToString());
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitAltar, (floor / 10).ToString());


        Debug.Log(MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor));

        //give you stuff
        //level = 25 * (floor/90) capped at 25

        int newLevel = Mathf.FloorToInt(25f * ((floor - 1) / 90f));
        if (newLevel < 1)
        {
            newLevel = 1;
        }
        else if (newLevel > 25)
        {
            newLevel = 25;
        }
        pd.level = newLevel;
        for (int i = 1; i < newLevel; i++)
        {
            if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Envy))
            {
                pd.upgrades.Add(PlayerData.LevelUpgrade.SP);
            } else
            {
                if (i % 3 == 0)
                {
                    pd.upgrades.Add(PlayerData.LevelUpgrade.HP);
                }
                else if (i % 3 == 1)
                {
                    pd.upgrades.Add(PlayerData.LevelUpgrade.EP);
                }
                else
                {
                    pd.upgrades.Add(PlayerData.LevelUpgrade.SP);
                }
            }
        }
        pd.UpdateMaxStats();
        pd.FullHeal();

        //give you rand(1,4) random drops from pit boxes
        //if items reach limit, delete ones that don't fit (start adding from the higher floors so you get good stuff more likely)
        for (int i = floor - 1; i > 0; i--)
        {
            int r = RandomGenerator.GetIntRange(1, 3);
            for (int j = 0; j < r; j++)
            {
                (PickupUnion p, int c) = PitObstacleScript.ChooseRandomRewardStatic(i);
                MainManager.Instance.PickupInstant(p);
            }

            if (i == 13 && pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).jumpLevel < 1)
            {
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).jumpLevel = 1;
            }
            if (i == 23 && pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).jumpLevel < 1)
            {
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).jumpLevel = 1;
            }
            if (i == 33 && (pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).weaponLevel < 1 || (pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).weaponLevel < 1)))
            {
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).weaponLevel = 1;
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).weaponLevel = 1;
            }
            if (i == 43 && pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).jumpLevel < 2)
            {
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).jumpLevel = 2;
            }
            if (i == 53 && pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).jumpLevel < 2)
            {
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).jumpLevel = 2;
            }
            if (i == 63 && (pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).weaponLevel < 2 || (pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).weaponLevel < 2)))
            {
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).weaponLevel = 2;
                pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).weaponLevel = 2;
            }
        }

        pd.coins = (floor * 10);
        if (pd.coins > 999)
        {
            pd.coins = 999;
        }
        if (pd.coins < 0)
        {
            pd.coins = 0;
        }


        //fly exit
        mapScript.OnExit(exitID);

        //set it again because lobby sets it to 1
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, (floor).ToString());
        //Debug.Log("Exit " + exitID);

        if (MainManager.Instance.Camera.mode == WorldCamera.CameraMode.FollowPlayer)
        {
            MainManager.Instance.Camera.mode = WorldCamera.CameraMode.FollowPlayerNoVertical;
        }
        WorldPlayer wp = WorldPlayer.Instance;

        bool fadeoutDone = false;
        IEnumerator Fadeout()
        {
            yield return StartCoroutine(MainManager.Instance.FadeToBlack());
            fadeoutDone = true;
        }

        float fly = wp.rb.velocity.y;
        if (fly < 15)
        {
            fly = 15;
        }

        StartCoroutine(Fadeout());
        while (!fadeoutDone)
        {
            wp.rb.velocity = Vector3.up * fly;
            for (int i = 0; i < wp.followers.Count; i++)
            {
                wp.followers[i].rb.velocity = Vector3.up * fly;
            }
            yield return null;
        }

        (Vector3 playerPos, float playerYaw) = (Vector3.zero, 0); //GetRelativeOffset(WorldPlayer.Instance.transform.position, WorldPlayer.Instance.GetTrueFacingRotation());

        string nextMap = MainManager.MapID.RabbitHole_NormalFloor.ToString();
        if (floor % 10 == 0)
        {
           nextMap = MainManager.MapID.RabbitHole_RestFloor.ToString();
        }
        if (floor == 100)
        {
           nextMap = MainManager.MapID.RabbitHole_FinalFloor.ToString();
        }

        MainManager.MapID mid = MainManager.MapID.None;
        Enum.TryParse(nextMap, out mid);
        yield return MainManager.Instance.StartCoroutine(MainManager.Instance.ChangeMap(mid, 0, playerPos, playerYaw));
    }

    public string RequestTextData(string request)
    {
        return "";
    }

    public void SendTextData(string data)
    {

    }

    public void EnableSpeakingAnim()
    {
    }

    public bool SpeakingAnimActive()
    {
        return false;
    }

    public void DisableSpeakingAnim()
    {
    }

    public void SetAnimation(string animationID, bool force = false, float time = -1)
    {
    }

    public void SendAnimationData(string data)
    {
    }

    public Vector3 GetTextTailPosition()
    {
        return transform.position;
    }

    public void TextBleep()
    {
    }

    public void SetFacing(Vector3 facingTarget)
    {
    }

    public void EmoteEffect(TagEntry.Emote emote)
    {
    }
}
