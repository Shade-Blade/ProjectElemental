using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerSettings : Pause_HandlerShared_BoxMenu
{
    int level;
    float sideDir;

    int[] originalValues;
    int[] maxValues;

    public bool awaitReload;
    public bool awaitMain;
    public PromptBoxMenu prompt;

    public bool startMenu;  //removes the bottom 2 options (reload save, exit to main menu)

    public static Pause_HandlerSettings BuildMenu(Pause_SectionShared section = null, bool startMenu = false)
    {
        GameObject newObj = new GameObject("Pause Settings Menu");
        Pause_HandlerSettings newMenu = newObj.AddComponent<Pause_HandlerSettings>();

        newMenu.SetSubsection(section);
        newMenu.originalValues = SettingsManager.Instance.settingsValues;
        newMenu.maxValues = SettingsManager.Instance.settingsMaxValues;
        newMenu.startMenu = startMenu;
        newMenu.Init();

        return newMenu;
    }

    public override int GetObjectCount()
    {
        return SettingsManager.Instance.settingsText.Length - 2 - (startMenu ? 2 : 0);
    }


    public override void MenuUpdate()
    {
        lifetime += Time.deltaTime;
        //improper to make the menu work like this but ehh
        if (prompt != null && !prompt.menuDone)
        {
            //Reset these so that things don't become sus after you select something
            inputDir = 0;
            holdDur = 0;
            holdValue = 0;
            sideDir = 0;
            return;
        }
        if (prompt != null && prompt.menuDone && awaitReload)
        {
            int index = prompt.menuIndex;
            prompt.Clear();
            Destroy(prompt.gameObject);
            prompt = null;
            if (index == 0)
            {
                MainManager.Instance.pauseMenuScript.Unpause();
                MainManager.Instance.LoadSave(MainManager.Instance.saveIndex);
            }
            return;
        }
        if (prompt != null && prompt.menuDone && awaitMain)
        {
            int index = prompt.menuIndex;
            prompt.Clear();
            Destroy(prompt.gameObject);
            prompt = null;
            if (index == 0)
            {
                MainManager.Instance.pauseMenuScript.Unpause();
                MainManager.Instance.ReturnToStartMenu();
            }
            return;
        }

        int oc = GetObjectCount();
        if (oc > 0)
        {
            if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) != -inputDir) || InputManager.GetAxisVertical() == 0)
            {
                holdDur = 0;
                holdValue = 0;
                inputDir = -Mathf.Sign(InputManager.GetAxisVertical());
                if (InputManager.GetAxisVertical() == 0)
                {
                    inputDir = 0;
                }
                //Debug.Log(InputManager.GetAxisHorizontal());
                //now go
                if (inputDir != 0)
                {
                    if (inputDir > 0)
                    {
                        index++;
                    }
                    else
                    {
                        index--;
                    }
                }

                if (index > oc - 1)
                {
                    //index = itemList.Count - 1;
                    index = 0;
                }
                if (index < 0)
                {
                    //index = 0;
                    index = oc - 1;
                }

                level = originalValues[index];

                if (inputDir != 0)
                {
                    //Debug.Log("Apply update? " + (section != null));
                    SendUpdate();
                    //MainManager.ListPrint(itemList, index);
                    //Debug.Log(itemList[index]);
                }
            }

            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.Z))
            {
                if (index == oc - 1)
                {
                    index = 0;
                }
                else
                {
                    index += PAGE_SIZE;

                    if (index > oc - 1)
                    {
                        //index = badgeList.Count - 1;
                        index = oc - 1;
                    }
                }

                level = originalValues[index];
                SendUpdate();
            }

            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.Y))
            {
                if (index == 0)
                {
                    index = oc - 1;
                }
                else
                {
                    index -= PAGE_SIZE;

                    if (index < 0)
                    {
                        //index = 0;
                        index = 0;
                    }
                }

                level = originalValues[index];
                SendUpdate();
            }

            if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) == -inputDir) && InputManager.GetAxisVertical() != 0)
            {
                holdDur += Time.deltaTime;

                if (holdDur >= HYPER_SCROLL_TIME)
                {
                    int pastHoldValue = holdValue;

                    if (MainManager.Instance.GetHyperScrollRate() * (holdDur - HYPER_SCROLL_TIME) > holdValue)
                    {
                        holdValue = (int)(MainManager.Instance.GetHyperScrollRate() * (holdDur - HYPER_SCROLL_TIME));
                    }

                    if (inputDir > 0)
                    {
                        index += (holdValue - pastHoldValue);
                    }
                    else
                    {
                        index -= (holdValue - pastHoldValue);
                    }

                    //No loop around
                    if (index > oc - 1)
                    {
                        index = oc - 1;
                    }
                    if (index < 0)
                    {
                        index = 0;
                    }
                    level = originalValues[index];
                    SendUpdate();
                }
            }

            if (InputManager.GetAxisVertical() == 0)
            {
                if (Mathf.Sign(InputManager.GetAxisHorizontal()) != sideDir || InputManager.GetAxisHorizontal() == 0)
                {
                    sideDir = Mathf.Sign(InputManager.GetAxisHorizontal());
                    if (InputManager.GetAxisHorizontal() == 0)
                    {
                        sideDir = 0;
                    }

                    if (sideDir > 0)
                    {
                        level += 1;
                    }
                    if (sideDir < 0)
                    {
                        level -= 1;
                    }

                    if (level < 0)
                    {
                        level = 0;
                    }
                    if (level > maxValues[index])
                    {
                        level = maxValues[index];
                    }

                    originalValues[index] = level;
                    SettingsManager.Instance.settingsValues[index] = level;
                    SettingsManager.Instance.UpdateSetting((SettingsManager.Setting)index, level);
                    SendUpdate();
                }
            }

            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.A)) //Press A to select stuff
            {
                //go to submenu
                Select();
            }
            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
            {
                PopSelf();
            }
        }
        else
        {
            //No items

            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
            {
                PopSelf();
            }
        }
    }

    public override void Select()
    {
        if (startMenu)
        {
            if (index == GetObjectCount() - 1)  //rebind
            {
                Pause_SectionShared newSection = null;
                if (section != null)
                {
                    newSection = section.GetSubsection(new int[] { index, level });
                    ((Pause_SectionSettings)section).ReenableRebindMenu();
                }
                Debug.Log("Rebind");
                MenuHandler b = null;
                b = Pause_HandlerSettings_RebindingControl.BuildMenu(newSection);

                b.transform.parent = transform;
                PushState(b);
                b.menuExit += InvokeExit;
            }
        } else
        {
            //(only the bottom three are selectible)
            if (index == GetObjectCount() - 3)  //rebind
            {
                Pause_SectionShared newSection = null;
                if (section != null)
                {
                    newSection = section.GetSubsection(new int[] { index, level });
                    ((Pause_SectionSettings)section).ReenableRebindMenu();
                }
                Debug.Log("Rebind");
                MenuHandler b = null;
                b = Pause_HandlerSettings_RebindingControl.BuildMenu(newSection);

                b.transform.parent = transform;
                PushState(b);
                b.menuExit += InvokeExit;
            }

            //reload last save
            if (index == GetObjectCount() - 2)
            {
                Debug.Log("Reload");
                prompt = PromptBoxMenu.BuildMenu(new string[] { "Yes", "No" }, new string[] { "0", "1" }, 1, "Do you want to reload to your last save?");
                awaitReload = true;
            }

            //main menu
            if (index == GetObjectCount() - 1)
            {
                Debug.Log("Main");
                prompt = PromptBoxMenu.BuildMenu(new string[] { "Yes", "No" }, new string[] { "0", "1" }, 1, "Do you want to go to the main menu?");
                awaitMain = true;
            }
        }
    }

    public override void SendUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(new int[] { index, level });
        }
    }

    public override void Init()
    {
        base.Init();

        if (section != null)
        {
            int[] uo = (int[])(section.GetState());
            int newIndex = uo[0];
            index = newIndex;
        }

        if (index > GetObjectCount() - 1)
        {
            index = GetObjectCount() - 1;
        }
        if (index < 0)
        {
            index = 0;
        }

        if (section != null)
        {
            level = originalValues[index];
            section.ApplyUpdate(new int[] { index, level });
        }
    }

    public override void Clear()
    {
        base.Clear();
        SettingsManager.Instance.SaveSettings();
        Debug.Log("save settings");
    }
}
