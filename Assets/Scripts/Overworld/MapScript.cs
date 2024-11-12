using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript : MonoBehaviour
{
    //inspector settings
    public string mapName;
    //public MainManager.MapID mapName;
    public string worldLocation;
    //public MainManager.WorldLocation worldLocation; //should coincide with skybox ID (but may not always)
    public string skyboxID;
    //public MainManager.SkyboxID skyboxID;
    public Color ambientLight = new Color(0.5f, 0.5f, 0.5f);    //Should be roughly (5/7) the light's color? (with light intensity = 1)
    public GameObject playerHolder;    //player becomes child of this

    //runtime variables

    //Note: May not be necessarily used, often I will probably just put variables in the things that need them (switch states probably are stored in the switch for example)
    //public bool[] flags;
    //public string[] vars;
    public Dictionary<string, bool> flags;
    public Dictionary<string, string> vars;

    [HideInInspector]
    public string[][] textFile;

    //public bool[] enemiesActive;

    [HideInInspector]
    public List<MapExit> exits;

    [HideInInspector]
    public List<WorldObject> worldObjects;  //unordered (no ids)
    [HideInInspector]
    public List<WorldEntity> worldEntities; //ordered by meid

    [HideInInspector]
    public bool halted = false;

    [HideInInspector]
    public bool inEncounter = false;
    [HideInInspector]
    public IWorldBattleEntity worldBattleEntity;    //The entity you get into a battle with (also has to contain the cutscene handlers for ending battle)

    [HideInInspector]
    public bool battleHalt = false;

    bool init = false;

    [HideInInspector]
    public bool exitsActive = false;

    public float worldspaceYaw;

    // Start is called before the first frame update
    private void Start()
    {
        if (!init)
        {
            MapInit();
            exitsActive = true;
        }
    }

    public virtual void MapInit()
    {
        init = true;

        ResetVars();

        RenderSettings.ambientLight = ambientLight;

        /*
        if (playerHolder != null)
        {
            CreatePlayerEntities(playerHolder);
        }
        */

        if (textFile == null)
        {
            MainManager.MapID mid = MainManager.MapID.None;
            Enum.TryParse(mapName, out mid);
            textFile = MainManager.GetMapText(mid);
            if (textFile == null)
            {
                Debug.LogWarning("Map with no map text file: " + mid);
            }
            if (mid == MainManager.MapID.None && !mapName.Equals("None"))
            {
                Debug.LogError("Map name " + mapName + " could not be parsed!");
            }
        }

        //checking
        MainManager.WorldLocation wl = MainManager.WorldLocation.None;
        Enum.TryParse(worldLocation, out wl);
        if (wl == MainManager.WorldLocation.None && !worldLocation.Equals("None"))
        {
            Debug.LogError("World location " + worldLocation + " could not be parsed!");
        }
        if (worldLocation.Equals("None"))
        {
            Debug.LogWarning("Map with \"None\" location.");
        }
        MainManager.SkyboxID si;
        Enum.TryParse(skyboxID, out si);
        if (si == MainManager.SkyboxID.Invalid)
        {
            Debug.LogError("Skybox " + skyboxID + " could not be parsed!");
        }

        //Bad variable naming
        if (worldObjects == null || worldObjects.Count == 0)
        {
            WorldObject[] worldObjects = FindObjectsOfType<WorldObject>();
            this.worldObjects = new List<WorldObject>();

            for (int i = 0; i < worldObjects.Length; i++)
            {
                AddObject(worldObjects[i]);
                worldObjects[i].WorldInit();
            }
        }

        if (worldEntities == null || worldEntities.Count == 0)
        {
            WorldEntity[] worldEntities = FindObjectsOfType<WorldEntity>();
            this.worldEntities = new List<WorldEntity>();

            for (int i = 0; i < worldEntities.Length; i++)
            {
                AddEntity(worldEntities[i]);
                worldEntities[i].WorldInit();
            }
        }

        if (exits == null || exits.Count == 0)
        {
            MapExit[] exits = FindObjectsOfType<MapExit>();
            this.exits = new List<MapExit>();

            for (int i = 0; i < exits.Length; i++)
            {
                bool a = false;
                for (int j = 0; j < this.exits.Count; j++)
                {
                    if (exits[i].exitID < this.exits[j].exitID)
                    {
                        this.exits.Insert(j, exits[i]);
                        a = true;
                        break;
                    }
                }
                if (!a)
                {
                    this.exits.Add(exits[i]);
                }
            }
        }
    }

    public void ResetVars()
    {
        //flags = new bool[MainManager.MAP_FLAG_COUNT];
        //vars = new string[MainManager.MAP_VAR_COUNT];
        flags = new Dictionary<string, bool>();
        vars = new Dictionary<string, string>();
    }

    public virtual void OnExit(int exitID)
    {

    }

    public void Halt()
    {
        halted = true;
    }
    public void UnHalt()
    {
        halted = false;
    }
    public bool GetHalted()
    {
        return halted;
    }

    public void Disable()
    {
        halted = true;
        gameObject.SetActive(false);
    }
    public void Enable()
    {
        halted = false;
        gameObject.SetActive(true);
    }

    public virtual void SetDefaultCamera()
    {
        WorldCameraSettings wcs = new WorldCameraSettings();
        wcs.worldspaceYaw = worldspaceYaw;
        wcs.cameraEulerAngles = Vector3.zero;
        //wcs.directionVector = new Vector3(0, 1.2f, -3f);
        wcs.directionVector = new Vector3(0, 1f, -2.5f);
        wcs.movementHalflife = 0.05f;
        wcs.mode = WorldCamera.CameraMode.FollowPlayer;
        wcs.distance = 4.75f;

        MainManager.Instance.Camera.SetCameraSettings(wcs);

        //Make it so that map transitions move your camera to point in the direction you're going while first person
        //(Irrelevant to any other camera mode)
        if (MainManager.Instance.Cheat_FirstPersonCamera || wcs.mode == WorldCamera.CameraMode.FirstPerson_DoNotUse)
        {
            //MainManager.Instance.Camera.targetYaw = (180 / Mathf.PI) * Mathf.Atan2(WorldPlayer.Instance.scriptedInput.y, WorldPlayer.Instance.scriptedInput.x);

            float angle = (WorldPlayer.Instance.GetTrueFacingRotation() + 90);
            if (angle > 360)
            {
                angle -= 360;
            }
            if (angle < 0)
            {
                angle += 360;
            }
            MainManager.Instance.Camera.transform.eulerAngles = (angle) * Vector3.up;
            MainManager.Instance.Camera.targetYaw = angle;
        }
    }

    public void AddObject(WorldObject wo)
    {
        worldObjects.Add(wo);
    }

    public void AddEntity(WorldEntity we)
    {
        SiftEntities();
        for (int i = 0; i < worldEntities.Count; i++)
        {
            if (worldEntities[i].meid > we.meid)
            {
                worldEntities.Insert(i, we);
                return;
            }
        }
        worldEntities.Add(we);
    }
    public void RemoveEntity(WorldEntity we)
    {
        worldEntities.Remove(we);
    }
    public WorldEntity GetEntityByID(int id)
    {
        SiftEntities();
        for (int i = 0; i < worldEntities.Count; i++)
        {
            if (worldEntities[i].meid == id)
            {
                return worldEntities[i];
            }

            if (worldEntities[i].meid > id)
            {
                return null;
            }
        }
        return null;
    }
    public void SiftEntities()
    {
        for (int i = 0; i < worldEntities.Count; i++)
        {
            if (worldEntities[i] == null)
            {
                worldEntities.RemoveAt(i);
                i--;
                continue;
            }
        }
    }
    public void SiftObjects()
    {
        for (int i = 0; i < worldObjects.Count; i++)
        {
            if (worldObjects[i] == null)
            {
                worldObjects.RemoveAt(i);
                i--;
                continue;
            }
        }
    }

    public bool ExitsActive()
    {
        return exitsActive;
    }

    public void SetExitsActive(bool a)
    {
        exitsActive = a;
    }


    public IEnumerator DoEntrance(int entrance, Vector3 offset, float yawOffset)
    {
        //Entrance -1 is a special case (used by save file loading)
        if (entrance == -1)
        {
            WorldPlayer.Instance.transform.position = offset;
            WorldPlayer.Instance.FollowerWarpSetState();
            MainManager.Instance.Camera.SnapToTargets();
            yield return StartCoroutine(MainManager.Instance.UnfadeToBlack());
            exitsActive = true;

            yield return null;
        }


        //Debug.Log("Try to do entrance " + entrance);
        for (int i = 0; i < exits.Count; i++)
        {
            if (exits[i].exitID == entrance)
            {
                yield return StartCoroutine(exits[i].DoEntrance(offset, yawOffset));
                exitsActive = true;
                //Debug.Log("Done entering map");
                yield break;
            }
        }

        if (entrance != -1)
        {
            Debug.LogWarning("Entrance not found in this map: " + entrance);
        }

        //the black screen hides the camera movement but it's still a good idea to snap anyway
        MainManager.Instance.Camera.SnapToTargets();
        yield return StartCoroutine(MainManager.Instance.UnfadeToBlack());
        exitsActive = true;

        yield return null;
    }

    //If overkill level is low enough and you have overkill on -> instantly end battle
    public void StartBattleOrOverkill(int overkillLevel, IWorldBattleEntity entity, BattleStartArguments bsa = null)
    {
        if (MainManager.Instance.playerData.BadgeEquippedCount(Badge.BadgeType.Overkill) > 0)
        {
            if (MainManager.Instance.playerData.level - 2 + 2 * MainManager.Instance.playerData.BadgeEquippedCount(Badge.BadgeType.Overkill) >= overkillLevel)
            {
                HandleBattleOutcome(BattleHelper.BattleOutcome.Win);
                return;
            } 
        }

        //start normally
        StartBattle(entity, bsa);
    }
    public void StartBattle(IWorldBattleEntity entity, BattleStartArguments bsa = null)
    {
        if (battleHalt)
        {
            return;
        }
        battleHalt = true;
        worldBattleEntity = entity;
        WorldPlayer.Instance.DestroyPerpetualParticleObject();
        //new BattleStartArguments(-1)
        StartCoroutine(MainManager.Instance.EnterBattle(worldBattleEntity.GetEncounter(), bsa == null ? worldBattleEntity.GetBattleStartArguments() : bsa));
    }

    public void HandleBattleOutcome(BattleHelper.BattleOutcome outcome)
    {
        battleHalt = false;
        worldBattleEntity.HandleBattleOutcome(outcome);
    }

    //Dying = no game over (map may have special stuff to handle this scenario)
    public bool CanHandleBattleLoss()
    {
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if ((MainManager.Instance.mapHalted || MainManager.Instance.isPaused) && !halted)
        {
            Halt();
        } else if (!(MainManager.Instance.mapHalted || MainManager.Instance.isPaused) && halted)
        {
            UnHalt();
        }
    }

    //called right before the player warps back to their original spot
    //note: possible problem is if you can move stuff around that gets reset to be on top of the player's reset point
    public void HazardReset()
    {
        SiftObjects();
        for (int i = 0; i < worldObjects.Count; i++)
        {
            worldObjects[i].OnHazardReset();
        }
    }

    public MainManager.SkyboxID GetSkyboxID()
    {
        Enum.TryParse(skyboxID, out MainManager.SkyboxID si);
        return si;
    }

    //May be text shorthand or not
    //Ideally the map's tattle should be the first thing in the map's text file (so the tattle will end up being "l0" to reference local text file line 0)
    public string GetTattle()
    {
        if (textFile == null)
        {
            return worldLocation + " " + mapName + " - Default map tattle";
        } else
        {
            //Hardcoded
            return textFile[0][0];
        }
    }
}
