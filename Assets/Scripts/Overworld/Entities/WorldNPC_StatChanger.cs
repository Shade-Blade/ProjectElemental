using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNPC_StatChanger : WorldNPCEntity
{
    int[] raiseMaxLevels;
    int[] lowerMaxLevels;
    int[] currentLevels;

    public int costPerChange = 0;

    public override IEnumerator InteractCutscene()
    {

        string[][] testTextFile = new string[16][];
        testTextFile[0] = new string[1];
        testTextFile[1] = new string[1];
        testTextFile[2] = new string[1];
        testTextFile[3] = new string[1];
        testTextFile[4] = new string[1];
        testTextFile[5] = new string[1];
        testTextFile[6] = new string[1];
        testTextFile[7] = new string[1];
        testTextFile[8] = new string[1];
        testTextFile[9] = new string[1];
        testTextFile[10] = new string[1];
        testTextFile[11] = new string[1];
        testTextFile[12] = new string[1];
        testTextFile[13] = new string[1];
        testTextFile[14] = new string[1];
        testTextFile[15] = new string[1];

        testTextFile[0][0] = "So you two kids are adventurers now. I hope you're looking after eachother. Things back home are pretty quiet, or at least about as quiet as it can be.<next>I can change your stats to whatever you want, but if you want one of your stats raised, I'll have to lower one of your other stats. Lowering one of your stats will give you Astral Tokens.<next>It might be a good idea to hold onto one of those Astral Tokens, you might find something valuable that needs one of them.<prompt,Increase stats (Y),1,Decrease stats (Y),2,Cancel,3,2>";
        testTextFile[1][0] = "So you two kids are adventurers now. I hope you're looking after eachother. Things back home are pretty quiet, or at least about as quiet as it can be.<next>I can change your stats to whatever you want, but if you want one of your stats raised, I'll have to lower one of your other stats. Lowering one of your stats will give you Astral Tokens.<next>It might be a good idea to hold onto one of those Astral Tokens, you might find something valuable that needs one of them.<prompt,Increase stats (Y),1,Decrease stats (X),2,Cancel,3,2>";
        testTextFile[2][0] = "So you two kids are adventurers now. I hope you're looking after eachother. Things back home are pretty quiet, or at least about as quiet as it can be.<next>I can change your stats to whatever you want, but if you want one of your stats raised, I'll have to lower one of your other stats. Lowering one of your stats will give you Astral Tokens.<next>It might be a good idea to hold onto one of those Astral Tokens, you might find something valuable that needs one of them.<prompt,Increase stats (X),1,Decrease stats (Y),2,Cancel,3,2>";
        testTextFile[3][0] = "So you two kids are adventurers now. I hope you're looking after eachother. Things back home are pretty quiet, or at least about as quiet as it can be.<next>I can change your stats to whatever you want, but if you want one of your stats raised, I'll have to lower one of your other stats. Lowering one of your stats will give you Astral Tokens.<next>It might be a good idea to hold onto one of those Astral Tokens, you might find something valuable that needs one of them.<prompt,Increase stats (X),1,Decrease stats (X),2,Cancel,3,2>";

        testTextFile[4][0] = "I'm so sorry, but I can't increase your stats unless you decrease one of them first.";
        testTextFile[5][0] = "You two look pretty lightheaded... I shouldn't try to lower your stats any more. Why don't you try raising them?";
        testTextFile[6][0] = "<color,red>Impossible state (can't increase or decrease stats)</color>"; 

        testTextFile[7][0] = "Right now your maximum stats are <var,0> Max HP, <var,1> Max EP, <var,2> Max SP, and you have <var,3> Astral Tokens. What stat do you want me to increase for you?<dataget,arg,raise><genericmenu,arg,5,false,false,false,true>";
        testTextFile[8][0] = "So you want me to increase your <var,0> from <var,1> to <var,2>?<prompt,Yes,1,No,2,1>";
        testTextFile[9][0] = "Perfect! Now you should feel happier and healthier!";

        testTextFile[10][0] = "Right now your maximum stats are <var,0> Max HP, <var,1> Max EP, <var,2> Max SP, and you have <var,3> Astral Tokens. What stat do you want me to decrease for you?<dataget,arg,lower><genericmenu,arg,5,false,false,false,true>";
        testTextFile[11][0] = "So you want me to decrease your <var,0> from <var,1> to <var,2>?<prompt,Yes,1,No,2,1>";
        testTextFile[12][0] = "Don't get too carried away with that, you might bite off more than you can chew!";
        testTextFile[13][0] = "Don't get too carried away with that, you might bite off more than you can chew! Also, I think you'll have to put your badges back on.";

        testTextFile[14][0] = "Well, it was nice seeing you again.";
        testTextFile[15][0] = "(error - this should cost 0 coins so you can't have less than that)";

        int state = 0;

        SetupMaximums();

        PlayerData pd = MainManager.Instance.playerData;

        bool increase = false;
        bool decrease = false;
        for (int i = 0; i < raiseMaxLevels.Length; i++)
        {
            increase |= raiseMaxLevels[i] != 0;
        }
        for (int i = 0; i < lowerMaxLevels.Length; i++)
        {
            decrease |= lowerMaxLevels[i] != 0;
        }

        if (increase)
        {
            if (decrease)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 1, this));
            }
        } else
        {
            if (decrease)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 2, this));
            }
            else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 3, this));
            }
        }

        string menuResult = MainManager.Instance.lastTextboxMenuResult;
        int.TryParse(menuResult, out state);

        if (state == 3)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 14, this));
            yield break;
        }

        if ((state == 1 || state == 2) && (!increase && !decrease))
        {
            if (pd.coins < costPerChange)
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 15, this));
                yield break;
            } else
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 6, this));
                yield break;
            }
        }

        if (state == 1 && !increase)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 4, this));
            yield break;
        }

        if (state == 2 && !decrease)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 5, this));
            yield break;
        }


        while (state == 1)
        {
            string[] tempVars = new string[] { GetCurrentHPString(), GetCurrentEPString(), GetCurrentSPString(), GetCurrentAstralString() };

            //menu
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 7, this, tempVars));

            int index = -1;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg2"), out index);

            if (FormattedString.ParseArg(menuResult, "arg0").Equals("cancel"))
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 14, this));
                yield break;
            }
            else
            {
                //not in the cancel state so there is a max level value
                int level = 0;
                int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out level);

                string[] tempVarsB = null;
                if (index == 0)
                {
                    tempVarsB = new string[] { "HP", GetCurrentHPString(), GetCurrentHPString(level) };
                }
                if (index == 1)
                {
                    tempVarsB = new string[] { "EP", GetCurrentEPString(), GetCurrentEPString(level) };
                }
                if (index == 2)
                {
                    tempVarsB = new string[] { "SP", GetCurrentSPString(), GetCurrentSPString(level) };
                }
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 8, this, tempVarsB));

                int ynstate = 0;
                menuResult = MainManager.Instance.lastTextboxMenuResult;
                int.TryParse(menuResult, out ynstate);

                if (ynstate == 1)
                {
                    //chose to raise a stat

                    int downgradesRemoved = 0;
                    for (int i = 0; i < pd.downgrades.Count; i++)
                    {
                        bool remove = false;

                        if (index == 0 && pd.downgrades[i] == PlayerData.LevelUpgrade.HP)
                        {
                            remove = true;
                        }
                        if (index == 1 && pd.downgrades[i] == PlayerData.LevelUpgrade.EP)
                        {
                            remove = true;
                        }
                        if (index == 2 && pd.downgrades[i] == PlayerData.LevelUpgrade.SP)
                        {
                            remove = true;
                        }

                        if (remove)
                        {
                            downgradesRemoved++;
                            pd.downgrades.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    int upgradesToAdd = level - downgradesRemoved;

                    if (upgradesToAdd < 0)
                    {
                        //??? impossible state

                        //delete upgrades if possible
                        for (int i = 0; i < pd.upgrades.Count; i++)
                        {
                            bool remove = false;

                            if (index == 0 && pd.upgrades[i] == PlayerData.LevelUpgrade.HP)
                            {
                                remove = true;
                            }
                            if (index == 1 && pd.upgrades[i] == PlayerData.LevelUpgrade.EP)
                            {
                                remove = true;
                            }
                            if (index == 2 && pd.upgrades[i] == PlayerData.LevelUpgrade.SP)
                            {
                                remove = true;
                            }

                            if (upgradesToAdd == 0)
                            {
                                break;
                            }

                            if (remove && upgradesToAdd > 0)
                            {
                                upgradesToAdd++;
                                pd.downgrades.RemoveAt(i);
                                i--;
                                continue;
                            }
                        }

                        for (int i = 0; i < -upgradesToAdd; i++)
                        {
                            pd.downgrades.Add(PlayerData.LevelUpgrade.HP);
                        }
                    }

                    for (int i = 0; i < upgradesToAdd; i++)
                    {
                        if (index == 0)
                        {
                            pd.upgrades.Add(PlayerData.LevelUpgrade.HP);
                        }
                        if (index == 1)
                        {
                            pd.upgrades.Add(PlayerData.LevelUpgrade.EP);
                        }
                        if (index == 2)
                        {
                            pd.upgrades.Add(PlayerData.LevelUpgrade.SP);
                        }
                    }

                    pd.coins -= level * costPerChange;
                    pd.astralTokens -= level;
                    pd.UpdateMaxStats();



                    SetupMaximums();
                    yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 9, this));
                }
            }
        }


        while (state == 2)
        {
            string[] tempVars = new string[] { GetCurrentHPString(), GetCurrentEPString(), GetCurrentSPString(), GetCurrentAstralString() };

            //menu
            yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 10, this, tempVars));

            int index = -1;
            menuResult = MainManager.Instance.lastTextboxMenuResult;
            int.TryParse(FormattedString.ParseArg(menuResult, "arg2"), out index);

            if (FormattedString.ParseArg(menuResult, "arg0").Equals("cancel"))
            {
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 14, this));
                yield break;
            }
            else
            {
                //not in the cancel state so there is a max level value
                int level = 0;
                int.TryParse(FormattedString.ParseArg(menuResult, "arg1"), out level);

                string[] tempVarsB = null;
                if (index == 0)
                {
                    tempVarsB = new string[] { "HP", GetCurrentHPString(), GetCurrentHPString(-level) };
                }
                if (index == 1)
                {
                    tempVarsB = new string[] { "EP", GetCurrentEPString(), GetCurrentEPString(-level) };
                }
                if (index == 2)
                {
                    tempVarsB = new string[] { "SP", GetCurrentSPString(), GetCurrentSPString(-level) };
                }
                yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 11, this, tempVarsB));

                int ynstate = 0;
                menuResult = MainManager.Instance.lastTextboxMenuResult;
                int.TryParse(menuResult, out ynstate);

                if (ynstate == 1)
                {
                    //chose to raise a stat

                    int upgradesRemoved = 0;
                    for (int i = 0; i < pd.upgrades.Count; i++)
                    {
                        bool remove = false;

                        if (index == 0 && pd.upgrades[i] == PlayerData.LevelUpgrade.HP)
                        {
                            remove = true;
                        }
                        if (index == 1 && pd.upgrades[i] == PlayerData.LevelUpgrade.EP)
                        {
                            remove = true;
                        }
                        if (index == 2 && pd.upgrades[i] == PlayerData.LevelUpgrade.SP)
                        {
                            remove = true;
                        }

                        if (remove)
                        {
                            upgradesRemoved++;
                            pd.upgrades.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    int downgradesToAdd = level - upgradesRemoved;

                    if (downgradesToAdd < 0)
                    {
                        //??? impossible state
                        for (int i = 0; i < pd.downgrades.Count; i++)
                        {
                            bool remove = false;

                            if (index == 0 && pd.downgrades[i] == PlayerData.LevelUpgrade.HP)
                            {
                                remove = true;
                            }
                            if (index == 1 && pd.downgrades[i] == PlayerData.LevelUpgrade.EP)
                            {
                                remove = true;
                            }
                            if (index == 2 && pd.downgrades[i] == PlayerData.LevelUpgrade.SP)
                            {
                                remove = true;
                            }

                            if (downgradesToAdd == 0)
                            {
                                break;
                            }

                            if (remove && downgradesToAdd < 0)
                            {
                                downgradesToAdd++;
                                pd.downgrades.RemoveAt(i);
                                i--;
                                continue;
                            }
                        }

                        for (int i = 0; i < -downgradesToAdd; i++)
                        {
                            pd.upgrades.Add(PlayerData.LevelUpgrade.HP);
                        }
                    }

                    for (int i = 0; i < downgradesToAdd; i++)
                    {
                        if (index == 0)
                        {
                            pd.downgrades.Add(PlayerData.LevelUpgrade.HP);
                        }
                        if (index == 1)
                        {
                            pd.downgrades.Add(PlayerData.LevelUpgrade.EP);
                        }
                        if (index == 2)
                        {
                            pd.downgrades.Add(PlayerData.LevelUpgrade.SP);
                        }
                    }

                    pd.astralTokens += level;

                    int badgeCount = pd.equippedBadges.Count;
                    pd.UpdateMaxStats();

                    if (badgeCount == pd.equippedBadges.Count)
                    {
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 12, this));
                    } else
                    {
                        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 13, this));
                    }

                    SetupMaximums();
                }
            }
        }
    }
    public override string RequestTextData(string request)
    {
        //Debug.Log("Request " + request);

        PlayerData pd = MainManager.Instance.playerData;

        if (request.Equals("raise"))
        {
            //name, right text, canUse, desc, max level, backgroundColor (invalid = no background)
            //(End of list = level descriptor)

            //Pack a list
            List<string> nameList = new List<string>
            {
                "Raise HP",
                "Raise EP",
                "Raise SP"
            };

            List<string> rightTextList = new List<string>();
            List<bool> canUseList = new List<bool>();
            List<string> descList = new List<string>();
            List<int> maxLevelList = new List<int>();

            string current = "";

            if (raiseMaxLevels[0] == 0) { 
                if (pd.party.Count == 1)
                {
                    current += pd.GetMaxHP(pd.party[0].entityID);
                } else
                {
                    current += pd.GetMaxHP(BattleHelper.EntityID.Wilex);
                    current += ",";
                    current += pd.GetMaxHP(BattleHelper.EntityID.Luna);
                }
                current += "<hp>";
                canUseList.Add(false);
                descList.Add("Raise your Max HP at the cost of coins and Astral Tokens.");
                maxLevelList.Add(1);
            }
            else
            {
                current += "<rarrow>";

                int offset = 0;
                if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Greed))
                {
                    offset = 1;
                }
                if (pd.party.Count == 1)
                {
                    current += PlayerData.PlayerDataEntry.GetMaxHP(pd.party[0].entityID, currentLevels[0] + 1 - offset);
                }
                else
                {
                    current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Wilex, currentLevels[0] + 1 - offset);
                    current += ",";
                    current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Luna, currentLevels[0] + 1 - offset);
                }
                current += "<hp>,";
                if (costPerChange > 0)
                {
                    current += costPerChange.ToString();
                    current += "<coin>,";
                }
                current += 1;
                current += "<astraltoken>";

                canUseList.Add(true);
                descList.Add("Raise your Max HP at the cost of coins and Astral Tokens.");
                maxLevelList.Add(raiseMaxLevels[0]);
            }
            rightTextList.Add(current);



            current = "";

            if (raiseMaxLevels[1] == 0)
            {
                current += pd.GetMaxEP();
                current += "<ep>";
                canUseList.Add(false);
                descList.Add("Raise your Max EP at the cost of coins and Astral Tokens.");
                maxLevelList.Add(1);
            }
            else
            {
                current += "<rarrow>";

                current += PlayerData.GetMaxEP(currentLevels[1] + 1);
                current += "<ep>,";
                if (costPerChange > 0)
                {
                    current += costPerChange.ToString();
                    current += "<coin>,";
                }
                current += 1;
                current += "<astraltoken>";

                canUseList.Add(true);
                descList.Add("Raise your Max EP at the cost of coins and Astral Tokens.");
                maxLevelList.Add(raiseMaxLevels[1]);
            }
            rightTextList.Add(current);



            current = "";

            if (raiseMaxLevels[2] == 0)
            {
                current += pd.GetMaxSP();
                current += "<sp>";
                canUseList.Add(false);
                descList.Add("Raise your Max SP at the cost of coins and Astral Tokens.");
                maxLevelList.Add(1);
            }
            else
            {
                current += "<rarrow>";

                current += PlayerData.GetMaxSP(currentLevels[2] + 1);
                current += "<sp>,";
                if (costPerChange > 0)
                {
                    current += costPerChange.ToString();
                    current += "<coin>,";
                }
                current += 1;
                current += "<astraltoken>";

                canUseList.Add(true);
                descList.Add("Raise your Max SP at the cost of coins and Astral Tokens.");
                maxLevelList.Add(raiseMaxLevels[2]);
            }
            rightTextList.Add(current);


            string output = GenericBoxMenu.PackMenuString("", null, nameList, rightTextList, canUseList, descList, maxLevelList);
            //Debug.Log(output);
            return output;
        }



        if (request.Equals("lower"))
        {
            //name, right text, canUse, desc, max level, backgroundColor (invalid = no background)
            //(End of list = level descriptor)

            //Pack a list
            List<string> nameList = new List<string>
            {
                "Lower HP",
                "Lower EP",
                "Lower SP"
            };

            List<string> rightTextList = new List<string>();
            List<bool> canUseList = new List<bool>();
            List<string> descList = new List<string>();
            List<int> maxLevelList = new List<int>();

            string current = "";

            if (lowerMaxLevels[0] == 0)
            {
                if (pd.party.Count == 1)
                {
                    current += pd.GetMaxHP(pd.party[0].entityID);
                }
                else
                {
                    current += pd.GetMaxHP(BattleHelper.EntityID.Wilex);
                    current += ",";
                    current += pd.GetMaxHP(BattleHelper.EntityID.Luna);
                }
                current += "<hp>";
                canUseList.Add(false);
                descList.Add("Lower your Max HP to receive Astral Tokens.");
                maxLevelList.Add(1);
            }
            else
            {
                current += "<rarrow>";

                int offset = 0;
                if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Greed))
                {
                    offset = 1;
                }
                if (pd.party.Count == 1)
                {
                    current += PlayerData.PlayerDataEntry.GetMaxHP(pd.party[0].entityID, currentLevels[0] - 1 - offset);
                }
                else
                {
                    current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Wilex, currentLevels[0] - 1 - offset);
                    current += ",";
                    current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Luna, currentLevels[0] - 1 - offset);
                }
                current += "<hp>,+";
                current += 1;
                current += "<astraltoken>";

                canUseList.Add(true);
                descList.Add("Lower your Max HP to receive Astral Tokens.");
                maxLevelList.Add(lowerMaxLevels[0]);
            }
            rightTextList.Add(current);



            current = "";

            if (lowerMaxLevels[1] == 0)
            {
                current += pd.GetMaxEP();
                current += "<ep>";
                canUseList.Add(false);
                descList.Add("Lower your Max EP to receive Astral Tokens.");
                maxLevelList.Add(1);
            }
            else
            {
                current += "<rarrow>";

                current += PlayerData.GetMaxEP(currentLevels[1] - 1);
                current += "<ep>,+";
                current += 1;
                current += "<astraltoken>";

                canUseList.Add(true);
                descList.Add("Lower your Max EP to receive Astral Tokens.");
                maxLevelList.Add(lowerMaxLevels[1]);
            }
            rightTextList.Add(current);



            current = "";

            if (lowerMaxLevels[2] == 0)
            {
                current += pd.GetMaxSP();
                current += "<sp>";
                canUseList.Add(false);
                descList.Add("Lower your Max SP to receive Astral Tokens.");
                maxLevelList.Add(1);
            }
            else
            {
                current += "<rarrow>";

                current += PlayerData.GetMaxSP(currentLevels[2] - 1);
                current += "<sp>,+";
                current += 1;
                current += "<astraltoken>";

                canUseList.Add(true);
                descList.Add("Lower your Max SP to receive Astral Tokens.");
                maxLevelList.Add(lowerMaxLevels[2]);
            }
            rightTextList.Add(current);


            string output = GenericBoxMenu.PackMenuString("", null, nameList, rightTextList, canUseList, descList, maxLevelList);
            //Debug.Log(output);
            return output;
        }

        string index = FormattedString.ParseArg(request, "arg1");
        int intLevel = -1;
        int.TryParse(index, out intLevel);

        if (FormattedString.ParseArg(request, "arg0").Equals("Raise HP"))
        {
            string current = "Raise HP|";
            //make one entry
            current += "<rarrow>";

            int offset = 0;
            if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Greed))
            {
                offset = 1;
            }
            if (pd.party.Count == 1)
            {
                current += PlayerData.PlayerDataEntry.GetMaxHP(pd.party[0].entityID, currentLevels[0] + intLevel - offset);
            }
            else
            {
                current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Wilex, currentLevels[0] + intLevel - offset);
                current += ",";
                current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Luna, currentLevels[0] + intLevel - offset);
            }
            current += "<hp>,";
            if (costPerChange > 0)
            {
                current += (intLevel * costPerChange).ToString();
                current += "<coin>,";
            }
            current += intLevel;
            current += "<astraltoken>";

            current += "|";

            bool canUse = (pd.astralTokens >= intLevel) && (pd.coins >= (intLevel * costPerChange));
            current += canUse.ToString();

            return current;
        }

        if (FormattedString.ParseArg(request, "arg0").Equals("Raise EP"))
        {
            string current = "Raise EP|";
            //make one entry
            current += "<rarrow>";

            current += PlayerData.GetMaxEP(currentLevels[1] + intLevel);
            current += "<ep>,";
            if (costPerChange > 0)
            {
                current += (intLevel * costPerChange).ToString();
                current += "<coin>,";
            }
            current += intLevel;
            current += "<astraltoken>";

            current += "|";

            bool canUse = (pd.astralTokens >= intLevel) && (pd.coins >= (intLevel * costPerChange));
            current += canUse.ToString();

            return current;
        }

        if (FormattedString.ParseArg(request, "arg0").Equals("Raise SP"))
        {
            string current = "Raise SP|";
            //make one entry
            current += "<rarrow>";

            current += PlayerData.GetMaxEP(currentLevels[2] + intLevel);
            current += "<sp>,";
            if (costPerChange > 0)
            {
                current += (intLevel * costPerChange).ToString();
                current += "<coin>,";
            }
            current += intLevel;
            current += "<astraltoken>";

            current += "|";

            bool canUse = (pd.astralTokens >= intLevel) && (pd.coins >= (intLevel * costPerChange));
            current += canUse.ToString();

            return current;
        }


        if (FormattedString.ParseArg(request, "arg0").Equals("Lower HP"))
        {
            string current = "Lower HP|";
            //make one entry
            current += "<rarrow>";

            int offset = 0;
            if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Greed))
            {
                offset = 1;
            }
            if (pd.party.Count == 1)
            {
                current += PlayerData.PlayerDataEntry.GetMaxHP(pd.party[0].entityID, currentLevels[0] - intLevel - offset);
            }
            else
            {
                current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Wilex, currentLevels[0] - intLevel - offset);
                current += ",";
                current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Luna, currentLevels[0] - intLevel - offset);
            }
            current += "<hp>,+";
            current += intLevel;
            current += "<astraltoken>";

            return current;
        }

        if (FormattedString.ParseArg(request, "arg0").Equals("Lower EP"))
        {
            string current = "Lower EP|";
            //make one entry
            current += "<rarrow>";

            current += PlayerData.GetMaxEP(currentLevels[1] - intLevel);
            current += "<ep>,+";
            current += intLevel;
            current += "<astraltoken>";

            return current;
        }

        if (FormattedString.ParseArg(request, "arg0").Equals("Lower SP"))
        {
            string current = "Lower SP|";
            //make one entry
            current += "<rarrow>";

            current += PlayerData.GetMaxEP(currentLevels[2] - intLevel);
            current += "<sp>,+";
            current += intLevel;
            current += "<astraltoken>";

            return current;
        }


        return "nop";
    }

    public override void SendTextData(string data)
    {
        Debug.Log("Received data: " + data);
    }

    public string GetCurrentHPString(int delta = 0)
    {
        PlayerData pd = MainManager.Instance.playerData;
        int offset = 0;
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Greed))
        {
            offset = 1;
        }
        string current = "";
        if (pd.party.Count == 1)
        {
            current += PlayerData.PlayerDataEntry.GetMaxHP(pd.party[0].entityID, currentLevels[0] + delta - offset);
        }
        else
        {
            current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Wilex, currentLevels[0] + delta - offset);
            current += ",";
            current += PlayerData.PlayerDataEntry.GetMaxHP(BattleHelper.EntityID.Luna, currentLevels[0] + delta - offset);
        }
        current += "<hp>";
        return current;
    }
    public string GetCurrentEPString(int delta = 0)
    {
        string current = "";
        current += PlayerData.GetMaxEP(currentLevels[1] + delta);
        current += "<ep>";
        return current;
    }
    public string GetCurrentSPString(int delta = 0)
    {
        string current = "";
        current += PlayerData.GetMaxSP(currentLevels[2] + delta);
        current += "<sp>";
        return current;
    }
    public string GetCurrentAstralString()
    {
        PlayerData pd = MainManager.Instance.playerData;
        string current = "";
        current += pd.astralTokens;
        current += "<astraltoken>";
        return current;
    }
    public void SetupMaximums()
    {
        PlayerData pd = MainManager.Instance.playerData;

        int hpUpgrades = 0;
        int epUpgrades = 0;
        int spUpgrades = 0;
        int hpDowngrades = 0;
        int epDowngrades = 0;
        int spDowngrades = 0;

        for (int i = 0; i < pd.upgrades.Count; i++)
        {
            if (pd.upgrades[i] == PlayerData.LevelUpgrade.HP)
            {
                hpUpgrades++;
            }
            if (pd.upgrades[i] == PlayerData.LevelUpgrade.EP)
            {
                epUpgrades++;
            }
            if (pd.upgrades[i] == PlayerData.LevelUpgrade.SP)
            {
                spUpgrades++;
            }
        }
        for (int i = 0; i < pd.downgrades.Count; i++)
        {
            if (pd.downgrades[i] == PlayerData.LevelUpgrade.HP)
            {
                hpDowngrades++;
            }
            if (pd.downgrades[i] == PlayerData.LevelUpgrade.EP)
            {
                epDowngrades++;
            }
            if (pd.downgrades[i] == PlayerData.LevelUpgrade.SP)
            {
                spDowngrades++;
            }
        }


        int hpDelta = hpUpgrades - hpDowngrades;
        int epDelta = epUpgrades - epDowngrades;
        int spDelta = spUpgrades - spDowngrades;

        //Minimum is minus 2
        //Max is 8 or 10
        int upgradeMax = PlayerData.GetMaxUpgrades();

        raiseMaxLevels = new int[3];
        lowerMaxLevels = new int[3];
        currentLevels = new int[3];

        currentLevels[0] = hpDelta;
        currentLevels[1] = epDelta;
        currentLevels[2] = spDelta;

        raiseMaxLevels[0] = upgradeMax - hpDelta;
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Greed))
        {
            raiseMaxLevels[0] = 0 - hpDelta;
        }

        raiseMaxLevels[1] = upgradeMax - epDelta;
        raiseMaxLevels[2] = upgradeMax - spDelta;

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Envy))
        {
            //No
            raiseMaxLevels[2] = 0;
        }

        //check costs
        int maxRaise = 100;
        if (costPerChange > 0)
        {
            maxRaise = pd.coins / costPerChange;
        }

        //raise is also capped by astral tokens
        if (maxRaise > pd.astralTokens)
        {
            maxRaise = pd.astralTokens;
        }

        for (int i = 0; i < raiseMaxLevels.Length; i++)
        {
            if (raiseMaxLevels[i] > maxRaise)
            {
                raiseMaxLevels[i] = maxRaise;
            }
            if (raiseMaxLevels[i] < 0)
            {
                raiseMaxLevels[i] = 0;
            }
        }


        lowerMaxLevels[0] = hpDelta - (-2);
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Greed))
        {
            lowerMaxLevels[0] = hpDelta - (-1);
        }

        lowerMaxLevels[1] = epDelta - (-2);
        lowerMaxLevels[2] = spDelta - (-2);

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Envy))
        {
            //No
            lowerMaxLevels[2] = 0;
        }

        for (int i = 0; i < lowerMaxLevels.Length; i++)
        {
            if (lowerMaxLevels[i] < 0)
            {
                lowerMaxLevels[i] = 0;
            }
        }
    }
}
