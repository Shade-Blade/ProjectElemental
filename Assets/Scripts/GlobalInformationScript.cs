using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void LoadInfoText()
    {
        infoText = MainManager.GetAllTextFromFile("DialogueText/InformationText");
    }
    public void LoadLoreText()
    {
        loreText = MainManager.GetAllTextFromFile("DialogueText/LoreText");
    }
}
