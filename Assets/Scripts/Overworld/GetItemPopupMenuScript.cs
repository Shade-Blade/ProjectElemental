using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//note: also used for getting badges, ribbons, etc
public class GetItemPopupMenuScript : MenuHandler
{
    public BattlePopupScript bps;
    public DescriptionBoxScript description;

    PickupUnion pu;

    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;

    public string popupString;
    public string descriptionString;

    public bool exit;

    const float MIN_LIFETIME = 0.25f;
    public float time;

    public GameObject itemSprite;


    public static GetItemPopupMenuScript BuildMenu(PickupUnion pu)//, bool normalPopup = false)
    {
        //GameObject newObj = new GameObject("Get Item Popup");
        GameObject newObj = Instantiate(MainManager.Instance.getItemPopup, MainManager.Instance.Canvas.transform);
        GetItemPopupMenuScript newMenu = newObj.GetComponent<GetItemPopupMenuScript>();

        newMenu.Setup(pu);
        newMenu.exit = false;
        newMenu.Init();

        return newMenu;
    }


    public void Setup(PickupUnion pu)
    {
        this.pu = pu.Copy();
        string article = PickupUnion.GetArticle(pu);
        if (article.Equals(""))
        {
            popupString = "You got " + PickupUnion.GetSpriteString(pu) + " " + PickupUnion.GetName(pu);
        }
        else
        {
            popupString = "You got " + article + " " + PickupUnion.GetSpriteString(pu) + " " + PickupUnion.GetName(pu);
        }
        descriptionString = PickupUnion.GetDescription(pu);

        bps.SetText(popupString);
        description.SetText(descriptionString);
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    void MenuUpdate()
    {
        time += Time.deltaTime;

        if (time > MIN_LIFETIME && ((InputManager.GetButtonDown(InputManager.Button.A) || InputManager.GetButtonDown(InputManager.Button.B)))) //Press B to go back
        {
            exit = true;

            InvokeExit(this, new MenuExitEventArgs(GetResult()));
            PopSelf();
        }
    }

    public override void Init()
    {
        active = true;
        exit = false;

        //spawn a sprite
        Sprite isp = PickupUnion.GetSprite(pu);

        if (WorldPlayer.Instance != null)
        {
            WorldPlayer.Instance.scriptedAnimation = true;
            WorldPlayer.Instance.SendAnimationData("unshowback");
            WorldPlayer.Instance.SetAnimation("itemuse");
            Vector3 position = WorldPlayer.Instance.transform.position + Vector3.up * 0.8f;

            GameObject so = new GameObject("Get Item Sprite");
            so.transform.parent = MainManager.Instance.mapScript.transform; //not the optimal place but ehh
            SpriteRenderer s = so.AddComponent<SpriteRenderer>();
            s.sprite = isp;
            s.material = PickupUnion.GetSpriteMaterial(pu);
            so.transform.position = position;
            itemSprite = so;
        }
    }
    public override void Clear()
    {
        active = false;
        Destroy(bps.gameObject);
        Destroy(description.gameObject);
        WorldPlayer.Instance.scriptedAnimation = false;
        if (itemSprite != null)
        {
            Destroy(itemSprite);
        }
    }
    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }
    public override MenuResult GetResult()
    {
        return new MenuResult(null);
    }
}
