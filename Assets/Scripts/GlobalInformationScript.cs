using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//area text and pause menu text piggybacks off this script because I don't feel like making more scripts
public class GlobalInformationScript : MonoBehaviour
{
    private static GlobalInformationScript instance;
    public static GlobalInformationScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GlobalInformationScript>(); //this should work
                if (instance == null)
                {
                    GameObject b = new GameObject("GlobalInformationScript");
                    GlobalInformationScript c = b.AddComponent<GlobalInformationScript>();
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

    public string[][] infoText;
    public string[][] loreText;
    public string[][] areaText;
    public string[][] statusText;

    public void LoadInfoText()
    {
        infoText = MainManager.GetAllTextFromFile("DialogueText/InformationText");
    }
    public void LoadLoreText()
    {
        loreText = MainManager.GetAllTextFromFile("DialogueText/LoreText");
    }

    public void LoadAreaText()
    {
        areaText = MainManager.GetAllTextFromFile("DialogueText/AreaText");

        //integrity verification
        MainManager.WorldLocation wl = MainManager.WorldLocation.Nowhere;
        for (int i = 1; i < (int)(MainManager.WorldLocation.EndOfTable); i++)
        {
            Enum.TryParse(areaText[i][0], out wl);
            if (wl != (MainManager.WorldLocation)(i))
            {
                Debug.LogWarning("Parse error for world location: expected = " + (MainManager.WorldLocation)(i) + " vs actual = " + areaText[i][0]);
            }
        }
    }
    public void LoadStatusText()
    {
        statusText = MainManager.GetAllTextFromFile("DialogueText/StatusText");

        //integrity verification]
        Pause_SectionStatus.MenuNodeType mnt;
        for (int i = 1; i < (int)(Pause_SectionStatus.MenuNodeType.EndOfTable); i++)
        {
            Enum.TryParse(statusText[i][0], out mnt);
            if (mnt != (Pause_SectionStatus.MenuNodeType)(i))
            {
                Debug.LogWarning("Parse error for status menu element: expected = " + (Pause_SectionStatus.MenuNodeType)(i) + " vs actual = " + statusText[i][0]);
            }
        }
    }


    //likely will be unnecessary
    //text is not very memory intensive?
    //would likely need the garbage collector to do stuff though (this likely won't help that much?)
    public void UnloadInfo()
    {
        UnloadInfoText();
        UnloadLoreText();
    }
    public void UnloadInfoText()
    {
        infoText = null;
    }
    public void UnloadLoreText()
    {
        loreText = null;
    }
}
