using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PitAltarScript : WorldObject, IInteractable, ITextSpeaker
{
    public GameObject subobject;
    public InteractTrigger it;

    public GameObject block;


    public override void Awake()
    {
        base.Awake();
        it.interactable = this;

        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        }
        int floor = int.Parse(floorNo);

        string pitaltar = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitAltar);
        if (pitaltar == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitAltar, 0.ToString());
            pitaltar = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitAltar);
        }
        int altar = int.Parse(pitaltar);

        //You already got the altar for this floor
        if (altar >= (floor / 10))
        {
            Destroy(block);
            Destroy(gameObject);
        }
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

    public void Interact()
    {
        StartCoroutine(InteractCutscene());
    }

    public IEnumerator InteractCutscene()
    {
        string[][] testTextFile = new string[18][];
        testTextFile[0] = new string[1];

        string pitaltar = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitAltar);
        if (pitaltar == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitAltar, 0.ToString());
            pitaltar = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitAltar);
        }
        int altar = int.Parse(pitaltar);

        testTextFile[0][0] = "";

        switch (altar)
        {
            case 0:
                testTextFile[0][0] = "<system>You can now use the Soul Moves: Hasten and Revitalize! Hasten lets you get a bonus action next turn, and Revitalize can restore your HP and EP.";
                break;
            case 1:
                testTextFile[0][0] = "<system>You can now use the Soul Move: Leaf Storm! Leaf Storm lets you hit all enemies with Earth damage.";
                break;
            case 2:
                testTextFile[0][0] = "<system>You can now use the Soul Move: Electro Discharge! Electro Discharge lets you reduce the Attack of all enemies for a short time.";
                break;
            case 3:
                testTextFile[0][0] = "<system>You can now use the Soul Move: Mist Wave! Mist Wave lets you hit all enemies with Water damage.";
                break;
            case 4:
                testTextFile[0][0] = "<system>You can now use the Soul Move: Overheat! Overheat lets you reduce the Defense of all enemies for a short time.";
                break;
            case 5:
                testTextFile[0][0] = "<system>You can now use the Soul Move: Void Crush! Void Crush lets you instantly defeat low level enemies (or enemies with low health or affected by ailments.)";
                break;
            case 6:
                testTextFile[0][0] = "<system>You can now use the Soul Move: Flash Freeze! Flash Freeze lets you try to Freeze all enemies. (This can hit Ethereal targets.)";
                break;
            case 7:
                testTextFile[0][0] = "<system>You can now use the Soul Moves: Cleanse and Blight! Cleanse lets you remove all buffs from enemies, and Blight lets you inflict two special debuffs that let you heal HP and EP by attacking enemies.";
                break;
            case 8:
                testTextFile[0][0] = "<system>You can now use the Soul Moves: Elemental Conflux and Prismatic Blast. Elemental Conflux gives you a wide variety of powerful buffs, and Prismatic Blast does strong Prismatic damage to all enemies.";
                break;
        }

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));


        altar++;

        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitAltar, altar.ToString());

        Destroy(block);
        Destroy(gameObject);
    }
}
