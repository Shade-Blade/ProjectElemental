using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//list where you can add and remove elements while still being able to iterate over all of them exactly once
//Can result in endless loops if you keep adding to it
[System.Serializable]
public class SafeList<T>
{
    [System.Serializable]
    public class SafeListEntry
    {
        public T inner;
        public bool b;

        public SafeListEntry(T p_inner)
        {
            inner = p_inner;
            b = false;
        }
        public SafeListEntry(T p_inner, bool p_b)
        {
            inner = p_inner;
            b = p_b;
        }
    }

    [SerializeField]
    private List<SafeListEntry> entryList; //the T gets stored inside the entries without <>?

    /*
    [SerializeField]
    private List<T> innerlist;
    [SerializeField]
    private List<bool> boollist; //used for iteration
    */
    public int Count {
        get {
            return entryList.Count;
        }
    }

    public List<T> InnerList
    {
        get
        {
            return new List<T>(entryList.ConvertAll(t => t.inner)); //another medium-depth copy
        }
    }

    public SafeList() {
        entryList = new List<SafeListEntry>();
    }    
    public SafeList(List<T> list)
    {
        entryList = new List<SafeListEntry>();
        for (int i = 0; i < entryList.Count; i++)
        {
            entryList.Add(new SafeListEntry(list[i]));
        }
    }
    public SafeList(List<T> list, List<bool> blist)
    {
        entryList = new List<SafeListEntry>();
        if (list.Count != blist.Count)
        {
            throw new InvalidOperationException("Mismatched counts in parameters.");
        }
        entryList = new List<SafeListEntry>();
        for (int i = 0; i < entryList.Count; i++)
        {
            entryList.Add(new SafeListEntry(list[i], blist[i]));
        }
    } //medium depth copy: all elements are identical but list is not
    public SafeList(SafeList<T> list)
    {
        entryList = new List<SafeListEntry>();
        for (int i = 0; i < list.Count; i++)
        {
            entryList.Add(new SafeListEntry(list[i],list.GetBool(i)));
        }
    }

    public T this[int index]
    {
        get => entryList[index].inner;
        set => entryList[index].inner = value;
    }
    public bool GetBool(int index)
    {
        return entryList[index].b;
    }

