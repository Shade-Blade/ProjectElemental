using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static EnemyMove;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SettingsManager>(); //this should work
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    private static SettingsManager instance;

    public enum Setting
    {
        None = -1,
        MasterVolume,
        MusicVolume,
        SFXVolume,
        TextVolume,
        SystemVolume,
        FPS,
        RebindControls,
        ReloadSave,
        MainMenu
    }

    public string[][] settingsText;
    public int[] settingsValues;
    public int[] settingsMaxValues;
    public int[] settingsDefaultValues;

    public void LoadSettingsText()
    {
        settingsText = MainManager.GetAllTextFromFile("DialogueText/SettingsText");
        settingsMaxValues = new int[settingsText.Length - 1];
        settingsDefaultValues = new int[settingsText.Length - 1];

        for (int i = 1; i < settingsText.Length - 1; i++)
        {
            int.TryParse(settingsText[i][3], out settingsMaxValues[i - 1]);
            int.TryParse(settingsText[i][4], out settingsDefaultValues[i - 1]);
        }
    }

    public void UpdateSetting(Setting s, int index)
    {
        settingsValues[(int)s] = index;

        switch (s)
        {
            case Setting.FPS:
                //target frame rate is ignored if nonzero?
                QualitySettings.vSyncCount = 0;
                switch (index)
                {
                    case 0:
                        Application.targetFrameRate = 30;
                        break;
                    case 1:
                        Application.targetFrameRate = 60;
                        break;
                    case 2:
                        Application.targetFrameRate = 90;
                        break;
                    case 3:
                        Application.targetFrameRate = 120;
                        break;
                }
                break;
        }
    }

    public string GetSettingString()
    {
        string output = MainManager.ListToString(settingsValues);
        string outputB = InputManager.GetKeyCodeString();
        string outputC = InputManager.GetDirectionalKeyCodeString();
        return output + "\n" + outputB + "\n" + outputC;
    }

    public bool SaveSettings()
    {
        string settingString = GetSettingString();
        try
        {
            TextWriter tw = File.CreateText("settingst.txt");
            tw.Write(settingString);
            tw.Close();
            if (File.Exists("settings.txt"))
            {
                File.Delete("settings.txt");
            }
            File.Move("settingst.txt", "settings.txt");
            File.Delete("settingst.txt");

            for (int i = 0; i < settingsValues.Length; i++)
            {
                UpdateSetting((Setting)i, settingsValues[i]);
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Settings Saving] Exception: " + e);
            return false;
        }
    }

    public bool LoadSettings()
    {
        try
        {
            if (!File.Exists("settings.txt"))
            {
                //set to defaults
                if (settingsDefaultValues == null || settingsDefaultValues.Length == 0)
                {
                    LoadSettingsText();
                }
                settingsValues = new int[settingsDefaultValues.Length];
                for (int i = 0; i < settingsValues.Length; i++)
                {
                    settingsValues[i] = settingsDefaultValues[i];
                }
                //make a save file secretly
                return SaveSettings();
            }
            string data = File.ReadAllText("settings.txt");
            LoadSettingsWithString(data);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[Loading Settings] Exception: " + e);
            return false;
        }
    }

    public void LoadSettingsWithString(string data)
    {
        string[] split = data.Split("\n");
        List<int> values = MainManager.ParseIntList(split[0]);

        if (settingsDefaultValues == null || settingsDefaultValues.Length == 0)
        {
            LoadSettingsText();
        }

        settingsValues = new int[settingsDefaultValues.Length];
        if (values.Count != settingsDefaultValues.Length)
        {
            Debug.LogError("Save was not parsed right: " + values.Count + " vs " + settingsValues.Length);
        }
        for (int i = 0; i < settingsValues.Length; i++)
        {
            if (i > values.Count - 1)
            {
                settingsValues[i] = settingsDefaultValues[i];
            } else
            {
                settingsValues[i] = values[i];
            }
            UpdateSetting((Setting)i, settingsValues[i]);
        }

        InputManager.SetControlsWithKeyCodeStrings(split[1], split[2]);        
    }

    public void UpdateSettings()
    {

    }
}
