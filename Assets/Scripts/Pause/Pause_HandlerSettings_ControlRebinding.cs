using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerSettings_RebindingControl : Pause_HandlerShared_BoxMenu
{
    public float waitTime;
    public const float CONTROL_WAIT_TIME = 5;

    public KeyCode lastKeyCode;

    public static Pause_HandlerSettings_RebindingControl BuildMenu(Pause_SectionShared section = null)
    {
        GameObject newObj = new GameObject("Pause Settings Rebind Controls Menu");
        Pause_HandlerSettings_RebindingControl newMenu = newObj.AddComponent<Pause_HandlerSettings_RebindingControl>();

        newMenu.SetSubsection(section);
        newMenu.Init();


        return newMenu;
    }

    public override int GetObjectCount()
    {
        if (InputManager.Instance.inputText == null)
        {
            InputManager.Instance.LoadInputText();
        }
        return InputManager.Instance.inputText.Length - 2;
    }

    public void OnGUI()
    {
        //note: second condition prevents this from being run the same frame you select something
        if (waitTime == 0 || waitTime == CONTROL_WAIT_TIME)
        {
            return;
        }

        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.isKey)
        {
            if (e.character == '~')
            {
                //No
                return;
            }

            //check that it is not used
            for (int i = 0; i < InputManager.Instance.keyCodes.Length; i++)
            {
                if (e.keyCode == InputManager.Instance.keyCodes[i])
                {
                    return;
                }
            }
            for (int i = 0; i < InputManager.Instance.directionalKeyCodes.Length; i++)
            {
                if (e.keyCode == InputManager.Instance.directionalKeyCodes[i])
                {
                    return;
                }
            }

            lastKeyCode = e.keyCode;
            //Debug.Log("Correctly see " + lastKeyCode);
            waitTime = 0;
            return;
        }
    }


    public override void MenuUpdate()
    {
        lifetime += Time.deltaTime;
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            inputDir = 0;
            holdDur = 0;
            holdValue = 0;
            if (waitTime < 0)
            {
                lastKeyCode = 0;
                SendUpdate();
            }
            return;
        } else
        {
            waitTime = 0;
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
        //Can always select
        //What to do then?

        if (index == GetObjectCount() - 1)
        {
            //Reset
            InputManager.Instance.ResetToDefaultKeyCodes();
            section.ApplyUpdate(null);
        }
        else
        {
            section.ApplyUpdate(index);

            IEnumerator SetKeyCode(int index)
            {
                while (waitTime > 0)
                {
                    yield return null;
                }

                //Debug.Log("Select keycode with " + lastKeyCode);

                if (lastKeyCode != 0)
                {
                    InputManager.Instance.keyCodes[index] = lastKeyCode;
                    section.ApplyUpdate(null);
                }
            }

            IEnumerator SetDirectionKeyCode(int index)
            {
                while (waitTime > 0)
                {
                    yield return null;
                }

                //Debug.Log("Select directional with " + lastKeyCode);

                if (lastKeyCode != 0)
                {
                    InputManager.Instance.directionalKeyCodes[index] = lastKeyCode;
                    section.ApplyUpdate(null);
                }
            }

            waitTime = CONTROL_WAIT_TIME;
            if (index <= 3)
            {
                StartCoroutine(SetDirectionKeyCode(index));
            } else
            {
                StartCoroutine(SetKeyCode(index - 4));
            }
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
            section.ApplyUpdate(index);
        }
    }
}
