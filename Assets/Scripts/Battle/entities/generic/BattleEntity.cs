using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static BattleHelper;
using static UnityEngine.GraphicsBuffer;

//Data structure for enemy specific data (read from the data table)
//Note that enemies will have enemy specific code so not everything needs to be data driven
//Dynamic stuff can be set up on initialization so variable hp and stats don't have to be here
//Player characters don't use this because their data has to be more dynamic
public class BattleEntityData
{
    public BattleHelper.EntityID entityID;
    public int maxHP;
    public int level;
    public int bonusXP;
    public bool heavy;                                                              
    public float moneyMult;
    public Item.ItemType dropItemType;
    public int statusMaxTurns;
    public List<StatusTableEntry> statusTable;
    public List<DefenseTableEntry> defenseTable;
    public ulong entityProperties;
    public float width;
    public float height;
    public Vector3 offset;
    public Vector3 statusOffset;
    public Vector3 healthBarOffset;
    public Vector3 selectionOffset;
    public Vector3 stompOffset;
    public Vector3 kickOffset;
    public Vector3 hammerOffset;
    public Vector3 underOffset;
    public float entitySpeed;

    public static BattleEntityData ParseEntityData(string[] entry, BattleHelper.EntityID eid)
    {
        BattleEntityData ed = new BattleEntityData();

        if (entry.Length > 0)
        {
            BattleHelper.EntityID tempID = BattleHelper.EntityID.DebugEntity;   //default of 0 is safe

            if (Enum.TryParse(entry[0], true, out tempID))
            {
            }
            else
            {
                Debug.LogError("[Enemy Parsing] " + eid + " Can't parse entity id \"" + entry[0] + "\"");
            }

            if (eid != tempID)
            {
                Debug.LogError("[Enemy Parsing] Mismatch of entity ids (given = " + eid + ", parsed = " + entry[0] + ")");
            }
        }

        int tempint = 0;
        float tempfloat = 0;
        bool tempbool = false;
        if (entry.Length > 1)
        {
            //maxHP
            int.TryParse(entry[1], out tempint);
            ed.maxHP = tempint;
        }

        if (entry.Length > 2)
        {
            //level
            int.TryParse(entry[2], out tempint);
            ed.level = tempint;
        }

        if (entry.Length > 3)
        {
            //bonus xp
            int.TryParse(entry[3], out tempint);
            ed.bonusXP = tempint;
        }

        if (entry.Length > 4)
        {
            //heavy
            bool.TryParse(entry[4], out tempbool);
            ed.heavy = tempbool;
        }

        if (entry.Length > 5)
        {
            //money mult (*money is normally calculated with level)
            float.TryParse(entry[5], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out tempfloat);
            ed.moneyMult = tempfloat;
        }

        Item.ItemType tempType = Item.ItemType.None;
        if (entry.Length > 6)
        {
            //drop item (which item comes out of an encounter is determined by what dies last?)
            Enum.TryParse(entry[6], true, out tempType);
            ed.dropItemType = tempType;
        }

        if (entry.Length > 7)
        {
            //status max turns
            int.TryParse(entry[7], out tempint);
            ed.statusMaxTurns = tempint;
        }

        if (entry.Length > 8)
        {
            //status table
            List<StatusTableEntry> tempST = ParseStatusTableEntries(entry[8]);
            ed.statusTable = tempST;
        }

        if (entry.Length > 9)
        {
            //defense table
            List<DefenseTableEntry> tempDT = ParseDefenseTableEntries(entry[9]);
            ed.defenseTable = tempDT;
        }

        if (entry.Length > 10)
        {
            //entity properties
            ulong entityProperties = ParseEntityProperties(entry[10]);
            ed.entityProperties = entityProperties;
        }

        if (entry.Length > 11)
        {
            //width
            float.TryParse(entry[11], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out tempfloat);
            ed.width = tempfloat;
        }

        if (entry.Length > 12)
        {
            //height
            float.TryParse(entry[12], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out tempfloat);
            ed.height = tempfloat;
        }

        Vector3 tempvector = Vector3.zero;
        if (entry.Length > 13)
        {
            //offset
            tempvector = ParseVector3(entry[13]);
            //Debug.Log(eid + " " + tempvector);
            ed.offset = tempvector;
        }

        if (entry.Length > 14)
        {
            //status offset
            tempvector = ParseVector3(entry[14]);
            ed.statusOffset = tempvector;
        }

        if (entry.Length > 15)
        {
            //health bar offset
            tempvector = ParseVector3(entry[15]);
            ed.healthBarOffset = tempvector;
        }

        if (entry.Length > 16)
        {
            //selection offset
            tempvector = ParseVector3(entry[16]);
            ed.selectionOffset = tempvector;
        }

        if (entry.Length > 17)
        {
            //stomp offset
            tempvector = ParseVector3(entry[17]);
            ed.stompOffset = tempvector;
        }

        if (entry.Length > 18)
        {
            //kick offset
            tempvector = ParseVector3(entry[18]);
            ed.kickOffset = tempvector;
        }

        if (entry.Length > 19)
        {
            //hammer offset
            tempvector = ParseVector3(entry[19]);
            ed.hammerOffset = tempvector;
        }

        if (entry.Length > 20)
        {
            //under offset
            tempvector = ParseVector3(entry[20]);
            ed.underOffset = tempvector;
        }


        if (entry.Length > 21)
        {
            //entity speed
            float.TryParse(entry[21], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out tempfloat);
            ed.entitySpeed = tempfloat;
        }


        return ed;
    }

    //format: effect:sus/modifier|effect2:sus/modifier
    public static List<StatusTableEntry> ParseStatusTableEntries(string entry)
    {
        string[] entries = entry.Split("|");
        List<StatusTableEntry> output = new List<StatusTableEntry>();

        string tempProp = "";
        string tempPropArgs = "";
        string tempPropSus = ""; //amogus
        string tempPropModifier = "";

        Effect.EffectType tempPropEffect = Effect.EffectType.Default;
        float tempPropSus2 = 0;
        float tempPropModifier2 = 0;

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Length == 0)
            {
                continue;
            }

            tempProp = entries[i].Split(":")[0];

            if (entries[i].Split(":").Length < 2)
            {
                Debug.LogError("Malformed status table entry (line in question: " + entry[i] + ")");
                continue;
            }
            tempPropArgs = entries[i].Split(":")[1];
            tempPropSus = tempPropArgs.Split("/")[0];
            tempPropModifier = tempPropArgs.Split("/")[1];

            if (Enum.TryParse(tempProp, true, out tempPropEffect))
            {
            }
            else
            {
                Debug.LogError("[Enemy Parsing] " + i + " Can't parse effect type \"" + tempProp + "\"");
                continue;
            }

            float.TryParse(tempPropSus, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out tempPropSus2);
            float.TryParse(tempPropModifier, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out tempPropModifier2);

            output.Add(new StatusTableEntry(tempPropEffect, tempPropSus2, tempPropModifier2));
        }

        return output;
    }

    //format: type:def|type2:def2
    //def == N  => null (immunity constant)
    //def == A  => absorb (null + 1)
    public static List<DefenseTableEntry> ParseDefenseTableEntries(string entry)
    {
        string[] entries = entry.Split("|");
        List<DefenseTableEntry> output = new List<DefenseTableEntry>();

        string tempProp = "";
        string tempNumber = "";

        BattleHelper.DamageType tempType = BattleHelper.DamageType.Default;
        int tempDef = 0;

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Length == 0)
            {
                continue;
            }

            tempProp = entries[i].Split(":")[0];

            if (entries[i].Split(":").Length < 2)
            {
                Debug.LogError("Malformed defense table entry (line in question: " + entry[i] + ")");
                continue;
            }
            tempNumber = entries[i].Split(":")[1];

            if (Enum.TryParse(tempProp, true, out tempType))
            {
            }
            else
            {
                Debug.LogError("[Enemy Parsing] " + i + " Can't parse damage type \"" + tempProp + "\"");
                continue;
            }

            bool specialCase = false;

            if (tempNumber.Equals("N"))
            {
                specialCase = true;
                tempDef = DefenseTableEntry.IMMUNITY_CONSTANT;
            }

            if (tempNumber.Equals("A"))
            {
                specialCase = true;
                tempDef = DefenseTableEntry.IMMUNITY_CONSTANT + 1;
            }

            if (!specialCase)
            {
                int.TryParse(tempNumber, out tempDef);
            }

            output.Add(new DefenseTableEntry(tempType, tempDef));
        }

        return output;
    }

    //format: x|y|z
    public static Vector3 ParseVector3(string entry)
    {
        string[] entries = entry.Split("|");
        Vector3 output = Vector3.zero;

        float x = 0;
        float y = 0;
        float z = 0;

        if (entries.Length > 3)
        {
            Debug.LogWarning("[Enemy Parsing] Vector3 is too long: (" + entry + "), superfluous numbers will be ignored");
        }

        //Note: zero length string will end up parsing to 0,0,0
        if (entries.Length > 0)
        {
            float.TryParse(entries[0], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x);
        }

        if (entries.Length > 1)
        {
            float.TryParse(entries[1], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y);
        }

        if (entries.Length > 2)
        {
            float.TryParse(entries[2], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out z);
        }

        output = Vector3.right * x + Vector3.up * y + Vector3.forward * z;
        return output;
    }

    //format prop1|prop2...
    public static ulong ParseEntityProperties(string entry)
    {
        BattleHelper.EntityProperties output = 0;

        string[] tempEntityProperties = entry.Split("|");

        BattleHelper.EntityProperties d;
        for (int m = 0; m < tempEntityProperties.Length; m++)
        {
            if (tempEntityProperties[m].Length == 0)
            {
                continue;
            }
            if (Enum.TryParse(tempEntityProperties[m], true, out d))
            {
                output |= d;
            }
            else
            {
                Debug.LogError("[Enemy Parsing] Can't parse entity property \"" + tempEntityProperties[m] + "\"");
            }
        }

        return (ulong)output;
    }

    public static BattleEntityData GetBattleEntityData(BattleHelper.EntityID eid)
    {
        int index = (int)eid;

        if (index < 0)
        {
            return null;
        }

        if (index > MainManager.Instance.battleEntityTable.Length - 1)
        {
            return null;
        }

        return MainManager.Instance.battleEntityTable[index];
    }
}

//supplemental data for the bestiary
//throws a warning if the data structure does not match once the initialize script runs
public class BattleEntityMovesetData
{
    public BattleHelper.EntityID entity;
    public List<EnemyMove.MoveIndex> moveset;

    public BattleEntityMovesetData(BattleHelper.EntityID p_entity, List<EnemyMove.MoveIndex> p_moveset)
    {
        entity = p_entity;
        moveset = p_moveset;
    }

    public static BattleEntityMovesetData Parse(string[] entry, BattleHelper.EntityID entity)
    {
        BattleEntityMovesetData output = new BattleEntityMovesetData(BattleHelper.EntityID.DebugEntity, null);

        if (Enum.TryParse(entry[0], out output.entity))
        {
            if (output.entity != entity)
            {
                Debug.LogWarning("[BattleEntityMovesetData Parsing] Entity mismatch: " + entry[0] + " vs expected " + entity);
            }
        } else
        {
            Debug.LogError("[BattleEntityMovesetData Parsing] Entity could not be parsed " + entry[0]);
        }

        string[] subEntry = entry[1].Split("|");

        output.moveset = new List<EnemyMove.MoveIndex>();
        EnemyMove.MoveIndex tempMove;
        if (subEntry.Length != 1 || !subEntry[0].Equals(""))
        {
            for (int i = 0; i < subEntry.Length; i++)
            {
                tempMove = EnemyMove.MoveIndex.Unknown;
                if (Enum.TryParse(subEntry[i], out tempMove))
                {
                    output.moveset.Add(tempMove);
                }
                else
                {
                    Debug.LogError("[BattleEntityMovesetData Parsing] " + output.entity + " Move could not be parsed " + subEntry[i]);
                }
            }
        }       

        return output;
    }

    public static BattleEntityMovesetData GetBattleEntityMovesetData(BattleHelper.EntityID eid)
    {
        int index = (int)eid;

        if (index < 0)
        {
            return null;
        }

        if (index > MainManager.Instance.battleEntityMovesetTable.Length - 1)
        {
            return null;
        }

        return MainManager.Instance.battleEntityMovesetTable[index];
    }

    public string GetMovesetBestiaryString()
    {
        //Each page is 14? lines
        //Each line is 42? chars

        //so to be safe I'll only allow 13 lines per page
        string output = "Moveset Data<line>";

        bool hard = false;

        if (MainManager.Instance.playerData.BadgeEquipped(Badge.BadgeType.SuperCurse))
        {
            hard = true;
        }
        if (MainManager.Instance.playerData.BadgeEquipped(Badge.BadgeType.UltraCurse))
        {
            hard = true;
        }
        if (MainManager.Instance.playerData.BadgeEquipped(Badge.BadgeType.MegaCurse))
        {
            hard = true;
        }

        //0 index?
        int curLine = 1;
        for (int i = 0; i < moveset.Count; i++)
        {
            string nextName = "<color,blue>" + EnemyMove.GetNameWithIndex(moveset[i]) + "</color>";
            string nextDesc = "";
            if (hard)
            {
                nextDesc = EnemyMove.GetHardDescriptionWithIndex(moveset[i]);
            }
            else
            {
                nextDesc = EnemyMove.GetDescriptionWithIndex(moveset[i]);
            }
            //2 base needed because name and newline
            int neededLines = 2 + Mathf.CeilToInt(FormattedString.StripTags(nextDesc).Length / 42f);

            //Debug.Log(nextName + " " + curLine + " " + nextDesc + " " + nextDesc.Length + " " + neededLines);

            if (curLine + neededLines > 11)
            {
                output += "<next>";
                curLine = -1;   //since the newline is not added (but is accounted for in neededLines), need to start higher up
            } else
            {
                output += "<line>";
            }

            output += nextName;
            output += "<line>";
            output += nextDesc;
            output += "<line>";

            curLine += neededLines;
        }

        return output;
    }
}

public class BestiaryOrderEntry
{
    public EntityID eid;
    public int index;
    public int subindex;    //0 = first one, 1 = "b", 2 = "c"...

    public BestiaryOrderEntry(EntityID p_eid, int p_index, int p_subindex = 0)
    {
        eid = p_eid;
        index = p_index;
        subindex = p_subindex;
    }

    //note: not the number that appears in the bestiary, this is the actual, 0 indexed value used to index into the bestiary order array and the bestiary text array
    //(Note that getting this index will give you a way to get the BestiaryOrderEntry)
    public static int GetBestiaryOrderIndex(EntityID eid)
    {
        for (int i = 0; i < MainManager.Instance.bestiaryOrder.Length; i++)
        {
            if (MainManager.Instance.bestiaryOrder[i].eid == eid)
            {
                return i;
            }
        }

        return -1;
    }

    public static BestiaryOrderEntry GetBestiaryOrderEntry(EntityID eid)
    {
        //slightly roundabout way to do this but it isn't that bad
        return MainManager.Instance.bestiaryOrder[GetBestiaryOrderIndex(eid)];
    }

    public static string GetBestiaryOrderNumberString(EntityID eid)
    {
        BestiaryOrderEntry boe = GetBestiaryOrderEntry(eid);
        string bestiaryOrderNumber = boe.index.ToString();
        if (boe.subindex != 0)
        {
            bestiaryOrderNumber += (char)('a' + boe.subindex);
        }

        return bestiaryOrderNumber;
    }
}

public class BattleEntity : MonoBehaviour, ITextSpeaker
{
    //public static GameObject baseObject;

    //Enemy stats (mostly)
    //specific entity values (Almost all of these are in the enemy data file)
    public BattleHelper.EntityID entityID;                                                       //what kind of entity is this?
    public int hp;
    public int maxHP = 5;
    //public int ep;                                                                  //unused for most enemies
    //public int maxEP;                                                               //unused for most enemies
    public int stamina;                                                               //unused for most enemies
    public int agility;                                                               //unused for most enemies

    public int level;                                                               //determines which level you stop getting XP from this enemy (determines XP gain)
    public int bonusLevel;                                                             //bonus XP (so I can make low level big xp givers without letting you grind to max immediately)
    public float moneyMult;                                                         //coin drops are proportional with level
    public Item.ItemType dropItemType;                                              //item drop after battle (if applicable)
    public bool heavy;                                                              //can certain things launch them up?

    public List<StatusTableEntry> statusTable = new List<StatusTableEntry>();       //determines how much enemy resists statuses
    public int statusMaxTurns = 0;                                                     //Finite turns you can use statuses on an enemy before it becomes immune (goes down when you use statuses)
    public int baseStatusMaxTurns = 0;                                              //note: this is the base, the above one goes down as you apply status turns


    public List<DefenseTableEntry> defenseTable = new List<DefenseTableEntry>();    //determines defense to damage types

    public float attackMultiplier;
    public bool applyCurseAttack;

    //battle variables
    public int posId;                                                               //players have negative IDs (-1, -2), enemies have positive IDs (starting from 0)
                                                                                    //enemies are enumerated lowest to highest, (1,2,3)
    public Vector3 homePos;                                                         //where entity should end up after each turn
    public Vector3 offset = Vector3.up * 0.5f;                                      //Each entity has a true object and a subobject, this controls subobject offset
    public float height = 1f;                                                     //Future thing to do: a debug method to draw the bounding box
    public float width = 1f;

    //Important enemy specific values
    public GameObject subObject;                                                    //affected by offset (where visuals should go)
    public GameObject dropShadow;
    public bool noShadow;                                                           //Block shadow creation
        //AnimationController is glued to the Subobject (Implementation note: In the future I will make it so that all the animation controllers are prefabs and they get put in the subobject slot here)
    public float entitySpeed = 8;                                                       //how fast entity should go (can be overridden)

    //Extra visuals (where status icons, health bars, and selection arrows end up)
    public Vector3 statusOffset = Vector3.up * 1f + Vector3.right * 0.6f;
    public Vector3 healthBarOffset = Vector3.down * 0.15f;
    public Vector3 selectionOffset = Vector3.up * 1.5f;
    //public Vector3 damageOffset;
    private List<GameObject> statusIcons = new List<GameObject>();
    private GameObject hpbar;

    private GameObject effectParticle;

    //a few other offset things (but these are scaled based on height / width, such that -0.5, 1, 0 is the top left corner always for example)
    public Vector3 stompOffset = new Vector3(0,1,0);
    public Vector3 kickOffset = new Vector3(-0.5f, 0.5f, 0);
    public Vector3 hammerOffset = new Vector3(-0.5f, 0, 0);
    public Vector3 underOffset = Vector3.zero;

    //debug print damage
    public const bool DAMAGE_DEBUG = false;

    public int sameFrameHealEffects;


    public bool inCombo //Set by ComboHit, reset by Hit (if it stays true after an attack then that is a sign of an incorrectly coded attack)
    {
        get; private set;
    }

    public BattleEntity curTarget
    {
        get
        {
            return BattleControl.Instance.GetEntityByID(targetID);
        }
        set
        {
            targetID = BattleControl.Instance.FindEntityID(value);
        }
    }                                                   //what is the entity targetting? (Target is by ID, so changing ids WILL change target!)
    public int targetID = int.MinValue;                                                            //what ID is the entity targetting (more stable)
    //public TargetArea targetArea;

    public List<Effect> effects = new List<Effect>();                              //current status effects
    public List<Effect> bufferedEffects = new List<Effect>();                      //list of statuses to apply in post move phase (after ticking down other statuses)

    public List<int> contactImmunityList = new List<int>();                         //only 1 contact hazard per enemy can activate on a given attacker (per turn)

    //public string[] variables;                                                    //entity specific data (unused because entity scripts can do this)
    public ulong entityProperties;                                                   //entity properties
    public bool idleActive = false;                                                 //should idle be active? (false causes idle to stop)
    public bool idleRunning = false;                                                //is idle script running?


    public AnimationController ac;
    public bool isSpeaking;
    public bool flipDefault;                                                        //should flip be default (True for all enemies)

    //public Item? heldItem;                                                        //mostly unused on players
    public bool hasIdle = false;                                                    //if true, idle method runs whenever it isn't doing anything else
    public int damageEventsThisTurn = 0;                                               //counts damage events in a turn
    public int absorbDamageEvents = 0;
    public bool hitThisTurn = false;                                                //did you get hurt this turn? (Flag checked and reset at the start of the "round", before player can move)
    public bool hitLastTurn = false;                                                //did you get hurt last turn? (Starts false at the start of battle because that makes more sense)
    public bool attackThisTurn = false;                                             //did you attack on your turn?
    public bool attackLastTurn = false;                                             //did you attack last turn?
    public int attackHitCount = 0;                                               //counts # of attack hits in a turn (as in you deal damage) (includes counters?)
    public int cumulativeAttackHitCount = 0;                                        //Attack hits in a battle
    public int chargedAttackCount = 0;                                              //Number of attacks that used charge (used for charge removal)
    public BattleEntity lastAttacker;                                               //which entity attacked this entity last? (enviro damage is targetless and does not set this) (Note: many counters use this to determine what attacked them)
    public int lastDamageTaken;                                                     //damage from last damage source (not statuses)
    public int lastDamageTakenInput;                                                //damage taken but before the defense calculation
    public DamageType lastDamageType;
    public bool bufferRemoveCharge = false;                                         //next time you try to remove charge, if this is true, set this to false instead of removing (but also gets reset in postmove phase)

    public bool statusCatalyst;
    public bool wakeUp;

    public int counterFlareDamage;                                                //Damage to deal by Counter Flare (Set before Postmove is run)
    public int arcDischargeDamage;                                              //Damage to deal by Arc Discharge
    public int splotchDamage;                                                //Damage to deal by Splotch
    public int magmaDamage;
    public int damageTakenThisTurn;                                        //Non-status damage taken since last PostMove call (Used to calculate Astral Wall stuff)

    //
    public int counterFlareTrackedDamage;
    public int arcDischargeTrackedDamage;
    public int splotchTrackedDamage;
    //note: no special handling for magma since that isn't something that just turns on mid turn (while the other effects might be applied some turn)

    public int damageTakenLastTurn;
    public bool alive;

    public bool perfectKill;    //was this entity perfect killed? Set to true in the take damage things

    public List<BattleHelper.Event> eventQueue = new List<BattleHelper.Event>();                                     //what events this entity has to do (When inEvent is false, start the next one on the list)
    public bool inEvent;
    public bool immediateInEvent    //inEvent but also works even if this is the same frame as an event being queued
    {
        get
        {
            return inEvent || eventQueue.Count > 0;
        }
    }

    //public bool hasInterruptEvent = false;                                      //does this entity have an event that interrupts the normal movement? (if I have a lot of them I'll have to use the move pattern)

    public List<Move> moveset = new List<Move>();                       //what moves can this entity do?
    public Move currMove;                                                     //what are you doing this turn?
    //public List<Move> extraMoves = new List<Move>();
    protected Coroutine currExec;                                                     //move coroutine being executed (only for starting and stopping properly)
    public bool moveExecuting = false;                                                     //set to true when time to move, set to false when everything is done
    public bool moveActive = false;                                                 //More commonly used than moveExecuting. May be set to false by the move itself with YieldMove()
    //future note: yield turn stuff should be avoided to avoid problems with scheduling of out of turn events

    //counts actions in your turn (if you have bonus turns this will be the action number but 0 indexed)
    public int actionCounter = 0;

    //These are the main methods that you override in enemies
    //may also want to override DoEvent for special enemies

    //replace this with something that reads from file
    public virtual string GetName()
    {
        return GetNameStatic(entityID);
    }
    public static string GetNameStatic(BattleHelper.EntityID entityID)
    {
        //entityID
        if ((int)entityID < 0)
        {
            return entityID.ToString();
        }
        string output = MainManager.Instance.enemyText[(int)entityID + 1][1];
        return output;
    }

    public static Sprite GetBestiarySprite(BattleHelper.EntityID entityID)
    {
        //TODO fix this
        return null;
    }

    public static string GetBestiarySideText(BattleHelper.EntityID entityID)
    {
        //will use entitydata
        BattleEntityData bed = BattleEntityData.GetBattleEntityData(entityID);

        if (bed == null)
        {
            return "???";
        }

        string output = "";
        if ((bed.entityProperties & (ulong)BattleHelper.EntityProperties.HideHP) != 0)
        {
            output = "<align,right>Base Max HP: <color,red>?</color>";
        }
        else
        {
            output = "<align,right>Base Max HP: " + bed.maxHP;           
        }
        output += "<line>Level: " + bed.level + " (+" + bed.bonusXP + ")";
        output += "<line>Coin Bonus: " + MainManager.PrecisionTruncate(bed.moneyMult);

        int normalDefense = 0;
        bool normalDefenseSet = false;
        for (int i = 0; i < bed.defenseTable.Count; i++)
        {
            if (bed.defenseTable[i].type == DamageType.Normal)
            {
                normalDefense = bed.defenseTable[i].amount;
                normalDefenseSet = true;
            }
        }
        if (!normalDefenseSet)
        {
            for (int i = 0; i < bed.defenseTable.Count; i++)
            {
                if (bed.defenseTable[i].type == DamageType.Default)
                {
                    normalDefense = bed.defenseTable[i].amount;
                }
            }
        }

        if (normalDefense > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            output += "<line>Defense: <color,red>Absorb</color>";
        }
        else if (normalDefense == DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            output += "<line>Defense: <color,red>Infinite</color>";
        }
        else
        {
            output += "<line>Defense: " + normalDefense;
        }
        output += "<line>Max Status Turns: " + bed.statusMaxTurns;

        return output;
    }

    public static string GetBestiaryEntry(BattleHelper.EntityID entityID)
    {
        string output = "";

        string bioString = MainManager.Instance.enemyText[(int)entityID + 1][2];
        bioString = FormattedString.ReplaceTagWith(bioString, TagEntry.TextTag.Next, "<line><line>");

        string keruString = "Keru's Notes<next>" + MainManager.Instance.enemyText[(int)entityID + 1][3];
        keruString = FormattedString.ReplaceTagWith(keruString, TagEntry.TextTag.Next, "<line><line>");

        string wilexString = "Wilex's Notes<next>" + MainManager.Instance.enemyText[(int)entityID + 1][4];
        wilexString = FormattedString.ReplaceTagWith(wilexString, TagEntry.TextTag.Next, "<line><line>");

        string lunaString = "Luna's Notes<next>" + MainManager.Instance.enemyText[(int)entityID + 1][5];
        lunaString = FormattedString.ReplaceTagWith(lunaString, TagEntry.TextTag.Next, "<line><line>");

        output = bioString + "<next>" + keruString + "<next>" + wilexString + "<next>" + lunaString;

        //
        BattleEntityMovesetData bemd = BattleEntityMovesetData.GetBattleEntityMovesetData(entityID);
        output += "<next>" + bemd.GetMovesetBestiaryString();

        return output;
    }
    public static bool GetBestiaryFlag(BattleHelper.EntityID entityID)
    {
        return MainManager.Instance.GetBestiaryFlag(entityID);
    }
    public static string GetTattleStatic(BattleHelper.EntityID tattlerID, BattleHelper.EntityID entityID)
    {
        int index = 3;
        switch (tattlerID)
        {
            case BattleHelper.EntityID.Keru:
                index = 3;
                break;
            case BattleHelper.EntityID.Wilex:
                index = 4;
                break;
            case BattleHelper.EntityID.Luna:
                index = 5;
                break;
        }
        string output = MainManager.Instance.enemyText[(int)entityID + 1][index];
        return output;
        //return GetName() + " (" + hp + "/" + maxHP + ") hp.";
    }
    public virtual string GetTattle(BattleHelper.EntityID tattlerID)
    {
        int index = 3;
        switch (tattlerID)
        {
            case BattleHelper.EntityID.Keru:
                index = 3;
                break;
            case BattleHelper.EntityID.Wilex:
                index = 4;
                break;
            case BattleHelper.EntityID.Luna:
                index = 5;
                break;
        }
        string output = MainManager.Instance.enemyText[(int)entityID + 1][index];
        return output;
        //return GetName() + " (" + hp + "/" + maxHP + ") hp.";
    }

