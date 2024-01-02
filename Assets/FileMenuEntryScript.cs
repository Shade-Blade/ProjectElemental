using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileMenuEntryScript : MonoBehaviour
{
    public Image background;
    public TextDisplayer maintext;
    public TextDisplayer specialtext;
    public TextDisplayer playtime;
    public TextDisplayer level;
    public TextDisplayer divider;
    public TextDisplayer progress;
    public TextDisplayer dividerB;
    public TextDisplayer lastsavelocation;

    public TextDisplayer newgame;

    public FileMenuEntry entry;

    public void Setup(FileMenuEntry p_entry)
    {
        entry = p_entry;

        if (entry == null)
        {
            //Error
            maintext.gameObject.SetActive(false);
            specialtext.gameObject.SetActive(false);
            playtime.gameObject.SetActive(false);
            level.gameObject.SetActive(false);
            divider.gameObject.SetActive(false);
            dividerB.gameObject.SetActive(false);
            progress.gameObject.SetActive(false);
            lastsavelocation.gameObject.SetActive(false);
            newgame.gameObject.SetActive(true);

            newgame.SetText("<color,red>File reading error.</color>", true, true);
            return;
        }

        if (entry.name == null || entry.name.Length == 0)
        {
            maintext.gameObject.SetActive(false);
            specialtext.gameObject.SetActive(false);
            playtime.gameObject.SetActive(false);
            level.gameObject.SetActive(false);
            divider.gameObject.SetActive(false);
            dividerB.gameObject.SetActive(false);
            progress.gameObject.SetActive(false);
            lastsavelocation.gameObject.SetActive(false);
            newgame.gameObject.SetActive(true);
        }
        else
        {
            maintext.gameObject.SetActive(true);
            specialtext.gameObject.SetActive(true);
            playtime.gameObject.SetActive(true);
            level.gameObject.SetActive(true);
            divider.gameObject.SetActive(true);
            dividerB.gameObject.SetActive(true);
            progress.gameObject.SetActive(true);
            lastsavelocation.gameObject.SetActive(true);
            newgame.gameObject.SetActive(false);

            maintext.SetText(entry.name, true, true);
            specialtext.SetText(entry.specialSprites, true, true);
            playtime.SetText(entry.playtime, true, true);
            progress.SetText(entry.progressSprites, true, true);
            level.SetText(entry.levelText, true, true);
            lastsavelocation.SetText(entry.worldLocation + " - " + entry.worldMap, true, true);
        }
    }
}
