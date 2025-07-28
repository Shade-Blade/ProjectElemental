using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;

public class Pause_HandlerMap : Pause_HandlerShared
{
    public class UpdateObject
    {
        public Vector3 position;
        public MainManager.WorldLocation location;

        public UpdateObject(Vector3 p_position, MainManager.WorldLocation p_location)
        {
            position = p_position;
            location = p_location;
        }
    }

    Vector3 pointerPos;
    MainManager.WorldLocation worldLocation;

    public List<MapDotScript> dots;
    public MapDotScript selectedDot;

    public float holdTime;

    public bool awaitWarp;
    public PromptBoxMenu prompt;

    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;




    public static Pause_HandlerMap BuildMenu(Pause_SectionShared section = null)
    {
        GameObject newObj = new GameObject("Pause Map Menu");
        Pause_HandlerMap newMenu = newObj.AddComponent<Pause_HandlerMap>();

        newMenu.SetSubsection(section);
        newMenu.Init();

        return newMenu;
    }

    public override void Init()
    {
        if (section != null)
        {
            UpdateObject uo = (UpdateObject)section.GetState();
            pointerPos = uo.position;
            worldLocation = uo.location;
            dots = ((Pause_SectionMap)section).GetDots();
            selectedDot = dots.Find((e) => (uo.location == e.GetWorldLocation()));
        }

        base.Init();
    }

    public void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    public bool FastTravelUnlocked()
    {
        return MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_AirShell);
    }

    public virtual void MenuUpdate()
    {
        lifetime += Time.deltaTime;
        if (prompt != null && !prompt.menuDone)
        {
            //Reset these so that things don't become sus after you select something
            holdTime = 0;
            return;
        }
        if (prompt != null && prompt.menuDone && awaitWarp)
        {
            int index = prompt.menuIndex;
            prompt.Clear();
            Destroy(prompt.gameObject);
            prompt = null;
            if (index == 0)
            {
                MainManager.Instance.pauseMenuScript.Unpause();
                MainManager.Instance.StartCoroutine(MainManager.Instance.FastTravelWarp(worldLocation));
            }
            awaitWarp = false;
            return;
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Close);
            PopSelf();
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.A) && FastTravelUnlocked() && selectedDot != null)
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
            prompt = PromptBoxMenu.BuildMenu(new string[] { "Yes", "No" }, new string[] { "0", "1" }, 1, "Do you want to warp to " + MainManager.GetAreaName(selectedDot.worldLocation) +"?");
            awaitWarp = true;
        }

        Vector3 inputDir = Vector2.up * (InputManager.GetAxisVertical()) + Vector2.right * (InputManager.GetAxisHorizontal());

        float speed = 180 + (180 * holdTime);

        if (inputDir != Vector3.zero)
        {
            holdTime += Time.deltaTime;
        } else
        {
            holdTime = 0;
        }
        if (holdTime > 1)
        {
            holdTime = 1;
        }

        float attractSpeed = speed * 0.5f;

        //Move
        if (selectedDot != null && Vector3.Dot(inputDir, selectedDot.transform.localPosition - pointerPos) >= 0)
        {
            //Attract first
            Vector3 delta = (selectedDot.transform.localPosition - pointerPos).normalized * Time.deltaTime * attractSpeed;

            if (delta.magnitude > (selectedDot.transform.localPosition - pointerPos).magnitude)
            {
                pointerPos = selectedDot.transform.localPosition;
            }
            else
            {
                pointerPos += delta;
            }
        }

        pointerPos += inputDir * Time.deltaTime * speed;

        if (pointerPos.x > 390)
        {
            pointerPos.x = 390;
        }
        if (pointerPos.x < -390)
        {
            pointerPos.x = -390;
        }
        if (pointerPos.y > 390)
        {
            pointerPos.y = 390;
        }
        if (pointerPos.y < -390)
        {
            pointerPos.y = -390;
        }

        //selected dot stuff
        if (selectedDot != null && (pointerPos - selectedDot.transform.localPosition).magnitude > 12.5f) 
        {
            selectedDot = null;
            worldLocation = MainManager.WorldLocation.Nowhere;
        } else
        {
            //Seek out other dots?
            for (int i = 0; i < dots.Count; i++)
            {
                if ((pointerPos - dots[i].transform.localPosition).magnitude < 12.5f)
                {
                    selectedDot = dots[i];
                    worldLocation = dots[i].GetWorldLocation();
                }
            }
        }

        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(pointerPos, worldLocation));
        }
    }
}