    public override string ToString()
    {
        return GetName();
    }

    //called first thing in initialize
    public void SetStatsWithBEData()
    {
        BattleEntityData bed = BattleEntityData.GetBattleEntityData(entityID);

        if (bed == null)
        {
            return;
        }

        maxHP = BattleControl.Instance.CurseMultiply(bed.maxHP);        
        if (bed.maxHP > 0 && maxHP < 1)
        {
            maxHP = 1;
        }
        level = bed.level;
        bonusLevel = bed.bonusXP;
        heavy = bed.heavy;
        moneyMult = bed.moneyMult;
        dropItemType = bed.dropItemType;
        baseStatusMaxTurns = bed.statusMaxTurns;
        statusTable = bed.statusTable;
        defenseTable = bed.defenseTable;
        entityProperties = bed.entityProperties;
        width = bed.width;
        height = bed.height;
        offset = bed.offset;
        statusOffset = bed.statusOffset;
        healthBarOffset = bed.healthBarOffset;
        selectionOffset = bed.selectionOffset;
        stompOffset = bed.stompOffset;
        kickOffset = bed.kickOffset;
        hammerOffset = bed.hammerOffset;
        underOffset = bed.underOffset;
        entitySpeed = bed.entitySpeed;

        //Future thing
        /*
        if (GetEntityProperty(BattleHelper.EntityProperties.Limiter))
        {
            int cut = Mathf.CeilToInt(bed.maxHP / 2f);
            maxHP = cut + BattleControl.Instance.CurseMultiply(cut);
        }
        */
    }

    //Initialize: called immediately after adding the default script to a base object
    public virtual void Initialize()
    {
        if (flipDefault && ac != null)
        {
            ac.SendAnimationData("xflip");
        }

        if (!GetEntityProperty(BattleHelper.EntityProperties.SuppressMovesetWarning))
        {
            BattleEntityMovesetData bemd = BattleEntityMovesetData.GetBattleEntityMovesetData(entityID);

            EnemyMove em = null;
            for (int i = 0; i < bemd.moveset.Count; i++)
            {
                if (i > moveset.Count - 1)
                {
                    em = null;
                } else
                {
                    em = moveset[i] as EnemyMove;
                }
                if (em != null)
                {
                    if (em.GetMoveIndex() != bemd.moveset[i])
                    {
                        Debug.LogWarning("[BattleEntity] Moveset data mismatch for " + GetName() + ": " + em.GetMoveIndex() + " vs data table " + bemd.moveset[i]);
                    }
                } else
                {
                    if (i <= moveset.Count - 1)
                    {
                        Debug.LogWarning("[BattleEntity] Moveset data mismatch for " + GetName() + ": Null(Not EnemyMove?) vs data table " + bemd.moveset[i]);
                    }
                    else
                    {
                        Debug.LogWarning("[BattleEntity] Moveset data mismatch for " + GetName() + ": Null vs data table " + bemd.moveset[i]);
                    }
                }
            }
            if (bemd.moveset.Count != moveset.Count)
            {
                Debug.LogWarning("[BattleEntity] Moveset data mismatch: Moveset and data table for " + GetName() + " are mismatched in length (" + moveset.Count + " vs " + bemd.moveset.Count + ")");
            }
        }

        SetStatsWithBEData();

        //attackMultiplier = BattleControl.Instance.GetCurseMultiplier();
        attackMultiplier = 1;
        applyCurseAttack = true;
        statusMaxTurns = baseStatusMaxTurns;
        hp = maxHP;

        stamina = 0;
        agility = 0;

        damageEventsThisTurn = 0;

        damageTakenThisTurn = 0;
        counterFlareDamage = 0;
        counterFlareTrackedDamage = 0;
        arcDischargeDamage = 0;
        arcDischargeTrackedDamage = 0;
        splotchDamage = 0;
        splotchTrackedDamage = 0;
        damageTakenLastTurn = 0;

        //debug status testing
        if (entityID == BattleHelper.EntityID.DebugEntity)
        {
            ReceiveEffectForce(new Effect(Effect.EffectType.Miracle, 1, Effect.INFINITE_DURATION));
        }

        if (entityID == BattleHelper.EntityID.DebugEntity)
        {
            DebugJump d = gameObject.AddComponent<DebugJump>(); //my current system requires adding all possible move scripts to an enemy
            DebugFly de = gameObject.AddComponent<DebugFly>();
            DebugCounter df = gameObject.AddComponent<DebugCounter>();
            DebugRush dr = gameObject.AddComponent<DebugRush>();
            moveset.Add(d);
            moveset.Add(de);
            moveset.Add(df);
            moveset.Add(dr);

            currMove = d;
        }

        //Dropshadow is parallel to subobject (because it should not inherit subobject's rotation)
        if (!noShadow && dropShadow == null)
        {
            dropShadow = Instantiate(Resources.Load<GameObject>("Overworld/Other/DropShadow"), transform);
        }
    }

    public virtual void PostPositionInitialize()
    {
        //Default: 0 is ground level so it auto sets the grounded flag (you can unset it later)
        if (homePos.y == 0)
        {
            SetEntityProperty(BattleHelper.EntityProperties.Grounded, true);
        }
    }

    //Encounter data level variables
    public virtual void SetEncounterVariables(string variable)   //object would be more versatile but the way you would get these variables is probably from some string table 
    {

    }
    public virtual void SetVariables(string variable)   //object would be more versatile but the way you would get these variables is probably from some string table 
    {

    }
    
    //first thing in battle (then pre battle coroutines in order)
    public virtual void PreBattle()
    {

    }

    public virtual bool HasPreBattleCoroutine()
    {
        return false;
    }
    public virtual IEnumerator PreBattleCoroutine()
    {
        yield return null;
    }

    //first strike phase
    public virtual IEnumerator FirstStrike(BattleStartArguments.FirstStrikeMove move = BattleStartArguments.FirstStrikeMove.Default)
    {
        Debug.Log("Default first strike: " + GetName() + " (move) " + moveset[0].GetName());
        currMove = moveset[0];

        //targets front player character
        //note that they must be alive at the start of battle due to how things are set up
        List<BattleEntity> elist = BattleControl.Instance.GetEntitiesSorted(this, currMove.GetTargetArea(this));
        curTarget = elist.Count > 0 ? elist[0] : null;
        yield return StartCoroutine(ExecuteMoveCoroutine());
    }

    //called before turn, before ChooseMove()
    public virtual IEnumerator PreMove() 
    {
        if (!CanMove())
        {
            currMove = null;
        }

        //if (attackDamageCount > 0)
        //{
        //    attackThisTurn = true;
        //}

        attackThisTurn = false;

        attackHitCount = 0;
        chargedAttackCount = 0;
        //damageEventsCount = 0;
        absorbDamageEvents = 0;
        //Debug.Log(name + " reset");

        if (currMove != null)
        {
            currMove.PreMove(this);
        }

        if (idleRunning)
        {
            StopCoroutine("Idle");
        }

        yield return null;
    }

    //called before moves execute
    public virtual void ChooseMove() //somehow determine what move you do
    {
        //reset the list
        while (contactImmunityList.Count > 0)
        {
            contactImmunityList.RemoveAt(0);
        }

        actionCounter++;

        if (!CanMove())
        {
            currMove = null;
            return;
        }

        ChooseMoveInternal();
        
        //suspicious code
        if (currMove != null)
        {
            currMove.PreMove(this);
        }        
    }

    public virtual void ChooseMoveInternal()
    {
        //Debug
        //RandomGenerator r = new RandomGenerator();

        //Debug.Log(1 + BattleControl.Instance.turnCount + " " + moveset.Count);
        //debug / default
        //do it in order
        if (moveset.Count == 0)
        {
            currMove = null;
        } else if (moveset.Count == 1)
        {
            currMove = moveset[0];
        } else
        {
            //note: posId >= 0 for enemies
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % moveset.Count];
        }

