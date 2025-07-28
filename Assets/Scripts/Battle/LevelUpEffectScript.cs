using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpEffectScript : MonoBehaviour
{
    public TextDisplayer levelUpText;
    public TextDisplayer levelLeftText;
    public Image arrow;
    public TextDisplayer levelRightText;
    public GameObject levelMaxText;

    public int state;
    float stateWait;

    int levelLeft;
    int levelRight;
    bool maxOn;


    public void Setup(int levelA, int levelB, bool max)
    {
        state = 0;
        stateWait = 0;

        levelLeft = levelA;
        levelRight = levelB;
        maxOn = max;

        levelUpText.SetText("<scroll,0.6><fadeinwave,0.4,30,0,0,0><rainbow>Level up!</rainbow></fadeinwave>", false);
        levelLeftText.gameObject.SetActive(false);
        //levelLeftText.SetText("", true);
        arrow.gameObject.SetActive(false);
        //levelRightText.SetText("", true);
        levelRightText.gameObject.SetActive(false);
        levelMaxText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case 0: //Level Up falls in
                if (levelUpText.scrollDone)
                {
                    stateWait += Time.deltaTime;
                    if (stateWait > 0.5f)
                    {
                        state = 1;
                        stateWait = 0;
                        levelLeftText.gameObject.SetActive(true);
                        levelLeftText.SetText("<scroll,0.6><fadeingrow,0.5><outline,#a0ffa0>" + levelLeft + "</outline><fadeingrow>", false);
                    }
                }
                break;
            case 1: //left side
                stateWait += Time.deltaTime;
                if (levelLeftText.scrollDone && stateWait > 0.25f)
                {
                    state = 2;
                    stateWait = 0;
                    arrow.gameObject.SetActive(true);
                    arrow.color = new Color(1, 1, 1, 0);
                }
                break;
            case 2: //arrow
                stateWait += Time.deltaTime;
                float opacity = stateWait;
                opacity *= (1 / 0.3f);
                if (opacity > 1)
                {
                    opacity = 1;
                }
                if (opacity < 0)
                {
                    opacity = 0;
                }
                arrow.color = new Color(1, 1, 1, opacity);
                if (stateWait > 0.3f)
                {
                    state = 3;
                    stateWait = 0;
                    levelRightText.gameObject.SetActive(true);
                    if (maxOn)
                    {
                        levelRightText.SetText("<scroll,0.6><fadeinshrink,0.5><outline,#ffffa0>" + levelRight + "</outline></fadeinshrink>", false);
                    }
                    else
                    {
                        levelRightText.SetText("<scroll,0.6><fadeinshrink,0.5><outline,#a0ffa0>" + levelRight + "</outline></fadeinshrink>", false);
                    }
                }
                break;
            case 3: //right side
                arrow.color = Color.white;
                stateWait += Time.deltaTime;
                if (levelRightText.scrollDone && stateWait > 0.25f)
                {
                    state = 4;
                    stateWait = 0;
                    if (maxOn)
                    {
                        levelMaxText.gameObject.SetActive(true);
                    }
                }
                break;
            case 4: //awaiting A press
                if ((InputManager.GetButtonDown(InputManager.Button.Start) || InputManager.GetButtonDown(InputManager.Button.A)))
                {
                    state = 5;
                    stateWait = 0;
                }
                break;
            case 5: //flying up
                stateWait += Time.deltaTime;
                gameObject.transform.localPosition = Vector3.up * stateWait * 2500;

                if (stateWait > 0.25f)
                {
                    Destroy(gameObject);
                }

                break;
        }
    }
}