    public void Add(T o, bool set = false)
    {
        entryList.Add(new SafeListEntry(o, set));
    }
    public void Insert(int i, T o, bool set = false)
    {
        entryList.Insert(i, new SafeListEntry(o, set));
    }
    public void RemoveAt(int index)
    {
        entryList.RemoveAt(index);
    }
    public bool Remove(T o) //bool indicates whether removal worked
    {
        for (int i = 0; i < Count; i++)
        {
            if (o.Equals(entryList[i].inner))
            {
                RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    public List<SafeListEntry> FindAll(Predicate<SafeListEntry> p)
    {
        return entryList.FindAll(p);
    }
    public List<T> FindAll(Predicate<T> p)
    {
        //lambda shenanigans
        return entryList.FindAll((e) => (p.Invoke(e.inner))).ConvertAll((e) => (e.inner));
    }
    public int FindIndex(T o)
    {
        for (int j = 0; j < Count; j++)
        {
            if (entryList[j].inner.Equals(o)) //== does not work here
            {
                return j;
            }
        }
        return -1;
    }

    public void Sort(Comparer<T> c)
    {
        Comparer<SafeListEntry> c2 = Comparer<SafeListEntry>.Create((a, b) => (c.Compare(a.inner, b.inner)));
        //use built in sort
        entryList.Sort(c2);
    }

    //iteration stuff
    public void SetBools(bool set = false)
    {
        for (int i = 0; i < entryList.Count; i++)
        {
            entryList[i].b = set;
        }
    }
    public void SetBools(Predicate<T> p, bool set = true) //set all that satisfy condition
    {
        bool p2(SafeListEntry a) => (p.Invoke(a.inner));
        for (int i = 0; i < entryList.Count; i++)
        {
            if (p2(entryList[i]))
            {
                entryList[i].b = set;
            }
        }
    }
    public T next() //find next false, set it to true
    {
        if (!hasNext())
        {
            return default;
        }
        for (int i = 0; i < entryList.Count; i++)
        {
            if (!entryList[i].b)
            {
                entryList[i].b = true;
                return entryList[i].inner;
            }
        }
        //hasNext() should return false if there is something that can be enumerated
        throw new InvalidOperationException();
    }
    public bool hasNext()
    {
        for (int i = 0; i < entryList.Count; i++)
        {
            if (!entryList[i].b)
            {
                return true;
            }
        }
        return false;
    }

    public bool Contains(T b)
    {
        for (int i = 0; i < entryList.Count; i++)
        {
            if (entryList[i].inner.Equals(b))
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        string output = "";
        for (int i = 0; i < entryList.Count; i++)
        {
            output += "(";
            output += entryList[i].inner.ToString();
            output += ", " + entryList[i].b;
            output += ")";
            if (i < entryList.Count - 1)
            {
                output += ",";
            }
        }
        return output;
    }
}

//Circle buffer (useful for storing input history)
[System.Serializable]
public class CircleBuffer<T>
{
    public int pointerHead;     //Index in internalList of the last added element (Elements after are from long before)
    public List<T> internalList;
    public int Size             //Size of buffer
    {
        get => internalList.Count;
    }

    public CircleBuffer(int size) {
        internalList = new List<T>();
        for (int i = 0; i < size; i++)
        {
            internalList.Add(default); //default = T (C# is weird)
        }
    }

    //put a thing into the buffer
    public void Push(T input)
    {
        pointerHead++;
        if (pointerHead > Size - 1)
        {
            pointerHead = 0;
        }

        internalList[pointerHead] = input;
    }

    //put a thing into the buffer, but backwards (Avoid using both versions of push)
    public void ReversePush(T input)
    {
        pointerHead--;
        if (pointerHead < 0)
        {
            pointerHead = Size - 1;
        }

        internalList[pointerHead] = input;
    }

    public T this[int index]
    {
        get => Get(index);
    }

    //How many indices off from the head element
    //so 0 = head element, -1 = previous, +1 = future (*set long before!)
    //You can access elements higher than the size, it wraps around (but that is not very useful)
    public T Get(int deltaIndex)    
    {
        int trueIndex = deltaIndex + pointerHead;

        int loopCount = 0;

        while (trueIndex < 0)
        {
            loopCount++;
            trueIndex += Size;      //below 0, (-1) is next but has to become (size - 1)
        }

        while (trueIndex > Size - 1)
        {
            loopCount++;
            trueIndex -= Size;      //after (size - 1),  (size) is next but has to become 0
        }

        if (loopCount > 1)
        {
            Debug.LogWarning("Avoid accessing a circle buffer with an index with magnitude larger than size (" + deltaIndex + " accessing buffer of size " + Size + ")");
        }

        return internalList[trueIndex];
    }
}

/*
public class MoveListEntry
{
    public BattleEntity user;
    public Move move;
    public int temporalID;     //1 = first, 2+ = multiple

    public MoveListEntry(BattleEntity p_user, Move p_move)
    {
        user = p_user;
        move = p_move;
        temporalID = 1;
    }

    public MoveListEntry(BattleEntity p_user, Move p_move, int p_temporalID)
    {
        user = p_user;
        move = p_move;
        temporalID = p_temporalID;
    }
}
*/

public class ReactionMoveListEntry
{
    public BattleEntity user;       //Theoretically can be null ("world" reactions) (but in that case the move must be built specifically to handle that)
    public BattleEntity target;     //Target of the reaction (usually the causer) (can be null, sometimes this is also the user which is roughly equivalent)
    public Move move;
    public bool ignoreImmobile;               //event will trigger regardless of mobility
    public bool isItem;             //item based (and so the move will be an ItemMove)

    public ReactionMoveListEntry(BattleEntity p_user, BattleEntity p_target, Move p_move, bool p_ignoreImmobile = false, bool p_isItem = false)
    {
        user = p_user;
        target = p_target;
        move = p_move;
        ignoreImmobile = p_ignoreImmobile;
        isItem = p_isItem;
    }
}

public class BattlePopup
{
    public BattleEntity target;
    public string text;
    public string[] vars;

    //public Status status;

    public BattlePopup(BattleEntity p_target, string p_text, string[] p_vars = null)
    {
        target = p_target;
        text = p_text;
        vars = p_vars;
    }

    public BattlePopup(BattleEntity p_target, Effect status)
    {
        //the status popups


        vars = new string[12];

        //vars[0] = (int)status.effect + "";

        string effect = (int)status.effect + "";

        vars[0] = status.duration + "";
        vars[1] = status.potency + "";

        vars[2] = (10 * status.potency) + "";   //sleep/poison proportion
        vars[3] = (2 * status.potency) + "";      //sleep/poison low end
        vars[4] = (10 * status.potency) + "";   //sleep/poison high end (Note: currently the same as var[2] but I am too lazy to change this)
        vars[5] = MainManager.Percent(((status.potency) / (2f + (status.potency))), 1) + "";   //mist wall damage reduction
        vars[6] = Effect.ILLUMINATE_CAP + "";
        vars[7] = MainManager.Percent(((status.potency) / (8f)), 1) + "";  //Soulbleed's damage over time proportion thing (take ?% of your damage as Damage Over Time)
        vars[8] = (5 + (5 * status.potency)) + "";    //Sunflame damage received percentage
        vars[9] = (1 + (1 * status.potency)) + "";    //Sunflame low end
        vars[10] = (5 + (5 * status.potency)) + "";   //Sunflame high end

        float boost = 1;
        switch (status.potency)
        {
            case 1:
                boost = (4.00001f / 3);
                break;
            case 2:
                boost = 1.5f;
                break;
            case 3:
                boost = 2f;
                break;
            default:
                boost = status.potency - 1;
                break;
        }

        vars[11] = MainManager.Percent(boost - 1,1) + "";

        string[] entry = BattleControl.Instance.effectText[(int)(status.effect + 1)];

        text = "<effectsprite," + status.effect + "> " + entry[1];
        //text += "<rainbow><effectsprite," + vars[0] + "> " + (Status.StatusEffect)vars[0] + "</rainbow><buttonsprite,A>";
        //text += "<shaky><underlay,red><effectsprite," + vars[0] + "> " + (Status.StatusEffect)vars[0] + "</underlay></shaky><buttonsprite,B><buttonsprite,Z><buttonsprite,Start>";
        //text += "<wavy><effectsprite," + vars[0] + "> " + (Status.StatusEffect)vars[0] + "</wavy>";
        //text = "<effectsprite," + vars[0] + "> has <rainbow>weird</rainbow> effects in it <buttonsprite,sus>";

        if (status.potency == Effect.INFINITE_DURATION)
        {
            text += " X";
        }
        else
        {
            text += " " + vars[1];
        }

        if (status.duration == Effect.INFINITE_DURATION)
        {
            //text += " (Indefinite)";
        } else
        {
            if (status.duration == 1)
            {
                text += " (" + status.duration + " t)";
            }
            else
            {
                text += " (" + status.duration + " t)";
            }
        }

        text += ": ";

        string effectTextDesc = entry[2];

        string output = FormattedString.ParseVars(effectTextDesc, vars);

        text += output;
    }

    public BattlePopup(PlayerMove.CantMoveReason cantmove)
    {
        text = BattleControl.Instance.cantMoveText[(int)(cantmove + 1)][1];
    }

    public BattlePopup(BattleHelper.EnvironmentalEffect ee, float power, bool hard)
    {
        string[] entry = BattleControl.Instance.enviroEffectText[(int)ee];


        vars = new string[14];

        //var 0,1 = every 1 turn values (lose X every Y turns)
        //var 2,3 = every 2 turn values
        //var 4,5 = every 3 turn values
        //var 6,7 = every 0.5 turn values
        //var 8,9 = (3 * e1t, 3 * e2t)
        //var 10,11 = (6 * e1t, 6 * e2t)
        //var 12,13 = (12 * e1t, 12 * e2t)

        (int a, int b) = BattleControl.EnviroXEveryYTurns(1, power);
        vars[0] = a.ToString();
        vars[1] = b.ToString();

        vars[8] = (a * 3).ToString();
        vars[10] = (a * 6).ToString();
        vars[12] = (a * 12).ToString();

        (a, b) = BattleControl.EnviroXEveryYTurns(2, power);
        vars[2] = a.ToString();
        vars[3] = b.ToString();

        vars[9] = (a * 3).ToString();
        vars[11] = (a * 6).ToString();
        vars[13] = (a * 12).ToString();

        (a, b) = BattleControl.EnviroXEveryYTurns(3, power);
        vars[4] = a.ToString();
        vars[5] = b.ToString();

        (a, b) = BattleControl.EnviroXEveryYTurns(0.5f, power);
        vars[6] = a.ToString();
        vars[7] = b.ToString();

        //state enum has all the enviro effects in it so this works
        text = "<statesprite," + ee + "> " + entry[1];

        text += ": ";

        string effectTextDesc = hard ? entry[3] : entry[2];

        string output = FormattedString.ParseVars(effectTextDesc, vars);

        text += output;
    }
}

public class BattleStartArguments
{
    public int firstStrikePosId = FIRSTSTRIKE_NULL_ENTITY;
    public FirstStrikeMove move;
    public List<VariableEntry> variableList;
    public Vector3 position;    //Overworld position (for the fadeout thing)
    public string mapData;
    public ulong properties;    //BattleHelper.BattleProperties

    public const int FIRSTSTRIKE_NULL_ENTITY = int.MinValue;
    public const int FIRSTSTRIKE_FRONTMOST_ALLY = int.MinValue + 1;
    public const int FIRSTSTRIKE_FRONTMOST_ENEMY = int.MaxValue;

    //public object startVariables;

    //(future thing may be to add special variables)
    //(these will have to be enemy specific)

    public enum FirstStrikeMove
    {
        Default = 0,    //usually enemies should only have 1 first strike move
        Stomp,
        Weapon,
        DoubleJump,
        SuperJump,
        DashHop,
        Dig
    }

    public BattleStartArguments(int p_firstStrikePosId = FIRSTSTRIKE_NULL_ENTITY, FirstStrikeMove p_move = FirstStrikeMove.Default, List<VariableEntry> p_variableList = null, Vector3 p_position = default, string p_mapData = null)
    {
        firstStrikePosId = p_firstStrikePosId;
        move = p_move;
        variableList = p_variableList;
        position = p_position;
        mapData = p_mapData;
    }

    public class VariableEntry
    {
        public int posId;
        public string variable;
        public VariableEntry(int p_posId, string p_variable)
        {
            posId = p_posId;
            variable = p_variable;
        }
    }
}

public class BattleControl : MonoBehaviour
{
    private static BattleControl instance;
    public static BattleControl Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BattleControl>(); //this should work
                //do not create (because the things that check BattleControl should only exist in battle)
                //  (and if something goes wrong I don't want rogue BattleControls to be created)
                /*
                if (instance == null)
                {
                    GameObject b = new GameObject("BattleControl");
                    BattleControl c = b.AddComponent<BattleControl>();
                    instance = c;
                }
                */
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    private bool assetsLoaded = false;

    //static assets
    //to do later: find a better way to do this?
    //probably going to be really bad for ram to load everything at the same time
    //at least some of these should be loaded on the fly

    public GameObject damageEffect;
    public GameObject statDisplayer;
    public GameObject epDisplayer;
    public GameObject seDisplayer;
    public GameObject xpDisplayer;
    public GameObject expDisplayer;
    public GameObject coinDisplayer;
    public GameObject itemDisplayer;
    public GameObject battleSight;
    public GameObject statusIcon;
    public GameObject stateIcon;
    public GameObject hpbar;
    //public Sprite[] statusSprites;
    //public Sprite[] stateSprites;

    public GameObject actionCommandNice;
    public GameObject actionCommandGood;
    public GameObject actionCommandGreat;
    public GameObject actionCommandPerfect;
    public GameObject actionCommandMiss;

    public GameObject hitNormal;
    public GameObject hitNormalTriple;
    public GameObject hitLight;
    public GameObject hitLightTriple;
    public GameObject hitDark;
    public GameObject hitDarkTriple;
    public GameObject hitFire;
    public GameObject hitFireEmber;
    public GameObject hitFireEmberTriple;
    public GameObject hitWater;
    public GameObject hitWaterTriple;
    public GameObject hitAir;
    public GameObject hitAirTriple;
    public GameObject hitEarth;
    public GameObject hitEarthTriple;
    public GameObject hitPrismatic;
    public GameObject hitPrismaticTriple;
    public GameObject hitVoid;
    public GameObject hitVoidTriple;

    public GameObject effectBuff;
    public GameObject effectDebuff;
    public GameObject effectBuffPermanent;
    public GameObject effectDebuffPermanent;
    public GameObject effectStatus;
    public GameObject effectImmune;
    public GameObject effectImmuneNegative;

    public GameObject levelUpEffect;
    public GameObject xpGetEffect;
    public GameObject levelUpMenu;

    //Menu assets (used by menu scripts)
    public GameObject nameBoxBase;
    public GameObject popupBoxBase;
    public GameObject movePopupBoxBase;
    public GameObject menuBase;
    public GameObject menuEntryBase;
    public GameObject descriptionBoxBase;
    public GameObject pointerBase;
    public GameObject baseMenuOption;
    public GameObject baseMenuDescriptor;
    public GameObject baseMenuBSwap;

    //Other sprites
    public Sprite damageEffectStar;
    public Sprite heartEffect;
    public Sprite energyEffect;
    public Sprite hexagonEffect;
    public Sprite soulEffect;
    public Sprite staminaEffect;
    public Sprite coinEffect;

    public string[][] effectText;
    public string[][] cantMoveText;
    public string[][] enviroEffectText;

    public string[][] wilexText;
    public string[][] lunaText;
    public string[][] soulText;


    //stuff for later
    public List<BattleAction> tactics = new List<BattleAction>();
    public BattleAction superSwap = null;
    //public BattleAction switchCharacter = null;
    public BattleAction badgeSwap = null;
    public BattleAction ribbonSwap = null;

    public NamePopupScript movePopup;

    //some important constants (make into data files later?)
    //(Gets initialized in init)
    //this is an (?) x 5 array
    [SerializeField]
    public Color[][] hpBarColors;

    /*
    public Color hpBarColorA;
    public Color hpBarColorB;
    public Color emptyBar;
    public Color hpTextColor;
    public Color hpOutlineColor;
    */

    //public int hpColorIndex = 1;

    public ulong battleProperties;

    public int firstStrikePosId = int.MinValue;    //int.minvalue = no first strike, otherwise execute the first strike of the entity with the ID
    public BattleStartArguments.FirstStrikeMove firstStrikeMove = BattleStartArguments.FirstStrikeMove.Default;

    //these stats end up here
    public int ep;
    public int maxEP;

    public int se;
    public int maxSE;   //= max SP

    public int curseLevel = 0;  //0 = normal mode, 1 = 1.33, 2 = 1.5, 3 = 2, 4 = 2.66, 5 = 3, 6 = 4,    -1 = 0.75x
    public int startEnemyCount = 0;

    public int battleXP = 0;
    public float visualXP = 0;
    public bool perfectKillSatisfied;

    public int coinDrops;
    public Item.ItemType dropItem = Item.ItemType.None;
    public int dropItemCount;
    public bool forceDropItem;

    public int MultiSupplyUses;
    public int QuickSupplyUses;
    public int VoidSupplyUses;

    public int badgeSwapUses;

    public BattleHelper.EnvironmentalEffect enviroEffect;

    //Bonus energy types
    //These start at 0
    //the stat displayers for these only appear when needed because otherwise you have no use for them
    //Max is maxSE because having too many numbers is bad
    //public int spectralEnergy;
    //public int astralEnergy;
    //public int aetherEnergy;


    public List<GameObject> statDisplayers;

    [SerializeField]
    private SafeList<BattleEntity> entities = new SafeList<BattleEntity>();

    private List<ReactionMoveListEntry> reactionMoveList = new List<ReactionMoveListEntry>();   //List of reaction moves to invoke
    private BattleHelper.Event lastEventID;   //Last event that occured to an entity (broadcasted to all entities for special reactions)
    private BattleEntity lastEventEntity;                //Entity that was affected by last event (broadcasted to all entities for special reactions)
    private int lastEventReactions;               //Number of reactions to last event (useful if number of reactions needs to be limited)

    private List<BattlePopup> battlePopupList = new List<BattlePopup>(); //List of battle popups that are buffered
    //public BattleHelper.EffectPopupPriority maxPriorityBlocked = BattleHelper.EffectPopupPriority.SpecialProperties;    //Which effects don't appear? (effects of lower values appear except Never priority ones)

    public bool showHPBars;

    public int turnCount { get; private set; }    //starts at 1, incremented right before choose move phase
    public bool doTurnLoop { get; private set; } = false; //do turn loop
    public bool turnLoopRunning { get; private set; } = false;

    public bool interrupt;

    public BattleMapScript battleMapScript;

    public List<BattleEntity> deathAnimList = new List<BattleEntity>();

    //These are only used a few times, so keep the references the same while in the same battle
    public PlayerData playerData; //updated at end of battle

    public List<Item> usedItems = new List<Item>(); //Items you used in battle

    public EncounterData encounterData;

    private Coroutine turnCoroutine;

    public DoublePool ABTargetPool;

    public static BattleControl StartBattleStatic(BattleStartArguments bsa)
    {
        Debug.Log("Battle Start");

        BattleControl b = FindObjectOfType<BattleControl>();
        if (b == null)
        {
            GameObject newObj = new GameObject("BattleControl");
            BattleControl newBC = newObj.AddComponent<BattleControl>();
            newObj.AddComponent<PlayerTurnController>();
            newBC.LoadBattleAssets();
            newBC.StartBattle(bsa);
            return newBC;
        }
        else
        {
            if (!b.assetsLoaded)
            {
                b.LoadBattleAssets();
            }
            if (b.gameObject.GetComponent<PlayerTurnController>() == null)
            {
                b.gameObject.AddComponent<PlayerTurnController>();
            }
            b.StartBattle(bsa);
            return b;
        }
    }

    public static int GetOverkillLevel(EncounterData ed)
    {
        int maxlevel = 0;
        for (int i = 0; i < ed.encounterList.Count; i++)
        {
            BattleEntityData bed = BattleEntityData.GetBattleEntityData(ed.encounterList[i].GetEntityID());
            if (bed.level + bed.bonusXP > maxlevel)
            {
                maxlevel = bed.level + bed.bonusXP;
            }
        }
        return maxlevel;
    }

    public static bool GetProperty(ulong properties, BattleHelper.BattleProperties property, bool b = true)
    {
        //this looks wacky but it works
        //return true;
        return b != (((ulong)property & properties) == 0);
    }
    public bool GetProperty(BattleHelper.BattleProperties property, bool b = true)
    {
        //this looks wacky but it works
        //return true;
        return b != (((ulong)property & battleProperties) == 0);
    }
    public void SetProperty(BattleHelper.BattleProperties property, bool b = true)
    {
        if (b)
        {
            battleProperties |= (ulong)property;
        }
        else
        {
            battleProperties &= ~((ulong)property);
        }
        //Debug.Log(entityProperties);
    }

    public bool HarderEnviroEffects()
    {
        return curseLevel > 0;
    }
    public float EnviroEffectPower()
    {
        if (playerData.BadgeEquipped(Badge.BadgeType.WeatherShield))
        {
            return (1 / (playerData.BadgeEquippedCount(Badge.BadgeType.WeatherShield) - 0.0001f));
        }

        return 1;
    }
    public int EnviroEveryXTurns(float frequency)
    {
        return EnviroEveryXTurns(frequency, EnviroEffectPower());
    }
    public int EnviroEveryXTurns(float frequency, float eep)
    {
        //make the timings for "every turn", "every second turn", "every third turn" consistent
        //Returns: how many ticks should pass (usually 0 but if it is larger than 1 then make the effect stronger
        return EnviroEveryXTurns(frequency, eep, turnCount);
    }
    public int EnviroEveryXTurns(float frequency, float eep, int tc)
    {
        if (MainManager.Instance.Cheat_DoubleStrengthEnviroEffects)
        {
            eep *= 2;
        }
        if (MainManager.Instance.Cheat_QuadrupleStrengthEnviroEffects)
        {
            eep *= 4;
        }


        //this last one is for if you want to change tc in the formula (to make something like every X attacks or something)

        //make the timings for "every turn", "every second turn", "every third turn" consistent
        //Returns: how many ticks should pass (usually 0 but if it is larger than 1 then make the effect stronger

        float adjustedTurnCount = eep * tc;
        float higherCount = eep * (tc + 1);

        int delta = ((int)(higherCount / frequency)) - ((int)(adjustedTurnCount / frequency));

        return delta;
    }
    public static (int, int) EnviroXEveryYTurns(float frequency, float eep)    //give out a "X every Y turns" thing
    {
        float power = frequency / eep;

        int numerator = 1;
        int denominator = (int)power;

        float currentBest = (numerator / (0.0f + denominator));

        //Try to find a fraction of the form A / B very close to (1 / power)

        while (numerator < 9)
        {
            if (currentBest - power < 0.02f)
            {
                return (numerator, denominator);
            }

            numerator++;
            denominator = (int)(power * numerator);
        }

        return (numerator, denominator);
    }


    public static void SetCameraDefault(float halfLife = 0.05f)
    {
        MainManager.Instance.Camera.SetManual(new Vector3(0, 1.5f, -4.8f), new Vector3(0, 0, 0), halfLife);


        //Was 0, 2, 6.5
        //Camera vertical edges at z = 0 are y = 5.75 and y = -1.75
        //The equidistant point is at y = 2 (which is the y pos of default)
    }
    public static void SetCameraDefaultDelayed(float halfLife = 0.05f)
    {
        MainManager.Instance.Camera.SetManualDelayed(new Vector3(0, 1.5f, -4.8f), new Vector3(0, 0, 0), halfLife);


        //Was 0, 2, 6.5
        //Camera vertical edges at z = 0 are y = 5.75 and y = -1.75
        //The equidistant point is at y = 2 (which is the y pos of default)
    }
    public static void SetCameraSettings(Vector3 focus, float distance = 4.8f, float halfLife = 0.05f)
    {
        MainManager.Instance.Camera.SetManual(focus + distance * Vector3.back, new Vector3(0, 0, 0), halfLife);
    }
    public static void SetCameraSettingsDelayed(Vector3 focus, float distance = 4.8f, float halfLife = 0.05f)
    {
        MainManager.Instance.Camera.SetManualDelayed(focus + distance * Vector3.back, new Vector3(0, 0, 0), halfLife);
    }


    public BattleEntity SummonEntity(BattleHelper.EntityID eid, bool negativeID = false)
    {
        return SummonEntity(eid, FindUnoccupiedID(negativeID));
    } //finds a positional ID that is available
    public BattleEntity SummonEntity(BattleHelper.EntityID eid, int posid, string bonusData = null)
    {
        GameObject g = EnemyBuilder.BuildEnemy(eid, BattleHelper.GetDefaultPosition(posid));
        BattleEntity b = g.GetComponent<BattleEntity>();
        b.posId = posid;
        b.homePos = BattleHelper.GetDefaultPosition(posid);
        AddEntity(b);
        b.PostPositionInitialize();
        b.SetEncounterVariables(bonusData);
        return b;
    }
    public BattleEntity SummonEntity(BattleHelper.EntityID eid, int posid, Vector3 vpos, string bonusData = null)
    {
        GameObject g = EnemyBuilder.BuildEnemy(eid, vpos);
        BattleEntity b = g.GetComponent<BattleEntity>();
        b.posId = posid;
        b.homePos = vpos;
        AddEntity(b);
        b.PostPositionInitialize();
        b.SetEncounterVariables(bonusData);
        return b;
    }
    public BattleEntity SummonEntity(PlayerData.PlayerDataEntry pdataentry, int posid, Vector3 vpos)
    {
        BattleEntity b = SummonEntity(pdataentry.entityID, posid, vpos);
        b.maxHP = pdataentry.maxHP;
        b.attackMultiplier = 1;
        b.applyCurseAttack = MainManager.Instance.Cheat_PlayerCurseAttack;
        b.hp = pdataentry.hp;
        b.stamina = 0;
        b.agility = pdataentry.GetAgility(maxEP);
        if (b.hp <= 0)
        {
            b.transform.eulerAngles = Vector3.left * 90;
        }
        b.PostPositionInitialize();
        return b;
    }
    public List<BattleEntity> SummonEntities(EncounterData edata)
    {
        List<BattleEntity> output = new List<BattleEntity>();
        foreach (EncounterData.EncounterDataEntry ede in edata.encounterList)
        {
            BattleEntity b;
            if (ede.usePos)
            {
                b = SummonEntity(ede.GetEntityID(), ede.posid, ede.pos, ede.bonusdata);
            } else
            {
                b = SummonEntity(ede.GetEntityID(), ede.posid, ede.bonusdata);
            }
            output.Add(b);
        }
        return output;
    }

    public List<BattleEntity> SummonEntities(PlayerData pdata)
    {
        List<BattleEntity> output = new List<BattleEntity>();
        int posid = -1;
        if (pdata.party.Count == 1)
        {
            //Special logic
            Vector3 midPos = Vector3.left * 1.25f + Vector3.right * -1.8f + Vector3.forward * -0.075f;
            BattleEntity b = SummonEntity(pdata.party[0], posid, midPos);
            b.level = pdata.level;
            output.Add(b);
        }
        else
        {
            for (int i = 0; i < pdata.party.Count; i++)
            {
                BattleEntity b = SummonEntity(pdata.party[i], posid, BattleHelper.GetDefaultPosition(posid));
                b.level = pdata.level;
                output.Add(b);
                posid--;
            }
        }
        return output;
    }
    public void AddEntity(BattleEntity be)
    {
        entities.Add(be);
        //Sort by id
        entities.Sort(Comparer<BattleEntity>.Create((a, b) => (a.posId - b.posId)));
    }
    public int FindUnoccupiedID(bool negative)
    {
        entities.Sort(Comparer<BattleEntity>.Create((a, b) => (a.posId - b.posId)));
        int output = 0;

        if (negative)
        {
            output = -1;
            bool check = false;
            while (true)
            {
                check = true; //if you get through without finding a collision, return output
                for (int i = 0; i < entities.Count; i++)
                {
                    if (output == entities[i].posId)
                    {
                        //bad
                        break;
                    }
                    if (entities[i].posId > output)
                    {
                        //found a blank id?
                        return output;
                    }
                }
                if (check)
                {
                    return output;
                }
                output--;
            }
        } else
        {
            bool check = false;
            while (true)
            {
                check = true;
                for (int i = 0; i < entities.Count; i++)
                {
                    if (output == entities[i].posId)
                    {
                        //bad
                        check = false;
                        break;
                    }
                    if (entities[i].posId > output)
                    {
                        //found a blank id?
                        return output;
                    }
                }
                if (check)
                {
                    return output;
                }
                output++;
            }
        }
    }
    public bool RemoveEntityAtId(int posid)
    {
        //Debug.Log("Remove " + posid + "? " + (GetIndexFromID(posid) > -1) + " which is "+entities[GetIndexFromID(posid)].entityID);
        if (GetIndexFromID(posid) > -1)
        {
            BattleEntity a = entities[GetIndexFromID(posid)];
            //Debug.Log(entities);
            entities.RemoveAt(GetIndexFromID(posid));
            //Debug.Log(entities);
            //Debug.Log(entities.Contains(a));

            return true;
        } else
        {
            return false;
        }
    }
    private void RemoveEntity(int index)
    {
        entities.RemoveAt(index);
    }
    public void RemoveEntity(BattleEntity b)
    {
        entities.Remove(b);
    }

    public BattleEntity GetEntityByID(int id)
    {
        //Save some time
        if (id == int.MinValue)
        {
            return null;
        }
        for (int i = 0; i < entities.Count; i++)
        {
            if (id == entities[i].posId)
            {
                return entities[i];
            }
        }
        return null;
    }
    public int GetIndexFromID(int id)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            if (id == entities[i].posId)
            {
                return i;
            }
        }
        return -1;
    }

    //closer to center is first (assumes that purely player characters or purely enemies are in the list)
    public int StandardEntitySort(BattleEntity a, BattleEntity b)
    {
        return (a.posId < 0 ? -1 : 1) * MainManager.FloatCompare(a.homePos.x, b.homePos.x);
    }
    public bool HasEntity(BattleEntity b)
    {
        return entities.Contains(b);
    }

    public List<BattleEntity> GetEntities()
    {
        return entities.InnerList;
    }
    public List<BattleEntity> GetEntities(Predicate<BattleEntity> p)
    {
        return entities.InnerList.FindAll(p);
    }
    public List<BattleEntity> GetEntities(BattleEntity caller, TargetArea t, bool ignoreNoTarget = false) //return entities that fulfil target requirements
    {
        List<BattleEntity> list = new List<BattleEntity>();
        Func<BattleEntity, BattleEntity, bool> checker = t.checkerFunction;

        //Debug.Log(t.range);
        if (checker != null)
        {
            //lambda shenanigans
            list = GetEntities((e) => (ignoreNoTarget || !e.GetEntityProperty(BattleHelper.EntityProperties.NoTarget)) && checker.Invoke(caller, e));
        }
        return list;
    }
    public List<BattleEntity> GetEntitiesSorted()
    {
        List<BattleEntity> list = GetEntities();

        list.Sort((a, b) => StandardEntitySort(a, b));
        return list;
    }
    public List<BattleEntity> GetEntitiesSorted(Predicate<BattleEntity> p)
    {
        List<BattleEntity> list = GetEntities(p);
        list.Sort((a, b) => StandardEntitySort(a, b));
        return list;
    }
    public List<BattleEntity> GetEntitiesSorted(BattleEntity caller, TargetArea t, bool ignoreNoTarget = false) //return entities that fulfil target requirements
    {
        List<BattleEntity> list = GetEntities(caller, t, ignoreNoTarget);

        list.Sort((a, b) => StandardEntitySort(a, b));
        return list;
    }
    public List<BattleEntity> GetEntitiesSorted(BattleEntity caller, Move move, int level = 1, bool ignoreNoTarget = false)
    {
        return GetEntitiesSorted(caller, move.GetTargetArea(caller, level), ignoreNoTarget);
    }

    public List<BattleAction> GetTactics(BattleEntity caller)
    {
        List<BattleAction> ta = tactics;
        PlayerEntity t = (PlayerEntity)caller;
        if (t != null)
        {
            ta = t.tactics;
        }
        return ta;
    }

    //(use in case you can't trust b.id)
    public int FindEntityID(BattleEntity b)
    {
        if (b == null || !entities.InnerList.Contains(b))
        {
            return int.MinValue;
        } else
        {
            return b.posId;
        }
    } //if entity is not on the list, this returns int min value
    public bool EntityValid(BattleEntity b)
    {
        if (entities.InnerList.Contains(b))
        {
            return true;
        }
        return false;
    } //if an entity is in the list of entities and so can be targetted correctly

    //are there no enemies above this one? (i.e. can you jump on this)
    //this is an annoying condition though
    public bool IsTopmost(BattleEntity b)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            //don't check the other side (this topmost check is usually used for trying to target enemies)
            if (b.posId < 0 ^ entities[i].posId < 0)
            {
                continue;
            }

            //obviously don't check yourself
            if (entities[i] == b)
            {
                continue;
            }

            //use homePos because idle animations might make them move around?
            //note that enemies have a bottom center anchor
            //so that is how height and width are measured

            //X check
            if (Mathf.Abs(b.homePos.x - entities[i].homePos.x) < entities[i].width)
            {
                //Y check
                if (entities[i].homePos.y > b.homePos.y)
                {
                    //ceiling check (ceiling enemies do not trigger the topmost blocking)
                    if (!entities[i].GetEntityProperty(BattleHelper.EntityProperties.Ceiling))
                    {
                        return false;
                    }
                }
            }

            //There is no Z check because depth perception is pretty bad in this system
            //(so z shenanigans should be avoided always)
        }

        return true;
    }

    public bool IsFrontmostLow(BattleEntity caller, BattleEntity check, bool ignoreNoTarget = false)
    {
        return GetFrontmostLow(caller, ignoreNoTarget) == check;  //pointer match
    }

    public BattleEntity GetFrontmostLow(BattleEntity caller, bool ignoreNoTarget = false)
    {
        List<BattleEntity> bl = GetEntities(caller, new TargetArea(TargetArea.TargetAreaType.LiveEnemyLow), ignoreNoTarget);

        if (bl.Count == 0)
        {
            return null;
        }
        bl.Sort((a, b) => ((caller.posId < 0 ? 1 : -1) * MainManager.FloatCompare(a.homePos.x, b.homePos.x)));

        return bl[0];  //pointer match
    }
    public BattleEntity GetFrontmostAlly(int posId, bool ignoreNoTarget = false)
    {
        List<BattleEntity> bl = GetEntities((e) => (!(e.posId >= 0 ^ posId >= 0)));

        if (bl.Count == 0)
        {
            return null;
        }

        //Different sign from FrontmostLow check above because that check is done on the enemies of caller (different sign)
        //(while this check is done on entities with the same sign)
        bl.Sort((a, b) => ((posId < 0 ? -1 : 1) * MainManager.FloatCompare(a.homePos.x, b.homePos.x)));

        return bl[0];  //pointer match
    }