        BasicTargetChooser();
    }

    public virtual void BasicTargetChooser()
    {
        if (currMove == null)
        {
            curTarget = null;
        }
        else
        {
            List<BattleEntity> bl = BattleControl.Instance.GetEntities(this, currMove.GetTargetArea(this));

            TargetStrategy strategy = new TargetStrategy(TargetStrategy.TargetStrategyType.FrontMost);

            bl.Sort((a, b) => strategy.selectorFunction(a, b));

            //Not a normal attack because there are only 2 player entities
            if (bl.Count > 2)
            {
                curTarget = bl[BattleControl.Instance.GetPsuedoRandom(bl.Count, posId)];
            } else if (bl.Count > 1)
            {
                //Debug.Log(bl[0]);
                curTarget = bl[BattleControl.Instance.GetTargetFromDoublePool() ? 0 : 1];
            }
            else if (bl.Count > 0)
            {
                curTarget = bl[0];
            }
            else
            {
                //note: real enemies should probably have something to actually handle this
                //(i.e. choose a special move or just do nothing)
                Debug.Log("No targets found");
                curTarget = null;
            }

            if (GetBerserkTarget() != null)
            {
                if (bl.Contains(curTarget))
                {
                    curTarget = GetBerserkTarget();
                }
            }
        }
    }
    public virtual void BasicOffsetTargetChooser(int offsetA, int offsetB)
    {
        if (currMove == null)
        {
            curTarget = null;
        }
        else
        {
            List<BattleEntity> bl = BattleControl.Instance.GetEntities(this, currMove.GetTargetArea(this));

            TargetStrategy strategy = new TargetStrategy(TargetStrategy.TargetStrategyType.FrontMost);

            bl.Sort((a, b) => strategy.selectorFunction(a, b));

            //Not a normal attack because there are only 2 player entities
            if (bl.Count > 2)
            {
                curTarget = bl[BattleControl.Instance.GetPsuedoRandom(bl.Count, posId + offsetA + offsetB)];
            }
            else if (bl.Count > 1)
            {
                //Debug.Log(bl[0]);
                curTarget = bl[BattleControl.Instance.GetTargetFromDoublePool(offsetA, offsetB) ? 0 : 1];
            }
            else if (bl.Count > 0)
            {
                //1 target?
                curTarget = bl[0];
            }
            else
            {
                //note: real enemies should probably have something to actually handle this
                //(i.e. choose a special move or just do nothing)
                Debug.Log("No targets found");
                curTarget = null;
            }

            if (GetBerserkTarget() != null)
            {
                if (bl.Contains(curTarget))
                {
                    curTarget = GetBerserkTarget();
                }
            }
        }
    }

    //only first target
    public virtual void SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType tst)
    {
        if (currMove == null)
        {
            curTarget = null;
        }
        else
        {
            List<BattleEntity> bl = BattleControl.Instance.GetEntities(this, currMove.GetTargetArea(this));

            TargetStrategy strategy = new TargetStrategy(tst);

            bl.Sort((a, b) => strategy.selectorFunction(a, b));

            //No decisionmaking
            //Only choose the first one
            if (bl.Count > 0)
            {
                curTarget = bl[0];
            }
            else
            {
                //note: real enemies should probably have something to actually handle this
                //(i.e. choose a special move or just do nothing)
                Debug.Log("No targets found");
                curTarget = null;
            }

            if (GetBerserkTarget() != null)
            {
                if (bl.Contains(curTarget))
                {
                    curTarget = GetBerserkTarget();
                }
            }
        }
    }

    public virtual void SpecialTargetChooser(TargetStrategy.TargetStrategyType tst)
    {
        if (currMove == null)
        {
            curTarget = null;
        }
        else
        {
            List<BattleEntity> bl = BattleControl.Instance.GetEntities(this, currMove.GetTargetArea(this));

            TargetStrategy strategy = new TargetStrategy(tst);

            bl.Sort((a, b) => strategy.selectorFunction(a, b));

            //Not a normal attack because there are only 2 player entities
            if (bl.Count > 2)
            {
                curTarget = bl[BattleControl.Instance.GetPsuedoRandom(bl.Count, posId)];
            }
            else if (bl.Count > 1)
            {
                //Debug.Log(bl[0]);
                curTarget = bl[BattleControl.Instance.GetTargetFromDoublePool() ? 0 : 1];
            }
            else if (bl.Count > 0)
            {
                curTarget = bl[0];
            }
            else
            {
                //note: real enemies should probably have something to actually handle this
                //(i.e. choose a special move or just do nothing)
                Debug.Log("No targets found");
                curTarget = null;
            }

            if (GetBerserkTarget() != null)
            {
                if (bl.Contains(curTarget))
                {
                    curTarget = GetBerserkTarget();
                }
            }
        }
    }

    //called after all turns
    public virtual IEnumerator PostMove() //after all moves have been executed
    {
        //reset state in case sus stuff happens
        if (ac != null)
        {
            SetIdleAnimation();
            if (flipDefault)
            {
                ac.SendAnimationData("xflip");
            } else
            {
                ac.SendAnimationData("xunflip");
            }
        }

        //reset the list
        while (contactImmunityList.Count > 0)
        {
            contactImmunityList.RemoveAt(0);
        }

        if (attackHitCount > 0)
        {
            attackLastTurn = true;
        } else
        {
            attackLastTurn = false;
        }

        hitLastTurn = hitThisTurn;
        hitThisTurn = false;

        actionCounter = 0;

        if (currMove != null)
        {
            currMove.PostMove(this);
        }

        //Status effect actions
        ValidateEffects();
        /*
        if (HasStatus(Status.StatusEffect.SoulVine))
        {
            BattleEntity target = BattleControl.Instance.GetEntityByID(GetStatusEntry(Status.StatusEffect.SoulVine).casterID);
            if (target != null)
            {
                TakeDamageStrict(1 * (int)MainManager.StatusLevelToPower(GetStatusEntry(Status.StatusEffect.SoulVine).potency));
                HealHealth(GetStatusEntry(Status.StatusEffect.SoulVine).casterID, 1 * (int)MainManager.StatusLevelToPower(GetStatusEntry(Status.StatusEffect.SoulVine).potency));
            }
            yield return new WaitForSeconds(0.5f);
        }
        */
        /*
        if (HasStatus(Status.StatusEffect.SoulVortex))
        {
            BattleEntity target = BattleControl.Instance.GetEntityByID(GetStatusEntry(Status.StatusEffect.SoulVortex).casterID);
            if (target != null)
            {
                target.soulVortexDef = GetStatusEntry(Status.StatusEffect.SoulVortex).potency * 2;
            }
        }
        */

        bool effectStasis = false;
        if (HasEffect(Effect.EffectType.EffectStasis))
        {
            effectStasis = true;
        }

        bool statusDamage = false;

        if (HasEffect(Effect.EffectType.Poison))
        {
            statusDamage = true;
            int poisonDamage = maxHP / 10;

            if (poisonDamage < 2)
            {
                poisonDamage = 2;
            }

            if (poisonDamage > 10)
            {
                poisonDamage = 10;
            }

            TakeDamageStatus(poisonDamage * GetEffectEntry(Effect.EffectType.Poison).potency);
            yield return new WaitForSeconds(0.5f);
        }
        if (HasEffect(Effect.EffectType.Sunflame))
        {
            statusDamage = true;
            int sfDamage = maxHP / 10;

            int powermult = 1 + GetEffectEntry(Effect.EffectType.Sunflame).potency;

            if (sfDamage < 2)
            {
                sfDamage = 2;
            }

            if (sfDamage > 10)
            {
                sfDamage = 10;
            }

            TakeDamageStatus((int)(sfDamage * powermult * 0.5f));
            yield return new WaitForSeconds(0.5f);
        }
        if (HasEffect(Effect.EffectType.DamageOverTime))
        {
            TakeDamageStatus(GetEffectEntry(Effect.EffectType.DamageOverTime).potency);
            yield return new WaitForSeconds(0.5f);
        }
        if (HasEffect(Effect.EffectType.HealthLoss))
        {
            //statusDamage = true;
            //TakeDamageStatus(GetStatusEntry(Status.StatusEffect.HealthLoss).potency);

            //subtly different from damage
            HealHealth(-1 * GetEffectEntry(Effect.EffectType.HealthLoss).potency);
            yield return new WaitForSeconds(0.5f);
        }
        if (HasEffect(Effect.EffectType.HealthRegen))
        {
            HealHealth(GetEffectEntry(Effect.EffectType.HealthRegen).potency);
            yield return new WaitForSeconds(0.5f);
        }
        if (HasEffect(Effect.EffectType.Sleep))
        {
            int sleepHeal = maxHP / 10;

            if (sleepHeal < 2)
            {
                sleepHeal = 2;
            }

            if (sleepHeal > 10)
            {
                sleepHeal = 10;
            }
            HealHealth(sleepHeal * GetEffectEntry(Effect.EffectType.Sleep).potency);
            yield return new WaitForSeconds(0.5f);
        }
        if (HasEffect(Effect.EffectType.EnergyRegen))
        {
            HealEnergy(GetEffectEntry(Effect.EffectType.EnergyRegen).potency);
            yield return new WaitForSeconds(0.5f);
        }
        if (HasEffect(Effect.EffectType.EnergyLoss))
        {
            RemoveEnergy(GetEffectEntry(Effect.EffectType.EnergyLoss).potency);
            yield return new WaitForSeconds(0.5f);
        }

        if (HasEffect(Effect.EffectType.SoulRegen))
        {
            HealEnergy(GetEffectEntry(Effect.EffectType.SoulRegen).potency);
            yield return new WaitForSeconds(0.5f);
        }
        if (HasEffect(Effect.EffectType.SoulLoss))
        {
            RemoveEnergy(GetEffectEntry(Effect.EffectType.SoulLoss).potency);
            yield return new WaitForSeconds(0.5f);
        }

        if (HasEffect(Effect.EffectType.Hustle))
        {
            InflictEffectForce(this, new Effect(Effect.EffectType.BonusTurns, GetEffectEntry(Effect.EffectType.Hustle).potency, Effect.INFINITE_DURATION));
        }

        if (HasEffect(Effect.EffectType.Slow) && (BattleControl.Instance.turnCount % (GetEffectEntry(Effect.EffectType.Slow).potency + 1) != 0))
        {
            InflictEffectForce(this, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
        }

        if (HasEffect(Effect.EffectType.AstralWall) && damageTakenThisTurn >= GetEffectEntry(Effect.EffectType.AstralWall).potency)
        {
            RemoveEffect(Effect.EffectType.AstralWall);
        }
        damageTakenLastTurn = damageTakenThisTurn;
        counterFlareDamage = 0;
        counterFlareTrackedDamage = 0;
        arcDischargeDamage = 0;
        arcDischargeTrackedDamage = 0;
        splotchDamage = 0;
        splotchTrackedDamage = 0;
        damageTakenThisTurn = 0;


        //decrement turncount
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].duration != Effect.INFINITE_DURATION)
            {
                bool applyStasis = effectStasis && effects[i].duration <= 1;
                applyStasis &= (effects[i].effect != Effect.EffectType.EffectStasis);
                if (!applyStasis)
                {
                    effects[i].duration--;
                }
            }
        }
        ValidateEffects();

        ApplyBufferedEffects();

        //wait on the events
        while (inEvent)
        {
            yield return null;
        }

        //negate Charge if needed

        CheckRemoveFocus();

        //check everything right after your turn

        //enforce this check
        //note that the damaging statuses trigger their effects already so no deathcheck
        if (!statusDamage)
        {
            DeathCheck();
        }

        //Fix a problem I'm having
        SetIdleAnimation();

        if (!idleRunning && hasIdle && idleActive)
        {
            StartCoroutine("Idle");
        }
    }

    public virtual IEnumerator PostBattle()
    {
        yield break;
    }

    public virtual void DeathCheck()
    {
        //probably unnecessary
        /*
        for (int i = 0; i < eventQueue.Count; i++)
        {
            switch (eventQueue[i])
            {
                case BattleHelper.Event.Death:
                case BattleHelper.Event.StatusDeath:
                    return;
            }
        }
        */

        //only call this when you are not in an event (otherwise whatever event you're in might be the death event or death event may be on queue)
        if (hp <= 0 && alive)
        {
            QueueEvent(BattleHelper.Event.Death);
        }
    }

    /*
    public float GetMovePriority()
    {
        if (currMove == null)
        {
            return float.PositiveInfinity;
        }
        return currMove.time * GetAttribute(BattleHelper.Attribute.SpeedMult);
    }
    public int CompareMove(BattleEntity b)
    {
        //Debug.Log(GetMovePriority() + " " + b.GetMovePriority());
        if (currMove == null && b.currMove == null) //null check
        {
            return 0;
        }
        if (GetMovePriority() - b.GetMovePriority() > 0)
        {
            return 1;
        }
        else if (GetMovePriority() - b.GetMovePriority() < 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
    */

    //Replace with more dynamic system? that may be better for counters that only work on specific moves, but that is a weird idea

    //can this entity react to damage events? (potentially countering against the attacker)
    //Note: current system is set up that the counter has to be the last move you do
    public virtual bool CanReact() //strong counter and weak counter work differently
    {
        return false;
        //Debug.Log(currMove.GetName() + " " + currMove.isCounterReact);
        //return currMove != null && currMove.isCounterReact && hp > 0;
    }

    //Status damage
    public virtual void TakeDamageStatus(int damage, bool hide = false) //strict damage (used by more internal things, does not increment damage event counter, does status hurt events)
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.Invulnerable))
        {
            return;
        }

        bool preAlive = hp > 0;
        hp -= (damage);
        if (!hide)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Damage, damage, GetDamageEffectPosition(), this, BattleHelper.DamageType.Normal);
        }

        if (ShouldShowHPBar())
        {
            ShowHPBar();
        }    
        QueueEvent(BattleHelper.Event.StatusHurt);
        if (hp == 0 && damage != 0 && perfectKill)
        {
            perfectKill = false;    //if you take damage at 0 hp, forfeit the perfect kill bool
        }
        if (hp == 0 && preAlive)
        {
            perfectKill = true; //Went from above 0 to 0, so perfect kill
        }
        if (hp < 0)
        {
            perfectKill = false;
        }
        if (hp <= 0)
        {
            hp = 0;
            QueueEvent(BattleHelper.Event.StatusDeath);
        }
    }

    //This is past the attack and defense calculation (so this is only for special properties)
    public virtual int TakeDamage(int damage, BattleHelper.DamageType type, ulong properties) //default type damage
    {
        lastDamageTakenInput = damage;

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.ContactHazard) && ShouldShowHPBar())
        {
            ShowHPBar();
        }

        //failsafe in case damage calculation gets sussy
        if (damage < 0)
        {
            //HealHealth(-damage);
            //return;
            damage = 0;
        }

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            if (Invulnerable())
            {
                //note that this function is not actually the one that queues the hurt events
                //so there may be some possibilities of desync?
                damageEventsThisTurn++;
                absorbDamageEvents++;
                //Debug.Log(name + " " + damageEventsCount);
                hitThisTurn = true;
                //How about I just make it dink
                BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Damage, 0, GetDamageEffectPosition(), this, type, properties);
                return 0;
            }

            int bonusResistance = 0;

            if (BattleHelper.GetDamageProperty(properties, DamageProperties.ContactHazard) && GetEntityProperty(EntityProperties.SoftTouch))
            {
                bonusResistance += 2;
            }

            int damageReduction = damage - MainManager.DamageReductionFormula(damage, GetResistance() + bonusResistance);
            damage -= damageReduction;

            //Apply special properties
            damage = ApplyDefensiveProperties(damage, properties);

            //Quantum Shield
            if (TokenRemoveOne(Effect.EffectType.QuantumShield))
            {
                damage = 0;
            }

            //Apply Astral Wall
            damageTakenThisTurn += damage;
            if (HasEffect(Effect.EffectType.CounterFlare))
            {
                counterFlareTrackedDamage += damage;
            }
            if (HasEffect(Effect.EffectType.ArcDischarge))
            {
                arcDischargeDamage += damage;
            }
            if (HasEffect(Effect.EffectType.Splotch))
            {
                splotchDamage += damage;
            }

            if (HasEffect(Effect.EffectType.AstralWall))
            {
                if (damageTakenThisTurn > GetEffectEntry(Effect.EffectType.AstralWall).potency)
                {
                    int diff = damageTakenThisTurn - GetEffectEntry(Effect.EffectType.AstralWall).potency;

                    damageTakenThisTurn -= diff;
                    damage -= diff;

                    if (damage < 0)
                    {
                        damage = 0;
                    }
                }
            }

            //Anti-death statuses
            if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode) && HasEffect(Effect.EffectType.Miracle))
            {
                if (damage >= hp)
                {
                    BattleControl.Instance.CreateEffectParticles(GetEffectEntry(Effect.EffectType.Miracle), this);
                    TokenRemoveOne(Effect.EffectType.Miracle);
                    SetEntityProperty(BattleHelper.EntityProperties.NoMiracle, true);
                    //ReceiveEffectForce(new Effect(Effect.EffectType.NoMiracle, Effect.INFINITE_DURATION, 1));
                    damage = hp - 1;
                }
            }
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.TrueMinOne))
        {
            //Debug.Log("tenacious");
            if (damage < 1)
            {
                damage = 1;
            }
        }

        bool preAlive = hp > 0;

        if (HasEffect(Effect.EffectType.Soulbleed))
        {
            sbyte bleedDamage = (sbyte)(damage / 8);
            if (damage > 0 && bleedDamage <= 0)
            {
                bleedDamage = 1;
            }
            if (bleedDamage > 0)
            {
                ReceiveEffectForce(new Effect(Effect.EffectType.DamageOverTime, bleedDamage, 3), posId, Effect.EffectStackMode.KeepDurAddPot);
            }
        }

        if (HasEffect(Effect.EffectType.Soften))
        {
            sbyte softDamage = (sbyte)(damage / 3);
            if (softDamage > 0)
            {
                ReceiveEffectForce(new Effect(Effect.EffectType.DamageOverTime, softDamage, 3), posId, Effect.EffectStackMode.KeepDurAddPot);
            }
        } else
        {
            hp -= damage;
        }

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            //if (HasEffect(Effect.EffectType.ExactDamageKill))
            if (GetEntityProperty(BattleHelper.EntityProperties.ExactDamageKill))
            {
                //lol
                //for clarity reasons I will set this up with healing
                if (hp < 0)
                {
                    int heal = -hp;
                    hp = 0;
                    HealHealth(heal);
                }
            }
        }           

        //Post damage calculation
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.RemoveMaxHP))
        {
            maxHP -= damage;

            if (maxHP < 0)
            {
                maxHP = 0;
            }
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.RemoveMaxHP))
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.MaxHPDamage, damage, GetDamageEffectPosition(), this, type, properties);
        }
        else
        {
            if (HasEffect(Effect.EffectType.Soften))
            {
                BattleControl.Instance.CreateDamageEffect(DamageEffect.SoftDamage, damage, GetDamageEffectPosition(), this, type, properties);
            } else
            {
                if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Unblockable))
                {
                    BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.UnblockableDamage, damage, GetDamageEffectPosition(), this, type, properties);
                }
                else
                {
                    BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Damage, damage, GetDamageEffectPosition(), this, type, properties);
                }
            }
        }

        if (hp == 0 && damage != 0 && perfectKill)
        {
            perfectKill = false;    //if you take damage at 0 hp, forfeit the perfect kill bool
        }
        if (hp == 0 && preAlive)
        {
            perfectKill = true; //Went from above 0 to 0, so perfect kill
        }
        if (hp < 0)
        {
            perfectKill = false;
        }
        if (hp < 0)
        {
            hp = 0;
        }

        lastDamageTaken = damage;
        lastDamageType = type;

        //note that this function is not actually the one that queues the hurt events
        //so there may be some possibilities of desync?
        damageEventsThisTurn++;
        absorbDamageEvents++;
        //Debug.Log(name + " " + damageEventsCount);
        hitThisTurn = true;

        return damage;
    }

    //Calculation only (no side effects) version of TakeDamage
    //Note that some stuff can just use the return value from TakeDamage
    public virtual int TakeDamageCalculation(int damage, BattleHelper.DamageType type, ulong properties)
    {
        if (damage < 0)
        {
            return 0;
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return damage;
        }

        if (Invulnerable())
        {
            return 0;
        }

        int bonusResistance = 0;

        if (BattleHelper.GetDamageProperty(properties, DamageProperties.ContactHazard) && GetEntityProperty(EntityProperties.SoftTouch))
        {
            bonusResistance += 2;
        }

        int damageReduction = damage - MainManager.DamageReductionFormula(damage, GetResistance() + bonusResistance);
        damage -= damageReduction;

        //Apply special properties
        damage = ApplyDefensiveProperties(damage, properties);

        //Quantum Shield
        if (HasEffect(Effect.EffectType.QuantumShield))
        {
            damage = 0;
        }

        //Anti-death statuses
        if (HasEffect(Effect.EffectType.Miracle))
        {
            if (damage >= hp)
            {
                damage = hp - 1;
            }
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.TrueMinOne))
        {
            if (damage < 1)
            {
                damage = 1;
            }
        }

        return damage;
    }

    public int ApplyResistance(int damage)
    {
        return MainManager.DamageReductionFormula(damage, GetResistance());
    }
    public int ApplyDefensiveProperties(int damage, ulong properties)
    {

        //critical = negate hits that are weaker than current HP (*feel like this is better than "weaker than max hp" as it opens up hp down strats)
        if (GetEntityProperty(BattleHelper.EntityProperties.Hardened))
        {
            if (damage < hp)
            {
                damage = 0;
            }
        }
        //sturdy = dampen hits to be 1 less than max hp
        if (GetEntityProperty(BattleHelper.EntityProperties.Sturdy))
        {
            int maximum = maxHP - 1;
            maximum = Mathf.Max(1, maximum);

            if (damage > maximum)
            {
                damage = maximum;
            }
        }
        //toughness = weaker version of Critical (Negates damage below N)
        if (GetEntityProperty(BattleHelper.EntityProperties.Toughness))
        {
            if (damage <= 3)
            {
                damage = 0;
            }
        }
        //limiter = alternate version of Sturdy (Caps damage at N)
        if (GetEntityProperty(BattleHelper.EntityProperties.Resistant))
        {
            if (damage > 1)
            {
                damage = 1;
            }
        }

        if (BattleControl.Instance.enviroEffect == EnvironmentalEffect.WhiteoutBlizzard)
        {
            if (damageEventsThisTurn == 0)
            {
                damage = Mathf.CeilToInt(damage * 0.5f);
            }
        }
        if (BattleControl.Instance.enviroEffect == EnvironmentalEffect.TrialOfResolve)
        {
            //note: 0 damage hits will increment this, otherwise immunity would be permanent
            if (damageEventsThisTurn == 0)
            {
                damage = 0;
            }
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NonLethal) && damage >= hp)
        {
            damage = hp - 1;
            if (damage < 1)
            {
                damage = 0;
            }
        }

        return damage;
    }

    //to do: move some of this to BattleControl since the ep count is there
    public int RemoveEnergy(int amount, bool hide = false, Vector3? offset = null)
    {
        int a = BattleControl.Instance.ep;
        BattleControl.Instance.ep -= amount;
        if (!hide)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.DrainEnergy, amount, GetDamageEffectPosition() + offset.GetValueOrDefault(), this);
        }

        if (BattleControl.Instance.ep < 0)
        {
            BattleControl.Instance.ep = 0;
            return a;
        }
        else
        {
            return amount;
        }
    }
    public bool TryRemoveEnergy(int amount) //false = failed (did not remove), true = success (did remove energy)
    {
        if (BattleControl.Instance.ep < amount)
        {
            return false;
        }
        RemoveEnergy(amount);
        return true;
    }
    public void HealHealth(int p_targetID, int health)
    {
        BattleEntity target = BattleControl.Instance.GetEntityByID(p_targetID);
        target.HealHealth(health);
    }
    public void HealEnergy(int p_targetID, int energy)
    {
        BattleEntity target = BattleControl.Instance.GetEntityByID(p_targetID);
        target.HealEnergy(energy);
    }
    public virtual void HealHealth(int health)
    {
        if (HasEffect(Effect.EffectType.Soulbleed) && health > 0)
        {
            health = 0;
        }

        if (!BattleControl.IsPlayerControlled(this, false) && ShouldShowHPBar())
        {
            ShowHPBar();
        }
        //Subtle difference: Negative healing will not reduce you to 0 and bypasses all the damage stuff
        /*
        if (health < 0)
        {
            TakeDamage(-health);
            return;
        }
        */

        bool e = false;
        float reviveProportion = 0;
        if (hp <= 0 && health > 0 && maxHP > 0) //if maxHP = 0, then no amount of revive will work because you will still be at 0
        {
            reviveProportion = health / 60; //prevent the effect from looking weird (so a 60 hp revive will look equally strong in all cases)
            if (reviveProportion > 1)
            {
                reviveProportion = 1;
            }
            e = true;
        }

        if (!e && !alive && health > 0 && maxHP > 0)
        {
            reviveProportion = health / 60; //prevent the effect from looking weird (so a 60 hp revive will look equally strong in all cases)
            if (reviveProportion > 1)
            {
                reviveProportion = 1;
            }
            e = true;
        }

        if (health < 0)   //minus healing at 0 hp = no effect
        {
            if (hp <= 0)
            {
                hp += health;
                if (hp < 0)
                {
                    hp = 0;
                }
            } else
            {
                if (hp < 1)
                {
                    hp = 1;
                }
            }
        }
        else
        {
            hp += health;
        }

        if (health < 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.NegativeHeal, health, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeHealParticles(this, (int)(1 + 9 * Mathf.Log(health, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Heal, health, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateHealParticles(this, (int)(1 + 9 * Mathf.Log(health, 60)));
        }

        if (e)
        {
            BattleControl.Instance.CreateReviveParticles(this, 1 + 3 * reviveProportion);
            QueueEvent(BattleHelper.Event.Revive);
        }

        if (hp < 0)
        {
            hp = 1;
        }
        if (hp > maxHP)
        {
            hp = maxHP;
        }
    }
    public virtual int HealHealthTrackOverhealPay(int health)
    {
        if (HasEffect(Effect.EffectType.Soulbleed) && health > 0)
        {
            health = 0;
        }

        //Subtle difference: Negative healing will not reduce you to 0 and bypasses all the damage stuff
        /*
        if (health < 0)
        {
            TakeDamage(-health);
            return;
        }
        */

        bool e = false;
        if (hp <= 0 && health > 0)
        {
            e = true;
        }

        int payHealth = 0;
        if (health < 0)
        {
            payHealth = -health;
            hp += health;
            if (hp < 1)
            {
                payHealth -= -hp + 1;
                hp = 1;
            }
        }
        else
        {
            hp += health;
        }

        if (health < 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.NegativeHeal, health, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeHealParticles(this, (int)(1 + 9 * Mathf.Log(health, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Heal, health, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateHealParticles(this, (int)(1 + 9 * Mathf.Log(health, 60)));
        }

        if (e)
        {
            QueueEvent(BattleHelper.Event.Revive);
        }

        int overheal = payHealth;
        if (hp > maxHP)
        {
            overheal = hp - maxHP;
            hp = maxHP;
        }
        return overheal;
    }
    public void HealEnergy(int energy)
    {
        /*
        if (entityID >= 0)
        {
            return;
        }
        */

        BattleControl.Instance.AddEP(this, energy);

        //Vector3 hoffset = offset;

        if (energy > 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Energize, energy, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateEnergyParticles(this, (int)(1 + 9 * Mathf.Log(energy, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.DrainEnergy, energy, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeEnergyParticles(this, (int)(1 + 9 * Mathf.Log(energy, 60)));
        }
    }
    public int HealEnergyTrackOverhealPay(int energy)
    {
        /*
        if (entityID >= 0)
        {
            return;
        }
        */

        int overheal = BattleControl.Instance.AddEP(this, energy);

        //Vector3 hoffset = offset;

        if (energy > 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Energize, energy, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateEnergyParticles(this, (int)(1 + 9 * Mathf.Log(energy, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.DrainEnergy, energy, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeEnergyParticles(this, (int)(1 + 9 * Mathf.Log(energy, 60)));
        }

        return overheal;
    }

    public void HealSoulEnergy(int se)
    {
        if (entityID >= 0)
        {
            return;
        }

        BattleControl.Instance.AddSE(this, se);

        if (se > 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.SoulEnergize, se, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateSoulParticles(this, (int)(1 + 9 * Mathf.Log(se, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.DrainSoulEnergy, se, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeSoulParticles(this, (int)(1 + 9 * Mathf.Log(se, 60)));
        }
    }
    public int HealSoulEnergyTrackOverhealPay(int se)
    {
        int overheal = BattleControl.Instance.AddSE(this, se);

        if (se > 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.SoulEnergize, se, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateSoulParticles(this, (int)(1 + 9 * Mathf.Log(se, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.DrainSoulEnergy, se, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeSoulParticles(this, (int)(1 + 9 * Mathf.Log(se, 60)));
        }

        return overheal;
    }

    public virtual void PerTurnStaminaHeal()
    {
        int realAgility = GetRealAgility();
        stamina += realAgility;
        if (stamina >= BattleControl.Instance.GetMaxStamina(this))
        {
            stamina = BattleControl.Instance.GetMaxStamina(this);
        }
        if (stamina < 0)
        {
            stamina = 0;
        }
    }
    public virtual void HealStamina(int st)
    {
        stamina += st;
        if (st > 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Stamina, st, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateStaminaParticles(this, (int)(1 + 9 * Mathf.Log(st, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.DrainStamina, st, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeStaminaParticles(this, (int)(1 + 9 * Mathf.Log(st, 60)));
        }

        if (stamina > BattleControl.Instance.GetMaxStamina(this))
        {
            stamina = BattleControl.Instance.GetMaxStamina(this);
        }

        if (stamina < 0)
        {
            stamina = 0;
        }
    }
    public virtual int HealStaminaTrackOverhealPay(int st)
    {
        stamina += st;
        if (st > 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Stamina, st, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateStaminaParticles(this, (int)(1 + 9 * Mathf.Log(st, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.DrainStamina, st, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeStaminaParticles(this, (int)(1 + 9 * Mathf.Log(st, 60)));
        }

        int payStamina = 0;
        if (st > 0)
        {

        } else
        {
            payStamina = -st;
        }

        if (stamina < 0)
        {
            payStamina += stamina;
            stamina = 0;
        }

        int overheal = 0;
        if (stamina > BattleControl.Instance.GetMaxStamina(this))
        {
            overheal = BattleControl.Instance.GetMaxStamina(this) - stamina;
            stamina = BattleControl.Instance.GetMaxStamina(this);
        }

        return overheal;
    }

    //stupid notation
    //also there is no overheal version
    public void HealCoins(int coins)
    {
        if (entityID >= 0)
        {
            return;
        }

        BattleControl.Instance.AddCoins(this, coins);

        if (coins > 0)
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Coins, coins, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateCoinParticles(this, (int)(1 + 9 * Mathf.Log(coins, 60)));
        }
        else
        {
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.NegativeCoins, coins, GetDamageEffectPosition(), this);
            BattleControl.Instance.CreateNegativeCoinParticles(this, (int)(1 + 9 * Mathf.Log(coins, 60)));
        }
    }

    public Vector3 ApplyScaledOffset(Vector3 offset)
    {
        Vector3 output = transform.position;
        output += offset.x * width * Vector3.right;
        output += offset.y * height * Vector3.up;
        return output;
    }

    public void HideEffectIcons()
    {
        for (int i = 0; i < statusIcons.Count; i++)
        {
            Destroy(statusIcons[i]);
        }
    }
    public void ShowEffectIcons()
    {
        int numStateIcons = ShowStateIcons();

        for (int i = 0; i < effects.Count; i++)
        {
            GameObject g = Instantiate(BattleControl.Instance.statusIcon);
            g.transform.SetParent(transform);
            g.transform.localPosition = statusOffset + new Vector3(width * 0.5f + 0.1f, height, 0) + Vector3.up * StatusIconScript.VOFFSET * (i + numStateIcons);

            statusIcons.Add(g);

            StatusIconScript s = g.GetComponent<StatusIconScript>();

            s.Setup(effects[i]);
        }
    }

    public virtual int ShowStateIcons()
    {
        int output = 0;

        if (GetEntityProperty(BattleHelper.EntityProperties.Toughness))
        {
            MakeStateIcon(output, BattleHelper.EntityState.Toughness);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.Resistant))
        {
            MakeStateIcon(output, BattleHelper.EntityState.Limiter);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.Hardened))
        {
            MakeStateIcon(output, BattleHelper.EntityState.Critical);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.Sturdy))
        {
            MakeStateIcon(output, BattleHelper.EntityState.Sturdy);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.Unpiercable))
        {
            MakeStateIcon(output, BattleHelper.EntityState.Unpiercable);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.ExactDamageKill))
        {
            MakeStateIcon(output, BattleHelper.EntityState.ExactDamageKill);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.NoMiracle))
        {
            MakeStateIcon(output, BattleHelper.EntityState.NoMiracle);
            output++;
        }

        //note: player characters bypass the smt system
        if (statusMaxTurns <= 0 && posId >= 0)
        {
            MakeStateIcon(output, BattleHelper.EntityState.NoStatus);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.DebuffImmune))
        {
            MakeStateIcon(output, BattleHelper.EntityState.NoDebuff);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.StateStunned))
        {
            MakeStateIcon(output, BattleHelper.EntityState.StateStunned);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            MakeStateIcon(output, BattleHelper.EntityState.StateCharge);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.StateDefensive))
        {
            MakeStateIcon(output, BattleHelper.EntityState.StateDefensive);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.StateRage))
        {
            MakeStateIcon(output, BattleHelper.EntityState.StateRage);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.StateCounter))
        {
            MakeStateIcon(output, BattleHelper.EntityState.StateCounter);
            output++;
        }
        if (GetEntityProperty(BattleHelper.EntityProperties.StateCounterHeavy))
        {
            MakeStateIcon(output, BattleHelper.EntityState.StateCounterHeavy);
            output++;
        }
        if (GetEntityProperty(BattleHelper.EntityProperties.StateContactHazard))
        {
            MakeStateIcon(output, BattleHelper.EntityState.StateContactHazard);
            output++;
        }
        if (GetEntityProperty(BattleHelper.EntityProperties.StateContactHazardHeavy))
        {
            MakeStateIcon(output, BattleHelper.EntityState.StateContactHazardHeavy);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.CharacterMark))
        {
            MakeStateIcon(output, BattleHelper.EntityState.CharacterMark);
            output++;
        }

        if (GetEntityProperty(BattleHelper.EntityProperties.PositionMark))
        {
            MakeStateIcon(output, BattleHelper.EntityState.PositionMark);
            output++;
        }

        return output;
    }

    public void MakeStateIcon(int offset, BattleHelper.EntityState state, int potency = int.MinValue, int duration = int.MinValue)
    {
        GameObject g = Instantiate(BattleControl.Instance.stateIcon);
        g.transform.SetParent(transform);
        g.transform.localPosition = statusOffset + new Vector3(width * 0.5f + 0.1f, height, 0) + Vector3.up * StateIconScript.VOFFSET * (offset);
        statusIcons.Add(g);
        StateIconScript s = g.GetComponent<StateIconScript>();
        s.Setup(state, potency, duration);
    }

    public void HideHPBar()
    {
        if (hpbar != null)
        {
            Destroy(hpbar);
            hpbar = null;       //is this safe?
        }
    }
    public void ShowHPBar()
    {
        //Debug.Log("Show " + GetName());
        //note: hpbarIsActive is always set to false when the hpbar does not exist
        //unless you somehow interrupt these methods

        //but it should work with this condition now since I have it explicitly set hpbar to null
        //(the problem seems to happen when you hide and show on the same frame)
        if (hpbar == null) // || hpbarIsActive == false)
        {
            GameObject g = Instantiate(BattleControl.Instance.hpbar);
            g.transform.SetParent(transform);
            g.transform.localPosition = healthBarOffset + Vector3.down * (0.15f);

            hpbar = g;

            HPBarScript h = g.GetComponent<HPBarScript>();

            h.Setup(this);
        }
    }

    public bool ShouldShowHPBar()
    {
        bool bestiaryFlag = MainManager.Instance.GetBestiaryFlag(entityID);

        return bestiaryFlag || BattleControl.Instance.playerData.BadgeEquipped(Badge.BadgeType.HealthSight);
    }

    //Returns properties to OR onto the damage properties of the move
    //(mostly for making very specific badges work)
    public virtual ulong PreDealDamage(BattleEntity target, BattleHelper.DamageType type, ulong properties = 0, BattleHelper.ContactLevel contact = BattleHelper.ContactLevel.Infinite)
    {
        ulong outProperties = 0;

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (GetEntityProperty(EntityProperties.SoftTouch))
            {
                outProperties |= (ulong)BattleHelper.DamageProperties.SoftTouch;
            }
        }

        if (target is PlayerEntity pcaller)
        {
            //Not a good place to put this but it is convenient
            if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode) && pcaller.BadgeEquipped(Badge.BadgeType.RevivalFlame))
            {
                pcaller.revivalFlameHPCheck = false;
                if (target.maxHP <= maxHP)  //player character is target here
                {
                    pcaller.revivalFlameHPCheck = true;
                }
            }
        }

        return outProperties;
    }

    public virtual void PostDealDamage(BattleEntity target, BattleHelper.DamageType type, ulong properties = 0, BattleHelper.ContactLevel contact = BattleHelper.ContactLevel.Infinite, int damageTaken = 0)
    {

    }

    //Damage methods
    //Don't override them to change stuff
    //(virtual only because I want to insert some stuff before the player version)
    public int DealDamage(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties = 0, BattleHelper.ContactLevel contact = BattleHelper.ContactLevel.Infinite)
    {
        properties |= PreDealDamage(target, type, properties, contact);
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (applyCurseAttack)
            {
                damage = BattleControl.Instance.CurseAttackCalculation(damage);
            }
            damage = (int)(damage * attackMultiplier + 0.001f);
        }

        bool canCounterProperties = !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NoCounterProperties);
        //canCounterProperties = !canCounterProperties;

        //Debug.Log(target.CanCounter() + "" + !IsUncounterable() + "" + canCounterProperties);
        /*
        if (target.CanReact() && canCounterProperties)
        {
            target.ExecuteCounterBuffered(this);    //add it to reaction queue
        }
        */

        attackThisTurn = true;
        attackHitCount++;
        cumulativeAttackHitCount++;
        //ulong noChargeBlock = (ulong)BattleHelper.DamageProperties.NoCharge + (ulong)BattleHelper.DamageProperties.Hardcode + (ulong)BattleHelper.DamageProperties.Static
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NoChargeProperties))
        {
            if (HasEffect(Effect.EffectType.Focus) || HasEffect(Effect.EffectType.Defocus))
            {
                chargedAttackCount++;
            }
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PreserveFocus))
        {
            bufferRemoveCharge = true;
        }


        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            int d = target.TakeDamage(damage, type, properties);
            if (canCounterProperties)
            {
                //Apply contact hazards
                target.TryContactHazard(this, contact, type, d);
            }
            target.lastAttacker = this;
            InvokeHurtEvents(target, type, properties);
            return d;
        }

        int inputDamage = AttackBoostCalculation(target, damage, type, properties);
        int reducedDamage = DefenseCalculation(target, inputDamage, type, properties);

        int takenDamage = 0;

        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            takenDamage = -inputDamage; //deals "negative damage"
            target.HealHealth(inputDamage);
            OnHitEffects(target, takenDamage, type, properties, inputDamage);
        }
        else
        {
            takenDamage = target.TakeDamage(reducedDamage, type, properties);
            OnHitEffects(target, takenDamage, type, properties, inputDamage);
        }

        if (canCounterProperties)
        {
            //Apply contact hazards
            target.TryContactHazard(this, contact, type, takenDamage);
        }

        target.lastAttacker = this;

        InvokeHurtEvents(target, type, properties);

        #pragma warning disable CS0162
        if (DAMAGE_DEBUG)
        {
            Debug.Log("Deal damage: " + damage + " -> " + inputDamage + " -> " + reducedDamage + " -> " + takenDamage);
        }

        PostDealDamage(target, type, properties, contact, takenDamage);
        return takenDamage;
    }

    //hitIndex = 0 for first hit, 1 for second...
    public int DealDamageMultihit(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties, BattleHelper.ContactLevel contact,
                                    int hitIndex, Func<int,int,int> reductionFormula)
    {
        //this is literally just DealDamage with one line added
        //Refactor later?
        properties |= PreDealDamage(target, type, properties, contact);

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (applyCurseAttack)
            {
                damage = BattleControl.Instance.CurseAttackCalculation(damage);
            }
            damage = (int)(damage * attackMultiplier + 0.001f);
        }

        bool canCounterProperties = !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NoCounterProperties);

        //Debug.Log(target.CanCounter() + "" + !IsUncounterable() + "" + canCounterProperties);
        /*
        if (target.CanReact() && canCounterProperties)
        {
            target.ExecuteCounterBuffered(this);    //add it to reaction queue
        }
        */

        attackThisTurn = true;
        attackHitCount++;
        cumulativeAttackHitCount++;
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NoChargeProperties))
        {
            if (HasEffect(Effect.EffectType.Focus) || HasEffect(Effect.EffectType.Defocus))
            {
                chargedAttackCount++;
            }
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PreserveFocus))
        {
            bufferRemoveCharge = true;
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            int d = target.TakeDamage(damage, type, properties);
            if (canCounterProperties)
            {
                //Apply contact hazards
                //the attacker gets hit by the target's contact hazard
                target.TryContactHazard(this, contact, type, d);
            }
            target.lastAttacker = this;
            InvokeHurtEvents(target, type, properties);
            return d;
        }

        //The true damage number, accounting for all attack bonuses and modifiers
        int inputDamage = AttackBoostCalculation(target, damage, type, properties);
        int reducedDamage = DefenseCalculation(target, inputDamage, type, properties);
        int takenDamage = 0;

        //Apply formula
        reducedDamage = reductionFormula(reducedDamage, hitIndex);


        //this is hacky
        //need to calculate damage reduction after other stuff in the player function (within TakeDamage)
        //actually, you know what, just do it the non hacky way, doesn't matter
        /*
        if (!(target is PlayerEntity))
        {
            reducedDamage = MainManager.DamageReductionFormula(reducedDamage, target.GetResistance());
        }
        */

        //Debug.Log(target.GetDefense(type));


        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            takenDamage = -inputDamage;
            target.HealHealth(inputDamage);
            OnHitEffects(target, takenDamage, type, properties, reductionFormula(inputDamage, hitIndex));
        }
        else
        {
            takenDamage = target.TakeDamage(reducedDamage, type, properties);
            OnHitEffects(target, takenDamage, type, properties, reductionFormula(inputDamage, hitIndex));
        }

        if (canCounterProperties)
        {
            //Apply contact hazards
            //the attacker gets hit by the target's contact hazard
            target.TryContactHazard(this, contact, type, takenDamage);
        }

        target.lastAttacker = this;

        InvokeHurtEvents(target, type, properties);

        PostDealDamage(target, type, properties, contact, takenDamage);
        return takenDamage;
    }

    //this one just calls the other DealDamageMultihit
    public int DealDamageMultihit(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties, BattleHelper.ContactLevel contact,
                                    int hitIndex, BattleHelper.MultihitReductionFormula formula)
    {
        return DealDamageMultihit(target, damage, type, properties, contact, hitIndex, BattleHelper.GetMultihitReductionFormula(formula));
    }

    public int DealDamagePooled(BattleEntity other, BattleEntity target, int damage, int damageO, BattleHelper.DamageType type, ulong properties, BattleHelper.ContactLevel contact)
    {
        properties |= PreDealDamage(target, type, properties, contact);
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (applyCurseAttack)
            {
                damage = BattleControl.Instance.CurseAttackCalculation(damage);
                damageO = BattleControl.Instance.CurseAttackCalculation(damageO);
            }

            damage = (int)(damage * attackMultiplier + 0.001f);
            damageO = (int)(damageO * other.attackMultiplier + 0.001f);
        }

        //Debug.Log(damage + " " + damageO);

        bool canCounterProperties = !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NoCounterProperties);

        //Debug.Log(target.CanCounter() + "" + !IsUncounterable() + "" + canCounterProperties);
        /*
        if (target.CanReact() && canCounterProperties)
        {
            target.ExecuteCounterBuffered(this);    //add it to reaction queue
        }
        */

        attackThisTurn = true;
        attackHitCount++;
        cumulativeAttackHitCount++;
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NoChargeProperties))
        {
            if (HasEffect(Effect.EffectType.Focus) || HasEffect(Effect.EffectType.Defocus))
            {
                chargedAttackCount++;
            }
            if (other.HasEffect(Effect.EffectType.Focus) || other.HasEffect(Effect.EffectType.Defocus))
            {
                other.chargedAttackCount++;
            }
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PreserveFocus))
        {
            bufferRemoveCharge = true;
            other.bufferRemoveCharge = true;
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            int d = target.TakeDamage(damage + damageO, type, properties);
            if (canCounterProperties)
            {
                //Apply contact hazards
                //the attacker gets hit by the target's contact hazard
                target.TryContactHazard(this, contact, type, d);
            }
            target.lastAttacker = this;
            InvokeHurtEvents(target, type, properties);
            return d;
        }

        int inputDamage = AttackBoostCalculation(target, damage, type, properties);
        //Debug.Log(inputDamage);
        inputDamage += other.AttackBoostCalculation(target, damageO, type, properties);
        //Debug.Log(inputDamage);
        int reducedDamage = DefenseCalculation(target, inputDamage, type, properties);
        int takenDamage = 0;

        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            takenDamage = -inputDamage;
            target.HealHealth(inputDamage);
            OnHitEffects(target, takenDamage, type, properties, inputDamage);
        }
        else
        {
            takenDamage = target.TakeDamage(reducedDamage, type, properties);
            OnHitEffects(target, takenDamage, type, properties, inputDamage);
        }

        if (canCounterProperties)
        {
            //Apply contact hazards
            //the attacker gets hit by the target's contact hazard
            target.TryContactHazard(this, contact, type, takenDamage);
        }

        target.lastAttacker = this;

        InvokeHurtEvents(target, type, properties);

        PostDealDamage(target, type, properties, contact, takenDamage);
        return takenDamage;
    }
    public int DealDamagePooledMultihit(BattleEntity other, BattleEntity target, int damage, int damageO, BattleHelper.DamageType type, ulong properties, BattleHelper.ContactLevel contact,
                                        int hitIndex, Func<int, int, int> reductionFormula)
    {
        properties |= PreDealDamage(target, type, properties, contact);
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (applyCurseAttack)
            {
                damage = BattleControl.Instance.CurseAttackCalculation(damage);
                damageO = BattleControl.Instance.CurseAttackCalculation(damageO);
            }

            damage = (int)(damage * attackMultiplier + 0.001f);
            damageO = (int)(damageO * other.attackMultiplier + 0.001f);
        }

        bool canCounterProperties = !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NoCounterProperties);

        //Debug.Log(target.CanCounter() + "" + !IsUncounterable() + "" + canCounterProperties);
        /*
        if (target.CanReact() && canCounterProperties)
        {
            target.ExecuteCounterBuffered(this);    //add it to reaction queue
        }
        */

        attackThisTurn = true;
        attackHitCount++;
        cumulativeAttackHitCount++;
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NoChargeProperties))
        {
            if (HasEffect(Effect.EffectType.Focus) || HasEffect(Effect.EffectType.Defocus))
            {
                chargedAttackCount++;
            }
            if (other.HasEffect(Effect.EffectType.Focus) || other.HasEffect(Effect.EffectType.Defocus))
            {
                other.chargedAttackCount++;
            }
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PreserveFocus))
        {
            bufferRemoveCharge = true;
            other.bufferRemoveCharge = true;
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            int d = target.TakeDamage(damage, type, properties);
            target.lastAttacker = this;
            InvokeHurtEvents(target, type, properties);
            if (canCounterProperties)
            {
                //Apply contact hazards
                //the attacker gets hit by the target's contact hazard
                target.TryContactHazard(this, contact, type, d);
            }
            return d;
        }

        int inputDamage = AttackBoostCalculation(target, damage, type, properties);
        inputDamage += other.AttackBoostCalculation(target, damageO, type, properties);
        int reducedDamage = DefenseCalculation(target, inputDamage, type, properties);
        int takenDamage = 0;

        //Apply formula
        reducedDamage = reductionFormula(reducedDamage, hitIndex);

        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            takenDamage = -inputDamage;
            target.HealHealth(inputDamage);
            OnHitEffects(target, takenDamage, type, properties, reductionFormula(inputDamage, hitIndex));
        }
        else
        {
            takenDamage = target.TakeDamage(reducedDamage, type, properties);
            OnHitEffects(target, takenDamage, type, properties, reductionFormula(inputDamage, hitIndex));
        }

        if (canCounterProperties)
        {
            //Apply contact hazards
            //the attacker gets hit by the target's contact hazard
            target.TryContactHazard(this, contact, type, takenDamage);
        }

        target.lastAttacker = this;

        InvokeHurtEvents(target, type, properties);

        PostDealDamage(target, type, properties, contact, takenDamage);
        return takenDamage;
    }
    public int DealDamagePooledMultihit(BattleEntity other, BattleEntity target, int damage, int damageO, BattleHelper.DamageType type, ulong properties, BattleHelper.ContactLevel contact,
                                        int hitIndex, BattleHelper.MultihitReductionFormula formula)
    {
        return DealDamagePooledMultihit(other, target, damage, damageO, type, properties, contact, hitIndex, BattleHelper.GetMultihitReductionFormula(formula));
    }

    //applies attack boosts (note: paraylze's attack down has to be here)
    public int AttackBoostCalculation(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties)
    {
        int output = AttackBoostCalculationStatic(this, target, damage, GetBadgeAttackBonus(), GetEffectAttackBonus(), type, properties);
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if ((type & DamageType.Fire) != 0 && BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.AdvancedElementCalc))
            {
                output = AttackBoostCalculationStatic(this, target, damage, GetBadgeAttackBonus(), GetEffectAttackBonus(true), type, properties);
            }
        }

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (HasEffect(Effect.EffectType.Paralyze))
            {
                output /= 2;
            }

            //instinct
            if (HasEffect(Effect.EffectType.Dizzy) && BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HitsWhileDizzyWeak))
            {
                output /= 2;
            }

            //sleepwalk
            if (HasEffect(Effect.EffectType.Sleep))
            {
                output /= 2;
            }

            if (HasEffect(Effect.EffectType.Dread))
            {
                output = 0;
            }

            //status exploit
            if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.StatusExploit))
            {
                if (target.HasStatus())
                {
                    bool doBoost = false;

                    if (!doBoost && target.HasEffect(Effect.EffectType.Berserk) && ((type & BattleHelper.DamageType.Fire) != 0))
                    {
                        doBoost = true;
                    }
                    if (!doBoost && target.HasEffect(Effect.EffectType.Sleep) && ((type & BattleHelper.DamageType.Dark) != 0))
                    {
                        doBoost = true;
                    }
                    if (!doBoost && target.HasEffect(Effect.EffectType.Paralyze) && ((type & BattleHelper.DamageType.Water) != 0))
                    {
                        doBoost = true;
                    }
                    if (!doBoost && target.HasEffect(Effect.EffectType.Dizzy) && ((type & BattleHelper.DamageType.Light) != 0))
                    {
                        doBoost = true;
                    }
                    if (!doBoost && target.HasEffect(Effect.EffectType.Freeze) && ((type & BattleHelper.DamageType.Earth) != 0))
                    {
                        doBoost = true;
                    }
                    if (!doBoost && target.HasEffect(Effect.EffectType.Poison) && ((type & BattleHelper.DamageType.Air) != 0))
                    {
                        doBoost = true;
                    }

                    if (doBoost)
                    {
                        output = (int)(output * 1.5f);
                    }
                }
            }

            if (target.HasEffect(Effect.EffectType.Sleep) && BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.NightmareStrike))
            {
                output = (int)(output * 1.5f);
            }

            if (target.HasEffect(Effect.EffectType.Berserk) && BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Aggravate))
            {
                output = (int)(output * 1.5f);
            }

            if (target.HasEffect(Effect.EffectType.Freeze) && BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Icebreaker))
            {
                output = (int)(output * 1.5f);
            }


            //Burden of Pride
            if (BattleControl.IsPlayerControlled(this, false) && MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Pride))
            {
                //sift for properties
                DamageProperties dp = DamageProperties.AC_Premature | DamageProperties.AC_Success | DamageProperties.AC_SuccessStall | DamageProperties.Item;
                //static is already checked early
                //Debug.Log(!GetDamageProperty(properties, dp));
                if (!GetDamageProperty(properties, dp))
                {
                    output /= 2;
                }
            }

            if (target.HasEffect(Effect.EffectType.Supercharge))
            {
                int oldoutput = output;
                output = (int)(output * (1f + 0.334f * target.GetEffectEntry(Effect.EffectType.Supercharge).potency));
                if (output < oldoutput + target.GetEffectEntry(Effect.EffectType.Supercharge).potency)
                {
                    output = oldoutput + target.GetEffectEntry(Effect.EffectType.Supercharge).potency;
                }
            }
        }

        return output;
    }

    //applies defense (and piercing)
    public int DefenseCalculation(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties)
    {
        return DefenseCalculationStatic(target, damage, type, properties);
    }

    //applies attack boosts
    public static int AttackBoostCalculationSourcelessStatic(BattleEntity target, int damage, int badgeDamage, int effectDamage, BattleHelper.DamageType type, ulong properties)
    {
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return damage;
        }

        //The true damage number, accounting for all attack bonuses and modifiers
        int inputDamage = damage + badgeDamage;

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            inputDamage = damage;
            badgeDamage = 0;
            effectDamage = 0;
        }

        if (inputDamage < 0)
        {
            inputDamage = 0;
        }

        float lightmultiplier = 0;
        float darkmultiplier = 0;

        float hppro = ((1.0f * target.hp) / target.maxHP);
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.LightDarkAbsolute))
        {
            hppro = ((1.0f * target.hp) / 60);
        }

        int lightbonus = 0;
        int darkbonus = 0;
        int firebonus = 0;
        int waterbonus = 0;
        int earthbonus = 0;
        int airbonus = 0;

        bool lightdark = false;

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.AdvancedElementCalc))
        {
            //Advanced damage types often use attributes of the caller, which can't be accessed here
            //So make assumptions
            float shppro = 0.5f;
            bool attackLastTurn = false;

            float delta = hppro - shppro;
            if (delta < 0)
            {
                delta = -delta;
            }
            //somehow
            if (delta > 1)
            {
                delta = 1;
            }

            //float shppro = ((1.0f * caller.hp) / caller.maxHP);
            //if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.LightDarkAbsolute))
            //{
            //    shppro = ((1.0f * caller.hp) / 60);
            //}

            if (((uint)type & (uint)BattleHelper.DamageType.Light) != 0)
            {
                if (((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
                {
                    //Special formula (more extreme at low and high hp)
                    //If I used the normal formula, then light + dark attacks would just have an unconditional 2x multiplier
                    //(since the bonuses would exactly cancel each other out, not counting rounding error)
                    lightmultiplier = Mathf.Max((4 * delta) - 2, 2 - (4 * delta));
                    lightdark = true;
                }
                else
                {
                    lightmultiplier = 1 - delta;
                }
            }

            if (!lightdark && ((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
            {
                darkmultiplier = delta;
            }

            lightbonus = (int)(lightmultiplier * inputDamage);
            darkbonus = (int)(darkmultiplier * inputDamage);

            if (((uint)type & (uint)BattleHelper.DamageType.Fire) != 0)
            {
                firebonus = target.damageTakenLastTurn + target.damageTakenThisTurn;
                if (effectDamage < 0)
                {
                    firebonus = -2 * effectDamage + target.damageTakenLastTurn + target.damageTakenThisTurn;
                }

                int absEffectDamage = effectDamage < 0 ? -effectDamage : effectDamage;
                int cap = (inputDamage + absEffectDamage);
                if (firebonus > cap)
                {
                    firebonus = cap;
                }
            }

            if (!target.hitLastTurn && ((uint)type & (uint)BattleHelper.DamageType.Water) != 0)
            {
                if (!attackLastTurn)
                {
                    waterbonus = (int)(1f * (inputDamage));
                }
                else
                {
                    waterbonus = (int)((2.00001f / 3f) * (inputDamage));
                }

                if (waterbonus < 1)
                {
                    waterbonus = 1;
                }
            }

            //hitThisTurn is set later so this is not always active (intended)
            if (target.hitThisTurn && ((uint)type & (uint)BattleHelper.DamageType.Earth) != 0)
            {
                earthbonus = (int)(inputDamage * ((1.00001f / 3f) * (target.damageEventsThisTurn + 1)));
                if (target.damageEventsThisTurn + 1 > 4)
                {
                    earthbonus = (int)(inputDamage * ((1.00001f / 3f) * (4)));
                }

                if (earthbonus < 1)
                {
                    earthbonus = 1;
                }
            }

            airbonus = target.GetDefense();
            if (airbonus >= DefenseTableEntry.IMMUNITY_CONSTANT)
            {
                //don't deal 10000000 damage to things
                airbonus = 0;
            }
            //another failsafe cap
            if (airbonus > inputDamage + effectDamage)
            {
                airbonus = inputDamage + effectDamage;
            }
        }
        else
        {
            if (((uint)type & (uint)BattleHelper.DamageType.Light) != 0)
            {
                if (((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
                {
                    //Special formula (more extreme at low and high hp)
                    //If I used the normal formula, then light + dark attacks would just have an unconditional 2x multiplier
                    //(since the bonuses would exactly cancel each other out, not counting rounding error)
                    lightmultiplier = Mathf.Max((4 * hppro) - 2, 2 - (4 * hppro));
                    lightdark = true;
                }
                else
                {
                    lightmultiplier = hppro;
                }
            }

            if (!lightdark && ((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
            {
                darkmultiplier = 1 - hppro;
            }

            lightbonus = (int)(lightmultiplier * inputDamage);
            darkbonus = (int)(darkmultiplier * inputDamage);

            if (((uint)type & (uint)BattleHelper.DamageType.Fire) != 0)
            {
                firebonus = target.damageTakenLastTurn + target.damageTakenThisTurn;

                int cap = (int)((2.00001f / 3f) * (inputDamage + effectDamage));
                if (cap < 1)
                {
                    cap = 1;
                }
                if (firebonus > cap)
                {
                    firebonus = cap;
                }
            }

            if (!target.hitLastTurn && ((uint)type & (uint)BattleHelper.DamageType.Water) != 0)
            {
                waterbonus = (int)((2.00001f / 3f) * (inputDamage));
                if (waterbonus < 1)
                {
                    waterbonus = 1;
                }
            }

            //hitThisTurn is set later so this is not always active (intended)
            if (target.hitThisTurn && ((uint)type & (uint)BattleHelper.DamageType.Earth) != 0)
            {
                earthbonus = (int)((2.00001f / 3f) * (inputDamage));
                if (earthbonus < 1)
                {
                    earthbonus = 1;
                }
            }

            //nothing
            airbonus = 0;
        }
        inputDamage += effectDamage;

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PlusOneOnBuff) && effectDamage > 0)
        {
            inputDamage++;
        }

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.IgnoreElementCalculation))
        {
            inputDamage += lightbonus;
            inputDamage += darkbonus;
            inputDamage += firebonus;
            inputDamage += waterbonus;
            inputDamage += earthbonus;
            inputDamage += airbonus;

            //Debug.Log("Light: " + lightbonus + ", Dark: " + darkbonus + ", Fire: " + firebonus + ", Water: " + waterbonus + ", Earth: " + earthbonus);
        }


        return inputDamage;
    }
    public static int AttackBoostCalculationStatic(BattleEntity caller, BattleEntity target, int damage, int badgeDamage, int effectDamage, BattleHelper.DamageType type, ulong properties)
    {
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return damage;
        }

        //The true damage number, accounting for all attack bonuses and modifiers
        int inputDamage = damage + badgeDamage;

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            inputDamage = damage;
            badgeDamage = 0;
            effectDamage = 0;
        }


        //Enviro effect boosts
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.IgnoreElementCalculation))
        {
            bool player = BattleControl.IsPlayerControlled(caller, false);
            
            switch (BattleControl.Instance.enviroEffect)
            {
                case EnvironmentalEffect.ElectricWind:
                    if (player && ((uint)type & (uint)DamageType.Air) != 0)
                    {
                        inputDamage += 1;
                    }
                    break;
                case EnvironmentalEffect.SeasideAir:
                    if (player && ((uint)type & (uint)DamageType.Water) != 0)
                    {
                        inputDamage += 2;
                    }
                    break;
                case EnvironmentalEffect.ScaldingHeat:
                    if (player && ((uint)type & (uint)DamageType.Fire) != 0)
                    {
                        inputDamage += 2;
                    }
                    break;
                case EnvironmentalEffect.DarkFog:
                    if (player && ((uint)type & (uint)DamageType.Dark) != 0)
                    {
                        inputDamage += 2;
                    }
                    break;
                case EnvironmentalEffect.FrigidBreeze:
                    if (player && ((uint)type & (uint)DamageType.Light) != 0)
                    {
                        inputDamage += 2;
                    }
                    break;

                case EnvironmentalEffect.SacredGrove:
                    if (((uint)type & (uint)DamageType.Earth) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.IonizedSand:
                    if (((uint)type & (uint)DamageType.Air) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.WhiteoutBlizzard:
                    if (((uint)type & (uint)DamageType.Light) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.VoidShadow:
                    if (((uint)type & (uint)DamageType.Dark) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.ScaldingMagma:
                    if (((uint)type & (uint)DamageType.Fire) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.CounterWave:
                    if (((uint)type & (uint)DamageType.Water) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.TrialOfSimplicity:
                    if (((uint)type & (uint)DamageType.Earth) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.TrialOfHaste:
                    if (((uint)type & (uint)DamageType.Air) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.TrialOfResolve:
                    if (((uint)type & (uint)DamageType.Light) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.TrialOfAmbition:
                    if (((uint)type & (uint)DamageType.Dark) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.TrialOfZeal:
                    if (((uint)type & (uint)DamageType.Fire) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
                case EnvironmentalEffect.TrialOfPatience:
                    if (((uint)type & (uint)DamageType.Water) != 0)
                    {
                        inputDamage *= 2;
                    }
                    break;
            }
        }



        if (inputDamage < 0)
        {
            inputDamage = 0;
        }

        //Count buffs and debuffs
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.CountBuffs))
        {
            inputDamage += target.CountBuffPotency();
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.CountDebuffs))
        {
            inputDamage += target.CountDebuffPotency();
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.CountDebuffs2))
        {
            inputDamage += target.CountDebuffPotency() * 2;
        }


        float lightmultiplier = 0;
        float darkmultiplier = 0;

        float hppro = ((1.0f * target.hp) / target.maxHP);
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.LightDarkAbsolute))
        {
            hppro = ((1.0f * target.hp) / 60);
        }

        int lightbonus = 0;
        int darkbonus = 0;
        int firebonus = 0;
        int waterbonus = 0;
        int earthbonus = 0;
        int airbonus = 0;

        bool lightdark = false;

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.AdvancedElementCalc))
        {
            //Advanced damage types often use attributes of the caller, which can't be accessed here
            //So make assumptions
            float shppro = ((1.0f * caller.hp) / caller.maxHP);
            if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.LightDarkAbsolute))
            {
                shppro = ((1.0f * caller.hp) / 60);
            }
            bool attackLastTurn = caller.attackLastTurn;

            float delta = hppro - shppro;
            if (delta < 0)
            {
                delta = -delta;
            }
            //somehow
            if (delta > 1)
            {
                delta = 1;
            }

            //float shppro = ((1.0f * caller.hp) / caller.maxHP);
            //if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.LightDarkAbsolute))
            //{
            //    shppro = ((1.0f * caller.hp) / 60);
            //}

            if (((uint)type & (uint)BattleHelper.DamageType.Light) != 0)
            {
                if (((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
                {
                    //Special formula (more extreme at low and high hp)
                    //If I used the normal formula, then light + dark attacks would just have an unconditional 2x multiplier
                    //(since the bonuses would exactly cancel each other out, not counting rounding error)
                    lightmultiplier = Mathf.Max((4 * delta) - 2, 2 - (4 * delta));
                    lightdark = true;
                }
                else
                {
                    lightmultiplier = 1 - delta;
                }
            }

            if (!lightdark && ((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
            {
                darkmultiplier = delta;
            }

            lightbonus = (int)(lightmultiplier * inputDamage);
            darkbonus = (int)(darkmultiplier * inputDamage);

            if (((uint)type & (uint)BattleHelper.DamageType.Fire) != 0)
            {
                //Also: bonus based on damage since last move

                int angrybonus = caller.damageTakenLastTurn + caller.damageTakenThisTurn;
                //no cap because it will get capped later
                //if (angrybonus > inputDamage + effectDamage)
                //{
                //   angrybonus = inputDamage + effectDamage;
                //}

                firebonus = target.damageTakenLastTurn + target.damageTakenThisTurn + angrybonus;
                if (effectDamage < 0)
                {
                    firebonus = -2 * effectDamage + target.damageTakenLastTurn + target.damageTakenThisTurn + angrybonus;
                }

                int absEffectDamage = effectDamage < 0 ? -effectDamage : effectDamage;
                int cap = inputDamage + absEffectDamage;
                if (firebonus > cap)
                {
                    firebonus = cap;
                }
            }

            if (!target.hitLastTurn && ((uint)type & (uint)BattleHelper.DamageType.Water) != 0)
            {
                if (!attackLastTurn)
                {
                    waterbonus = (int)(1f * (inputDamage));
                }
                else
                {
                    waterbonus = (int)((2.00001f / 3f) * (inputDamage));
                }

                if (waterbonus < 1)
                {
                    waterbonus = 1;
                }
            }

            //hitThisTurn is set later so this is not always active (intended)
            if (target.hitThisTurn && ((uint)type & (uint)BattleHelper.DamageType.Earth) != 0)
            {
                earthbonus = (int)(inputDamage * ((1.00001f / 3f) * (target.damageEventsThisTurn + 1)));
                if (target.damageEventsThisTurn + 1 > 4)
                {
                    earthbonus = (int)(inputDamage * ((1.00001f / 3f) * (4)));
                }

                if (earthbonus < 1)
                {
                    earthbonus = 1;
                }
            }

            airbonus = target.GetDefense();
            if (airbonus >= DefenseTableEntry.IMMUNITY_CONSTANT)
            {
                //don't deal 10000000 damage to things
                airbonus = 0;
            }

            if (airbonus > inputDamage + effectDamage)
            {
                airbonus = inputDamage + effectDamage;
            }
        }
        else
        {
            if (((uint)type & (uint)BattleHelper.DamageType.Light) != 0)
            {
                if (((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
                {
                    //Special formula (more extreme at low and high hp)
                    //If I used the normal formula, then light + dark attacks would just have an unconditional 2x multiplier
                    //(since the bonuses would exactly cancel each other out, not counting rounding error)
                    lightmultiplier = Mathf.Max((4 * hppro) - 2, 2 - (4 * hppro));
                    lightdark = true;
                }
                else
                {
                    lightmultiplier = hppro;
                }
            }

            if (!lightdark && ((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
            {
                darkmultiplier = 1 - hppro;
            }

            lightbonus = (int)(lightmultiplier * inputDamage);
            darkbonus = (int)(darkmultiplier * inputDamage);

            if (((uint)type & (uint)BattleHelper.DamageType.Fire) != 0)
            {
                //firebonus = (int)(0.5f * (inputDamage + effectDamage));
                firebonus = target.damageTakenLastTurn + target.damageTakenThisTurn;
                int cap = (int)((2.00001f / 3f) * (inputDamage + effectDamage));
                if (cap < 1)
                {
                    cap = 1;
                }
                if (firebonus > cap)
                {
                    firebonus = cap;
                }

            }

            if (!target.hitLastTurn && ((uint)type & (uint)BattleHelper.DamageType.Water) != 0)
            {
                waterbonus = (int)((2.00001f / 3f) * (inputDamage));

                if (waterbonus < 1)
                {
                    waterbonus = 1;
                }
            }

            //hitThisTurn is set later so this is not always active (intended)
            if (target.hitThisTurn && ((uint)type & (uint)BattleHelper.DamageType.Earth) != 0)
            {
                earthbonus = (int)((2.00001f / 3f) * (inputDamage));

                if (earthbonus < 1)
                {
                    earthbonus = 1;
                }
            }

            //nothing
            airbonus = 0;
        }

        inputDamage += effectDamage;

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PlusOneOnBuff) && effectDamage > 0)
        {
            inputDamage++;
        }

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.IgnoreElementCalculation))
        {
            inputDamage += lightbonus;
            inputDamage += darkbonus;
            inputDamage += firebonus;
            inputDamage += waterbonus;
            inputDamage += earthbonus;
            inputDamage += airbonus;

            //Debug.Log("Light: " + lightbonus + ", Dark: " + darkbonus + ", Fire: " + firebonus + ", Water: " + waterbonus + ", Earth: " + earthbonus);
        }


        return inputDamage;
    }

    //applies defense (and piercing)
    public static int DefenseCalculationStatic(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties)
    {
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return damage;
        }

        int reducedDamage = damage;

        if (target.GetEntityProperty(BattleHelper.EntityProperties.Unpiercable))
        {
            //enforced
            bool aircheckA = (((uint)type & (uint)BattleHelper.DamageType.Air) != 0);
            reducedDamage = MainManager.DamageFormula(damage, target.GetDefense(type));
            if (aircheckA)
            {
                if (target.HasAirDefense())
                {
                    //get defense is correct
                } else
                {
                    //get defense is wrong
                    reducedDamage = MainManager.DamageFormula(damage, target.GetDefense());
                }
            }
        }
        else
        {
            bool isPierce = BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PierceDef);
            if (!isPierce)
            {
                //Air type check (special)
                bool aircheck = (((uint)type & (uint)BattleHelper.DamageType.Air) != 0);
                //not necessary, GetDefense now works differently

                if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PierceOne))
                {
                    int def2 = target.GetDefense(type);
                    int def = def2 - 1;
                    if (def2 <= 0)
                    {
                        def = def2;
                    }
                    reducedDamage = MainManager.DamageFormula(damage, def);
                }
                else
                {
                    reducedDamage = MainManager.DamageFormula(damage, target.GetDefense(type));
                }
            }

            if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.MinOne) && reducedDamage < 1 && damage > 0)
            {
                reducedDamage = 1;
            }
        }

        return reducedDamage;
    }

    
    //Some actual damage calculation methods
    public int DealDamageCalculation(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties)
    {
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (applyCurseAttack)
            {
                damage = BattleControl.Instance.CurseAttackCalculation(damage);
            }
            damage = (int)(damage * attackMultiplier + 0.001f);
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return target.TakeDamageCalculation(damage, type, properties);
        }

        int inputDamage = AttackBoostCalculation(target, damage, type, properties);
        int reducedDamage = DefenseCalculation(target, inputDamage, type, properties);
        int takenDamage = TakeDamageCalculation(reducedDamage, type, properties);

        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            takenDamage = -inputDamage;
            return takenDamage;
        }
        else
        {
            takenDamage = target.TakeDamageCalculation(reducedDamage, type, properties);
        }

        return takenDamage;
    }
    public int DealDamageMultihitCalculation(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties,
                                    int hitIndex, Func<int, int, int> reductionFormula)
    {
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (applyCurseAttack)
            {
                damage = BattleControl.Instance.CurseAttackCalculation(damage);
            }
            damage = (int)(damage * attackMultiplier + 0.001f);
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return target.TakeDamageCalculation(reductionFormula(damage, hitIndex), type, properties);
        }

        int inputDamage = AttackBoostCalculation(target, damage, type, properties);
        int reducedDamage = DefenseCalculation(target, inputDamage, type, properties);
        reducedDamage = reductionFormula(reducedDamage, hitIndex);
        int takenDamage = 0;

        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            takenDamage = -inputDamage;
            return takenDamage;
        }
        else
        {
            takenDamage = target.TakeDamageCalculation(reducedDamage, type, properties);
        }        

        return takenDamage;
    }
    public int DealDamageMultihitCalculation(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties,
                                    int hitIndex, BattleHelper.MultihitReductionFormula formula)
    {
        return DealDamageMultihitCalculation(target, damage, type, properties, hitIndex, BattleHelper.GetMultihitReductionFormula(formula));
    }
    public int DealDamagePooledCalculation(BattleEntity other, BattleEntity target, int damage, int damageO, BattleHelper.DamageType type, ulong properties)
    {
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (applyCurseAttack)
            {
                damage = BattleControl.Instance.CurseAttackCalculation(damage);
                damageO = BattleControl.Instance.CurseAttackCalculation(damageO);
            }
            damage = (int)(damage * attackMultiplier + 0.001f);
            damageO = (int)(damageO * other.attackMultiplier + 0.001f);
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return target.TakeDamageCalculation(damage + damageO, type, properties);
        }

        int inputDamage = AttackBoostCalculation(target, damage, type, properties);
        inputDamage += other.AttackBoostCalculation(target, damageO, type, properties);
        int reducedDamage = DefenseCalculation(target, inputDamage, type, properties);
        int takenDamage = 0;

        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            takenDamage = -inputDamage;
            return takenDamage;
        }
        else
        {
            takenDamage = target.TakeDamageCalculation(reducedDamage, type, properties);
        }

        return takenDamage;
    }
    public int DealDamagePooledMultihitCalculation(BattleEntity other, BattleEntity target, int damage, int damageO, BattleHelper.DamageType type, ulong properties,
                                    int hitIndex, Func<int, int, int> reductionFormula)
    {
        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (applyCurseAttack)
            {
                damage = BattleControl.Instance.CurseAttackCalculation(damage);
                damageO = BattleControl.Instance.CurseAttackCalculation(damageO);
            }
            damage = (int)(damage * attackMultiplier + 0.001f);
            damageO = (int)(damageO * other.attackMultiplier + 0.001f);
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return target.TakeDamageCalculation(reductionFormula(damage + damageO, hitIndex), type, properties);
        }

        int inputDamage = AttackBoostCalculation(target, damage, type, properties);
        inputDamage += other.AttackBoostCalculation(target, damageO, type, properties);
        int reducedDamage = DefenseCalculation(target, inputDamage, type, properties);
        int takenDamage = 0;

        //Apply formula
        reducedDamage = reductionFormula(reducedDamage, hitIndex);

        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            takenDamage = -inputDamage;
            //target.HealHealth(inputDamage);
        }
        else
        {
            takenDamage = target.TakeDamageCalculation(reducedDamage, type, properties);
        }

        return takenDamage;
    }
    public int DealDamagePooledMultihitCalculation(BattleEntity other, BattleEntity target, int damage, int damageO, BattleHelper.DamageType type, ulong properties,
                                        int hitIndex, BattleHelper.MultihitReductionFormula formula)
    {
        return DealDamagePooledMultihitCalculation(other, target, damage, damageO, type, properties, hitIndex, BattleHelper.GetMultihitReductionFormula(formula));
    }

    //damage here is taken damage (the final value given by TakeDamage)
    //start damage is the attack boosted damage
    //This is run after TakeDamage
    public virtual void OnHitEffects(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties, int startDamage = 0)
    {
        if (damage < 0)
        {
            return;
        }

        //Apply HP drain
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HPDrainOneToOne))
        {
            if (damage < 1)
            {
                HealHealth(1);
            }
            else
            {
                HealHealth(damage);
            }
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HPDrainTwoToOne))
        {
            if (damage / 2 > 0)
            {
                HealHealth(damage / 2);
            }
            else
            {
                HealHealth(1);
            }
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HPDrainOneMax))
        {
            HealHealth(1);
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.EPDrainOneToOne))
        {
            if (damage < 1)
            {
                HealEnergy(1);
            }
            else
            {
                HealEnergy(damage);
            }
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.EPDrainTwoToOne))
        {
            if (damage / 2 > 0)
            {
                HealEnergy(damage / 2);
            }
            else
            {
                HealEnergy(1);
            }
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.EPDrainOneMax))
        {
            HealEnergy(1);
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.EPLossOneToOne))
        {
            if (damage < 1)
            {
                target.HealEnergy(-1);
            }
            else
            {
                target.HealEnergy(-damage);
            }
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.EPLossTwoToOne))
        {
            if (damage / 2 > 0)
            {
                target.HealEnergy(-damage / 2);
            }
            else
            {
                target.HealEnergy(-1);
            }
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.EPLossOneMax))
        {
            target.HealEnergy(-1);
        }

        //Astral recovery
        if (target.HasEffect(Effect.EffectType.AstralRecovery) && target.hp > 0 && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            target.HealHealth(startDamage / 5);
        }

        //Apply sprout
        if (target.HasEffect(Effect.EffectType.DrainSprout))
        {
            //target gets defocus
            //note: inflict status
            sbyte drain = target.GetEffectEntry(Effect.EffectType.DrainSprout).potency;
            InflictEffect(target, new Effect(Effect.EffectType.Defocus, drain, Effect.INFINITE_DURATION));
            HealHealth(2 * drain);
        }

        if (target.HasEffect(Effect.EffectType.BoltSprout))
        {
            //target gets sunder (buffered)
            //note: inflict status
            sbyte drain = target.GetEffectEntry(Effect.EffectType.BoltSprout).potency;
            InflictEffectBuffered(target, new Effect(Effect.EffectType.Sunder, drain, Effect.INFINITE_DURATION));
            HealEnergy(2 * drain);
        }

        if (BattleControl.IsPlayerControlled(this, false) && BattleControl.Instance.enviroEffect == EnvironmentalEffect.SeasideAir)
        {
            bool hard = BattleControl.Instance.HarderEnviroEffects();
            float power = BattleControl.Instance.EnviroEffectPower();

            if (hard)
            {
                int procSeasideAir = BattleControl.Instance.EnviroEveryXTurns(2f, power, cumulativeAttackHitCount);
                if (procSeasideAir > 0)
                {
                    InflictEffect(this, new Effect(Effect.EffectType.Sunder, (sbyte)procSeasideAir, Effect.INFINITE_DURATION));
                }
            }
            else
            {
                int procSeasideAir = BattleControl.Instance.EnviroEveryXTurns(4f, power, cumulativeAttackHitCount);
                if (procSeasideAir > 0)
                {
                    InflictEffect(this, new Effect(Effect.EffectType.Sunder, (sbyte)procSeasideAir, Effect.INFINITE_DURATION));
                }
            }
        }

        if (target.HasEffect(Effect.EffectType.ParryAura))
        {
            InflictEffect(target, new Effect(Effect.EffectType.Focus, target.GetEffectEntry(Effect.EffectType.ParryAura).potency, Effect.INFINITE_DURATION));
        }
        if (target.HasEffect(Effect.EffectType.BolsterAura))
        {
            InflictEffectBuffered(target, new Effect(Effect.EffectType.Absorb, target.GetEffectEntry(Effect.EffectType.BolsterAura).potency, Effect.INFINITE_DURATION));
        }

        if (target.HasEffect(Effect.EffectType.Elusive))
        {
            InflictEffectBuffered(target, new Effect(Effect.EffectType.Ethereal, 1, target.GetEffectEntry(Effect.EffectType.Elusive).potency));
        }
    }


    //invoke hurt events (used in DealDamage)
    //Debuff-only moves should use this method to properly do the hurt events without having to actually do damage
    public static void InvokeHurtEvents(BattleEntity target, BattleHelper.DamageType type, ulong properties)
    {
        target.InvokeHurtEvents(type, properties);
    }
    //version that should be used if outside of DealDamage
    public static void SpecialInvokeHurtEvents(BattleEntity attacker, BattleEntity target, BattleHelper.DamageType type, ulong properties)
    {
        target.lastAttacker = attacker;
        target.lastDamageType = type;
        target.lastDamageTaken = 0;
        target.lastDamageTakenInput = 0;

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.SuppressHitParticles))
        {
            BattleControl.Instance.CreateHitEffect(0, target.GetDamageEffectPosition(), target, type, properties);
        }

        target.InvokeHurtEvents(type, properties);
    }
    public virtual void InvokeHurtEvents(BattleHelper.DamageType type, ulong properties)
    {
        wakeUp = false;
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.ContactHazard))
        {
            QueueEvent(BattleHelper.Event.HiddenHurt);
            //if (target.hp == 0)
            //{
            //    target.hp = 1;
            //}
            return;
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.MetaKnockback))
        {
            QueueEvent(BattleHelper.Event.MetaKnockbackHurt);
            if (hp <= 0 && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Combo))
            {
                QueueEvent(BattleHelper.Event.Death);
            }
            return;
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Knockback))
        {
            QueueEvent(BattleHelper.Event.KnockbackHurt);
            if (hp <= 0 && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Combo))
            {
                QueueEvent(BattleHelper.Event.Death);
            }
            return;
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Combo))
        {
            QueueEvent(BattleHelper.Event.ComboHurt);
        }
        else
        {
            QueueEvent(BattleHelper.Event.Hurt);

            if (!BattleHelper.GetDamageProperty(properties, DamageProperties.SoftTouch))
            {
                if (!GetEntityProperty(EntityProperties.DeepSleep) && HasEffect(Effect.EffectType.Sleep))
                {
                    Effect s = GetEffectEntry(Effect.EffectType.Sleep);
                    s.potency--;
                    if (s.potency <= 0)
                    {
                        effects.Remove(s);
                        QueueEvent(BattleHelper.Event.CureStatus);
                    }
                    wakeUp = true;
                }
                if (!GetEntityProperty(EntityProperties.Glacier) && HasEffect(Effect.EffectType.Freeze))
                {
                    Effect s = GetEffectEntry(Effect.EffectType.Freeze);
                    s.potency--;
                    if (s.potency <= 0)
                    {
                        effects.Remove(s);
                        QueueEvent(BattleHelper.Event.CureStatus);
                    }
                }
            }

            if (hp <= 0 && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Combo))
            {
                QueueEvent(BattleHelper.Event.Death);
            }
        }
    }


    //invoke miss events
    public void InvokeMissEvents(BattleEntity target)
    {
        target.QueueEvent(BattleHelper.Event.Miss);
        BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Miss, target.GetDamageEffectPosition(), target);
    }


    //Sourceless damage (environmental effects do that)
    //Does not apply effect boosts
    //Items may do this too?
    public static int DealDamageSourceless(BattleEntity target, int damage, BattleHelper.DamageType type, ulong properties)
    {
        int output = 0;

        int inputDamage = AttackBoostCalculationSourcelessStatic(target, damage, 0, 0, type, properties);
        int reducedDamage = DefenseCalculationStatic(target, inputDamage, type, properties);

        if (target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            target.HealHealth(inputDamage);
            output = inputDamage;
        }
        else
        {
            output = target.TakeDamage(reducedDamage, type, properties);
        }

        target.lastAttacker = null;    //Sourceless damage

        InvokeHurtEvents(target, type, properties);

        return output;
    }

    //can I apply contact hazard damage to the target?
    //if so, do contact damage
    //(Note: this is for an attack that the target inflicted on this entity, of type (type) and contact level (contact) (that dealt damage damage, this is from TakeDamage))
    public virtual void TryContactHazard(BattleEntity target, BattleHelper.ContactLevel contact, BattleHelper.DamageType type, int damage)
    {
        EnvironmentalContactHazards(target, contact, type, damage);

        if (target.CanTriggerContactHazard(contact, type, damage))
        {
            //do
            
            //Check for contact hazard immunity list
            //(prevents multihits on the same target from hurting multiple times)
            //(does not prevent multitarget moves from doing the same!)

            if (target.contactImmunityList.Contains(posId))
            {
                return;
            }

            if (contact <= BattleHelper.ContactLevel.Contact)
            {
                //example function
                //DealDamage(target, 1, BattleHelper.DamageType.Normal, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                //target.contactImmunityList.Add(posId);                
            }
        }
    }
    public void EnvironmentalContactHazards(BattleEntity target, BattleHelper.ContactLevel contact, BattleHelper.DamageType type, int damage)
    {
        if (BattleControl.Instance.enviroEffect == EnvironmentalEffect.CounterWave && BattleControl.IsPlayerControlled(target, true))
        {
            DealDamage(target, 4, BattleHelper.DamageType.Water, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
        }
        if (BattleControl.Instance.enviroEffect == EnvironmentalEffect.TrialOfPatience && BattleControl.IsPlayerControlled(target, true))
        {
            if (damage > 0)
            {
                DealDamage(target, Mathf.CeilToInt(0.5f * damage), BattleHelper.DamageType.Water, (ulong)(BattleHelper.DamageProperties.StandardContactHazard | DamageProperties.IgnoreElementCalculation), BattleHelper.ContactLevel.Contact);
            }
        }
    }

    //can I trigger contact hazards?
    //Some enemies can avoid contact hazards naturally
    //(ex: a shelled enemy can touch stuff just fine, but may get poked through with ranged contact hazards)
    public virtual bool CanTriggerContactHazard(BattleHelper.ContactLevel contact, BattleHelper.DamageType type, int damage)
    {
        return true;
    }

    //try to attack, does it hit? (false = miss)
    public bool GetAttackHit(BattleEntity target, BattleHelper.DamageType type, ulong properties = 0)
    {
        if (HasEffect(Effect.EffectType.Dizzy) && !(BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HitsWhileDizzy) || BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HitsWhileDizzyWeak)))
        {
            return false;
        }

        //miss?
        if (!target.GetAttackHit(type, properties))
        {
            return false;
        }

        return true; //BattleHelper.AttackOutcome.Normal;
    }
    public bool GetAttackHitNoSideEffects(BattleEntity target, BattleHelper.DamageType type, ulong properties = 0)
    {
        if (HasEffect(Effect.EffectType.Dizzy) && !(BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HitsWhileDizzy) || BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HitsWhileDizzyWeak)))
        {
            return false;
        }

        //miss?
        if (!target.GetAttackHitNoSideEffects(type, properties))
        {
            return false;
        }

        return true; //BattleHelper.AttackOutcome.Normal;
    }
    //use this once per attack and only against the enemies you try to hit
    //(timing is not required?)
    public virtual bool GetAttackHit(BattleHelper.DamageType type, ulong properties = 0)
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.Illusory))
        {
            return false;
        }

        if (HasEffect(Effect.EffectType.Ethereal) && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PiercesEthereal) && ((type & DamageType.PierceEthereal) == 0))
        {
            return false;
        }
        return true;
    }
    public virtual bool GetAttackHitNoSideEffects(BattleHelper.DamageType type, ulong properties = 0)
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.Illusory))
        {
            return false;
        }

        if (HasEffect(Effect.EffectType.Ethereal) && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.PiercesEthereal) && ((type & DamageType.PierceEthereal) == 0))
        {
            return false;
        }
        return true;
    }

    //Property methods
    public virtual bool GetEntityProperty(BattleHelper.EntityProperties property, bool b = true)
    {
        //hard code stuff implying flags
        
        if ((property & BattleHelper.EntityProperties.Airborne) != 0 && homePos.y > AIRBORNE_CUTOFFHEIGHT)
        {
            //Debug.Log("high");
            return b; //high enough
        }
        if ((property & BattleHelper.EntityProperties.Airborne) != 0 && homePos.y <= AIRBORNE_CUTOFFHEIGHT)
        {
            //Debug.Log("high");
            return !b; //high enough
        }

        if ((property & BattleHelper.EntityProperties.LowStompable) != 0 && (stompOffset.y * height + homePos.y) < LOWSTOMPABLE_CUTOFFHEIGHT)
        {
            //Debug.Log("high");
            return b; //high enough
        }
        if ((property & BattleHelper.EntityProperties.LowStompable) != 0 && (stompOffset.y * height + homePos.y) >= LOWSTOMPABLE_CUTOFFHEIGHT)
        {
            //Debug.Log("high");
            return !b; //high enough
        }


        //Debug.Log(entityProperties + " vs "+(uint)property + " is "+ ((entityProperties / (uint)property) % 2 == 1));        

        //return ((entityProperties / (uint)property) % 2 == 1) == b; //% 2 removes larger flags, / property removes smaller flags (output = 1 if flag)

        //this looks wacky but it works
        return b != (((ulong)property & entityProperties) == 0);
    }
    public void SetEntityProperty(BattleHelper.EntityProperties property, bool b = true)
    {
        //Debug.Log(entityProperties + " |= " + (ulong)property);
        if (b)
        {
            entityProperties |= (ulong)property;
        }
        else
        {
            entityProperties &= ~((ulong)property);
        }
    }
    public void ForceEntityProperties(ulong propertyBlock)
    {
        entityProperties = propertyBlock;

        //Debug.Log(entityProperties);
    } //sets properties to block
    public void SetEntityProperties(ulong propertyBlock, bool b = true)
    {
        if (b)
        {
            entityProperties |= propertyBlock;
        }
        else
        {
            entityProperties &= ~propertyBlock;
        }

        //Debug.Log(entityProperties);
    }
    public bool Invulnerable()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.Invulnerable))
        {
            return true;
        }

        if (HasEffect(Effect.EffectType.TimeStop))
        {
            return true;
        }

        /*
        if (HasStatus(Status.StatusEffect.Shield))
        {
            return true;
        }
        if (HasStatus(Status.StatusEffect.WeakShield) && damageEventsCount < GetStatusEntry(Status.StatusEffect.WeakShield).potency)
        {
            return true;
        }
        */
        return false;
    }
    public bool IsAlive()
    {
        if (hp > 0)
        {
            alive = true;
        }

        return alive;
    }

    public Vector3 GetDamageEffectPosition()
    {
        return transform.position + offset + Vector3.up * (height / 2) - Vector3.forward * 0.1f;
    }

    //Read defense table from data
    public void ResetDefenseTable()
    {
        BattleEntityData bed = BattleEntityData.GetBattleEntityData(entityID);
        List<DefenseTableEntry> tempDT = new List<DefenseTableEntry>();
        for (int i = 0; i < bed.defenseTable.Count; i++)
        {
            tempDT.Add(new DefenseTableEntry(bed.defenseTable[i].type, bed.defenseTable[i].amount));
        }
        defenseTable = tempDT;
    }
    public void SetDefense(DamageType dt, int set)
    {
        for (int i = 0; i < defenseTable.Count; i++)
        {
            if (dt == defenseTable[i].type)
            {
                defenseTable[i].amount = set;
            }
            return;
        }

        //add new entry
        defenseTable.Add(new DefenseTableEntry(dt, set));
    }
    public virtual int GetDefense()
    {
        //Check defense table for untyped defense
        int defense = 0;
        for (int i = 0; i < defenseTable.Count; i++)
        {
            if (defenseTable[i].type == BattleHelper.DamageType.Default)
            {
                defense += defenseTable[i].amount;
                break;
            }
        }

        if (defense >= DefenseTableEntry.IMMUNITY_CONSTANT)
        {
            return defense;
        }

        return DefenseTransform(defense + GetBadgeDefenseBonus() + GetEffectDefenseBonus());
    }
    public virtual int GetDefense(BattleHelper.DamageType type) //note: Normal falls through to GetDefense()
    {
        //Note: we need to split it into its components and find the smallest
        bool[] bitsplit = new bool[32];
        int[] defenseblock = new int[32];

        int bitcount = 0;
        for (int i = 0; i < 32; i++)
        {
            bitsplit[i] = ((uint)type & (1 << i)) != 0;
            if (bitsplit[i])
            {
                bitcount++;
            }
            defenseblock[i] = int.MinValue;
        }


        //combined damage types (Note that normal + X is just counted as type X)
        if (bitcount > 1)
        {
            int mindef = int.MaxValue;
            for (int i = 0; i < 32; i++)
            {
                if (bitsplit[i])
                {
                    defenseblock[i] = GetDefense((BattleHelper.DamageType)(1 << i));
                    if (mindef == int.MaxValue || defenseblock[i] < mindef)
                    {
                        mindef = defenseblock[i];
                    }
                }
            }

            if (mindef == int.MaxValue)
            {
                //uh oh
                return GetDefense();
            }

            return mindef;  //already uses GetDefense to check this so don't add the boosts again
        }


        int defense = 0;
        bool found = false;
        for (int i = 0; i < defenseTable.Count; i++)
        {
            if (defenseTable[i].type == type)
            {
                defense += defenseTable[i].amount;
                found = true;
                break;
            }
        }

        if (!found)
        {
            //special cases
            if (type == DamageType.Prismatic || type == DamageType.Void)
            {
                return GetDefense(DamageType.Everything);
            }
            if (type == BattleHelper.DamageType.Air)
            {
                return 0;
            }
            return GetDefense();
        } else
        {
            if (defense >= DefenseTableEntry.IMMUNITY_CONSTANT)
            {
                //boosts are meaningless
                return defense;
            }

            return DefenseTransform(defense + GetBadgeDefenseBonus() + GetEffectDefenseBonus(), type);
        }
    }
    public virtual int DefenseTransform(int value, BattleHelper.DamageType type = BattleHelper.DamageType.Default)
    {
        return value;
    }

    public bool HasAirDefense()
    {
        for (int i = 0; i < defenseTable.Count; i++)
        {
            if (defenseTable[i].type == BattleHelper.DamageType.Air)
            {
                return true;
            }
        }

        return false;
    }

    public virtual int GetEffectAttackBonus(bool absolute = false) //attack bonus from effects (Absolute = treat debuffs as buffs, you can get "buffs are debuffs" by multiplying by -1)
    {
        Effect temp;
        int output = 0;

        int negative = absolute ? 1 : -1;

        temp = GetEffectEntry(Effect.EffectType.AttackBoost);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.AttackReduction);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        temp = GetEffectEntry(Effect.EffectType.Berserk);
        if (temp != null)
        {
            output += temp.potency * 1;
        }

        temp = GetEffectEntry(Effect.EffectType.Sunflame);
        if (temp != null)
        {
            output += temp.potency * 1;
        }

        temp = GetEffectEntry(Effect.EffectType.AttackUp);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.AttackDown);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        temp = GetEffectEntry(Effect.EffectType.Illuminate);
        if (temp != null)
        {
            output += temp.potency * 1;
        }

        temp = GetEffectEntry(Effect.EffectType.Focus);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.Defocus);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        return output;
    }
    public virtual int GetEffectDefenseBonus(bool absolute = false) //defense bonus from effects
    {
        Effect temp;
        int output = 0;

        int negative = absolute ? 1 : -1;

        temp = GetEffectEntry(Effect.EffectType.DefenseBoost);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.DefenseReduction);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        temp = GetEffectEntry(Effect.EffectType.Berserk);
        if (temp != null)
        {
            output += temp.potency * negative;
        }
        //temp = GetStatusEntry(Status.StatusEffect.Petrify);
        //if (temp != null)
        //{
        //    output += temp.potency * 9;
        //}
        temp = GetEffectEntry(Effect.EffectType.Freeze);
        if (temp != null)
        {
            output += temp.potency * negative;
        }
        //temp = GetStatusEntry(Status.StatusEffect.Paralyze);
        //if (temp != null)
        //{
        //    output += temp.potency * 1;
        //}

        temp = GetEffectEntry(Effect.EffectType.Brittle);
        if (temp != null)
        {
            output += temp.potency * 2 * negative;
        }

        temp = GetEffectEntry(Effect.EffectType.DefenseUp);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.DefenseDown);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        temp = GetEffectEntry(Effect.EffectType.Absorb);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.Sunder);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        //Debug.Log(output);

        return output;
    }
    public virtual int GetEffectEnduranceBonus(bool absolute = false) //endurance bonus (effectively cost down)
    {
        Effect temp;
        int output = 0;

        int negative = absolute ? 1 : -1;

        temp = GetEffectEntry(Effect.EffectType.EnduranceReduction);
        if (temp != null)
        {
            output += temp.potency * negative;
        }
        temp = GetEffectEntry(Effect.EffectType.EnduranceBoost);
        if (temp != null)
        {
            output += temp.potency * 1;
        }

        temp = GetEffectEntry(Effect.EffectType.EnduranceUp);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.EnduranceDown);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        temp = GetEffectEntry(Effect.EffectType.Burst);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.Enervate);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        //Calculated later (in standard cost calculation)
        //output -= actionCounter;

        return output;
    }
    public virtual int GetEffectAgilityBonus(bool absolute = false)
    {
        Effect temp;
        int output = 0;

        int negative = absolute ? 1 : -1;

        temp = GetEffectEntry(Effect.EffectType.AgilityReduction);
        if (temp != null)
        {
            output += temp.potency * negative;
        }
        temp = GetEffectEntry(Effect.EffectType.AgilityBoost);
        if (temp != null)
        {
            output += temp.potency * 1;
        }

        temp = GetEffectEntry(Effect.EffectType.AgilityUp);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.AgilityDown);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        return output;
    }
    public virtual int GetEffectHasteBonus(bool absolute = false) //agility bonus (haste) (lets you pay moves at lower stamina)
    {
        Effect temp;
        int output = 0;

        int negative = absolute ? 1 : -1;

        temp = GetEffectEntry(Effect.EffectType.Haste);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.Hamper);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        return output;
    }

    public virtual int GetEffectFlowBonus(bool absolute = false)
    {
        Effect temp;
        int output = 0;

        int negative = absolute ? 1 : -1;

        temp = GetEffectEntry(Effect.EffectType.FlowDown);
        if (temp != null)
        {
            output += temp.potency * negative;
        }
        temp = GetEffectEntry(Effect.EffectType.FlowUp);
        if (temp != null)
        {
            output += temp.potency * 1;
        }

        temp = GetEffectEntry(Effect.EffectType.Awaken);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        temp = GetEffectEntry(Effect.EffectType.Disorient);
        if (temp != null)
        {
            output += temp.potency * negative;
        }

        return output;
    }

    public virtual int GetEffectResistanceBonus()
    {
        Effect temp;
        int output = 0;

        temp = GetEffectEntry(Effect.EffectType.MistWall);
        if (temp != null)
        {
            output += temp.potency * 1;
        }
        return output;
    }


    //placeholders for player functions (I need them here to make the damage formula consistent)
    public virtual int GetBadgeAttackBonus()
    {
        return 0;
    }
    public virtual int GetBadgeDefenseBonus()
    {
        return 0;
    }
    public virtual int GetBadgeEnduranceBonus()
    {
        return 0;
    }
    public virtual int GetBadgeAgilityBonus()
    {
        return 0;
    }
    public virtual int GetBadgeFlowBonus()
    {
        return 0;
    }
    public virtual int GetBadgeResistanceBonus()
    {
        return 0;
    }

    public virtual float GetItemUseBonus()  //power of item is multiplied by this
    {
        return 1;
    }
    public virtual float GetItemReceiveBonus()
    {
        return 1;
    }
    public virtual int GetBoostedAgility()
    {
        return agility + GetBadgeAgilityBonus() + GetEffectAgilityBonus();
    }
    public virtual int GetRealAgility()
    {
        if (HasEffect(Effect.EffectType.Exhausted))
        {
            return 0;
        }
        //the number representing stamina increase per turn
        int agility = GetBoostedAgility();
        int realAgility = agility;

        int half = Mathf.CeilToInt(agility / 2f);

        if (posId < 0 && BattleControl.Instance.playerData.BadgeEquipped(Badge.BadgeType.HeadStart) && BattleControl.Instance.turnCount < 1)
        {
            realAgility += half;
        }

        //Debug.Log(posId + " " + BattleControl.Instance.GetFrontmostAlly(posId) + " vs " + this);
        //a bit sus to do this every frame
        if (BattleControl.Instance.GetFrontmostAlly(posId) == this)
        {
            //add half
            realAgility += half;
        }

        return realAgility;
    }

    public virtual int GetResistance()
    {
        return GetBadgeResistanceBonus() + GetEffectResistanceBonus();
    }

    //Status methods

    //these methods work based on target

    public bool VoidCrushWorks(BattleEntity target, float multiplier = 1)
    {
        return (target.hp > 0) && target.hp <= VoidCrushHP(target, multiplier);
    }
    public int VoidCrushHP(BattleEntity target, float multiplier = 1)
    {
        int hp = 0;
        int maxHP = target.maxHP;
        int levDiff = target.level + target.bonusLevel - level;

        bool hasStatus = false;

        //using a bunch of HasStatus calls is inefficient
        for (int i = 0; i < effects.Count; i++)
        {
            if (Effect.GetEffectClass(effects[i].effect) == Effect.EffectClass.Status)
            {
                hasStatus = true;
                break;
            }
        }

        if (!target.GetEntityProperty(BattleHelper.EntityProperties.NoVoidCrush))
        {
            if (levDiff <= 0)
            {
                hp = maxHP;
            } else
            {
                //if ((hp * 10f) / maxHP <= (10 - levDiff) * multiplier * (hasStatus ? 2 : 1))
                //{
                //    doesWork = true;
                //}
                hp = (int)((maxHP * (10 - levDiff) * multiplier * (hasStatus ? 2 : 1)) / 10f);

                if (hp > maxHP)
                {
                    hp = maxHP;
                }
            }

            if (multiplier >= 10 && levDiff <= 10)
            {
                hp = maxHP;
            }
        } else
        {
            hp = 0;
        }

        if (hp < 0)
        {
            hp = 0; //equivalent (hp can never go below 0 and you can't really void crush a 0 hp thing)
        }

        return hp;
    }
    public bool TryVoidCrush(BattleEntity target, float multiplier = 1)
    {
        bool doesWork = target.hp <= VoidCrushHP(target, multiplier);

        if (doesWork)
        {
            //target.TakeDamageStatus(target.hp);
            DealDamage(target, target.hp, BattleHelper.DamageType.Dark, (ulong)BattleHelper.DamageProperties.Hardcode);
        } else
        {
            target.TakeDamage(0, BattleHelper.DamageType.Dark, (ulong)BattleHelper.DamageProperties.Hardcode);
        }

        return doesWork;
    }

    public int CountBuffPotency()
    {
        int output = 0;
        for (int i = 0; i < effects.Count; i++)
        {
            //Effect.GetEffectClass(statuses[i].effect) == Effect.EffectClass.BuffDebuff
            if (Effect.IsCleanseable(effects[i].effect))
            {
                output += effects[i].potency;
            }
        }
        return output;
    }
    public int CountDebuffPotency()
    {
        int output = 0;
        for (int i = 0; i < effects.Count; i++)
        {
            //Effect.GetEffectClass(statuses[i].effect) == Effect.EffectClass.BuffDebuff
            if (Effect.IsCurable(effects[i].effect))
            {
                output += effects[i].potency;
            }
        }
        if (HasStatus())
        {
            output += 2;
        }
        return output;
    }

    public void ApplyBufferedEffects()
    {
        //Debug.Log(name + " buffered");
        //Apply buffered statuses
        while (bufferedEffects.Count > 0)
        {
            ReceiveEffectForce(bufferedEffects[0], bufferedEffects[0].casterID);
            bufferedEffects.RemoveAt(0);
        }
        ValidateEffects(); //validate should be idempotent if you do it multiple times with no change
    }

    public virtual void InflictEffectBuffered(BattleEntity target, Effect se, int casterID = Effect.NULL_CASTERID, Effect.EffectStackMode mode = Effect.EffectStackMode.Default) //BattleHelper.EffectPopupPriority priority = BattleHelper.EffectPopupPriority.Never)
    {
        if (target != null)
        {
            bool statusWorks = true;

            //check status table?
            if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status)
            {
                StatusTableEntry st = target.GetStatusTableEntry(se.effect);

                //Check hp/maxhp < modified suseptibility
                //Modified suseptibility = s - 1 + (mst / bsmt)

                //so
                //(hp/maxhp) <= s + (smt - bsmt) / bsmt
                //(hp/maxhp) * bsmt <= s * bsmt + smt - bsmt
                //hp * bsmt <= maxhp * (s * bsmt + smt - bsmt)
                //Yay no division required so no rounding error (?)
                //but this equation is a little wacky looking

                //statusWorks = (target.hp * target.baseStatusMaxTurns <= target.maxHP * (st.susceptibility * target.baseStatusMaxTurns + target.statusMaxTurns - target.baseStatusMaxTurns));
                statusWorks = target.StatusWillWork(se.effect);
            }

            //Debug.Log(statusWorks + " " + target.baseStatusMaxTurns);

            //bmst less than 0 is immunity to everything you can inflict
            if ((target.baseStatusMaxTurns >= 0 || Effect.IsCleanseable(se.effect, true)) && statusWorks)
            {
                BattleControl.Instance.CreateEffectParticles(se, target);
                target.ReceiveEffectBuffered(se, casterID, mode);
            } else
            {
                //blocked, but why
                bool statusBlocked = (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status) && target.StatusWorkingHP(se.effect) > 0;

                if (statusBlocked)
                {
                    BattleControl.Instance.CreateStatusNotYetParticles(se, target);
                } else
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                }
            }
        }
    }
    public virtual void InflictEffect(BattleEntity target, Effect se, int casterID = Effect.NULL_CASTERID, Effect.EffectStackMode mode = Effect.EffectStackMode.Default) // BattleHelper.EffectPopupPriority priority = BattleHelper.EffectPopupPriority.Never)
    {
        if (target != null)
        {
            if (HasEffect(Effect.EffectType.Inverted))
            {
                Effect e = se.Copy();
                InvertEffect(e);
                se = e;
            }

            bool statusWorks = true;

            if (target.HasEffect(Effect.EffectType.Immunity) || target.HasEffect(Effect.EffectType.TimeStop))
            {
                if (Effect.IsCurable(se.effect))
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                    return;
                }
            }

            if (target.HasEffect(Effect.EffectType.Seal) || target.HasEffect(Effect.EffectType.TimeStop))
            {
                if (Effect.IsCleanseable(se.effect))
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                    return;
                }
            }

            if (target.GetEntityProperty(EntityProperties.NoMiracle))
            {
                if (se.effect == Effect.EffectType.Miracle)
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                    return;
                }
            }

            //check status table?
            if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status)
            {
                StatusTableEntry st = target.GetStatusTableEntry(se.effect);

                //Check hp/maxhp < modified suseptibility
                //Modified suseptibility = s -  1 + (mst / bsmt)

                //so
                //(hp/maxhp) <= s + (smt - bsmt) / bsmt
                //(hp/maxhp) * bsmt <= s * bsmt + smt - bsmt
                //hp * bsmt <= maxhp * (s * bsmt + smt - bsmt)
                //Yay no division required so no rounding error (?)
                //but this equation is a little wacky looking

                //statusWorks = (target.hp * target.baseStatusMaxTurns <= target.maxHP * (st.susceptibility * target.baseStatusMaxTurns + target.statusMaxTurns - target.baseStatusMaxTurns));
                statusWorks = target.StatusWillWork(se.effect);
                //Debug.Log(statusWorks);
            }

            //Debug.Log(statusWorks + " " + target.baseStatusMaxTurns);

            //bmst less than 0 is immunity to everything you can inflict
            if ((target.baseStatusMaxTurns >= 0 || Effect.IsCleanseable(se.effect, true)) && statusWorks)
            {
                BattleControl.Instance.CreateEffectParticles(se, target);
                target.ReceiveEffect(se, casterID, mode);
            }
            else
            {
                //blocked, but why
                bool statusBlocked = (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status) && target.StatusWorkingHP(se.effect) > 0;

                if (statusBlocked)
                {
                    BattleControl.Instance.CreateStatusNotYetParticles(se, target);
                }
                else
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                }
            }
        }
    }

    //Only use it when absolutely necessary (or if it should bypass anti-effect stuff for some reason)
    public virtual void InflictEffectForce(BattleEntity target, Effect se, int casterID = Effect.NULL_CASTERID, Effect.EffectStackMode mode = Effect.EffectStackMode.Default) // BattleHelper.EffectPopupPriority priority = BattleHelper.EffectPopupPriority.Never)
    {
        if (target != null)
        {
            target.ReceiveEffectForce(se, casterID, mode);
        }
    }
    public virtual void ReceiveEffect(Effect se, int casterID = Effect.NULL_CASTERID, Effect.EffectStackMode mode = Effect.EffectStackMode.Default) // BattleHelper.EffectPopupPriority priority = BattleHelper.EffectPopupPriority.Never)
    {
        if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status)
        {
            se.duration = (sbyte)(se.duration * GetStatusTurnModifier(se.effect));

            //Enforce turncount restriction
            sbyte maximum = (sbyte)statusMaxTurns;

            if (se.duration > maximum)
            {
                se.duration = maximum;
            }

            //Apply turncount
            statusMaxTurns -= se.duration;
        }

        if (se.potency > 0 && se.duration > 0)
        {
            ReceiveEffectForce(se, casterID, mode);
        }
    }
    public virtual void ReceiveEffectBuffered(Effect se, int casterID = Effect.NULL_CASTERID, Effect.EffectStackMode mode = Effect.EffectStackMode.Default) // BattleHelper.EffectPopupPriority priority = BattleHelper.EffectPopupPriority.Never)
    {
        if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status)
        {
            se.duration = (sbyte)(se.duration * GetStatusTurnModifier(se.effect));

            //Enforce turncount restriction
            sbyte maximum = (sbyte)statusMaxTurns;

            if (se.duration > maximum)
            {
                se.duration = maximum;
            }

            //Apply turncount
            statusMaxTurns -= se.duration;
        }

        if (se.potency > 0 && se.duration > 0)
        {
            ReceiveEffectForceBuffered(se, casterID, mode);
        }
    }
    public void ReceiveEffectForce(Effect se, int casterID = Effect.NULL_CASTERID, Effect.EffectStackMode mode = Effect.EffectStackMode.Default, bool makePopup = true) // BattleHelper.EffectPopupPriority priority = BattleHelper.EffectPopupPriority.Never)
    {
        if (hp <= 0 && !GetEntityProperty(BattleHelper.EntityProperties.GetEffectsAtNoHP))
        {
            return;
        }

        //bool makePopup = true;

        //these are more internal effects
        if (se.effect == Effect.EffectType.BonusTurns)
        {
            makePopup = false;
        }
        if (se.effect == Effect.EffectType.Cooldown)
        {
            makePopup = false;
        }

        bool noveltyCheck = false;

        se.casterID = casterID;

        Effect entry = GetEffectEntry(se.effect);
        if (entry != null)
        {
            if (casterID != Effect.NULL_CASTERID)
            {
                entry.casterID = casterID; //overwrite casterID for now
            } else if (se.casterID != Effect.NULL_CASTERID)
            {
                entry.casterID = se.casterID;
            }

            //Debug.Log(entry + " " + se);
            switch (mode)
            {
                case Effect.EffectStackMode.KeepDurAddPot:
                    if (entry.duration < se.duration)
                    {
                        int total = entry.potency * entry.duration;
                        entry.potency = se.potency;
                        entry.duration = se.duration;
                        entry.potency += (sbyte)(total / se.duration);
                        if ((sbyte)(total / se.duration) < 1)
                        {
                            entry.potency++;
                        }
                    }
                    else
                    {
                        int total = se.potency * se.duration;
                        entry.potency += (sbyte)(total / entry.duration);
                        if ((sbyte)(total / entry.duration) < 1)
                        {
                            entry.potency++;
                        }
                    }
                    break;
                case Effect.EffectStackMode.KeepPotAddDur:
                    if (entry.duration == Effect.INFINITE_DURATION || se.duration == Effect.INFINITE_DURATION)
                    {
                        //No
                    } else
                    {
                        if (entry.potency < se.potency)
                        {
                            int total = entry.potency * entry.duration;
                            entry.potency = se.potency;
                            entry.duration = se.duration;
                            entry.duration += (sbyte)(total / se.potency);
                            if ((sbyte)(total / se.potency) < 1)
                            {
                                entry.duration++;
                            }
                        }
                        else
                        {
                            int total = se.potency * se.duration;
                            entry.duration += (sbyte)(total / entry.potency);
                            if ((sbyte)(total / entry.potency) < 1)
                            {
                                entry.duration++;
                            }
                        }
                    }
                    break;
                case Effect.EffectStackMode.AdditivePot:
                    entry.potency += se.potency;
                    break;
                case Effect.EffectStackMode.AdditiveDur:
                    entry.duration += se.duration;
                    break;
                case Effect.EffectStackMode.OverwriteLow:
                    if (entry.potency < se.potency)
                    {
                        entry.potency = se.potency;
                    }
                    if (entry.duration < se.duration)
                    {
                        entry.duration = se.duration;
                    }
                    break;
                default:
                    //Debug.Log("Mode" + mode + " " + Status.GetEffectClass(se.effect) + " stack");
                    switch (Effect.GetEffectClass(se.effect))
                    {
                        case Effect.EffectClass.Static:
                            //AdditivePot
                            entry.potency += se.potency;
                            break;
                        case Effect.EffectClass.Status:
                            //OverwriteLow
                            if (entry.potency < se.potency)
                            {
                                entry.potency = se.potency;
                            }
                            if (entry.duration < se.duration)
                            {
                                entry.duration = se.duration;
                            }
                            break;
                        case Effect.EffectClass.BuffDebuff:
                            //KeepPotAddDur
                            if (entry.duration == Effect.INFINITE_DURATION || se.duration == Effect.INFINITE_DURATION)
                            {
                                //No
                            }
                            else
                            {
                                if (entry.potency < se.potency)
                                {
                                    int total = entry.potency * entry.duration;
                                    entry.potency = se.potency;
                                    entry.duration = se.duration;
                                    entry.duration += (sbyte)(total / se.potency);
                                    if ((sbyte)(total / se.potency) < 1)
                                    {
                                        entry.duration++;
                                    }
                                }
                                else
                                {
                                    int total = se.potency * se.duration;
                                    entry.duration += (sbyte)(total / entry.potency);
                                    if ((sbyte)(total / entry.potency) < 1)
                                    {
                                        entry.duration++;
                                    }
                                }

                                //special case (to nerf stacking sticky then inverting it)
                                if (se.effect == Effect.EffectType.Sticky)
                                {
                                    if (entry.duration > 5)
                                    {
                                        entry.duration = 5;
                                    }
                                }
                            }

                            break;
                        case Effect.EffectClass.Token:
                            //Special case!
                            if (se.effect == Effect.EffectType.ItemBoost)
                            {
                                //OverwriteLow
                                if (entry.potency < se.potency)
                                {
                                    entry.potency = se.potency;
                                }
                            } else
                            {
                                //AdditivePot
                                entry.potency += se.potency;
                            }
                            break;
                    }
                    break;
            }
            //Debug.Log("Result: " + entry);
        }
        else
        {
            //new status, just add the effect
            effects.Add(se);
            noveltyCheck = true;
        }

        //Enforce status uniqueness
        //we already know which status is the newest, so just delete all the others (though technically there should only be 1 other status because of this code)
        bool infinitepriority = false;

        for (int i = 0; i < effects.Count; i++)
        {
            if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status && (Effect.GetEffectClass(effects[i].effect) == Effect.EffectClass.Status && effects[i].effect != se.effect))
            {
                //Infinite turncount statuses have special priority which makes them negate other statuses
                if (effects[i].duration == Effect.INFINITE_DURATION)
                {
                    infinitepriority = true;
                    break;
                }
                if (effects[i] == se)
                {
                    continue;
                }
                statusMaxTurns += effects[i].duration; //refund the missing amount
                if (statusMaxTurns > baseStatusMaxTurns)
                {
                    statusMaxTurns = baseStatusMaxTurns;
                }
                effects.RemoveAt(i);
                i--;                        //remove things properly!
            }
        }

        if (infinitepriority)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                if (Effect.GetEffectClass(effects[i].effect) == Effect.EffectClass.Status && effects[i].duration < Effect.INFINITE_DURATION)
                {
                    statusMaxTurns += effects[i].duration; //refund the missing amount
                    if (statusMaxTurns > baseStatusMaxTurns)
                    {
                        statusMaxTurns = baseStatusMaxTurns;
                    }
                    effects.RemoveAt(i);
                    i--;                        //remove things properly!
                }
            }
        }

        //Simplify conflicting statuses (i.e. attack up and attack down)
        //These use the KeepDurAddPot system with slight modifications
        //...but the tokens / static effects just use additive stacking

        //store them as pairs (a-b, c-d...)
        Effect.EffectType[] cs =
        {
            Effect.EffectType.AttackBoost,
            Effect.EffectType.AttackReduction,

            Effect.EffectType.DefenseBoost,
            Effect.EffectType.DefenseReduction,

            Effect.EffectType.EnduranceBoost,
            Effect.EffectType.EnduranceReduction,

            Effect.EffectType.AgilityBoost,
            Effect.EffectType.AgilityReduction,

            Effect.EffectType.MaxHPBoost,
            Effect.EffectType.MaxHPReduction,

            Effect.EffectType.MaxEPBoost,
            Effect.EffectType.MaxEPReduction,

            Effect.EffectType.MaxSEBoost,
            Effect.EffectType.MaxSEReduction,

            Effect.EffectType.AttackUp,
            Effect.EffectType.AttackDown,

            Effect.EffectType.DefenseUp,
            Effect.EffectType.DefenseDown,

            Effect.EffectType.EnduranceUp,
            Effect.EffectType.EnduranceDown,

            Effect.EffectType.AgilityUp,
            Effect.EffectType.AgilityDown,

            Effect.EffectType.FlowUp,
            Effect.EffectType.FlowDown,

            Effect.EffectType.HealthRegen,
            Effect.EffectType.HealthLoss,

            Effect.EffectType.EnergyRegen,
            Effect.EffectType.EnergyLoss,

            Effect.EffectType.SoulRegen,
            Effect.EffectType.SoulLoss,

            Effect.EffectType.Focus,
            Effect.EffectType.Defocus,

            Effect.EffectType.Absorb,
            Effect.EffectType.Sunder,

            Effect.EffectType.Burst,
            Effect.EffectType.Enervate,

            Effect.EffectType.Haste,
            Effect.EffectType.Hamper,

            Effect.EffectType.Awaken,
            Effect.EffectType.Disorient,

            Effect.EffectType.BonusTurns,
            Effect.EffectType.Cooldown,
        };

        //4d loop seems bad for performance
        //(or at least it is potentially a ton of operations considering how long the list of possible conflicting statuses is)
        /*
        for (int i = 0; i < statuses.Count; i++)
        {
            for (int j = 0; j < conflictingStatuses.Length; j++)
            {
                //...
            }
        }
        */

        //better way? make a list of indices of the statuses above for future reference to avoid loop retreading
        int[] csi = new int[cs.Length];
        for (int i = 0; i < csi.Length; i++)
        {
            csi[i] = -1;
        }
        for (int i = 0; i < effects.Count; i++)
        {
            //Debug.Log(effects[i]);
            for (int j = 0; j < cs.Length; j++)
            {
                if (effects[i].effect == cs[j])
                {
                    csi[j] = i;
                }
            }
        }

        int temppowerA = 0;
        int temppowerB = 0;
        int tempduration = 0;
        //reset the weaker status to 0,0 so I can get rid of it later
        for (int i = 0; i < cs.Length; i += 2)
        {
            if (csi[i] != -1 && csi[i + 1] != -1)
            {
                if (effects[csi[i]].duration != Effect.INFINITE_DURATION && effects[csi[i + 1]].duration != Effect.INFINITE_DURATION)
                {
                    temppowerA = effects[csi[i]].duration * effects[csi[i]].potency;
                    temppowerB = effects[csi[i + 1]].duration * effects[csi[i + 1]].potency;
                    if (temppowerA > temppowerB)
                    {
                        //First status wins out
                        //Duration = higher of the 2
                        //Potency = ((TemppowerA - TemppowerB) / dur)
                        //Potency rounds down but gets floored at 1
                        tempduration = effects[csi[i]].duration;
                        if (effects[csi[i + 1]].duration > tempduration)
                        {
                            tempduration = effects[csi[i + 1]].duration;
                        }
                        if (tempduration > Effect.MAX_NORMAL_DURATION)
                        {
                            tempduration = Effect.MAX_NORMAL_DURATION;
                        }

                        effects[csi[i]].duration = (sbyte)tempduration;
                        effects[csi[i]].potency = (sbyte)((temppowerA - temppowerB) / tempduration);
                        if (effects[csi[i]].potency < 1)
                        {
                            effects[csi[i]].potency = 1;
                        }

                        effects[csi[i + 1]].duration = 0;
                        effects[csi[i + 1]].potency = 0;
                    }
                    else if (temppowerB > temppowerA)
                    {
                        //Second status wins out
                        //Duration = higher of the 2
                        //Potency = ((TemppowerB - TemppowerA) / dur)
                        //Potency rounds down but gets floored at 1
                        tempduration = effects[csi[i]].duration;
                        if (effects[csi[i + 1]].duration > tempduration)
                        {
                            tempduration = effects[csi[i + 1]].duration;
                        }
                        if (tempduration > Effect.MAX_NORMAL_DURATION)
                        {
                            tempduration = Effect.MAX_NORMAL_DURATION;
                        }

                        effects[csi[i + 1]].duration = (sbyte)tempduration;
                        effects[csi[i + 1]].potency = (sbyte)((temppowerB - temppowerA) / tempduration);
                        if (effects[csi[i + 1]].potency < 1)
                        {
                            effects[csi[i + 1]].potency = 1;
                        }

                        effects[csi[i]].duration = 0;
                        effects[csi[i]].potency = 0;
                    }
                    else
                    {
                        //Perfectly cancels out both of them
                        effects[csi[i]].duration = 0;
                        effects[csi[i]].potency = 0;
                        effects[csi[i + 1]].duration = 0;
                        effects[csi[i + 1]].potency = 0;
                    }
                } else if (effects[csi[i]].duration != Effect.INFINITE_DURATION || effects[csi[i + 1]].duration != Effect.INFINITE_DURATION)
                {
                    /*
                    //The infinite duration one wins
                    //By convention this really should never happen though
                    if (effects[conflictingStatusIndices[i]].duration == Effect.INFINITE_DURATION)
                    {
                        effects[conflictingStatusIndices[i + 1]].duration = 0;
                        effects[conflictingStatusIndices[i + 1]].potency = 0;
                    }
                    else
                    {
                        effects[conflictingStatusIndices[i]].duration = 0;
                        effects[conflictingStatusIndices[i]].potency = 0;
                    }
                    */

                    //New behavior: Subtract potency only (use the same logic as the thing below)
                    if (effects[csi[i]].potency > effects[csi[i + 1]].potency)
                    {
                        effects[csi[i]].potency -= effects[csi[i + 1]].potency;
                        effects[csi[i + 1]].potency = 0;
                    }
                    else if (effects[csi[i]].potency < effects[csi[i + 1]].potency)
                    {
                        effects[csi[i + 1]].potency -= effects[csi[i]].potency;
                        effects[csi[i]].potency = 0;
                    }
                    else
                    {
                        //Perfectly cancels out both of them
                        effects[csi[i]].potency = 0;
                        effects[csi[i + 1]].potency = 0;
                    }
                }
                else
                {
                    //2 infinite duration ones
                    //Use additive stacking
                    //The KeepDurAddPot stacking would give the same result but eh
                    if (effects[csi[i]].potency > effects[csi[i + 1]].potency)
                    {
                        effects[csi[i]].potency -= effects[csi[i + 1]].potency;
                        effects[csi[i + 1]].potency = 0;
                    }
                    else if (effects[csi[i]].potency < effects[csi[i + 1]].potency)
                    {
                        effects[csi[i + 1]].potency -= effects[csi[i]].potency;
                        effects[csi[i]].potency = 0;
                    }
                    else
                    {
                        //Perfectly cancels out both of them
                        effects[csi[i]].potency = 0;
                        effects[csi[i + 1]].potency = 0;
                    }
                }

                //Debug.Log(effects[conflictingStatusIndices[i]]);
                //Debug.Log(effects[conflictingStatusIndices[i + 1]]);
            }
        }

        if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status && noveltyCheck)
        {
            QueueEvent(BattleHelper.Event.StatusInflicted);
        }

        for (int i = 0; i < effects.Count; i++)
        {
            //Debug.Log(effects[i]);
            if (effects.Count > 0 && effects[i].potency <= 0) //potency 0 is not allowed
            {
                effects.Remove(effects[i]);
                if (i > 0)
                {
                    i--;
                }
                QueueEvent(BattleHelper.Event.CureStatus);
            }
            if (effects.Count > 0 && effects[i].duration <= 0)
            {
                effects.Remove(effects[i]);
                if (i > 0)
                {
                    i--;
                }
                //do status curing code
                QueueEvent(BattleHelper.Event.CureStatus);
            }
        }

        //effect legality check
        //don't make a popup if the effect is 0 potency or duration for some reason (i.e. it would get removed immediately)
        bool legalValues = true;
        if (se.duration <= 0 || se.potency <= 0)
        {
            legalValues = false;
        }

        if (noveltyCheck && makePopup && legalValues)
        {            
            BattleControl.Instance.AddBattlePopup(this, se);
        }

        //Debug.Log(se.casterID);

        ValidateEffects();
    }
    public void ReceiveEffectForceBuffered(Effect se, int casterID = Effect.NULL_CASTERID, Effect.EffectStackMode mode = Effect.EffectStackMode.Default) // BattleHelper.EffectPopupPriority priority = BattleHelper.EffectPopupPriority.Never)
    {
        se.casterID = casterID;
        bufferedEffects.Add(se);        
    }

    public float GetStatusTurnModifier(Effect.EffectType effect)
    {
        return GetStatusTableEntry(effect).turnMod;
    }

    public StatusTableEntry GetStatusTableEntry(Effect.EffectType effect)
    {
        for (int i = 0; i < statusTable.Count; i++)
        {
            if (statusTable[i].status == effect)
            {
                return statusTable[i];
            }
        }
        return GetStatusTableEntry();
    }

    public StatusTableEntry GetStatusTableEntry()
    {
        for (int i = 0; i < statusTable.Count; i++)
        {
            if (statusTable[i].status == Effect.EffectType.Default)
            {
                return statusTable[i];
            }
        }
        //debug
        return new StatusTableEntry(Effect.EffectType.Default, 1, 1);

        //return default    //just a 0 turn 0 modifier thing I think
    }

    //will this status work on you
    public virtual bool StatusWillWork(Effect.EffectType se, float boost = 1, int lostHP = 0)
    {
        //StatusTableEntry ste = GetStatusTableEntry(se);

        //this has no division so there should not be any floating point errors
        //(other than the error of susceptibility being a float)
        //(but as long as the error is in the positive direction it should work out)
        //return hp * baseStatusMaxTurns <= maxHP * (ste.susceptibility * baseStatusMaxTurns + statusMaxTurns - baseStatusMaxTurns);

        //Debug.Log(hp + " " + StatusWorkingHP(se));

        //meh, use the statusworkinghp to make things consistent
        return (hp > 0) && hp - lostHP <= StatusWorkingHP(se) * boost;
    }
    public virtual int StatusWorkingHP(Effect.EffectType se)
    {
        StatusTableEntry ste = GetStatusTableEntry(se);
        //the hp you must be <= to inflict the status
        int val = (int)((maxHP * (ste.susceptibility * baseStatusMaxTurns + statusMaxTurns - baseStatusMaxTurns)) / baseStatusMaxTurns);

        //formula can go below but it really doesn't mean anything
        if (val < 0)
        {
            val = 0;
        }
        return val;
    }

    public bool HasStatus()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            //Debug.Log(effect + " "+statuses[i].effect);
            if (Effect.GetEffectClass(effects[i].effect) == Effect.EffectClass.Status)
            {
                return true;
            }
        }
        return false;
    }
    public bool HasEffect(Effect.EffectType effect)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            //Debug.Log(effect + " "+statuses[i].effect);
            if (effect == effects[i].effect)
            {
                return true;
            }
        }
        return false;
    }
    public void RemoveAllEffects()
    {
        effects.RemoveAll((e) => true);
        ValidateEffects();
    }
    public void CleanseEffects(bool curePermanents = true)
    {
        //(note: 1 + count because I want the effect to be noticeable even if you have nothing)
        int power = 1 + effects.FindAll((e) => Effect.IsCleanseable(e.effect, curePermanents)).Count;
        effects.RemoveAll((e) => Effect.IsCleanseable(e.effect, curePermanents));

        if (power > 4)
        {
            power = 4;
        }

        BattleControl.Instance.CreateCleanseParticles(this, power);
        if (power > 1)
        {
            QueueEvent(BattleHelper.Event.CureStatus);
        }
        ValidateEffects();
    }
    public int CureEffects(bool curePermanents = true)
    {
        //(note: 1 + count because I want the effect to be noticeable even if you have nothing)
        int power = 1 + effects.FindAll((e) => Effect.IsCurable(e.effect, curePermanents)).Count;
        effects.RemoveAll((e) => Effect.IsCurable(e.effect, curePermanents));

        int powerB = power - 1;
        if (power > 4)
        {
            power = 4;
        }

        BattleControl.Instance.CreateCureParticles(this, power);
        if (power > 1)
        {
            QueueEvent(BattleHelper.Event.CureStatus);
        }
        ValidateEffects();

        return powerB;
    }
    public void CureDeathCurableEffects()
    {
        effects.RemoveAll((e) => Effect.IsDeathCurable(e.effect));
        ValidateEffects();
    }
    public void RemoveEffect(Effect.EffectType i)
    {
        effects.RemoveAll((Effect s) => s.effect == i);
        ValidateEffects();
    }
    public void RemoveEffectsFromCaster(int casterID) //cure statuses from a certain ID
    {
        effects.RemoveAll((Effect s) => s.casterID == casterID);
        ValidateEffects();
    }
    public void RemoveEffectsFromCaster(int casterID, Effect.EffectType e) //cure statuses from a certain ID
    {
        effects.RemoveAll((Effect s) => s.casterID == casterID && s.effect == e);
        ValidateEffects();
    }
    public Effect GetEffectEntry(Effect.EffectType effect) //try to find the status entry for an effect (1 per effect)
    {
        Effect entry = null;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].effect == effect)
            {
                entry = effects[i];
                return entry;
            }
        }
        return entry;
    }
    public int GetEffectDuration(Effect.EffectType effect)
    {
        Effect s = GetEffectEntry(effect);
        if (s != null) {
            return s.duration;
        }
        return 0;
    }
    public int GetEffectPotency(Effect.EffectType effect)
    {
        Effect s = GetEffectEntry(effect);
        if (s != null)
        {
            return s.potency;
        }
        return 0;
    }
    //gets max of party potency
    public int GetPartyMaxEffectPotency(Effect.EffectType effect)
    {
        int potency = 0;

        foreach (BattleEntity b in BattleControl.Instance.GetEntitiesSorted(this, new TargetArea(TargetArea.TargetAreaType.LiveAlly)))
        {
            int currPotency = b.GetEffectPotency(effect);
            if (potency < currPotency)
            {
                potency = currPotency;
            }
        }

        return potency;
    }
    public virtual void ValidateEffects()
    {
        /*
        //remove link statuses if caster is invalid
        //devoured effect and soul moves get nullified if caster is not there
        //this is effectively instant
        Status.StatusEffect[] effects = { Status.StatusEffect.Devoured, Status.StatusEffect.SoulChain, Status.StatusEffect.SoulVine, Status.StatusEffect.SoulVortex };

        for (int i = 0; i < effects.Length; i++)
        {
            if (GetStatusEntry(effects[i]) != null)
            {
                if (BattleControl.Instance.GetEntityByID(GetStatusEntry(effects[i]).casterID) == null)
                {
                    //Debug.Log("c");
                    //GetStatusEntry(effects[i]).duration = 0;
                    statuses.Remove(GetStatusEntry(effects[i])); //hope this works
                }
            }
        }
        */

        for (int i = 0; i < effects.Count; i++)
        {
            //Debug.Log(this + " " + effects[i]);
            //Enforce caps
            //Note: enemies are allowed to cheat the positive cap (Ha)
            if (effects[i].effect == Effect.EffectType.Focus || effects[i].effect == Effect.EffectType.Defocus)
            {
                if (effects[i].potency > Effect.FOCUS_CAP && !(!BattleControl.IsPlayerControlled(this, true) && effects[i].effect == Effect.EffectType.Focus))
                {
                    effects[i].potency = Effect.FOCUS_CAP;
                }
            }
            if (effects[i].effect == Effect.EffectType.Absorb || effects[i].effect == Effect.EffectType.Sunder)
            {
                if (effects[i].potency > Effect.ABSORB_CAP && !(!BattleControl.IsPlayerControlled(this, true) && effects[i].effect == Effect.EffectType.Absorb))
                {
                    effects[i].potency = Effect.ABSORB_CAP;
                }

            }
            if (effects[i].effect == Effect.EffectType.Burst || effects[i].effect == Effect.EffectType.Enervate)
            {
                if (effects[i].potency > Effect.BURST_CAP && !(!BattleControl.IsPlayerControlled(this, true) && effects[i].effect == Effect.EffectType.Burst))
                {
                    effects[i].potency = Effect.BURST_CAP;
                }
            }

            if (effects.Count > 0 && effects[i].potency <= 0) //potency 0 is not allowed
            {
                effects.Remove(effects[i]);
                if (i > 0)
                {
                    i--;
                }
                QueueEvent(BattleHelper.Event.CureStatus);
            }
            if (effects.Count > 0 && effects[i].duration <= 0)
            {
                effects.Remove(effects[i]);
                if (i > 0)
                {
                    i--;
                }
                //do status curing code
                QueueEvent(BattleHelper.Event.CureStatus);
            }
        }

        effects.Sort((a, b) => ((int)a.effect - (int)b.effect));

        EffectSpriteUpdate();
    }
    public void InvertEffect(Effect e)
    {
        //store them as pairs (a-b, c-d...)
        //some effects are off limits due to being problematic
        //(infinite attack reduction -> boost is a bit too good)
        //there are also a few "sproadic" effects

        if (Effect.GetEffectClass(e.effect) == Effect.EffectClass.Static)
        {
            return;
        }

        Effect.EffectType ne = Effect.InvertEffectType(e.effect);
        if (ne == Effect.EffectType.Default)
        {
            return;
        }

        e.effect = ne;


        //audit some specific cases

        //this is symmetric
        //may lead to sussery with stacking sticky and then inverting it?
        //But sticky is a very rare debuff only given out by very specific items
        //Ended up making Sticky not stack past 5 duration which should be balanced?
        //  (5 sticky -> 4 item boost = 3x items, but you need to use at least 3 items to get that to work)
        if (e.effect == Effect.EffectType.Sticky)
        {
            e.duration = (sbyte)(e.potency + 1);
            e.potency = 1;
        }
        if (e.effect == Effect.EffectType.ItemBoost)
        {
            e.potency = (sbyte)(e.duration - 1);
            e.duration = Effect.INFINITE_DURATION;
        }
        if (e.effect == Effect.EffectType.MistWall)
        {
            e.potency = 1;
        }
        if (e.effect == Effect.EffectType.AstralWall)
        {
            sbyte min = (sbyte)Mathf.CeilToInt(maxHP / 4f);
            e.potency = min;
        }
        if (e.effect == Effect.EffectType.Inverted)
        {
            e.potency = 1;
        }
        if (e.effect == Effect.EffectType.TimeStop)
        {
            e.potency = 1;
        }
    }

    public string GetEffectParticleName()
    {
        if (HasEffect(Effect.EffectType.Paralyze))
        {
            return "Paralyze";
        }
        if (HasEffect(Effect.EffectType.Dizzy))
        {
            return "Dizzy";
        }
        if (HasEffect(Effect.EffectType.Poison))
        {
            return "Poison";
        }
        if (HasEffect(Effect.EffectType.Freeze))
        {
            return "Freeze";
        }
        return "?";
    }
    public GameObject CreateEffectParticle()
    {
        float newScale = Mathf.Max(this.height, this.width);
        GameObject eo = null;

        if (eo == null && HasEffect(Effect.EffectType.Paralyze))
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Perpetual_Paralyze"), subObject.transform);
        }
        if (eo == null && HasEffect(Effect.EffectType.Dizzy))
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Perpetual_Dizzy"), subObject.transform);
        }
        if (eo == null && HasEffect(Effect.EffectType.Poison))
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Perpetual_Poison"), subObject.transform);
        }
        if (eo == null && HasEffect(Effect.EffectType.Freeze))
        {
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Perpetual_Freeze"), subObject.transform);
        }

        if (eo != null)
        {
            eo.transform.localScale = newScale * Vector3.one;
            eo.transform.position = ApplyScaledOffset(Vector3.up * 0.5f);
        }
        return eo;
    }
    public void EffectSpriteUpdate()
    {
        //Don't refresh the particle if it's just going to spawn the same effect
        //(prevents stuttering?)
        if (effectParticle != null && !effectParticle.name.Contains(GetEffectParticleName()))
        {           
            Destroy(effectParticle);
        }
        if (effectParticle == null)
        {
            effectParticle = CreateEffectParticle();
        }

        //send data in accordance with the effects you have

        //going to try to reduce the number of materials/shaders I need
        //Currently:
        //1 for normal
        //1 for bigradient
        //1 for partial transparent bigradient
        //x2 (+1 for buff/debuff)

        //things to send
        //status
        string statusColorString = "X";

        if (HasEffect(Effect.EffectType.Berserk))
        {
            statusColorString = MainManager.ColorToString(new Color(0.5f, 0f, 0f, 1f));
            statusColorString += "|" + MainManager.ColorToString(new Color(1f, 0.2f, 0f, 1f));
            statusColorString += "|" + MainManager.ColorToString(new Color(1f, 0.6f, 0.3f, 1f));
            statusColorString += "|" + 0.1f;
        }
        if (HasEffect(Effect.EffectType.Freeze))
        {
            statusColorString = MainManager.ColorToString(new Color(0.24f, 0.48f, 0.8f, 1f));
            statusColorString += "|" + MainManager.ColorToString(new Color(0.5f, 0.7f, 1f, 1f));
            statusColorString += "|" + MainManager.ColorToString(new Color(0.63f, 0.78f, 0.96f, 1f));
            statusColorString += "|" + 0.15f;
        }
        if (HasEffect(Effect.EffectType.Poison))
        {
            statusColorString = MainManager.ColorToString(new Color(0.21f, 0.03f, 0.3f, 1f));
            statusColorString += "|" + MainManager.ColorToString(new Color(0.75f, 0f, 1f, 1f));
            statusColorString += "|" + MainManager.ColorToString(new Color(1f, 0.15f, 1f, 1f));
            statusColorString += "|" + 0.4f;
        }
        if (HasEffect(Effect.EffectType.Paralyze))
        {
            statusColorString = MainManager.ColorToString(new Color(0.4f, 0.4f, 0.05f, 1f));
            statusColorString += "|" + MainManager.ColorToString(new Color(0.7f, 0.7f, 0.2f, 1f));
            statusColorString += "|" + MainManager.ColorToString(new Color(1f, 1f, 0.4f, 1f));
            statusColorString += "|" + 0.5f;
        }

        //ethereal
        if (HasEffect(Effect.EffectType.Ethereal))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.5f, 0f, 0f, 0.75f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1f, 0.6f, 0.3f, 0.5f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1f, 0.6f, 0.3f, 0.5f));
                statusColorString += "|" + 0.1f;
            }
            statusColorString += "_" + MainManager.ColorToString(new Color(0.5f, 0f, 0f, 0.1f)) + "_";
        } else if (HasEffect(Effect.EffectType.Illuminate))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(1.3f, 1f, 0.3f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1.4f, 1.3f, 1f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(2.3f, 2.3f, 1.3f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_" + MainManager.ColorToString(new Color(1f, 0.85f, 0.5f, 0.1f)) + "_";
        } else if (HasEffect(Effect.EffectType.MistWall))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.5f, 0.8f, 0.8f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.5f, 1f, 1f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.7f, 1.2f, 1.2f, 1f));
                statusColorString += "|" + 0.6f;
            }
            statusColorString += "_" + MainManager.ColorToString(new Color(0f, 0.6f, 0.6f, 0.1f)) + "_";
        }
        else if (HasEffect(Effect.EffectType.AstralWall))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.6f, 0f, 1f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.4f, 0f, 0.6f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0f, 0f, 0f, 1f));
                statusColorString += "|" + 0.6f;
            }
            statusColorString += "_" + MainManager.ColorToString(new Color(0.25f, 0.1f, 0.4f, 0.1f)) + "_";
        }
        else if (HasEffect(Effect.EffectType.CounterFlare))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(1f, 0.6f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1.4f, 0.7f, 0.3f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(2.3f, 1.6f, 0.6f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_" + MainManager.ColorToString(new Color(0.75f, 0.5f, 0.25f, 0.1f)) + "_";
        }
        else if (HasEffect(Effect.EffectType.Supercharge))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(1.25f, 0.3f, 1.25f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1.4f, 0.5f, 1.4f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(2.3f, 1f, 1.3f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_" + MainManager.ColorToString(new Color(1f, 0.3f, 1f, 0.1f)) + "_";
        }
        else if (HasEffect(Effect.EffectType.QuantumShield))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0f, 0.3f, 1.2f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0f, 0f, 0.36f, 0.7f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.2f, 0.2f, 0.2f, 0.6f));
                statusColorString += "|" + 0.1f;
            }
            statusColorString += "_" + MainManager.ColorToString(new Color(0f, 0f, 0.5f, 0.1f)) + "_";
        }
        else if (HasEffect(Effect.EffectType.Soften))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.4f, 1.3f, 0.4f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1f, 1.4f, 1f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1.3f, 2.3f, 1.3f, 1f));
                statusColorString += "|" + 0.6f;
            }
            statusColorString += "_" + MainManager.ColorToString(new Color(0f, 0.5f, 0f, 0.1f)) + "_";
        }
        else if (HasEffect(Effect.EffectType.Splotch))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0f, 0f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.15f, 0.15f, 0.15f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.3f, 0.25f, 0.2f, 1f));
                statusColorString += "|" + 0.1f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.DrainSprout))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0f, 0.7f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0f, 0.35f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.6f, 0.7f, 0.6f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.BoltSprout))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.6f, 0.7f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.25f, 0.35f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.7f, 0.7f, 0.6f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.Sticky))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.2f, 0.1f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.6f, 0.5f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1f, 0.9f, 0.4f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.Soulbleed))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.7f, 0.0f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.35f, 0f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.7f, 0.5f, 0.5f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.Sunflame))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(1f, 0.85f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.8f, 0.8f, 0.4f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1f, 1f, 1f, 1f));
                statusColorString += "|" + 0.35f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.Brittle))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0f, 0.6f, 0.6f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.25f, 0.5f, 0.5f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1f, 1f, 1f, 1f));
                statusColorString += "|" + 0.35f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.Inverted))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.75f, 0.6f, 1f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.4f, 0.2f, 0.5f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.1f, 0f, 0.1f, 1f));
                statusColorString += "|" + 0.2f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.Dread))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.5f, 0.2f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.25f, 0.15f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.7f, 0.6f, 0.3f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.ArcDischarge))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(1f, 0.5f, 1.1f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1f, 0.4f, 1f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(1f, 1f, 1f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.TimeStop))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0f, 0.0f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0f, 0f, 1f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.6f, 0.6f, 1f, 1f));
                statusColorString += "|" + 0.2f;
            }
            statusColorString += "_X_";
        }
        else if (HasEffect(Effect.EffectType.Exhausted))
        {
            if (statusColorString.Equals("X"))
            {
                statusColorString = MainManager.ColorToString(new Color(0.1f, 0.7f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.3f, 0.6f, 0f, 1f));
                statusColorString += "|" + MainManager.ColorToString(new Color(0.6f, 0.55f, 0.6f, 1f));
                statusColorString += "|" + 0.5f;
            }
            statusColorString += "_X_";
        }
        else
        {
            statusColorString += "_X_";
        }

        string buffString = "X";
        string debuffString = "X";
        if (HasEffect(Effect.EffectType.AttackBoost) || HasEffect(Effect.EffectType.AttackUp))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            } else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(1f, 0, 0, 1));
        }
        if (HasEffect(Effect.EffectType.DefenseBoost) || HasEffect(Effect.EffectType.DefenseUp))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(0, 0, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.EnduranceBoost) || HasEffect(Effect.EffectType.EnduranceUp))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(1f, 1f, 0, 1));
        }
        if (HasEffect(Effect.EffectType.AgilityBoost) || HasEffect(Effect.EffectType.AgilityUp))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(0, 1f, 0, 1));
        }
        if (HasEffect(Effect.EffectType.FlowUp))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(0.5f, 0, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.Hustle))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(0.8f, 0.6f, 0.3f, 1));
        }
        if (HasEffect(Effect.EffectType.Focus))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(1f, 0, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.Absorb))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(0, 1f, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.Burst))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(1f, 0.5f, 0, 1));
        }
        if (HasEffect(Effect.EffectType.Haste))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(0.5f, 1f, 0, 1));
        }
        if (HasEffect(Effect.EffectType.Awaken))
        {
            if (buffString.Length > 1)
            {
                buffString += "|";
            }
            else
            {
                buffString = "";
            }
            buffString += MainManager.ColorToString(new Color(0.8f, 0.5f, 1f, 1));
        }

        buffString += "_";


        if (HasEffect(Effect.EffectType.AttackReduction) || HasEffect(Effect.EffectType.AttackDown))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(1f, 0, 0, 1));
        }
        if (HasEffect(Effect.EffectType.DefenseReduction) || HasEffect(Effect.EffectType.DefenseDown))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(0, 0, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.EnduranceReduction) || HasEffect(Effect.EffectType.EnduranceDown))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(1f, 1f, 0, 1));
        }
        if (HasEffect(Effect.EffectType.AgilityReduction) || HasEffect(Effect.EffectType.AgilityDown))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(0, 1f, 0, 1));
        }
        if (HasEffect(Effect.EffectType.FlowDown))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(0.5f, 0, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.Slow))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(0.8f, 0.5f, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.Defocus))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(1f, 0, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.Sunder))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(0, 1f, 1f, 1));
        }
        if (HasEffect(Effect.EffectType.Enervate))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(1f, 0.5f, 0, 1));
        }
        if (HasEffect(Effect.EffectType.Hamper))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(0.5f, 1f, 0, 1));
        }
        if (HasEffect(Effect.EffectType.Disorient))
        {
            if (debuffString.Length > 1)
            {
                debuffString += "|";
            }
            else
            {
                debuffString = "";
            }
            debuffString += MainManager.ColorToString(new Color(0.8f, 0.5f, 1f, 1));
        }


        if (ac != null)
        {
            ac.SendAnimationData("effect_" + statusColorString + buffString + debuffString);
        }
    }

    public void DropShadowUpdate(bool force = false)
    {
        if (!force && (dropShadow == null || noShadow))    //assumes that the floor doesn't drop from under you (but you would also have to be perfectly stationary)
        {
            return;
        }

        if (dropShadow == null)
        {
            return;
        }

        float shadowHeight = transform.position.y + 0.5f;

        dropShadow.transform.localPosition = Vector3.down * (shadowHeight / 2 - 0.005f);
        dropShadow.transform.localScale = (Vector3.right + Vector3.forward) * width * 1.4f + Vector3.up * (shadowHeight);
    }

    //return false if you do not have effect, true if you have effect (but effect is cured by this)
    public bool TokenRemove(Effect.EffectType e)
    {
        if (GetEffectEntry(e) != null)
        {
            effects.Remove(GetEffectEntry(e));
            return true;
        }
        return false;
    }
    //same as above but only removes 1 potency level at a time
    public bool TokenRemoveOne(Effect.EffectType e)
    {
        if (GetEffectEntry(e) != null)
        {
            GetEffectEntry(e).potency--;
            if (GetEffectEntry(e).potency < 1)
            {
                effects.Remove(GetEffectEntry(e));
            }
            return true;
        }
        return false;
    }

    public virtual void CheckRemoveAbsorb()
    {
        //Debug.Log(name + " check " + absorbDamageEvents);
        if (absorbDamageEvents > 0)
        {
            RemoveEffect(Effect.EffectType.Absorb);
            RemoveEffect(Effect.EffectType.Sunder);
            if (HasEffect(Effect.EffectType.Brittle))
            {
                Effect e = GetEffectEntry(Effect.EffectType.Brittle);
                e.potency++;
            }
        }
        absorbDamageEvents = 0;
    }
    public virtual void CheckRemoveFocus()
    {
        if (chargedAttackCount > 0)
        {
            chargedAttackCount = 0;
            if (bufferRemoveCharge)
            {
                bufferRemoveCharge = false;
            }
            else
            {
                RemoveEffect(Effect.EffectType.Focus);
                RemoveEffect(Effect.EffectType.Defocus);
            }
        }
        if (bufferRemoveCharge)
        {
            bufferRemoveCharge = false;
        }
    }

    public virtual bool CanMove() //some statuses prevent movement
    {
        ValidateEffects();

        Effect.EffectType[] effectList =
            new Effect.EffectType[] {
                Effect.EffectType.Freeze,
                Effect.EffectType.Sleep,
                Effect.EffectType.TimeStop,
            };

        foreach (Effect.EffectType e in effectList)
        {
            if (GetEffectEntry(e) != null)
            {
                return false;
            }
        }
        /*
        if (HasStatus(Status.StatusEffect.Slow))
        {
            return BattleControl.Instance.turnCount % (GetStatusEntry(Status.StatusEffect.Slow).potency + 1) == 1; //slow enemies can move on turn 1
        }
        */
        if (hp <= 0)
        {
            DeathCheck();   //may break stuff later but it shouldn't?
            return false;
        }
        return true;
    }

    public virtual bool AutoMove()
    {
        return HasEffect(Effect.EffectType.Berserk);
    }

    public virtual BattleEntity GetBerserkTarget()
    {
        if (HasEffect(Effect.EffectType.Berserk) && GetEffectEntry(Effect.EffectType.Berserk).casterID != Effect.NULL_CASTERID)
        {
            return BattleControl.Instance.GetEntityByID(GetEffectEntry(Effect.EffectType.Berserk).casterID);
        }

        return null;
    }

    //Move stuff
    public virtual IEnumerator ExecuteMoveCoroutine()
    {
        if (currMove == null)
        {
            moveExecuting = false;
            moveActive = false;
            yield break;
        }
        //moving = true;    //Set already
        yield return currExec = StartCoroutine(currMove.Execute(this)); //this looks wrong
        moveExecuting = false;
        moveActive = false;
        //negate Charge if needed
        CheckRemoveFocus();
    }
    public virtual IEnumerator ExecuteMoveCoroutine(int level)
    {
        if (currMove == null)
        {
            moveExecuting = false;
            moveActive = false;
            yield break;
        }
        //moving = true;    //Set already
        yield return currExec = StartCoroutine(currMove.Execute(this, level)); //this looks wrong
        moveExecuting = false;
        moveActive = false;
        //negate Charge if needed
        CheckRemoveFocus();
    }
    public virtual IEnumerator ExecuteMoveCoroutine(Move m)
    {
        //Debug.Log(GetName() + " " + m.GetName());
        if (currMove == null)
        {
            moveExecuting = false;
            moveActive = false;
            yield break;
        }
        //moving = true;
        yield return currExec = StartCoroutine(m.Execute(this)); //this looks wrong
        moveExecuting = false;
        moveActive = false;
    } //note that you can make entities do arbitrary moves due to how its coded
    /*
    public virtual void ExecuteCounterBuffered(BattleEntity causer) 
    {
        //BattleControl.Instance.AddReactionMoveEvent(this, causer, currMove);
    } //adds currmove to reaction queue as a counter
    */
    public void StartMove()
    {
        if (currMove == null)
        {
            return;
        }
        currExec = StartCoroutine(currMove.Execute(this));
    }

    public void YieldMove()
    {
        moveActive = false;
    }


    public virtual IEnumerator Idle()
    {
        //Note: hopefully this will work in all cases (there shouldn't be a persistent thing that causes the animation to be wrong)
        //while (true)
        //{
            SetIdleAnimation();
            yield return null;
        //}
    }

    //Event methods
    public void QueueEvent(BattleHelper.Event eventID)
    {
        //Debug.Log(eventID + "" + ToString());
        //broadcast when event is actually executing (the out of turn stuff has to wait for the events anyway)
        //BattleControl.Instance.BroadcastEvent(this, eventID);
        eventQueue.Add(eventID);
        //if this is the only event on the queue, do it immediately
        if (!inEvent && eventQueue.Count == 1)
        {
            //Debug
            //Debug.Log("EntityEvent: " + eventID);

            StartCoroutine(DoEvent(eventID));
            eventQueue.RemoveAt(0);
        }
    }
    //  execute event (called in Update when event queue exists)
    public virtual IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler(eventID));
    }

    //Default event handlers
    public IEnumerator DefaultEventHandler(BattleHelper.Event eventID)
    {
        if (idleRunning)
        {
            StopCoroutine("Idle");
        }

        inEvent = true;

        IsAlive();

        //Debug.Log(eventID);

        BattleControl.Instance.BroadcastEvent(this, eventID);

        switch (eventID)
        {
            case BattleHelper.Event.StatusDeath:
            case BattleHelper.Event.Death:
                SetAnimation("dead");
                if (GetEntityProperty(EntityProperties.KeepEffectsAtNoHP))
                {
                    CureDeathCurableEffects();
                }
                yield return StartCoroutine(DefaultDeathEvent());
                break;
            case BattleHelper.Event.HiddenHurt:
                break;
            case BattleHelper.Event.ComboHurt:
                SetAnimation("hurt", true);
                //yield return StartCoroutine(Shake(0.1f, 0.05f));
                inCombo = true;
                break;
            case BattleHelper.Event.StatusHurt:
            case BattleHelper.Event.Hurt:
                SetAnimation("hurt", true);
                yield return StartCoroutine(DefaultHurtEvent());
                SetIdleAnimation();
                break;
            case BattleHelper.Event.KnockbackHurt:
                SetAnimation("hurt", true);
                yield return StartCoroutine(DefaultKnockbackHurt(false));
                SetIdleAnimation();
                break;
            case BattleHelper.Event.MetaKnockbackHurt:
                SetAnimation("hurt", true);
                yield return StartCoroutine(DefaultKnockbackHurt(true));
                SetIdleAnimation();
                break;
            case BattleHelper.Event.CureStatus:
                yield return StartCoroutine(DefaultCureEffect());
                break;
            case BattleHelper.Event.Heal:
                yield return StartCoroutine(DefaultHealEvent());                
                break;
            case BattleHelper.Event.Revive:
                yield return StartCoroutine(DefaultReviveEvent());
                break;
        }
        yield return null;

        //BattleControl.Instance.BroadcastEvent(this, eventID);

        inEvent = false;

        //by default the idle script is stopped
        if (!idleRunning && hasIdle && idleActive)
        {
            StartCoroutine("Idle");
        }
    }

    public IEnumerator DefaultEventHandler_Flying(BattleHelper.Event eventID)
    {
        if (idleRunning)
        {
            StopCoroutine("Idle");
        }

        inEvent = true;

        IsAlive();

        //Debug.Log(eventID);

        BattleControl.Instance.BroadcastEvent(this, eventID);

        switch (eventID)
        {
            case BattleHelper.Event.StatusDeath:
            case BattleHelper.Event.Death:
                SetAnimation("dead");
                if (GetEntityProperty(EntityProperties.KeepEffectsAtNoHP))
                {
                    CureDeathCurableEffects();
                }
                yield return StartCoroutine(DefaultDeathEvent());
                break;
            case BattleHelper.Event.HiddenHurt:
                break;
            case BattleHelper.Event.ComboHurt:
                //yield return StartCoroutine(Shake(0.1f, 0.05f));
                inCombo = true;
                break;
            case BattleHelper.Event.StatusHurt:
                SetAnimation("hurt");
                yield return StartCoroutine(DefaultHurtEvent());
                SetIdleAnimation();
                break;
            case BattleHelper.Event.Hurt:
                SetAnimation("hurt");
                yield return StartCoroutine(DefaultHurtEvent());
                yield return StartCoroutine(FlyingFallDown());
                SetIdleAnimation();
                break;
            case BattleHelper.Event.KnockbackHurt:
                SetAnimation("hurt");
                yield return StartCoroutine(DefaultKnockbackHurt(false));
                yield return StartCoroutine(FlyingFallDown());
                SetIdleAnimation();
                break;
            case BattleHelper.Event.MetaKnockbackHurt:
                SetAnimation("hurt");
                yield return StartCoroutine(DefaultKnockbackHurt(true));
                yield return StartCoroutine(FlyingFallDown());
                SetIdleAnimation();
                break;
            case BattleHelper.Event.CureStatus:
                yield return StartCoroutine(DefaultCureEffect());
                break;
            case BattleHelper.Event.Heal:
                yield return StartCoroutine(DefaultHealEvent());
                break;
            case BattleHelper.Event.Revive:
                yield return StartCoroutine(DefaultReviveEvent());
                break;
        }
        yield return null;

        inEvent = false;

        //by default the idle script is stopped
        if (!idleRunning && hasIdle && idleActive)
        {
            StartCoroutine("Idle");
        }
    }

    public IEnumerator FlyingFlyBackUp()
    {
        //fly back up        
        float yPos = BattleHelper.GetDefaultPosition(posId).y;
        if (yPos == 0)
        {
            yPos = BattleHelper.GetDefaultPosition(posId + 10).y;
        }
        homePos.y = yPos;
        yield return StartCoroutine(Move(homePos));
        SetEntityProperty(BattleHelper.EntityProperties.Grounded, false);
    }

    public IEnumerator FlyingFallDown()
    {
        if (homePos.y > 0)
        {
            //Fall down
            float dist = homePos.y;
            homePos.y = 0;
            yield return StartCoroutine(Jump(homePos, 0, dist / 8));
            SetEntityProperty(BattleHelper.EntityProperties.Grounded, true);
        }
    }


    public virtual void Update() //dispatches events in sequence
    {
        if (!inEvent && eventQueue.Count > 0)
        {
            //Debug.Log("EntityEvent: " + eventQueue[0]);

            //do an event
            StartCoroutine(DoEvent(eventQueue[0]));
            //inEvent = true;
            eventQueue.RemoveAt(0);
        }

        DropShadowUpdate();
    }

    public virtual void LateUpdate()
    {
        sameFrameHealEffects = 0;
    }

    public virtual IEnumerator DefaultKnockbackHurt(bool meta)
    {
        HideHPBar();
        BattleEntity target = BattleControl.Instance.GetKnockbackTarget(this);

        if (heavy)
        {
            target = null;
        }

        if (target == null)
        {
            yield return StartCoroutine(Spin(Vector3.up * 360, 0.25f));
        }
        else
        {
            Vector3 tpos = target.transform.position + target.height * 0.5f * Vector3.up + ((width + target.width) / 2) * Vector3.left;
            float delta = Mathf.Abs(tpos.x - transform.position.x);
            yield return StartCoroutine(JumpHeavy(tpos, 0.25f, 0.075f * delta + 0.20f, -0.25f));

            int damage = lastDamageTaken / 2;
            if (damage < 1 && lastDamageTaken > 0)
            {
                damage = 1;
            }
            ulong properties = (ulong)BattleHelper.DamageProperties.Static;
            if (meta)
            {
                properties |= (ulong)BattleHelper.DamageProperties.MetaKnockback;
            }
            //damage
            DealDamage(target, damage, 0, properties);
            yield return StartCoroutine(JumpHeavy(homePos, 2, 0.5f, -0.25f));
        }
    }

    public virtual IEnumerator DefaultDeathEvent()
    {
        alive = false;
        BattleControl.Instance.AddToDeathList(this);
        RemoveAllEffects();
        BattleControl.Instance.RemoveEntityAtId(posId); //create consistent death timing
        yield return StartCoroutine(Spin(Vector3.up * 360, 0.5f));
        BattleControl.Instance.CreateDeathSmoke(this);
        BattleControl.Instance.DropExperience(this);
        yield return StartCoroutine(Spin(Vector3.left * 90, 0.125f)); //may have to remove this for midair entities
        HideHPBar();
        BattleControl.Instance.RemoveFromDeathList(this);

        if (!perfectKill)
        {
            BattleControl.Instance.perfectKillSatisfied = false;
        }

        //Debug.Log("Perfect kill? " + (perfectKill ? "true" : "false"));

        Destroy(gameObject);
    }

    public virtual IEnumerator DefaultHurtEvent()
    {
        inCombo = false;
        yield return StartCoroutine(Shake(0.5f, 0.05f));
        if (!BattleControl.Instance.showHPBars)
        {
            HideHPBar();
        }
    }

    public virtual IEnumerator DefaultCureEffect()
    {
        yield return StartCoroutine(Jump(homePos, 0.5f, 0.25f));
    }

    public virtual IEnumerator DefaultHealEvent()
    {
        yield return new WaitForSeconds(0.25f);
        if (!BattleControl.Instance.showHPBars)
        {
            HideHPBar();
        }
    }

    public virtual IEnumerator DefaultReviveEvent()
    {
        SetRotation(Vector3.zero);
        yield return StartCoroutine(Jump(homePos, 0.5f, 0.25f));
    }


    //before executing a reaction
    //This is where you put the particle thing in?
    //Also where some of the logic behind preventing multiple counters for the same attack should go
    public virtual IEnumerator PreReact(Move move, BattleEntity target)
    {
        yield return null;
    }

    public void Effect_ReactionCounter()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/React/Effect_ReactCounter"), gameObject.transform);
        eo.transform.position = ApplyScaledOffset(stompOffset) + Vector3.down * 1;
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }
    public void Effect_ReactionDefend()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/React/Effect_ReactDefend"), gameObject.transform);
        eo.transform.position = ApplyScaledOffset(stompOffset) + Vector3.down * 1;
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }
    public void Effect_ReactionAttack()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/React/Effect_ReactAttack"), gameObject.transform);
        eo.transform.position = ApplyScaledOffset(stompOffset) + Vector3.down * 1;
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }

    //called for every event
    //return true if you invoke some kind of reaction event (i.e. if the entity reacts to an event it should add something to the reaction queue and return true)
    //This is how enemies can have their own, natural counters
    //(Note: this is supposed to add a move to the reaction queue if you are reacting to the event)
    public virtual bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        return false;
    }

    //Movement subroutines
    //yield return WaitForSeconds(0.5f)
    //yield return StartCoroutine(MovementMethod()) (replace name with right method)
    //Note: Rotation and scale stuff only touches the subobject (rotating the real object is not really ever needed)

    //how long does the move coroutine take
    public float GetMoveTime(Vector3 position)
    {
        if (transform.position == position)
        {
            return 0;
        }

        return (position - transform.position).magnitude / entitySpeed;
    }
    public float GetMoveTime(Vector3 position, float speed)
    {
        if (transform.position == position)
        {
            return 0;
        }

        return (position - transform.position).magnitude / speed;
    }

    public IEnumerator Move(Vector3 position, bool animate = true, bool animateEnd = true) //Move position based on speed (per frame).
    {
        if (transform.position == position)
        {
            yield break;
        }

        bool frameOne = false;

        if (animate)
        {
            yield return null;
            if (flipDefault)
            {
                if ((transform.position - position).x < 0)
                {
                    SendAnimationData("xunflip");
                }
            }
            else
            {
                if ((transform.position - position).x > 0)
                {
                    SendAnimationData("xflip");
                }
            }
            SetAnimation("walk");
        }

        while (true)
        {
            if (transform.position == position)
            {
                break;
            }

            //try moving
            Vector3 diff = position - transform.position;
            if (diff.magnitude > entitySpeed * Time.deltaTime)
            {
                diff = diff.normalized * entitySpeed * Time.deltaTime;
                transform.position += diff;
                frameOne = true;
                yield return null; //Keep going
            }
            else
            {
                transform.position = position;
                break;
            }
        }

        if (!frameOne)
        {
            yield return null;
        }

        if (animate && animateEnd)
        {
            if (flipDefault)
            {
                SendAnimationData("xflip");
            }
            else
            {
                SendAnimationData("xunflip");
            }
            SetIdleAnimation();
        }
    }
    public IEnumerator Move(Vector3 position, float speed, bool animate = true, bool animateEnd = true) //Move position based on speed (per frame).
    {
        yield return StartCoroutine(Move(position, speed, "walk", animate, animateEnd));
    }
    public IEnumerator Move(Vector3 position, float speed, string anim, bool animate = true, bool animateEnd = true) //Move position based on speed (per frame).
    {
        if (transform.position == position)
        {
            yield break;
        }

        bool frameOne = false;

        if (animate)
        {
            yield return null;
            if (flipDefault)
            {
                if ((transform.position - position).x < 0)
                {
                    SendAnimationData("xunflip");
                }
            }
            else
            {
                if ((transform.position - position).x > 0)
                {
                    SendAnimationData("xflip");
                }
            }
            SetAnimation(anim);
        }

        while (true)
        {
            if (transform.position == position)
            {
                break;
            }

            //Debug.Log(entityID + " " + transform.position);

            //try moving
            Vector3 diff = position - transform.position;
            if (diff.magnitude > speed * Time.deltaTime)
            {
                diff = diff.normalized * speed * Time.deltaTime;
                transform.position += diff;
                frameOne = true;
                yield return null; //Keep going
            }
            else
            {
                transform.position = position;
                break;
            }
        }

        if (!frameOne)
        {
            yield return null;
        }

        if (animate && animateEnd)
        {
            if (flipDefault)
            {
                SendAnimationData("xflip");
            }
            else
            {
                SendAnimationData("xunflip");
            }
            SetIdleAnimation();
        }
    }

    public IEnumerator Jump(Vector3 targetPos, float height, float duration, string upAnim, string downAnim, bool animate = true, bool animateEnd = true)
    {
        yield return null;
        float initialTime = Time.time;
        Vector3 initialPos = transform.position;
        Vector3 midPos = Vector3.Lerp(initialPos, targetPos, 0.5f) + height * Vector3.up;
        float completion = (Time.time - initialTime) / duration;
        //make a bezier curve

        while (completion < 1)
        {
            Vector3 pastPos = transform.position;
            transform.position = MainManager.BezierCurve(completion, initialPos, midPos, targetPos);
            if (animate)
            {
                if (flipDefault)
                {
                    if ((transform.position - pastPos).x > 0)
                    {
                        SendAnimationData("xunflip");
                    }
                }
                else
                {
                    if ((transform.position - pastPos).x < 0)
                    {
                        SendAnimationData("xflip");
                    }
                }
                if ((transform.position - pastPos).y > 0)
                {
                    SetAnimation(upAnim);
                }
                else
                {
                    SetAnimation(downAnim);
                }
            }
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }
        //force position = endpoint (prevent lag glitches)
        transform.position = targetPos;

        if (animate && animateEnd)
        {
            if (flipDefault)
            {
                SendAnimationData("xflip");
            }
            else
            {
                SendAnimationData("xunflip");
            }
            SetIdleAnimation();
        }
    }
    public IEnumerator Jump(Vector3 targetPos, float height, float duration, bool animate = true, bool animateEnd = true)
    {
        yield return StartCoroutine(Jump(targetPos, height, duration, "jump", "fall", animate, animateEnd));
    }
    public IEnumerator JumpHeavy(Vector3 targetPos, float height, float duration, float heaviness, string upAnim, string downAnim, bool animate = true, bool animateEnd = true)  //-1.0 - 1.0, negative = slow start, heavy end, positive = heavy start, slow end. Values outside the -1.0 to 1.0 range create overshooting
    {
        yield return null;
        float initialTime = Time.time;
        Vector3 initialPos = transform.position;
        Vector3 midPos = Vector3.Lerp(initialPos, targetPos, 0.5f) + height * Vector3.up;
        float completion = (Time.time - initialTime) / duration;
        float falsecompletion = MainManager.EasingQuadratic(completion, heaviness);
        //make a bezier curve

        while (completion < 1)
        {
            Vector3 pastPos = transform.position;
            transform.position = MainManager.BezierCurve(falsecompletion, initialPos, midPos, targetPos);
            if (animate)
            {
                if (flipDefault)
                {
                    if ((transform.position - pastPos).x > 0)
                    {
                        SendAnimationData("xunflip");
                    }
                }
                else
                {
                    if ((transform.position - pastPos).x < 0)
                    {
                        SendAnimationData("xflip");
                    }
                }
                if ((transform.position - pastPos).y > 0)
                {
                    SetAnimation(upAnim);
                }
                else
                {
                    SetAnimation(downAnim);
                }
            }
            completion = (Time.time - initialTime) / duration;
            falsecompletion = completion * (1f + heaviness) + completion * completion * -heaviness;
            yield return null;
        }
        //force position = endpoint (prevent lag glitches)
        transform.position = targetPos;

        if (animate && animateEnd)
        {
            if (flipDefault)
            {
                SendAnimationData("xflip");
            }
            else
            {
                SendAnimationData("xunflip");
            }
            SetIdleAnimation();
        }
    }
    public IEnumerator JumpHeavy(Vector3 targetPos, float height, float duration, float heaviness, bool animate = true, bool animateEnd = true)  //-1.0 - 1.0, negative = slow start, heavy end, positive = heavy start, slow end. Values outside the -1.0 to 1.0 range create overshooting
    {
        yield return StartCoroutine(JumpHeavy(targetPos, height, duration, heaviness, "jump", "fall", animate, animateEnd));        
    }
    public IEnumerator Jump(Vector3 targetPos, float height, bool animate = true, bool animateEnd = true) //calculate duration from speed and x diff
    {
        Vector3 initialPos = transform.position;
        float duration = (targetPos[0] - initialPos[0]) / entitySpeed;
        if (duration < 0)
        {
            duration = -duration;
        }
        yield return StartCoroutine(Jump(targetPos, height, duration, animate, animateEnd));
    }
    //calculates a jump from 100% to whenever you hit the ground
    public IEnumerator ExtrapolateJumpHeavy(Vector3 startPos, Vector3 targetPos, float height, float duration, float heaviness, string upAnim, string downAnim, bool animate = true, bool animateEnd = true)
    {
        float initialTime = Time.time;
        Vector3 initialPos = startPos;
        Vector3 midPos = Vector3.Lerp(initialPos, targetPos, 0.5f) + height * Vector3.up;
        float completion = 1 + ((Time.time - initialTime) / duration);
        float falsecompletion = MainManager.EasingQuadratic(completion, heaviness);
        //make a bezier curve

        while (true)
        {
            Vector3 pastPos = transform.position;
            transform.position = MainManager.BezierCurve(falsecompletion, initialPos, midPos, targetPos);
            if (animate)
            {
                if (flipDefault)
                {
                    if ((transform.position - pastPos).x > 0)
                    {
                        SendAnimationData("xunflip");
                    }
                }
                else
                {
                    if ((transform.position - pastPos).x < 0)
                    {
                        SendAnimationData("xflip");
                    }
                }
                if ((transform.position - pastPos).y > 0)
                {
                    SetAnimation(upAnim);
                }
                else
                {
                    SetAnimation(downAnim);
                }
            }
            completion = 1 + ((Time.time - initialTime) / duration);
            falsecompletion = MainManager.EasingQuadratic(completion, heaviness);

            //force position = endpoint (prevent lag glitches)
            if (transform.position.y < 0)
            {
                transform.position += Vector3.down * transform.position.y;
                if (animate && animateEnd)
                {
                    if (flipDefault)
                    {
                        SendAnimationData("xflip");
                    }
                    else
                    {
                        SendAnimationData("xunflip");
                    }
                    SetIdleAnimation();
                }
                yield break;
            }
            yield return null;
        }
    }
    public IEnumerator ExtrapolateJumpHeavy(Vector3 startPos, Vector3 targetPos, float height, float duration, float heaviness, bool animate = true, bool animateEnd = true)
    {
        yield return StartCoroutine(ExtrapolateJumpHeavy(startPos, targetPos, height, duration, heaviness, "jump", "fall", true, true));
    }
    //calculates a jump from 100% to whenever you hit the ground
    public IEnumerator ExtrapolateJump(Vector3 startPos, Vector3 targetPos, float height, bool animate = true, bool animateEnd = true)
    {
        yield return ExtrapolateJump(startPos, targetPos, height, "jump", "fall", animate, animateEnd);
    }
    public IEnumerator ExtrapolateJump(Vector3 startPos, Vector3 targetPos, float height, string upAnim, string downAnim, bool animate = true, bool animateEnd = true)
    {
        float initialTime = Time.time;
        Vector3 initialPos = startPos;
        Vector3 midPos = Vector3.Lerp(initialPos, targetPos, 0.5f) + height * Vector3.up;
        float duration = (targetPos[0] - initialPos[0]) / entitySpeed;
        if (duration < 0)
        {
            duration = -duration;
        }

        float completion = 1 + ((Time.time - initialTime) / duration);
        //make a bezier curve

        while (true)
        {
            Vector3 pastPos = transform.position;
            transform.position = MainManager.BezierCurve(completion, initialPos, midPos, targetPos);
            if (animate)
            {
                if (flipDefault)
                {
                    if ((transform.position - pastPos).x > 0)
                    {
                        SendAnimationData("xunflip");
                    }
                }
                else
                {
                    if ((transform.position - pastPos).x < 0)
                    {
                        SendAnimationData("xflip");
                    }
                }
                if ((transform.position - pastPos).y > 0)
                {
                    SetAnimation(upAnim);
                }
                else
                {
                    SetAnimation(downAnim);
                }
            }
            completion = 1 + ((Time.time - initialTime) / duration);

            //force position = endpoint (prevent lag glitches)
            if (transform.position.y < 0)
            {
                transform.position += Vector3.down * transform.position.y;
                if (animate && animateEnd)
                {
                    if (flipDefault)
                    {
                        SendAnimationData("xflip");
                    }
                    else
                    {
                        SendAnimationData("xunflip");
                    }
                    SetIdleAnimation();
                }
                yield break;
            }
            yield return null;
        }
    }

    public IEnumerator FollowBezierCurve(float duration, params Vector3[] points) //First point is always the initial position!
    {
        float initialTime = Time.time;
        Vector3 initialPos = transform.position;
        float completion = (Time.time - initialTime) / duration;
        //make a bezier curve

        while (completion < 1)
        {
            transform.position = MainManager.BezierCurve(completion, points);
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }
        //force position = endpoint (prevent lag glitches)
        transform.position = points[points.Length - 1];
    }
    public IEnumerator FollowBezierCurve(float duration, Func<float, float> easing, params Vector3[] points) //Use a function to determine t (make t go up non-linearly)
    {
        float initialTime = Time.time;
        Vector3 initialPos = transform.position;
        float completion = (Time.time - initialTime) / duration;
        //make a bezier curve

        float amount;

        while (completion < 1)
        {
            amount = easing.Invoke(completion);
            if (amount < 0 || amount > 1)
            {
                //Debug.LogWarning("Easing function returned value outside normal bounds.");
            }
            transform.position = MainManager.BezierCurve(amount, points);
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }
        //force position = endpoint (prevent lag glitches)
        transform.position = points[points.Length - 1];
    }
    public IEnumerator FollowBezierCurve(float duration, Func<float, float> easing, float cutoff, params Vector3[] points) //Use a function to determine t (make t go up non-linearly)
    {
        float initialTime = Time.time;
        Vector3 initialPos = transform.position;
        float completion = (Time.time - initialTime) / duration;
        //make a bezier curve

        float amount;

        while (completion < cutoff)
        {
            amount = easing.Invoke(completion);
            if (amount < 0 || amount > 1)
            {
                //Debug.LogWarning("Easing function returned value outside normal bounds.");
            }
            transform.position = MainManager.BezierCurve(amount, points);
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }
        //force position = endpoint (prevent lag glitches)
        transform.position = points[points.Length - 1];
    }
    public IEnumerator FollowBezierCurveUntilGround(float duration, float startCompletion, params Vector3[] points)
    {
        float initialTime = Time.time;
        Vector3 initialPos = transform.position;
        float completion = startCompletion + (Time.time - initialTime) / duration;
        //make a bezier curve

        while (true)
        {
            transform.position = MainManager.BezierCurve(completion, points);
            completion = startCompletion + (Time.time - initialTime) / duration;

            //force position = endpoint (prevent lag glitches)
            if (transform.position.y < 0)
            {
                transform.position += Vector3.down * transform.position.y;
                yield break;
            }

            yield return null;
        }
    }
    public IEnumerator FollowBezierCurveUntilGround(float duration, float startCompletion, Func<float, float> easing, params Vector3[] points)
    {
        float initialTime = Time.time;
        Vector3 initialPos = transform.position;
        float completion = startCompletion + (Time.time - initialTime) / duration;
        //make a bezier curve

        float amount;

        while (true)
        {
            amount = easing.Invoke(completion);
            transform.position = MainManager.BezierCurve(amount, points);
            completion = startCompletion + (Time.time - initialTime) / duration;

            //force position = endpoint (prevent lag glitches)
            if (transform.position.y < 0)
            {
                transform.position += Vector3.down * transform.position.y;
                yield break;
            }

            yield return null;
        }
    }
    public IEnumerator SmoothScale(float duration, Vector3 scale) //smoothly scale from old to new
    {
        float initialTime = Time.time;
        Vector3 initialScale = subObject.transform.localScale;
        float completion = (Time.time - initialTime) / duration;

        while (completion < 1)
        {
            subObject.transform.localScale = Vector3.Lerp(initialScale, scale, completion);
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }
        //force position = endpoint (prevent lag glitches)
        subObject.transform.localScale = scale;
    }
    public IEnumerator SmoothScale(float duration, Func<float, float> easing, Vector3 scale) //smoothly scale from old to new
    {
        float initialTime = Time.time;
        Vector3 initialScale = subObject.transform.localScale;
        float completion = (Time.time - initialTime) / duration;
        float amount;
        while (completion < 1)
        {
            amount = easing.Invoke(completion);
            if (amount < 0 || amount > 1)
            {
                //Debug.LogWarning("Easing function returned value outside normal bounds.");
            }

            subObject.transform.localScale = Vector3.LerpUnclamped(initialScale, scale, amount);
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }
        //force position = endpoint (prevent lag glitches)
        subObject.transform.localScale = scale;
    }
    //SmoothScale but with preset scaling
    public IEnumerator Squish(float duration, float deltaScale)
    {
        Vector3 newScale = Vector3.right * (1 + deltaScale) + Vector3.up * (1 - deltaScale) + Vector3.forward;
        yield return StartCoroutine(SmoothScale(duration, newScale));
    }
    //SmoothScale but with scale = 1
    public IEnumerator RevertScale(float duration)
    {
        yield return StartCoroutine(SmoothScale(duration, Vector3.one));
    }

    public IEnumerator Spin(Vector3 angle, float duration, bool animate = true) //euler angle rotation
    {
        float initialTime = Time.time;
        Vector3 initialAngle = subObject.transform.eulerAngles;
        float completion = (Time.time - initialTime) / duration;

        while (completion < 1)
        {
            if (animate)
            {
                float newangle = (initialAngle + angle * completion).y;
                if ((newangle > 90 || newangle < -90) && (newangle < 270 && newangle > -270))
                {
                    if (flipDefault)
                    {
                        SendAnimationData("xunflip");
                    }
                    else
                    {
                        SendAnimationData("xflip");
                    }
                    subObject.transform.eulerAngles = initialAngle + angle * completion + Vector3.up * 180;
                }
                else
                {
                    if (flipDefault)
                    {
                        SendAnimationData("xflip");
                    }
                    else
                    {
                        SendAnimationData("xunflip");
                    }
                    subObject.transform.eulerAngles = initialAngle + angle * completion;
                }
            } else
            {
                subObject.transform.eulerAngles = initialAngle + angle * completion;
            }
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }
        if (flipDefault)
        {
            SendAnimationData("xflip");
        }
        else
        {
            SendAnimationData("xunflip");
        }
        //force position = endpoint (prevent lag glitches)
        //Debug.Log("end");
        subObject.transform.eulerAngles = initialAngle + angle;
    }
    public IEnumerator Spin(Vector3 angle, Func<float, float> easing, float duration, bool animate = true) //euler angle rotation
    {
        float initialTime = Time.time;
        Vector3 initialAngle = subObject.transform.eulerAngles;
        float completion = (Time.time - initialTime) / duration;
        float amount;

        while (completion < 1)
        {
            amount = easing.Invoke(completion);
            if (amount < 0 || amount > 1)
            {
                //Debug.LogWarning("Easing function returned value outside normal bounds.");
            }

            if (animate)
            {
                float newangle = (initialAngle + angle * completion).y;
                if ((newangle > 90 || newangle < -90) && (newangle < 270 && newangle > -270))
                {
                    if (flipDefault)
                    {
                        SendAnimationData("xunflip");
                    }
                    else
                    {
                        SendAnimationData("xflip");
                    }
                    subObject.transform.eulerAngles = initialAngle + angle * completion + Vector3.up * 180;
                }
                else
                {
                    if (flipDefault)
                    {
                        SendAnimationData("xflip");
                    }
                    else
                    {
                        SendAnimationData("xunflip");
                    }
                    subObject.transform.eulerAngles = initialAngle + angle * completion;
                }
            }
            else
            {
                subObject.transform.eulerAngles = initialAngle + angle * completion;
            }
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }
        if (flipDefault)
        {
            SendAnimationData("xflip");
        }
        else
        {
            SendAnimationData("xunflip");
        }
        //force position = endpoint (prevent lag glitches)
        //Debug.Log("end");
        subObject.transform.eulerAngles = initialAngle + angle;
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        float initialTime = Time.time;
        float completion = (Time.time - initialTime) / duration;
        Vector3 initialPos = subObject.transform.position;
        Vector3 offset;

        while (completion < 1)
        {
            offset = UnityEngine.Random.insideUnitSphere;
            offset *= magnitude;
            subObject.transform.position = initialPos + offset;
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }

        subObject.transform.position = initialPos;
    }
    public IEnumerator Shake(float duration, Vector3 magnitude)
    {
        float initialTime = Time.time;
        float completion = (Time.time - initialTime) / duration;
        Vector3 initialPos = subObject.transform.position;
        Vector3 offset;

        while (completion < 1)
        {
            offset = UnityEngine.Random.insideUnitSphere;
            offset = offset[0] * magnitude[0] * Vector3.right + offset[1] * magnitude[1] * Vector3.up + offset[2] * magnitude[2] * Vector3.forward;
            subObject.transform.position = initialPos + offset;
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }

        subObject.transform.position = initialPos;
    }

    //Instantaneous methods (not coroutines)
    public void Warp(Vector3 point) //position shift
    {
        transform.position = point;
    }
    public void SetRotation(Vector3 angle)
    {
        subObject.transform.eulerAngles = angle;
    }
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    //Text related methods
    public virtual string RequestTextData(string request)
    {
        return "";
    }

    public virtual void SendTextData(string data)
    {

    }

    public virtual Vector3 GetTextTailPosition()
    {
        return transform.position + Vector3.up * height;
    }

    public virtual void EnableSpeakingAnim()
    {
        isSpeaking = true;
        SetAnimation("talk", true);
    }
    public virtual bool SpeakingAnimActive()
    {
        return isSpeaking;
    }
    public virtual void DisableSpeakingAnim()
    {
        isSpeaking = false;
        SetIdleAnimation(true); //note: no way to check for what the real last anim was at this point (probably won't matter)
    }

    public virtual void SetIdleAnimation(bool force = false)
    {
        if (!alive)
        {
            SetAnimation("dead", true);
            return;
        }
        if (HasEffect(Effect.EffectType.Freeze) || HasEffect(Effect.EffectType.TimeStop))
        {
            SetAnimation("idlefrozen", true);
        }
        else if (HasEffect(Effect.EffectType.Sleep))
        {
            SetAnimation("idlesleep", true);
        }
        else if (HasEffect(Effect.EffectType.Dizzy) || HasEffect(Effect.EffectType.Dread))
        {
            SetAnimation("idledizzy", true);
        }
        else if (HasEffect(Effect.EffectType.Berserk) || HasEffect(Effect.EffectType.Sunflame))
        {
            SetAnimation("idleangry", true);
        }
        else if (HasEffect(Effect.EffectType.Poison) || HasEffect(Effect.EffectType.Paralyze) || HasEffect(Effect.EffectType.Soulbleed) || HasEffect(Effect.EffectType.Exhausted))
        {
            SetAnimation("idleweak", true);
        }
        else
        {
            SetAnimation("idle", true);
        }
    }

    public virtual void SetAnimation(string name, bool force = false)
    {
        //Debug.Log(this.name + " animation " + name);
        if (ac != null)
        {
            ac.SetAnimation(name, force);
        }
    }
    public virtual void SendAnimationData(string data)
    {
        //Debug.Log(name + " animation data " + data);
        if (ac != null)
        {
            ac.SendAnimationData(data);
        }
    }


    public virtual void TextBleep()
    {

    }

    public virtual void SetFacing(Vector3 facingTarget)
    {
    }

    public virtual void EmoteEffect(TagEntry.Emote emote)
    {
        switch (emote)
        {
            case TagEntry.Emote.Alert:
                Particle_Alert();
                break;
            case TagEntry.Emote.Question:
                Particle_Miss();
                break;
            case TagEntry.Emote.AngryFizzle:
                Particle_GiveUp();
                break;
        }
    }

    public void Particle_Alert()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Effect_Enemy_Alert"), gameObject.transform);
        eo.transform.position = transform.position;
        if (height != 0)
        {
            eo.transform.position += Vector3.down + Vector3.up * height;
        }
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }
    public void Particle_Miss()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Effect_Enemy_Miss"), gameObject.transform);
        eo.transform.position = transform.position;
        if (height != 0)
        {
            eo.transform.position += Vector3.down + Vector3.up * height;
        }
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }
    public void Particle_GiveUp()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Effect_Enemy_GiveUp"), gameObject.transform);
        eo.transform.position = transform.position;
        if (height != 0)
        {
            eo.transform.position += Vector3.down + Vector3.up * height;
        }
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }


    public void OnDrawGizmosSelected()
    {
        DebugDrawHitbox();
    }

    //reveals the hidden values with Debug.DrawRay
    public void DebugDrawHitbox()
    {
        DebugDrawX(transform.position, Color.white);
        DebugDrawX(transform.position + offset + Vector3.up * (height/2), new Color(0.8f, 0.8f, 0.8f));
        DebugDrawX(subObject.transform.position, new Color(0.5f, 0.5f, 0.5f));

        DebugDrawX(transform.position + healthBarOffset + Vector3.down * 0.15f, new Color(0.8f, 0.8f, 0));
        DebugDrawX(transform.position + statusOffset + new Vector3(width * 0.5f + 0.1f, height, 0), new Color(0.8f, 0, 0.8f));
        DebugDrawX(transform.position + selectionOffset + Vector3.up * (height + 0.5f), new Color(0, 0, 0.8f));

        //hitbox is bottom centered on transform position

        Vector3 bottomLeft = transform.position + Vector3.left * 0.5f * width;
        Vector3 topLeft = bottomLeft + Vector3.up * height;
        Vector3 bottomRight = bottomLeft + Vector3.right * width;

        Debug.DrawRay(bottomLeft, Vector3.right * width, new Color(0, 1, 0), 1f, false);
        Debug.DrawRay(topLeft, Vector3.right * width, new Color(0, 1, 0), 1f, false);
        Debug.DrawRay(bottomLeft, Vector3.up * height, new Color(0, 1, 0), 1f, false);
        Debug.DrawRay(bottomRight, Vector3.up * height, new Color(0, 1, 0), 1f, false);
    }

    public void DebugDrawX(Vector3 position, Color color, float size = 0.1f)
    {
        Vector3 posA = position + size * 0.5f * (Vector3.left + Vector3.down);
        Vector3 posB = position + size * 0.5f * (Vector3.left + Vector3.up);
        Vector3 posC = position + size * 0.5f * (Vector3.right + Vector3.up);
        Vector3 posD = position + size * 0.5f * (Vector3.right + Vector3.down);

        Vector3 deltaA = posC - posA;
        Vector3 deltaB = posD - posB;

        Debug.DrawRay(posA, deltaA, color, 1f, false);
        Debug.DrawRay(posB, deltaB, color, 1f, false);
    }


    public T GetOrAddComponent<T>() where T : Component
    {
        if (gameObject.GetComponent<T>())
            return gameObject.GetComponent<T>();
        else
            return gameObject.AddComponent<T>() as T;
    }
}
