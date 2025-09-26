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
    public Color ambientLight = new Color(0.5f, 0.5f, 0.5f);    //Should be roughly (5/7) the light's color? (with light intensity = 1)
    public bool useDefaultLight;

    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    //Now you can do stuff in the pre and post turn
    public virtual IEnumerator OnPreTurn() //after turn increment
    {
        yield break;
    }

    public virtual IEnumerator OnPostTurn()
    {
        yield break;
    }
    public virtual IEnumerator OnOutOfTurn()
    {
        yield break;
    }

    public virtual void React(BattleEntity b, BattleHelper.Event eventID)
    {

    }

    public virtual void OnBattleStart()
    {
        //Default enviro effect thing

        BattleHelper.EnvironmentalEffect effect = BattleHelper.EnvironmentalEffect.None;

        //Note: For stuff like fog this is necessary because otherwise the fog would appear in battle and obscure everything
        //(i.e. you would have to change out the material anyway even if you wanted battle fog for some reason)
        WorldCamera.CameraEffect camEffect = WorldCamera.CameraEffect.None;

        Enum.TryParse(MainManager.Instance.mapScript.worldLocation, out MainManager.WorldLocation wl);
        if (useDefaultLight)
        {
            RenderSettings.ambientLight = MainManager.GetDefaultAmbientColor(wl);
            Light l = GetComponentInChildren<Light>();
            if (l != null)
            {
                l.color = MainManager.GetDefaultLightColor(wl);
            }
        } else
        {
            RenderSettings.ambientLight = ambientLight;
        }
        switch (wl)
        {
            case MainManager.WorldLocation.SacredGrove:
                effect = BattleHelper.EnvironmentalEffect.SacredGrove;
                camEffect = WorldCamera.CameraEffect.VignetteLeafy;
                break;
            case MainManager.WorldLocation.TrialOfSimplicity:
                effect = BattleHelper.EnvironmentalEffect.TrialOfSimplicity;
                camEffect = WorldCamera.CameraEffect.VignetteLeafy;
                break;
            case MainManager.WorldLocation.TempestDesert:
                effect = BattleHelper.EnvironmentalEffect.ElectricWind;
                camEffect = WorldCamera.CameraEffect.VignetteWeakSpark;
                break;
            case MainManager.WorldLocation.HiddenOasis:
                effect = BattleHelper.EnvironmentalEffect.IonizedSand;
                camEffect = WorldCamera.CameraEffect.VignetteSpark;
                break;
            case MainManager.WorldLocation.TrialOfHaste:
                effect = BattleHelper.EnvironmentalEffect.TrialOfHaste;
                camEffect = WorldCamera.CameraEffect.VignetteSpark;
                break;
            case MainManager.WorldLocation.GemstoneIslands:
                effect = BattleHelper.EnvironmentalEffect.SeasideAir;
                camEffect = WorldCamera.CameraEffect.VignetteWeakWater;
                break;
            case MainManager.WorldLocation.SapphireAtoll:
                effect = BattleHelper.EnvironmentalEffect.CounterWave;
                camEffect = WorldCamera.CameraEffect.VignetteWater;
                break;
            case MainManager.WorldLocation.TrialOfPatience:
                effect = BattleHelper.EnvironmentalEffect.TrialOfPatience;
                camEffect = WorldCamera.CameraEffect.VignetteWater;
                break;
            case MainManager.WorldLocation.InfernalCaldera:
                effect = BattleHelper.EnvironmentalEffect.ScaldingHeat;
                camEffect = WorldCamera.CameraEffect.VignetteWeakFire;
                break;
            case MainManager.WorldLocation.MoltenPit:
                effect = BattleHelper.EnvironmentalEffect.ScaldingMagma;
                camEffect = WorldCamera.CameraEffect.VignetteFire;
                break;
            case MainManager.WorldLocation.TrialOfZeal:
                effect = BattleHelper.EnvironmentalEffect.TrialOfZeal;
                camEffect = WorldCamera.CameraEffect.VignetteFire;
                break;
            case MainManager.WorldLocation.ShroudedValley:
                effect = BattleHelper.EnvironmentalEffect.DarkFog;
                camEffect = WorldCamera.CameraEffect.VignetteWeakDark;
                break;
            case MainManager.WorldLocation.SinisterCave:
                effect = BattleHelper.EnvironmentalEffect.VoidShadow;
                camEffect = WorldCamera.CameraEffect.VignetteDark;
                break;
            case MainManager.WorldLocation.TrialOfAmbition:
                effect = BattleHelper.EnvironmentalEffect.TrialOfAmbition;
                camEffect = WorldCamera.CameraEffect.VignetteDark;
                break;
            case MainManager.WorldLocation.RadiantPlateau:
                effect = BattleHelper.EnvironmentalEffect.FrigidBreeze;
                camEffect = WorldCamera.CameraEffect.VignetteWeakIce;
                break;
            case MainManager.WorldLocation.RadiantPeak:
                effect = BattleHelper.EnvironmentalEffect.WhiteoutBlizzard;
                camEffect = WorldCamera.CameraEffect.VignetteIce;
                break;
            case MainManager.WorldLocation.TrialOfResolve:
                effect = BattleHelper.EnvironmentalEffect.TrialOfResolve;
                camEffect = WorldCamera.CameraEffect.VignetteIce;
                break;
            case MainManager.WorldLocation.AetherTrench:
                effect = BattleHelper.EnvironmentalEffect.AetherHunger;
                camEffect = WorldCamera.CameraEffect.VignetteAetherRays;
                break;
            case MainManager.WorldLocation.MoltenTitan:
                effect = BattleHelper.EnvironmentalEffect.DigestiveAcid;
                camEffect = WorldCamera.CameraEffect.VignetteAcid;
                break;
        }

        BattleControl.Instance.enviroEffect = effect;

        //Camera effect thing :)
        MainManager.Instance.Camera.ProcessEffectChange(camEffect);
    }

    public virtual void OnBattleEnd()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    //Note: No skybox = uses the previously loaded skybox
    //if you want a black skybox you have to explicitly load it
    public bool HasSkybox()
    {
        return skyboxID != null && skyboxID.Length > 0;
    }

    public MainManager.SkyboxID GetSkyboxID()
    {
        Enum.TryParse(skyboxID, out MainManager.SkyboxID si);
        return si;
    }
}