    public BattleEntity GetKnockbackTarget(BattleEntity caller)
    {
        BattleEntity target = null;
        List<BattleEntity> list = GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));

        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int sgn = caller.posId < 0 ? -1 : 1;
                if (list[i].posId == caller.posId)
                {
                    continue;
                }
                if (list[i].homePos.x * sgn < caller.homePos.x * sgn - caller.width / 2)
                {
                    continue;
                }
                if (list[i].homePos.y < caller.homePos.y - caller.height * 0.25f)
                {
                    continue;
                }
                if (list[i].homePos.y > caller.homePos.y + caller.height * 0.75f)
                {
                    continue;
                }
                return list[i];
            }
        }

        return target;
    }

    //swap the casterIDs of a list of entities
    //1 -> 2, 2 -> 3... n -> 1
    //Position Mark is also swapped (Character mark is not, because those effects should use references to the BattleEntity directly which will not be affected by this)
    public void SwapEffectCasters(List<BattleEntity> list)
    {
        List<int> intList = new List<int>();
        List<bool> positionMarkList = new List<bool>();

        for (int i = 0; i < list.Count; i++)
        {
            intList.Add(list[i].posId);
            positionMarkList.Add(list[i].GetEntityProperty(BattleHelper.EntityProperties.PositionMark));
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (i == list.Count - 1)
            {
                list[i].SetEntityProperty(BattleHelper.EntityProperties.PositionMark, positionMarkList[0]);
            }
            else
            {
                list[i].SetEntityProperty(BattleHelper.EntityProperties.PositionMark, positionMarkList[i + 1]);
            }
        }

        for (int i = 0; i < entities.Count; i++)
        {
            for (int j = 0; j < entities[i].effects.Count; j++)
            {
                if (entities[i].effects[j].casterID == Effect.NULL_CASTERID)
                {
                    continue;
                }

                for (int k = 0; k < intList.Count; k++)
                {
                    if (k == intList.Count - 1)
                    {
                        if (intList[k] == entities[i].effects[j].casterID)
                        {
                            entities[i].effects[j].casterID = intList[0];
                            break;
                        }
                    } else
                    {
                        if (intList[k] == entities[i].effects[j].casterID)
                        {
                            entities[i].effects[j].casterID = intList[k + 1];
                            break;
                        }
                    }
                }
            }
        }
    }

    public static int XPCalculation(int startEnemyCount, int playerLevel, int enemyLevel, int enemyBonusXP)
    {
        return MainManager.XPCalculation(startEnemyCount, playerLevel, enemyLevel, enemyBonusXP);
    }

    //moving the battle data back to the main manager
    public void EOBUpdatePlayerData()
    {
        Debug.Log("Update");
        MainManager.Instance.playerData = playerData.Copy();
        PlayerData realPlayerData = MainManager.Instance.playerData;
        //note that this does not update maxHp and maxEp
        //the level up methods can work with them independently

        //note that only entities already in the playerdata are updated
        List<BattleEntity> internalParty = GetEntities((e) => (true));

        internalParty.Sort((a, b) => -(a.posId - b.posId));

        for (int i = 0; i < internalParty.Count; i++)
        {
            //Try to find the correct value
            PlayerData.PlayerDataEntry p = realPlayerData.GetPlayerDataEntry(internalParty[i].entityID);
            if (p == null)
            {
                continue;
            }
            //Debug.Log(p.entityID + " found");
            //p.maxHp = internalParty[i].maxHP;
            p.hp = internalParty[i].hp;
            if (p.hp < 1)
            {
                p.hp = 1;
            }
            //p.statuses = internalParty[i].statuses.ConvertAll((e) => (e));
        }

        realPlayerData.ep = ep;
        realPlayerData.se = se;

        //playerData.maxEp = maxEP;
    }

    //status icons only appear on your turn
    public void ShowEffectIcons()
    {
        HideEffectIcons();  //fix problem?
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].ShowEffectIcons();
        }
    }
    public void HideEffectIcons()
    {
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].HideEffectIcons();
        }
    }

    public void ShowHPBars()
    {
        HideHPBars();   //fix problem?
        showHPBars = true;
        for (int i = 0; i < entities.Count; i++)
        {
            //Debug.Log(entities[i] + " is" + (IsPlayerControlled(entities[i]) ? " " : " not ") + "player controlled");
            if (!IsPlayerControlled(entities[i], false) && entities[i].ShouldShowHPBar())
            {
                entities[i].ShowHPBar();
            }
        }
    }
    public void HideHPBars()
    {
        showHPBars = false;
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].HideHPBar();
        }
    }

    public Color[] GetHPBarColors()
    {
        //hp bar top, hp bar bottom, buffer bar, empty bar, hp text, hp text outline
        Color[] output;

        output = hpBarColors[GetHPBarStyleIndex()];

        /*
        output[0] = hpBarColorA;
        output[1] = hpBarColorB;
        output[2] = bufferBar;
        output[3] = emptyBar;
        output[4] = hpTextColor;
        output[5] = hpOutlineColor;
        */

        if (output.Length != 6)
        {
            Debug.LogError("HP Bar style has the wrong length: " + output.Length + " != 5");
        }

        return output;
    }

    public int GetHPBarStyleIndex()
    {
        if (curseLevel + 1 >= hpBarColors.Length)
        {
            return hpBarColors.Length - 1;
        }
        if (curseLevel + 1 < 0)
        {
            return 0;
        }

        //return 1;
        return curseLevel + 1; //hpColorIndex;
    }

    public int GetCurseLevel()
    {
        return curseLevel;
    }

    public float GetCurseMultiplier()
    {
        return GetCurseMultiplier(curseLevel);
    }

    public float GetCurseMultiplier(int level)
    {
        switch (level)
        {
            case 0:
                return 1;
            case 1:
                return (4.0001f / 3f);
            case 2:
                return 1.5f;
            case 3:
                return 2f;
            case 4:
                return (8.0001f / 3f);
            case 5:
                return 3f;
            case 6:
                return 4f;
            case -1:
                return (3 / 4f);
            default:
                //extrapolate I guess
                if (level > 6)
                {
                    return GetCurseMultiplier(level - 6) * 4;
                }
                if (level < 0)
                {
                    return 1 / (GetCurseMultiplier(-level));
                }

                return 1;
        }
    }

    public int CurseMultiply(int num)
    {
        return CurseMultiply(num, GetCurseLevel());
    }
    public int CurseMultiply(int num, int level)
    {
        float a = GetCurseMultiplier(level) * num;

        int b = (int)a;

        //ceil
        if (a - b > 0.01f)
        {
            return b + 1;
        }
        else
        {
            return b;
        }
    }
    public int CurseAttackCalculation(int num)
    {
        return CurseAttackCalculation(num, GetCurseLevel());
    }
    public int CurseAttackCalculation(int num, int level)
    {
        //rules
        //curse multiplier X has to be at least 1 above curse multiplier (X - 1)        

        //0 damage is 0 damage
        if (num <= 0)
        {
            return 0;
        }

        if (level == 0) //Nop
        {
            return num;
        } else if (level > 0)
        {
            int a = CurseMultiply(num, level);

            return Mathf.Max(a, CurseAttackCalculation(num, level - 1) + 1);
        }
        else //if (level < 0)
        {
            int a = CurseMultiply(num, level);

            //enforce a 1 floor
            if (a < 1)
            {
                return 1;
            }

            int b = CurseAttackCalculation(num, level + 1) - 1;

            if (b < 1)
            {
                return 1;
            }

            return Mathf.Min(a, b);
        }
    }

    public void CureStatusesFromCaster(int casterID)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].CureEffectsFromCaster(casterID);
        }
    }
    public void CureStatusesFromCaster(int casterID, Effect.EffectType effect)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].CureEffectsFromCaster(casterID, effect);
        }
    }

    public int GetEP(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return ep;
        } else
        {
            return 0;
        }
    }
    public int GetMaxEP(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return maxEP;
        }
        else
        {
            return 0;
        }
    }
    public int GetMaxStamina(BattleEntity target)
    {
        return GetMaxEP(target) / 2;
    }
    public int AddEP(BattleEntity target, int p_ep)
    {
        if (target.posId < 0)
        {
            int overheal = 0;
            ep += p_ep;
            if (p_ep < 0)
            {
                overheal = -p_ep;
            }
            if (ep < 0)
            {
                overheal = -p_ep + ep;
                ep = 0;
            } else if (ep > maxEP)
            {
                overheal = ep - maxEP;
                ep = maxEP;
            }
            return overheal;
        }
        else
        {
            if (p_ep > 0)
            {
                return p_ep;
            } else
            {
                return 0;
            }
        }
    }
    public void SetEP(BattleEntity target, int p_ep)
    {
        if (target.posId < 0)
        {
            ep = p_ep;
            if (ep < 0)
            {
                ep = 0;
            }
            else if (ep > maxEP)
            {
                ep = maxEP;
            }
        }
        else
        {
            return;
        }
    }

    public int GetSE(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return se;
        }
        else
        {
            return 0;
        }
    }
    public int GetMaxSE(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return maxSE;
        }
        else
        {
            return 0;
        }
    }
    public int AddSE(BattleEntity target, int p_se)
    {
        if (target.posId < 0)
        {
            int overheal = 0;
            se += p_se;
            if (p_se < 0)
            {
                overheal = -p_se;
            }
            if (se < 0)
            {
                overheal = -p_se + se;
                se = 0;
            }
            else if (se > maxSE)
            {
                overheal = se - maxSE;
                se = maxSE;
            }
            return overheal;
        }
        else
        {
            if (p_se > 0)
            {
                return p_se;
            }
            else
            {
                return 0;
            }
        }
    }
    public void SetSE(BattleEntity target, int p_se)
    {
        if (target.posId < 0)
        {
            se = p_se;
            if (se < 0)
            {
                se = 0;
            }
            else if (se > maxSE)
            {
                se = maxSE;
            }
        }
        else
        {
            return;
        }
    }

    public int CountAllItemsOfType(BattleEntity target, Item.ItemType type)
    {
        List<Item> inventory = GetItemInventory(target);

        if (inventory == null)
        {
            return 0;
        }

        int c = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].type == type)
            {
                c++;
            }
        }
        return c;
    }
    public int GetMaxItemInventory(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return playerData.GetMaxInventorySize();
        }
        else
        {
            return 0;
        }
    }
    public List<Item> GetItemInventory(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return playerData.itemInventory;
        } else
        {
            return null;
        }
    }
    public List<Item> GetUsedItemInventory(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return usedItems;
        }
        else
        {
            return null;
        }
    }
    public bool AddItemInventory(BattleEntity caller, Item input)
    {
        if (caller.posId < 0)
        {
            return playerData.AddItem(input);
        }
        else
        {
            return false;
        }
    }
    public bool InsertItemInventory(BattleEntity caller, int index, Item input)
    {
        if (caller.posId < 0)
        {
            return playerData.InsertItem(index, input);
        }
        else
        {
            return false;
        }
    }

    //things shouldn't interact with coins but ehh
    public int GetCoins(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return playerData.coins;
        }
        else
        {
            return 0;
        }
    }
    public int GetMaxCoins(BattleEntity target)
    {
        if (target.posId < 0)
        {
            return PlayerData.MAX_MONEY;
        }
        else
        {
            return 0;
        }
    }
    public void AddCoins(BattleEntity target, int p_c)
    {
        if (target.posId < 0)
        {
            playerData.coins += p_c;
            if (playerData.coins < 0)
            {
                playerData.coins = 0;
            }
            else if (playerData.coins > PlayerData.MAX_MONEY)
            {
                playerData.coins = PlayerData.MAX_MONEY;
            }
        }
        else
        {
            return;
        }
    }
    public void SetCoins(BattleEntity target, int p_c)
    {
        if (target.posId < 0)
        {
            playerData.coins = p_c;
        }
        else
        {
            return;
        }
    }

    public bool AllEntitiesNotMoving()
    {
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].moveExecuting)
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckEntitiesStationary()
    {
        bool a = true;

        for (int i = 0; i < entities.Count; i++)
        {
            a &= !entities[i].moveExecuting;
        }

        for (int i = 0; i < entities.Count; i++)
        {
            a &= !entities[i].immediateInEvent;
        }

        a &= DeathListEmpty();

        return a;
    }

    public void AddToDeathList(BattleEntity b)
    {
        deathAnimList.Add(b);
    }

    public void RemoveFromDeathList(BattleEntity b)
    {
        deathAnimList.Remove(b);
    }

    public bool DeathListEmpty()
    {
        for (int i = 0; i < deathAnimList.Count; i++)
        {
            if (deathAnimList[i] == null)
            {
                deathAnimList.RemoveAt(i);
                i--;
            }
        }

        if (deathAnimList.Count == 0)
        {
            return true;
        }
        return false;
    }

    public void AbsorbFocusCheck()
    {
        for (int j = 0; j < entities.Count; j++)
        {
            entities[j].CheckRemoveFocus();
            entities[j].CheckRemoveAbsorb();
        }
    }

    public void LoadBattleAssets()
    {
        assetsLoaded = true;
        damageEffect = Resources.Load<GameObject>("VFX/Battle/Effect_Damage");
        statDisplayer = Resources.Load<GameObject>("Battle/Stat Displayer");
        xpDisplayer = Resources.Load<GameObject>("Battle/XP Displayer");
        epDisplayer = Resources.Load<GameObject>("Battle/EP Displayer");
        seDisplayer = Resources.Load<GameObject>("Battle/SE Displayer");
        expDisplayer = Resources.Load<GameObject>("Battle/EXP Displayer");
        coinDisplayer = Resources.Load<GameObject>("Battle/Coin Displayer");
        itemDisplayer = Resources.Load<GameObject>("Battle/Item Displayer");
        battleSight = Resources.Load<GameObject>("Battle/BattleSightHolder");

        statusIcon = Resources.Load<GameObject>("Battle/StatusIcon");
        stateIcon = Resources.Load<GameObject>("Battle/StateIcon");
        hpbar = Resources.Load<GameObject>("Battle/HPBar");

        //statusSprites = Resources.LoadAll<Sprite>("Sprites/Battle/EffectIconsV7");
        //stateSprites = Resources.LoadAll<Sprite>("Sprites/Battle/StateIconsV2");

        actionCommandNice = Resources.Load<GameObject>("VFX/Battle/ActionCommand/Effect_ActionCommandNice");
        actionCommandGood = Resources.Load<GameObject>("VFX/Battle/ActionCommand/Effect_ActionCommandGood");
        actionCommandGreat = Resources.Load<GameObject>("VFX/Battle/ActionCommand/Effect_ActionCommandGreat");
        actionCommandPerfect = Resources.Load<GameObject>("VFX/Battle/ActionCommand/Effect_ActionCommandPerfect");
        actionCommandMiss = Resources.Load<GameObject>("VFX/Battle/ActionCommand/Effect_ActionCommandMiss");

        hitNormal = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Normal");
        hitNormalTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_NormalTriple");

        hitLight = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Shine");
        hitLightTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_ShineTriple");
        hitDark = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Dark");
        hitDarkTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_DarkTriple");
        hitFire = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Fire");
        hitFireEmber = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_FireEmbers");
        hitFireEmberTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_FireEmbersTriple");
        hitWater = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Splash");
        hitWaterTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_SplashTriple");
        hitAir = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Spark");
        hitAirTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_SparkTriple");
        hitEarth = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Leaf");
        hitEarthTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_LeafTriple");
        hitPrismatic = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Prismatic");
        hitPrismaticTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_PrismaticTriple");
        hitVoid = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_Void");
        hitVoidTriple = Resources.Load<GameObject>("VFX/Battle/Hit/Effect_Hit_VoidTriple");

        effectBuff = Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Buff");
        effectDebuff = Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Debuff");
        effectBuffPermanent = Resources.Load<GameObject>("VFX/Battle/Effect/Effect_BuffExtended");
        effectDebuffPermanent = Resources.Load<GameObject>("VFX/Battle/Effect/Effect_DebuffExtended");
        effectStatus = Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Status");
        effectImmune = Resources.Load<GameObject>("VFX/Battle/Effect_Immune");
        effectImmuneNegative = Resources.Load<GameObject>("VFX/Battle/Effect_ImmuneNegative");

        levelUpEffect = Resources.Load<GameObject>("Battle/LevelUpEffect");
        xpGetEffect = Resources.Load<GameObject>("Battle/GetXPEffect");
        levelUpMenu = Resources.Load<GameObject>("Battle/LevelUpMenu");

        nameBoxBase = (GameObject)Resources.Load("Menu/NameBox");
        popupBoxBase = (GameObject)Resources.Load("Menu/BattlePopup");
        movePopupBoxBase = (GameObject)Resources.Load("Menu/BattleMovePopup");
        menuBase = MainManager.Instance.menuBase; //(GameObject)Resources.Load("Menu/MoveMenuBase");
        menuEntryBase = MainManager.Instance.menuEntryBase; //(GameObject)Resources.Load("Menu/MoveMenuEntry");
        descriptionBoxBase = MainManager.Instance.descriptionBoxBase; //(GameObject)Resources.Load("Menu/DescriptionBox");
        pointerBase = (GameObject)Resources.Load("Menu/SelectPointer");
        baseMenuOption = (GameObject)Resources.Load("Menu/BaseMenuOption");
        baseMenuDescriptor = (GameObject)Resources.Load("Menu/BaseMenuDescriptor");
        baseMenuBSwap = (GameObject)Resources.Load("Menu/BaseMenuBSwap");

        damageEffectStar = MainManager.Instance.damageEffectStar;
        energyEffect = MainManager.Instance.energyEffect;
        heartEffect = MainManager.Instance.heartEffect;
        hexagonEffect = MainManager.Instance.hexagonEffect;
        soulEffect = MainManager.Instance.soulEffect;
        staminaEffect = MainManager.Instance.staminaEffect;
        coinEffect = MainManager.Instance.coinEffect;

        effectText = MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/EffectText").text);
        cantMoveText = MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/CantMoveText").text);
        enviroEffectText = MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/EnviroEffectText").text);

        wilexText = MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/WilexMoveText").text);
        lunaText = MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/LunaMoveText").text);
        soulText = MainManager.CSVParse(Resources.Load<TextAsset>("DialogueText/SoulMoveText").text);

        statDisplayers = new List<GameObject>();
        SetupBattleActions();

        hpBarColors = new Color[8][];

        //to do: make this a data table
        //manually specifying everything in code is messy

        //"easy" (green/blue) (blue numbers though)
        hpBarColors[0] = new Color[] { new Color(0f, 1f, 0.57f, 1), new Color(0f, 0.80f, 0.5f, 1), new Color(0.66f, 0.91f, 0.99f, 1), new Color(0, 0.55f, 0.6f, 1), new Color(0.66f, 0.91f, 0.99f, 1), new Color(0, 0, 0, 1) };

        //"normal" (yellow/red)
        hpBarColors[1] = new Color[] { new Color(0.88f, 0.88f, 0, 1), new Color(0.85f, 0.70f, 0, 1), new Color(1, 1, 1, 1), new Color(0.8f, 0, 0, 1), new Color(1, 1, 1, 1), new Color(0, 0, 0, 1) };

        //"hard / super curse" (red/dark pink)
        hpBarColors[2] = new Color[] { new Color(1f, 0, 0, 1), new Color(0.75f, 0, 0, 1), new Color(1, 0.65f, 0.65f, 1), new Color(0.3f, 0, 0.2f, 1), new Color(1, 0.65f, 0.65f, 1), new Color(0, 0, 0, 1) };

        //"ultra curse" (gold/brown)
        hpBarColors[3] = new Color[] { new Color(1, 0.88f, 0.22f, 1), new Color(0.85f, 0.66f, 0.02f, 1), new Color(1, 1, 0.65f, 1), new Color(0.7f, 0.36f, 0, 1), new Color(1, 1, 0.65f, 1), new Color(0, 0, 0, 1) };

        //"mega curse" (cyan/dark gray)
        hpBarColors[4] = new Color[] { new Color(0, 0.9f, 0.9f, 1), new Color(0, 0.75f, 0.80f, 1), new Color(0.65f, 1, 1, 1), new Color(0.5f, 0.5f, 0.6f, 1), new Color(0.65f, 1, 1, 1), new Color(0, 0, 0, 1) };

        //(lime ish green/dark green)
        hpBarColors[5] = new Color[] { new Color(0.4f, 1f, 0, 1), new Color(0.3f, 0.8f, 0, 1), new Color(0.65f, 1, 0.65f, 1), new Color(0, 0.375f, 0, 1), new Color(0.65f, 1, 0.65f, 1), new Color(0, 0, 0, 1) };

        //(magenta/purple)
        hpBarColors[6] = new Color[] { new Color(0.9f, 0, 0.9f, 1), new Color(0.75f, 0, 0.75f, 1), new Color(1, 0.65f, 1, 1), new Color(0.35f, 0, 0.65f, 1), new Color(1, 0.65f, 1, 1), new Color(0, 0, 0, 1) };

        //(white/black) (HP number colors are black with white outline (inverted of normal))
        hpBarColors[7] = new Color[] { new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(0.5f, 0.5f, 0.5f, 1), new Color(0, 0, 0, 1), new Color(0, 0, 0, 1), new Color(1, 1, 1, 1) };
    }
    /*
    public void UnloadBattleAssets()
    {
    }
    */

    //Battle control
    public void StartBattle(BattleStartArguments bsa = null)
    {
        //Specifically start at this point in the cycle so that you get 2 front targets and 1 back target in the first part of the sequence
        //In short battles you may only see the very first part of the sequence so ensuring that it is fine is important
        ABTargetPool = new DoublePool(4, 12, 9, 12);

        coinDrops = 0;

        //set turns up
        doTurnLoop = true; //gets started by Update()
        turnCount = 0;

        //borrow references from MainManager
        playerData = MainManager.Instance.playerData.Copy();
        if (MainManager.Instance.gameOverPlayerData != null)
        {
            //Debug.Log("set");
            MainManager.Instance.playerData = MainManager.Instance.gameOverPlayerData.Copy();
        }
        MainManager.Instance.gameOverPlayerData = null;
        encounterData = MainManager.Instance.nextBattle;

        MainManager.BattleMapID bmapName = encounterData.battleMapName;
        battleMapScript = MainManager.Instance.CreateMap(bmapName);

        playerData.totalBattles++;

        //get player party
        ep = playerData.ep;
        se = playerData.se;
        maxEP = playerData.maxEP;
        maxSE = playerData.maxSE;
        SummonEntities(playerData);

        //apply curses
        curseLevel = 0;

        if (playerData.BadgeEquipped(Badge.BadgeType.SuperCurse))
        {
            curseLevel += 1 * playerData.BadgeEquippedCount(Badge.BadgeType.SuperCurse);
        }
        if (playerData.BadgeEquipped(Badge.BadgeType.UltraCurse))
        {
            curseLevel += 2 * playerData.BadgeEquippedCount(Badge.BadgeType.UltraCurse);
        }
        if (playerData.BadgeEquipped(Badge.BadgeType.MegaCurse))
        {
            curseLevel += 3 * playerData.BadgeEquippedCount(Badge.BadgeType.MegaCurse);
        }

        perfectKillSatisfied = true;

        //spectralEnergy = 0;
        //astralEnergy = 0;
        //aetherEnergy = 0;

        //get enemy party
        SummonEntities(encounterData);

        List<BattleEntity> enemies = GetEntitiesSorted((e) => (e.entityID >= 0 && EntityCountsForBattleEnd(e)));
        startEnemyCount = enemies.Count;
        battleXP = 0;
        dropItem = enemies.Count > 0 ? enemies[0].dropItemType : Item.ItemType.None;

        if (bsa != null)
        {
            firstStrikePosId = bsa.firstStrikePosId;
            firstStrikeMove = bsa.move;

            if (bsa.variableList != null)
            {
                for (int i = 0; i < bsa.variableList.Count; i++)
                {
                    BattleEntity b = GetEntityByID(bsa.variableList[i].posId);
                    if (b != null)
                    {
                        b.SetVariables(bsa.variableList[i].variable);
                    }
                }
            }
        }


        RebuildStatDisplayers();
        showHPBars = false;

        /*
        SummonEntity(BattleHelper.EntityID.Mage, -2);
        SummonEntity(BattleHelper.EntityID.Knight, -1);
        SummonEntity(BattleHelper.EntityID.DebugEntity, 0);
        SummonEntity(BattleHelper.EntityID.DebugEntity, 1);
        SummonEntity(BattleHelper.EntityID.DebugEntity, 2);
        */

        //do this after stuff is summoned so I can mess with entities in the map scripts
        if (battleMapScript != null)
        {
            battleMapScript.OnBattleStart();
        }
    }

    public static bool EntityCountsForBattleEnd(BattleEntity b)
    {
        if (b.GetEntityProperty(BattleHelper.EntityProperties.NoCount))
        {
            return false;
        }

        if (b.IsAlive())
        {
            return true;
        }

        if (b.GetEntityProperty(BattleHelper.EntityProperties.CountAtZero) && b.hp <= 0)
        {
            return true;
        }

        return false;
    }
    public IEnumerator CheckEndBattle() //check after every possible way battle can end (but not during moves since that looks weird) (Waits for events to end)
    {
        if (MainManager.Instance.Cheat_BattleWin)
        {
            yield return new WaitUntil(() => CheckEntitiesStationary());
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => DeathListEmpty());
            yield return StartCoroutine(EndBattle(BattleHelper.BattleOutcome.Win));
            yield break;
        }


        if (entities.FindAll(e => EntityCountsForBattleEnd(e) && IsPlayerControlled(e, true)).Count == 0) //no live players
        {
            if (entities.FindAll(e => EntityCountsForBattleEnd(e) && e.posId >= 0).Count == 0) //no live enemies
            {
                yield return new WaitUntil(() => CheckEntitiesStationary());
                yield return new WaitForSeconds(0.1f);
                yield return new WaitUntil(() => DeathListEmpty());
                yield return StartCoroutine(EndBattle(BattleHelper.BattleOutcome.Tie)); //how?
            }
            else
            {
                yield return new WaitUntil(() => CheckEntitiesStationary());
                yield return StartCoroutine(EndBattle(BattleHelper.BattleOutcome.Death));
            }
        }
        else if (entities.FindAll(e => EntityCountsForBattleEnd(e) && e.posId >= 0).Count == 0)
        {
            yield return new WaitUntil(() => CheckEntitiesStationary());
            yield return new WaitForSeconds(0.1f);
            yield return new WaitUntil(() => DeathListEmpty());
            yield return StartCoroutine(EndBattle(BattleHelper.BattleOutcome.Win));
        }

        yield return null;
    } //end battle under conditions
    public IEnumerator EndBattle(BattleHelper.BattleOutcome outcome)
    {
        DestroyMovePopup();

        //don't need them anymore
        GlobalItemScript.Instance.ClearItemMoves();

        Debug.Log("End battle with outcome " + outcome + " (Turn "+turnCount +")");
        if (turnCoroutine != null) //unity is programmed weird and this is actually executed before the startcoroutine returns sometimes
            StopCoroutine(turnCoroutine);
        turnLoopRunning = false;
        doTurnLoop = false; //we are done

        //wait until animations done
        yield return new WaitUntil(() => CheckEntitiesStationary());

        //Move camera

        //tally max damage per turn
        int perTurnDamage = 0;
        List<PlayerEntity> pel = GetPlayerEntities();
        for (int i = 0; i < pel.Count; i++)
        {
            perTurnDamage += pel[i].perTurnDamageDealt;
        }

        switch (outcome)
        {
            case BattleHelper.BattleOutcome.Win:
                playerData.battlesWon++;
                break;
            case BattleHelper.BattleOutcome.Flee:
                playerData.battlesFled++;
                break;
            case BattleHelper.BattleOutcome.Death:
            case BattleHelper.BattleOutcome.Tie:
                //these will be filled with wrong data in the case of a loss
                for (int i = 0; i < statDisplayers.Count; i++)
                {
                    Destroy(statDisplayers[i].gameObject);
                }
                playerData.battlesLost++;
                MainManager.Instance.playerData.battlesLost++;
                break;
        }

        for (int i = 0; i < entities.Count; i++)
        {
            yield return StartCoroutine(entities[i].PostBattle());
        }

        if (outcome == BattleHelper.BattleOutcome.Win)    //Only win state revives you
        {
            List<PlayerEntity> pe = GetPlayerEntities();
            for (int i = 0; i < pe.Count; i++)
            {
                pe[i].CureAllEffects();
                if (pe[i].hp < 1)
                {
                    pe[i].HealHealth(1);
                }
            }
        }

        yield return new WaitUntil(() => CheckEntitiesStationary());

        if (outcome == BattleHelper.BattleOutcome.Win)
        {
            yield return StartCoroutine(BattleVictory());
        }

        //fix player data

        //note: death/tie will not
        //so your state will effectively revert to before the battle (cutscene handlers may override by setting you to 1 hp though)
        //  This way battles that kick you out to the overworld instead of game overing are not paradoxically worse
        //  (due to the existence of the "retry battle" menu on the game over screen)
        switch (outcome)
        {
            case BattleHelper.BattleOutcome.Flee:
                if (!GetProperty(battleProperties, BattleHelper.BattleProperties.FleeResetStats))
                {
                    EOBUpdatePlayerData();
                    Item.ApplyVolatile();
                }
                break;
            case BattleHelper.BattleOutcome.Exit:
            case BattleHelper.BattleOutcome.Win:
                EOBUpdatePlayerData();
                Item.ApplyVolatile();
                break;
        }
        //MainManager.Instance.playerData = playerData;
        if (battleMapScript != null)
            battleMapScript.OnBattleEnd();

        //Try to start coroutine from mainmanager to avoid problems with this script being destroyed
        MainManager.Instance.StartCoroutine(MainManager.Instance.ReturnFromBattle(outcome, GetProperty(BattleHelper.BattleProperties.CanRetry)));

        //get stuck in a loop until main manager destroys this
        while (true)
        {
            yield return null;
        }
    }
    public IEnumerator BattleVictory()
    {
        //The "you got X xp" cutscene
        //If you level up then go to the level up cutscene

        float averageX = 0;
        List<PlayerEntity> pel = GetPlayerEntities();
        for (int i = 0; i < pel.Count; i++)
        {
            averageX += pel[i].transform.position.x;
            pel[i].SetAnimation("battlewin", true);
        }
        averageX /= pel.Count;
        //Pan camera over
        SetCameraSettingsDelayed(new Vector3(averageX, 0.85f, 0f), 2.4f, 0.125f);

        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < pel.Count; i++)
        {
            pel[i].SetIdleAnimation();
        }

        //xp gained

        bool decisive = false;
        bool perfect = false;
        if (playerData.BadgeEquipped(Badge.BadgeType.DecisiveVictory))
        {
            if (turnCount <= 1)
            {
                decisive = true;
            }
        }
        if (playerData.BadgeEquipped(Badge.BadgeType.PerfectVictory))
        {
            if (perfectKillSatisfied)
            {
                perfect = true;
            }
        }

        if (decisive)
        {
            battleXP *= (1 + playerData.BadgeEquippedCount(Badge.BadgeType.DecisiveVictory));
            coinDrops *= (1 + playerData.BadgeEquippedCount(Badge.BadgeType.DecisiveVictory));
        }
        if (perfect)
        {
            battleXP = Mathf.CeilToInt(battleXP * (1 + 0.5f * (1 + playerData.BadgeEquippedCount(Badge.BadgeType.PerfectVictory))));
            coinDrops = Mathf.CeilToInt(coinDrops * (1 + 0.5f * (1 + playerData.BadgeEquippedCount(Badge.BadgeType.PerfectVictory))));
        }

        if (battleXP < 0)
        {
            battleXP = 0;
        }
        if (battleXP > 100)
        {
            battleXP = 100;
        }

        //Drop item?
        //current logic: 1/4 base, but x2 if it dropped 0 xp, x2 if you have itemfinder
        //  <= 4 makes 1/2, <= 2 makes guaranteed
        //  Super/ultra curse makes 1/2, mega curse makes guaranteed

        int itemFinder = playerData.BadgeEquippedCount(Badge.BadgeType.ItemFinder);

        int battleBoost = 0;
        if (playerData.totalBattles % 2 == 0)
        {
            battleBoost = 1;
        }
        if (playerData.totalBattles % 4 == 0)
        {
            battleBoost = 2;
        }

        int xpBoost = 0;
        if (battleXP == 0)
        {
            xpBoost = 1;
        }

        int lowItemBoost = 0;
        if (playerData.itemInventory.Count <= 4)
        {
            lowItemBoost = 1;
        }
        if (playerData.itemInventory.Count <= 2)
        {
            lowItemBoost = 2;
        }

        //if you use a massive curse level then you get a ton of items I guess
        int curseBoost = curseLevel >= 0 ? curseLevel : 0;

        int boostAmount = lowItemBoost + battleBoost + xpBoost + itemFinder + curseBoost;
        if (dropItemCount > 0)
        {
            dropItemCount = boostAmount / 2;
        }

        if (forceDropItem && dropItemCount <= 0)
        {
            dropItemCount = 1;
        }

        //Debug.Log("Item drop determiners: " + itemFinder + " " + checkDropItem + " " + shouldDropItem + " " + battleXP + " " + playerData.totalBattles);

        Debug.Log((perfect ? "Perfect: " : "") + (decisive ? "Decisive: " : "") + "Win battle " + playerData.totalBattles + " with " + battleXP + " xp gained. Drop item? " + dropItemCount);

        yield return new WaitUntil(() => (visualXP == battleXP));

        //You got X XP!
        //playerData.exp += battleXP;

        GameObject xpg = Instantiate(xpGetEffect, MainManager.Instance.Canvas.transform);
        GetXPEffectScript xpgs = xpg.GetComponent<GetXPEffectScript>();
        //level was already increased so the right side is [level], left side is [level - 1]

        int fortunePower = 0;
        foreach (CharmEffect c in playerData.charmEffects)
        {
            if (c.charmType == CharmEffect.CharmType.Fortune)
            {
                fortunePower = c.charges;
            }
        }

        xpgs.Setup(battleXP, perfect, decisive, fortunePower);

        yield return new WaitUntil(() => xpgs == null);

        /*
        yield return new WaitForSeconds(0.5f);

        int oldXP = battleXP;
        battleXP = 0;   //drain all the XP?
        StartCoroutine(ExpScroll(oldXP));

        IEnumerator ExpScroll(int oldXP)
        {
            int oldPDExp = playerData.exp;
            while (visualXP != battleXP)
            {
                playerData.exp = oldPDExp + (oldXP - (int)visualXP);
                yield return null;
            }
            playerData.exp = oldPDExp + oldXP;
        }

        yield return new WaitUntil(() => (visualXP == battleXP));
        */

        if (playerData.exp >= 100)
        {
            playerData.exp -= 100;
            playerData.level += 1;
            SetCameraDefault(0.125f);
            yield return StartCoroutine(LevelUp());
        }

        yield return null;
    }
    public IEnumerator LevelUp()
    {
        List<PlayerEntity> pe = GetPlayerEntities();

        bool healdone = false;
        IEnumerator LevelUpHeal()
        {            
            for (int i = 0; i < pe.Count; i++)
            {
                pe[i].HealHealth(pe[i].maxHP);
            }
            yield return new WaitForSeconds(0.5f);
            pe[0].HealEnergy(maxEP);
            yield return new WaitForSeconds(0.5f);
            pe[0].HealSoulEnergy(maxSE);
            healdone = true;
        }

        bool wtpA = false;
        bool wtpB = false;
        IEnumerator WalkToPositionTrackedA(BattleEntity be, Vector3 position)
        {
            yield return StartCoroutine(be.Move(position));
            be.SetAnimation("levelup");
            wtpA = true;
        }
        IEnumerator WalkToPositionTrackedB(BattleEntity be, Vector3 position)
        {
            yield return StartCoroutine(be.Move(position));
            be.SetAnimation("levelup");
            wtpB = true;
        }

        //do this in parallel
        StartCoroutine(LevelUpHeal());

        //do this in parallel also
        if (pe.Count == 1)
        {
            Vector3 targetPos = Vector3.zero;
            StartCoroutine(WalkToPositionTrackedA(pe[0], targetPos));
            wtpB = true;
        } else
        {
            Vector3[] targetPositions = new Vector3[pe.Count];

            //wacky way of making this work with 3 characters (but I'm not going to have 3 characters anyway)
            for (int i = 0; i < pe.Count; i++)
            {
                targetPositions[i] = Vector3.left * 5 + (Vector3.right * i * (10 / (pe.Count - 1)));
                StartCoroutine(WalkToPositionTrackedA(pe[0], targetPositions[i]));
                if (i == pe.Count - 1)
                {
                    StartCoroutine(WalkToPositionTrackedB(pe[1], targetPositions[i]));
                }
            }
        }

        GameObject lue = Instantiate(levelUpEffect, MainManager.Instance.Canvas.transform);
        LevelUpEffectScript lues = lue.GetComponent<LevelUpEffectScript>();
        //level was already increased so the right side is [level], left side is [level - 1]
        lues.Setup(playerData.level - 1, playerData.level, (playerData.level >= PlayerData.GetMaxLevel()));

        yield return new WaitUntil(() => lue == null);
        yield return new WaitUntil(() => healdone && wtpA && wtpB);

        //Now do the actual choice thing
        GameObject lum = Instantiate(levelUpMenu, MainManager.Instance.Canvas.transform);
        LevelUpMenuScript lums = lum.GetComponent<LevelUpMenuScript>();
        lums.Setup(playerData);
        yield return new WaitUntil(() => lums == null);

        //recalculate max stats, heal you to your new max (actually the level up thing does this on its own)

        //Other level up bonuses (unlocked moves)
        string levelupText = "";
        if (playerData.level == 3)
        {
            if (levelupText.Length > 0)
            {
                levelupText += "<next>";
            }
            levelupText += "Wilex can now use Slip Slash!";
        }

        if (playerData.level == 6)
        {
            if (levelupText.Length > 0)
            {
                levelupText += "<next>";
            }
            levelupText += "Luna can now use Hammer Throw!";
        }

        if (playerData.level == 12)
        {
            if (levelupText.Length > 0)
            {
                levelupText += "<next>";
            }
            levelupText += "Luna can now use Meteor Stomp!";
        }

        if (playerData.level == 15)
        {
            if (levelupText.Length > 0)
            {
                levelupText += "<next>";
            }
            levelupText += "Wilex can now use Double Stomp!";
        }

        if (playerData.level == 21)
        {
            if (levelupText.Length > 0)
            {
                levelupText += "<next>";
            }
            levelupText += "Wilex can now use Team Quake!";
        }

        if (playerData.level == 21)
        {
            if (levelupText.Length > 0)
            {
                levelupText += "<next>";
            }
            levelupText += "Luna can now use Team Throw!";
        }

        if (levelupText.Length > 0)
        {
            yield return StartCoroutine(MainManager.Instance.DisplayTextBox(levelupText, null));
        }

        //Wait for the healing effect to be done (also wait for the level up bonuses)
        yield return null;
    }
    public void Destroy()
    {
        for (int i = 0; i < statDisplayers.Count; i++)
        {
            Destroy(statDisplayers[i].gameObject);
        }

        Destroy(battleMapScript.gameObject);
    }

    public void SetupBattleActions()
    {
        /*
        tactics.Add(Instance.gameObject.AddComponent<Check>());
        tactics.Add(Instance.gameObject.AddComponent<SwapEntities>());
        tactics.Add(Instance.gameObject.AddComponent<Rest>());
        tactics.Add(Instance.gameObject.AddComponent<Flee>());
        tactics.Add(Instance.gameObject.AddComponent<TurnRelay>());
        */

        superSwap = PlayerTurnController.Instance.GetOrAddComponent<BA_SuperSwapEntities>();
        badgeSwap = PlayerTurnController.Instance.GetOrAddComponent<BA_BadgeSwap>();
        ribbonSwap = PlayerTurnController.Instance.GetOrAddComponent<BA_RibbonSwap>();
        //switchCharacter = Instance.gameObject.AddComponent<SwitchCharacter>();
    }

    //also create effects?
    public void DropExperience(BattleEntity be)
    {
        int coinsDropped = Mathf.CeilToInt(be.level * be.moneyMult / 2f);   //15-20 ish per encounter in a chapter 1 state (Should be enough to buy everything you need)
        //Balancing note
        //Roughly 20 ish encounters per chapter (???)
        //20 ish coints per encounter in chapter 1 is enough to buy 1 item per encounter
        //  So if you need to grind 10 items you fight 10 encounters (but you will probably do that anyway)
        //  You will probably have surplus but that is good because you need that to buy badges and other expensive stuff

        if (playerData.BadgeEquipped(Badge.BadgeType.MoneyBoost))
        {
            coinsDropped = Mathf.CeilToInt(coinsDropped * (1 + playerData.BadgeEquippedCount(Badge.BadgeType.MoneyBoost)));
        }

        if (be.dropItemType != Item.ItemType.None)
        {
            //Dropped item is based on the front enemy (this should be the one that appears in the overworld)
            //dropItem = be.dropItemType;
            dropItemCount = 1;

            if (be.moneyMult >= 2)
            {
                forceDropItem = true;
            }
        }


        int xpDropped = MainManager.XPCalculation(startEnemyCount, playerData.level, be.level, be.bonusLevel);
        //curse multiplication

        //Not what the attack calculation should be used for but it works
        //(attack calculation forces curse + 1 to have at least 1 more damage than curse)
        xpDropped = CurseAttackCalculation(xpDropped);
        coinsDropped = CurseAttackCalculation(coinsDropped);

        //Debug.Log(coinsDropped);

        bool fortuneCoins = false;

        //Fortune
        foreach (CharmEffect c in playerData.charmEffects)
        {
            if (c.charmType == CharmEffect.CharmType.Fortune)
            {
                float power = c.GetFortunePower();
                xpDropped = Mathf.CeilToInt(power * xpDropped);
                coinDrops += Mathf.CeilToInt(power * coinsDropped);
                //Debug.Log("Fortune coins: " + (power * coinsDropped));
                fortuneCoins = true;
            }
        }

        if (!fortuneCoins)
        {
            //Debug.Log("Coins: " + (coinsDropped));
            coinDrops += coinsDropped;
        }


        //Cap at 100
        if (xpDropped < 0)
        {
            xpDropped = 0;
        }
        if (xpDropped > 100)
        {
            xpDropped = 100;
        }

        //Enforce level cap
        if (playerData.level >= PlayerData.GetMaxLevel())
        {
            xpDropped = 0;
        }

        if (xpDropped > 0)
        {
            float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
            GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_Fountain_Exp"), gameObject.transform);
            eo.transform.position = be.transform.position;
            EffectScript_Fountain es_f = eo.GetComponent<EffectScript_Fountain>();
            es_f.Setup(newScale, xpDropped);
        }

        battleXP += xpDropped;
    }

    public GameObject CreateActionCommandEffect(BattleHelper.ActionCommandText a, Vector3 position, BattleEntity target = null)
    {
        //nice = yellow
        //good = lime
        //great = green
        //perfect = cyan
        //miss = gray

        GameObject o = null;// = Instantiate(damageEffect);

        switch (a)
        {
            case BattleHelper.ActionCommandText.Nice:
                o = Instantiate(actionCommandNice);
                break;
            case BattleHelper.ActionCommandText.Good:
                o = Instantiate(actionCommandGood);
                break;
            case BattleHelper.ActionCommandText.Great:
                o = Instantiate(actionCommandGreat);
                break;
            case BattleHelper.ActionCommandText.Perfect:
                o = Instantiate(actionCommandPerfect);
                break;
            case BattleHelper.ActionCommandText.Miss:
                o = Instantiate(actionCommandMiss);
                break;
            default:
                return null;
        }

        o.transform.SetParent(transform);
        o.transform.position = position;
        o.GetComponent<EffectScript_ActionCommandText>().Setup();

        if (target != null)
        {
            o.GetComponent<EffectScript_ActionCommandText>().SetDir(IsPlayerControlled(target, false));
        }

        return o;
    }
    public GameObject CreateActionCommandEffect(int successes, Vector3 position, BattleEntity target)
    {
        //note that this will never produce the "Perfect" text
        switch (successes)
        {
            case 1:
                CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, position, target);
                break;
            case 2:
                CreateActionCommandEffect(BattleHelper.ActionCommandText.Good, position, target);
                break;
            case 3:
                CreateActionCommandEffect(BattleHelper.ActionCommandText.Great, position, target);
                break;
            default:
                if (successes < 1) {
                    //create nothing
                    //CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, position, target);
                }
                else
                {
                    CreateActionCommandEffect(BattleHelper.ActionCommandText.Great, position, target);
                }
                break;
        }
        return null;
    }
    public GameObject CreateDamageEffect(BattleHelper.DamageEffect b, int s, Vector3 position, BattleEntity be)
    {
        GameObject o = Instantiate(damageEffect);
        o.transform.SetParent(transform);

        bool player = IsPlayerControlled(be, false);
        float playermult = player ? 1 : -1;
        be.sameFrameHealEffects++;

        if (be.sameFrameHealEffects == 2)
        {
            position += Vector3.right * 0.5f * playermult;
        }
        if (be.sameFrameHealEffects == 3)
        {
            position += Vector3.left * 0.5f * playermult;
        }
        if (be.sameFrameHealEffects == 4)
        {
            position += Vector3.right * 0.3f * playermult + Vector3.up;
        }
        if (be.sameFrameHealEffects == 5)
        {
            position += Vector3.left * 0.3f * playermult + Vector3.up;
        }

        o.transform.position = position;
        o.GetComponent<EffectScript_Damage>().Setup(b, s);
        return o;
    }
    /*
    public GameObject CreateDamageEffect(BattleHelper.DamageEffect b, int s, Vector3 position, BattleEntity be)
    {
        GameObject o = Instantiate(damageEffect);
        o.transform.SetParent(transform);
        o.transform.position = position;
        o.GetComponent<DamageEffectScript>().Setup(b, s);

        o.GetComponent<DamageEffectScript>().SetDir(IsPlayerControlled(be));

        return o;
    }
    */

    public void CreateHealParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_HealthSparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }
    public void CreateEnergyParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_EnergySparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }
    public void CreateSoulParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_SoulSparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }
    public void CreateStaminaParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_StaminaSparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }
    public void CreateCoinParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_CoinSparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }

    public void CreateNegativeHealParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_NegativeHealthSparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }
    public void CreateNegativeEnergyParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_NegativeEnergySparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }
    public void CreateNegativeSoulParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_NegativeSoulSparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }
    public void CreateNegativeStaminaParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_NegativeStaminaSparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }
    public void CreateNegativeCoinParticles(BattleEntity be, int power)
    {
        //int power = 2;

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position + Vector3.up * (be.height / 2);

        GameObject eo = null;
        EffectScript_Generic es_s = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_NegativeCoinSparkle"), gameObject.transform);
        eo.transform.position = position;
        es_s = eo.GetComponent<EffectScript_Generic>();
        es_s.Setup(newScale, power);
    }

    public GameObject CreateDamageEffect(BattleHelper.DamageEffect b, int s, Vector3 position, BattleEntity be, BattleHelper.DamageType type = BattleHelper.DamageType.Default, ulong properties = 0)
    {
        GameObject o = Instantiate(damageEffect);
        o.transform.SetParent(transform);
        o.transform.position = position;

        o.GetComponent<EffectScript_Damage>().Setup(b, s, type, properties);

        bool player = IsPlayerControlled(be, false);
        o.GetComponent<EffectScript_Damage>().SetDir(player);

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.SuppressHitParticles))
        {
            CreateHitEffect(s, position, be, type, properties);
        }

        return o;
    }
    public void CreateHitEffect(int s, Vector3 position, BattleEntity be, BattleHelper.DamageType type = BattleHelper.DamageType.Default, ulong properties = 0)
    {
        //Power calculation
        //0 = 0.5
        //1-6 = 1
        //7-12 = 1 to 2
        //13-18 = 0.66 to 1 with triple
        //18+ = 1 to 2 with triple

        //multitypes get proportioned (2 type -> each gets half)
        //  also clamped between 1 and 2

        //Fire will spawn multiple effects since it doesn't have a power difference (going 1,2,3,4)
        //(though it will increase the spread of those effects)

        //upper end is probably whatever lags out stuff too much

        float normal = 0;

        if (type == BattleHelper.DamageType.Default)
        {
            Debug.LogWarning("Hit effect with Default damage. Use Normal type instead.");
        }

        if (type == BattleHelper.DamageType.Normal || type == BattleHelper.DamageType.Default)
        {
            normal = numberTransformNormal(s);
            //Debug.Log("Normal = " + normal);
        }

        float light = 0;
        float dark = 0;
        float fire = 0;
        float water = 0;
        float air = 0;
        float earth = 0;

        float prismatic = 0;
        float voidd = 0;

        int typeCount = 0;

        if (((uint)type & (uint)BattleHelper.DamageType.Light) != 0)
        {
            typeCount++;
            light = s;
        }
        if (((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
        {
            typeCount++;
            dark = s;
        }
        if (((uint)type & (uint)BattleHelper.DamageType.Fire) != 0)
        {
            typeCount++;
            fire = s;
        }
        if (((uint)type & (uint)BattleHelper.DamageType.Water) != 0)
        {
            typeCount++;
            water = s;
        }
        if (((uint)type & (uint)BattleHelper.DamageType.Air) != 0)
        {
            typeCount++;
            air = s;
        }
        if (((uint)type & (uint)BattleHelper.DamageType.Earth) != 0)
        {
            typeCount++;
            earth = s;
        }
        if (((uint)type & (uint)BattleHelper.DamageType.Prismatic) != 0)
        {
            typeCount++;
            prismatic = s;
        }
        if (((uint)type & (uint)BattleHelper.DamageType.Void) != 0)
        {
            typeCount++;
            voidd = s;
        }

        if (typeCount > 0)
        {
            light /= typeCount;
            dark /= typeCount;
            fire /= typeCount;
            water /= typeCount;
            air /= typeCount;
            earth /= typeCount;
            prismatic /= typeCount;
            voidd /= typeCount;
        } else
        {
            //return;
        }

        float numberTransformNormal(float f)
        {
            if (f == 0)
            {
                return 0f;
            }
            float o = (f / 12);
            if (o < 0.5f)
            {
                return 0.5f;
            }

            //the thing with triple can be done later
            if (o > 6)
            {
                return 6;
            }
            return o;
        }
        float numberTransform(float f)
        {
            if (f == 0)
            {
                return 0f;
            }
            float o = f / 6;
            if (o < 1)
            {
                return 1;
            }

            //the thing with triple can be done later
            if (o > 6)
            {
                return 6;
            }
            return o;
        }
        float numberTransformClamp(float f)
        {
            if (f == 0)
            {
                return 0f;
            }
            float o = f / 6;
            if (o < 1)
            {
                return 1;
            }

            if (o > 2)
            {
                return 2;
            }
            return o;
        }

        //perform the numerical conversion (part 1)
        if (typeCount > 1)
        {
            light = numberTransformClamp(light);
            dark = numberTransformClamp(dark);
            fire = numberTransformClamp(fire);
            water = numberTransformClamp(water);
            air = numberTransformClamp(air);
            earth = numberTransformClamp(earth);

            prismatic = numberTransformClamp(prismatic);
            voidd = numberTransformClamp(voidd);
        }
        else
        {
            light = numberTransform(light);
            dark = numberTransform(dark);
            fire = numberTransform(fire);
            water = numberTransform(water);
            air = numberTransform(air);
            earth = numberTransform(earth);

            prismatic = numberTransform(prismatic);
            voidd = numberTransform(voidd);
        }

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy

        GameObject e = null;
        EffectScript_Generic es_h = null;

        void EffectSpawn(float power, GameObject objA, GameObject objB)
        {
            //Debug.Log(power);
            //time to do stuff
            if (power > 0)
            {
                if (power > 2)
                {
                    e = Instantiate(objA, gameObject.transform);
                    e.transform.position = position;
                    es_h = e.GetComponent<EffectScript_Generic>();
                    es_h.Setup(newScale, power / 3);
                }
                else
                {
                    e = Instantiate(objB, gameObject.transform);
                    e.transform.position = position;
                    es_h = e.GetComponent<EffectScript_Generic>();
                    es_h.Setup(newScale, power);
                }
            }
        }

        //0 damage special case
        if (s == 0)
        {
            if (type == BattleHelper.DamageType.Normal || type == BattleHelper.DamageType.Default)
            {
                EffectSpawn(0.25f, hitNormalTriple, hitNormal);
            }
            if (((uint)type & (uint)BattleHelper.DamageType.Light) != 0)
            {
                EffectSpawn(0.5f, hitLightTriple, hitLight);
            }
            if (((uint)type & (uint)BattleHelper.DamageType.Dark) != 0)
            {
                EffectSpawn(0.5f, hitDarkTriple, hitDark);
            }
            if (((uint)type & (uint)BattleHelper.DamageType.Fire) != 0)
            {
                e = Instantiate(hitFire, gameObject.transform);
                e.transform.position = position;
                es_h = e.GetComponent<EffectScript_Generic>();
                es_h.Setup(newScale * 0.25f, fire);

                //new: ember effects
                e = Instantiate(hitFireEmber, gameObject.transform);
                e.transform.position = position;
                es_h = e.GetComponent<EffectScript_Generic>();
                es_h.Setup(newScale * 0.25f, fire);
            }
            if (((uint)type & (uint)BattleHelper.DamageType.Water) != 0)
            {
                EffectSpawn(0.5f, hitWaterTriple, hitWater);
            }
            if (((uint)type & (uint)BattleHelper.DamageType.Air) != 0)
            {
                EffectSpawn(0.5f, hitAirTriple, hitAir);
            }
            if (((uint)type & (uint)BattleHelper.DamageType.Earth) != 0)
            {
                EffectSpawn(0.5f, hitEarthTriple, hitEarth);
            }
            if (((uint)type & (uint)BattleHelper.DamageType.Prismatic) != 0)
            {
                EffectSpawn(0.5f, hitPrismaticTriple, hitPrismatic);
            }
            if (((uint)type & (uint)BattleHelper.DamageType.Void) != 0)
            {
                EffectSpawn(0.5f, hitVoidTriple, hitVoid);
            }
            return;
        }
        
        //time to do stuff
        EffectSpawn(normal, hitNormalTriple, hitNormal);

        EffectSpawn(light, hitLightTriple, hitLight);
        EffectSpawn(dark, hitDarkTriple, hitDark);
        EffectSpawn(water, hitWaterTriple, hitWater);
        EffectSpawn(air, hitAirTriple, hitAir);
        EffectSpawn(earth, hitEarthTriple, hitEarth);


        EffectSpawn(prismatic, hitPrismaticTriple, hitPrismatic);
        EffectSpawn(voidd, hitVoidTriple, hitVoid);

        //fire is a special case
        if (fire > 0)
        {
            if (fire > 2)
            {
                fire = 2 + (fire - 2) * 0.5f;
            }

            float outOffset = newScale * ((fire - 1) * (0.8f / 3));

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);

            int count = Mathf.CeilToInt(fire) - 1;

            float delta = 0;
            if (count > 0)
            {
                delta = (Mathf.PI * 2) / (count);
            }

            Vector3 posA = Vector3.zero;// Vector3.up * Mathf.Sin(angle) + Vector3.right * Mathf.Cos(angle);

            e = Instantiate(hitFire, gameObject.transform);
            e.transform.position = position;
            es_h = e.GetComponent<EffectScript_Generic>();
            es_h.Setup(newScale * 0.5f, fire);

            e = Instantiate(hitFireEmber, gameObject.transform);
            e.transform.position = position;
            es_h = e.GetComponent<EffectScript_Generic>();
            es_h.Setup(newScale * 0.5f, fire);

            for (int i = 0; i < count; i++)
            {
                posA = Vector3.up * Mathf.Sin(angle + i * delta) + Vector3.right * Mathf.Cos(angle + i * delta);
                e = Instantiate(hitFire, gameObject.transform);
                e.transform.position = position + posA * outOffset;
                es_h = e.GetComponent<EffectScript_Generic>();
                es_h.Setup(newScale * 0.5f, fire);

                e = Instantiate(hitFireEmber, gameObject.transform);
                e.transform.position = position + posA * outOffset;
                es_h = e.GetComponent<EffectScript_Generic>();
                es_h.Setup(newScale * 0.5f, fire);
            }
        }
    }
    //placed in the InflictEffect method and the InflictEffectBuffered method (note that these two are parallel)
    //Note that the "Force" methods will bypass this
    public void CreateEffectParticles(Effect e, BattleEntity be)    //derive position from the entity's info
    {
        Vector3 position = be.transform.position;

        bool buff = false;
        bool debuff = false;
        bool buffext = false;
        bool debuffext = false;
        bool status = false;
        Color effectColor = new Color(0,0,0,1);

        float power = e.potency;
        power = (power - 1) * (3f / 5f) + 1;

        if (power < 1)
        {
            power = 1;
        }
        if (power > 4)
        {
            power = 4;
        }

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy

        GameObject eo = null;
        EffectScript_Generic es_g = null;

        switch (e.effect)
        {
            case Effect.EffectType.AttackBoost:
                buffext = true;
                effectColor = new Color(1, 0.15f, 0.15f, 0.8f);
                break;
            case Effect.EffectType.DefenseBoost:
                buffext = true;
                effectColor = new Color(0.15f, 0.15f, 1f, 0.8f);
                break;
            case Effect.EffectType.EnduranceBoost:
                buffext = true;
                effectColor = new Color(1, 1f, 0.15f, 0.8f);
                break;
            case Effect.EffectType.AgilityBoost:
                buffext = true;
                effectColor = new Color(0.15f, 1f, 0.15f, 0.8f);
                break;

            case Effect.EffectType.AttackReduction:
                debuffext = true;
                effectColor = new Color(1, 0.15f, 0.15f, 0.8f);
                break;
            case Effect.EffectType.DefenseReduction:
                debuffext = true;
                effectColor = new Color(0.15f, 0.15f, 1f, 0.8f);
                break;
            case Effect.EffectType.EnduranceReduction:
                debuffext = true;
                effectColor = new Color(1, 1f, 0.15f, 0.8f);
                break;
            case Effect.EffectType.AgilityReduction:
                debuffext = true;
                effectColor = new Color(0.15f, 1f, 0.15f, 0.8f);
                break;


            case Effect.EffectType.Berserk:
                status = true;
                effectColor = new Color(1, 0.3f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Dizzy:
                status = true;
                effectColor = new Color(0.3f, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Freeze:
                status = true;
                effectColor = new Color(0.4f, 1f, 1f, 0.8f);
                break;
            case Effect.EffectType.Sleep:
                status = true;
                effectColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                break;
            case Effect.EffectType.Paralyze:
                status = true;
                effectColor = new Color(1, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Poison:
                status = true;
                effectColor = new Color(0.75f, 0.3f, 1f, 0.8f);
                break;

            case Effect.EffectType.HealthRegen:
                eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_HealthRegen"), gameObject.transform);
                eo.transform.position = be.GetDamageEffectPosition();
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, 3);
                break;
            case Effect.EffectType.EnergyRegen:
                eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_EnergyRegen"), gameObject.transform);
                eo.transform.position = be.GetDamageEffectPosition();
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, 3);
                break;
            case Effect.EffectType.SoulRegen:
                eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_SoulRegen"), gameObject.transform);
                eo.transform.position = be.GetDamageEffectPosition();
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, 3);
                break;

            case Effect.EffectType.AttackUp:
                buff = true;
                effectColor = new Color(1, 0.3f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.DefenseUp:
                buff = true;
                effectColor = new Color(0.3f, 0.3f, 1f, 0.8f);
                break;
            case Effect.EffectType.EnduranceUp:
                buff = true;
                effectColor = new Color(1, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.AgilityUp:
                buff = true;
                effectColor = new Color(0.3f, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.FlowUp:
                buff = true;
                effectColor = new Color(0.7f, 0.3f, 1f, 0.8f);
                break;
            case Effect.EffectType.Focus:
                buff = true;
                effectColor = new Color(1, 0.3f, 1f, 0.8f);
                break;
            case Effect.EffectType.Absorb:
                buff = true;
                effectColor = new Color(0.3f, 1f, 1f, 0.8f);
                break;
            case Effect.EffectType.Burst:
                buff = true;
                effectColor = new Color(1f, 0.75f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Haste:
                buff = true;
                effectColor = new Color(0.75f, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Awaken:
                buff = true;
                effectColor = new Color(0.8f, 0.5f, 1f, 0.8f);
                break;
            case Effect.EffectType.BonusTurns:
                buff = true;
                effectColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                break;
            case Effect.EffectType.Hustle:
                buff = true;
                effectColor = new Color(1f, 0.8f, 0.6f, 0.8f);
                break;

            case Effect.EffectType.HealthLoss:
                eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_HealthLoss"), gameObject.transform);
                eo.transform.position = be.GetDamageEffectPosition();
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, 3);
                break;
            case Effect.EffectType.EnergyLoss:
                eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_EnergyLoss"), gameObject.transform);
                eo.transform.position = be.GetDamageEffectPosition();
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, 3);
                break;
            case Effect.EffectType.SoulLoss:
                eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_SoulLoss"), gameObject.transform);
                eo.transform.position = be.GetDamageEffectPosition();
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, 3);
                break;


            case Effect.EffectType.AttackDown:
                debuff = true;
                effectColor = new Color(1, 0.3f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.DefenseDown:
                debuff = true;
                effectColor = new Color(0.3f, 0.3f, 1f, 0.8f);
                break;
            case Effect.EffectType.EnduranceDown:
                debuff = true;
                effectColor = new Color(1, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.AgilityDown:
                debuff = true;
                effectColor = new Color(0.3f, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.FlowDown:
                debuff = true;
                effectColor = new Color(0.7f, 0.3f, 1f, 0.8f);
                break;
            case Effect.EffectType.Defocus:
                debuff = true;
                effectColor = new Color(1, 0.3f, 1f, 0.8f);
                break;
            case Effect.EffectType.Sunder:
                debuff = true;
                effectColor = new Color(0.3f, 1f, 1f, 0.8f);
                break;
            case Effect.EffectType.Enervate:
                debuff = true;
                effectColor = new Color(1f, 0.75f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Hamper:
                debuff = true;
                effectColor = new Color(0.75f, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Disorient:
                debuff = true;
                effectColor = new Color(0.8f, 0.5f, 1f, 0.8f);
                break;
            case Effect.EffectType.Cooldown:
                debuff = true;
                effectColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                break;
            case Effect.EffectType.Slow:
                debuff = true;
                effectColor = new Color(0.8f, 0.5f, 1f, 1.0f);
                break;

            case Effect.EffectType.Miracle:
                eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Miracle"), gameObject.transform);
                eo.transform.position = position;
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, power);
                break;

            case Effect.EffectType.Immunity:
                eo = Instantiate(effectImmune, gameObject.transform);
                eo.transform.position = be.GetDamageEffectPosition();
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, 3);
                break;
            case Effect.EffectType.Seal:
                eo = Instantiate(effectImmuneNegative, gameObject.transform);
                eo.transform.position = be.GetDamageEffectPosition();
                es_g = eo.GetComponent<EffectScript_Generic>();
                es_g.Setup(newScale, 3);
                break;
        }

        if (status)
        {
            eo = Instantiate(effectStatus, gameObject.transform);
            eo.transform.position = position;
            EffectScript_Status es_s = eo.GetComponent<EffectScript_Status>();
            es_s.Setup(effectColor, newScale, Math.Clamp((int)e.duration, 1, 4));
        }

        if (buff)
        {
            eo = Instantiate(effectBuff, gameObject.transform);
            eo.transform.position = position;
            EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
            es_b.Setup(effectColor, newScale, power);
        }

        if (debuff)
        {
            eo = Instantiate(effectDebuff, gameObject.transform);
            eo.transform.position = position;
            EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
            es_b.Setup(effectColor, newScale, power);
        }

        if (buffext)
        {
            eo = Instantiate(effectBuffPermanent, gameObject.transform);
            eo.transform.position = position;
            EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
            es_b.Setup(effectColor, newScale, power);
        }

        if (debuffext)
        {
            eo = Instantiate(effectDebuffPermanent, gameObject.transform);
            eo.transform.position = position;
            EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
            es_b.Setup(effectColor, newScale, power);
        }
    }
    public void CreateStatusNotYetParticles(Effect e, BattleEntity be) //status would work if the entity was lower hp
    {
        Vector3 position = be.transform.position;

        bool status = false;
        Color effectColor = new Color(0, 0, 0, 1);

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy

        GameObject eo = null;

        switch (e.effect)
        {
            case Effect.EffectType.Berserk:
                status = true;
                effectColor = new Color(1, 0.3f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Dizzy:
                status = true;
                effectColor = new Color(0.3f, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Freeze:
                status = true;
                effectColor = new Color(0.4f, 1f, 1f, 0.8f);
                break;
            case Effect.EffectType.Sleep:
                status = true;
                effectColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                break;
            case Effect.EffectType.Paralyze:
                status = true;
                effectColor = new Color(1, 1f, 0.3f, 0.8f);
                break;
            case Effect.EffectType.Poison:
                status = true;
                effectColor = new Color(0.75f, 0.3f, 1f, 0.8f);
                break;
        }

        if (status)
        {
            eo = Instantiate(effectStatus, gameObject.transform);
            eo.transform.position = position;
            EffectScript_Status es_s = eo.GetComponent<EffectScript_Status>();
            es_s.Setup(effectColor, newScale, 0.5f);
        }
    }
    public void CreateEffectBlockedParticles(Effect e, BattleEntity be) //whenever the effect infliction fails for some reason (except for statuses that get the above method)
    {
        //Debug.Log("Effect blocked");
        Vector3 position = be.GetDamageEffectPosition();

        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy

        if (Effect.IsCleanseable(e.effect, true))
        {
            GameObject eo = Instantiate(effectImmuneNegative, gameObject.transform);
            eo.transform.position = position;
            EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
            es_g.Setup(newScale, 1);
        } else
        {
            GameObject eo = Instantiate(effectImmune, gameObject.transform);
            eo.transform.position = position;
            EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
            es_g.Setup(newScale, 1);
        }
    }
    public void CreateDeathSmoke(BattleEntity be)
    {
        //note: might look weird if giant enemies use this
        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position;

        GameObject eo = null;
        EffectScript_Generic es_g = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_DeathSmoke"), gameObject.transform);
        eo.transform.position = position;
        es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(newScale, 1);
    }
    public void CreateReviveParticles(BattleEntity be, float power)
    {
        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position;

        GameObject eo = null;
        EffectScript_Generic es_g = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect_Revive"), gameObject.transform);
        eo.transform.position = position;
        es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(newScale, power);
    }
    public void CreateCureParticles(BattleEntity be, float power)
    {
        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position;

        GameObject eo = null;
        EffectScript_GenericColorRateOverTime es_b = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Cure"), gameObject.transform);
        eo.transform.position = position;
        es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(new Color(0.6f, 0.6f, 0.6f, 0.8f), newScale, power);
    }
    public void CreateCleanseParticles(BattleEntity be, float power)
    {
        float newScale = Mathf.Max(be.height, be.width);  //looks fine as long as I don't make a super elongated or tall enemy
        Vector3 position = be.transform.position;

        GameObject eo = null;
        EffectScript_GenericColorRateOverTime es_b = null;

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Effect/Effect_Cleanse"), gameObject.transform);
        eo.transform.position = position;
        es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(new Color(0.4f, 0.4f, 0.6f, 0.8f), newScale, power);
    }

    public void RebuildStatDisplayers()
    {
        //Debug.Log("statDisplayer update");
        if (statDisplayers.Count > 0)
        {
            for (int i = 0; i < statDisplayers.Count; i++)
            {
                Destroy(statDisplayers[i]);
            }
        }

        //List<BattleEntity> blist = GetEntitiesSorted((e) => (e.posId < 0 && e.posId > -10));
        List<PlayerEntity> blist = GetPlayerEntities(false);

        for (int i = 0; i < blist.Count; i++)
        {
            GameObject g = Instantiate(statDisplayer, MainManager.Instance.Canvas.transform);
            StatDisplayerScript s = g.GetComponent<StatDisplayerScript>();
            s.SetEntity(blist[i]);
            s.SetPosition(blist.Count - i - 1);
            statDisplayers.Add(g);
        }

        GameObject h = Instantiate(epDisplayer, MainManager.Instance.Canvas.transform);
        EPDisplayerScript d = h.GetComponent<EPDisplayerScript>();
        d.Setup(playerData);
        d.SetPosition();
        statDisplayers.Add(h);

        GameObject k = Instantiate(seDisplayer, MainManager.Instance.Canvas.transform);
        SEDisplayerScript se = k.GetComponent<SEDisplayerScript>();
        se.Setup(playerData);
        se.SetPosition();
        statDisplayers.Add(k);

        GameObject x = Instantiate(xpDisplayer, MainManager.Instance.Canvas.transform);
        XPDisplayerScript xp = x.GetComponent<XPDisplayerScript>();
        xp.Setup();
        //se.SetPosition();
        statDisplayers.Add(x);

        //Bad naming
        GameObject ex = Instantiate(expDisplayer, MainManager.Instance.Canvas.transform);
        EXPDisplayerScript exp = ex.GetComponent<EXPDisplayerScript>();
        exp.Setup(playerData);
        //se.SetPosition();
        statDisplayers.Add(ex);

        GameObject cn = Instantiate(coinDisplayer, MainManager.Instance.Canvas.transform);
        CoinDisplayerScript cns = cn.GetComponent<CoinDisplayerScript>();
        cns.Setup(playerData);
        //se.SetPosition();
        statDisplayers.Add(cn);

        GameObject it = Instantiate(itemDisplayer, MainManager.Instance.Canvas.transform);
        ItemDisplayerScript its = it.GetComponent<ItemDisplayerScript>();
        its.Setup(playerData);
        //se.SetPosition();
        statDisplayers.Add(it);

        if (playerData.BadgeEquipped(Badge.BadgeType.BattleSight))
        {
            GameObject bs = Instantiate(battleSight, MainManager.Instance.Canvas.transform);
            BattleSightScript bss = bs.GetComponent<BattleSightScript>();

            PlayerEntity[] pel = FindObjectsOfType<PlayerEntity>();

            PlayerEntity wilex = null;
            PlayerEntity luna = null;
            for (int i = 0; i < pel.Length; i++)
            {
                if (pel[i].entityID == BattleHelper.EntityID.Wilex)
                {
                    wilex = pel[i];
                }
                if (pel[i].entityID == BattleHelper.EntityID.Luna)
                {
                    luna = pel[i];
                }
            }

            bss.Setup(playerData, wilex, luna);
            statDisplayers.Add(bs);
        }
    }

    public void Start()
    {
        //LoadBattleAssets();
        //StartBattle();

        //this is a 169 cycle I think
        //(the first pool is a 13 cycle
        //  but it pushes the second pool around in such a way that it takes 169 steps to reorient

        //DoublePool testPool = new DoublePool(5, 12, 2, 15); //87/180
        //975/1800 (54.2%)

        //DoublePool testPool = new DoublePool(3, 7, 1, 7); //4/7
        //28/49 (57%)

        //DoublePool testPool = new DoublePool(5, 13, 4, 13); //9/13 
        //115/169 (68%)

        //The normal pool is a 24 cycle
        //(the second pool gets pushed 6/12 which resets every 2 cycles)
        //30/48 (62.5%) (it will always be -2 from correct value?)

        /*
        bool a;
        string output = "(";
        int count = 0;
        for (int i = 0; i < 180 * 100; i++)
        {
            a = testPool.Increment(5, 2);
            output += a ? "y" : "n";
            if (a)
            {
                count++;
            }
        }

        output += ")";

        Debug.Log(count + " / " + 180);
        Debug.Log(output);
        */

        /*
        count = 0;
        output = "(";
        for (int i = 0; i < 24 * 10; i++)
        {
            a = GetTargetFromDoublePool();
            output += a ? "y" : "n";
            if (a)
            {
                count++;
            }
        }

        output += ")";

        Debug.Log(count + " / " + 24);
        Debug.Log("current: " + output);
        */
    }
    public void Update()
    {
        if (!turnLoopRunning && doTurnLoop)
        {
            if (turnCoroutine != null)
            {
                StopCoroutine(turnCoroutine);
                turnLoopRunning = true;
                turnCoroutine = StartCoroutine(DoTurns());
            }
            else
            {
                turnLoopRunning = true;
                turnCoroutine = StartCoroutine(DoTurns());
            }
        }

        //Visual things
        visualXP = MainManager.EasingQuadraticTime(visualXP, battleXP, 50);

        //stat thing
        playerData.GetPlayerDataEntry(GetFrontmostAlly(-1).entityID).timeInFront += Time.deltaTime;

        playerData.ep = ep;
        playerData.maxEP = maxEP;
        playerData.se = se;
        playerData.maxSE = maxSE;
    }

    public bool GetTargetFromDoublePool()
    {
        return ABTargetPool.Increment( 5, 3 );   //8/12 = 2/3 bias towards true
    }
    public bool GetTargetFromDoublePool(int offsetA, int offsetB)
    {
        ABTargetPool.Increment(offsetA, offsetB);
        return ABTargetPool.Increment(5, 3);   //8/12 = 2/3 bias towards true
    }

    //Usually for more complicated decisionmaking in enemies
    public int GetPsuedoRandom(int maxExclusive, int offset = 0)
    {
        //may get slow with big numbers
        //but low numbers are pretty fast ish
        //nth prime is roughly n log n  (might be an O notation ish bound though)
        //So this is roughly n^2 ish complexity?
        int prime = MainManager.GetPrime(2 + offset);
        if (prime == maxExclusive)
        {
            prime = MainManager.GetPrime(3 + offset);
        }

        //note (maxExclusive - 1) is -1 in mod MaxExclusive space
        //which is not very interesting
        int output = offset * (prime) + 13 + 3 * turnCount;

        if (output < 0)
        {
            output += Mathf.CeilToInt((-output) / (0f + maxExclusive)) * maxExclusive;
        }

        return output % maxExclusive;
    }

    public static bool IsPlayerControlled(BattleEntity b, bool checkNoCount)
    {
        //Special case so I can make player characters get devoured so they aren't targettable
        //though in that specific case both of these will be set active
        if (checkNoCount && (b.GetEntityProperty(BattleHelper.EntityProperties.NoCount) || b.GetEntityProperty(BattleHelper.EntityProperties.NoTarget)))
        {
            return false;
        }

        return b.posId < 0 && b.posId > -10;
    }

    public List<PlayerEntity> GetPlayerEntities(bool checkNoCount = true)
    {
        //Note: checks IsPlayerControlled as well as checking entity is PlayerEntity

        List<BattleEntity> elist = GetEntitiesSorted((e) => IsPlayerControlled(e, checkNoCount));
        List<PlayerEntity> output = new List<PlayerEntity>();

        for (int i = 0; i < elist.Count; i++)
        {
            if (elist[i] is PlayerEntity)
            {
                output.Add((PlayerEntity)elist[i]);
            }
        }

        return output;
    }

    public bool CanUseMetaItemMove(MetaItemMove.Move move)
    {
        switch (move)
        {
            case MetaItemMove.Move.Normal:
                return playerData.itemInventory.Count > 0;
            case MetaItemMove.Move.Multi:
                if (MainManager.Instance.Cheat_InfiniteBite)
                {
                    return playerData.itemInventory.Count > 1;
                }
                else
                {
                    return playerData.itemInventory.Count > 1 && (MultiSupplyUses < (2 * playerData.BadgeEquippedCount(Badge.BadgeType.MultiSupply)));
                }
            case MetaItemMove.Move.Quick:
                return playerData.itemInventory.Count > 0 && (QuickSupplyUses < (2 * playerData.BadgeEquippedCount(Badge.BadgeType.QuickSupply)));
            case MetaItemMove.Move.Void:
                return usedItems.Count > 0 && (VoidSupplyUses < (2 * playerData.BadgeEquippedCount(Badge.BadgeType.VoidSupply)));
        }

        return false;
    }
    public PlayerMove.CantMoveReason GetCantUseReasonMetaItemMove(MetaItemMove.Move move)
    {
        switch (move)
        {
            case MetaItemMove.Move.Normal:
                return PlayerMove.CantMoveReason.NoItems;
            case MetaItemMove.Move.Multi:
                if (playerData.itemInventory.Count == 0)
                {
                    return PlayerMove.CantMoveReason.NoItems;
                }
                if (MainManager.Instance.Cheat_InfiniteBite)
                {
                    return PlayerMove.CantMoveReason.ItemMultiSupplyBlock;
                }
                else
                {
                    if (playerData.itemInventory.Count == 1)
                    {
                        return PlayerMove.CantMoveReason.ItemMultiSupplyBlock;
                    }
                    return PlayerMove.CantMoveReason.ItemMultiSupplyBlock;
                }
            case MetaItemMove.Move.Quick:
                if (playerData.itemInventory.Count == 0)
                {
                    return PlayerMove.CantMoveReason.NoItems;
                }
                return PlayerMove.CantMoveReason.MoveExpended;
            case MetaItemMove.Move.Void:
                if (usedItems.Count == 0)
                {
                    return PlayerMove.CantMoveReason.NoItems;
                }
                return PlayerMove.CantMoveReason.MoveExpended;
        }

        return PlayerMove.CantMoveReason.Unknown;
    }

    public int GetMetaItemUsesRemaining(MetaItemMove.Move move)
    {
        switch (move)
        {
            case MetaItemMove.Move.Normal:
                return -1;
            case MetaItemMove.Move.Multi:
                if (MainManager.Instance.Cheat_InfiniteBite)
                {
                    return -1;
                }
                else
                {
                    return ((2 * playerData.BadgeEquippedCount(Badge.BadgeType.MultiSupply)) - MultiSupplyUses);
                }
            case MetaItemMove.Move.Quick:
                return ((2 * playerData.BadgeEquippedCount(Badge.BadgeType.QuickSupply)) - QuickSupplyUses);
            case MetaItemMove.Move.Void:
                return ((2 * playerData.BadgeEquippedCount(Badge.BadgeType.VoidSupply)) - VoidSupplyUses);
        }

        return -1;
    }



    public void DebugDrawHitboxes()
    {
        Debug.Log("Debug hitboxes");
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].DebugDrawHitbox();
        }
    }

    public IEnumerator StartOfBattleEvents()
    {
        yield return new WaitForSeconds(0.7f);  //wait for fade in

        //DebugDrawHitboxes();

        entities.Sort(Comparer<BattleEntity>.Create((a, b) => PosCompare(a, b)));

        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();

            current.PreBattle();
        }

        //show the inn effect popup if needed (since the effect applies right after this)
        //To do: do that

        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();

            if (current.HasPreBattleCoroutine())
            {
                yield return StartCoroutine(current.PreBattleCoroutine());
                yield return new WaitForSeconds(0.5f);
            }
        }

        //tick down inn effects at this point
        PlayerData pd = playerData;
        for (int i = 0; i < pd.innEffects.Count; i++)
        {
            pd.innEffects[i].charges--;

            if (pd.innEffects[i].charges <= 0)
            {
                pd.innEffects.RemoveAt(i);
                i--;
                continue;
            }
        }

        //Tick down charm effects here too
        for (int i = 0; i < pd.charmEffects.Count; i++)
        {
            pd.charmEffects[i].duration--;

            if (pd.charmEffects[i].duration <= 0)
            {
                pd.charmEffects[i].duration = pd.charmEffects[i].resetDuration;
                pd.charmEffects[i].charges -= 1;
                if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Attack)
                {
                    pd.charmEffects[i].charmType = CharmEffect.CharmType.Defense;
                }
                else if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Defense)
                {
                    pd.charmEffects[i].charmType = CharmEffect.CharmType.Attack;
                }
            }

            if (pd.charmEffects[i].duration <= 0 || pd.charmEffects[i].charges <= 0)
            {
                pd.charmEffects.RemoveAt(i);
                i--;
                continue;
            }
        }

        //Evaluate special first strike things
        bool autoStrike = !GetProperty(BattleHelper.BattleProperties.IgnoreFirstStrikeBadges) && playerData.BadgeEquipped(Badge.BadgeType.AutoStrike);
        bool inviteDanger = !GetProperty(BattleHelper.BattleProperties.IgnoreFirstStrikeBadges) && playerData.BadgeEquipped(Badge.BadgeType.InviteDanger);
        bool dodgeStep = !GetProperty(BattleHelper.BattleProperties.IgnoreFirstStrikeBadges) && playerData.BadgeEquipped(Badge.BadgeType.DodgeStep);
        bool smartAmbush = !GetProperty(BattleHelper.BattleProperties.IgnoreFirstStrikeBadges) && playerData.BadgeEquipped(Badge.BadgeType.SmartAmbush);

        bool headStart = playerData.BadgeEquipped(Badge.BadgeType.HeadStart);

        List<PlayerEntity> party = GetPlayerEntities();

        if (autoStrike && firstStrikePosId < 0)  //includes null ID
        {
            firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ALLY;
        }

        if (dodgeStep && firstStrikePosId >= 0)
        {
            firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_NULL_ENTITY;
        }

        if (inviteDanger)
        {
            if (firstStrikePosId != BattleStartArguments.FIRSTSTRIKE_NULL_ENTITY && firstStrikePosId < 0)
            {
                for (int i = 0; i < party.Count; i++)
                {
                    party[i].InflictEffect(party[i], new Effect(Effect.EffectType.Focus, Effect.INFINITE_DURATION, (sbyte)(4 * playerData.BadgeEquippedCount(Badge.BadgeType.InviteDanger))));
                }
            } else
            {
                for (int i = 0; i < party.Count; i++)
                {
                    party[i].InflictEffect(party[i], new Effect(Effect.EffectType.Focus, Effect.INFINITE_DURATION, (sbyte)(2 * playerData.BadgeEquippedCount(Badge.BadgeType.InviteDanger))));
                }
            }            
            firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ENEMY;
        }

        if (smartAmbush)
        {
            if (firstStrikePosId != BattleStartArguments.FIRSTSTRIKE_NULL_ENTITY && firstStrikePosId < 0)
            {
                for (int i = 0; i < party.Count; i++)
                {
                    party[i].InflictEffect(party[i], new Effect(Effect.EffectType.BonusTurns, (sbyte)(playerData.BadgeEquippedCount(Badge.BadgeType.SmartAmbush)), Effect.INFINITE_DURATION));
                }
            }
        }

        if (headStart)
        {
            for (int i = 0; i < party.Count; i++)
            {
                party[i].InflictEffect(party[i], new Effect(Effect.EffectType.BonusTurns, (sbyte)(playerData.BadgeEquippedCount(Badge.BadgeType.HeadStart)), Effect.INFINITE_DURATION));
            }
        }

        //only the designated first striker will first strike
        if (firstStrikePosId != BattleStartArguments.FIRSTSTRIKE_NULL_ENTITY)
        {
            //special codes:
            //max = frontmost enemy
            //min + 1 = frontmost ally (*normally -1)
            if (firstStrikePosId == BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ENEMY || firstStrikePosId == BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ALLY)
            {
                List<BattleEntity> elist = null;
                if (firstStrikePosId == BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ENEMY)
                {
                    elist = GetEntities((e) => (e.posId >= 0));
                } else
                {
                    elist = GetEntities((e) => (e.posId < 0));
                }

                elist.Sort((a, b) => StandardEntitySort(a, b));

                BattleEntity firstStriker = elist[0]; //GetEntityByID(firstStrikePosId);
                if (firstStriker != null)
                {
                    yield return StartCoroutine(firstStriker.FirstStrike(firstStrikeMove));
                }
            } else
            {
                BattleEntity firstStriker = GetEntityByID(firstStrikePosId);
                if (firstStriker != null)
                {
                    yield return StartCoroutine(firstStriker.FirstStrike(firstStrikeMove));
                }
            }
        }

        /*
        entities.Sort(Comparer<BattleEntity>.Create((a, b) => PosCompare(a, b)));
        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();

            yield return StartCoroutine(current.FirstStrike());
            if (current.HasFirstStrike())
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        */

        //check for some badges
        List<PlayerEntity> playerEntities = GetPlayerEntities();
        for (int i = 0; i < playerEntities.Count; i++)
        {
            if (playerEntities[i].BadgeEquipped(Badge.BadgeType.QuickNap))
            {
                //possible higher potency status!
                playerEntities[i].InflictEffect(playerEntities[i], new Effect(Effect.EffectType.Sleep, (sbyte)(playerEntities[i].BadgeEquippedCount(Badge.BadgeType.QuickNap)), 2));
            }
        }
    }

    //Check for enemy out of turn events to execute them
    //Executes after every player / enemy turn
    public IEnumerator RunOutOfTurnEvents()
    {
        //Debug.Log("RunOutOfTurnEvents " + reactionMoveList.Count);
        //wait until animations done
        yield return new WaitUntil(() => CheckEntitiesStationary());


        PlayerTurnController.Instance.ResetActionCommands();

        AbsorbFocusCheck();
        //apply buffered effects
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].ApplyBufferedEffects();
        }

        while (reactionMoveList.Count > 0)
        {
            yield return new WaitUntil(() => CheckEntitiesStationary());

            //Now, show prompts if necessary
            yield return StartCoroutine(HandleBattlePopups());

            //Ensure that the reactor is actually valid and can move
            if ((reactionMoveList[0].user == null && reactionMoveList[0].move.GetSourceless()) || (EntityValid(reactionMoveList[0].user) && (reactionMoveList[0].ignoreImmobile || reactionMoveList[0].user.CanMove())))
            {
                //Debug.Log(reactionMoveList[0]);

                //wait for the user to be out of events first
                yield return new WaitUntil(() => !reactionMoveList[0].user.immediateInEvent);

                //item validity check
                if (reactionMoveList[0].isItem)
                {
                    ItemMove i = (ItemMove)(reactionMoveList[0].move);
                    if (Instance.playerData.HasItem(i.GetItem()))
                    {
                        yield return StartCoroutine(reactionMoveList[0].user.PreReact(reactionMoveList[0].move, reactionMoveList[0].target));
                        yield return StartCoroutine(reactionMoveList[0].move.ExecuteOutOfTurn(reactionMoveList[0].user, reactionMoveList[0].target));
                        //yield return StartCoroutine(CheckEndBattle());
                        if (reactionMoveList.Count > 1)
                        {
                            yield return new WaitForSeconds(0.5f);
                        }
                    }
                } else
                {
                    DisplayMovePopup(reactionMoveList[0].move.GetName());
                    //this replaces the wait for 0.25 seconds
                    yield return StartCoroutine(reactionMoveList[0].user.PreReact(reactionMoveList[0].move, reactionMoveList[0].target));
                    yield return StartCoroutine(reactionMoveList[0].move.ExecuteOutOfTurn(reactionMoveList[0].user, reactionMoveList[0].target));
                    //yield return StartCoroutine(CheckEndBattle());
                    DestroyMovePopup();
                    if (reactionMoveList.Count > 1)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }

            if (reactionMoveList.Count > 0)
            {
                //Debug.Log("Removing move: " + reactionMoveList[0].move.GetName());
                reactionMoveList.RemoveAt(0);   //basically acts like a queue like this
            }

            //Check again (In case the reaction caused some damage events)
            AbsorbFocusCheck();
            //apply buffered effects
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].ApplyBufferedEffects();
            }

            yield return new WaitUntil(() => CheckEntitiesStationary());
        }

        float timestamp = Time.time;

        yield return StartCoroutine(HandleBattlePopups());

        //wait until animations done (putting this after the popup check since the popups won't interfere with the events)
        yield return new WaitUntil(() => CheckEntitiesStationary());

        //Only check for battle completion once all the events are gone
        //(just in case stuff like revives are supposed to be queued up)
        //Debug.Log(reactionMoveList.Count + " reaction moves in queue (should be 0)");
        //Debug.Log("Check End Battle");
        yield return StartCoroutine(CheckEndBattle());
    }
    public bool ItemReactionExists(Item.ItemType it)
    {
        for (int i = 0; i < reactionMoveList.Count; i++)
        {
            if (!reactionMoveList[i].isItem)
            {
                continue;
            }

            ItemMove im = (ItemMove)(reactionMoveList[i].move);

            if (im.GetItemType() == it)
            {
                return true;
            }
        }

        return false;
    }

    public static int PosCompare(BattleEntity a, BattleEntity b)
    {
        int aa = MainManager.FloatCompare(a.homePos.x, b.homePos.x);

        if (aa == 0)
        {
            return (int)Mathf.Sign(a.posId - b.posId);
        }

        return aa;
    }

    public void DisplayMovePopup(string name)
    {
        //Debug.Log("Display " + name);
        DestroyMovePopup();
        GameObject go = Instantiate(movePopupBoxBase, MainManager.Instance.Canvas.transform);
        movePopup = go.GetComponent<NamePopupScript>();

        movePopup.SetText(name, true, true);
    }
    public void DestroyMovePopup()
    {
        //Debug.Log("Destroy");
        if (movePopup != null)
        {
            Destroy(movePopup.gameObject);
        }
    }

    public void AddBattlePopup(BattleEntity target, Effect se)
    {
        //forbid you from adding it if one already exists
        //(so a spread buff move won't show a bunch of popups)

        /*
        battlePopupList.Add(new BattlePopup(target, se));

        if (battlePopupList.Count > 1)
        {
            for (int i = battlePopupList.Count - 2; i > -1; i--)
            {
                if (battlePopupList[i].text.Equals(battlePopupList[battlePopupList.Count - 1].text))
                {
                    battlePopupList.RemoveAt(battlePopupList.Count - 1);
                    return;
                }
            }
        }
        */

        //Safer version
        BattlePopup newPopup = new BattlePopup(target, se);

        bool doAdd = true;
        if (battlePopupList.Count > 0)
        {
            for (int i = 0; i < battlePopupList.Count; i++)
            {
                if (battlePopupList[i].text.Equals(newPopup.text)) {
                    doAdd = false;
                }
            }
        }

        if (doAdd)
        {
            battlePopupList.Add(newPopup);
        }
    }
    public void AddBattlePopup(BattleHelper.EnvironmentalEffect ee)
    {
        //forbid you from adding it if one already exists
        //(so a spread buff move won't show a bunch of popups)

        /*
        battlePopupList.Add(new BattlePopup(target, se));

        if (battlePopupList.Count > 1)
        {
            for (int i = battlePopupList.Count - 2; i > -1; i--)
            {
                if (battlePopupList[i].text.Equals(battlePopupList[battlePopupList.Count - 1].text))
                {
                    battlePopupList.RemoveAt(battlePopupList.Count - 1);
                    return;
                }
            }
        }
        */

        bool enviroStrong = HarderEnviroEffects();
        float enviroPower = EnviroEffectPower();

        //Safer version
        BattlePopup newPopup = new BattlePopup(ee, enviroPower, enviroStrong);

        bool doAdd = true;
        if (battlePopupList.Count > 0)
        {
            for (int i = 0; i < battlePopupList.Count; i++)
            {
                if (battlePopupList[i].text.Equals(newPopup.text))
                {
                    doAdd = false;
                }
            }
        }

        if (doAdd)
        {
            battlePopupList.Add(newPopup);
        }
    }

    public IEnumerator HandleBattlePopups()
    {
        //sus technology
        //Debug.Log("Battle popups start " + battlePopupList.Count);

        while (battlePopupList.Count > 0)
        {
            yield return new WaitUntil(() => CheckEntitiesStationary());
            if (battlePopupList.Count == 0)
            {
                Debug.LogWarning("(A) Popups disappeared without being accounted for correctly");
                yield break;
            }

            float timestamp = Time.time;

            BattlePopupMenuScript popup = BattlePopupMenuScript.buildMenu(battlePopupList[0].text, battlePopupList[0].vars);
            popup.transform.parent = transform;

            yield return new WaitUntil(() => popup.exit);

            popup.Clear();
            Destroy(popup.gameObject);

            bool stop = battlePopupList.Count <= 1;
            if (battlePopupList.Count > 0)
            {                
                battlePopupList.RemoveAt(0);
            }
            else
            {
                Debug.LogWarning("(B) Popups disappeared without being accounted for correctly");
                yield break;
            }
        }

        //Debug.Log("Battle popups end");
        
        yield return null;
    }

    public void AddReactionMoveEvent(BattleEntity caller, BattleEntity target, Move move, bool ignoreImmobile = false, bool isItem = false)
    {
        //Debug.Log(caller.name + " " + target.name + " " + move.GetName());
        reactionMoveList.Add(new ReactionMoveListEntry(caller, target, move, ignoreImmobile, isItem));
    }
    public void BroadcastEvent(BattleEntity battleEntity, BattleHelper.Event eventID)
    {
        lastEventID = eventID;
        lastEventEntity = battleEntity;
        lastEventReactions = 0;

        //Debug.Log(battleEntity + " triggered " + eventID);        

        GlobalReactions(battleEntity, eventID);

        //entity that triggered event gets priority
        //probably not going to make an entity react to itself but idk
        if (battleEntity.ReactToEvent(battleEntity, eventID, lastEventReactions))
        {
            lastEventReactions++;
        }

        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i] != battleEntity)
            {
                if (entities[i].ReactToEvent(battleEntity, eventID, lastEventReactions))
                {
                    lastEventReactions++;
                }
            }
        }
    }

    public void GlobalReactions(BattleEntity battleEntity, BattleHelper.Event eventID)
    {
        //global "reactions" (mostly auto use items) get priority

        //Item auto activation
        //this has to be mostly hardcoded as conditions change depending on what item is activated

        //bool autoReviveActivation = false;
        bool miracleNeedle = false;
        int miracleNeedleID = int.MinValue;
        bool miracleShroom = false;
        bool strangeShroom = false;

        bool slimeBomb = false;
        bool pepperNeedle = false;
        bool stickySpore = false;
        bool goldBomb = false;

        //To do: create a more decentralized system for checking this? (a special method)
        for (int i = 0; i < playerData.itemInventory.Count; i++)
        {
            /*
            if (!autoReviveActivation && playerData.itemInventory[i].type == Item.ItemType.DebugAutoRevive)
            {
                if ((lastEventID == BattleHelper.Event.Death || lastEventID == BattleHelper.Event.StatusDeath) && IsPlayerControlled(battleEntity))
                {
                    autoReviveActivation = true;
                    AddReactionMoveEvent(battleEntity, battleEntity, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                }
            }
            */

            if (!miracleNeedle && playerData.itemInventory[i].type == Item.ItemType.MiracleNeedle)
            {
                //if (IsPlayerControlled(battleEntity) && battleEntity.hp <= 0)
                if ((lastEventID == BattleHelper.Event.Death || lastEventID == BattleHelper.Event.StatusDeath) && IsPlayerControlled(battleEntity, false))
                {
                    //now check for existing miracle needle event
                    if (!ItemReactionExists(Item.ItemType.MiracleNeedle))
                    {
                        miracleNeedle = true;
                        miracleNeedleID = battleEntity.posId;
                        AddReactionMoveEvent(battleEntity, battleEntity, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                    }
                }
            }

            if (!miracleNeedle && !miracleShroom && playerData.itemInventory[i].type == Item.ItemType.MiracleShroom)
            {
                //Debug.Log("Miracle shroom A " + (!ItemReactionExists(Item.ItemType.MiracleNeedle)) + " " + (!ItemReactionExists(Item.ItemType.MiracleShroom)) + " " + (entities.FindAll(e => IsPlayerControlled(e) && e.IsAlive()).Count == 0));
                //Debug.Log("Miracle shroom B " + (entities.FindAll(e => e.hp > 0).Count == 0));
                //avoid false negatives?
                //if ((lastEventID == BattleHelper.Event.Death || lastEventID == BattleHelper.Event.StatusDeath) && IsPlayerControlled(battleEntity))
                //{
                //Note: if a needle event exists, don't activate the shroom
                if (!ItemReactionExists(Item.ItemType.MiracleNeedle) && !ItemReactionExists(Item.ItemType.MiracleShroom) && entities.FindAll(e => IsPlayerControlled(e, true) && e.IsAlive()).Count == 0)
                {
                    miracleShroom = true;
                    AddReactionMoveEvent(battleEntity, battleEntity, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                }
                //}

                if (!miracleShroom && !ItemReactionExists(Item.ItemType.MiracleNeedle) && !ItemReactionExists(Item.ItemType.MiracleShroom) && entities.FindAll(e => e.hp > 0).Count == 0)
                {
                    miracleShroom = true;
                    AddReactionMoveEvent(battleEntity, battleEntity, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                }
            }

            if (!strangeShroom && playerData.itemInventory[i].type == Item.ItemType.StrangeMushroom)
            {
                if (lastEventID == BattleHelper.Event.PayEnergy && IsPlayerControlled(battleEntity, false))
                {
                    if (ep == 0)
                    {
                        strangeShroom = true;
                        AddReactionMoveEvent(battleEntity, battleEntity, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                    }
                }
            }


            //the attacker items
            if (!slimeBomb && playerData.itemInventory[i].type == Item.ItemType.SlimeBomb)
            {
                if (lastEventID == BattleHelper.Event.Rest && IsPlayerControlled(battleEntity, false))
                {
                    slimeBomb = true;
                    AddReactionMoveEvent(battleEntity, battleEntity, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                }
            }

            if (!pepperNeedle && playerData.itemInventory[i].type == Item.ItemType.PepperNeedle)
            {
                //target the attacker
                if (lastEventID == BattleHelper.Event.Hurt && IsPlayerControlled(battleEntity, false))
                {
                    if (battleEntity.lastAttacker != null && battleEntity.lastAttacker.posId >= 0)
                    {
                        pepperNeedle = true;
                        AddReactionMoveEvent(battleEntity, battleEntity.lastAttacker, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                    }
                }
            }

            if (!stickySpore && playerData.itemInventory[i].type == Item.ItemType.StickySpore)
            {
                //target the damaged enemy
                if (lastEventID == BattleHelper.Event.Hurt && battleEntity.posId >= 0)
                {
                    stickySpore = true;

                    //Problem: this does not reference any player entities
                    //(so just grab one that's available I guess)

                    //note: player entities are enemies in this case
                    BattleEntity caster = GetEntitiesSorted(battleEntity, new TargetArea(TargetArea.TargetAreaType.Enemy))[0];

                    AddReactionMoveEvent(caster, battleEntity, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                }
            }

            //gold bomb backfire
            if (!goldBomb && playerData.itemInventory[i].type == Item.ItemType.GoldBomb)
            {
                //target the damaged enemy
                if (lastEventID == BattleHelper.Event.Hurt && IsPlayerControlled(battleEntity, false))
                {
                    goldBomb = true;

                    //Debug.Log("goldBomb");

                    //whoever gets hit triggers the bombs                      
                    BattleEntity caster = battleEntity;

                    AddReactionMoveEvent(caster, battleEntity, Item.GetItemMoveScript(playerData.itemInventory[i]), true, true);
                }
            }
        }
    }

    public IEnumerator DoTurns()
    {
        if (!doTurnLoop)
        {
            //Debug.LogError("Invalid state");
            yield break;
        }

        turnLoopRunning = true;


        //Start of battle stuff
        if (turnCount < 1)
        {
            yield return StartCoroutine(StartOfBattleEvents());

            if (enviroEffect != BattleHelper.EnvironmentalEffect.None)
            {
                AddBattlePopup(enviroEffect);
            }
        }

        //why not
        yield return StartCoroutine(RunOutOfTurnEvents());


        //premove stuff (might move right before choosing move)
        //Sort by id
        //entities.Sort(Comparer<BattleEntity>.Create((a, b) => (a.posId - b.posId)));

        entities.Sort(Comparer<BattleEntity>.Create((a, b) => PosCompare(a,b)));

        //increment stamina
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].PerTurnStaminaHeal();
        }

        List<PlayerEntity> p = GetPlayerEntities();
        if (se < maxSE && p.Count > 0 && !playerData.BadgeEquipped(Badge.BadgeType.DarkConcentration))
        {
            if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Sloth))
            {
                p[0].HealSoulEnergy(3);
            }
            else
            {
                p[0].HealSoulEnergy(1);
            }
        }

        /*
        entities.SetBools();
        while (entities.hasNext())
        {        
            BattleEntity current = entities.next();
            //Debug.Log(current.id + " pre");
            yield return StartCoroutine(current.PreMove());
            yield return new WaitUntil(() => !interrupt);
        }
        */

        turnCount++;
        playerData.cumulativeBattleTurns++;
        playerData.GetPlayerDataEntry(GetFrontmostAlly(-1).entityID).turnsInFront++;
        if (battleMapScript != null)
        {
            battleMapScript.OnPreTurn();
        }

        yield return StartCoroutine(RunOutOfTurnEvents());
        if (!doTurnLoop)
        {
            //Debug.LogError("Invalid state");
            yield break;
        }
        
        /*
        //choose move before moving
        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();
            //Debug.Log(current.id + " choose");
            current.ChooseMove(); //player choose move is empty
        }
        */

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Sloth))
        {
            foreach (PlayerEntity pe in p)
            {
                if (!pe.HasEffect(Effect.EffectType.Slow))
                {
                    pe.InflictEffectForce(pe, new Effect(Effect.EffectType.Slow, 1, Effect.INFINITE_DURATION));
                }
            }
        }

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_Burden_Wrath))
        {
            List<BattleEntity> bl = GetEntitiesSorted((e) => (e.posId < 0));

            if (bl.Count > 0 && !bl[0].HasEffect(Effect.EffectType.Berserk))
            {
                bl[0].InflictEffectForce(bl[0], new Effect(Effect.EffectType.Berserk, 1, Effect.INFINITE_DURATION));
            }
        }


        //let player choose move
        yield return StartCoroutine(PlayerTurnController.Instance.TakeTurn());

        yield return StartCoroutine(RunOutOfTurnEvents());

        //debug
        //DebugDrawHitboxes();

        //now, fire off entity moves in order

        /*
        List<MoveListEntry> moves = new List<MoveListEntry>();

        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();

            if (IsPlayerControlled(current))
            {
                continue;
            }

            //premove here
            yield return StartCoroutine(current.PreMove());
            yield return new WaitUntil(() => !interrupt);            

            //if (current.GetMovePriority() != float.PositiveInfinity)
            if (current.CanMove())
            {
                //choosemove here
                current.ChooseMove(); //player choose move is empty

                //Debug.Log(current.GetMovePriority() + " " + current.entityID + " " + current.currMove.GetName());
                moves.Add(new MoveListEntry(current, current.currMove));
 
                int specialID = 1;
                if (current.extraMoves != null)
                {
                    for (int i = 0; i < current.extraMoves.Count; i++)
                    {
                        //time += current.extraMoves[i].time * current.GetAttribute(BattleHelper.Attribute.SpeedMult);
                        specialID++;    //so first one gets the id of 2 (first one gets a 1)
                        moves.Add(new MoveListEntry(current, current.extraMoves[i], specialID));    
                    }
                }
            }
        }

        //Old ver (sort by time)
        //moves.Sort((a, b) => (int)Mathf.Sign(a.time - b.time));

        //New ver (sort by positionalID)
        //newer: sort by pos
        moves.Sort((a, b) => PosCompare(a.user, b.user));
        for (int i = 0; i < moves.Count; i++)
        {
            //hardcode player characters moving in PlayerTurnController (and not here!)
            if (IsPlayerControlled(moves[i].user))
            {
                continue;
            }

            if (moves[i].user != null && entities.Contains(moves[i].user))
            {
                if (moves[i].move != null)
                {
                    //if (moves[i].time > moves[i].user.GetMovePriority())
                    if (moves[i].temporalID > 1)
                    {
                        moves[i].user.PreMultiMove();
                    }

                    moves[i].user.moveExecuting = true;
                    moves[i].user.moveActive = true;
                    StartCoroutine(moves[i].user.ExecuteMoveCoroutine(moves[i].move));
                    yield return new WaitUntil(() => moves[i].user == null || !moves[i].user.moveActive);

                    //Note: These coroutines will have to wait for all moveExecuting values to complete if there is something to do
                    //yield return StartCoroutine(CheckEndBattle());                    

                    yield return StartCoroutine(RunOutOfTurnEvents());
                }
            }

            //negate absorb of enemies, check for special turn order breaking events

            yield return new WaitForSeconds(0.5f);
        }
        */


        /*
        //entities.Sort(Comparer<BattleEntity>.Create((a, b) => (a.CompareMove(b))));
        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();
            //Debug.Log(current.id + " go "+current.GetMovePriority());
            if (current.GetMovePriority() != float.PositiveInfinity)
            {
                current.moving = true;
                StartCoroutine(current.ExecuteMoveCoroutine());
                yield return new WaitUntil(() => !current.moving || current == null);
            }
            yield return new WaitForSeconds(0.5f);
        }
        */

        //Version 3
        //Enemies choose moves right before execution
        //
        entities.Sort(Comparer<BattleEntity>.Create((a, b) => PosCompare(a, b)));
        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();

            if (IsPlayerControlled(current, false))
            {
                continue;
            }

            //Apply enemy bonus turns and cooldown

            bool keepMoving = true;

            //Cooldown prevents the enemy from acting immediately
            if (current.HasEffect(Effect.EffectType.Cooldown))
            {
                //Debug.Log(current + " a");
                keepMoving = false;
                current.TokenRemoveOne(Effect.EffectType.Cooldown);
            }

            while (keepMoving)
            {
                //Debug.Log(current + " b");
                current.ChooseMove();

                if (current.currMove != null)
                {
                    current.moveExecuting = true;
                    current.moveActive = true;
                    DisplayMovePopup(current.currMove.GetName());
                    yield return new WaitForSeconds(0.5f); //note: not in reactions because they have their own standard particle effect
                    StartCoroutine(current.ExecuteMoveCoroutine(current.currMove));
                    yield return new WaitUntil(() => current == null || !current.moveActive);
                    DestroyMovePopup();
                }

                //try to use bonus moves
                if (current.HasEffect(Effect.EffectType.BonusTurns))
                {
                    current.TokenRemoveOne(Effect.EffectType.BonusTurns);
                } else
                {
                    keepMoving = false;
                }
            }

            //Note: These coroutines will have to wait for all moveExecuting values to complete if there is something to do
            //yield return StartCoroutine(CheckEndBattle());

            HideHPBars();
            HideEffectIcons();

            yield return StartCoroutine(RunOutOfTurnEvents());
        }

        //Counter Flare activates before the PostMove stuff since the damage taken this turn variable gets changed by postmove
        //Counter Flare calculation + Splotch calculation
        entities.Sort(Comparer<BattleEntity>.Create((a, b) => PosCompare(a, b)));
        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();
            if (current.damageTakenThisTurn > 0)
            {
                if (current.TokenRemoveOne(Effect.EffectType.CounterFlare))
                {
                    current.counterFlareDamage = current.damageTakenThisTurn;
                }
                else
                {
                    current.counterFlareDamage = 0;
                }
            }
            if (current.HasEffect(Effect.EffectType.ArcDischarge))
            {
                current.arcDischargeDamage = Mathf.CeilToInt(0.125f * current.damageTakenThisTurn);
            } else
            {
                current.arcDischargeDamage = 0;
            }
            if (current.HasEffect(Effect.EffectType.Splotch))
            {
                current.splotchDamage = Mathf.CeilToInt(0.5f * current.damageTakenThisTurn);
            } else
            {
                current.splotchDamage = 0;
            }

            if (enviroEffect == BattleHelper.EnvironmentalEffect.TrialOfZeal)
            {
                current.magmaDamage = current.damageTakenThisTurn;
            }
            else if (enviroEffect == BattleHelper.EnvironmentalEffect.ScaldingMagma)
            {
                current.magmaDamage = Mathf.CeilToInt(0.5f * current.damageTakenThisTurn);
            } else
            {
                current.magmaDamage = 0;
            }
        }

        //Counter Flare + Splotch activation
        entities.Sort(Comparer<BattleEntity>.Create((a, b) => PosCompare(a, b)));
        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();
            //not the "right" property to use but I don't feel like making another property for a thing I probably won't use
            if (current.magmaDamage > 0 && (current.hp > 0 || current.GetEntityProperty(BattleHelper.EntityProperties.GetEffectsAtNoHP)))
            {
                Move sp = GetOrAddComponent<ScaldingMagma>();
                DisplayMovePopup(sp.GetName());
                yield return new WaitForSeconds(0.5f);
                yield return sp.Execute(current);
                DestroyMovePopup();
            }
            if (current.arcDischargeDamage > 0 && (current.hp > 0 || current.GetEntityProperty(BattleHelper.EntityProperties.GetEffectsAtNoHP)))
            {
                Move sp = GetOrAddComponent<ArcDischarge>();
                DisplayMovePopup(sp.GetName());
                yield return new WaitForSeconds(0.5f);
                yield return sp.Execute(current);
                DestroyMovePopup();
            }
            if (current.splotchDamage > 0 && (current.hp > 0 || current.GetEntityProperty(BattleHelper.EntityProperties.GetEffectsAtNoHP)))
            {
                Move sp = GetOrAddComponent<Splotch>();
                DisplayMovePopup(sp.GetName());
                yield return new WaitForSeconds(0.5f);
                yield return sp.Execute(current);
                DestroyMovePopup();
            }
            if (current.counterFlareDamage > 0 && (current.hp > 0 || current.GetEntityProperty(BattleHelper.EntityProperties.GetEffectsAtNoHP)))
            {
                Move cf = GetOrAddComponent<CounterFlare>();
                DisplayMovePopup(cf.GetName());
                yield return new WaitForSeconds(0.5f);
                yield return cf.Execute(current);
                DestroyMovePopup();
            }
        }


        //post move stuff

        //tally max damage per turn
        int perTurnDamage = 0;
        List<PlayerEntity> pel = GetPlayerEntities();
        for (int i = 0; i < pel.Count; i++)
        {
            perTurnDamage += pel[i].perTurnDamageDealt;
        }
        playerData.UpdateMaxDamageDealt(perTurnDamage);

        //post move
        entities.Sort(Comparer<BattleEntity>.Create((a, b) => PosCompare(a, b)));
        entities.SetBools();
        while (entities.hasNext())
        {
            BattleEntity current = entities.next();
            //Debug.Log(current.id + " post");
            yield return StartCoroutine(current.PostMove());
        }

        /*
        //enviro effect moves
        if (enviroEffect == BattleHelper.EnvironmentalEffect.IonizedSand && (turnCount % 3 == 0))
        {
            Move move = GetOrAddComponent<IonizedSandBolt>();
            yield return move.Execute(null);
            //Destroy(move);
        }
        if (enviroEffect == BattleHelper.EnvironmentalEffect.TrialOfHaste && (turnCount % 3 == 0))
        {
            Move move = GetOrAddComponent<TrialOfHasteBolt>();
            yield return move.Execute(null);
            //Destroy(move);
        }
        */
        


        //tick down charm effects
        PlayerData pd = playerData;
        for (int i = 0; i < pd.charmEffects.Count; i++)
        {
            pd.charmEffects[i].duration--;

            if (pd.charmEffects[i].duration <= 0)
            {
                pd.charmEffects[i].duration = pd.charmEffects[i].resetDuration;
                pd.charmEffects[i].charges -= 1;
                if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Attack)
                {
                    pd.charmEffects[i].charmType = CharmEffect.CharmType.Defense;
                }
                else if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Defense)
                {
                    pd.charmEffects[i].charmType = CharmEffect.CharmType.Attack;
                }
            }

            if (pd.charmEffects[i].duration <= 0 || pd.charmEffects[i].charges <= 0)
            {
                pd.charmEffects.RemoveAt(i);
                i--;
                continue;
            }
        }

        if (battleMapScript != null)
        {
            battleMapScript.OnPostTurn();
        }
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(RunOutOfTurnEvents());
        turnLoopRunning = false;
    }


    public T GetOrAddComponent<T>() where T : Component
    {
        if (gameObject.GetComponent<T>())
            return gameObject.GetComponent<T>();
        else
            return gameObject.AddComponent<T>() as T;
    }
}
