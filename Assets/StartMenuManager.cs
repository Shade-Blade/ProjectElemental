using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    public GameObject startMenu;

    public static StartMenuManager BuildMenu()
    {
        GameObject newObj = Instantiate((GameObject)Resources.Load("Menu/StartMenu/StartMenuManager"), MainManager.Instance.transform);
        StartMenuManager newMenu = newObj.GetComponent<StartMenuManager>();
        newMenu.Init();
        return newMenu;
    }

    public void Init()
    {
        MainManager.Instance.worldMode = MainManager.WorldMode.Start;
        if (startMenu == null)
        {
            startMenu = StartMenu_Base.BuildMenu().gameObject;
        }
        MainManager.Instance.Camera.SetManual(new Vector3(0, 2, -6.5f), new Vector3(0, 0, 0), 0.05f);
        MainManager.Instance.SetSkybox(MainManager.SkyboxID.SolarGrove);
        MainManager.Instance.PlayMusic(MainManager.Sound.Music_Title);
    }

    public void Clear()
    {
        if (startMenu != null)
        {
            Destroy(startMenu);
        }
        Destroy(gameObject);
    }
}
