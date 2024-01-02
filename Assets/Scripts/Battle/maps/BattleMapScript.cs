using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//used in battlecontrol for the battle map stuff
public class BattleMapScript : MonoBehaviour
{
    //public MainManager.BattleMapID mapName;       //not necessary
    //public MainManager.SkyboxID skyboxID;
    public string skyboxID;
    
    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    public virtual void OnPreTurn() //after turn increment
    {

    }

    public virtual void OnPostTurn()
    {

    }

    public virtual void OnBattleStart()
    {
        //Default enviro effect thing

        BattleHelper.EnvironmentalEffect effect = BattleHelper.EnvironmentalEffect.None;

        Enum.TryParse(MainManager.Instance.mapScript.worldLocation, out MainManager.WorldLocation wl);
        switch (wl)
        {
            case MainManager.WorldLocation.SacredGrove:
                effect = BattleHelper.EnvironmentalEffect.SacredGrove;
                break;
            case MainManager.WorldLocation.TrialOfSimplicity:
                effect = BattleHelper.EnvironmentalEffect.TrialOfSimplicity;
                break;
            case MainManager.WorldLocation.TempestDesert:
                effect = BattleHelper.EnvironmentalEffect.ElectricWind;
                break;
            case MainManager.WorldLocation.HiddenOasis:
                effect = BattleHelper.EnvironmentalEffect.IonizedSand;
                break;
            case MainManager.WorldLocation.TrialOfHaste:
                effect = BattleHelper.EnvironmentalEffect.TrialOfHaste;
                break;
            case MainManager.WorldLocation.GemstoneIslands:
                effect = BattleHelper.EnvironmentalEffect.SeasideAir;
                break;
            case MainManager.WorldLocation.SapphireAtoll:
                effect = BattleHelper.EnvironmentalEffect.CounterWave;
                break;
            case MainManager.WorldLocation.TrialOfPatience:
                effect = BattleHelper.EnvironmentalEffect.TrialOfPatience;
                break;
            case MainManager.WorldLocation.InfernalCaldera:
                effect = BattleHelper.EnvironmentalEffect.ScaldingHeat;
                break;
            case MainManager.WorldLocation.MoltenPit:
                effect = BattleHelper.EnvironmentalEffect.ScaldingMagma;
                break;
            case MainManager.WorldLocation.TrialOfZeal:
                effect = BattleHelper.EnvironmentalEffect.TrialOfZeal;
                break;
            case MainManager.WorldLocation.ShroudedValley:
                effect = BattleHelper.EnvironmentalEffect.DarkFog;
                break;
            case MainManager.WorldLocation.SinisterCave:
                effect = BattleHelper.EnvironmentalEffect.VoidShadow;
                break;
            case MainManager.WorldLocation.TrialOfAmbition:
                effect = BattleHelper.EnvironmentalEffect.TrialOfAmbition;
                break;
            case MainManager.WorldLocation.RadiantPlateau:
                effect = BattleHelper.EnvironmentalEffect.FrigidBreeze;
                break;
            case MainManager.WorldLocation.RadiantPeak:
                effect = BattleHelper.EnvironmentalEffect.WhiteoutBlizzard;
                break;
            case MainManager.WorldLocation.TrialOfResolve:
                effect = BattleHelper.EnvironmentalEffect.TrialOfResolve;
                break;
            case MainManager.WorldLocation.AetherTrench:
                effect = BattleHelper.EnvironmentalEffect.AetherHunger;
                break;
            case MainManager.WorldLocation.MoltenTitan:
                effect = BattleHelper.EnvironmentalEffect.DigestiveAcid;
                break;
        }

        BattleControl.Instance.enviroEffect = effect;
    }

    public virtual void OnBattleEnd()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public MainManager.SkyboxID GetSkyboxID()
    {
        Enum.TryParse(skyboxID, out MainManager.SkyboxID si);
        return si;
    }
}