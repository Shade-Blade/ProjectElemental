using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitNPCSpawner : WorldObject
{
    public List<GameObject> positions;
    public List<float> chances;

    public float limWeight;

    public List<GameObject> npcList;

    public override void Awake()
    {
        base.Awake();

        PlayerData pd = MainManager.Instance.playerData;

        List<GameObject> shuffledPositions = MainManager.ShuffleList(positions);

        bool hardcodeVali = false;
        bool hardcodePalla = false;

        if (pd.astralTokens > 0)
        {
            hardcodeVali = true;
        }
        if (pd.storageInventory.Count > 0)
        {
            hardcodePalla = true;
        }

        int j = 0;

        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = 1.ToString();
        }
        int floor = int.Parse(floorNo);

        List<PitNPCHolderScript.PitNPC> pastResults = new List<PitNPCHolderScript.PitNPC>();

        for (int i = 0; i < positions.Count; i++)
        {
            if (RandomGenerator.Get() < chances[j])
            {
                //Generate an npc at position

                bool resultLegal = false;
                List<IRandomTableEntry<PitNPCHolderScript.PitNPC>> miscTableEntries = new List<IRandomTableEntry<PitNPCHolderScript.PitNPC>>
                {
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Lim, limWeight),
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Rosette, HasPowerCharmTotem() ? 0.25f: 0.75f),
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Bead, HasFortuneCharmTotem() ? 0.25f: 0.75f),
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.InnkeeperGryphon, pd.GetStatPercentage() > 0.4f ? 0.5f : 4), //stand in for all innkeepers
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Vali, 0.5f),
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Palla, pd.itemInventory.Count >= pd.maxInventorySize - 2 ? 1 : 0.5f),
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Torstrum, pd.itemInventory.Count > 3 ? 1.5f : 0.5f),   //stand in for all cooks
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Wolfram, (pd.coins > 250 || pd.badgeInventory.Count < (floor / 2)) ? 2 : 1),
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Alumi, (pd.coins > 250 || pd.ribbonInventory.Count < (floor / 15)) ? 2 : 1),
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Spruce, pd.coins > 200 ? 2 : 1),
                    new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Roielle, (pd.coins > 250 || pd.GetHealthPercentage() > 0.666f || pd.GetEnergyPercentage() > 0.666f) ? 2 : 1), //stand in for the 3 "rare item" merchants
                };
                RandomTable<PitNPCHolderScript.PitNPC> mstable = new RandomTable<PitNPCHolderScript.PitNPC>(miscTableEntries);

                PitNPCHolderScript.PitNPC msresult = PitNPCHolderScript.PitNPC.Lim;
                while (!resultLegal)
                {
                    msresult = mstable.Output();
                    resultLegal = true;

                    if (pastResults.Contains(msresult))
                    {
                        resultLegal = false;
                    }
                }

                if (hardcodeVali && msresult != PitNPCHolderScript.PitNPC.Lim)
                {
                    msresult = PitNPCHolderScript.PitNPC.Vali;
                    hardcodeVali = false;
                    j--;    //use the higher chances for the next npc spawn (so that hardcoded spawns don't block others from spawning as often)
                }
                if (hardcodePalla && msresult != PitNPCHolderScript.PitNPC.Lim)
                {
                    msresult = PitNPCHolderScript.PitNPC.Palla;
                    hardcodePalla = false;
                    j--;
                }
                pastResults.Add(msresult);

                if (msresult == PitNPCHolderScript.PitNPC.Torstrum)
                {
                    List<IRandomTableEntry<PitNPCHolderScript.PitNPC>> cookTableEntries = new List<IRandomTableEntry<PitNPCHolderScript.PitNPC>>
                    {
                        new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Torstrum, pd.itemInventory.Count > 6 ? 2 : 1f),
                        new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Stella, pd.itemInventory.Count > 3 ? 2 : 1f),
                        new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Sizzle, pd.itemInventory.Count > 6 ? 2 : 1f),
                        new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Gourmand, 2)
                    };
                    RandomTable<PitNPCHolderScript.PitNPC> cookTable = new RandomTable<PitNPCHolderScript.PitNPC>(cookTableEntries);

                    msresult = cookTable.Output();
                }

                if (msresult == PitNPCHolderScript.PitNPC.Roielle)
                {
                    List<IRandomTableEntry<PitNPCHolderScript.PitNPC>> shopTableEntries = new List<IRandomTableEntry<PitNPCHolderScript.PitNPC>>
                    {
                        new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.Roielle, pd.coins > 250 ? 2 : 1f),
                        new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.ShopkeeperMosquito, pd.GetHealthPercentage() > 0.666f ? 2 : 1f),
                        new RandomTableEntry<PitNPCHolderScript.PitNPC>(PitNPCHolderScript.PitNPC.ShopkeeperSpeartongue, pd.GetEnergyPercentage() > 0.666f ? 2 : 1f),
                    };
                    RandomTable<PitNPCHolderScript.PitNPC> shopTable = new RandomTable<PitNPCHolderScript.PitNPC>(shopTableEntries);

                    msresult = shopTable.Output();
                }

                if (msresult == PitNPCHolderScript.PitNPC.InnkeeperGryphon)
                {
                    //determine correct one to spawn
                    int index = (floor / 10);
                    if (RandomGenerator.Get() < 0.5f)
                    {
                        index += 1;
                    }
                    if (RandomGenerator.Get() < 0.3f)
                    {
                        index += 1 * (RandomGenerator.Get() < 0.667f ? 1 : -1);
                    }
                    if (RandomGenerator.Get() < 0.3f)
                    {
                        index += 1 * (RandomGenerator.Get() < 0.667f ? 1 : -1);
                    }

                    if (index < 0)
                    {
                        index = 0;
                    }
                    if (index > 11)
                    {
                        index = 11;
                    }

                    switch (index)
                    {
                        case 0:
                            msresult = PitNPCHolderScript.PitNPC.InnkeeperGryphon;
                            break;
                        case 1:
                            msresult = PitNPCHolderScript.PitNPC.Pyri;
                            break;
                        case 2:
                            msresult = PitNPCHolderScript.PitNPC.InnkeeperRabbit;
                            break;
                        case 3:
                            msresult = PitNPCHolderScript.PitNPC.Glaze;
                            break;
                        case 4:
                            msresult = PitNPCHolderScript.PitNPC.InnkeeperJellyfish;
                            break;
                        case 5:
                            msresult = PitNPCHolderScript.PitNPC.InnkeeperAshcrest;
                            break;
                        case 6:
                            msresult = PitNPCHolderScript.PitNPC.InnkeeperMosquito;
                            break;
                        case 7:
                            msresult = PitNPCHolderScript.PitNPC.InnkeeperHawk;
                            break;
                        case 8:
                            msresult = PitNPCHolderScript.PitNPC.InnkeeperChaintail;
                            break;
                        case 9:
                            msresult = PitNPCHolderScript.PitNPC.Aurelia;
                            break;
                        case 10:
                            msresult = PitNPCHolderScript.PitNPC.InnkeeperPlaguebud;
                            break;
                        case 11:
                            msresult = PitNPCHolderScript.PitNPC.Blanca;
                            break;
                    }
                }

                //Create npc using result

                GameObject go = Instantiate(npcList[(int)msresult], shuffledPositions[i].transform);
                PitNPCHolderScript pnhs = go.GetComponent<PitNPCHolderScript>();
                pnhs.Initialize();
            }
            j++;
        }
    }

    public bool HasPowerCharmTotem()
    {
        PlayerData pd = MainManager.Instance.playerData;
        for (int i = 0; i < pd.charmEffects.Count; i++)
        {
            if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Attack)
            {
                return true;
            }
            if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Defense)
            {
                return true;
            }
        }

        for (int i = 0; i < pd.keyInventory.Count; i++)
        {
            if (pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemA)
            {
                return true;
            }
            if (pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemB)
            {
                return true;
            }
            if (pd.keyInventory[i].type == KeyItem.KeyItemType.PowerTotemC)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasFortuneCharmTotem()
    {
        PlayerData pd = MainManager.Instance.playerData;
        for (int i = 0; i < pd.charmEffects.Count; i++)
        {
            if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Fortune)
            {
                return true;
            }
        }

        for (int i = 0; i < pd.keyInventory.Count; i++)
        {
            if (pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemA)
            {
                return true;
            }
            if (pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemB)
            {
                return true;
            }
            if (pd.keyInventory[i].type == KeyItem.KeyItemType.FortuneTotemC)
            {
                return true;
            }
        }

        return false;
    }
}
