using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificFloorLauncherScript : WorldObject, ITextSpeaker, ISignalReceiver
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
        yield return null;

        StartCoroutine(MainManager.Instance.ExecuteCutscene(FlyCutscene(101)));
    }

    public IEnumerator FlyCutscene(int floor)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerShockwave2"), MainManager.Instance.mapScript.transform);
        effect.transform.position = transform.position;

        PlayerData pd = MainManager.Instance.playerData;

        //go to floor
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, (floor).ToString());
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitAltar, (floor / 10).ToString());


        Debug.Log(MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor));


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

    public void SetAnimation(string animationID, bool force = false)
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
