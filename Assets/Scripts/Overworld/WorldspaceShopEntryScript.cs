using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldspaceShopEntryScript : MonoBehaviour
{
    public Transform evalPoint;
    public TextDisplayer textDisplayer;
    public TextDisplayer nameDisplayer;


    public static WorldspaceShopEntryScript BuildMenu(Transform evalPoint, string text, string nameText)//, bool normalPopup = false)
    {
        //GameObject newObj = new GameObject("Get Item Popup");
        GameObject newObj = Instantiate(MainManager.Instance.worldspaceShopEntry, MainManager.Instance.Canvas.transform);
        WorldspaceShopEntryScript newMenu = newObj.GetComponent<WorldspaceShopEntryScript>();

        newMenu.evalPoint = evalPoint;
        newMenu.textDisplayer.SetText(text, true, true);
        newMenu.nameDisplayer.SetText(nameText, true, true);
        newMenu.nameDisplayer.gameObject.SetActive(false);

        return newMenu;
    }

    public void ShowName()
    {
        nameDisplayer.gameObject.SetActive(true);
    }

    public void HideName()
    {
        nameDisplayer.gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        //How is this happening?
        if (evalPoint == null)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = MainManager.Instance.WorldPosToCanvasPosB(evalPoint.position);
    }
}
